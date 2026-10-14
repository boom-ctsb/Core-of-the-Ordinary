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

    [Tooltip("สถานะปลดล็อคของแต่ละสกิลใน allSkills")]
    public bool[] unlockedSkills = new bool[5] { true, true, true, false, false };

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

    public bool IsSkillUnlocked(int index)
    {
        EnsureSkillArrays();
        return index >= 0 && index < unlockedSkills.Length && unlockedSkills[index];
    }

    public SkillAction GetSkillByIndexRaw(int index)
    {
        EnsureSkillArrays();
        if (index < 0 || index >= allSkills.Length) return null;
        return allSkills[index];
    }

    public string GetEquippedSkillAvailabilityMessage(int slotNumber)
    {
        int index = slotNumber == 1 ? equippedSkill1Index : equippedSkill2Index;
        return GetSkillAvailabilityMessage(index);
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
        EnsureSkillArrays();
        if (index < 0 || index >= allSkills.Length) return null;
        if (!IsSkillUnlocked(index)) return null;
        return allSkills[index];
    }

    private string GetSkillAvailabilityMessage(int index)
    {
        EnsureSkillArrays();

        if (index < 0 || index >= allSkills.Length)
            return "ไม่ได้ตั้งสกิลในช่องนี้";

        if (allSkills[index] == null)
            return "ช่องนี้ยังไม่มีสกิล";

        if (!IsSkillUnlocked(index))
            return "สกิลนี้ยังล็อคอยู่";

        return "";
    }

    private void OnValidate()
    {
        EnsureSkillArrays();

        equippedSkill1Index = ClampEquippedIndex(equippedSkill1Index);
        equippedSkill2Index = ClampEquippedIndex(equippedSkill2Index);
    }

    private int ClampEquippedIndex(int index)
    {
        if (index < -1) return -1;
        if (index >= allSkills.Length) return allSkills.Length - 1;
        return index;
    }

    private void EnsureSkillArrays()
    {
        if (allSkills == null || allSkills.Length != 5)
        {
            SkillAction[] resized = new SkillAction[5];
            if (allSkills != null)
            {
                for (int i = 0; i < Mathf.Min(allSkills.Length, resized.Length); i++)
                    resized[i] = allSkills[i];
            }
            allSkills = resized;
        }

        if (unlockedSkills == null || unlockedSkills.Length != 5)
        {
            bool[] resized = new bool[5] { true, true, true, false, false };
            if (unlockedSkills != null)
            {
                for (int i = 0; i < Mathf.Min(unlockedSkills.Length, resized.Length); i++)
                    resized[i] = unlockedSkills[i];
            }
            unlockedSkills = resized;
        }
    }
}