using System.Collections;
using UnityEngine;

/// <summary>
/// ติดกับ NPC หรือตัวละคร Playable ใน Overworld
/// trigger = PressKey (กด Space ใกล้ๆ) หรือ AutoOnEnter (เข้าใกล้แล้วเข้าฉาก Dialogue เลย)
/// เล่น Dialogue → Complete Task (เฉพาะเมื่อ task อยู่ในขั้น Explore ปัจจุบัน)
/// หลัง task สำเร็จ กลับมาคุยอีกจะเล่น repeatDialogueData (Repeat Beat)
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class TalkInteractable : MonoBehaviour
{
    [Header("Task")]
    [Tooltip("ต้องตรงกับ taskId ใน EpisodeData (steps[].tasks[])\nถ้าว่าง = ไม่ผูกกับ task ไหน")]
    [SerializeField] private string taskId;

    [Header("Trigger")]
    [Tooltip("PressKey = เข้าใกล้แล้วกดปุ่ม\nAutoOnEnter = เข้าใกล้แล้วเข้าฉาก Dialogue ทันที (ถ้ามี task จะเล่นเฉพาะตอน task ทำได้)")]
    [SerializeField] private InteractTrigger trigger = InteractTrigger.PressKey;

    [Header("Dialogue")]
    [SerializeField] private DialogueData dialogueData;

    [Tooltip("Dialogue แบบ Repeat Beat — เล่นเมื่อกลับมาคุยอีกหลัง task นี้สำเร็จแล้ว (ว่าง = ไม่ตอบสนอง)\nถ้าไม่ได้ผูก task จะเล่น dialogueData ทุกครั้งตามปกติ")]
    [SerializeField] private DialogueData repeatDialogueData;

    [Header("Hint UI")]
    [SerializeField] private GameObject hintUI;

    [Header("Input")]
    [SerializeField] private KeyCode interactKey = KeyCode.Space;

    [Header("Debug")]
    [SerializeField] private bool verboseLog = false;

    private bool _playerNearby = false;
    private PlayerController _playerRef = null;
    private bool _autoFired = false;
    private bool _hintShown = false;

    // ─────────────────────────────────────────

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Awake()
    {
        if (hintUI != null) hintUI.SetActive(false);
    }

    // ─────────────────────────────────────────
    // Trigger
    // ─────────────────────────────────────────

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc == null) return;

        _playerNearby = true;
        _playerRef = pc;
        _autoFired = false;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc == null) return;

        _playerNearby = false;
        _playerRef = null;
        SetHint(false);
    }

    // ─────────────────────────────────────────
    // Input
    // ─────────────────────────────────────────

    private void Update()
    {
        if (!_playerNearby) return;

        bool busy = (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying)
                    || (UIManager.Instance != null && UIManager.Instance.IsOverlayOpen)
                    || (EpisodeManager.Instance != null && !EpisodeManager.Instance.IsExploring());

        if (trigger == InteractTrigger.AutoOnEnter)
        {
            if (busy || _autoFired) return;

            // มี task ผูกอยู่ → เข้าฉากอัตโนมัติเฉพาะตอน task ทำได้ (กันเล่นซ้ำ/เล่นก่อนถึงขั้น)
            if (!string.IsNullOrEmpty(taskId) && EpisodeManager.Instance != null
                && !EpisodeManager.Instance.IsTaskAvailable(taskId))
                return;

            _autoFired = true;
            TryInteract();
            return;
        }

        SetHint(!busy);
        if (busy) return;

        if (Input.GetKeyDown(interactKey))
            TryInteract();
    }

    // ─────────────────────────────────────────
    // Interact
    // ─────────────────────────────────────────

    private void TryInteract()
    {
        EpisodeTask task = GetTask();
        bool canComplete = task != null && !task.isCompleted
                           && (EpisodeManager.Instance == null || EpisodeManager.Instance.IsTaskAvailable(taskId));

        SetHint(false);

        // task ทำไปแล้ว → Repeat Beat / task ของขั้นที่ยังมาไม่ถึง → ยังไม่ตอบสนอง
        if (task != null && !canComplete)
        {
            if (task.isCompleted && repeatDialogueData != null && DialogueManager.Instance != null)
                DialogueManager.Instance.StartDialogue(repeatDialogueData, _playerRef);
            return;
        }

        if (dialogueData != null && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(dialogueData, _playerRef);

            if (canComplete && task.requiresDialogue)
                StartCoroutine(CompleteAfterDialogue());
            else if (canComplete)
                TryCompleteTask();
        }
        else if (canComplete)
        {
            TryCompleteTask();
        }

        if (verboseLog) Debug.Log($"[TalkInteractable] interact: {gameObject.name}, taskId='{taskId}'");
    }

    private IEnumerator CompleteAfterDialogue()
    {
        yield return new WaitUntil(() =>
            DialogueManager.Instance == null || !DialogueManager.Instance.IsPlaying);

        TryCompleteTask();
    }

    private void TryCompleteTask()
    {
        if (string.IsNullOrEmpty(taskId)) return;
        if (EpisodeManager.Instance == null) return;
        EpisodeManager.Instance.CompleteTask(taskId);
    }

    private EpisodeTask GetTask()
    {
        if (string.IsNullOrEmpty(taskId)) return null;
        if (EpisodeManager.Instance == null) return null;
        return EpisodeManager.Instance.GetTask(taskId);
    }

    private void SetHint(bool show)
    {
        if (_hintShown == show) return;
        _hintShown = show;
        if (hintUI != null) hintUI.SetActive(show);
    }
}