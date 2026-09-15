using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EpisodeSlotUI : MonoBehaviour
{
    [SerializeField] private Image thumbnailImage;
    [SerializeField] private TextMeshProUGUI epNameText;

    private EpisodeData _data;
    private StageData _stage;

    private void Awake()
    {
        var btn = GetComponent<Button>();
        if (btn != null) btn.onClick.AddListener(OnClick);
    }

    public void Setup(EpisodeData data, StageData stage)
    {
        _data = data;
        _stage = stage;
        if (thumbnailImage != null) thumbnailImage.sprite = data.epThumbnail;
        if (epNameText != null) epNameText.text = data.epName;
    }

    private void OnClick()
    {
        if (_data == null) return;
        StoryMenuController.Instance.OpenEpisodeDetail(_data, _stage);
    }
}