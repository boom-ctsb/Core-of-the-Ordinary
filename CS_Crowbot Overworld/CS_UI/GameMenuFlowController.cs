using UnityEngine;
using UnityEngine.UI;

public class GameMenuFlowController : MonoBehaviour
{
    public static GameMenuFlowController Instance { get; private set; }

    public enum MenuState
    {
        Closed = 0,
        Mission = 1,
        PartySelect = 2
    }

    [Header("Input (Legacy; works with Active Input Handling = Both)")]
    [SerializeField] private KeyCode openMissionKey = KeyCode.Tab;
    [SerializeField] private KeyCode backKey = KeyCode.Escape;

    [Header("Overlay")]
    [SerializeField] private CanvasGroup dimmer;
    [Range(0f, 1f)]
    [SerializeField] private float dimmerAlpha = 0.65f;

    [Header("Panels")]
    [SerializeField] private GameObject missionPanelRoot;
    [SerializeField] private GameObject partySelectPanelRoot;

    [Header("Mission Buttons")]
    [SerializeField] private Button missionExitButton;
    [SerializeField] private Button missionAcceptButton;

    [Header("Party Select Buttons")]
    [SerializeField] private Button partyExitButton;
    [SerializeField] private Button partyAcceptButton; // (optional) เผื่อมี "Confirm"

    [Header("Behavior")]
    [SerializeField] private bool pauseGameWhenOpen = true;
    [SerializeField] private bool allowEscBack = true;

    public MenuState State { get; private set; } = MenuState.Closed;

    // ข้อมูลที่เลือกจากหน้าภารกิจ
    public int SelectedMissionIndex { get; private set; } = -1;

    private float _prevTimeScale = 1f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Hook buttons
        if (missionExitButton != null) missionExitButton.onClick.AddListener(CloseAll);
        if (missionAcceptButton != null) missionAcceptButton.onClick.AddListener(GoToPartySelect);

        if (partyExitButton != null) partyExitButton.onClick.AddListener(CloseAll);
        if (partyAcceptButton != null) partyAcceptButton.onClick.AddListener(ConfirmParty); // optional

        // start closed
        SetState(MenuState.Closed, immediate: true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(openMissionKey))
        {
            if (State == MenuState.Closed) OpenMission();
            else CloseAll();
            return;
        }

        if (!allowEscBack) return;

        if (State != MenuState.Closed && Input.GetKeyDown(backKey))
        {
            // ESC behavior:
            // PartySelect -> Mission
            // Mission -> CloseAll
            if (State == MenuState.PartySelect) OpenMission();
            else CloseAll();
        }
    }

    // ===== Public API =====

    public void OpenMission()
    {
        SetState(MenuState.Mission, immediate: false);
    }

    public void GoToPartySelect()
    {
        // Accept on mission -> open party select (still paused)
        SetState(MenuState.PartySelect, immediate: false);
    }

    public void CloseAll()
    {
        SetState(MenuState.Closed, immediate: false);
    }

    public void SetSelectedMission(int index)
    {
        SelectedMissionIndex = index;
    }

    // เผื่อ Input System ใหม่: เอาไป bind Action แล้วเรียกได้
    public void ToggleMissionFromInputSystem()
    {
        if (State == MenuState.Closed) OpenMission();
        else CloseAll();
    }

    // ===== Party confirm (optional) =====
    private void ConfirmParty()
    {
        // ตรงนี้คุณค่อยต่อไปเข้าระบบเริ่มสู้ / ยืนยันทีม / ฯลฯ
        // ตอนนี้ผมทำให้ปิดเมนูกลับไปเล่นก่อน
        CloseAll();
    }

    // ===== Internals =====
    private void SetState(MenuState newState, bool immediate)
    {
        if (State == newState && !immediate) return;

        // Handle pause transitions
        bool goingFromClosedToOpen = (State == MenuState.Closed && newState != MenuState.Closed);
        bool goingFromOpenToClosed = (State != MenuState.Closed && newState == MenuState.Closed);

        State = newState;

        // Panels
        if (missionPanelRoot != null) missionPanelRoot.SetActive(State == MenuState.Mission);
        if (partySelectPanelRoot != null) partySelectPanelRoot.SetActive(State == MenuState.PartySelect);

        // Dimmer
        bool overlayVisible = State != MenuState.Closed;
        ApplyDimmer(overlayVisible);

        // UIManager overlay flag (cursor)
        if (UIManager.Instance != null)
            UIManager.Instance.SetOverlayOpen(overlayVisible);

        if (pauseGameWhenOpen)
        {
            if (goingFromClosedToOpen)
            {
                _prevTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }
            else if (goingFromOpenToClosed)
            {
                Time.timeScale = _prevTimeScale;
            }
        }
    }

    private void ApplyDimmer(bool visible)
    {
        if (dimmer == null) return;

        dimmer.alpha = visible ? dimmerAlpha : 0f;
        dimmer.blocksRaycasts = visible;
        dimmer.interactable = visible;
    }
}