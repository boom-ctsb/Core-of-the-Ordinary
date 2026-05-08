using System.Collections.Generic;
using UnityEngine;

public class PartySelectionData : MonoBehaviour
{
    public static PartySelectionData Instance { get; private set; }

    // ✅ วิธี B: เก็บ BaseCharacterData
    public readonly List<BaseCharacterData> SelectedParty = new List<BaseCharacterData>(8);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (transform.parent != null)
            transform.SetParent(null);

        DontDestroyOnLoad(gameObject);

        EnsureSize8();
    }

    private void EnsureSize8()
    {
        if (SelectedParty.Count == 8) return;

        SelectedParty.Clear();
        for (int i = 0; i < 8; i++)
            SelectedParty.Add(null);
    }

    public void SetPartyFromSlots(BaseCharacterData[] backRow, BaseCharacterData[] frontRow)
    {
        EnsureSize8();

        for (int i = 0; i < 4; i++)
            SelectedParty[i] = (backRow != null && i < backRow.Length) ? backRow[i] : null;

        for (int i = 0; i < 4; i++)
            SelectedParty[4 + i] = (frontRow != null && i < frontRow.Length) ? frontRow[i] : null;
    }

    public int CountFront()
    {
        int count = 0;
        for (int i = 4; i < 8; i++)
            if (SelectedParty[i] != null) count++;
        return count;
    }

    public int CountBack()
    {
        int count = 0;
        for (int i = 0; i < 4; i++)
            if (SelectedParty[i] != null) count++;
        return count;
    }
}