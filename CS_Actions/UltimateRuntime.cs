using System;
using UnityEngine;

public class UltimateRuntime : MonoBehaviour
{
    [Header("Skill")]
    [SerializeField] private UltimateSkillAction ultimateSkill;

    [Header("Tier from Charge (Threshold: 1/3/5 boxes)")]
    [SerializeField] private int[] tierChargeBoxesThreshold = new int[3] { 1, 3, 5 };

    [Tooltip("ถ้า true = ChargeTimer เป็น 'จำนวนกล่อง' เลย ไม่ต้องแปลง")]
    [SerializeField] private bool chargeValueIsBoxes = false;

    [Tooltip("จำนวนกล่องชาร์จทั้งหมด (ใช้แปลง points->boxes). ปกติ 10")]
    [SerializeField] private int chargeBoxesCount = 10;

    [Header("Consume")]
    [SerializeField] private bool consumeChargeOnUse = true;

    [Header("Cooldown")]
    [Tooltip("เริ่มเกมให้ติดคูลดาวน์เลย (ใช้ cooldownSeconds จาก UltimateSkillAction)")]
    [SerializeField] private bool startOnCooldown = true;
    [SerializeField] private float cooldownRemaining = 0f;

    public event Action<UltimateRuntime> OnChanged;

    public UltimateSkillAction Skill => ultimateSkill;
    public bool HasSkill => ultimateSkill != null;
    public float CooldownRemaining => cooldownRemaining;

    private BattleCharacter owner;

    private int lastTier = int.MinValue;
    private float lastChargeTimer = float.MinValue;
    private float lastCooldown = float.MinValue;

    public int MaxTier => 3;

    public int Tier
    {
        get
        {
            if (!HasSkill || owner == null) return 0;

            int boxes = chargeValueIsBoxes
                ? Mathf.FloorToInt(owner.ChargeTimer)
                : ChargePointsToBoxes(owner.ChargeTimer);

            int t1 = Mathf.Max(0, tierChargeBoxesThreshold[0]);
            int t2 = Mathf.Max(0, tierChargeBoxesThreshold[1]);
            int t3 = Mathf.Max(0, tierChargeBoxesThreshold[2]);

            if (boxes >= t3) return 3;
            if (boxes >= t2) return 2;
            if (boxes >= t1) return 1;
            return 0;
        }
    }

    private void Awake()
    {
        owner = GetComponent<BattleCharacter>();
        if (owner == null)
            Debug.LogError("UltimateRuntime ต้องอยู่บน GameObject ที่มี BattleCharacter", this);

        if (tierChargeBoxesThreshold == null || tierChargeBoxesThreshold.Length != 3)
            tierChargeBoxesThreshold = new int[3] { 1, 3, 5 };

        chargeBoxesCount = Mathf.Max(1, chargeBoxesCount);

        ApplyStartCooldownIfNeeded();
        FireChangedIfNeeded(force: true);
    }

    private void Update()
    {
        if (!HasSkill || owner == null) return;

        bool changed = false;

        // ✅ คูลดาวน์ทำงานเสมอ
        if (cooldownRemaining > 0f)
        {
            cooldownRemaining -= Time.deltaTime;
            if (cooldownRemaining < 0f) cooldownRemaining = 0f;
            changed = true;
        }

        if (Mathf.Abs(owner.ChargeTimer - lastChargeTimer) > 0.0001f)
            changed = true;

        if (Tier != lastTier)
            changed = true;

        if (changed)
            FireChangedIfNeeded(force: false);
    }

    private void ApplyStartCooldownIfNeeded()
    {
        if (startOnCooldown && ultimateSkill != null)
            cooldownRemaining = Mathf.Max(0f, ultimateSkill.cooldownSeconds);
    }

    private void FireChangedIfNeeded(bool force)
    {
        if (owner == null)
        {
            if (force) OnChanged?.Invoke(this);
            return;
        }

        int t = Tier;
        float ch = owner.ChargeTimer;
        float cd = cooldownRemaining;

        bool shouldFire =
            force ||
            t != lastTier ||
            Mathf.Abs(ch - lastChargeTimer) > 0.0001f ||
            Mathf.Abs(cd - lastCooldown) > 0.0001f;

        lastTier = t;
        lastChargeTimer = ch;
        lastCooldown = cd;

        if (shouldFire)
            OnChanged?.Invoke(this);
    }

    public int GetChargeBoxesNow()
    {
        if (owner == null) return 0;
        return chargeValueIsBoxes ? Mathf.FloorToInt(owner.ChargeTimer) : ChargePointsToBoxes(owner.ChargeTimer);
    }

    private int ChargePointsToBoxes(float points)
    {
        float perBox = 100f / Mathf.Max(1, chargeBoxesCount);
        if (perBox <= 0.0001f) return 0;
        return Mathf.FloorToInt(Mathf.Clamp(points, 0f, 100f) / perBox);
    }

    private float BoxesToChargePoints(int boxes)
    {
        float perBox = 100f / Mathf.Max(1, chargeBoxesCount);
        return Mathf.Max(0, boxes) * perBox;
    }

    public void SetSkill(UltimateSkillAction skill, bool resetCooldown = true)
    {
        ultimateSkill = skill;

        if (resetCooldown)
            ApplyStartCooldownIfNeeded();
        else if (ultimateSkill == null)
            cooldownRemaining = 0f;

        FireChangedIfNeeded(force: true);
    }

    public void StartCooldown()
    {
        if (!HasSkill) return;

        // ✅ คูลดาวน์ใช้เสมอ
        cooldownRemaining = Mathf.Max(0f, ultimateSkill.cooldownSeconds);
        FireChangedIfNeeded(force: true);
    }

    public void ConsumeChargeByCurrentTier()
    {
        if (!consumeChargeOnUse) return;
        if (owner == null) return;

        int t = Tier;
        if (t <= 0) return;

        int useBoxes = (t == 1) ? tierChargeBoxesThreshold[0]
                    : (t == 2) ? tierChargeBoxesThreshold[1]
                               : tierChargeBoxesThreshold[2];

        if (chargeValueIsBoxes)
            owner.ChargeTimer -= useBoxes;
        else
            owner.ChargeTimer -= BoxesToChargePoints(useBoxes);

        FireChangedIfNeeded(force: true);
    }

    public float ComputeDamageWithTier()
    {
        if (!HasSkill || owner == null) return 0f;

        float baseDmg = ultimateSkill.ComputeDamage(owner);
        float mul = ComputeTierMultiplier(Tier);
        return Mathf.Max(0f, baseDmg * mul);
    }

    private float ComputeTierMultiplier(int tier)
    {
        if (tier <= 1) return 1f;
        if (tier == 2) return 1.5f;
        return 2f;
    }

    // ✅ ทุกเงื่อนไขต้องผ่านคูลดาวน์เสมอ
    public bool CanUse(out string reason)
    {
        reason = "";

        if (!HasSkill) { reason = "no ultimate skill"; return false; }
        if (owner == null || owner.Stats == null || owner.Stats.IsDead) { reason = "owner invalid"; return false; }

        // --- เงื่อนไขตาม costType ---
        if (ultimateSkill.costType == UltimateCostType.Charge)
        {
            if (owner.ChargeTimer < ultimateSkill.requiredCharge)
            {
                reason = "not enough charge";
                return false;
            }
        }
        else if (ultimateSkill.costType == UltimateCostType.Shield)
        {
            if (owner.ShieldHP < ultimateSkill.requiredShield)
            {
                reason = "not enough shield";
                return false;
            }
        }

        // --- Tier ขั้นต่ำ ---
        if (Tier <= 0) { reason = "tier 0"; return false; }

        // --- คูลดาวน์เสมอ ---
        if (cooldownRemaining > 0f)
        {
            reason = "cooldown";
            return false;
        }

        return true;
    }

    public float GetCooldownPercent01()
    {
        if (!HasSkill) return 0f;

        float total = Mathf.Max(0.0001f, ultimateSkill.cooldownSeconds);
        return Mathf.Clamp01(1f - (cooldownRemaining / total));
    }
}