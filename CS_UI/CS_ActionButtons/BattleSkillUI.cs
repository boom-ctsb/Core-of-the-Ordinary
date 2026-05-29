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

    [Header("Character Panel Preview Target")]
    [SerializeField] private CharacterPanel currentCharacterPanel; // ✅ ลาก panel ของตัวที่กำลัง active หรือ bind runtime

    [Header("Debug")]
    [SerializeField] private bool verboseLog = false;

    [Header("Tooltip")]
    [SerializeField] private ActionTooltipUI actionTooltipUI;

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

    // ✅ ถ้ามีระบบ bind panel runtime จะใช้ตัวนี้
    public void SetCurrentCharacterPanel(CharacterPanel panel)
    {
        currentCharacterPanel = panel;
    }

    public void ShowFor(BattleCharacter character)
    {
        currentCharacter = character;

        if (currentCharacterPanel != null)
        {
            currentCharacterPanel.Initialize(character);
            Debug.Log($"BattleSkillUI.ShowFor -> bind panel to {(character != null ? character.CharacterName : "null")}");
        }

        if (root != null) root.SetActive(true);
        RebuildButtons();
    }

    public void Hide()
    {
        OnActionButtonHoverExit("");
        currentCharacter = null;

        if (currentCharacterPanel != null)
            currentCharacterPanel.Initialize(null);

        ClearButtons();
        if (root != null) root.SetActive(false);

        if (actionTooltipUI != null)
            actionTooltipUI.Hide();
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

        // ✅ เปลี่ยนจาก GetComponent เป็น GetComponentInChildren
        ActionButton ab = go.GetComponentInChildren<ActionButton>(true);
        if (ab != null)
        {
            ab.SetActionId(actionId);
            ab.Bind(this);

            if (verboseLog)
                Debug.Log($"✅ Bind ActionButton: {actionId} on '{go.name}'");
        }
        else if (verboseLog)
        {
            Debug.LogWarning($"⚠️ SpawnButton: prefab '{prefab.name}' ไม่มี ActionButton");
        }

        UltimateButtonUI ultUI = go.GetComponentInChildren<UltimateButtonUI>(true);
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

    // ✅ Hover enter
    public void OnActionButtonHoverEnter(string actionId)
    {
        int deltaBoxes = GetChargePreviewDeltaBoxes(actionId);

        Debug.Log($"UI Hover Enter: {actionId}, tooltip={(actionTooltipUI != null)}");

        if (currentCharacter != null && currentCharacterPanel != null)
            currentCharacterPanel.SetChargePreview(deltaBoxes);

        if (actionTooltipUI != null)
        {
            string displayName = GetActionDisplayName(actionId);
            string desc = GetActionDescription(actionId);
            int costBoxes = GetActionChargeCostBoxes(actionId);

            actionTooltipUI.Show(displayName, desc, costBoxes);
        }
    }

    public void OnActionButtonHoverExit(string actionId)
    {
        Debug.Log($"UI Hover Exit: {actionId}");

        if (currentCharacterPanel != null)
            currentCharacterPanel.ClearChargePreview();

        if (actionTooltipUI != null)
            actionTooltipUI.Hide();
    }

    private int GetChargePreviewDeltaBoxes(string actionId)
    {
        if (currentCharacter == null) return 0;

        AllyCharacterData data = currentCharacter.CharacterData as AllyCharacterData;
        if (data == null) return 0;

        if (actionId == "Attack")
        {
            Debug.Log("Preview Attack => +1");
            return +1;
        }

        if (actionId == "Skill1")
        {
            SkillAction s = data.GetEquippedSkill1();
            Debug.Log($"Preview Skill1 => {(s != null ? s.skillName : "null")} cost={(s != null ? s.chargeCost : -1)}");
            return SkillChargeCostToBoxes(s);
        }

        if (actionId == "Skill2")
        {
            SkillAction s = data.GetEquippedSkill2();
            Debug.Log($"Preview Skill2 => {(s != null ? s.skillName : "null")} cost={(s != null ? s.chargeCost : -1)}");
            return SkillChargeCostToBoxes(s);
        }

        if (actionId == "Ultimate")
            return 0;

        return 0;
    }

    private int SkillChargeCostToBoxes(SkillAction skill)
    {
        if (skill == null) return 0;

        int totalBoxes = 10;
        float perBox = 100f / totalBoxes;

        int costBoxes = Mathf.CeilToInt(skill.chargeCost / perBox);

        if (costBoxes > 0)
            return -costBoxes;

        return 0;
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

    private SkillAction GetSkillByActionId(string actionId)
    {
        if (currentCharacter == null) return null;

        AllyCharacterData data = currentCharacter.CharacterData as AllyCharacterData;
        if (data == null) return null;

        if (actionId == "Skill1")
            return data.GetEquippedSkill1();

        if (actionId == "Skill2")
            return data.GetEquippedSkill2();

        return null;
    }

    private string GetActionDisplayName(string actionId)
    {
        if (actionId == "Attack")
            return "Attack";

        if (actionId == "Ultimate")
            return "Ultimate";

        SkillAction skill = GetSkillByActionId(actionId);
        if (skill != null && !string.IsNullOrWhiteSpace(skill.skillName))
            return skill.skillName;

        return actionId;
    }

    private string GetActionDescription(string actionId)
    {
        if (actionId == "Attack")
            return "โจมตีปกติใส่ศัตรู 1 เป้าหมาย และเพิ่ม Charge";

        if (actionId == "Ultimate")
            return "ใช้ท่าไม้ตาย";

        SkillAction skill = GetSkillByActionId(actionId);
        if (skill != null)
            return skill.description;

        return "";
    }

    private int GetActionChargeCostBoxes(string actionId)
    {
        if (actionId == "Attack")
            return 0;

        if (actionId == "Ultimate")
            return 0; // เอา ulti ไว้ก่อนตามที่คุยกัน

        SkillAction skill = GetSkillByActionId(actionId);
        if (skill == null) return 0;

        return Mathf.Max(0, ChargePointsToBoxes(skill.chargeCost));
    }

    private int ChargePointsToBoxes(float chargePoints)
    {
        int totalBoxes = 10;
        float perBox = 100f / totalBoxes;

        if (chargePoints <= 0f) return 0;
        return Mathf.CeilToInt(chargePoints / perBox);
    }
}