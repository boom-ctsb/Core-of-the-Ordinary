using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// แสดงหน้าเลือก 2 สกิล จาก 4 สกิล (บันทึกลง equippedSkill1Index / equippedSkill2Index ของ AllyCharacterData)
/// </summary>
public class SkillSelectionPanel : MonoBehaviour
{
    private const int RequiredSelectionCount = 2;

    [SerializeField] private Image portraitImage;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private Transform skillButtonContainer;
    [SerializeField] private GameObject skillButtonPrefab;

    [SerializeField] private Button confirmButton;
    [SerializeField] private TextMeshProUGUI selectedCountText; // แสดง "เลือกแล้ว: 2/2"

    private AllyCharacterData currentData;
    private readonly List<SkillButton> skillButtons = new List<SkillButton>();
    private readonly List<SkillButton> selectedButtons = new List<SkillButton>();

    private System.Action<List<SkillAction>> onSkillsConfirmed;

    private void Awake()
    {
        confirmButton.onClick.AddListener(OnConfirmSkills);
        gameObject.SetActive(false);
    }

    /// <summary>
    /// เปิดหน้าเลือกสกิลจากตัวละครในสนามรบ (ใช้ได้เฉพาะฝ่ายเรา)
    /// </summary>
    public void ShowSkillSelection(BattleCharacter character, System.Action<List<SkillAction>> callback)
    {
        AllyCharacterData data = character != null ? character.CharacterData as AllyCharacterData : null;
        if (data == null)
        {
            Debug.LogWarning("⚠️ ShowSkillSelection: ตัวละครนี้ไม่มี AllyCharacterData");
            return;
        }

        ShowSkillSelection(data, callback);
    }

    /// <summary>
    /// เปิดหน้าเลือกสกิลจากข้อมูลตัวละครโดยตรง
    /// </summary>
    public void ShowSkillSelection(AllyCharacterData data, System.Action<List<SkillAction>> callback)
    {
        if (data == null) return;

        currentData = data;
        onSkillsConfirmed = callback;
        selectedButtons.Clear();

        if (portraitImage != null)
            portraitImage.sprite = data.portraitSprite;

        if (characterNameText != null)
            characterNameText.text = data.characterName;

        CreateSkillButtons(data);
        PreselectEquippedSkills(data);

        gameObject.SetActive(true);
        UpdateSelectedCountText();
    }

    /// <summary>
    /// สร้างปุ่มสกิลให้เลือก (4 สกิล)
    /// </summary>
    private void CreateSkillButtons(AllyCharacterData data)
    {
        foreach (Transform child in skillButtonContainer)
            Destroy(child.gameObject);

        skillButtons.Clear();

        for (int i = 0; i < AllyCharacterData.MaxSkills; i++)
        {
            GameObject btnObj = Instantiate(skillButtonPrefab, skillButtonContainer);
            SkillButton skillBtn = btnObj.GetComponent<SkillButton>();

            skillBtn.Initialize(data.GetSkillByIndexRaw(i), i, data.IsSkillUnlocked(i), OnSkillClicked);
            skillButtons.Add(skillBtn);
        }
    }

    /// <summary>
    /// ติ๊กสกิลที่ equip อยู่ตอนนี้ไว้ล่วงหน้า
    /// </summary>
    private void PreselectEquippedSkills(AllyCharacterData data)
    {
        SelectIfValid(data.equippedSkill1Index);

        if (data.equippedSkill2Index != data.equippedSkill1Index)
            SelectIfValid(data.equippedSkill2Index);
    }

    private void SelectIfValid(int index)
    {
        if (index < 0 || index >= skillButtons.Count) return;

        SkillButton btn = skillButtons[index];
        if (btn.Skill == null || !currentData.IsSkillUnlocked(index)) return;

        selectedButtons.Add(btn);
        btn.SetSelected(true);
    }

    /// <summary>
    /// เมื่อกดสกิล - เลือกได้สูงสุด 2 สกิล
    /// </summary>
    private void OnSkillClicked(SkillButton skillBtn)
    {
        if (selectedButtons.Contains(skillBtn))
        {
            selectedButtons.Remove(skillBtn);
            skillBtn.SetSelected(false);
        }
        else if (selectedButtons.Count < RequiredSelectionCount)
        {
            selectedButtons.Add(skillBtn);
            skillBtn.SetSelected(true);
        }
        else
        {
            Debug.Log("⚠️ เลือกสกิลได้สูงสุด 2 สกิล!");
        }

        UpdateSelectedCountText();
    }

    /// <summary>
    /// ยืนยันการเลือกสกิล
    /// </summary>
    private void OnConfirmSkills()
    {
        if (currentData == null) return;

        if (selectedButtons.Count < RequiredSelectionCount)
        {
            Debug.Log("⚠️ ต้องเลือก 2 สกิล!");
            return;
        }

        SkillButton first = selectedButtons[0];
        SkillButton second = selectedButtons[1];

        currentData.equippedSkill1Index = first.SkillIndex;
        currentData.equippedSkill2Index = second.SkillIndex;

        Debug.Log($"✅ {currentData.characterName} เลือกสกิล: {first.Skill.skillName}, {second.Skill.skillName}");

        List<SkillAction> result = new List<SkillAction> { first.Skill, second.Skill };

        gameObject.SetActive(false);
        onSkillsConfirmed?.Invoke(result);
    }

    private void UpdateSelectedCountText()
    {
        if (selectedCountText != null)
            selectedCountText.text = $"เลือกแล้ว: {selectedButtons.Count}/{RequiredSelectionCount}";
    }
}