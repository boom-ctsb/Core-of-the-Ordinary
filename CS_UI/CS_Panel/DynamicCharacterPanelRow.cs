using System.Collections.Generic;
using UnityEngine;

public class DynamicCharacterPanelRow : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RectTransform container;
    [SerializeField] private CharacterPanel panelPrefab;

    [Header("Debug")]
    [SerializeField] private bool verbose = true;

    private readonly List<CharacterPanel> _spawnedPanels = new List<CharacterPanel>();
    public IReadOnlyList<CharacterPanel> SpawnedPanels => _spawnedPanels;

    public void Clear()
    {
        for (int i = 0; i < _spawnedPanels.Count; i++)
        {
            if (_spawnedPanels[i] != null)
                Destroy(_spawnedPanels[i].gameObject);
        }
        _spawnedPanels.Clear();
    }

    public void Build(List<BattleCharacter> charactersInOrder)
    {
        if (container == null)
        {
            Debug.LogError("❌ DynamicCharacterPanelRow: container is null", this);
            return;
        }
        if (panelPrefab == null)
        {
            Debug.LogError("❌ DynamicCharacterPanelRow: panelPrefab is null", this);
            return;
        }

        Clear();

        if (charactersInOrder == null)
        {
            if (verbose) Debug.Log("[DynamicCharacterPanelRow] charactersInOrder is null -> build nothing", this);
            return;
        }

        if (verbose)
        {
            Debug.Log($"[DynamicCharacterPanelRow] Build '{container.name}' count={charactersInOrder.Count} (before children={container.childCount})", this);
        }

        for (int i = 0; i < charactersInOrder.Count; i++)
        {
            BattleCharacter bc = charactersInOrder[i];
            if (bc == null) continue;

            CharacterPanel panel = Instantiate(panelPrefab, container);
            panel.gameObject.SetActive(true);
            panel.Initialize(bc);

            _spawnedPanels.Add(panel);

            if (verbose)
                Debug.Log($"  + panel '{panel.name}' for {bc.CharacterName}", this);
        }

        if (verbose)
            Debug.Log($"[DynamicCharacterPanelRow] After build children={container.childCount}, spawnedPanels={_spawnedPanels.Count}", this);
    }

    public void HighlightCurrentTurn(BattleCharacter current)
    {
        for (int i = 0; i < _spawnedPanels.Count; i++)
        {
            var panel = _spawnedPanels[i];
            if (panel == null) continue;

            if (current != null && panel.character == current) panel.Highlight();
            else panel.Normalize();
        }
    }
}