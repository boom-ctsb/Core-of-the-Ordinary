using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [Header("Factory / Party")]
    [SerializeField] private CharacterFactory characterFactory;
    [SerializeField] private string selectedEnemyName = "Dysnorma";

    [Header("Dynamic Ally Panels Builder")]
    [SerializeField] private BattleAllyPanelBuilder allyPanelBuilder;

    [Header("Enemy")]
    [SerializeField] private EnemyPanel enemyPanel;

    [Tooltip("ศัตรูโจมตีแล้วได้ Charge เพิ่มกี่หน่วย (0..100)")]
    [SerializeField] private float enemyAttackChargeGain = 10f;

    [Header("Team Shield UI (Fixed Position)")]
    [Tooltip("ใส่ TeamShieldUIBinder ที่วางไว้บน Empty Object ในฉาก (ตำแหน่งตายตัว)")]
    [SerializeField] private TeamShieldUIBinder allyTeamShieldUI;

    [Header("Camera")]
    [SerializeField] private SimpleCameraController cameraController;
    [SerializeField] private bool returnToIdleAfterAction = true;

    [Header("Skill UI")]
    [SerializeField] private BattleSkillUI skillUI;

    [Header("Basic Attack -> Charge (Box Unit)")]
    [SerializeField] private int basicAttackChargeBoxesGain = 1;

    [Header("Charge Tuning")]
    [SerializeField] private int chargeBoxesCount = 10;

    [Header("Turn Flow (Real-time ATB option)")]
    [SerializeField] private bool enemyCanActWhilePlayerChoosing = true;

    [Header("Backline Passive Units (No Field)")]
    [SerializeField] private List<BacklinePassiveUnit> backlineUnits = new List<BacklinePassiveUnit>();

    [Header("Debug")]
    [SerializeField] private bool verboseLog = false;

    private readonly List<BattleCharacter> allCharacters = new List<BattleCharacter>();
    private readonly BattleCharacter[] alliesBySlots = new BattleCharacter[8];
    private readonly List<BattleCharacter> allies = new List<BattleCharacter>();

    private BattleCharacter currentReadyCharacter;
    private Dysnorma enemyInstance;

    private readonly List<TeamOnHitBuff> activeTeamOnHitBuffs = new List<TeamOnHitBuff>();

    [Header("Team Shield (Runtime Debug)")]
    [SerializeField] private float allyTeamShieldHP = 0f;

    private void Awake()
    {
        if (cameraController == null)
            cameraController = FindObjectOfType<SimpleCameraController>();

        if (skillUI == null)
            skillUI = FindObjectOfType<BattleSkillUI>(true);

        if (skillUI != null)
            skillUI.Bind(this);

        InstantiateSelectedCharactersFromSelectionData();

        if (cameraController != null)
            cameraController.ToIdle();
    }

    private void InstantiateSelectedCharactersFromSelectionData()
    {
        allCharacters.Clear();
        allies.Clear();
        enemyInstance = null;
        currentReadyCharacter = null;
        activeTeamOnHitBuffs.Clear();

        allyTeamShieldHP = 0f;
        SyncTeamShieldUI();

        for (int i = 0; i < alliesBySlots.Length; i++)
            alliesBySlots[i] = null;

        if (characterFactory == null)
        {
            Debug.LogError("❌ BattleManager: characterFactory is null", this);
            return;
        }

        if (PartySelectionData.Instance == null)
        {
            Debug.LogError("❌ BattleManager: PartySelectionData.Instance is null", this);
            return;
        }

        var party = PartySelectionData.Instance.SelectedParty;
        if (party == null || party.Count != 8)
        {
            Debug.LogError("❌ BattleManager: SelectedParty invalid (need 8 slots)", this);
            return;
        }

        for (int slot = 0; slot < 8; slot++)
        {
            BaseCharacterData cd = party[slot];
            if (cd == null) continue;

            BattleCharacter bc = characterFactory.CreateAllyBySelectedSlot(cd, slot);
            if (bc == null) continue;

            alliesBySlots[slot] = bc;
            allCharacters.Add(bc);
            allies.Add(bc);

            if (verboseLog)
                Debug.Log($"[BattleManager] Spawn Ally '{bc.CharacterName}' at slot={slot}");
        }

        if (allyPanelBuilder != null)
            allyPanelBuilder.BuildFromAlliesSlots(alliesBySlots);

        BattleCharacter enemy = characterFactory.CreateEnemy(selectedEnemyName);
        if (enemy is Dysnorma d)
        {
            enemyInstance = d;
            allCharacters.Add(d);

            if (enemyPanel != null)
                enemyPanel.Initialize(d);
        }
        else if (enemy != null)
        {
            allCharacters.Add(enemy);
            Debug.LogWarning("⚠️ Enemy ที่สร้างมาไม่ใช่ Dysnorma: combo อาจไม่ทำงาน");
        }

        InitializeBacklineUnits();
    }

    private void InitializeBacklineUnits()
    {
        if (backlineUnits == null) return;

        for (int i = 0; i < backlineUnits.Count; i++)
        {
            BacklinePassiveUnit unit = backlineUnits[i];
            if (unit == null) continue;
            unit.Initialize(this);
        }
    }

    public IReadOnlyList<BattleCharacter> GetEnemiesReadOnly()
    {
        List<BattleCharacter> enemies = new List<BattleCharacter>();
        for (int i = 0; i < allCharacters.Count; i++)
        {
            BattleCharacter c = allCharacters[i];
            if (c == null || c.CharacterData == null) continue;
            if (c.CharacterData.faction == CharacterFaction.Enemy && c.Stats != null && !c.Stats.IsDead)
                enemies.Add(c);
        }
        return enemies;
    }

    private void Update()
    {
        if (enemyPanel != null)
            enemyPanel.UpdatePanel();

        TickTeamOnHitBuffs(Time.deltaTime);

        for (int i = 0; i < allCharacters.Count; i++)
        {
            BattleCharacter character = allCharacters[i];
            if (character == null) continue;
            if (character.Stats != null && character.Stats.IsDead) continue;

            character.UpdateATB(Time.deltaTime);

            if (IsAlly(character) && character.IsATBFull)
            {
                if (currentReadyCharacter == null)
                {
                    OnCharacterATBReady(character);
                    break;
                }
            }

            if (character is Dysnorma enemy && enemy.IsATBFull)
            {
                if (currentReadyCharacter == null || enemyCanActWhilePlayerChoosing)
                {
                    OnCharacterATBReady(enemy);
                    break;
                }
            }
        }
    }

    private void SyncTeamShieldUI()
    {
        if (allyTeamShieldUI != null)
            allyTeamShieldUI.SetShieldValue(allyTeamShieldHP);
    }

    private void AddOrReplaceAllyTeamShield(float amount, bool replaceInsteadOfStack)
    {
        amount = Mathf.Max(0f, amount);

        if (replaceInsteadOfStack)
            allyTeamShieldHP = amount;
        else
            allyTeamShieldHP += amount;

        SyncTeamShieldUI();
    }

    private float ApplyDamageToAllyWithTeamShield(BattleCharacter allyTarget, float rawDamage, BattleCharacter attacker)
    {
        if (allyTarget == null || allyTarget.Stats == null || allyTarget.Stats.IsDead)
            return 0f;

        float dmgAfterDef = Mathf.Max(1f, rawDamage - allyTarget.EffectiveDefense);

        if (allyTeamShieldHP > 0f)
        {
            float absorbed = Mathf.Min(allyTeamShieldHP, dmgAfterDef);
            allyTeamShieldHP -= absorbed;
            dmgAfterDef -= absorbed;
            SyncTeamShieldUI();
        }

        if (dmgAfterDef <= 0f)
            return 0f;

        allyTarget.Stats.CurrentHP = Mathf.Max(0f, allyTarget.Stats.CurrentHP - dmgAfterDef);
        return dmgAfterDef;
    }

    private void TickTeamOnHitBuffs(float dt)
    {
        if (activeTeamOnHitBuffs.Count == 0) return;

        for (int i = activeTeamOnHitBuffs.Count - 1; i >= 0; i--)
        {
            TeamOnHitBuff b = activeTeamOnHitBuffs[i];
            if (b == null)
            {
                activeTeamOnHitBuffs.RemoveAt(i);
                continue;
            }

            b.Tick(dt);

            if (b.IsExpired)
                activeTeamOnHitBuffs.RemoveAt(i);
        }
    }

    private bool IsAlly(BattleCharacter c)
    {
        if (c == null || c.CharacterData == null) return false;
        return c.CharacterData.faction == CharacterFaction.Ally;
    }

    private BattleCharacter GetCurrentEnemyTarget()
    {
        if (enemyInstance != null && enemyInstance.Stats != null && !enemyInstance.Stats.IsDead)
            return enemyInstance;

        for (int i = 0; i < allCharacters.Count; i++)
        {
            BattleCharacter c = allCharacters[i];
            if (c == null || c.CharacterData == null || c.Stats == null) continue;
            if (c.CharacterData.faction == CharacterFaction.Enemy && !c.Stats.IsDead)
                return c;
        }
        return null;
    }

    private void OnCharacterATBReady(BattleCharacter character)
    {
        if (character == null) return;

        if (IsAlly(character))
        {
            if (currentReadyCharacter != null) return;

            currentReadyCharacter = character;

            if (cameraController != null)
                cameraController.FocusCharacter(character);

            if (skillUI != null)
                skillUI.ShowFor(character);

            if (allyPanelBuilder != null)
                allyPanelBuilder.HighlightCurrentTurn(character);

            return;
        }

        if (character is Dysnorma enemy)
        {
            BattleCharacter target = FindRandomAliveFrontAllyOrFallback();
            if (target == null) return;

            float dmg = enemy.GetBasicAttackDamage();

            float dealt = ApplyDamageToAllyWithTeamShield(target, dmg, enemy);

            enemy.ChargeTimer = enemy.ChargeTimer + enemyAttackChargeGain;

            if (verboseLog)
                Debug.Log($"[BattleManager] Enemy attack: target={target.CharacterName} dealt={dealt:F0} enemyCharge+={enemyAttackChargeGain:F0} teamShield={allyTeamShieldHP:F0}");

            enemy.ResetATB();
        }
    }

    private float BoxesToChargePoints(int boxes)
    {
        int safeCount = Mathf.Max(1, chargeBoxesCount);
        float perBox = 100f / safeCount;
        return Mathf.Max(0, boxes) * perBox;
    }

    private bool ValidateReady(BattleCharacter attacker, string actionIdForLog)
    {
        if (attacker == null)
        {
            Debug.LogWarning($"⚠️ {actionIdForLog}: attacker is null");
            return false;
        }

        if (attacker != currentReadyCharacter)
        {
            Debug.LogWarning($"⚠️ {actionIdForLog}: attacker != currentReadyCharacter (attacker={attacker.CharacterName}, current={currentReadyCharacter?.CharacterName})");
            return false;
        }

        if (!IsAlly(attacker))
        {
            Debug.LogWarning($"⚠️ {actionIdForLog}: currentReadyCharacter ไม่ใช่ Ally");
            return false;
        }

        if (!attacker.IsATBFull)
        {
            Debug.LogWarning($"⛔ {actionIdForLog}: ATB ยังไม่เต็ม (ATB={attacker.ATBTimer}/{attacker.ATBMaxTime})");
            return false;
        }

        return true;
    }

    private BattleCharacter FindRandomAliveFrontAllyOrFallback()
    {
        List<BattleCharacter> candidates = new List<BattleCharacter>(4);

        for (int slot = 4; slot < 8; slot++)
        {
            BattleCharacter bc = alliesBySlots[slot];
            if (bc == null) continue;
            if (!IsAlly(bc)) continue;
            if (bc.Stats == null) continue;
            if (bc.Stats.IsDead) continue;

            candidates.Add(bc);
        }

        if (candidates.Count > 0)
            return candidates[Random.Range(0, candidates.Count)];

        candidates.Clear();
        for (int i = 0; i < allies.Count; i++)
        {
            BattleCharacter a = allies[i];
            if (a == null) continue;
            if (!IsAlly(a)) continue;
            if (a.Stats == null) continue;
            if (a.Stats.IsDead) continue;

            candidates.Add(a);
        }

        if (candidates.Count == 0) return null;
        return candidates[Random.Range(0, candidates.Count)];
    }

    private void EndPlayerTurn(BattleCharacter attacker, string actionIdForLog)
    {
        if (verboseLog)
            Debug.Log($"[BattleManager] EndTurn action={actionIdForLog} attacker={attacker.CharacterName}");

        if (attacker.IsATBFull)
            attacker.ResetATB();

        if (skillUI != null)
            skillUI.Hide();

        if (returnToIdleAfterAction && cameraController != null)
            cameraController.ToIdle();

        currentReadyCharacter = null;

        if (allyPanelBuilder != null)
            allyPanelBuilder.ResetHighlights();
    }

    // =======================
    // Team Follow-up Buff (เดิม)
    // =======================
    private void RegisterTeamFollowUpHit(BattleCharacter owner, SkillAction skill)
    {
        if (owner == null || skill == null) return;
        if (!skill.enableTeamFollowUpHit) return;

        float dur = Mathf.Max(0f, skill.buffDurationSeconds);
        if (dur <= 0f)
        {
            Debug.LogWarning($"⚠️ FollowUpHit '{skill.skillName}' duration <= 0");
            return;
        }

        TeamOnHitBuff existing = null;
        for (int i = 0; i < activeTeamOnHitBuffs.Count; i++)
        {
            TeamOnHitBuff b = activeTeamOnHitBuffs[i];
            if (b == null) continue;

            if (b.Owner == owner)
            {
                existing = b;
                break;
            }
        }

        if (existing != null)
        {
            existing.UpdatePower(skill.followUpOwnerAtkMultiplier, skill.followUpFlatBonusDamage);
            existing.AddEnemyComboOnProc = skill.followUpAddsEnemyCombo;
            existing.ComboAddAmount = Mathf.Max(0, skill.followUpEnemyComboAdd);
            existing.RefreshDuration(dur);

            Debug.Log($"🟣 TeamBuff[{skill.skillName}] REFRESH owner={owner.CharacterName} dur={dur:F1}s");
            return;
        }

        TeamOnHitBuff buff = new TeamOnHitBuff(
            owner,
            skill.followUpOwnerAtkMultiplier,
            skill.followUpFlatBonusDamage,
            dur
        );

        buff.AddEnemyComboOnProc = skill.followUpAddsEnemyCombo;
        buff.ComboAddAmount = Mathf.Max(0, skill.followUpEnemyComboAdd);

        activeTeamOnHitBuffs.Add(buff);

        Debug.Log($"🟣 TeamBuff[{skill.skillName}] NEW owner={owner.CharacterName} dur={dur:F1}s");
    }

    private void NotifyAllyAttackLanded(BattleCharacter attackerWhoHit, BattleCharacter enemyTarget)
    {
        if (activeTeamOnHitBuffs.Count == 0) return;
        if (enemyTarget == null || enemyTarget.Stats == null || enemyTarget.Stats.IsDead) return;

        for (int i = activeTeamOnHitBuffs.Count - 1; i >= 0; i--)
        {
            TeamOnHitBuff b = activeTeamOnHitBuffs[i];
            if (b == null || b.IsExpired)
            {
                activeTeamOnHitBuffs.RemoveAt(i);
                continue;
            }

            if (b.Owner == null || b.Owner.Stats == null || b.Owner.Stats.IsDead)
                continue;

            float raw = (b.Owner.EffectiveAttack * b.OwnerAttackMultiplier) + b.FlatBonusDamage;
            float dealt = enemyTarget.TakeDamage(raw, b.Owner);

            Debug.Log($"🟣 FollowUpHit owner={b.Owner.CharacterName} procBy={attackerWhoHit.CharacterName} -> {enemyTarget.CharacterName} dealt={dealt:F0}");
        }
    }

    // =======================
    // Player actions
    // =======================
    private void ExecuteBasicAttack(BattleCharacter attacker)
    {
        const string ACTION_ID = "Attack";
        if (!ValidateReady(attacker, ACTION_ID)) return;

        BattleCharacter enemy = GetCurrentEnemyTarget();
        if (enemy == null)
        {
            Debug.LogWarning("⚠️ ไม่มีศัตรูให้โจมตี");
            EndPlayerTurn(attacker, ACTION_ID);
            return;
        }

        float raw = attacker.Stats != null ? attacker.Stats.Attack : 0f;
        float dealt = enemy.TakeDamage(raw, attacker);

        if (dealt > 0f)
            NotifyAllyAttackLanded(attacker, enemy);

        float chargePoints = BoxesToChargePoints(basicAttackChargeBoxesGain);
        attacker.AddCharge(chargePoints);

        attacker.PlayActionAnimation(ACTION_ID);

        EndPlayerTurn(attacker, ACTION_ID);
    }

    private List<BattleCharacter> GetAliveAllies()
    {
        List<BattleCharacter> result = new List<BattleCharacter>();
        for (int i = 0; i < allies.Count; i++)
        {
            BattleCharacter a = allies[i];
            if (a == null || a.Stats == null) continue;
            if (a.Stats.IsDead) continue;
            result.Add(a);
        }
        return result;
    }

    private List<BattleCharacter> GetSkillTargets(BattleCharacter attacker, SkillAction skill)
    {
        List<BattleCharacter> targets = new List<BattleCharacter>();
        if (attacker == null || skill == null) return targets;

        switch (skill.targetType)
        {
            case TargetType.Self:
                targets.Add(attacker);
                break;

            case TargetType.AllAllies:
                targets.AddRange(GetAliveAllies());
                break;

            case TargetType.EnemySingle:
                {
                    BattleCharacter enemy = GetCurrentEnemyTarget();
                    if (enemy != null) targets.Add(enemy);
                    break;
                }

            case TargetType.AllEnemies:
                {
                    BattleCharacter enemy = GetCurrentEnemyTarget();
                    if (enemy != null) targets.Add(enemy);
                    break;
                }

            case TargetType.AllySingle:
                targets.Add(attacker);
                break;
        }

        return targets;
    }

    // ✅ เปิดใช้ให้ Backline เรียกได้
    public List<BattleCharacter> GetTargetsForBacklineSkill(SkillAction skill, BattleCharacter proxySelf = null)
    {
        List<BattleCharacter> targets = new List<BattleCharacter>();
        if (skill == null) return targets;

        BattleCharacter requester = proxySelf;
        if ((skill.targetType == TargetType.Self || skill.targetType == TargetType.AllySingle) && requester == null)
        {
            List<BattleCharacter> alive = GetAliveAllies();
            if (alive.Count > 0) requester = alive[0];
        }

        return GetSkillTargets(requester, skill);
    }

    public bool ExecuteBacklineSkill(BacklinePassiveUnit unit, SkillAction skill)
    {
        if (unit == null || skill == null) return false;

        List<BattleCharacter> targets = GetTargetsForBacklineSkill(skill);
        if (targets == null || targets.Count == 0)
        {
            Debug.LogWarning($"⚠️ Backline: ไม่มี target สำหรับสกิล '{skill.skillName}'");
            return false;
        }

        bool ok = SkillExecutor.TryExecuteSkill_Backline(unit, skill, targets, out string reason);
        if (!ok)
        {
            Debug.LogWarning($"⛔ Backline ใช้สกิลไม่สำเร็จ: {skill.skillName} reason={reason}");
            return false;
        }

        if (skill.skillType == SkillType.Buff && skill.targetType == TargetType.AllAllies && skill.enableTeamShield)
        {
            float shield = Mathf.Max(0f, unit.Defense * skill.shieldFromOwnerDefenseMultiplier);
            AddOrReplaceAllyTeamShield(shield, skill.shieldReplaceInsteadOfStack);

            if (verboseLog)
                Debug.Log($"🛡️ Backline TeamShield +{shield:F0} (ownerDef={unit.Defense:F0} mul={skill.shieldFromOwnerDefenseMultiplier:F2}) => total={allyTeamShieldHP:F0}");
        }

        return true;
    }

    private void ExecuteSkillByActionId(BattleCharacter attacker, string actionId)
    {
        if (!ValidateReady(attacker, actionId)) return;

        AllyCharacterData allyData = attacker.CharacterData as AllyCharacterData;
        if (allyData == null)
        {
            Debug.LogWarning($"⚠️ {actionId}: attacker data ไม่ใช่ AllyCharacterData");
            return;
        }

        if (actionId == "Ultimate")
        {
            UltimateSkillAction ult = allyData.ultimateSkill;
            if (ult == null)
            {
                Debug.LogWarning($"⚠️ Ultimate: ยังไม่ได้ตั้ง UltimateSkillAction ใน AllyCharacterData ของ '{attacker.CharacterName}'");
                return;
            }

            ExecuteUltimate(attacker, ult);
            EndPlayerTurn(attacker, actionId);
            return;
        }

        SkillAction skill = null;
        if (actionId == "Skill1") skill = allyData.GetEquippedSkill1();
        else if (actionId == "Skill2") skill = allyData.GetEquippedSkill2();

        if (skill == null)
        {
            Debug.LogWarning($"⚠️ {actionId}: ยังไม่ได้ตั้ง SkillAction ใน AllyCharacterData ของ '{attacker.CharacterName}'");
            return;
        }

        List<BattleCharacter> targets = GetSkillTargets(attacker, skill);
        if (targets == null || targets.Count == 0)
        {
            Debug.LogWarning($"⚠️ {actionId}: ไม่มี target สำหรับสกิล '{skill.skillName}' (targetType={skill.targetType})");
            return;
        }

        bool ok = SkillExecutor.TryExecuteSkill(attacker, skill, targets, out string reason);
        if (!ok)
        {
            Debug.LogWarning($"⛔ ใช้สกิลไม่สำเร็จ: {skill.skillName} reason={reason}");
            return;
        }

        attacker.PlayActionAnimation(skill.animationActionId);

        if (skill.skillType == SkillType.Buff && skill.targetType == TargetType.AllAllies && skill.enableTeamShield)
        {
            float shield = Mathf.Max(0f, attacker.EffectiveDefense * skill.shieldFromOwnerDefenseMultiplier);
            AddOrReplaceAllyTeamShield(shield, skill.shieldReplaceInsteadOfStack);

            if (verboseLog)
                Debug.Log($"🛡️ TeamShield +{shield:F0} (ownerDef={attacker.EffectiveDefense:F0} mul={skill.shieldFromOwnerDefenseMultiplier:F2}) => total={allyTeamShieldHP:F0}");
        }

        if (skill.skillType == SkillType.Buff && skill.enableTeamFollowUpHit && skill.targetType == TargetType.AllAllies)
            RegisterTeamFollowUpHit(attacker, skill);

        if (skill.skillType == SkillType.Attack)
        {
            BattleCharacter enemy = GetCurrentEnemyTarget();
            if (enemy != null)
                NotifyAllyAttackLanded(attacker, enemy);
        }

        EndPlayerTurn(attacker, actionId);
    }

    public void OnPlayerSelectedAction(BattleCharacter character, string actionId)
    {
        if (string.IsNullOrEmpty(actionId))
        {
            Debug.LogWarning("⚠️ OnPlayerSelectedAction: actionId ว่าง");
            return;
        }

        if (verboseLog)
            Debug.Log($"[BattleManager] OnPlayerSelectedAction character={(character != null ? character.CharacterName : "null")} actionId={actionId}");

        if (actionId == "Attack")
        {
            ExecuteBasicAttack(character);
            return;
        }

        if (actionId == "Skill1" || actionId == "Skill2" || actionId == "Ultimate")
        {
            ExecuteSkillByActionId(character, actionId);
            return;
        }

        Debug.Log($"ℹ️ ยังไม่ทำ {actionId} ตอนนี้");
    }

    private void ExecuteUltimate(BattleCharacter attacker, UltimateSkillAction ult)
    {
        if (attacker == null || ult == null) return;

        UltimateRuntime rt = attacker.GetComponent<UltimateRuntime>();
        if (rt == null)
        {
            Debug.LogWarning($"⚠️ Ultimate: '{attacker.CharacterName}' ไม่มี UltimateRuntime");
            return;
        }

        string reason;
        if (!rt.CanUse(out reason))
        {
            Debug.LogWarning($"⛔ Ultimate ใช้ไม่ได้: {reason}");
            return;
        }

        BattleCharacter enemy = GetCurrentEnemyTarget();
        if (enemy == null)
        {
            Debug.LogWarning("⚠️ Ultimate: ไม่มีศัตรูให้โจมตี");
            return;
        }

        float dmg = rt.ComputeDamageWithTier();
        float dealt = enemy.TakeDamage(dmg, attacker);

        Debug.Log($"💥 Ultimate[{ult.skillName}] {attacker.CharacterName} -> {enemy.CharacterName} dealt={dealt:F0}");

        rt.ConsumeChargeByCurrentTier();
        rt.StartCooldown();
    }
}