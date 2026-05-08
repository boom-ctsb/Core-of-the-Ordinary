using System.Collections.Generic;
using UnityEngine;

public class BattleAllyPanelBuilder : MonoBehaviour
{
    [Header("Rows")]
    [SerializeField] private DynamicCharacterPanelRow backRow;
    [SerializeField] private DynamicCharacterPanelRow frontRow;

    [Header("Debug")]
    [SerializeField] private bool verbose = true;

    public void BuildFromAlliesSlots(IReadOnlyList<BattleCharacter> alliesBySlots8)
    {
        if (alliesBySlots8 == null)
        {
            Debug.LogError("❌ BattleAllyPanelBuilder: alliesBySlots8 is null", this);
            return;
        }

        if (alliesBySlots8.Count != 8)
        {
            Debug.LogError($"❌ BattleAllyPanelBuilder: need 8 slots but got {alliesBySlots8.Count}", this);
            return;
        }

        if (backRow == null) Debug.LogError("❌ BattleAllyPanelBuilder: backRow is null", this);
        if (frontRow == null) Debug.LogError("❌ BattleAllyPanelBuilder: frontRow is null", this);

        List<BattleCharacter> backChars = new List<BattleCharacter>(4);
        List<BattleCharacter> frontChars = new List<BattleCharacter>(4);

        for (int i = 0; i < 4; i++)
            if (alliesBySlots8[i] != null) backChars.Add(alliesBySlots8[i]);

        for (int i = 4; i < 8; i++)
            if (alliesBySlots8[i] != null) frontChars.Add(alliesBySlots8[i]);

        if (verbose)
        {
            Debug.Log($"[BattleAllyPanelBuilder] Build panels: back={backChars.Count}, front={frontChars.Count}", this);
            for (int i = 0; i < 8; i++)
            {
                var c = alliesBySlots8[i];
                Debug.Log($"  slot {i}: {(c != null ? c.CharacterName : "(null)")}", this);
            }
        }

        if (backRow != null) backRow.Build(backChars);
        if (frontRow != null) frontRow.Build(frontChars);
    }

    public void HighlightCurrentTurn(BattleCharacter current)
    {
        if (
backRow != null) backRow.HighlightCurrentTurn(current);
        if (frontRow != null) frontRow.HighlightCurrentTurn(current);
    }

    public void ResetHighlights()
    {
        // ให้ normalize ทุกตัวโดยเรียก HighlightCurrentTurn(null)
        // ใน DynamicCharacterPanelRow ด้านล่าง เราจะรองรับ current == null
        if (backRow != null) backRow.HighlightCurrentTurn(null);
        if (frontRow != null) frontRow.HighlightCurrentTurn(null);
    }
}