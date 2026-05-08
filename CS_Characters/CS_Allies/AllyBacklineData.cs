using UnityEngine;

[CreateAssetMenu(fileName = "NewAllyBacklineData", menuName = "Data/Characters/Ally Backline Data")]
public class AllyBacklineData : ScriptableObject
{
    [Header("Info")]
    public string characterName;
    public Sprite portraitSprite;

    [Header("Stats (ใช้กับสกิล)")]
    public float attack = 50f;
    public float defense = 10f;
    public float speed = 20f;

    [Header("Backline Skill")]
    public SkillAction passiveSkill;

    [Header("Timebar")]
    public float timebarMax = 100f;

    [Tooltip("อ้างอิงจาก BattleCharacter: baseFillPercentPerSecond")]
    [Range(0.01f, 1f)]
    public float baseFillPercentPerSecond = 0.20f;

    [Tooltip("ใช้คำนวณ speed factor (เหมือน BattleCharacter)")]
    public float speedBaseline = 25f;

    [Header("Trigger Window")]
    [Tooltip("หลังทำเงื่อนไขสำเร็จ จะให้ Timebar เดินนานกี่วินาที")]
    public float fillWindowSeconds = 3f;

    [Header("UI")]
    [Tooltip("ถ้าไม่ใส่ จะใช้ icon ของ skill")]
    public Sprite conditionIconOverride;
}