using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class AllyRosterSlotUI : MonoBehaviour
{
    [Header("UI (child Image in this Button)")]
    [SerializeField] private Image portraitImage;
    [SerializeField] private Text nameText; // optional

    [Header("Dim when selected (portrait only)")]
    [Range(0f, 1f)][SerializeField] private float normalAlpha = 1f;
    [Range(0f, 1f)][SerializeField] private float selectedAlpha = 0.35f;

    [Header("Behavior")]
    [SerializeField] private bool disableButtonWhenSelected = true;

    private Button _button;
    private BaseCharacterData _data;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClick);
        SetSelected(false);
    }

    public void Bind(BaseCharacterData data)
    {
        _data = data;

        if (data == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        if (portraitImage != null)
        {
            portraitImage.sprite = data.portraitSprite;
            portraitImage.enabled = data.portraitSprite != null;
        }

        if (nameText != null)
            nameText.text = data.characterName;

        SetSelected(false);
    }

    public BaseCharacterData GetData() => _data;

    public void SetSelected(bool selected)
    {
        if (portraitImage != null)
        {
            var c = portraitImage.color;
            c.a = selected ? selectedAlpha : normalAlpha;
            portraitImage.color = c;
        }

        if (disableButtonWhenSelected && _button != null)
            _button.interactable = !selected;
    }

    private void OnClick()
    {
        if (_data == null) return;
        AllySelectUIController.Instance?.OnRosterClicked(_data);
    }
}