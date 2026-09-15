using UnityEngine;

public enum TaskType
{
    Talk,     // คุยกับตัวละคร
    Collect,  // เก็บของ
    Clue,     // สืบเบาะแส
    Destroy   // ทำลาย
}

[System.Serializable]
public class EpisodeTask
{
    [Tooltip("ID ห้ามซ้ำกัน ใช้อ้างอิงจาก Interactable ในฉาก\nเช่น 'talk_nyx', 'collect_key', 'clue_desk'")]
    public string taskId;

    [Tooltip("ชื่อแสดงใน Task List UI")]
    public string taskName;

    public TaskType taskType;

    [Tooltip("ต้องทำ dialogue ก่อนถึงจะถือว่าสำเร็จมั้ย\n" +
             "ถ้าปิด = interact แล้วสำเร็จทันที (ไม่มี dialogue)")]
    public bool requiresDialogue = true;

    // ─── Runtime (ไม่ serialize) ───────────────
    [System.NonSerialized]
    public bool isCompleted = false;
}