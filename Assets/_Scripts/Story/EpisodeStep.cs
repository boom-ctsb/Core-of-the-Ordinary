using UnityEngine;

public enum StepType
{
    Explore,   // สำรวจ: ทำ task ให้ครบ (ลำดับไหนก็ได้) แล้วไปขั้นต่อไป
    Battle,    // สู้ Dysnorma (โหลดซีน Battle แล้วกลับมา)
    Dialogue   // เล่น Dialogue ล้วนๆ (เช่น ฉากจบ EP)
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
    [Tooltip("task ของขั้นนี้ — ต้องทำครบทุกอัน (ลำดับไหนก็ได้) ถึงจะไปขั้นต่อไป\ntaskId ต้องไม่ซ้ำกันทั้ง EP")]
    public EpisodeTask[] tasks = new EpisodeTask[0];

    [Header("Battle")]
    [Tooltip("ชื่อซีนสู้ (ต้องอยู่ใน Build Settings)")]
    public string battleSceneName = "BattleScene";

    [Tooltip("ชื่อศัตรูให้ CharacterFactory สร้าง (ว่าง = ใช้ค่าใน BattleManager)")]
    public string enemyName = "Dysnorma";
}