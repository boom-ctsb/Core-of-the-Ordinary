using UnityEngine;
using UnityEngine.UI;

public class UIPageSwitcher : MonoBehaviour
{
    [Header("Pages / Panels")]
    [SerializeField] private GameObject storyTaskPanel;     // รูป 1
    [SerializeField] private GameObject alliesSelectPanel;  // รูป 2

    [Header("Buttons (from Story/Task panel)")]
    [SerializeField] private Button storyAcceptButton;      // ไปหน้าเลือกตัว
    [SerializeField] private Button storyExitButton;        // ✅ ปิดหน้า Story/Task

    [Header("Buttons (from Allies Selection panel)")]
    [SerializeField] private Button alliesReturnButton;     // ✅ กลับไป Story/Task
    [SerializeField] private Button alliesAcceptButton;     // (ถ้าต้องการทำต่อ)

    private void Awake()
    {
        if (storyAcceptButton != null)
            storyAcceptButton.onClick.AddListener(OpenAlliesSelection);

        if (storyExitButton != null)
            storyExitButton.onClick.AddListener(CloseStoryTaskPanel);

        if (alliesReturnButton != null)
            alliesReturnButton.onClick.AddListener(OpenStoryTask);

        // alliesAcceptButton: แล้วแต่คุณว่าจะให้ทำอะไร (เช่นเริ่มฉากต่อสู้)
    }

    public void OpenAlliesSelection()
    {
        if (storyTaskPanel != null) storyTaskPanel.SetActive(false);
        if (alliesSelectPanel != null) alliesSelectPanel.SetActive(true);
    }

    public void OpenStoryTask()
    {
        if (alliesSelectPanel != null) alliesSelectPanel.SetActive(false);
        if (storyTaskPanel != null) storyTaskPanel.SetActive(true);
    }

    public void CloseStoryTaskPanel()
    {
        // ✅ ปิดแค่ Story/Task panel ตามที่ขอ
        if (storyTaskPanel != null) storyTaskPanel.SetActive(false);
    }
}