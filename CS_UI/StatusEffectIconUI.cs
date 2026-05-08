using UnityEngine;
using UnityEngine.UI;

public class StatusEffectIconUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image iconImage;

    [Tooltip("ตั้ง Image Type = Filled, Fill Method = Radial360, Clockwise = true")]
    [SerializeField] private Image radialFillImage;

    public void SetData(Sprite icon, float durationSeconds, float timeRemainingSeconds)
    {
        if (iconImage != null)
            iconImage.sprite = icon;

        if (radialFillImage != null)
        {
            float d = Mathf.Max(0.0001f, durationSeconds);
            float t = Mathf.Clamp(timeRemainingSeconds, 0f, d);

            // 1 -> 0 ตามเวลา (clockwise)
            radialFillImage.fillAmount = t / d;
        }
    }
}