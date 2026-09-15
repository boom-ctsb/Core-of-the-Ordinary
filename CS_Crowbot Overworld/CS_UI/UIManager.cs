using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ตัวกลางเช็คว่ามี overlay เปิดอยู่มั้ย
/// + แสดง/ซ่อน Prompt text (ใช้กับ ZoneSwitcher, InteractableDialogue)
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public bool IsOverlayOpen { get; private set; } = false;

    [Header("Prompt UI (optional)")]
    [Tooltip("Text ที่แสดงคำใบ้ เช่น 'กด F เพื่อสลับ' — ปล่อยว่างได้ถ้ายังไม่มี")]
    [SerializeField] private GameObject promptPanel;   // panel ที่ครอบ text ทั้งก้อน
    [SerializeField] private Text promptText;          // UnityEngine.UI.Text

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // ซ่อน prompt ตอนเริ่ม
        HidePrompt();
    }

    // ─────────────────────────────────────────
    // Overlay state
    // ─────────────────────────────────────────

    public void SetOverlayOpen(bool open)
    {
        IsOverlayOpen = open;
    }

    // ─────────────────────────────────────────
    // Prompt — ZoneSwitcher เรียก ShowPrompt / HidePrompt
    // ─────────────────────────────────────────

    public void ShowPrompt(string message)
    {
        if (promptText != null)
            promptText.text = message;

        if (promptPanel != null)
            promptPanel.SetActive(true);
    }

    public void HidePrompt()
    {
        if (promptPanel != null)
            promptPanel.SetActive(false);

        if (promptText != null)
            promptText.text = "";
    }
}