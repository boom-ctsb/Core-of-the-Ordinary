using UnityEngine;

/// <summary>
/// ข้อมูลเริ่มต้นของ 1 ชิ้นในฉาก (ตัวละคร, ของ, พื้นหลัง)
/// ใส่ใน DialogueData.initialElements[]
/// </summary>
[System.Serializable]
public class SceneElementData
{
    [Tooltip("ชื่อ ID ต้องไม่ซ้ำกัน เช่น 'fridge', 'char_frog', 'table'")]
    public string id;

    [Tooltip("รูป Sprite เริ่มต้น")]
    public Sprite sprite;

    [Tooltip("ตำแหน่งใน Canvas (anchoredPosition)")]
    public Vector2 position;

    [Tooltip("ลำดับชั้น — ตัวเลขมากกว่า = อยู่หน้ากว่า\n" +
             "แนะนำ: bg=0, ของหลัง=10, โต๊ะ=20, เก้าอี้=30, ตัวละคร=40, ของหน้า=50")]
    public int sortOrder = 10;

    [Tooltip("ขนาด Image (width, height) หน่วย pixel")]
    public Vector2 size = new Vector2(200f, 200f);

    [Tooltip("แสดงตั้งแต่ต้นมั้ย")]
    public bool visibleOnStart = true;
}