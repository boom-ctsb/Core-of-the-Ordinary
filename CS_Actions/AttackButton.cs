using UnityEngine;
using UnityEngine.UI;

/// ปุ่ม Attack - คลิกแล้วเรียก BattleStateMachine
public class AttackButton : MonoBehaviour
{
    [SerializeField] private Button attackButton;
    private BattleStateMachine battleStateMachine;

    private void Awake()
    {
        if (attackButton == null)
            attackButton = GetComponent<Button>();

        battleStateMachine = FindObjectOfType<BattleStateMachine>();
    }

    private void Start()
    {
        // ลิงก์ปุ่ม
        if (attackButton != null)
            attackButton.onClick.AddListener(OnAttackButtonClicked);
    }

    private void OnAttackButtonClicked()
    {
        Debug.Log("🔴 Attack Button Clicked!");

        if (battleStateMachine != null);
            battleStateMachine.OnAttackButtonPressed();         
    }
}