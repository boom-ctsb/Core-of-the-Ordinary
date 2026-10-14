using TMPro;
using UnityEngine;

/// <summary>
/// แสดงอุณหภูมิของตัวละคร (Crowbot)
/// - ผูกกับ TextMeshProUGUI
/// - อ่านค่าจาก OverheatModule
/// </summary>
public class TemperatureUI : MonoBehaviour
{
    [SerializeField] private OverheatModule overheat;
    [SerializeField] private TextMeshProUGUI temperatureText;

    [Header("Format")]
    [SerializeField] private string format = "TEMP {0:0}%";

    private void Awake()
    {
        if (overheat == null)
            overheat = GetComponentInParent<OverheatModule>();

        if (temperatureText == null)
            temperatureText = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (temperatureText == null) return;

        float temp = 0f;
        if (overheat != null) temp = overheat.Temperature;

        temperatureText.text = string.Format(format, temp);
    }
}