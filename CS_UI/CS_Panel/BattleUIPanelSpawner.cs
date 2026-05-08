using System.Collections.Generic;
using UnityEngine;

public class BattleUIPanelSpawner : MonoBehaviour
{
    [Header("Factory")]
    [SerializeField] private CharacterFactory characterFactory;

    [Header("UI Roots (มี HorizontalLayoutGroup + ChildAlignment=MiddleCenter)")]
    [SerializeField] private Transform backRowRoot;
    [SerializeField] private Transform frontRowRoot;

    [Header("Panel Prefabs")]
    [SerializeField] private CharacterPanel backPanelPrefab;
    [SerializeField] private CharacterPanel frontPanelPrefab;

    public readonly List<BattleCharacter> SpawnedAllies = new List<BattleCharacter>();
    private readonly List<CharacterPanel> _spawnedPanels = new List<CharacterPanel>();

    private void Start()
    {
        BuildPanelsFromSelectedParty();
    }

    public void BuildPanelsFromSelectedParty()
    {
        ClearSpawned();

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

        // Back 0..3
        for (int slot = 0; slot < 4; slot++)
        {
            BaseCharacterData cd = party[slot];
            if (cd == null) continue;

            BattleCharacter bc = characterFactory.CreateAllyBySelectedSlot(cd, slot);
            if (bc == null) continue;

            SpawnedAllies.Add(bc);

            if (backRowRoot != null && backPanelPrefab != null)
            {
                CharacterPanel panel = Instantiate(backPanelPrefab, backRowRoot);
                panel.gameObject.SetActive(true);
                panel.Initialize(bc);
                _spawnedPanels.Add(panel);
            }
        }

        // Front 4..7
        for (int slot = 4; slot < 8; slot++)
        {
            BaseCharacterData cd = party[slot];
            if (cd == null) continue;

            BattleCharacter bc = characterFactory.CreateAllyBySelectedSlot(cd, slot);
            if (bc == null) continue;

            SpawnedAllies.Add(bc);

            if (frontRowRoot != null && frontPanelPrefab != null)
            {
                CharacterPanel panel = Instantiate(frontPanelPrefab, frontRowRoot);
                panel.gameObject.SetActive(true);
                panel.Initialize(bc);
                _spawnedPanels.Add(panel);
            }
        }
    }

    private void ClearSpawned()
    {
        for (int i = 0; i < _spawnedPanels.Count; i++)
        {
            if (_spawnedPanels[i] != null)
                Destroy(_spawnedPanels[i].gameObject);
        }
        _spawnedPanels.Clear();

        // NOTE: ตัวละครที่สร้างใน scene จะไม่ถูก destroy ที่นี่
        SpawnedAllies.Clear();
    }
}