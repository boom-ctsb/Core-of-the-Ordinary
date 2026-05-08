using UnityEngine;

public enum SkillType
{
    Attack = 0,
    Heal = 1,
    Buff = 2,
    Debuff = 3
}

public enum TargetType
{
    EnemySingle = 0,
    AllEnemies = 1,

    Self = 10,
    AllySingle = 11,
    AllAllies = 12
}

[CreateAssetMenu(fileName = "Skill_", menuName = "Data/Skill")]
public class SkillAction : ScriptableObject
{
    [Header("Info")]
    public string skillName;
    public Sprite skillIcon;

    [TextArea(2, 6)]
    public string description;

    [Header("Type / Target")]
    public SkillType skillType = SkillType.Attack;
    public TargetType targetType = TargetType.EnemySingle;

    [Header("Power (Attack/Heal)")]
    public float powerBase = 0f;
    public float attackMultiplier = 1.0f;

    [Range(0f, 100f)]
    public float accuracy = 100f;

    [Header("Costs")]
    public float chargeCost = 0f;

    [Header("Buff/Debuff Duration (seconds)")]
    public float buffDurationSeconds = 5f;

    [Header("Buff (Temporary, per second)")]
    public float buffAttackAdd = 0f;
    public float buffDefenseAdd = 0f;
    public float buffSpeedAdd = 0f;
    public float buffHpRegenPerSecond = 0f;

    [Header("Buff: Evasion (Dodge enemy attacks)")]
    [Range(0f, 1f)]
    public float buffEvasionChanceAdd = 0f;

    [Header("Debuff (Temporary)")]
    public float debuffSpeedPercent = 0f;
    public float debuffAccuracyPercent = 0f;
    public float debuffAttackAdd = 0f;
    public float debuffDefenseAdd = 0f;

    [Header("Debuff Flags")]
    public bool isStunDebuff = false;

    [Header("Extra Effect: Target ATB Shift (units)")]
    public float targetATBShiftUnits = 0f;

    [Header("Extra Effect: Target Charge Shift (0..100)")]
    public float targetChargeShift = 0f;

    [Header("Team Buff: Follow-up Hit (On Ally Attack)")]
    public bool enableTeamFollowUpHit = false;
    public float followUpOwnerAtkMultiplier = 1.0f;
    public float followUpFlatBonusDamage = 0f;
    public bool followUpAddsEnemyCombo = true;
    public int followUpEnemyComboAdd = 1;

    [Header("Team Buff: Shield (from Owner DEF)")]
    public bool enableTeamShield = false;
    public float shieldFromOwnerDefenseMultiplier = 1.0f;
    public bool shieldReplaceInsteadOfStack = false;

    [Header("Status Icon (Optional Override Prefab)")]
    [Tooltip("ถ้าตั้ง จะใช้ prefab นี้เป็นไอคอนบัฟ/ดีบัฟของสกิลนี้ แทน prefab default ใน CharacterPanel")]
    public GameObject statusIconPrefabOverride;

    [Header("Post-Use ATB (optional)")]
    public bool overrideATBAfterUse = false;

    [Range(0f, 1f)]
    public float atbAfterUsePercent = 0f;

    [Header("Animation / VFX (optional)")]
    public string animationActionId = "Attack";
}