using UnityEngine;

public class Dysnorma : BattleCharacter
{
    [Header("Combo / Stagger (Player hits enemy)")]
    [SerializeField] private int comboMax = 10;

    [Tooltip("ตัวค���ณดาเมจสำหรับ hit ที่ 10")]
    [SerializeField] private float tenthHitDamageMultiplier = 1.5f;

    [Tooltip("ถ้า true จะรีเซ็ต combo กลับ 0 ทันทีหลัง hit ที่ 10")]
    [SerializeField] private bool resetComboAfterTenthHit = true;

    public int ComboMax => comboMax;
    public int ComboCount { get; private set; } = 0;

    // ใช้ใน BattleManager ฝั่งศัตรูโจมตี (ของเดิมคุณมีเรียกอยู่)
    public int ComboCountForDebug => ComboCount;

    public float GetBasicAttackDamage()
    {
        // ตัวอย่างง่าย ๆ
        return EffectiveAttack;
    }

    public void AddCombo(int amount)
    {
        // เผื่อคุณยังมีจุดอื่นเรียก
        ComboCount = Mathf.Clamp(ComboCount + amount, 0, comboMax);
    }

    public override float TakeDamage(float rawDamage, BattleCharacter attacker)
    {
        if (Stats == null) return 0f;
        if (Stats.IsDead) return 0f;

        // 1) นับ hit ก่อน เพื่อให้ "hit ที่ 10" คือครั้งนี้
        int next = Mathf.Clamp(ComboCount + 1, 0, comboMax);
        bool isTenth = (next >= comboMax);

        // 2) คำนวณดาเมจ (ใช้สูตร base class แต่เราต้องใส่ multiplier ก่อนหัก def)
        float finalRaw = rawDamage;
        if (isTenth)
            finalRaw *= Mathf.Max(1f, tenthHitDamageMultiplier);

        float dealt = base.TakeDamage(finalRaw, attacker);

        // 3) อัปเดต combo count
        if (isTenth)
        {
            if (resetComboAfterTenthHit)
                ComboCount = 0;
            else
                ComboCount = comboMax; // ค้างเต็ม
        }
        else
        {
            ComboCount = next;
        }

        return dealt;
    }
}