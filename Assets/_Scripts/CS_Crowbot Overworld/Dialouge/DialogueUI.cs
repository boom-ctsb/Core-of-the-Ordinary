using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// จัดการ UI กล่องข้อความในฉาก Dialogue
/// - แสดง/ซ่อน chatbox
/// - อัปเดตชื่อผู้พูดและข้อความ
/// </summary>
public class DialogueUI : MonoBehaviour
{
    [Header("Chatbox Panel")]
    [Tooltip("GameObject ของกล่องข้อความทั้งหมด (จะ SetActive true/false)")]
    [SerializeField] private GameObject chatboxPanel;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Speaker Name Box")]
    [Tooltip("GameObject ของกล่องชื่อ — ถ้าไม่มีชื่อจะซ่อนกล่องนี้")]
    [SerializeField] private GameObject speakerNameBox;

    [Header("Next Indicator")]
    [Tooltip("ไอคอน/ลูกศร 'กด Next' — แสดงเมื่อรอ input")]
    [SerializeField] private GameObject nextIndicator;

    // ─────────────────────────────────────────

    private void Awake()
    {
        HideChatbox();
    }

    // ─────────────────────────────────────────
    // แสดงกล่องข้อความ
    // ─────────────────────────────────────────

    public void ShowText(string speaker, string text)
    {
        if (chatboxPanel != null)
            chatboxPanel.SetActive(true);

        // ชื่อผู้พูด
        if (speakerNameText != null)
            speakerNameText.text = string.IsNullOrWhiteSpace(speaker) ? "" : speaker;

        // ซ่อน/แสดงกล่องชื่อตามว่ามีชื่อมั้ย
        if (speakerNameBox != null)
            speakerNameBox.SetActive(!string.IsNullOrWhiteSpace(speaker));

        // ข้อความ
        if (dialogueText != null)
            dialogueText.text = text ?? "";

        // แสดง next indicator
        SetNextIndicator(true);
    }

    // ─────────────────────────────────────────
    // ซ่อนกล่องข้อความ (ตอน Scene Beat)
    // ─────────────────────────────────────────

    public void HideChatbox()
    {
        if (chatboxPanel != null)
            chatboxPanel.SetActive(false);

        SetNextIndicator(false);
    }

    // ─────────────────────────────────────────
    // Next indicator
    // ─────────────────────────────────────────

    public void SetNextIndicator(bool show)
    {
        if (nextIndicator != null)
            nextIndicator.SetActive(show);
    }
}