using System.Collections.Generic;
using UnityEngine;

public class DynamicBacklinePanelRow : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RectTransform container;
    [SerializeField] private BacklinePanelUI panelPrefab;

    [Header("Debug")]
    [SerializeField] private bool verbose = true;

    private readonly List<BacklinePanelUI> _spawnedPanels = new List<BacklinePanelUI>();
    private readonly List<BacklinePassiveUnit> _spawnedUnits = new List<BacklinePassiveUnit>();

    public IReadOnlyList<BacklinePanelUI> SpawnedPanels => _spawnedPanels;
    public IReadOnlyList<BacklinePassiveUnit> SpawnedUnits => _spawnedUnits;

    public void Clear()
    {
        for (int i = 0; i < _spawnedPanels.Count; i++)
            if (_spawnedPanels[i] != null)
                Destroy(_spawnedPanels[i].gameObject);

        _spawnedPanels.Clear();
        _spawnedUnits.Clear();
    }

    // ✅ อ่าน SelectedBackline จาก PartySelectionData โดยตรง
    public void Build(BattleManager battleManager)
    {
        if (container == null)
        {
            Debug.LogError("❌ DynamicBacklinePanelRow: container is null", this);
            return;
        }

        if (panelPrefab == null)
        {
            Debug.LogError("❌ DynamicBacklinePanelRow: panelPrefab is null", this);
            return;
        }

        Clear();

        if (PartySelectionData.Instance == null)
        {
            Debug.LogError("❌ DynamicBacklinePanelRow: PartySelectionData.Instance is null", this);
            return;
        }

        var dataList = PartySelectionData.Instance.SelectedBackline;

        if (verbose)
            Debug.Log($"[DynamicBacklinePanelRow] Build from SelectedBackline count={dataList.Count}", this);

        for (int i = 0; i < dataList.Count; i++)
        {
            AllyBacklineData data = dataList[i];
            if (data == null) continue;

            BacklinePanelUI panel = Instantiate(panelPrefab, container);
            panel.gameObject.SetActive(true);

            // ✅ ดึง BacklinePassiveUnit จากบน prefab เดียวกัน
            BacklinePassiveUnit unit = panel.GetComponent<BacklinePassiveUnit>();
            if (unit != null)
            {
                unit.SetData(data);
                unit.Initialize(battleManager);
            }
            else
            {
                Debug.LogWarning($"⚠️ BacklinePanelUI prefab ไม่มี BacklinePassiveUnit บน GameObject เดียวกัน", panel);
            }

            panel.Bind(unit);

            _spawnedPanels.Add(panel);
            _spawnedUnits.Add(unit);

            if (verbose)
                Debug.Log($"  + backline panel '{data.characterName}'", this);
        }
    }
}