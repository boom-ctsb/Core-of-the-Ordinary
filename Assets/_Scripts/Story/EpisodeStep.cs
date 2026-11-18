using UnityEngine;

public enum StepType
{
    Explore,   // สำรวจ: ทำ task ตาม completionMode แล้วไปขั้นต่อไป
    Battle,    // สู้ Dysnorma (โหลดซีน Battle แล้วกลับมา)
    Dialogue   // เล่น Dialogue ล้วนๆ (เช่น ฉากจบ EP)
}

public enum TaskCompletionMode
{
    RequireAll,  // ต้องทำครบทุก task
    AnyOne       // ทำ task ใดก็ได้แค่ 1 อัน ก็ผ่านขั้น (เช่น "คุยกับ NPC" หรือ "เดินไปถึงจุด")
}

/// <summary>
/// 1 ขั้นของ EP — EpisodeManager เดินไปทีละขั้นตามลำดับใน EpisodeData.steps
/// ลำดับในแต่ละขั้น: spawn (ถ้ามี) → startDialogue → ตัวขั้น → endDialogue
/// </summary>
[System.Serializable]
public class EpisodeStep
{
    [Tooltip("ID ของขั้นนี้ ห้ามซ้ำ ใช้กับ StepActivator ในฉาก เช่น 'enter_house', 'fight_1'")]
    public string stepId;

    public StepType type = StepType.Explore;

    [Tooltip("ติ๊ก = ข้ามขั้นนี้ไปเลย (ใช้สลับตัวเลือก เช่น 'ไปถึงแล้วไม่มีสู้' โดยไม่ต้องลบขั้นทิ้ง)")]
    public bool skipStep = false;

    [Header("Objective Text")]
    [Tooltip("ข้อความภารกิจที่แสดงบนจอ เช่น 'สำรวจบ้านร้าง'")]
    public string objectiveText;

    [Tooltip("แสดงตัวนับ x/y ต่อท้าย (ใช้กับ Explore)")]
    public bool showCount = true;

    [Header("Dialogue (optional)")]
    [Tooltip("เล่นก่อนเริ่มขั้นนี้ (ว่าง = ข้าม)")]
    public DialogueData startDialogue;

    [Tooltip("เล่นหลังจบขั้นนี้ (ว่าง = ข้าม) — ของ Battle จะเล่นหลังชนะและกลับมาซีนเนื้อเรื่อง")]
    public DialogueData endDialogue;

    [Header("Move Player (optional)")]
    [Tooltip("ชื่อ GameObject SpawnPoint ที่จะย้ายผู้เล่นไปตอนเริ่มขั้นนี้ (ว่าง = ไม่ย้าย)")]
    public string spawnPointId;

    [Header("Explore")]
    [Tooltip("RequireAll = ต้องทำครบทุก task (ลำดับไหนก็ได้)\nAnyOne = ทำ task ไหนก็ได้ 1 อัน ก็ไปขั้นต่อไป (กล่องเขียวอ่อนที่มีตัวเลือกสีเขียว/ฟ้าในผัง)")]
    public TaskCompletionMode completionMode = TaskCompletionMode.RequireAll;

    [Tooltip("task ของขั้นนี้ taskId ต้องไม่ซ้ำกันทั้ง EP")]
    public EpisodeTask[] tasks = new EpisodeTask[0];

    [Header("Battle")]
    [Tooltip("ชื่อซีนสู้ (ต้องอยู่ใน Build Settings)")]
    public string battleSceneName = "BattleScene";

    [Tooltip("ชื่อศัตรูให้ CharacterFactory สร้าง (ว่าง = ใช้ค่าใน BattleManager)")]
    public string enemyName = "Dysnorma";

    [Tooltip("0 = สู้จนศัตรูตาย\n30 = เมื่อ HP ศัตรูเหลือ ≤ 30% ถือว่าชนะทันที (Dysnorma หนีและทิ้งของ)")]
    [Range(0f, 99f)]
    public float winWhenEnemyHpBelowPercent = 0f;
}