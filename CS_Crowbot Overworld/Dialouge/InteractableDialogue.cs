using UnityEngine;

/// <summary>
/// ติดกับ NPC หรือ Object ใน Overworld
/// - ตรวจว่า Player อยู่ใน range มั้ย (ใช้ Trigger Collider)
/// - แสดง hint UI "กด Space เพื่อคุย"
/// - เมื่อกด Space → ส่ง DialogueData ไปที่ DialogueManager
/// </summary>
public class InteractableDialogue : MonoBehaviour
{
    [Header("Data")]
    [Tooltip("บทสนทนาของ NPC / Object นี้")]
    [SerializeField] private DialogueData dialogueData;

    [Header("Interact Hint UI")]
    [Tooltip("GameObject ที่แสดง hint 'กด Space เพื่อคุย'\nจะแสดงเมื่อ Player เข้ามาใกล้")]
    [SerializeField] private GameObject interactHintUI;

    [Header("Input")]
    [SerializeField] private KeyCode interactKey = KeyCode.Space;

    [Header("Debug")]
    [SerializeField] private bool verboseLog = false;

    // ─────────────────────────────────────────
    // State
    // ─────────────────────────────────────────

    private bool playerInRange = false;
    private PlayerController playerInRangeRef = null;

    // ─────────────────────────────────────────
    // Init
    // ─────────────────────────────────────────

    private void Awake()
    {
        // ซ่อน hint ตอนเริ่ม
        SetHint(false);
    }

    // ─────────────────────────────────────────
    // Update — รับ input เมื่ออยู่ใน range
    // ─────────────────────────────────────────

    private void Update()
    {
        if (!playerInRange) return;
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying) return;

        if (Input.GetKeyDown(interactKey))
            TryStartDialogue();
    }

    // ─────────────────────────────────────────
    // Trigger — ตรวจ Player เข้า/ออก range
    // ─────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc == null) return;

        playerInRange = true;
        playerInRangeRef = pc;
        SetHint(true);

        if (verboseLog) Debug.Log($"[InteractableDialogue] Player เข้า range: {gameObject.name}");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc == null) return;

        playerInRange = false;
        playerInRangeRef = null;
        SetHint(false);

        if (verboseLog) Debug.Log($"[InteractableDialogue] Player ออก range: {gameObject.name}");
    }

    // ─────────────────────────────────────────
    // เริ่ม Dialogue
    // ─────────────────────────────────────────

    private void TryStartDialogue()
    {
        if (dialogueData == null)
        {
            Debug.LogWarning($"[InteractableDialogue] '{gameObject.name}' ไม่มี DialogueData");
            return;
        }

        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("[InteractableDialogue] ไม่เจอ DialogueManager ใน Scene"); 
            return;
        }

        SetHint(false);
        DialogueManager.Instance.StartDialogue(dialogueData, playerInRangeRef);
    }

    // ─────────────────────────────────────────
    // Helper
    // ─────────────────────────────────────────

    private void SetHint(bool show)
    {
        if (interactHintUI != null)
            interactHintUI.SetActive(show);
    }
}