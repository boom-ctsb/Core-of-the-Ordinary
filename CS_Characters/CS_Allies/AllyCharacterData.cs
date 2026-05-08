using UnityEngine;

[CreateAssetMenu(fileName = "NewAllyCharacterData", menuName = "Data/Characters/Ally Character Data")]
public class AllyCharacterData : BaseCharacterData
{
    [Header("Ally Layout (Selection helper)")]
    public AllyRow allyRow = AllyRow.Front;

    [Header("Ally UI (Action Buttons)")]
    public GameObject attackButtonPrefab;
    public GameObject skill1ButtonPrefab;
    public GameObject skill2ButtonPrefab;

    [Tooltip("Prefab ปุ่ม Ultimate แบบ default ของตัวละคร (ถ้า UltimateSkillAction ไม่ได้ใส่ prefab จะ fallback มาที่ตัวนี้)")]
    public GameObject ultimateButtonPrefab;

    [Header("Ultimate Skill (separate)")]
    [Tooltip("Ultimate ของตัวละคร (แยกจาก allSkills เพราะกติกาไม่เหมือนสกิลทั่วไป)")]
    public UltimateSkillAction ultimateSkill;

    [Header("Skills (Max 5, Equip 2)")]
    [Tooltip("สกิลทั้งหมดของตัวละคร (ใส่ได้สูงสุด 5)")]
    public SkillAction[] allSkills = new SkillAction[5];

    [Tooltip("index ใน allSkills ที่ถูก equip เป็น Skill1 (0-4), -1=ไม่มี")]
    public int equippedSkill1Index = 0;

    [Tooltip("index ใน allSkills ที่ถูก equip เป็น Skill2 (0-4), -1=ไม่มี")]
    public int equippedSkill2Index = 1;

    public SkillAction GetEquippedSkill1()
    {
        return GetSkillByIndex(equippedSkill1Index);
    }

    public SkillAction GetEquippedSkill2()
    {
        return GetSkillByIndex(equippedSkill2Index);
    }

    /// <summary>
    /// Ultimate: ถ้าสกิลมี prefab ของตัวเอง ให้ใช้ของสกิล
    /// ไม่งั้น fallback ไปใช้ ultimateButtonPrefab ของตัวละคร
    /// </summary>
    public GameObject GetUltimateButtonPrefab()
    {
        if (ultimateSkill != null && ultimateSkill.buttonPrefab != null)
            return ultimateSkill.buttonPrefab;

        return ultimateButtonPrefab;
    }

    private SkillAction GetSkillByIndex(int index)
    {
        if (allSkills == null) return null;
        if (index < 0 || index >= allSkills.Length) return null;
        return allSkills[index];
    }
}