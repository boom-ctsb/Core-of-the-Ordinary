using UnityEngine;

/// <summary>
/// ใช้ติดกับตัวละครที่มีระบบความร้อน (เช่น Crowbot)
/// - Temp วิ่ง 0..100
/// - ถ้า Temp >= 100 => Overheated (ใช้สกิล ThrusterDrive ไม่ได้)
/// - จะกลับมาใช้ได้เมื่อ Temp <= 0 (ตามที่คุณขอ)
/// </summary>
public class OverheatModule : MonoBehaviour
{
    [Header("Runtime")]
    [SerializeField, Range(0f, 100f)] private float temperature = 0f;

    public float Temperature => temperature;

    public bool IsOverheated => temperature >= 100f;

    public bool IsLockedUntilZero { get; private set; } = false;

    public void AddHeat(float amount)
    {
        amount = Mathf.Max(0f, amount);
        temperature = Mathf.Clamp(temperature + amount, 0f, 100f);

        if (temperature >= 100f)
            IsLockedUntilZero = true;
    }

    public void Cool(float perSecond, float dt)
    {
        if (perSecond <= 0f) return;
        dt = Mathf.Max(0f, dt);

        temperature = Mathf.Clamp(temperature - perSecond * dt, 0f, 100f);

        if (IsLockedUntilZero && temperature <= 0f)
            IsLockedUntilZero = false;
    }
}