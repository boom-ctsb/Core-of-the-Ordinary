using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AllySelectUIController : MonoBehaviour
{
    public static AllySelectUIController Instance { get; private set; }

    [Header("Roster Data — แนวหน้า")]
    [SerializeField] private List<BaseCharacterData> availableAllies = new List<BaseCharacterData>();

    [Header("Roster UI Slots — แนวหน้า (8 ช่อง)")]
    [SerializeField] private AllyRosterSlotUI[] rosterSlots = new AllyRosterSlotUI[8];

    [Header("Party UI Slots — แนวหน้า")]
    [SerializeField] private AllyPartySlotUI[] backSlots = new AllyPartySlotUI[4];
    [SerializeField] private AllyPartySlotUI[] frontSlots = new AllyPartySlotUI[4];

    [Header("Roster Data — แนวหลัง")]
    [SerializeField] private List<AllyBacklineData> availableBacklines = new List<AllyBacklineData>();

    [Header("Roster UI Slots — แนวหลัง (8 ช่อง)")]
    [SerializeField] private BacklineRosterSlotUI[] backlineRosterSlots = new BacklineRosterSlotUI[8];

    [Header("Party UI Slots — แนวหลัง (4 ช่อง)")]
    [SerializeField] private BacklinePartySlotUI[] backlinePartySlots = new BacklinePartySlotUI[4];

    [Header("Right Panel (สลับตาม Mode)")]
    [SerializeField] private GameObject rightPanelTeam;
    [SerializeField] private GameObject rightPanelStory;

    [Header("Scene Names")]
    [SerializeField] private string battleSceneName = "BattleScene";
    [SerializeField] private string storySceneName = "StoryScene";

    [Header("Buttons")]
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button returnButton;

    private readonly HashSet<string> _selectedIds = new HashSet<string>();
    private readonly HashSet<string> _selectedBacklineIds = new HashSet<string>();

    // ─────────────────────────────────────
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (acceptButton != null)
        {
            acceptButton.onClick.RemoveAllListeners();
            acceptButton.onClick.AddListener(OnAccept);
            Debug.Log("[AllySelectUI] Awake — acceptButton linked");
        }
        else
        {
            Debug.LogError("[AllySelectUI] Awake — acceptButton is NULL! ลาก Button ใน Inspector");
        }

        if (returnButton != null)
        {
            returnButton.onClick.RemoveAllListeners();
            returnButton.onClick.AddListener(OnReturn);
        }
        else
        {
            Debug.LogWarning("[AllySelectUI] Awake — returnButton is NULL");
        }
    }

    private void OnEnable()
    {
        RefreshRightPanel();
        BindRosterSlots();
        BindBacklineRosterSlots();
        RebuildSelectedIdCache();
        RebuildSelectedBacklineIdCache();
        UpdateRosterSelectedVisuals();
        UpdateBacklineRosterSelectedVisuals();
        RefreshAllPartySlots();
    }

    // ─────────────────────────────────────
    // Right Panel
    // ─────────────────────────────────────

    private void RefreshRightPanel()
    {
        bool isTeamMode = GameMenuFlowController.Instance == null
            || GameMenuFlowController.Instance.CurrentTeamSelectMode
               == GameMenuFlowController.TeamSelectMode.Team;

        if (rightPanelTeam != null) rightPanelTeam.SetActive(isTeamMode);
        if (rightPanelStory != null) rightPanelStory.SetActive(!isTeamMode);
    }

    // ─────────────────────────────────────
    // Buttons
    // ─────────────────────────────────────

    private void OnReturn()
    {
        var ctrl = GameMenuFlowController.Instance;
        if (ctrl == null) { gameObject.SetActive(false); return; }

        if (ctrl.CurrentTeamSelectMode == GameMenuFlowController.TeamSelectMode.Story)
            ctrl.GoToPage(GameMenuFlowController.MenuPage.QuestDetail);
        else
            ctrl.GoToPage(GameMenuFlowController.MenuPage.MenuButtons);
    }

    // ─────────────────────────────────────
    // ID helpers
    // ─────────────────────────────────────

    private string GetId(BaseCharacterData data)
    {
        if (data == null) return "";
        return !string.IsNullOrEmpty(data.characterId) ? data.characterId : data.characterName;
    }

    private string GetBacklineId(AllyBacklineData data) => data == null ? "" : data.characterName;

    // ─────────────────────────────────────
    // Bind Roster
    // ─────────────────────────────────────

    private void BindRosterSlots()
    {
        for (int i = 0; i < rosterSlots.Length; i++)
            rosterSlots[i]?.Bind(i < availableAllies.Count ? availableAllies[i] : null);
    }

    private void BindBacklineRosterSlots()
    {
        for (int i = 0; i < backlineRosterSlots.Length; i++)
            backlineRosterSlots[i]?.Bind(i < availableBacklines.Count ? availableBacklines[i] : null);
    }

    // ─────────────────────────────────────
    // Refresh Party Slots
    // ─────────────────────────────────────

    private void RefreshAllPartySlots()
    {
        foreach (var s in backSlots) s?.SetData(s.GetData());
        foreach (var s in frontSlots) s?.SetData(s.GetData());
        foreach (var s in backlinePartySlots) s?.SetData(s.GetData());
    }

    // ─────────────────────────────────────
    // Rebuild ID Cache
    // ─────────────────────────────────────

    private void RebuildSelectedIdCache()
    {
        _selectedIds.Clear();
        foreach (var s in backSlots) { var d = s?.GetData(); if (d != null) _selectedIds.Add(GetId(d)); }
        foreach (var s in frontSlots) { var d = s?.GetData(); if (d != null) _selectedIds.Add(GetId(d)); }
    }

    private void RebuildSelectedBacklineIdCache()
    {
        _selectedBacklineIds.Clear();
        foreach (var s in backlinePartySlots)
        { var d = s?.GetData(); if (d != null) _selectedBacklineIds.Add(GetBacklineId(d)); }
    }

    // ─────────────────────────────────────
    // Update Selected Visuals
    // ─────────────────────────────────────

    private void UpdateRosterSelectedVisuals()
    {
        foreach (var s in rosterSlots)
        {
            if (s == null) continue;
            var d = s.GetData();
            s.SetSelected(d != null && _selectedIds.Contains(GetId(d)));
        }
    }

    private void UpdateBacklineRosterSelectedVisuals()
    {
        foreach (var s in backlineRosterSlots)
        {
            if (s == null) continue;
            var d = s.GetData();
            s.SetSelected(d != null && _selectedBacklineIds.Contains(GetBacklineId(d)));
        }
    }

    // ─────────────────────────────────────
    // Add from Roster
    // ─────────────────────────────────────

    public void OnRosterClicked(BaseCharacterData data)
    {
        if (data == null || data.faction != CharacterFaction.Ally) return;

        string id = GetId(data);
        if (_selectedIds.Contains(id)) return;

        AllyPartySlotUI[] row = frontSlots;
        if (data is AllyCharacterData allyData)
            row = (allyData.allyRow == AllyRow.Front) ? frontSlots : backSlots;

        if (!InsertRightToLeftFirstEmpty(row, data)) return;

        RebuildSelectedIdCache();
        UpdateRosterSelectedVisuals();
        RefreshAllPartySlots();
    }

    public void OnBacklineRosterClicked(AllyBacklineData data)
    {
        if (data == null) return;

        string id = GetBacklineId(data);
        if (_selectedBacklineIds.Contains(id)) return;

        if (!InsertBacklineRightToLeftFirstEmpty(backlinePartySlots, data)) return;

        RebuildSelectedBacklineIdCache();
        UpdateBacklineRosterSelectedVisuals();
        RefreshAllPartySlots();
    }

    private bool InsertRightToLeftFirstEmpty(AllyPartySlotUI[] slots, BaseCharacterData data)
    {
        for (int i = 0; i < slots.Length; i++)
            if (slots[i] == null) { Debug.LogError($"Slot[{i}] not assigned.", this); return false; }

        for (int i = slots.Length - 1; i >= 0; i--)
        {
            if (slots[i].GetData() == null)
            {
                slots[i].SetData(data);
                _selectedIds.Add(GetId(data));
                return true;
            }
        }
        return false;
    }

    private bool InsertBacklineRightToLeftFirstEmpty(BacklinePartySlotUI[] slots, AllyBacklineData data)
    {
        for (int i = 0; i < slots.Length; i++)
            if (slots[i] == null) { Debug.LogError($"BacklineSlot[{i}] not assigned.", this); return false; }

        for (int i = slots.Length - 1; i >= 0; i--)
        {
            if (slots[i].GetData() == null)
            {
                slots[i].SetData(data);
                _selectedBacklineIds.Add(GetBacklineId(data));
                return true;
            }
        }
        return false;
    }

    // ─────────────────────────────────────
    // Remove
    // ─────────────────────────────────────

    public void RemoveFromPartyByData(BaseCharacterData data)
    {
        if (data == null) return;
        string id = GetId(data);

        if (!RemoveFromRowById(backSlots, id))
            RemoveFromRowById(frontSlots, id);

        RebuildSelectedIdCache();
        UpdateRosterSelectedVisuals();
        RefreshAllPartySlots();
    }

    public void RemoveBacklineFromPartyByData(AllyBacklineData data)
    {
        if (data == null) return;
        string id = GetBacklineId(data);
        RemoveBacklineFromRowById(backlinePartySlots, id);
        RebuildSelectedBacklineIdCache();
        UpdateBacklineRosterSelectedVisuals();
        RefreshAllPartySlots();
    }

    private bool RemoveFromRowById(AllyPartySlotUI[] slots, string id)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;
            var d = slots[i].GetData();
            if (d != null && GetId(d) == id) { slots[i].SetData(null); return true; }
        }
        return false;
    }

    private bool RemoveBacklineFromRowById(BacklinePartySlotUI[] slots, string id)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;
            var d = slots[i].GetData();
            if (d != null && GetBacklineId(d) == id) { slots[i].SetData(null); return true; }
        }
        return false;
    }

    // ─────────────────────────────────────
    // Accept
    // ─────────────────────────────────────

    private void OnAccept()
    {
        Debug.Log("[AllySelectUI] OnAccept called");

        int frontCount = CountFrontSelected();
        Debug.Log($"[AllySelectUI] frontCount = {frontCount}");

        if (frontCount <= 0)
        {
            Debug.LogWarning("⛔ ต้องเลือกตัวละครแนวหน้าอย่างน้อย 1 ตัว ก่อนกด Accept!");
            return;
        }

        if (PartySelectionData.Instance == null)
        {
            Debug.LogError("❌ PartySelectionData.Instance is null — ใส่ PartySelectionData ใน Scene ด้วย");
            return;
        }

        var back = new BaseCharacterData[4];
        var front = new BaseCharacterData[4];
        for (int i = 0; i < 4; i++)
        {
            back[i] = (backSlots != null && i < backSlots.Length && backSlots[i] != null) ? backSlots[i].GetData() : null;
            front[i] = (frontSlots != null && i < frontSlots.Length && frontSlots[i] != null) ? frontSlots[i].GetData() : null;
        }
        PartySelectionData.Instance.SetPartyFromSlots(back, front);

        var backline = new AllyBacklineData[4];
        for (int i = 0; i < 4; i++)
            backline[i] = (backlinePartySlots != null && i < backlinePartySlots.Length && backlinePartySlots[i] != null)
                ? backlinePartySlots[i].GetData() : null;
        PartySelectionData.Instance.SetBacklineFromSlots(backline);

        bool isTeamMode = GameMenuFlowController.Instance == null
            || GameMenuFlowController.Instance.CurrentTeamSelectMode
               == GameMenuFlowController.TeamSelectMode.Team;

        string scene = isTeamMode ? battleSceneName : storySceneName;
        Debug.Log($"[AllySelectUI] isTeamMode={isTeamMode}  scene='{scene}'");

        if (!Application.CanStreamedLevelBeLoaded(scene))
        {
            Debug.LogError($"❌ Scene '{scene}' ไม่อยู่ใน Build Settings!");
            return;
        }

        // ✅ reset timeScale ก่อน LoadScene เสมอ
        Time.timeScale = 1f;
        SceneManager.LoadScene(scene);
    }

    private int CountFrontSelected()
    {
        int n = 0;
        foreach (var s in frontSlots) if (s?.GetData() != null) n++;
        return n;
    }
}