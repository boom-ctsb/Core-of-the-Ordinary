using System.Collections.Generic;
using UnityEngine;

public class BattleAllyPanelBuilder : MonoBehaviour
{
    [Header("── แนวหลัง (Backline Passive) ──")]
    [SerializeField] private DynamicBacklinePanelRow backlineRow;

    [Header("── แนวหน้า (Front BattleCharacter) ──")]
    [SerializeField] private DynamicCharacterPanelRow frontRow;

    [Header("Debug")]
    [SerializeField] private bool verbose = true;

    public void BuildFromAlliesSlots(IReadOnlyList<BattleCharacter> alliesBySlots8)
    {
        if (alliesBySlots8 == null || alliesBySlots8.Count != 8)
        {
            Debug.LogError("❌ BattleAllyPanelBuilder: alliesBySlots8 invalid", this);
            return;
        }

        List<BattleCharacter> frontChars = new List<BattleCharacter>(4);
        for (int i = 4; i < 8; i++)
            if (alliesBySlots8[i] != null)
                frontChars.Add(alliesBySlots8[i]);

        if (verbose)
            Debug.Log($"[BattleAllyPanelBuilder] Build front panels: count={frontChars.Count}", this);

        if (frontRow != null) frontRow.Build(frontChars);
    }

    // ✅ รับแค่ BattleManager — dataList อ่านเองจาก PartySelectionData
    public void BuildBacklinePanels(BattleManager battleManager)
    {
        if (backlineRow == null)
        {
            if (verbose) Debug.LogWarning("⚠️ BattleAllyPanelBuilder: backlineRow is null", this);
            return;
        }

        backlineRow.Build(battleManager);
    }

    public void HighlightCurrentTurn(BattleCharacter current)
    {
        if (frontRow != null) frontRow.HighlightCurrentTurn(current);
    }

    public void ResetHighlights()
    {
        if (frontRow != null) frontRow.HighlightCurrentTurn(null);
    }
}