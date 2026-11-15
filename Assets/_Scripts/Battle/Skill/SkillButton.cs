using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ปุ่มสกิลแค่รูป (ชื่อสกิลอยู่ในรูปแล้ว)
/// </summary>
public class SkillButton : MonoBehaviour
{
    [SerializeField] private Image skillIcon;
    [SerializeField] private Image selectionHighlight;
    [SerializeField] private Image lockedOverlay;

    private Button button;
    private Image buttonImage;
    private SkillAction skill;
    private int skillIndex;
    private System.Action<SkillButton> onClicked;
    private bool isSelected;

    public SkillAction Skill => skill;
    public int SkillIndex => skillIndex;
    public bool IsSelected => isSelected;

    private void Awake()
    {
        CacheComponents();
        button.onClick.AddListener(OnClick);
    }

    private void CacheComponents()
    {
        if (button == null) button = GetComponent<Button>();
        if (buttonImage == null) buttonImage = GetComponent<Image>();
    }

    /// <summary>
    /// ตั้งค่าปุ่มสกิล (index = ตำแหน่งใน AllyCharacterData.allSkills)
    /// </summary>
    public void Initialize(SkillAction skill, int index, bool unlocked, System.Action<SkillButton> callback)
    {
        CacheComponents();

        this.skill = skill;
        this.skillIndex = index;
        this.onClicked = callback;

        if (skillIcon != null)
        {
            skillIcon.sprite = skill != null ? skill.skillIcon : null;
            skillIcon.enabled = skill != null && skill.skillIcon != null;
        }

        bool usable = skill != null && unlocked;
        button.interactable = usable;

        if (lockedOverlay != null)
            lockedOverlay.enabled = !usable;

        SetSelected(false);
    }

    private void OnClick()
    {
        onClicked?.Invoke(this);
    }

    /// <summary>
    /// แสดง/ซ่อน Highlight
    /// </summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (selectionHighlight != null)
            selectionHighlight.enabled = selected;

        if (buttonImage != null)
            buttonImage.color = selected ? new Color(0.7f, 1f, 0.7f) : Color.white;
    }
}