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

    public void Awake()
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

    private void Refresh()
    {
        bool has = _data != null;

        // blank overlay
        if (emptyOverlay != null)
            emptyOverlay.enabled = !has;

        // portrait
        if (portraitImage != null)
        {
            portraitImage.sprite = has ? _data.portraitSprite : null;
            portraitImage.enabled = has; // << สำคัญ: เปิดเมื่อมี data (อย่าผูกกับ sprite != null)
        }

        // Debug ช่วยจับ “รูปหาย”
        if (has && _data.portraitSprite == null)
        {
            Debug.LogWarning($"PartySlot: '{_data.characterName}' has NO portraitSprite assigned.", this);
        }
    }

    private void OnClick()
    {
        if (AllySelectUIController.Instance == null) return;
        if (_data == null) return; // ช่องว่างไม่ต้องทำอะไร

        AllySelectUIController.Instance.RemoveFromPartyByData(_data);
    }
}