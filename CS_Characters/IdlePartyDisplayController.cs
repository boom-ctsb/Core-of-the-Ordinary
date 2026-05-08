using System.Collections.Generic;
using UnityEngine;

public class IdlePartyDisplayController : MonoBehaviour
{
    [SerializeField] private CharacterFactory characterFactory;

    private readonly List<GameObject> spawned = new List<GameObject>();

    private void Start()
    {
        BuildIdleFrontRowFromSelection();
    }

    public void BuildIdleFrontRowFromSelection()
    {
        Clear();

        if (PartySelectionData.Instance == null)
        {
            Debug.LogError("❌ PartySelectionData.Instance is null");
            return;
        }

        if (characterFactory == null)
        {
            Debug.LogError("❌ characterFactory is null");
            return;
        }

        var party = PartySelectionData.Instance.SelectedParty;
        if (party == null || party.Count != 8)
        {
            Debug.LogError("❌ SelectedParty invalid (need 8 slots)");
            return;
        }

        // ✅ เอาแนวหน้า slot 4..7 (front)
        List<BaseCharacterData> frontOrdered = new List<BaseCharacterData>(4);
        for (int i = 4; i < 8; i++)
            frontOrdered.Add(party[i]); // มี null ได้

        var idleObjects = characterFactory.CreateIdleDisplayCentered(frontOrdered);
        spawned.AddRange(idleObjects);
    }

    private void Clear()
    {
        for (int i = 0; i < spawned.Count; i++)
            if (spawned[i] != null) Destroy(spawned[i]);

        spawned.Clear();
    }
}