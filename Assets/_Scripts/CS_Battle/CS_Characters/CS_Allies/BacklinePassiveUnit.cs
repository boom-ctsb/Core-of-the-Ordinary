using UnityEngine;

public interface IRuntimeBuffIconOwner
{
    void AddOrRefreshOwnedBuffIcon(TimedBuffInstance iconBuff);
}

public class BacklinePassiveUnit : MonoBehaviour, IRuntimeBuffIconOwner
{
    [Header("Data")]
    [SerializeField] private AllyBacklineData data;

    [Header("Refs")]
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private BacklinePanelUI panelUI;

    [Header("Target (optional)")]
    [Tooltip("ใช้เป็น self/allySingle ถ้า Skill ต้องการ")]
    [SerializeField] private BattleCharacter proxySelf;

    private float timebar = 0f;
    private float fillWindowRemaining = 0f;
    private bool isFilling = false;

    private readonly System.Collections.Generic.List<TimedBuffInstance> ownedBuffIcons = new System.Collections.Generic.List<TimedBuffInstance>();
    public event System.Action<BacklinePassiveUnit> OnOwnedBuffIconListChanged;

    public float Timebar => timebar;
    public float TimebarMax => data != null ? Mathf.Max(1f, data.timebarMax) : 100f;

    public bool IsFillWindowActive => isFilling && fillWindowRemaining > 0f;
    public float FillWindowRemaining => fillWindowRemaining;
    public float FillWindowDuration => data != null ? Mathf.Max(0.01f, data.fillWindowSeconds) : 1f;

    public float Attack => data != null ? data.attack : 0f;
    public float Defense => data != null ? data.defense : 0f;
    public float Speed => data != null ? data.speed : 0f;
    public Sprite PortraitSprite => data != null ? data.portraitSprite : null;
    public BacklineTrigger TriggerCondition => data != null ? data.triggerCondition : BacklineTrigger.OnEnemyStunned;
    public float PassiveTickProgress01 => TriggerCondition == BacklineTrigger.PassiveTimer ? Mathf.Clamp01(timebar / TimebarMax) : 0f;

    public System.Collections.Generic.IReadOnlyList<TimedBuffInstance> GetOwnedBuffIconsReadOnly() => ownedBuffIcons;

    private void Awake()
    {
        if (battleManager == null)
            battleManager = FindObjectOfType<BattleManager>();

        if (panelUI != null)
            panelUI.Bind(this);
    }

    // ✅ เพิ่มให้ BattleManager เรียก
    public void Initialize(BattleManager mgr)
    {
        battleManager = mgr;
        if (panelUI != null)
            panelUI.Bind(this);
    }

    private void OnEnable()
    {
        SkillExecutor.OnStunApplied += HandleStunApplied;
        BattleManager.OnAllyTookDamage += HandleAllyHit;
        BattleManager.OnAllyUsedSkill += HandleAllyUsedSkill;
    }

    private void OnDisable()
    {
        SkillExecutor.OnStunApplied -= HandleStunApplied;
        BattleManager.OnAllyTookDamage -= HandleAllyHit;
        BattleManager.OnAllyUsedSkill -= HandleAllyUsedSkill;
    }

    private void Update()
    {
        if (data == null) return;

        TickOwnedBuffIcons(Time.deltaTime);

        if (TriggerCondition == BacklineTrigger.PassiveTimer)
        {
            TickTimebar(Time.deltaTime);
            if (panelUI != null)
                panelUI.Refresh();
            return;
        }

        if (!IsFillWindowActive) return;

        float dt = Time.deltaTime;
        fillWindowRemaining -= dt;
        if (fillWindowRemaining < 0f) fillWindowRemaining = 0f;

        TickTimebar(dt);

        if (fillWindowRemaining <= 0f)
            StopFillWindow();

        if (panelUI != null)
            panelUI.Refresh();
    }

    private void HandleStunApplied(BattleCharacter attacker, BattleCharacter target, SkillAction skill)
    {
        if (TriggerCondition != BacklineTrigger.OnEnemyStunned) return;
        if (data == null) return;
        if (skill == null) return;
        if (target == null || target.CharacterData == null) return;

        if (target.CharacterData.faction != CharacterFaction.Enemy)
            return;

        StartFillWindow();
    }

    private void HandleAllyHit(BattleCharacter ally)
    {
        if (TriggerCondition != BacklineTrigger.OnAllyHit) return;
        if (ally == null) return;
        StartFillWindow();
    }

    private void HandleAllyUsedSkill(BattleCharacter ally, SkillAction skill)
    {
        if (TriggerCondition != BacklineTrigger.OnAllyUsedSkill) return;
        if (ally == null || skill == null) return;
        StartFillWindow();
    }

    private void StartFillWindow()
    {
        if (data == null) return;

        fillWindowRemaining = Mathf.Max(0.01f, data.fillWindowSeconds);
        isFilling = true;

        if (panelUI != null)
            panelUI.Refresh();
    }

    private void StopFillWindow()
    {
        isFilling = false;
        fillWindowRemaining = 0f;

        if (panelUI != null)
            panelUI.Refresh();
    }

    private void ResetTimebar()
    {
        timebar = 0f;
    }

    private void TickTimebar(float dt)
    {
        if (data == null) return;

        float safeBaseline = Mathf.Max(0.0001f, data.speedBaseline);
        float speedFactor = Mathf.Max(0.1f, Speed / safeBaseline);
        float baseFillPerSecond = TimebarMax * data.baseFillPercentPerSecond;

        timebar += dt * baseFillPerSecond * speedFactor;
        timebar = Mathf.Clamp(timebar, 0f, TimebarMax);

        if (timebar >= TimebarMax)
        {
            ExecuteBacklineSkill();
            ResetTimebar();

            if (TriggerCondition != BacklineTrigger.PassiveTimer)
                StopFillWindow();
        }
    }

    private void ExecuteBacklineSkill()
    {
        if (battleManager == null) return;
        if (data == null || data.passiveSkill == null) return;

        var targets = battleManager.GetTargetsForBacklineSkill(data.passiveSkill, proxySelf);
        if (targets == null || targets.Count == 0)
        {
            Debug.LogWarning($"⚠️ Backline '{data.characterName}' ไม่มี target ให้ใช้สกิล '{data.passiveSkill.skillName}'");
            return;
        }

        bool ok = SkillExecutor.TryExecuteSkill_Backline(
            user: this,
            skill: data.passiveSkill,
            targets: targets,
            failReason: out string reason
        );

        if (!ok)
            Debug.LogWarning($"⛔ Backline ใช้สกิลไม่สำเร็จ: {data.passiveSkill.skillName} reason={reason}");
    }

    public void SetData(AllyBacklineData newData)
    {
        data = newData;

        if (TriggerCondition == BacklineTrigger.PassiveTimer)
            isFilling = false;

        if (panelUI != null)
            panelUI.Refresh();
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
                    OnOwnedBuffIconListChanged?.Invoke(this);
                    return;
                }
            }
        }

        ownedBuffIcons.Add(iconBuff);
        OnOwnedBuffIconListChanged?.Invoke(this);
    }

    private void TickOwnedBuffIcons(float dt)
    {
        if (ownedBuffIcons.Count == 0) return;

        bool removedAny = false;
        for (int i = ownedBuffIcons.Count - 1; i >= 0; i--)
        {
            TimedBuffInstance icon = ownedBuffIcons[i];
            if (icon == null)
            {
                ownedBuffIcons.RemoveAt(i);
                removedAny = true;
                continue;
            }

            icon.Tick(dt);
            if (icon.IsExpired)
            {
                ownedBuffIcons.RemoveAt(i);
                removedAny = true;
            }
        }

        if (removedAny)
            OnOwnedBuffIconListChanged?.Invoke(this);
    }

    public Sprite GetConditionIcon()
    {
        if (data == null) return null;
        if (data.conditionIconOverride != null) return data.conditionIconOverride;
        if (data.passiveSkill != null && data.passiveSkill.skillIcon != null) return data.passiveSkill.skillIcon;
        return null;
    }
}