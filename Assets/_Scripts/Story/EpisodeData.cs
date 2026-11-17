using UnityEngine;

[CreateAssetMenu(fileName = "EpisodeData", menuName = "Coto/Story/EpisodeData")]
public class EpisodeData : ScriptableObject
{
    [Header("Info")]
    public Sprite epThumbnail;
    public string epName;
    public bool isMain = true;
    public bool isUnlocked = false;
    public bool isCompleted = false;

    [TextArea]
    public string storyDescription;

    [Header("── Intro ──────────────────────────────")]
    [Tooltip("Dialogue ที่เล่นก่อนเริ่ม EP\nถ้าว่าง = ข้ามไปเลย")]
    public DialogueData introDialogue;

    [Header("── Spawn ───────────────────────────────")]
    [Tooltip("ชื่อ GameObject SpawnPoint ใน Scene\nต้องตรงกับชื่อ GameObject ในฉาก")]
    public string spawnPointId;

    [Header("── Steps ──────────────────────────────")]
    [Tooltip("ลำดับขั้นของ EP (Explore / Battle / Dialogue) เดินทีละขั้นจากบนลงล่าง\n" +
             "ตัวอย่าง: Battle(สู้ต้น EP) → Explore(สำรวจบ้าน) → Battle → Explore → Dialogue(ฉากจบ)")]
    public EpisodeStep[] steps = new EpisodeStep[0];

    [Header("── Tasks (แบบเดิม) ─────────────────────")]
    [Tooltip("ใช้เฉพาะเมื่อ Steps ว่าง — ระบบจะมองเป็น 1 ขั้น Explore")]
    public EpisodeTask[] tasks = new EpisodeTask[0];

    // ─────────────────────────────────────────
    // Runtime helpers
    // ─────────────────────────────────────────

    [System.NonSerialized] private EpisodeStep[] legacySteps;

    /// <summary>ขั้นที่ใช้จริง — ถ้าไม่ได้ตั้ง Steps จะสร้าง Explore 1 ขั้นจาก tasks เดิม</summary>
    public EpisodeStep[] GetRuntimeSteps()
    {
        if (steps != null && steps.Length > 0) return steps;

        if (legacySteps == null)
        {
            legacySteps = new[]
            {
                new EpisodeStep
                {
                    stepId = "legacy_explore",
                    type = StepType.Explore,
                    tasks = tasks ?? new EpisodeTask[0]
                }
            };
        }
        return legacySteps;
    }

    /// <summary>Reset task ทั้งหมดเมื่อเริ่ม EP ใหม่</summary>
    public void ResetTasks()
    {
        foreach (var step in GetRuntimeSteps())
        {
            if (step.tasks == null) continue;
            foreach (var t in step.tasks)
                if (t != null) t.isCompleted = false;
        }
    }

    /// <summary>ทำ task ครบทุกอันของทุกขั้นแล้วมั้ย</summary>
    public bool AllTasksCompleted()
    {
        foreach (var step in GetRuntimeSteps())
        {
            if (step.tasks == null) continue;
            foreach (var t in step.tasks)
                if (t != null && !t.isCompleted) return false;
        }
        return true;
    }

    /// <summary>หา task จาก id (ค้นทุกขั้น)</summary>
    public EpisodeTask GetTask(string taskId)
    {
        if (string.IsNullOrEmpty(taskId)) return null;

        foreach (var step in GetRuntimeSteps())
        {
            if (step.tasks == null) continue;
            foreach (var t in step.tasks)
                if (t != null && t.taskId == taskId) return t;
        }
        return null;
    }
}