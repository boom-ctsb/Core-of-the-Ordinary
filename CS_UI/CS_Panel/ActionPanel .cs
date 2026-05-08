using UnityEngine;
using UnityEngine.UI;

public class ActionPanel : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button attackButton;

    public void ShowPanel()
    {
        canvasGroup.alpha = 1f;
        attackButton.interactable = true;
    }

    public void HidePanel()
    {
        canvasGroup.alpha = 0f;
        attackButton.interactable = false;
    }
}