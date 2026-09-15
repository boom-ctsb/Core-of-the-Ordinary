using UnityEngine;

/// <summary>
/// ติดกับปุ่มเปิด Menu ใน Overworld หรือใช้กด Key
/// ส่งต่อให้ GameMenuFlowController โดยตรง
/// </summary>
public class MenuOpenButton : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode menuKey = KeyCode.M;

    private void Update()
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying) return;

        if (Input.GetKeyDown(menuKey))
            ToggleMenu();
    }

    public void ToggleMenu()
    {
        if (GameMenuFlowController.Instance == null) return;

        if (GameMenuFlowController.Instance.IsOpen)
            GameMenuFlowController.Instance.CloseMenu();
        else
            GameMenuFlowController.Instance.OpenMenu();
    }
}