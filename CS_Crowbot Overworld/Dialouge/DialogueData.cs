using UnityEngine;

/// <summary>
/// ScriptableObject เก็บบทสนทนาทั้งหมดของ NPC / Object 1 ชิ้น
/// สร้างได้จาก Right-click → Create → Coto/Dialogue Data
/// </summary>
[CreateAssetMenu(fileName = "Dialogue_", menuName = "Coto/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [Header("Background")]
    [Tooltip("รูป bg หลักของฉากนี้ ทุก Beat จะใช้รูปนี้ถ้าไม่ได้ตั้ง backgroundOverride")]
    public Sprite defaultBackground;

    [Header("Element เริ่มต้นในฉาก")]
    [Tooltip("ใส่ทุกชิ้นที่จะปรากฏในฉากนี้ ทั้งตัวละครและของ\n" +
             "ลำดับใน array ไม่สำคัญ ใช้ sortOrder กำหนดว่าใครอยู่หน้าใคร")]
    public SceneElementData[] initialElements = new SceneElementData[0];

    [Header("บทสนทนาหลัก (ครั้งแรกที่คุย)")]
    public DialogueBeat[] beats = new DialogueBeat[0];

    [Header("บทสนทนาซ้ำ (กลับมาคุยใหม่)")]
    [Tooltip("ถ้าว่างเปล่า = วนซ้ำ beats ปกติตั้งแต่ต้น")]
    public DialogueBeat[] repeatBeats = new DialogueBeat[0];

    // ─────────────────────────────────────────
    // Helper: ให้ Manager เรียกเพื่อดึง beats ที่จะเล่น
    // ─────────────────────────────────────────

    /// <summary>
    /// ดึง beats ที่จะใช้เล่น — ถ้า hasSpokenBefore และมี repeatBeats ให้ใช้ repeatBeats
    /// </summary>
    public DialogueBeat[] GetBeatsToPlay(bool hasSpokenBefore)
    {
        if (hasSpokenBefore && repeatBeats != null && repeatBeats.Length > 0)
            return repeatBeats;

        return beats;
    }
}