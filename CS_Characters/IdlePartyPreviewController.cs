using System.Collections.Generic;
using UnityEngine;

public class IdlePartyPreviewController : MonoBehaviour
{
    [SerializeField] private CharacterFactory characterFactory;
    [SerializeField] private bool refreshOnStart = true;

    private readonly List<GameObject> spawned = new List<GameObject>();

    private void Awake()
    {
        if (characterFactory == null)
            characterFactory = FindObjectOfType<CharacterFactory>();
    }

    private void Start()
    {
        if (refreshOnStart)
            RefreshPreview();
    }

    [ContextMenu("Refresh Preview")]
    public void RefreshPreview()
    {
        if (characterFactory == null)
        {
            Debug.LogError("❌ IdlePartyPreviewController: ไม่พบ CharacterFactory");
            return;
        }

        if (PartySelectionData.Instance == null)
        {
            Debug.LogError("❌ IdlePartyPreviewController: PartySelectionData.Instance is null");
            return;
        }

        var party = PartySelectionData.Instance.SelectedParty;
        if (party == null || party.Count != 8)
        {
            Debug.LogError("❌ IdlePartyPreviewController: SelectedParty invalid (need 8 slots)");
            return;
        }

        // ลบของเก่า
        for (int i = 0; i < spawned.Count; i++)
            if (spawned[i] != null) Destroy(spawned[i]);

        spawned.Clear();

        // ✅ สร้างใหม่ front 4..7 แบบ centered
        List<BaseCharacterData> frontOrdered = new List<BaseCharacterData>(4);
        for (int i = 4; i < 8; i++)
            frontOrdered.Add(party[i]);

        var idleObjects = characterFactory.CreateIdleDisplayCentered(frontOrdered);
        spawned.AddRange(idleObjects);
    }
}