using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EpisodeListPageUI : MonoBehaviour
{
    [Header("Stage Info (ซ้าย)")]
    [SerializeField] private Image stageThumbnail;
    [SerializeField] private Image stageNameImage;
    [SerializeField] private TextMeshProUGUI stageStoryText;

    [Header("EP Slots — 4 ช่องตายตัว")]
    [SerializeField] private EpisodeSlotUI[] episodeSlots;

    [Header("Scroll Buttons (optional)")]
    [SerializeField] private Button scrollUpButton;
    [SerializeField] private Button scrollDownButton;

    [Header("Tab")]
    [SerializeField] private GameObject mainEpPanel;
    [SerializeField] private GameObject subEpPanel;

    private StageData _currentStage;
    private EpisodeData[] _mainEps;
    private EpisodeData[] _subEps;
    private EpisodeData[] _currentList;
    private int _startIndex = 0;

    // ─────────────────────────────────────────

    public void Open(StageData stage)
    {
        gameObject.SetActive(true);
        _currentStage = stage;

        if (stageThumbnail != null) stageThumbnail.sprite = stage.stageThumbnail;
        if (stageNameImage != null) stageNameImage.sprite = stage.stageNameImage;
        if (stageStoryText != null) stageStoryText.text = stage.storyDescription;

        // แยก Main / Sub จาก episodes ของ Stage นั้น
        var mainList = new System.Collections.Generic.List<EpisodeData>();
        var subList = new System.Collections.Generic.List<EpisodeData>();

        foreach (var ep in stage.episodes)
        {
            if (ep.isMain) mainList.Add(ep);
            else subList.Add(ep);
        }

        _mainEps = mainList.ToArray();
        _subEps = subList.ToArray();

        // ผูกปุ่ม Scroll (ถ้ามี)
        if (scrollUpButton != null)
        {
            scrollUpButton.onClick.RemoveAllListeners();
            scrollUpButton.onClick.AddListener(ScrollUp);
        }
        if (scrollDownButton != null)
        {
            scrollDownButton.onClick.RemoveAllListeners();
            scrollDownButton.onClick.AddListener(ScrollDown);
        }

        ShowMainTab();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────
    // Mouse Scroll
    // ─────────────────────────────────────────

    private void Update()
    {
        if (!gameObject.activeSelf) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) ScrollUp();
        else if (scroll < 0f) ScrollDown();
    }

    // ─────────────────────────────────────────
    // Tab
    // ─────────────────────────────────────────

    public void ShowMainTab()
    {
        if (mainEpPanel != null) mainEpPanel.SetActive(true);
        if (subEpPanel != null) subEpPanel.SetActive(false);
        SwitchList(_mainEps);
    }

    public void ShowSubTab()
    {
        if (mainEpPanel != null) mainEpPanel.SetActive(false);
        if (subEpPanel != null) subEpPanel.SetActive(true);
        SwitchList(_subEps);
    }

    // ─────────────────────────────────────────
    // Scroll
    // ─────────────────────────────────────────

    private void SwitchList(EpisodeData[] list)
    {
        _currentList = list;
        _startIndex = 0;
        RefreshSlots();
    }

    private void ScrollUp()
    {
        if (_startIndex <= 0) return;
        _startIndex--;
        RefreshSlots();
    }

    private void ScrollDown()
    {
        if (_currentList == null) return;
        if (_startIndex + episodeSlots.Length >= _currentList.Length) return;
        _startIndex++;
        RefreshSlots();
    }

    // ─────────────────────────────────────────
    // Refresh
    // ─────────────────────────────────────────

    private void RefreshSlots()
    {
        for (int i = 0; i < episodeSlots.Length; i++)
        {
            int dataIndex = _startIndex + i;

            if (_currentList != null && dataIndex < _currentList.Length)
            {
                episodeSlots[i].gameObject.SetActive(true);
                episodeSlots[i].Setup(_currentList[dataIndex], _currentStage);
            }
            else
            {
                episodeSlots[i].gameObject.SetActive(false);
            }
        }

        // อัปเดตปุ่ม interactable
        if (scrollUpButton != null)
            scrollUpButton.interactable = _startIndex > 0;

        if (scrollDownButton != null)
            scrollDownButton.interactable = _currentList != null &&
                _startIndex + episodeSlots.Length < _currentList.Length;
    }
}