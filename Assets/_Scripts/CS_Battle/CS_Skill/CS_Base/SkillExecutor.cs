using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public static class SkillExecutor
{
    public static event Action<BattleCharacter, BattleCharacter, SkillAction> OnStunApplied;

    public static bool TryExecuteSkill(BattleCharacter user, SkillAction skill, IReadOnlyList<BattleCharacter> targets, out string failReason)
    {
        failReason = "";

        if (user == null) { failReason = "user is null"; return false; }
        if (skill == null) { failReason = "skill is null"; return false; }
        if (targets == null || targets.Count == 0) { failReason = "no targets"; return false; }
        if (user.Stats == null || user.Stats.IsDead) { failReason = "user invalid"; return false; }
        if (user.ChargeTimer < skill.chargeCost) { failReason = "not enough charge"; return false; }

        bool appliedAny = ExecuteCore(
            skill: skill,
            targets: targets,
            attackValue: user.EffectiveAttack,
            accuracyMultiplier: user.EffectiveAccuracyMultiplier,
            attacker: user,
            iconOwner: user,
            onTargetHit: null
        );

        if (!appliedAny)
        {
            failReason = "skill had no effect";
            return false;
        }

        if (skill.chargeCost > 0f)
            user.ChargeTimer -= skill.chargeCost;

        if (skill.overrideATBAfterUse)
            user.SetATBPercent(skill.atbAfterUsePercent);

        return true;
    }

    public static bool TryExecuteSkill_Backline(BacklinePassiveUnit user, SkillAction skill, IReadOnlyList<BattleCharacter> targets, out string failReason)
    {
        failReason = "";

        if (user == null) { failReason = "user is null"; return false; }
        if (skill == null) { failReason = "skill is null"; return false; }
        if (targets == null || targets.Count == 0) { failReason = "no targets"; return false; }

        bool appliedAny = ExecuteCore(
            skill: skill,
            targets: targets,
            attackValue: user.Attack,
            accuracyMultiplier: 1f,
            attacker: null,
            iconOwner: user,
            onTargetHit: null
        );

        if (!appliedAny)
        {
            failReason = "skill had no effect";
            return false;
        }

        return true;
    }

    private static bool ExecuteCore(
        SkillAction skill,
        IReadOnlyList<BattleCharacter> targets,
        float attackValue,
        float accuracyMultiplier,
        BattleCharacter attacker,
        IRuntimeBuffIconOwner iconOwner,
        Action<BattleCharacter> onTargetHit)
    {
        bool appliedAny = false;

        for (int i = 0; i < targets.Count; i++)
        {
            BattleCharacter target = targets[i];
            if (target == null || target.Stats == null || target.Stats.IsDead)
                continue;

            switch (skill.skillType)
            {
                case SkillType.Attack:
                    appliedAny |= ApplyAttack(skill, target, attackValue, accuracyMultiplier, attacker, onTargetHit);
                    break;

                case SkillType.Heal:
                    appliedAny |= ApplyHeal(skill, target, attackValue);
                    break;

                case SkillType.Buff:
                    appliedAny |= ApplyBuff(skill, target, iconOwner);
                    break;

                case SkillType.Debuff:
                    appliedAny |= ApplyDebuff(skill, target, attackValue, accuracyMultiplier, attacker);
                    break;
            }
        }

        return appliedAny;
    }

    private static bool ApplyAttack(
        SkillAction skill,
        BattleCharacter target,
        float attackValue,
        float accuracyMultiplier,
        BattleCharacter attacker,
        Action<BattleCharacter> onTargetHit)
    {
        if (!RollHit(skill, target, accuracyMultiplier))
            return false;

        float rawDamage = Mathf.Max(0f, skill.powerBase + (attackValue * skill.attackMultiplier));
        float dealt = attacker != null ? target.TakeDamage(rawDamage, attacker) : target.TakeDamage(rawDamage);

        ApplyPostHitAdjustments(skill, target);

        if (dealt > 0f)
            onTargetHit?.Invoke(target);

        return dealt > 0f || skill.targetATBShiftUnits != 0f || skill.targetChargeShift != 0f;
    }

    private static bool ApplyHeal(SkillAction skill, BattleCharacter target, float attackValue)
    {
        if (target.Stats == null || target.Stats.IsDead)
            return false;

        float heal = Mathf.Max(0f, skill.powerBase + (attackValue * skill.attackMultiplier));
        if (heal <= 0f)
            return false;

        target.Stats.CurrentHP = Mathf.Min(target.Stats.MaxHP, target.Stats.CurrentHP + heal);
        ApplyPostHitAdjustments(skill, target);
        return true;
    }

    private static bool ApplyBuff(SkillAction skill, BattleCharacter target, IRuntimeBuffIconOwner iconOwner)
    {
        TimedBuffInstance buff = CreateBuffInstance(skill, false);
        if (buff == null)
            return false;

        target.AddTimedBuff(buff.CloneForRuntime());
        target.AddOrRefreshOwnedBuffIcon(buff.CloneForRuntime());
        iconOwner?.AddOrRefreshOwnedBuffIcon(buff.CloneForRuntime());
        ApplyPostHitAdjustments(skill, target);
        return true;
    }

    private static bool ApplyDebuff(
        SkillAction skill,
        BattleCharacter target,
        float attackValue,
        float accuracyMultiplier,
        BattleCharacter attacker)
    {
        bool applied = false;

        if (skill.isStunDebuff)
        {
            TimedBuffInstance stunMarker = CreateDebuffInstance(skill);
            if (stunMarker != null)
            {
                target.AddTimedBuff(stunMarker.CloneForRuntime());
                OnStunApplied?.Invoke(attacker, target, skill);
                applied = true;
            }
        }

        TimedBuffInstance debuff = CreateDebuffInstance(skill);
        if (debuff != null && RollHit(skill, target, accuracyMultiplier))
        {
            target.AddTimedBuff(debuff.CloneForRuntime());

            float rawDamage = Mathf.Max(0f, skill.powerBase + (attackValue * skill.attackMultiplier));
            if (rawDamage > 0f)
            {
                if (attacker != null) target.TakeDamage(rawDamage, attacker);
                else target.TakeDamage(rawDamage);
            }

            ApplyPostHitAdjustments(skill, target);
            applied = true;
        }

        return applied;
    }

    private static bool RollHit(SkillAction skill, BattleCharacter target, float accuracyMultiplier)
    {
        float accuracy01 = Mathf.Clamp01((skill.accuracy / 100f) * Mathf.Max(0.05f, accuracyMultiplier));
        float hitChance = Mathf.Clamp01(accuracy01 - target.EffectiveEvasionChance);
        return Random.value <= hitChance;
    }

    private static void ApplyPostHitAdjustments(SkillAction skill, BattleCharacter target)
    {
        if (skill.targetATBShiftUnits != 0f)
            target.ModifyATB(skill.targetATBShiftUnits);

        if (skill.targetChargeShift != 0f)
            target.ChargeTimer += skill.targetChargeShift;
    }

    private static TimedBuffInstance CreateBuffInstance(SkillAction skill, bool isDebuff)
    {
        if (skill == null)
            return null;

        bool hasStatEffect =
            skill.buffAttackAdd != 0f ||
            skill.buffDefenseAdd != 0f ||
            skill.buffSpeedAdd != 0f ||
            skill.buffHpRegenPerSecond != 0f ||
            skill.buffEvasionChanceAdd != 0f ||
            skill.enableTeamFollowUpHit ||
            skill.enableTeamShield;

        if (!hasStatEffect && !isDebuff)
            return null;

        TimedBuffInstance buff = new TimedBuffInstance
        {
            BuffKey = $"skill:{skill.skillName}",
            IsDebuff = isDebuff,
            DurationSeconds = Mathf.Max(0.01f, skill.buffDurationSeconds),
            AttackAdd = skill.buffAttackAdd,
            DefenseAdd = skill.buffDefenseAdd,
            SpeedAdd = skill.buffSpeedAdd,
            HpRegenPerSecond = skill.buffHpRegenPerSecond,
            EvasionChanceAdd = skill.buffEvasionChanceAdd,
            IconSprite = skill.skillIcon,
            IconPrefabOverride = skill.statusIconPrefabOverride
        };

        return buff;
    }

    private static TimedBuffInstance CreateDebuffInstance(SkillAction skill)
    {
        if (skill == null)
            return null;

        bool hasEffect =
            skill.debuffSpeedPercent != 0f ||
            skill.debuffAccuracyPercent != 0f ||
            skill.debuffAttackAdd != 0f ||
            skill.debuffDefenseAdd != 0f ||
            skill.isStunDebuff;

        if (!hasEffect)
            return null;

        TimedBuffInstance debuff = new TimedBuffInstance
        {
            BuffKey = $"skill:{skill.skillName}:debuff",
            IsDebuff = true,
            DurationSeconds = Mathf.Max(0.01f, skill.buffDurationSeconds),
            AttackAdd = skill.debuffAttackAdd,
            DefenseAdd = skill.debuffDefenseAdd,
            SpeedPercent = skill.debuffSpeedPercent,
            AccuracyPercent = skill.debuffAccuracyPercent,
            IconSprite = skill.skillIcon,
            IconPrefabOverride = skill.statusIconPrefabOverride
        };

        return debuff;
    }
}