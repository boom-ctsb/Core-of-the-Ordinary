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

    [Header("── Tasks ──────────────────────────────")]
    [Tooltip("รายการ Task ทั้งหมดของ EP นี้\nต้องทำครบทุกอันถึงจะไปต่อได้")]
    public EpisodeTask[] tasks = new EpisodeTask[0];

    // ─────────────────────────────────────────
    // Runtime helpers
    // ─────────────────────────────────────────

    /// <summary>Reset task ทั้งหมดเมื่อเริ่ม EP ใหม่</summary>
    public void ResetTasks()
    {
        foreach (var t in tasks)
            t.isCompleted = false;
    }

    /// <summary>ทำ task ครบทุกอันแล้วมั้ย</summary>
    public bool AllTasksCompleted()
    {
        foreach (var t in tasks)
            if (!t.isCompleted) return false;
        return true;
    }

    /// <summary>หา task จาก id</summary>
    public EpisodeTask GetTask(string taskId)
    {
        foreach (var t in tasks)
            if (t.taskId == taskId) return t;
        return null;
    }
}