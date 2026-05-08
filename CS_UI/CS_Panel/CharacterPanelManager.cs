using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ตัวจัดการ Panel ตัวละครทั้งหมดใน Battle Scene
/// mapping ที่ใช้:
/// index 0..3 = แนวหลัง (ซ้าย->ขวา)
/// index 4..7 = แนวหน้า (ซ้าย->ขวา)
/// </summary>
public class CharacterPanelManager : MonoBehaviour
{
    [Header("Panels (size must be 8)")]
    [SerializeField] private CharacterPanel[] characterPanels = new CharacterPanel[8];

    /// <summary>
    /// Initialize โดยรับ "ตำแหน่ง 8 ช่อง" มาเลย
    /// ถ้าช่องไหนเป็น null -> ซ่อน panel ช่องนั้น
    /// </summary>
    public void InitializePanelsBySlots(IReadOnlyList<BattleCharacter> slotCharacters)
    {
        if (slotCharacters == null || slotCharacters.Count != 8)
        {
            Debug.LogError("❌ InitializePanelsBySlots: ต้องส่ง list ขนาด 8 (Back4+Front4)");
            return;
        }

        if (characterPanels == null || characterPanels.Length != 8)
        {
            Debug.LogError("❌ characterPanels ต้องตั้ง size = 8 ใน Inspector");
            return;
        }

        Debug.Log("🔗 Connect Character Panels by SLOT:");

        for (int i = 0; i < 8; i++)
        {
            CharacterPanel panel = characterPanels[i];
            if (panel == null)
            {
                Debug.LogWarning($"⚠️ characterPanels[{i}] is null (Inspector ยังไม่ได้ลาก)");
                continue;
            }

            BattleCharacter character = slotCharacters[i];

            if (character != null)
            {
                panel.gameObject.SetActive(true);
                panel.Initialize(character);
                Debug.Log($"    slot {i}: {character.CharacterName} → Panel {i}");
            }
            else
            {
                panel.gameObject.SetActive(false);
                Debug.Log($"    slot {i}: (empty) → Panel {i} hidden");
            }
        }
    }

    /// <summary>
    /// (ยังเก็บของเดิมไว้) ถ้าคุณยังมีโค้ดเก่าที่ส่งเป็น list ธรรมดา
    /// จะผูกเรียงจาก 0..n แล้วซ่อนที่เหลือ
    /// </summary>
    public void InitializePanels(List<BattleCharacter> characters)
    {
        if (characters == null || characters.Count == 0)
        {
            Debug.LogError("❌ ไม่มีข้อมูลตัวละครในทีม!");
            return;
        }

        for (int i = 0; i < characterPanels.Length; i++)
        {
            if (characterPanels[i] == null) continue;

            if (i < characters.Count && characters[i] != null)
            {
                characterPanels[i].gameObject.SetActive(true);
                characterPanels[i].Initialize(characters[i]);
            }
            else
            {
                characterPanels[i].gameObject.SetActive(false);
            }
        }
    }

    private void Update()
    {
        foreach (var panel in characterPanels)
        {
            if (panel != null && panel.gameObject.activeSelf)
                panel.UpdatePanel();
        }
    }

    public void HighlightCurrentTurn(BattleCharacter currentCharacter)
    {
        foreach (var panel in characterPanels)
        {
            if (panel == null) continue;

            if (panel.character == currentCharacter) panel.Highlight();
            else panel.Normalize();
        }
    }

    public void ResetPanels()
    {
        foreach (var panel in characterPanels)
        {
            if (panel != null) panel.Normalize();
        }
    }
}