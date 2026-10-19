using TMPro;
using UnityEngine;

public class ActionTooltipUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI actionNameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI chargeCostText;

    [Header("Follow Mouse")]
    [SerializeField] private bool followMouse = true;
    [SerializeField] private Vector2 mouseOffset = new Vector2(24f, -24f);

    [Header("Debug")]
    [SerializeField] private bool verboseLog = true;

    private RectTransform rootRectTransform;

    private void Awake()
    {
        if (root == null)
            root = gameObject;

        rootRectTransform = root.GetComponent<RectTransform>();

        // ✅ Auto-find TMP fields ถ้าไม่ได้ assign ใน Inspector
        TryAutoFindTextRefs();

        // ✅ Log สถานะ ref ทุกตัวตอน Awake เพื่อ debug
        if (verboseLog)
        {
            Debug.Log($"[ActionTooltipUI] Awake — root={root?.name}, " +
                      $"actionNameText={actionNameText?.name ?? "NULL"}, " +
                      $"descriptionText={descriptionText?.name ?? "NULL"}, " +
                      $"chargeCostText={chargeCostText?.name ?? "NULL"}");
        }

        Hide();
    }

    /// <summary>
    /// ถ้า TMP fields ไม่ถูก assign ใน Inspector จะพยายามหาจาก children อัตโนมัติ
    /// (ค้นหาตามชื่อ object ก่อน ถ้าไม่เจอ ค้นลำดับ 0,1,2)
    /// </summary>
    private void TryAutoFindTextRefs()
    {
        TextMeshProUGUI[] allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);

        if (verboseLog)
            Debug.Log($"[ActionTooltipUI] TryAutoFind — found {allTexts.Length} TMP children");

        foreach (var t in allTexts)
        {
            string n = t.gameObject.name.ToLower();

            if (actionNameText == null && (n.Contains("name") || n.Contains("title") || n.Contains("action")))
            {
                actionNameText = t;
                if (verboseLog) Debug.Log($"[ActionTooltipUI] Auto-assigned actionNameText = '{t.gameObject.name}'");
            }
            else if (descriptionText == null && (n.Contains("desc") || n.Contains("detail") || n.Contains("info")))
            {
                descriptionText = t;
                if (verboseLog) Debug.Log($"[ActionTooltipUI] Auto-assigned descriptionText = '{t.gameObject.name}'");
            }
            else if (chargeCostText == null && (n.Contains("charge") || n.Contains("cost") || n.Contains("mp") || n.Contains("sp")))
            {
                chargeCostText = t;
                if (verboseLog) Debug.Log($"[ActionTooltipUI] Auto-assigned chargeCostText = '{t.gameObject.name}'");
            }
        }

        // ✅ ถ้าหาตามชื่อไม่เจอ fallback ตามลำดับ index
        if (allTexts.Length >= 1 && actionNameText == null)
        {
            actionNameText = allTexts[0];
            if (verboseLog) Debug.Log($"[ActionTooltipUI] Fallback actionNameText[0] = '{allTexts[0].gameObject.name}'");
        }
        if (allTexts.Length >= 2 && descriptionText == null)
        {
            descriptionText = allTexts[1];
            if (verboseLog) Debug.Log($"[ActionTooltipUI] Fallback descriptionText[1] = '{allTexts[1].gameObject.name}'");
        }
        if (allTexts.Length >= 3 && chargeCostText == null)
        {
            chargeCostText = allTexts[2];
            if (verboseLog) Debug.Log($"[ActionTooltipUI] Fallback chargeCostText[2] = '{allTexts[2].gameObject.name}'");
        }
    }

    private void Update()
    {
        if (!followMouse) return;
        if (root == null) return;
        if (!root.activeSelf) return;
        if (rootRectTransform == null) return;

        rootRectTransform.position = (Vector2)Input.mousePosition + mouseOffset;
    }

    public void Show(string actionName, string description, int chargeCostBoxes)
    {
        if (verboseLog)
            Debug.Log($"[ActionTooltipUI] Show called: name='{actionName}' | desc='{description}' | cost={chargeCostBoxes} " +
                      $"| nameField={actionNameText?.name ?? "NULL"} | descField={descriptionText?.name ?? "NULL"} | costField={chargeCostText?.name ?? "NULL"}");

        if (root == null) root = gameObject;

        if (rootRectTransform == null)
            rootRectTransform = root.GetComponent<RectTransform>();

        // ✅ ลอง auto-find อีกครั้งถ้า field ยัง null (กรณี Awake ไม่ทัน)
        if (actionNameText == null || descriptionText == null || chargeCostText == null)
            TryAutoFindTextRefs();

        if (actionNameText != null)
            actionNameText.text = string.IsNullOrWhiteSpace(actionName) ? "-" : actionName;
        else
            Debug.LogWarning("[ActionTooltipUI] ⚠️ actionNameText is NULL — ไม่สามารถแสดงชื่อได้");

        if (descriptionText != null)
            descriptionText.text = string.IsNullOrWhiteSpace(description) ? "-" : description;
        else
            Debug.LogWarning("[ActionTooltipUI] ⚠️ descriptionText is NULL — ไม่สามารถแสดง desc ได้");

        if (chargeCostText != null)
            chargeCostText.text = $"Charge Cost : {Mathf.Max(0, chargeCostBoxes)}";
        else
            Debug.LogWarning("[ActionTooltipUI] ⚠️ chargeCostText is NULL — ไม่สามารถแสดง cost ได้");

        root.SetActive(true);

        if (followMouse && rootRectTransform != null)
            rootRectTransform.position = (Vector2)Input.mousePosition + mouseOffset;
    }

    public void Hide()
    {
        if (verboseLog)
            Debug.Log("[ActionTooltipUI] Hide");

        if (root == null) root = gameObject;

        root.SetActive(false);
    }
}