using UnityEngine;
using UnityEngine.UI;

public class MissionSlotButton : MonoBehaviour
{
    [Header("Slot Data")]
    public int missionIndex;

    [Header("Optional UI")]
    public Text labelText;
    public string label;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        if (_button != null)
            _button.onClick.AddListener(OnClick);

        if (labelText != null && !string.IsNullOrEmpty(label))
            labelText.text = label;
    }

    private void OnClick()
    {
        if (GameMenuFlowController.Instance == null) return;
        GameMenuFlowController.Instance.SetSelectedMission(missionIndex);
    }
}