using UnityEngine;

public enum UltimateCostType
{
    None = 0,
    Charge = 1,
    Shield = 2
}

public enum UltimatePowerType
{
    Fixed = 0,
    FromUserAttack = 1,
    FromCurrentShield = 2,
    FromActiveBuffCount = 3
}

[CreateAssetMenu(fileName = "Ultimate_", menuName = "Data/Ultimate Skill")]
public class UltimateSkillAction : ScriptableObject
{
    [Header("Info")]
    public string skillName = "Ultimate";
    public Sprite skillIcon;

    [TextArea(2, 6)]
    public string description;

    [Header("UI")]
    [Tooltip("Prefab ปุ่ม Ultimate เฉพาะสกิลนี้ (ถ้าไม่ใส่ จะไปใช้ fallback จาก AllyCharacterData)")]
    public GameObject buttonPrefab;

    [Header("Cost Rule")]
    public UltimateCostType costType = UltimateCostType.None;

    [Tooltip("คูลดาวน์ของสกิล (ใช้เสมอ ไม่ว่า costType จะเป็นอะไร)")]
    public float cooldownSeconds = 10f;

    [Tooltip("ถ้า costType = Charge ต้องมี charge >= ค่านี้")]
    public float requiredCharge = 100f;

    [Tooltip("ถ้า costType = Shield ต้องมี shield >= ค่านี้ (และจะหักออก)")]
    public float requiredShield = 20f;

    [Header("Power Rule")]
    public UltimatePowerType powerType = UltimatePowerType.FromUserAttack;

    [Tooltip("ดาเมจฐาน")]
    public float baseDamage = 50f;

    [Tooltip("ตัวคูณเพิ่มเติมตาม powerType")]
    public float multiplier = 2f;

    [Tooltip("ถ้า powerType = FromActiveBuffCount จะใช้จำนวนบัฟ * ค่านี้")]
    public float perBuffBonusDamage = 15f;

    [Header("Targeting (ง่าย ๆ ก่อน)")]
    public TargetType targetType = TargetType.EnemySingle;

    // ===== Helpers =====
    public float ComputeDamage(BattleCharacter user)
    {
        if (user == null) return 0f;

        float result = baseDamage;

        switch (powerType)
        {
            case UltimatePowerType.Fixed:
                break;

            case UltimatePowerType.FromUserAttack:
                result += user.EffectiveAttack * multiplier;
                break;

            case UltimatePowerType.FromCurrentShield:
                result += user.ShieldHP * multiplier;
                break;

            case UltimatePowerType.FromActiveBuffCount:
                int buffCount = 0;
                var buffs = user.GetActiveBuffsReadOnly();
                for (int i = 0; i < buffs.Count; i++)
                {
                    var b = buffs[i];
                    if (b == null) continue;
                    if (b.IsExpired) continue;
                    if (b.IsDebuff) continue;
                    buffCount++;
                }
                result += buffCount * perBuffBonusDamage;
                break;
        }

        return Mathf.Max(0f, result);
    }
}