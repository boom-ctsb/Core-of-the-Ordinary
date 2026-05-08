using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyCharacterData", menuName = "Data/Characters/Enemy Character Data")]
public class EnemyCharacterData : BaseCharacterData
{
    [Header("Enemy Basic Attack")]
    public float basicAttackMultiplier = 1f;

    [Header("Enemy Combo Meter")]
    public int comboMax = 10;

    [Header("Enemy Pattern / AI (later)")]
    [Tooltip("เผื่ออนาคต: id ของ pattern หรือชื่อไฟล์/ชื่อ config")]
    public string patternId;

    [Tooltip("เผื่ออนาคต: เป็นบอสหรือไม่ (ไว้เปิด ultimate / phase)")]
    public bool isBoss = false;
}