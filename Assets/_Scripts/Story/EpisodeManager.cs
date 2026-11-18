using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// คุม EP ทั้งตอน: Intro → เดินตาม steps (Explore / Battle / Dialogue) ทีละขั้น → จบ EP
/// วางไว้ในซีนเนื้อเรื่อง 1 ตัว แล้วใส่ EpisodeData
/// </summary>
public class EpisodeManager : MonoBehaviour
{
    public static EpisodeManager Instance { get; private set; }

    [Header("Episode Config")]
    [SerializeField] private EpisodeData episodeData;

    [Header("References")]
    [SerializeField] private PlayerController playerController;

    [Tooltip("ถ้าใส่ จะ fade จอดำก่อนเข้าซีนสู้ (optional)")]
    [SerializeField] private FadeController fadeController;

    [Header("Events")]
    public UnityEvent onAllTasksCompleted;  // เรียกเมื่อทำ task ครบของ Explore step (ทุกขั้น)
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
        StepDialogue,    // กำลังเล่น dialogue ของขั้น
        Explore,         // เดินสำรวจ/interact ได้
        StepTransition,  // task ครบแล้ว รอเปลี่ยนขั้น (ห้าม interact)
        PreBattle,       // (คงไว้เพื่อความเข้ากันได้กับของเดิม)
        Battle,
        PostBattle,      // (คงไว้เพื่อความเข้ากันได้กับของเดิม)
        Completed
    }

    public EpisodePhase CurrentPhase { get; private set; } = EpisodePhase.Idle;

    public int CurrentStepIndex { get; private set; } = -1;

    public EpisodeStep CurrentStep
    {
        get
        {
            if (episodeData == null || CurrentStepIndex < 0) return null;
            EpisodeStep[] steps = episodeData.GetRuntimeSteps();
            return CurrentStepIndex < steps.Length ? steps[CurrentStepIndex] : null;
        }
    }

    public string CurrentStepId => CurrentStep != null ? CurrentStep.stepId : null;

    /// <summary>task ของขั้นปัจจุบัน (ว่างถ้าไม่ใช่ Explore)</summary>
    public EpisodeTask[] Tasks
    {
        get
        {
            EpisodeStep s = CurrentStep;
            return s != null && s.tasks != null ? s.tasks : Array.Empty<EpisodeTask>();
        }
    }

    /// <summary>ความคืบหน้า task เปลี่ยน (นับเพิ่ม / reset) — static เพื่อให้ UI subscribe ได้ไม่ว่า Awake ลำดับไหน</summary>
    public static event Action OnTaskProgressChanged;

    /// <summary>เปลี่ยนขั้น (เริ่มขั้นใหม่ / จบ EP)</summary>
    public static event Action OnStepChanged;

    private bool resumed;
    private BattleOutcome resumeOutcome;
    private bool battleLaunched;

    // ─────────────────────────────────────────
    // Init
    // ─────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // เลือก EP จากเมนูแล้ว → ใช้ตัวนั้น (ซีนเดียวรองรับได้หลาย EP) / เปิดซีนตรงๆ → ใช้ค่าใน Inspector
        if (StoryProgress.SelectedEpisode != null)
            episodeData = StoryProgress.SelectedEpisode;

        // ตัดสินใจใน Awake เพื่อให้ Interactable ที่อ่าน StoryProgress ใน Start เห็นสถานะที่ถูกต้อง
        if (episodeData != null)
        {
            resumed = StoryProgress.TryResume(episodeData, out resumeOutcome);
            if (!resumed) StoryProgress.Begin(episodeData);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        if (episodeData == null)
        {
            Debug.LogWarning("[EpisodeManager] ไม่มี EpisodeData!");
            return;
        }

        episodeData.ResetTasks();
        foreach (EpisodeStep step in episodeData.GetRuntimeSteps())
        {
            if (step.tasks == null) continue;
            foreach (EpisodeTask t in step.tasks)
                if (t != null && StoryProgress.IsTaskDone(t.taskId)) t.isCompleted = true;
        }

        OnTaskProgressChanged?.Invoke();
        OnStepChanged?.Invoke(); // ให้ StepActivator ซ่อนของที่ยังไม่ถึงขั้นตั้งแต่ช่วง Intro
        StartCoroutine(EpisodeFlowRoutine());
    }

    // ─────────────────────────────────────────
    // Flow
    // ─────────────────────────────────────────

    private IEnumerator EpisodeFlowRoutine()
    {
        EpisodeStep[] steps = episodeData.GetRuntimeSteps();
        int index = 0;
        bool skipStartDialogue = false;

        SetPlayerControl(false);

        if (!resumed)
        {
            // Intro dialogue (ต้นตอน)
            if (episodeData.introDialogue != null)
            {
                CurrentPhase = EpisodePhase.Intro;
                yield return PlayDialogue(episodeData.introDialogue);
                if (verboseLog) Debug.Log("[EpisodeManager] Intro จบแล้ว");
            }

            TeleportToSpawn(episodeData.spawnPointId);
        }
        else
        {
            index = StoryProgress.StepIndex;
            RestorePlayerPosition();

            if (index >= 0 && index < steps.Length)
            {
                if (resumeOutcome == BattleOutcome.Lost)
                {
                    // แพ้ → สู้ขั้นเดิมซ้ำ (ไม่เล่น startDialogue ซ้ำ)
                    skipStartDialogue = true;
                }
                else
                {
                    // ชนะ → เล่น endDialogue ของขั้นสู้ แล้วไปขั้นต่อไป
                    SetCurrentStep(index);
                    CurrentPhase = EpisodePhase.StepDialogue;
                    yield return PlayDialogue(steps[index].endDialogue);
                    index++;
                }
            }
        }

        for (; index < steps.Length; index++)
        {
            EpisodeStep step = steps[index];
            if (step.skipStep)
            {
                skipStartDialogue = false;
                continue;
            }
            SetCurrentStep(index);

            if (verboseLog) Debug.Log($"[EpisodeManager] ▶ Step {index}: '{step.stepId}' ({step.type})");

            if (!skipStartDialogue)
            {
                TeleportToSpawn(step.spawnPointId);

                if (step.startDialogue != null)
                {
                    CurrentPhase = EpisodePhase.StepDialogue;
                    yield return PlayDialogue(step.startDialogue);
                }
            }
            skipStartDialogue = false;

            switch (step.type)
            {
                case StepType.Explore:
                    yield return RunExploreStep(step);
                    break;

                case StepType.Battle:
                    yield return RunBattleStep(step, index);
                    if (battleLaunched) yield break; // ไปซีนสู้แล้ว กลับมาจะเริ่ม flow ใหม่แบบ resume
                    break;

                case StepType.Dialogue:
                    break;
            }

            // โหมดเดิม (ไม่มี Steps): ทำ task ครบแล้วอยู่ใน Explore ต่อ ไม่จบ EP เอง
            if (episodeData.steps == null || episodeData.steps.Length == 0)
            {
                CurrentPhase = EpisodePhase.Explore;
                SetPlayerControl(true);
                yield break;
            }

            CurrentPhase = EpisodePhase.StepDialogue;
            yield return PlayDialogue(step.endDialogue);
        }

        CompleteEpisode();
    }

    private IEnumerator RunExploreStep(EpisodeStep step)
    {
        if (step.tasks == null || step.tasks.Length == 0)
        {
            Debug.LogWarning($"[EpisodeManager] Explore step '{step.stepId}' ไม่มี task → ข้ามไปขั้นต่อไป");
            yield break;
        }

        CurrentPhase = EpisodePhase.Explore;
        SetPlayerControl(true);
        OnTaskProgressChanged?.Invoke();

        yield return new WaitUntil(() => AreStepTasksDone(step));

        CurrentPhase = EpisodePhase.StepTransition;
        if (verboseLog) Debug.Log($"[EpisodeManager] ✅ Step '{step.stepId}' task ครบแล้ว");
        onAllTasksCompleted?.Invoke();
    }

    private IEnumerator RunBattleStep(EpisodeStep step, int index)
    {
        CurrentPhase = EpisodePhase.Battle;
        SetPlayerControl(false);

        Vector3 pos = playerController != null ? playerController.transform.position : Vector3.zero;

        if (fadeController != null)
            yield return fadeController.FadeOutCoroutine();

        battleLaunched = StoryProgress.LaunchBattle(step.battleSceneName, step.enemyName, index, pos, step.winWhenEnemyHpBelowPercent);
        if (!battleLaunched) yield break; // โหลดซีนสู้ไม่ได้ → ข้ามการสู้ ไม่ให้เกมค้าง

        yield return new WaitUntil(() => false); // รอซีนถูกโหลดทับ
    }

    private void CompleteEpisode()
    {
        SetCurrentStep(-1);
        CurrentPhase = EpisodePhase.Completed;
        episodeData.isCompleted = true;
        StoryProgress.Clear();
        SetPlayerControl(true);
        onEpisodeCompleted?.Invoke();

        if (verboseLog) Debug.Log("[EpisodeManager] EP Completed!");
    }

    private void SetCurrentStep(int index)
    {
        CurrentStepIndex = index;
        StoryProgress.SetStepIndex(Mathf.Max(0, index));
        OnStepChanged?.Invoke();
        OnTaskProgressChanged?.Invoke();
    }

    private IEnumerator PlayDialogue(DialogueData data)
    {
        if (data == null) yield break;

        yield return new WaitUntil(() => DialogueManager.Instance != null && !DialogueManager.Instance.IsPlaying);
        DialogueManager.Instance.StartDialogue(data, playerController);
        yield return new WaitUntil(() => DialogueManager.Instance == null || !DialogueManager.Instance.IsPlaying);
    }

    // ─────────────────────────────────────────
    // Task System
    // ─────────────────────────────────────────

    /// <summary>
    /// เรียกจาก Interactable เมื่อ task สำเร็จ — รับเฉพาะ task ของขั้น Explore ปัจจุบัน
    /// </summary>
    public void CompleteTask(string taskId)
    {
        if (episodeData == null) return;

        EpisodeTask task = FindCurrentStepTask(taskId);
        if (task == null)
        {
            Debug.LogWarning($"[EpisodeManager] task '{taskId}' ไม่อยู่ในขั้นปัจจุบัน ('{CurrentStepId}') หรือไม่มีใน EpisodeData");
            return;
        }

        if (task.isCompleted)
        {
            if (verboseLog) Debug.Log($"[EpisodeManager] task '{taskId}' ทำแล้ว ข้าม");
            return;
        }

        task.isCompleted = true;
        StoryProgress.MarkTaskDone(taskId);

        // ปิดช่อง interact ทันที ไม่รอเฟรมถัดไป
        if (AreStepTasksDone(CurrentStep))
            CurrentPhase = EpisodePhase.StepTransition;

        OnTaskProgressChanged?.Invoke();
        if (verboseLog) Debug.Log($"[EpisodeManager] ✅ Task สำเร็จ: '{task.taskName}' ({taskId})");
    }

    /// <summary>task นี้ทำได้ตอนนี้ไหม (อยู่ในขั้นปัจจุบัน + ยังไม่ทำ + ขั้นยังไม่จบ) — ไม่มี EpisodeManager/ไม่ระบุ id = ทำได้เสมอ</summary>
    public bool IsTaskAvailable(string taskId)
    {
        if (string.IsNullOrEmpty(taskId)) return true;
        EpisodeTask t = FindCurrentStepTask(taskId);
        return t != null && !t.isCompleted && !AreStepTasksDone(CurrentStep);
    }

    /// <summary>task ของขั้นปัจจุบันผ่านเงื่อนไขแล้วหรือยัง (ตาม completionMode)</summary>
    public bool AreAllTasksDone() => AreStepTasksDone(CurrentStep);

    public void GetStepProgress(out int done, out int total)
    {
        done = 0;
        total = 0;

        // AnyOne = เป้าหมายมีแค่ 1 (ทำอันไหนก็ได้) แสดงเป็น 0/1 → 1/1
        if (CurrentStep != null && CurrentStep.completionMode == TaskCompletionMode.AnyOne)
        {
            total = 1;
            done = AreStepTasksDone(CurrentStep) ? 1 : 0;
            return;
        }

        foreach (EpisodeTask t in Tasks)
        {
            if (t == null) continue;
            total++;
            if (t.isCompleted) done++;
        }
    }

    private EpisodeTask FindCurrentStepTask(string taskId)
    {
        foreach (EpisodeTask t in Tasks)
            if (t != null && t.taskId == taskId) return t;
        return null;
    }

    private static bool AreStepTasksDone(EpisodeStep step)
    {
        if (step == null || step.tasks == null) return true;

        if (step.completionMode == TaskCompletionMode.AnyOne)
        {
            bool hasTask = false;
            foreach (EpisodeTask t in step.tasks)
            {
                if (t == null) continue;
                hasTask = true;
                if (t.isCompleted) return true;
            }
            return !hasTask;
        }

        foreach (EpisodeTask t in step.tasks)
            if (t != null && !t.isCompleted) return false;
        return true;
    }

    // ─────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────

    public bool IsExploring() => CurrentPhase == EpisodePhase.Explore;

    public bool IsInStep(string stepId) => !string.IsNullOrEmpty(stepId) && CurrentStepId == stepId;

    public void SetPlayerControl(bool enabled)
    {
        if (playerController != null)
            playerController.enabled = enabled;
    }

    private void TeleportToSpawn(string spawnId)
    {
        if (string.IsNullOrEmpty(spawnId)) return;

        GameObject spawnObj = GameObject.Find(spawnId);
        if (spawnObj == null)
        {
            Debug.LogWarning($"[EpisodeManager] ไม่เจอ SpawnPoint '{spawnId}'");
            return;
        }

        MovePlayer(spawnObj.transform.position);
        if (verboseLog) Debug.Log($"[EpisodeManager] Teleport → '{spawnId}'");
    }

    private void RestorePlayerPosition()
    {
        if (StoryProgress.HasPlayerPosition)
            MovePlayer(StoryProgress.PlayerPosition);
    }

    private void MovePlayer(Vector3 position)
    {
        if (playerController == null) return;

        var rb = playerController.GetComponent<Rigidbody2D>();
        if (rb != null) rb.velocity = Vector2.zero;

        playerController.transform.position = position;
    }

    /// <summary>ให้ Interactable เช็คสถานะ task ได้ (ค้นทุกขั้น)</summary>
    public EpisodeTask GetTask(string taskId)
    {
        return episodeData?.GetTask(taskId);
    }
}