using UnityEngine;
using UnityEngine.UI;

public class TestAttackButton : MonoBehaviour
{
    [SerializeField] private Button attackButton;
    [SerializeField] private BattleManager battleManager;

    private void Awake()
    {
        if (attackButton == null)
            attackButton = GetComponent<Button>();

        if (battleManager == null)
            battleManager = FindObjectOfType<BattleManager>();
    }

    private void Start()
    {
        if (attackButton != null)
            attackButton.onClick.AddListener(OnAttackButtonClicked);
    }

    public void OnAttackButtonClicked()
    {
        if (battleManager == null)
        {
            Debug.LogError("❌ TestAttackButton: ไม่พบ BattleManager");
            return;
        }
    }
}