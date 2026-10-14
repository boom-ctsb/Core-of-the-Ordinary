using UnityEngine;

public enum CharacterFaction
{
    Ally = 0,
    Enemy = 1
}

public enum AllyRow
{
    Front = 0,
    Back = 1
}

[CreateAssetMenu(fileName = "NewBaseCharacterData", menuName = "Data/Characters/Base Character Data")]
public class BaseCharacterData : ScriptableObject
{
    [Header("Identity (Shared)")]
    [Tooltip("ID ห้ามซ้ำ ใช้เป็น key อ้างอิงตัวละคร")]
    public string characterId;

    public string characterName;

    [Header("Faction (Shared)")]
    public CharacterFaction faction = CharacterFaction.Ally;

    [Header("Portrait (Shared)")]
    public Sprite portraitSprite;

    [TextArea(2, 6)]
    public string description;

    [Header("Prefabs (Shared)")]
    public GameObject characterPrefab;

    [Tooltip("Prefab สำหรับโชว์หน้ากล้อง idle (ถ้าไม่ใส่จะใช้ characterPrefab)")]
    public GameObject idleDisplayPrefab;

    [Header("Base Stats (Shared)")]
    public float maxHP = 100f;
    public float attack = 10f;
    public float defense = 5f;
    public float speed = 25f;

    // ✅ เพิ่ม: ข้อมูล Attack Action สำหรับ Tooltip
    [Header("Attack Action Info (for Tooltip)")]
    [Tooltip("ชื่อที่แสดงในกล่อง Tooltip ของปุ่ม Attack")]
    public string attackName = "Attack";

    [TextArea(2, 4)]
    [Tooltip("คำอธิบายในกล่อง Tooltip ของปุ่ม Attack")]
    public string attackDescription = "โจมตีปกติใส่ศัตรู 1 เป้าหมาย และเพิ่ม Charge";

    [Header("Animator Triggers (Shared, optional)")]
    public string attackAnimTrigger = "Attack";
    public string skill1AnimTrigger = "Skill1";
    public string skill2AnimTrigger = "Skill2";
    public string ultimateAnimTrigger = "Ultimate";
}