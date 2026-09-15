using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class EpisodeManager : MonoBehaviour
{
    public static EpisodeManager Instance { get; private set; }

    [Header("Episode Config")]
    [SerializeField] private EpisodeData episodeData;

    [Header("References")]
    [SerializeField] private PlayerController playerController;

    [Header("Events")]
    public UnityEvent onAllTasksCompleted;  // เรียกเมื่อทำ task ครบ (เช่น unlock DysnormaTrigger)
    public UnityEvent onEpisodeCompleted;   // เรียกเมื่อจบ EP

    [Header("Debug")]
    [SerializeField] private bool verboseLog = false;

    // ─────────────────────────────────────────
    // State
    // ─────────────────────────────────────────

    public enum EpisodePhase
    {
        Idle,
        Intro,
        Explore,
        PreBattle,
        Battle,
        PostBattle,
        Completed
    }

    public EpisodePhase CurrentPhase { get; private set; } = EpisodePhase.Idle;

    // ─────────────────────────────────────────
    // Init
    // ─────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (episodeData == null)
        {
            Debug.LogWarning("[EpisodeManager] ไม่มี EpisodeData!");
            return;
        }

        episodeData.ResetTasks();
        StartCoroutine(EpisodeIntroRoutine());
    }

    // ─────────────────────────────────────────
    // Intro → Teleport → Explore
    // ─────────────────────────────────────────

    private IEnumerator EpisodeIntroRoutine()
    {
        SetPlayerControl(false);

        // 1) Intro Dialogue
        if (episodeData.introDialogue != null)
        {
            CurrentPhase = EpisodePhase.Intro;
            yield return new WaitUntil(() => DialogueManager.Instance != null);
            DialogueManager.Instance.StartDialogue(episodeData.introDialogue, null);
            yield return new WaitUntil(() => !DialogueManager.Instance.IsPlaying);

            if (verboseLog) Debug.Log("[EpisodeManager] Intro จบแล้ว");
        }

        // 2) Teleport
        TeleportPlayerToSpawn();

        // 3) Explore
        CurrentPhase = EpisodePhase.Explore;
        SetPlayerControl(true);

        if (verboseLog) Debug.Log("[EpisodeManager] Explore Phase เริ่มแล้ว");
    }

    // ─────────────────────────────────────────
    // Task System
    // ─────────────────────────────────────────

    /// <summary>
    /// เรียกจาก TalkInteractable / ObjectInteractable เมื่อ task สำเร็จ
    /// </summary>
    public void CompleteTask(string taskId)
    {
        if (episodeData == null) return;

        EpisodeTask task = episodeData.GetTask(taskId);
        if (task == null)
        {
            Debug.LogWarning($"[EpisodeManager] ไม่เจอ task id='{taskId}' ใน EpisodeData");
            return;
        }

        if (task.isCompleted)
        {
            if (verboseLog) Debug.Log($"[EpisodeManager] task '{taskId}' ทำแล้ว ข้าม");
            return;
        }

        task.isCompleted = true;
        if (verboseLog) Debug.Log($"[EpisodeManager] ✅ Task สำเร็จ: '{task.taskName}' ({taskId})");

        // เช็คว่าครบทุก task มั้ย
        if (episodeData.AllTasksCompleted())
        {
            if (verboseLog) Debug.Log("[EpisodeManager] ✅ Task ครบทุกอันแล้ว!");
            onAllTasksCompleted?.Invoke();
        }
    }

    /// <summary>เช็คจากภายนอก เช่น DysnormaTrigger</summary>
    public bool AreAllTasksDone() => episodeData != null && episodeData.AllTasksCompleted();

    // ─────────────────────────────────────────
    // Post Battle
    // ─────────────────────────────────────────

    public void OnBattleWon()
    {
        StartCoroutine(PostBattleRoutine());
    }

    private IEnumerator PostBattleRoutine()
    {
        CurrentPhase = EpisodePhase.PostBattle;
        // TODO: postBattle dialogue ภายหลัง
        yield return null;

        CurrentPhase = EpisodePhase.Completed;
        episodeData.isCompleted = true;
        SetPlayerControl(true);
        onEpisodeCompleted?.Invoke();

        if (verboseLog) Debug.Log("[EpisodeManager] EP Completed!");
    }

    // ─────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────

    public bool IsExploring() => CurrentPhase == EpisodePhase.Explore;

    public void SetPlayerControl(bool enabled)
    {
        if (playerController != null)
            playerController.enabled = enabled;
    }

    private void TeleportPlayerToSpawn()
    {
        if (string.IsNullOrEmpty(episodeData.spawnPointId)) return;

        GameObject spawnObj = GameObject.Find(episodeData.spawnPointId);
        if (spawnObj == null)
        {
            Debug.LogWarning($"[EpisodeManager] ไม่เจอ SpawnPoint '{episodeData.spawnPointId}'");
            return;
        }

        if (playerController == null) return;

        var rb = playerController.GetComponent<Rigidbody2D>();
        if (rb != null) rb.velocity = Vector2.zero;

        playerController.transform.position = spawnObj.transform.position;

        if (verboseLog) Debug.Log($"[EpisodeManager] Teleport → '{episodeData.spawnPointId}'");
    }
    /// <summary>ให้ Interactable เช็คสถานะ task ได้</summary>
    public EpisodeTask GetTask(string taskId)
    {
        return episodeData?.GetTask(taskId);
    }
}