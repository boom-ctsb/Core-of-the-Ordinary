using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class BacklineRosterSlotUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image portraitImage;
    [SerializeField] private TextMeshProUGUI nameText;      // ← เปลี่ยนจาก Text เป็น TMP

    [Header("Dim when selected")]
    [Range(0f, 1f)][SerializeField] private float normalAlpha = 1f;
    [Range(0f, 1f)][SerializeField] private float selectedAlpha = 0.35f;

    [Header("Behavior")]
    [SerializeField] private bool disableButtonWhenSelected = true;

    private Button _button;
    private AllyBacklineData _data;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClick);
        SetSelected(false);
    }

    public void Bind(AllyBacklineData data)
    {
        _data = data;

        if (data == null) { gameObject.SetActive(false); return; }

        gameObject.SetActive(true);

        if (portraitImage != null)
        {
            portraitImage.sprite = data.portraitSprite;
            portraitImage.enabled = data.portraitSprite != null;
        }

        if (nameText != null) nameText.text = data.characterName;

        SetSelected(false);
    }

    public AllyBacklineData GetData() => _data;

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
        AllySelectUIController.Instance?.OnBacklineRosterClicked(_data);
    }
}