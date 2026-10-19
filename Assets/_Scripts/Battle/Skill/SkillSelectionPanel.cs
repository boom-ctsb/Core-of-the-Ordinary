using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// แสดงหน้าเลือก 2 สกิล จาก 5 สกิล
/// </summary>
public class SkillSelectionPanel : MonoBehaviour
{
    /*[SerializeField] private Image portraitImage;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private Transform skillButtonContainer;
    [SerializeField] private GameObject skillButtonPrefab;
    
    [SerializeField] private Button confirmButton;
    [SerializeField] private TextMeshProUGUI selectedCountText; // แสดง "เลือกแล้ว: 2/2"
    
    private BattleCharacter currentCharacter;
    private List<SkillButton> skillButtons = new List<SkillButton>();
    private List<SkillAction> selectedSkills = new List<SkillAction>();
    
    private System.Action<List<SkillAction>> onSkillsConfirmed;

    private void Awake()
    {
        confirmButton.onClick.AddListener(OnConfirmSkills);
        gameObject.SetActive(false);
    }

    /// <summary>
    /// เปิดหน้าเลือกสกิล
    /// </summary>
    public void ShowSkillSelection(BattleCharacter character, System.Action<List<SkillAction>> callback)
    {
        currentCharacter = character;
        onSkillsConfirmed = callback;
        selectedSkills.Clear();

        // อัปเดต UI
        portraitImage.sprite = character.CharacterData.portraitSprite;
        characterNameText.text = character.CharacterName;

        // สร้างปุ่มสกิล
        CreateSkillButtons(character);

        // แสดง Panel
        gameObject.SetActive(true);
        UpdateSelectedCountText();
    }

    /// <summary>
    /// สร้างปุ่มสกิลให้เลือก (5 สกิล)
    /// </summary>
    private void CreateSkillButtons(BattleCharacter character)
    {
        // ลบปุ่มเก่า
        foreach (Transform child in skillButtonContainer)
        {
            Destroy(child.gameObject);
        }
        skillButtons.Clear();

        // สร้างปุ่มใหม่
        for (int i = 0; i < character.AvailableSkills.Count; i++)
        {
            GameObject btnObj = Instantiate(skillButtonPrefab, skillButtonContainer);
            SkillButton skillBtn = btnObj.GetComponent<SkillButton>();

            skillBtn.Initialize(character.AvailableSkills[i],i,OnSkillSelected);

            skillButtons.Add(skillBtn);
        }
    }
    /// <summary>
    /// เมื่อกดสกิล - เลือกได้สูงสุด 2 สกิล
    /// </summary>
    private void OnSkillSelected(SkillButton skillBtn, SkillAction skill)
    {
        if (selectedSkills.Contains(skill))
        {
            // ถ้ากดอีกครั้ง = ยกเลิกเลือก
            selectedSkills.Remove(skill);
            skillBtn.SetSelected(false);
        }
        else
        {
            // ถ้าเลือ��น้อยกว่า 2 = สามารถเลือกได้
            if (selectedSkills.Count < 2)
            {
                selectedSkills.Add(skill);
                skillBtn.SetSelected(true);
            }
            else
            {
                Debug.Log("⚠️ เลือกสกิลได้สูงสุด 2 สกิล!");
            }
        }

        UpdateSelectedCountText();
    }

    /// <summary>
    /// ยืนยันการเลือกสกิล
    /// </summary>
    private void OnConfirmSkills()
    {
        if (selectedSkills.Count < 2)
        {
            Debug.Log("⚠️ ต้องเลือก 2 สกิล!");
            return;
        }

        Debug.Log($"✅ {currentCharacter.CharacterName} เลือกสกิล: {selectedSkills[0].skillName}, {selectedSkills[1].skillName}");
        
        // บันทึกสกิลที่เลือก
        currentCharacter.SelectSkills(selectedSkills);

        // ปิด Panel
        gameObject.SetActive(false);

        // เรียก Callback
        onSkillsConfirmed?.Invoke(selectedSkills);
    }

    private void UpdateSelectedCountText()
    {
        selectedCountText.text = $"เลือกแล้ว: {selectedSkills.Count}/2";
    }*/
}