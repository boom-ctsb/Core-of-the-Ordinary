using UnityEngine;

public enum BattleActionKind
{
    Skill,
    Ultimate
}

[CreateAssetMenu(fileName = "NewBattleAction", menuName = "Data/Battle Action")]
public class BattleActionDefinition : ScriptableObject
{
    [Header("UI")]
    public string actionName;
    public Sprite icon;

    [Header("Type")]
    public BattleActionKind kind = BattleActionKind.Skill;

    [Header("Early-stage Demo (optional)")]
    [Tooltip("ถ้า action นี้เป็นการโจมตี ให้ใช้ดาเมจเดโมนี้ไว้ทดสอบก่อน")]
    public float demoDamage = 0f;

    [Tooltip("ถ้า action นี้เพิ่ม Charge ให้ใส่จำนวน 'ช่อง' ที่จะเพิ่ม (10 ช่อง=100)")]
    public int chargeBoxesGain = 0;

    [Tooltip("action นี้นับเป็น 'การโจมตี' ไหม? (Attack/Skill/Ultimate ที่เป็นดาเมจ)")]
    public bool countsAsOffensiveHit = true;
}