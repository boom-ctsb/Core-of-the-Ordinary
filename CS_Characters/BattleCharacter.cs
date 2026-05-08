using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BattleCharacter : MonoBehaviour
{
    [Header("Identity / Data")]
    [SerializeField] protected string characterName;
    [SerializeField] protected CharacterStats stats;
    [SerializeField] protected BaseCharacterData characterData;

    [Header("Runtime Slot")]
    [SerializeField] private int slotIndex = -1;

    [Header("Timers")]
    [SerializeField] protected float atbTimer = 0f;
    [SerializeField] protected float atbMaxTime = 100f;
    [SerializeField] protected float chargeTimer = 0f;
    [SerializeField] protected float maxCharge = 100f;

    [Header("ATB Tuning")]
    [SerializeField] private float speedBaseline = 25f;
    [Range(0.01f, 1f)]
    [SerializeField] private float baseFillPercentPerSecond = 0.20f;

    [Header("ATB Full Fix")]
    [SerializeField] private float atbFullEpsilon = 0.5f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Buff Runtime (Debug)")]
    [SerializeField] private bool debugBuffTick = false;

    // =======================
    // Buffs (affect stats)
    // =======================
    private readonly List<TimedBuffInstance> activeBuffs = new List<TimedBuffInstance>();
    public event Action<BattleCharacter> OnBuffListChanged;
    public IReadOnlyList<TimedBuffInstance> GetActiveBuffsReadOnly() => activeBuffs;

    // ✅ Event: แจ้งเมื่อมี Debuff ถูกใส่สำเร็จ
    public event Action<BattleCharacter, TimedBuffInstance> OnDebuffApplied;

    private void InvokeBuffListChanged()
    {
        OnBuffListChanged?.Invoke(this);
    }

    private void InvokeDebuffApplied(TimedBuffInstance debuff)
    {
        if (debuff == null || !debuff.IsDebuff) return;
        OnDebuffApplied?.Invoke(this, debuff);
    }

    // =======================
    // Owned Buff Icons (UI only)
    // =======================
    private readonly List<TimedBuffInstance> ownedBuffIcons = new List<TimedBuffInstance>();
    public event Action<BattleCharacter> OnOwnedBuffIconListChanged;
    public IReadOnlyList<TimedBuffInstance> GetOwnedBuffIconsReadOnly() => ownedBuffIcons;

    private void InvokeOwnedBuffIconListChanged()
    {
        OnOwnedBuffIconListChanged?.Invoke(this);
    }

    public void AddOrRefreshOwnedBuffIcon(TimedBuffInstance iconBuff)
    {
        if (iconBuff == null) return;
        if (iconBuff.DurationSeconds <= 0f) return;

        if (!string.IsNullOrEmpty(iconBuff.BuffKey))
        {
            for (int i = 0; i < ownedBuffIcons.Count; i++)
            {
                TimedBuffInstance existing = ownedBuffIcons[i];
                if (existing == null) continue;

                if (!existing.IsExpired &&
                    existing.IsDebuff == iconBuff.IsDebuff &&
                    existing.BuffKey == iconBuff.BuffKey)
                {
                    existing.IconSprite = iconBuff.IconSprite;
                    existing.IconPrefabOverride = iconBuff.IconPrefabOverride;
                    existing.Refresh(iconBuff.DurationSeconds);

                    InvokeOwnedBuffIconListChanged();
                    return;
                }
            }
        }

        ownedBuffIcons.Add(iconBuff);
        InvokeOwnedBuffIconListChanged();
    }

    // =======================
    // Shield (Runtime)
    // =======================
    [Header("Shield (Runtime)")]
    [SerializeField] private float shieldHP = 0f;

    public float ShieldHP => shieldHP;

    public event Action<BattleCharacter, float> OnShieldChanged;

    private void InvokeShieldChanged()
    {
        OnShieldChanged?.Invoke(this, shieldHP);
    }

    public bool IsATBFull => atbTimer >= (atbMaxTime - atbFullEpsilon);

    public string CharacterName { get => characterName; set => characterName = value; }
    public CharacterStats Stats { get => stats; set => stats = value; }
    public BaseCharacterData CharacterData { get => characterData; set => characterData = value; }

    public int SlotIndex { get => slotIndex; set => slotIndex = value; }

    public float ATBTimer => atbTimer;
    public float ATBMaxTime => atbMaxTime;

    public float ChargeTimer
    {
        get => chargeTimer;
        set => chargeTimer = Mathf.Clamp(value, 0f, maxCharge);
    }

    public float EffectiveAttack => (stats != null ? stats.Attack : 0f) + GetTotalAttackAdd();
    public float EffectiveDefense => (stats != null ? stats.Defense : 0f) + GetTotalDefenseAdd();

    public float EffectiveSpeed
    {
        get
        {
            float baseSpeed = (stats != null ? stats.Speed : 0f);
            float flat = GetTotalSpeedAdd();
            float percent = GetTotalSpeedPercent();
            float value = (baseSpeed + flat) * (1f + percent);
            return Mathf.Max(0.1f, value);
        }
    }

    public float EffectiveAccuracyMultiplier
    {
        get
        {
            float p = GetTotalAccuracyPercent();
            return Mathf.Clamp(1f + p, 0.05f, 2f);
        }
    }

    public float EffectiveEvasionChance
    {
        get
        {
            float e = GetTotalEvasionChanceAdd();
            return Mathf.Clamp(e, 0f, 0.95f);
        }
    }

    protected virtual void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    protected virtual void Update()
    {
        TickBuffs(Time.deltaTime);
        TickOwnedBuffIcons(Time.deltaTime);
    }

    public virtual void SetupData(BaseCharacterData data)
    {
        if (data == null)
        {
            Debug.LogError("❌ SetupData failed: BaseCharacterData is null!");
            return;
        }

        characterData = data;
        characterName = data.characterName;

        stats = new CharacterStats(data.maxHP, data.attack, data.defense, data.speed);

        ResetATB();
        ChargeTimer = 0f;

        shieldHP = 0f;
        InvokeShieldChanged();

        activeBuffs.Clear();
        InvokeBuffListChanged();

        ownedBuffIcons.Clear();
        InvokeOwnedBuffIconListChanged();
    }

    public virtual void InitializeVisuals() { }

    public virtual void UpdateATB(float deltaTime)
    {
        if (stats == null) return;

        if (IsATBFull)
        {
            atbTimer = atbMaxTime;
            return;
        }

        float safeBaseline = Mathf.Max(0.0001f, speedBaseline);
        float speedFactor = EffectiveSpeed / safeBaseline;
        float baseFillPerSecond = atbMaxTime * baseFillPercentPerSecond;

        atbTimer += deltaTime * baseFillPerSecond * speedFactor;

        if (atbTimer >= (atbMaxTime - atbFullEpsilon))
            atbTimer = atbMaxTime;
        else
            atbTimer = Mathf.Clamp(atbTimer, 0f, atbMaxTime);
    }

    public void ResetATB() => atbTimer = 0f;

    public void SetATBPercent(float percent01)
    {
        float p = Mathf.Clamp01(percent01);
        atbTimer = atbMaxTime * p;
    }

    public void ModifyATB(float deltaUnits)
    {
        atbTimer = Mathf.Clamp(atbTimer + deltaUnits, 0f, atbMaxTime);
    }

    public void AddCharge(float chargeAmount) => ChargeTimer = chargeTimer + chargeAmount;

    // =======================
    // Shield API
    // =======================
    public void AddShield(float amount, bool replaceInsteadOfStack = false)
    {
        amount = Mathf.Max(0f, amount);

        if (replaceInsteadOfStack)
            shieldHP = amount;
        else
            shieldHP += amount;

        InvokeShieldChanged();
    }

    public void ClearShield()
    {
        shieldHP = 0f;
        InvokeShieldChanged();
    }

    // =======================
    // Damage
    // =======================
    public virtual float TakeDamage(float rawDamage)
    {
        return TakeDamage(rawDamage, null);
    }

    public virtual float TakeDamage(float rawDamage, BattleCharacter attacker)
    {
        if (stats == null) return 0f;
        if (stats.IsDead) return 0f;

        float dmgAfterDef = Mathf.Max(1f, rawDamage - EffectiveDefense);

        if (shieldHP > 0f)
        {
            float absorbed = Mathf.Min(shieldHP, dmgAfterDef);
            shieldHP -= absorbed;
            dmgAfterDef -= absorbed;

            InvokeShieldChanged();
        }

        if (dmgAfterDef <= 0f)
            return 0f;

        stats.CurrentHP = Mathf.Max(0f, stats.CurrentHP - dmgAfterDef);
        return dmgAfterDef;
    }

    // =======================
    // Buff API (affect stats)
    // =======================
    public void AddTimedBuff(TimedBuffInstance buff)
    {
        if (buff == null) return;
        if (buff.DurationSeconds <= 0f) return;

        // กติกา: มีได้แค่อันเดียวต่อ BuffKey + IsDebuff
        if (!string.IsNullOrEmpty(buff.BuffKey))
        {
            for (int i = 0; i < activeBuffs.Count; i++)
            {
                TimedBuffInstance existing = activeBuffs[i];
                if (existing == null) continue;

                if (!existing.IsExpired &&
                    existing.IsDebuff == buff.IsDebuff &&
                    existing.BuffKey == buff.BuffKey)
                {
                    existing.AttackAdd = buff.AttackAdd;
                    existing.DefenseAdd = buff.DefenseAdd;
                    existing.SpeedAdd = buff.SpeedAdd;

                    existing.SpeedPercent = buff.SpeedPercent;
                    existing.AccuracyPercent = buff.AccuracyPercent;
                    existing.EvasionChanceAdd = buff.EvasionChanceAdd;

                    existing.HpRegenPerSecond = buff.HpRegenPerSecond;

                    existing.IconSprite = buff.IconSprite;
                    existing.IconPrefabOverride = buff.IconPrefabOverride;

                    existing.Refresh(buff.DurationSeconds);

                    InvokeBuffListChanged();
                    InvokeDebuffApplied(existing); // ✅ แจ้ง debuff (refresh)

                    return;
                }
            }
        }

        activeBuffs.Add(buff);
        InvokeBuffListChanged();
        InvokeDebuffApplied(buff); // ✅ แจ้ง debuff (new)
    }

    private void TickBuffs(float dt)
    {
        if (Stats == null) return;
        if (activeBuffs.Count == 0) return;

        dt = Mathf.Max(0f, dt);

        bool removedAny = false;

        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            TimedBuffInstance b = activeBuffs[i];
            if (b == null)
            {
                activeBuffs.RemoveAt(i);
                removedAny = true;
                continue;
            }

            if (!Stats.IsDead && b.HpRegenPerSecond > 0f)
            {
                float heal = b.HpRegenPerSecond * dt;
                Stats.CurrentHP = Mathf.Min(Stats.MaxHP, Stats.CurrentHP + heal);

                if (debugBuffTick)
                    Debug.Log($"🌿 Regen {characterName} +{heal:F2} HP");
            }

            b.Tick(dt);

            if (b.IsExpired)
            {
                activeBuffs.RemoveAt(i);
                removedAny = true;
            }
        }

        if (removedAny)
            InvokeBuffListChanged();
    }

    // =======================
    // Owned Buff Icons Tick (UI only)
    // =======================
    private void TickOwnedBuffIcons(float dt)
    {
        if (ownedBuffIcons.Count == 0) return;

        dt = Mathf.Max(0f, dt);

        bool removedAny = false;

        for (int i = ownedBuffIcons.Count - 1; i >= 0; i--)
        {
            TimedBuffInstance b = ownedBuffIcons[i];
            if (b == null)
            {
                ownedBuffIcons.RemoveAt(i);
                removedAny = true;
                continue;
            }

            b.Tick(dt);

            if (b.IsExpired)
            {
                ownedBuffIcons.RemoveAt(i);
                removedAny = true;
            }
        }

        if (removedAny)
            InvokeOwnedBuffIconListChanged();
    }

    // =======================
    // Buff stat totals
    // =======================
    private float GetTotalAttackAdd()
    {
        float sum = 0f;
        for (int i = 0; i < activeBuffs.Count; i++)
            if (activeBuffs[i] != null) sum += activeBuffs[i].AttackAdd;
        return sum;
    }

    private float GetTotalDefenseAdd()
    {
        float sum = 0f;
        for (int i = 0; i < activeBuffs.Count; i++)
            if (activeBuffs[i] != null) sum += activeBuffs[i].DefenseAdd;
        return sum;
    }

    private float GetTotalSpeedAdd()
    {
        float sum = 0f;
        for (int i = 0; i < activeBuffs.Count; i++)
            if (activeBuffs[i] != null) sum += activeBuffs[i].SpeedAdd;
        return sum;
    }

    private float GetTotalSpeedPercent()
    {
        float sum = 0f;
        for (int i = 0; i < activeBuffs.Count; i++)
            if (activeBuffs[i] != null) sum += activeBuffs[i].SpeedPercent;
        return sum;
    }

    private float GetTotalAccuracyPercent()
    {
        float sum = 0f;
        for (int i = 0; i < activeBuffs.Count; i++)
            if (activeBuffs[i] != null) sum += activeBuffs[i].AccuracyPercent;
        return sum;
    }

    private float GetTotalEvasionChanceAdd()
    {
        float sum = 0f;
        for (int i = 0; i < activeBuffs.Count; i++)
            if (activeBuffs[i] != null) sum += activeBuffs[i].EvasionChanceAdd;
        return sum;
    }

    public void PlayActionAnimation(string actionId)
    {
        if (animator == null) return;
        if (characterData == null) return;

        string trigger = null;

        if (actionId == "Attack") trigger = characterData.attackAnimTrigger;
        else if (actionId == "Skill1") trigger = characterData.skill1AnimTrigger;
        else if (actionId == "Skill2") trigger = characterData.skill2AnimTrigger;
        else if (actionId == "Ultimate") trigger = characterData.ultimateAnimTrigger;

        if (string.IsNullOrEmpty(trigger)) return;
        animator.SetTrigger(trigger);
    }
}