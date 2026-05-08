using UnityEngine;
using UnityEngine.UI;

public enum BattleState
{
    Wait,
    Command,
    Action
}

public class BattleStateMachine : MonoBehaviour
{
    public BattleState CurrentState { get; private set; }

    [SerializeField] private BattleManager battleManager;
    [SerializeField] private SimpleCameraController cameraController;

    [SerializeField] private GameObject actionMenuPanel;
    [SerializeField] private Button attackButton;
    [SerializeField] private UltimateButtonUI ultimateButtonUI;

    private BattleCharacter activeCharacter;

    private void Awake()
    {
        CurrentState = BattleState.Wait;

        if (actionMenuPanel != null)
        {
            actionMenuPanel.SetActive(false);
        }

        if (attackButton != null)
        {
            attackButton.onClick.AddListener(OnAttackButtonPressed);
        }
    }

    public void OnCharacterATBFull(BattleCharacter character)
    {
        if (CurrentState != BattleState.Wait)
        {
            return;
        }

        activeCharacter = character;
        CurrentState = BattleState.Command;

        //battleManager.PauseAllTimeBars();
        cameraController.FocusCharacter(character);

        if (character is Dysnorma)
        {
            ExecuteEnemyTurn();
        }
        else
        {
            ShowActionMenu();
        }
        ultimateButtonUI.Bind(character);
    }

    private void ShowActionMenu()
    {
        if (actionMenuPanel != null)
        {
            actionMenuPanel.SetActive(true);
        }
    }

    public void OnAttackButtonPressed()
    {
        if (actionMenuPanel != null)
        {
            actionMenuPanel.SetActive(false);
        }

        CurrentState = BattleState.Action;

        EndTurn();
    }

    private void ExecuteEnemyTurn()
    {
        CurrentState = BattleState.Action;

        EndTurn();
    }

    public void EndTurn()
    {
        if (activeCharacter != null)
        {
            activeCharacter.ResetATB();

            CharacterPanel panel = FindPanelForCharacter(activeCharacter);
            if (panel != null)
            {
                panel.UpdatePanel();
            }
        }

        activeCharacter = null;
        CurrentState = BattleState.Wait;

        cameraController.ToIdle();
        //battleManager.ResumeAllTimeBars();
    }
    private CharacterPanel FindPanelForCharacter(BattleCharacter character)
    {
        CharacterPanel[] allPanels = FindObjectsOfType<CharacterPanel>();
        foreach (CharacterPanel panel in allPanels)
        {
            if (panel.character == character)
            {
                return panel;
            }
        }
        return null;
    }
}