using UnityEngine;

/// <summary>
/// 1 Beat = 1 ช่วงในบทสนทนา
/// มี 2 แบบ:
///   TextBeat  — มีกล่องข้อความ รอ player กด Space/Click
///   SceneBeat — ไม่มีกล่องข้อความ เปลี่ยนฉากแล้วรออัตโนมัติ
/// </summary>
[System.Serializable]
public class DialogueBeat
{
    [Header("── ประเภท Beat ──────────────────────────")]
    [Tooltip("ติ๊ก = Scene Beat (ไม่มีกล่องข้อความ)\nไม่ติ๊ก = Text Beat (มีกล่องข้อความ)")]
    public bool isSceneBeat = false;

    [Header("── Text Beat ─────────────────────────────")]
    [Tooltip("ชื่อที่แสดงในกล่อง เช่น 'Crowbot', 'Nyx'\nถ้าว่างจะไม่แสดงกล่องชื่อ")]
    public string speakerName;

    [TextArea(2, 5)]
    public string dialogueText;

    [Header("── Scene Beat ────────────────────────────")]
    [Tooltip("รอกี่วินาทีแล้วไปต่อเอง (ใช้เฉพาะ Scene Beat)")]
    public float waitSeconds = 1f;

    [Header("── เปลี่ยน Element พร้อมกัน ───────────────")]
    [Tooltip("รายการ element ที่จะเปลี่ยนพร้อมกันตอนถึง Beat นี้\n" +
             "เปลี่ยนได้หลายชิ้นพร้อมกัน เช่น กบยกมือ + ตู้เย็นเปิด")]
    public SceneElementChange[] changes = new SceneElementChange[0];
}