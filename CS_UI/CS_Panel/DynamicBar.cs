using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// DynamicBar สำหรับแสดงผลค่าแบบ Bar
/// </summary>
public class DynamicBar : MonoBehaviour
{
    [SerializeField] private Image barFill;          // Image ของ Bar
    [SerializeField] private Color colorLow = Color.red;
    [SerializeField] private Color colorMid = Color.yellow;
    [SerializeField] private Color colorFull = Color.green;

    private void Awake()
    {
        if (barFill == null)
            barFill = GetComponent<Image>();  // ถ้า null ให้ดึงจาก Self
    }

    /// <summary>
    /// อัปเดตค่า Fill ของ Bar และปรับสี
    /// </summary>
    public void SetFillAmount(float currentValue, float maxValue)
    {
        if (maxValue <= 0 || barFill == null) return;

        // คำนวณ Fill
        float fillAmount = Mathf.Clamp01(currentValue / maxValue);
        barFill.fillAmount = fillAmount;

        // ตั้งค่าสี
        if (fillAmount <= 0.33f)
            barFill.color = colorLow;
        else if (fillAmount <= 0.66f)
            barFill.color = colorMid;
        else
            barFill.color = colorFull;
    }
}