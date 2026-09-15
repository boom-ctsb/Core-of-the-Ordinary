using UnityEngine;

/// <summary>
/// ติดกับ Object ในฉาก — Collect / Clue / Destroy
/// กด Space เมื่ออยู่ใกล้ → (dialogue ถ้ามี) → Complete Task
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ObjectInteractable : MonoBehaviour
{
    [Header("Task")]
    [Tooltip("ต้องตรงกับ taskId ใน EpisodeData.tasks[]")]
    [SerializeField] private string taskId;

    [Header("Dialogue (optional)")]
    [Tooltip("ถ้าว่าง = ไม่มี dialogue, complete task ทันที")]
    [SerializeField] private DialogueData dialogueData;

    [Header("Behaviour หลัง Complete")]
    [Tooltip("ซ่อน GameObject นี้หลัง interact สำเร็จ\n(ใช้กับ Collect / Destroy)")]
    [SerializeField] private bool hideOnComplete = true;

    [Header("Hint UI")]
    [SerializeField] private GameObject hintUI;

    [Header("Input")]
    [SerializeField] private KeyCode interactKey = KeyCode.Space;

    [Header("Debug")]
    [SerializeField] private bool verboseLog = false;

    private bool _playerNearby = false;
    private PlayerController _playerRef = null;
    private bool _isDone = false;

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

        // ถ้าทำแล้ว ไม่แสดง hint อีก
        if (!_isDone) SetHint(true);
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
        if (!_playerNearby || _isDone) return;
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying) return;
        if (EpisodeManager.Instance != null && !EpisodeManager.Instance.IsExploring()) return;

        if (Input.GetKeyDown(interactKey))
            TryInteract();
    }

    // ─────────────────────────────────────────
    // Interact
    // ─────────────────────────────────────────

    private void TryInteract()
    {
        SetHint(false);

        if (dialogueData != null)
        {
            // มี dialogue → เล่นก่อน แล้วค่อย complete
            DialogueManager.Instance.StartDialogue(dialogueData, _playerRef);
            StartCoroutine(CompleteAfterDialogue());
        }
        else
        {
            // ไม่มี dialogue → complete ทันที
            OnComplete();
        }

        if (verboseLog) Debug.Log($"[ObjectInteractable] interact: {gameObject.name}, taskId='{taskId}'");
    }

    private System.Collections.IEnumerator CompleteAfterDialogue()
    {
        yield return new WaitUntil(() =>
            DialogueManager.Instance == null || !DialogueManager.Instance.IsPlaying);

        OnComplete();
    }

    private void OnComplete()
    {
        _isDone = true;

        // แจ้ง EpisodeManager
        if (!string.IsNullOrEmpty(taskId) && EpisodeManager.Instance != null)
            EpisodeManager.Instance.CompleteTask(taskId);

        // ซ่อน Object (Collect/Destroy)
        if (hideOnComplete)
            gameObject.SetActive(false);
    }

    private void SetHint(bool show)
    {
        if (hintUI != null) hintUI.SetActive(show);
    }
}