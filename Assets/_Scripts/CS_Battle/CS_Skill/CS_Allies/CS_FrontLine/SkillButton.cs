using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ปุ่มสกิลแค่รูป (ชื่อสกิลอยู่ในรูปแล้ว)
/// </summary>
public class SkillButton : MonoBehaviour
{
   /* [SerializeField] private Image skillIcon;
    [SerializeField] private Image selectionHighlight; // เน้นสกิลที่เลือก

    private Button button;
    private SkillAction skill;
    private System.Action<SkillButton, SkillAction> onSkillSelected;
    private bool isSelected = false;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    /// <summary>
    /// ตั้งค่าปุ่มสกิล
    /// </summary>
    public void Initialize(SkillAction skill, System.Action<SkillButton, SkillAction> callback)
    {
        this.skill = skill;
        this.onSkillSelected = callback;

        // แสดงแค่รูปสกิล
        if (skill.skillIcon != null)
            skillIcon.sprite = skill.skillIcon;

        // ซ่อน Highlight ตอนเริ่มต้น
        SetSelected(false);
    }

    /// <summary>
    /// เมื่อกดปุ่ม
    /// </summary>
    private void OnClick()
    {
        onSkillSelected?.Invoke(this, skill);
    }

    /// <summary>
    /// แสดง/ซ่อน Highlight
    /// </summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (selectionHighlight != null)
            selectionHighlight.enabled = selected;

        // เปลี่ยนสีปุ่มให้มืด ถ้าเลือก
        Image buttonImage = GetComponent<Image>();
        buttonImage.color = selected ? new Color(0.7f, 1f, 0.7f) : Color.white;
    }

    public bool IsSelected => isSelected;*/
}