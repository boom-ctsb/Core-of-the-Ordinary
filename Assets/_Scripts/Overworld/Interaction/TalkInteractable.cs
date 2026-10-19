using UnityEngine;

/// <summary>
/// ติดกับ NPC หรือตัวละคร Playable ใน Overworld
/// กด Space เมื่ออยู่ใกล้ → เล่น Dialogue → Complete Task
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class TalkInteractable : MonoBehaviour
{
    [Header("Task")]
    [Tooltip("ต้องตรงกับ taskId ใน EpisodeData.tasks[]\nถ้าว่าง = ไม่ผูกกับ task ไหน")]
    [SerializeField] private string taskId;

    [Header("Dialogue")]
    [SerializeField] private DialogueData dialogueData;

    [Header("Hint UI")]
    [SerializeField] private GameObject hintUI;

    [Header("Input")]
    [SerializeField] private KeyCode interactKey = KeyCode.Space;

    [Header("Debug")]
    [SerializeField] private bool verboseLog = false;

    private bool _playerNearby = false;
    private PlayerController _playerRef = null;

    // ─────────────────────────────────────────

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Awake()
    {
        SetHint(false);
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
        SetHint(true);
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

        // ห้าม interact ถ้า dialogue กำลังเล่นอยู่
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying) return;

        if (UIManager.Instance != null && UIManager.Instance.IsOverlayOpen) return;

        // ห้าม interact ถ้าไม่ได้อยู่ใน Explore phase
        if (EpisodeManager.Instance != null && !EpisodeManager.Instance.IsExploring()) return;

        if (Input.GetKeyDown(interactKey))
            TryInteract();
    }

    // ─────────────────────────────────────────
    // Interact
    // ─────────────────────────────────────────

    private void TryInteract()
    {
        EpisodeTask task = GetTask();

        // ถ้า task นี้ทำแล้ว — เล่น dialogue ซ้ำได้ แต่ไม่ complete อีก
        bool alreadyDone = task != null && task.isCompleted;

        if (dialogueData != null)
        {
            // มี dialogue → เล่นก่อน แล้วค่อย complete task ตอนจบ
            SetHint(false);
            DialogueManager.Instance.StartDialogue(dialogueData, _playerRef);

            if (!alreadyDone && task != null && task.requiresDialogue)
                StartCoroutine(CompleteAfterDialogue());
        }
        else
        {
            // ไม่มี dialogue → complete ทันที
            if (!alreadyDone)
                TryCompleteTask();
        }

        if (verboseLog) Debug.Log($"[TalkInteractable] interact: {gameObject.name}, taskId='{taskId}'");
    }

    private System.Collections.IEnumerator CompleteAfterDialogue()
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
        // ดึงจาก EpisodeData ผ่าน EpisodeManager
        return EpisodeManager.Instance.GetTask(taskId);
    }

    private void SetHint(bool show)
    {
        if (hintUI != null) hintUI.SetActive(show);
    }
}