using System;
using UnityEngine;
using UnityEngine.UI;

public class MissionUIController : MonoBehaviour
{
    public static MissionUIController Instance { get; private set; }

    [Header("Input (Legacy; works when Active Input Handling = Both)")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
    [SerializeField] private KeyCode closeKey = KeyCode.Escape;
    [SerializeField] private bool allowEscapeToClose = true;

    [Header("UI")]
    [SerializeField] private CanvasGroup dimmer;     // แผ่นดำโปร่งเต็มจอ
    [SerializeField] private GameObject rootPanel;   // panel หน้าภารกิจทั้งก้อน
    [SerializeField] private Button exitButton;
    [SerializeField] private Button acceptButton;

    [Header("Preview / Details (optional)")]
    [Tooltip("แสดงชื่อภารกิจที่เลือก")]
    [SerializeField] private Text selectedMissionNameText;

    [Tooltip("แสดงรายละเอียด/require")]
    [SerializeField] private Text requireText;

    [Tooltip("แสดงรางวัล/receive")]
    [SerializeField] private Text receiveText;

    [Header("Behavior")]
    [Range(0f, 1f)]
    [SerializeField] private float dimmerAlpha = 0.65f;

    [Tooltip("เปิดแล้วหยุดเวลาไหม")]
    [SerializeField] private bool pauseGameWhenOpen = true;

    [Tooltip("เริ่มเกมให้ปิดไว้ก่อน")]
    [SerializeField] private bool startClosed = true;

    public bool IsOpen { get; private set; }
    public int SelectedMissionIndex { get; private set; } = -1;

    // event ให้หน้าอื่นฟัง (เช่นหน้าจอเลือกตัวละคร)
    public event Action<int> OnAcceptMission; // ส่ง index ของภารกิจที่เลือก (หรือ -1 ถ้าไม่เลือก)

    private float _prevTimeScale = 1f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (exitButton != null) exitButton.onClick.AddListener(Close);
        if (acceptButton != null) acceptButton.onClick.AddListener(Accept);

        if (startClosed) CloseImmediate();
        else OpenImmediate();
    }

    private void Update()
    {
        // ถ้าคุณต้องการ “กันไม่ให้เปิดเมนูซ้อนเมนู”:
        // if (UIManager.Instance != null && UIManager.Instance.IsOverlayOpen && !IsOpen) return;

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

    // ====== Support Input System new ======
    // ให้คุณไป bind ใน InputAction แล้วเรียก method นี้ได้
    public void ToggleFromInputSystem()
    {
        Toggle();
    }

    // ====== Open / Close ======
    public void Toggle()
    {
        if (IsOpen) Close();
        else Open();
    }

    public void Open()
    {
        if (IsOpen) return;
        IsOpen = true;

        if (rootPanel != null) rootPanel.SetActive(true);
        ApplyDimmer(true);

        if (UIManager.Instance != null) UIManager.Instance.SetOverlayOpen(true);

        if (pauseGameWhenOpen)
        {
            _prevTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
    }

    public void Close()
    {
        if (!IsOpen) return;
        IsOpen = false;

        if (pauseGameWhenOpen)
        {
            Time.timeScale = _prevTimeScale;
        }

        if (rootPanel != null) rootPanel.SetActive(false);
        ApplyDimmer(false);

        if (UIManager.Instance != null) UIManager.Instance.SetOverlayOpen(false);
    }

    private void OpenImmediate()
    {
        IsOpen = true;
        if (rootPanel != null) rootPanel.SetActive(true);
        ApplyDimmer(true);

        if (UIManager.Instance != null) UIManager.Instance.SetOverlayOpen(true);

        if (pauseGameWhenOpen)
        {
            _prevTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }
    }

    private void CloseImmediate()
    {
        IsOpen = false;
        if (rootPanel != null) rootPanel.SetActive(false);
        ApplyDimmer(false);

        if (UIManager.Instance != null) UIManager.Instance.SetOverlayOpen(false);
    }

    private void ApplyDimmer(bool visible)
    {
        if (dimmer == null) return;

        dimmer.alpha = visible ? dimmerAlpha : 0f;
        dimmer.blocksRaycasts = visible;
        dimmer.interactable = visible;
    }

    // ====== Mission selection ======
    // เรียกจากปุ่มช่องๆด้านขวา (OnClick) แล้วส่ง index เข้ามา
    public void SelectMission(int index, string missionName = null, string require = null, string receive = null)
    {
        SelectedMissionIndex = index;

        if (selectedMissionNameText != null)
            selectedMissionNameText.text = string.IsNullOrEmpty(missionName) ? $"Mission #{index}" : missionName;

        if (requireText != null)
            requireText.text = require ?? "";

        if (receiveText != null)
            receiveText.text = receive ?? "";
    }

    // ====== Buttons ======
    public void Accept()
    {
        // ปุ่ม Accept: ปิดหน้าภารกิจ แล้วส่ง event ให้ไปหน้าเลือกตัวละครต่อ
        int index = SelectedMissionIndex;
        Close();
        OnAcceptMission?.Invoke(index);
    }
}