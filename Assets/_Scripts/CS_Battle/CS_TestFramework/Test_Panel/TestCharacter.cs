using UnityEngine;

public class TestCharacter : MonoBehaviour
{
    [SerializeField] private string characterName;
    [SerializeField] private float maxHP = 100f;
    [SerializeField] private float currentHP = 100f;

    [SerializeField] private float atbTimer = 0f;
    [SerializeField] private float atbMaxTime = 100f;
    [SerializeField] private float atbSpeed = 15f;  // ความเร็ว ATB

    [SerializeField] private int chargeCount = 0;   // จำนวนช่อง Charge ปัจจุบัน (0-10)
    [SerializeField] private int maxChargeCount = 10;  // จำนวนเต็มของช่อง Charge

    public string CharacterName => characterName;
    public float CurrentHP => currentHP;
    public float MaxHP => maxHP;

    public float ATBTimer => atbTimer;      // ค่าปัจจุบันของ ATB
    public float ATBMaxTime => atbMaxTime;  // ค่าสูงสุดของ ATB
    public int ChargeCount => chargeCount;  // จำนวนช่องที่ Charge อยู่
    public int MaxChargeCount => maxChargeCount; // ช่อง Charge สูงสุด

    private void Update()
    {
        // ถ้า ATB ยังไม่เต็ม → ขยับ ATB Timer
        if (atbTimer < atbMaxTime)
        {
            float fillRate = (atbSpeed / 100f) * Time.deltaTime * 100f;
            atbTimer = Mathf.Min(atbTimer + fillRate, atbMaxTime);
        }
    }

    /// <summary>
    /// รีเซตค่า ATB - เรียกจาก Panel หรือปุ่ม
    /// </summary>
    public void ResetATB()
    {
        atbTimer = 0f;
        Debug.Log($"🔄 {characterName} ATB Reset!");
    }

    /// <summary>
    /// เพิ่มจำนวนช่อง Charge ทีละ 2
    /// </summary>
    public void AddCharge(int chargeAmount = 2)
    {
        chargeCount = Mathf.Min(maxChargeCount, chargeCount + chargeAmount);
        Debug.Log($"⚡ {characterName} Charge เพิ่ม {chargeAmount}! ปัจจุบัน: {chargeCount}/{maxChargeCount}");
    }
}