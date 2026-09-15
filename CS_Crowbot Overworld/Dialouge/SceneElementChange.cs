using UnityEngine;

/// <summary>
/// 1 รายการเปลี่ยนแปลงของ element 1 ชิ้นใน Beat
/// เปลี่ยนได้แค่: รูป (sprite) และ visibility
/// ตำแหน่งกำหนดตอนแรกใน initialElements แล้วไม่เปลี่ยนอีก
/// </summary>
[System.Serializable]
public class SceneElementChange
{
    [Tooltip("ID ของ element ที่จะเปลี่ยน ต้องตรงกับ id ใน initialElements")]
    public string targetId;

    [Tooltip("รูปใหม่ที่จะ swap\nถ้าปล่อยว่าง = ไม่เปลี่ยนรูป")]
    public Sprite newSprite;

    [Tooltip("ติ๊กถ้าอยากเปลี่ยน visibility\nไม่ติ๊ก = ไม่เปลี่ยน")]
    public bool useNewVisible = false;
    public bool newVisible = true;
}