using UnityEngine;

/// <summary>
/// ควบคุมการเปิด/ปิด Menu
/// Tab = เปิด/ปิด — ส่งต่อทุกอย่างให้ GameMenuFlowController จัดการ
/// </summary>
public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [Header("Input")]
    [SerializeField] private KeyCode menuKey = KeyCode.Tab;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        if (GameMenuFlowController.Instance != null)
            return;

        if (DialogueManager.Instance != null && DialogueManager.Instance.IsPlaying) return;

        if (Input.GetKeyDown(menuKey))
            Toggle();
    }

    public void Toggle()
    {
        if (GameMenuFlowController.Instance == null) return;

        if (GameMenuFlowController.Instance.IsOpen)
            GameMenuFlowController.Instance.CloseMenu();
        else
            GameMenuFlowController.Instance.OpenMenu();
    }
}