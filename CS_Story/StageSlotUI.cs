using UnityEngine;
using UnityEngine.UI;

public class StageSlotUI : MonoBehaviour
{
    [Header("ผูกกับ Image ใน Empty Object นี้")]
    [SerializeField] private Image thumbnailImage;   // รูปปก
    [SerializeField] private Image nameImage;        // รูปชื่อ Stage

    private StageData _data;

    private void Awake()
    {
        var btn = GetComponent<Button>();
        if (btn != null) btn.onClick.AddListener(OnClick);
    }

    public void Setup(StageData data)
    {
        _data = data;
        if (thumbnailImage != null) thumbnailImage.sprite = data.stageThumbnail;
        if (nameImage      != null) nameImage.sprite      = data.stageNameImage;
    }

    private void OnClick()
    {
        if (_data == null) return;
        StoryMenuController.Instance.OpenEpisodeList(_data);
    }
}