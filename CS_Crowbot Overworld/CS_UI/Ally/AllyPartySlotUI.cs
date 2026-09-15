using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class AllyPartySlotUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image portraitImage;
    [SerializeField] private Image emptyOverlay;

    private BaseCharacterData _data;
    private Button _button;

    private void Awake()   // ← เพิ่ม private
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnClick);
        Refresh();
    }

    public void SetData(BaseCharacterData data)
    {
        _data = data;
        Refresh();
    }

    public BaseCharacterData GetData() => _data;

    // ← เปลี่ยนชื่อจาก Refresh เพื่อความชัดเจน + เรียกตรงได้
    private void Refresh()
    {
        bool has = _data != null;

        if (emptyOverlay != null)
            emptyOverlay.enabled = !has;

        if (portraitImage != null)
        {
            portraitImage.sprite = has ? _data.portraitSprite : null;
            portraitImage.enabled = has;
        }

        if (has && _data.portraitSprite == null)
            Debug.LogWarning($"PartySlot: '{_data.characterName}' has NO portraitSprite assigned.", this);
    }

    private void OnClick()
    {
        if (AllySelectUIController.Instance == null) return;
        if (_data == null) return;
        AllySelectUIController.Instance.RemoveFromPartyByData(_data);
    }
}