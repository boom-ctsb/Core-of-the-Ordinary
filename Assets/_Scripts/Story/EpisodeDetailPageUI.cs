using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EpisodeDetailPageUI : MonoBehaviour
{
    [Header("EP Info")]
    [SerializeField] private Image epThumbnail;
    [SerializeField] private TextMeshProUGUI epNameText;
    [SerializeField] private TextMeshProUGUI epStoryText;

    [Header("Buttons")]
    [SerializeField] private Button replayButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button acceptButton;

    private EpisodeData _ep;
    private StageData _stage;

    // ─────────────────────────────────────────

    public void Open(EpisodeData ep, StageData stage)
    {
        gameObject.SetActive(true);
        _ep = ep;
        _stage = stage;

        if (epThumbnail != null) epThumbnail.sprite = ep.epThumbnail;
        if (epNameText != null) epNameText.text = ep.epName;
        if (epStoryText != null) epStoryText.text = ep.storyDescription;

        // Back → กลับหน้า EP List
        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(() =>
            StoryMenuController.Instance.OpenEpisodeList(_stage));

        // Replay — TODO ภายหลัง
        replayButton.onClick.RemoveAllListeners();

        // Accept → ไปหน้าเลือกทีม (Story mode)
        acceptButton.onClick.RemoveAllListeners();
        acceptButton.onClick.AddListener(OnAccept);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────

    private void OnAccept()
    {
        if (GameMenuFlowController.Instance == null)
        {
            Debug.LogWarning("[EpisodeDetailPageUI] ไม่เจอ GameMenuFlowController ใน Scene!");
            return;
        }

        // mode = Story → Accept ในหน้าเลือกทีมจะไป StoryScene
        GameMenuFlowController.Instance.OpenTeamSelectForStory();
    }
}