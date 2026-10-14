using System.Collections.Generic;
using UnityEngine;

public class PartySelectionData : MonoBehaviour
{
    public static PartySelectionData Instance { get; private set; }

    // index 0-3 = แนวหลัง, 4-7 = แนวหน้า (ตรงกับ CharacterPanelManager)
    public readonly List<BaseCharacterData> SelectedParty = new List<BaseCharacterData>(8);
    public readonly List<AllyBacklineData> SelectedBackline = new List<AllyBacklineData>(4);

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }

        Instance = this;

        if (transform.parent != null) transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        EnsureSize8();
        EnsureBacklineSize4();
    }

    // ─── Party (8 slot) ───

    private void EnsureSize8()
    {
        SelectedParty.Clear();
        for (int i = 0; i < 8; i++) SelectedParty.Add(null);
    }

    /// <summary>
    /// backRow[0-3] = แนวหลัง, frontRow[0-3] = แนวหน้า
    /// → SelectedParty[0-3] = back, [4-7] = front
    /// </summary>
    public void SetPartyFromSlots(BaseCharacterData[] backRow, BaseCharacterData[] frontRow)
    {
        EnsureSize8();

        for (int i = 0; i < 4; i++)
            SelectedParty[i] = (backRow != null && i < backRow.Length) ? backRow[i] : null;

        for (int i = 0; i < 4; i++)
            SelectedParty[4 + i] = (frontRow != null && i < frontRow.Length) ? frontRow[i] : null;
    }

    public int CountFront() { int n = 0; for (int i = 4; i < 8; i++) if (SelectedParty[i] != null) n++; return n; }
    public int CountBack() { int n = 0; for (int i = 0; i < 4; i++) if (SelectedParty[i] != null) n++; return n; }

    // ─── Backline (4 slot) ───

    private void EnsureBacklineSize4()
    {
        SelectedBackline.Clear();
        for (int i = 0; i < 4; i++) SelectedBackline.Add(null);
    }

    public void SetBacklineFromSlots(AllyBacklineData[] slots)
    {
        EnsureBacklineSize4();
        for (int i = 0; i < 4; i++)
            SelectedBackline[i] = (slots != null && i < slots.Length) ? slots[i] : null;
    }

    public int CountBackline() { int n = 0; foreach (var b in SelectedBackline) if (b != null) n++; return n; }

    // ─── Debug ───
    public void DebugPrint()
    {
        Debug.Log($"[PartySelectionData] Front={CountFront()} Back={CountBack()} Backline={CountBackline()}");
        for (int i = 0; i < 8; i++)
            Debug.Log($"  Party[{i}] = {(SelectedParty[i] != null ? SelectedParty[i].characterName : "null")}");
        for (int i = 0; i < 4; i++)
            Debug.Log($"  Backline[{i}] = {(SelectedBackline[i] != null ? SelectedBackline[i].characterName : "null")}");
    }
}