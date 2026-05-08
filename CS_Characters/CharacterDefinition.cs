using UnityEngine;

/// <summary>
/// เก็บข้อมูลตัวละคร 1 ตัว (Idle + Action GameObject)
/// </summary>
[System.Serializable]
public class CharacterDefinition
{
    [SerializeField] public string characterName;           // ชื่อตัวละคร
    [SerializeField] public GameObject idleGameObject;      // ท่า Idle (ยืนปกติ)
    [SerializeField] public GameObject actionGameObject;    // ท่า Action (ตี/โจมตี)

    [Header("Stats")]
    [SerializeField] public float maxHP = 100f;
    [SerializeField] public float attack = 20f;
    [SerializeField] public float defense = 10f;
    [SerializeField] public float speed = 15f;

    [Header("Position")]
    [SerializeField] public int positionIndex = 0;  // 0-3 หน้า, 4-7 หลัง

    [Header("Portrait")]
    [SerializeField] public Sprite portraitSprite;

    public CharacterDefinition()
    {
        characterName = "New Character";
        idleGameObject = null;
        actionGameObject = null;
    }
}