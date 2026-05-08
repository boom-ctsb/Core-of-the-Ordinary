using UnityEngine;

public class BattleSkillUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("Slots (Empty Objects)")]
    [SerializeField] private Transform attackSlot;
    [SerializeField] private Transform skill1Slot;
    [SerializeField] private Transform skill2Slot;
    [SerializeField] private Transform ultimateSlot;

    [Header("Debug")]
    [SerializeField] private bool verboseLog = false;

    private BattleManager battleManager;
    private BattleCharacter currentCharacter;

    private GameObject attackButtonInstance;
    private GameObject skill1ButtonInstance;
    private GameObject skill2ButtonInstance;
    private GameObject ultimateButtonInstance;

    private void Awake()
    {
        if (root == null) root = gameObject;
        Hide();
    }

    public void Bind(BattleManager manager)
    {
        battleManager = manager;
    }

    public void ShowFor(BattleCharacter character)
    {
        currentCharacter = character;
        if (root != null) root.SetActive(true);
        RebuildButtons();
    }

    public void Hide()
    {
        currentCharacter = null;
        ClearButtons();
        if (root != null) root.SetActive(false);
    }

    private void RebuildButtons()
    {
        ClearButtons();

        if (currentCharacter == null) return;

        AllyCharacterData data = currentCharacter.CharacterData as AllyCharacterData;
        if (data == null)
        {
            if (verboseLog)
                Debug.LogWarning($"⚠️ BattleSkillUI: '{currentCharacter.CharacterName}' ไม่ใช่ AllyCharacterData");
            return;
        }

        attackButtonInstance = SpawnButton(data.attackButtonPrefab, attackSlot, "Attack");
        skill1ButtonInstance = SpawnButton(data.skill1ButtonPrefab, skill1Slot, "Skill1");
        skill2ButtonInstance = SpawnButton(data.skill2ButtonPrefab, skill2Slot, "Skill2");
        ultimateButtonInstance = SpawnButton(data.ultimateButtonPrefab, ultimateSlot, "Ultimate");
    }

    private GameObject SpawnButton(GameObject prefab, Transform slot, string actionId)
    {
        if (slot == null) return null;

        for (int i = slot.childCount - 1; i >= 0; i--)
            Destroy(slot.GetChild(i).gameObject);

        if (prefab == null) return null;

        GameObject go = Instantiate(prefab, slot);

        RectTransform rt = go.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localScale = Vector3.one;
            rt.anchoredPosition = Vector2.zero;
        }
        else
        {
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
        }

        ActionButton ab = go.GetComponent<ActionButton>();
        if (ab != null)
        {
            ab.SetActionId(actionId);
            ab.Bind(this);
        }

        // ✅ ถ้าเป็นปุ่ม Ultimate ให้ Bind กับตัวละครปัจจุบัน
        UltimateButtonUI ultUI = go.GetComponent<UltimateButtonUI>();
        if (ultUI != null && currentCharacter != null)
        {
            ultUI.Bind(currentCharacter);
        }

        return go;
    }

    public void OnActionButtonPressed(string actionId)
    {
        if (battleManager == null || currentCharacter == null) return;
        battleManager.OnPlayerSelectedAction(currentCharacter, actionId);
    }

    private void ClearButtons()
    {
        DestroyInstance(ref attackButtonInstance);
        DestroyInstance(ref skill1ButtonInstance);
        DestroyInstance(ref skill2ButtonInstance);
        DestroyInstance(ref ultimateButtonInstance);

        ClearSlot(attackSlot);
        ClearSlot(skill1Slot);
        ClearSlot(skill2Slot);
        ClearSlot(ultimateSlot);
    }

    private void DestroyInstance(ref GameObject go)
    {
        if (go != null) Destroy(go);
        go = null;
    }

    private void ClearSlot(Transform slot)
    {
        if (slot == null) return;
        for (int i = slot.childCount - 1; i >= 0; i--)
            Destroy(slot.GetChild(i).gameObject);
    }
}