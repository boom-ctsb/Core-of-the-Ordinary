using UnityEngine;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour
{
    [Header("Action")]
    [SerializeField] private string actionId = "Attack";

    [Header("Debug")]
    [SerializeField] private bool verboseLog = false;

    private Button button;
    private BattleSkillUI ownerUI;

    private void Awake()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("❌ ActionButton: GameObject นี้ไม่มี Button component");
            return;
        }
    }

    public void Bind(BattleSkillUI ui)
    {
        ownerUI = ui;

        if (button == null) return;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);

        if (verboseLog)
            Debug.Log($"[ActionButton] Bind ownerUI={(ownerUI != null ? ownerUI.name : "null")} actionId={actionId}");
    }

    public void SetActionId(string id)
    {
        actionId = id;

        if (verboseLog)
            Debug.Log($"[ActionButton] SetActionId={actionId}");
    }

    private void OnClick()
    {
        if (verboseLog)
            Debug.Log($"[ActionButton] Click actionId={actionId}");

        if (ownerUI == null)
        {
            Debug.LogError("❌ ActionButton: ownerUI เป็น null (ยังไม่ได้ Bind)");
            return;
        }

        ownerUI.OnActionButtonPressed(actionId);
    }
}