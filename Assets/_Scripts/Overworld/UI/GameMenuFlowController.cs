using UnityEngine;
using UnityEngine.UI;

public class GameMenuFlowController : MonoBehaviour
{
    public static GameMenuFlowController Instance { get; private set; }

    public enum MenuPage
    {
        Closed = 0,
        MenuButtons = 1,
        Story = 2,
        Map = 3,
        Team = 4,
        Character = 5,
        Archive = 6,
        Setting = 7,
        QuestDetail = 8,
        TeamSelect = 9
    }

    public enum TeamSelectMode
    {
        Team = 0,  // เปิดจากปุ่ม Team → Accept = Start Battle
        Story = 1   // เปิดจาก QuestDetail Accept → Accept = Start Story
    }

    public MenuPage CurrentPage { get; private set; } = MenuPage.Closed;
    public bool IsOpen => CurrentPage != MenuPage.Closed;
    public int SelectedMissionIndex { get; private set; } = -1;
    public TeamSelectMode CurrentTeamSelectMode { get; private set; } = TeamSelectMode.Team;

    [Header("Input")]
    [SerializeField] private KeyCode menuKey = KeyCode.Tab;
    [SerializeField] private KeyCode backKey = KeyCode.Escape;
    [SerializeField] private bool allowEscBack = true;

    [Header("Player")]
    [SerializeField] private PlayerController playerController;

    [Header("Dimmer")]
    [SerializeField] private CanvasGroup dimmer;
    [Range(0f, 1f)]
    [SerializeField] private float dimmerAlpha = 0.65f;

    [Header("Panels")]
    [SerializeField] private GameObject menuButtonsPanel;
    [SerializeField] private GameObject storyPanel;
    [SerializeField] private GameObject mapPanel;
    [SerializeField] private GameObject teamPanel;
    [SerializeField] private GameObject characterPanel;
    [SerializeField] private GameObject archivePanel;
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private GameObject questDetailPanel;
    [SerializeField] private GameObject teamSelectPanel;

    [Header("Behavior")]
    [SerializeField] private bool pauseGameWhenOpen = true;

    private float _prevTimeScale = 1f;

    // ─────────────────────────────────────────
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        GoToPage(MenuPage.Closed, immediate: true);
    }

    private void Update()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying)
            return;

        if (Input.GetKeyDown(menuKey))
        {
            if (IsOpen) CloseMenu();
            else OpenMenu();
            return;
        }

        if (allowEscBack && IsOpen && Input.GetKeyDown(backKey))
            HandleBack();
    }

    // ─────────────────────────────────────────
    // Back Logic
    // ─────────────────────────────────────────

    private void HandleBack()
    {
        switch (CurrentPage)
        {
            case MenuPage.MenuButtons:
                CloseMenu();
                break;
            case MenuPage.QuestDetail:
                GoToPage(MenuPage.Story);
                break;
            case MenuPage.TeamSelect:
                if (CurrentTeamSelectMode == TeamSelectMode.Story)
                    GoToPage(MenuPage.QuestDetail);
                else
                    GoToPage(MenuPage.MenuButtons);
                break;
            default:
                GoToPage(MenuPage.MenuButtons);
                break;
        }
    }

    // ─────────────────────────────────────────
    // Public API
    // ─────────────────────────────────────────

    public void OpenMenu() { GoToPage(MenuPage.MenuButtons); }
    public void CloseMenu() { GoToPage(MenuPage.Closed); }

    public void SetSelectedMission(int index)
    {
        SelectedMissionIndex = index;
        Debug.Log($"[GameMenuFlowController] Selected Mission: {index}");
    }

    // ─────────────────────────────────────────
    // ปุ่มเมนูหลัก
    // ─────────────────────────────────────────

    public void OnClick_Story() { GoToPage(MenuPage.Story); }
    public void OnClick_Map() { GoToPage(MenuPage.Map); }
    public void OnClick_Character() { GoToPage(MenuPage.Character); }
    public void OnClick_Archive() { GoToPage(MenuPage.Archive); }
    public void OnClick_Setting() { GoToPage(MenuPage.Setting); }
    public void OnClick_Resume() { CloseMenu(); }
    public void OnClick_Back() { HandleBack(); }

    // Team ในเมนูหลัก → mode = Team
    public void OnClick_Team()
    {
        CurrentTeamSelectMode = TeamSelectMode.Team;
        GoToPage(MenuPage.TeamSelect);
    }

    // Story Panel กดสล็อต Quest → เปิด QuestDetail
    public void OnClick_QuestDetail(int missionIndex)
    {
        SetSelectedMission(missionIndex);
        GoToPage(MenuPage.QuestDetail);
    }

    // QuestDetailPanel กด Accept → mode = Story
    public void OpenTeamSelectForStory()
    {
        CurrentTeamSelectMode = TeamSelectMode.Story;
        GoToPage(MenuPage.TeamSelect);
    }

    public void OnClick_Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ─────────────────────────────────────────
    // GoToPage
    // ─────────────────────────────────────────

    public void GoToPage(MenuPage page, bool immediate = false)
    {
        if (CurrentPage == page && !immediate) return;

        bool wasOpen = IsOpen;
        CurrentPage = page;
        bool nowOpen = IsOpen;

        if (pauseGameWhenOpen)
        {
            if (!wasOpen && nowOpen)
            {
                _prevTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }
            else if (wasOpen && !nowOpen)
            {
                Time.timeScale = _prevTimeScale;
            }
        }

        if (playerController != null)
            playerController.enabled = !nowOpen;

        ApplyDimmer(nowOpen);

        if (UIManager.Instance != null)
            UIManager.Instance.SetOverlayOpen(nowOpen);

        SetPanelActive(menuButtonsPanel, page == MenuPage.MenuButtons);
        SetPanelActive(storyPanel, page == MenuPage.Story);
        SetPanelActive(mapPanel, page == MenuPage.Map);
        SetPanelActive(teamPanel, page == MenuPage.Team);
        SetPanelActive(characterPanel, page == MenuPage.Character);
        SetPanelActive(archivePanel, page == MenuPage.Archive);
        SetPanelActive(settingPanel, page == MenuPage.Setting);
        SetPanelActive(questDetailPanel, page == MenuPage.QuestDetail);
        SetPanelActive(teamSelectPanel, page == MenuPage.TeamSelect);
    }

    // ─────────────────────────────────────────
    // Helper
    // ─────────────────────────────────────────

    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null) panel.SetActive(active);
    }

    private void ApplyDimmer(bool visible)
    {
        if (dimmer == null) return;
        dimmer.alpha = visible ? dimmerAlpha : 0f;
        dimmer.blocksRaycasts = visible;
        dimmer.interactable = visible;
    }
}