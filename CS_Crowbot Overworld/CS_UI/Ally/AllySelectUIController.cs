using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AllySelectUIController : MonoBehaviour
{
    public static AllySelectUIController Instance { get; private set; }

    [Header("Roster Data (Top Row)")]
    [SerializeField] private List<BaseCharacterData> availableAllies = new List<BaseCharacterData>();

    [Header("Roster UI Slots (Top Row)")]
    [SerializeField] private AllyRosterSlotUI[] rosterSlots = new AllyRosterSlotUI[8];

    [Header("Party UI Slots (Bottom Row)")]
    [SerializeField] private AllyPartySlotUI[] backSlots = new AllyPartySlotUI[4];
    [SerializeField] private AllyPartySlotUI[] frontSlots = new AllyPartySlotUI[4];

    [Header("Scene")]
    [SerializeField] private string battleSceneName = "BattleScene";

    [Header("Buttons")]
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button returnButton;

    private readonly HashSet<string> _selectedIds = new HashSet<string>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (acceptButton != null) acceptButton.onClick.AddListener(OnAccept);
        if (returnButton != null) returnButton.onClick.AddListener(() => gameObject.SetActive(false));
    }

    private void OnEnable()
    {
        BindRosterSlots();
        RebuildSelectedIdCache();
        UpdateRosterSelectedVisuals();
        RefreshPartySlots();
    }

    // ✅ วิธี B: ใช้ characterId (fallback -> characterName)
    private string GetId(BaseCharacterData data)
    {
        if (data == null) return "";
        if (!string.IsNullOrEmpty(data.characterId)) return data.characterId;
        return data.characterName;
    }

    private void BindRosterSlots()
    {
        for (int i = 0; i < rosterSlots.Length; i++)
        {
            BaseCharacterData d = (i >= 0 && i < availableAllies.Count) ? availableAllies[i] : null;
            rosterSlots[i]?.Bind(d);
        }
    }

    private void RefreshPartySlots()
    {
        for (int i = 0; i < backSlots.Length; i++)
            backSlots[i]?.SetData(backSlots[i].GetData());

        for (int i = 0; i < frontSlots.Length; i++)
            frontSlots[i]?.SetData(frontSlots[i].GetData());
    }

    private void RebuildSelectedIdCache()
    {
        _selectedIds.Clear();

        for (int i = 0; i < backSlots.Length; i++)
        {
            BaseCharacterData d = backSlots[i]?.GetData();
            if (d != null) _selectedIds.Add(GetId(d));
        }

        for (int i = 0; i < frontSlots.Length; i++)
        {
            BaseCharacterData d = frontSlots[i]?.GetData();
            if (d != null) _selectedIds.Add(GetId(d));
        }
    }

    private void UpdateRosterSelectedVisuals()
    {
        for (int i = 0; i < rosterSlots.Length; i++)
        {
            if (rosterSlots[i] == null) continue;

            BaseCharacterData d = rosterSlots[i].GetData();
            if (d == null) { rosterSlots[i].SetSelected(false); continue; }

            rosterSlots[i].SetSelected(_selectedIds.Contains(GetId(d)));
        }
    }

    // ====== add from roster (เติมขวา->ซ้าย) ======
    public void OnRosterClicked(BaseCharacterData data)
    {
        if (data == null) return;

        if (data.faction != CharacterFaction.Ally) return;

        string id = GetId(data);
        if (_selectedIds.Contains(id)) return;

        // ✅ ใช้ allyRow เฉพาะตอนเป็น AllyCharacterData
        AllyPartySlotUI[] row = frontSlots; // default
        AllyCharacterData allyData = data as AllyCharacterData;
        if (allyData != null)
            row = (allyData.allyRow == AllyRow.Front) ? frontSlots : backSlots;

        bool inserted = InsertRightToLeftFirstEmpty(row, data);
        if (!inserted) return;

        RebuildSelectedIdCache();
        UpdateRosterSelectedVisuals();
        RefreshPartySlots();
    }

    private bool InsertRightToLeftFirstEmpty(AllyPartySlotUI[] rowSlots, BaseCharacterData data)
    {
        for (int i = 0; i < rowSlots.Length; i++)
        {
            if (rowSlots[i] == null)
            {
                Debug.LogError($"Row slot not assigned at index {i}.", this);
                return false;
            }
        }

        for (int i = rowSlots.Length - 1; i >= 0; i--)
        {
            if (rowSlots[i].GetData() == null)
            {
                rowSlots[i].SetData(data);
                _selectedIds.Add(GetId(data));
                return true;
            }
        }
        return false;
    }

    // ====== remove by clicking bottom slot ======
    public void RemoveFromPartyByData(BaseCharacterData data)
    {
        if (data == null) return;
        string id = GetId(data);

        if (!RemoveFromRowById(backSlots, id))
            RemoveFromRowById(frontSlots, id);

        RebuildSelectedIdCache();
        UpdateRosterSelectedVisuals();
        RefreshPartySlots();
    }

    private bool RemoveFromRowById(AllyPartySlotUI[] rowSlots, string id)
    {
        for (int i = 0; i < rowSlots.Length; i++)
        {
            if (rowSlots[i] == null) continue;
            BaseCharacterData d = rowSlots[i].GetData();
            if (d == null) continue;

            if (GetId(d) == id)
            {
                rowSlots[i].SetData(null);
                return true;
            }
        }
        return false;
    }

    private void OnAccept()
    {
        int frontCount = 0;
        for (int i = 0; i < frontSlots.Length; i++)
        {
            if (frontSlots[i] != null && frontSlots[i].GetData() != null)
                frontCount++;
        }

        if (frontCount <= 0)
        {
            Debug.LogWarning("⛔ ต้องเลือกตัวละครแนวหน้าอย่างน้อย 1 ตัว ก่อนกด Accept!");
            return;
        }

        if (PartySelectionData.Instance == null)
        {
            Debug.LogError("PartySelectionData.Instance is null (ต้องมี GameObject ที่ใส่ PartySelectionData ในซีนเลือกทีม)");
            return;
        }

        BaseCharacterData[] back = new BaseCharacterData[4];
        BaseCharacterData[] front = new BaseCharacterData[4];

        for (int i = 0; i < 4; i++)
        {
            back[i] = backSlots[i] != null ? backSlots[i].GetData() : null;
            front[i] = frontSlots[i] != null ? frontSlots[i].GetData() : null;
        }

        PartySelectionData.Instance.SetPartyFromSlots(back, front);

        SceneManager.LoadScene(battleSceneName);
    }
}