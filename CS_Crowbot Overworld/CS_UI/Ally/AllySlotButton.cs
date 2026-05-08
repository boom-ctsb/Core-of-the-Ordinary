using UnityEngine;
using UnityEngine.UI;

public class AllySlotButton : MonoBehaviour
{
    [Header("Slot")]
    public int slotIndex; // 0..7 (0-3 แนวหน้า, 4-7 แนวหลัง) 

    [Header("UI")]
    public Text allyNameText;
    public Image highlightImage;

    [Header("State")]
    public bool isSelected;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        if (_button != null)
            _button.onClick.AddListener(ToggleSelected);

        Refresh();
    }

    public void SetAllyName(string allyName)
    {
        if (allyNameText != null) allyNameText.text = allyName;
    }

    public void ToggleSelected()
    {
        isSelected = !isSelected;
        Refresh();
    }

    private void Refresh()
    {
        if (highlightImage != null)
            highlightImage.enabled = isSelected;
    }

}