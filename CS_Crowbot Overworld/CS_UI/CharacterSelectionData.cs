using UnityEngine;

/// <summary>
/// เก็บข้อมูลการเลือกตัวละคร
/// </summary>
[System.Serializable]
public class CharacterSelectionData
{
    [SerializeField] public int[] selectedAlliesIndices = new int[4];  // เลือกที่ไหน
    [SerializeField] public int selectedDysnormaIndex = 0;             // ศัตรู

    public CharacterSelectionData()
    {
        // ค่าเริ่มต้น: เลือกตัวแรก ๆ
        selectedAlliesIndices = new int[] { 0, 1, 2, 3 };
        selectedDysnormaIndex = 0;
    }
}