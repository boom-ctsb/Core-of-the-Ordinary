using System.Collections.Generic;
using UnityEngine;

public class BattleAllyPanelBuilder : MonoBehaviour
{
    [Header("── ตัวต่อสู้ 8 ช่อง: แนวหลัง 0-3 ──")]
    [SerializeField] private DynamicCharacterPanelRow backRow;

    [Header("── ตัวต่อสู้ 8 ช่อง: แนวหน้า 4-7 ──")]
    [SerializeField] private DynamicCharacterPanelRow frontRow;

    [Header("── Support Units (SelectedBackline) ──")]
    [SerializeField] private DynamicBacklinePanelRow supportRow;

    [Header("Debug")]
    [SerializeField] private bool verbose = true;

    public void BuildFromAlliesSlots(IReadOnlyList<BattleCharacter> alliesBySlots8)
    {
        if (alliesBySlots8 == null || alliesBySlots8.Count != 8)
        {
            Debug.LogError("❌ BattleAllyPanelBuilder: alliesBySlots8 invalid", this);
            return;
        }

        List<BattleCharacter> backChars = new List<BattleCharacter>(4);
        for (int i = 0; i < 4; i++)
            if (alliesBySlots8[i] != null)
                backChars.Add(alliesBySlots8[i]);

        List<BattleCharacter> frontChars = new List<BattleCharacter>(4);
        for (int i = 4; i < 8; i++)
            if (alliesBySlots8[i] != null)
                frontChars.Add(alliesBySlots8[i]);

        if (verbose)
            Debug.Log($"[BattleAllyPanelBuilder] Build battle rows: back={backChars.Count}, front={frontChars.Count}", this);

        if (backRow != null) backRow.Build(backChars);
        if (frontRow != null) frontRow.Build(frontChars);
    }

    public void BuildSupportPanels(BattleManager battleManager)
    {
        if (supportRow == null)
        {
            if (verbose) Debug.LogWarning("⚠️ BattleAllyPanelBuilder: supportRow is null", this);
            return;
        }

        supportRow.Build(battleManager);
    }

    public void HighlightCurrentTurn(BattleCharacter current)
    {
        if (backRow != null) backRow.HighlightCurrentTurn(current);
        if (frontRow != null) frontRow.HighlightCurrentTurn(current);
    }

    public void ResetHighlights()
    {
        if (backRow != null) backRow.HighlightCurrentTurn(null);
        if (frontRow != null) frontRow.HighlightCurrentTurn(null);
    }
}