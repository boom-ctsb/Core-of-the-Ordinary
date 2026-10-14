using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BacklinePartySlotUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image portraitImage;
    [SerializeField] private Image emptyOverlay;
    [SerializeField] private TextMeshProUGUI nameText;

    private AllyBacklineData _data;
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClick);
        Refresh();
    }

    public void SetData(AllyBacklineData data)
    {
        _data = data;
        Refresh();
    }

    public AllyBacklineData GetData() => _data;

    private void Refresh()
    {
        bool has = _data != null;

        if (emptyOverlay != null) emptyOverlay.enabled = !has;
        if (nameText != null) nameText.text = has ? _data.characterName : "";

        if (portraitImage != null)
        {
            portraitImage.sprite = has ? _data.portraitSprite : null;
            portraitImage.enabled = has;
        }
    }

    private void OnClick()
    {
        if (AllySelectUIController.Instance == null) return;
        if (_data == null) return;
        AllySelectUIController.Instance.RemoveBacklineFromPartyByData(_data);
    }
}