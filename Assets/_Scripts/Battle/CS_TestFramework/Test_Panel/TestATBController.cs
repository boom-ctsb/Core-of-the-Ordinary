using UnityEngine;

/// <summary>
/// ทดสอบ TimeBar ทำงาน 0 → 100 แบบวนเวียน
/// (ตอนนี้: เต็มแล้วหยุด รอกดปุ่ม Reset)
/// </summary>
public class TestATBController : MonoBehaviour
{
    [SerializeField] private float atbTimer = 0f;
    [SerializeField] private float atbMaxTime = 100f;
    [SerializeField] private float atbSpeed = 15f;

    private bool isATBPaused = false;  // ← ตรวจสอบว่า ATB หยุดหรือยัง

    private void Update()
    {
        // ✅ ถ้า ATB หยุด → ไม่นับต่อ
        if (isATBPaused)
            return;

        // ATB ขยับตามค่า Speed
        float fillRate = (atbSpeed / 100f) * Time.deltaTime * 100f;
        atbTimer += fillRate;

        // ✅ ตอนเต็ม → หยุดการนับ (ไม่รีเซตอัตโนมัติ)
        if (atbTimer >= atbMaxTime)
        {
            atbTimer = atbMaxTime;  // ให้อยู่ที่ 100 เท่านั้น
            isATBPaused = true;     // หยุดการนับ
            Debug.Log("⏹️ ATB Full! Paused. รอกดปุ่ม Attack...");
        }
    }

    /// <summary>
    /// รีเซต ATB (เรียกจาก Button Attack)
    /// </summary>
    public void ResetATB()
    {
        atbTimer = 0f;
        isATBPaused = false;
        Debug.Log("🔄 ATB Reset! เดินต่อ...");
    }

    /// <summary>
    /// ให้ BattleCharacter อ่านค่า ATB
    /// </summary>
    public float GetATBTimer()
    {
        return atbTimer;
    }

    public float GetATBMaxTime()
    {
        return atbMaxTime;
    }
}