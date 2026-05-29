using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ActionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Button button;
    [SerializeField] private string actionId;

    private BattleSkillUI ownerUI;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
            button.onClick.AddListener(HandleClick);
    }

    public void Bind(BattleSkillUI ui)
    {
        ownerUI = ui;
    }

    public void SetActionId(string id)
    {
        actionId = id;
    }

    private void HandleClick()
    {
        if (ownerUI != null)
            ownerUI.OnActionButtonPressed(actionId);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"Hover Enter: {actionId}");
        if (ownerUI != null)
            ownerUI.OnActionButtonHoverEnter(actionId);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"Hover Exit: {actionId}");
        if (ownerUI != null)
            ownerUI.OnActionButtonHoverExit(actionId);
    }
}