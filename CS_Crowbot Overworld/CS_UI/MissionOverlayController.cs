using UnityEngine;
using UnityEngine.UI;

public class MissionOverlayController : MonoBehaviour
{
    [Header("Input (Legacy Input)")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
    [SerializeField] private KeyCode closeKey = KeyCode.Escape;
    [SerializeField] private bool allowEscapeToClose = true;

    [Header("UI References")]
    [Tooltip("CanvasGroup ของแผ่นดำโปร่ง (เต็มจอ)")]
    [SerializeField] private CanvasGroup dimmerCanvasGroup;

    [Tooltip("Panel หน้าภารกิจ")]
    [SerializeField] private GameObject missionPanel;

    [Header("Behavior")]
    [SerializeField] private bool pauseGameWhenOpen = true;

    [Range(0f, 1f)]
    [SerializeField] private float dimmerAlpha = 0.65f;

    [SerializeField] private bool startClosed = true;

    public bool IsOpen { get; private set; }

    private float _previousTimeScale = 1f;

    private void Awake()
    {
        if (dimmerCanvasGroup == null)
        {
            Debug.LogError($"{nameof(MissionOverlayController)}: dimmerCanvasGroup is not assigned.", this);
        }

        if (missionPanel == null)
        {
            Debug.LogError($"{nameof(MissionOverlayController)}: missionPanel is not assigned.", this);
        }

        if (startClosed)
            CloseImmediate();
        else
            OpenImmediate();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            Toggle();
            return;
        }

        if (allowEscapeToClose && IsOpen && Input.GetKeyDown(closeKey))
        {
            Close();
        }
    }

    public void Toggle()
    {
        if (IsOpen) Close();
        else Open();
    }

    public void Open()
    {
        if (IsOpen) return;
        IsOpen = true;

        if (missionPanel != null)
            missionPanel.SetActive(true);

        ApplyDimmer(true);

        if (pauseGameWhenOpen)
        {
            _previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
    }

    public void Close()
    {
        if (!IsOpen) return;
        IsOpen = false;

        if (pauseGameWhenOpen)
        {
            // กลับค่าเดิม
            Time.timeScale = _previousTimeScale;
        }

        if (missionPanel != null)
            missionPanel.SetActive(false);

        ApplyDimmer(false);
    }

    private void OpenImmediate()
    {
        IsOpen = true;
        if (missionPanel != null) missionPanel.SetActive(true);
        ApplyDimmer(true);

        if (pauseGameWhenOpen)
        {
            _previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
    }

    private void CloseImmediate()
    {
        IsOpen = false;

        if (pauseGameWhenOpen)
        {
            // กันเคสเริ่มเกมมา timeScale ไม่ใช่ 1
            _previousTimeScale = Time.timeScale;
            Time.timeScale = _previousTimeScale;
        }

        if (missionPanel != null) missionPanel.SetActive(false);
        ApplyDimmer(false);
    }

    private void ApplyDimmer(bool visible)
    {
        if (dimmerCanvasGroup == null) return;

        dimmerCanvasGroup.alpha = visible ? dimmerAlpha : 0f;
        dimmerCanvasGroup.blocksRaycasts = visible; // กันคลิกทะลุ
        dimmerCanvasGroup.interactable = visible;
    }

    // เผื่ออยากให้ปุ่ม UI เรียกได้
    public void SetToggleKey(KeyCode key) => toggleKey = key;
    public void SetPauseWhenOpen(bool pause) => pauseGameWhenOpen = pause;
}