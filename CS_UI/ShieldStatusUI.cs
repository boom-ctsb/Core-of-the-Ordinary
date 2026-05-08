using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShieldStatusUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI valueText;

    [Header("Optional Icon")]
    [SerializeField] private Sprite shieldIcon;

    public void SetValue(float shieldValue)
    {
        if (iconImage != null && shieldIcon != null)
            iconImage.sprite = shieldIcon;

        if (valueText != null)
            valueText.text = Mathf.CeilToInt(shieldValue).ToString();
    }
}