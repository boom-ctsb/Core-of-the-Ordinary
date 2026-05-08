using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI สำหรับเลือกตัวละคร (ยังไม่ใช้ในเกมจริง แต่เตรียมไว้)
/// </summary>
public class CharacterSelectionUI : MonoBehaviour
{
    [SerializeField] private BattleManager battleManager;

    [Header("Selection Buttons")]
    [SerializeField] private Button[] allySelectionButtons = new Button[4];      // เลือก Ally
    [SerializeField] private Button[] dysnormaSelectionButtons = new Button[1];  // เลือก Dysnorma

    [Header("Display")]
    [SerializeField] private Image previewImage;
    [SerializeField] private TextMeshProUGUI previewName;
    [SerializeField] private TextMeshProUGUI previewStats;

    private int[] selectedAlliesIndices = new int[] { 0, 1, 2, 3 };  // ค่าเริ่มต้น
    private int selectedDysnormaIndex = 0;

    private void Start()
    {
        // ลิงก์ Button (เมื่อปรับใช้จริง)
        // for (int i = 0; i < allySelectionButtons.Length; i++)
        // {
        //     int index = i;
        //     allySelectionButtons[i].onClick.AddListener(() => SelectAlly(index));
        // }
    }

    /// <summary>
    /// ส่งข้อมูลการเลือกไปยัง BattleManager
    /// </summary>
    public void ConfirmSelection()
    {
        // battleManager.SetSelectedCharacters(selectedAlliesIndices, selectedDysnormaIndex);
        // SceneManager.LoadScene("BattleScene");
    }
}