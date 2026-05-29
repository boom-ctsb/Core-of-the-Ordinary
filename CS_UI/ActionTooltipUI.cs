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
        // ✅ ถ้าไม่ได้ assign root ให้ใช้ gameObject นี้เลย
        if (root == null)
            root = gameObject;

        rootRectTransform = root.GetComponent<RectTransform>();

        Hide();
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
            Debug.Log($"[ActionTooltipUI] Show: name={actionName}, cost={chargeCostBoxes}");

        if (root == null)
            root = gameObject;

        if (rootRectTransform == null)
            rootRectTransform = root.GetComponent<RectTransform>();

        if (actionNameText != null)
            actionNameText.text = string.IsNullOrWhiteSpace(actionName) ? "-" : actionName;

        if (descriptionText != null)
            descriptionText.text = string.IsNullOrWhiteSpace(description) ? "-" : description;

        if (chargeCostText != null)
            chargeCostText.text = $"Charge Cost : {Mathf.Max(0, chargeCostBoxes)}";

        // ✅ set text ก่อน แล้วค่อย show
        root.SetActive(true);

        if (followMouse && rootRectTransform != null)
            rootRectTransform.position = (Vector2)Input.mousePosition + mouseOffset;
    }

    public void Hide()
    {
        if (verboseLog)
            Debug.Log("[ActionTooltipUI] Hide");

        if (root == null)
            root = gameObject;

        root.SetActive(false);
    }
}