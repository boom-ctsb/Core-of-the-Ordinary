using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// UI manager เบื้องต้นสำหรับแสดงข้อความชั่วคราว และ prompt ค้างบนจอ (optional)
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Message UI (temporary)")]
    public Text messageText;
    public float defaultDuration = 2f;

    [Header("Prompt UI (persistent while in zone)")]
    public Text promptText; // ตัวหนังสือที่จะแสดง "กด F เพื่อ..." (show/hide)

    [Header("Overlay / Menu (optional)")]
    [Tooltip("ถ้าเปิด overlay/menu ให้โชว์เมาส์และปลดล็อคเมาส์")]
    public bool showCursorWhenOverlayOpen = true;

    Coroutine hideCoroutine;

    public bool IsOverlayOpen { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (messageText != null) messageText.text = "";
        if (promptText != null)
        {
            promptText.text = "";
            promptText.gameObject.SetActive(false);
        }
    }

    // แสดงข้อความชั่วคราว
    public void ShowMessage(string text, float duration = -1f)
    {
        if (messageText == null) return;
        if (duration <= 0f) duration = defaultDuration;

        messageText.text = text;
        messageText.gameObject.SetActive(true);

        if (hideCoroutine != null) StopCoroutine(hideCoroutine);
        hideCoroutine = StartCoroutine(HideAfter(duration));
    }

    IEnumerator HideAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (messageText != null)
        {
            messageText.text = "";
            messageText.gameObject.SetActive(false);
        }
        hideCoroutine = null;
    }

    // Prompt: แสดงข้อความค้างบนจอจนกว่าจะเรียก HidePrompt()
    public void ShowPrompt(string text)
    {
        if (promptText == null) return;
        promptText.text = text;
        promptText.gameObject.SetActive(true);
    }

    public void HidePrompt()
    {
        if (promptText == null) return;
        promptText.text = "";
        promptText.gameObject.SetActive(false);
    }

    // ===== Overlay handling =====
    public void SetOverlayOpen(bool isOpen)
    {
        IsOverlayOpen = isOpen;

        if (!showCursorWhenOverlayOpen) return;

        if (isOpen)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}