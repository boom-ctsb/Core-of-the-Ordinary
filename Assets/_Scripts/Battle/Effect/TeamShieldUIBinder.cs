using UnityEngine;

public class TeamShieldUIBinder : MonoBehaviour
{
    [Header("Prefab / Anchor")]
    [SerializeField] private GameObject shieldPrefab;

    [Tooltip("จุดที่ให้ไอคอนโล่โผล่ (ถ้าไม่ใส่จะใช้ object นี้)")]
    [SerializeField] private Transform shieldAnchor;

    [Header("Optional: ใช้ UI ที่วางไว้ในซีนอยู่แล้ว (แทนการ Instantiate prefab)")]
    [SerializeField] private ShieldStatusUI shieldStatusUI;

    private GameObject instanceGO;

    private void Awake()
    {
        if (shieldAnchor == null)
            shieldAnchor = transform;

        if (shieldStatusUI == null)
            shieldStatusUI = GetComponentInChildren<ShieldStatusUI>(true);
    }

    public void SetShieldValue(float shieldValue)
    {
        if (shieldValue <= 0f)
        {
            SetVisible(false);
            return;
        }

        EnsureUI();

        if (shieldStatusUI == null)
            return;

        SetVisible(true);
        shieldStatusUI.SetValue(shieldValue);
    }

    private void EnsureUI()
    {
        if (shieldStatusUI != null) return;

        if (shieldPrefab == null)
        {
            Debug.LogError("❌ TeamShieldUIBinder: ไม่มี ShieldStatusUI และไม่ได้ใส่ shieldPrefab", this);
            return;
        }

        Transform parent = shieldAnchor != null ? shieldAnchor : transform;
        instanceGO = Instantiate(shieldPrefab, parent);

        RectTransform rt = instanceGO.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localScale = Vector3.one;
            rt.anchoredPosition = Vector2.zero;
        }
        else
        {
            instanceGO.transform.localPosition = Vector3.zero;
            instanceGO.transform.localScale = Vector3.one;
        }

        shieldStatusUI = instanceGO.GetComponent<ShieldStatusUI>();
        if (shieldStatusUI == null)
            Debug.LogWarning("⚠️ TeamShieldUIBinder: prefab ไม่มี ShieldStatusUI component", this);
    }

    private void SetVisible(bool visible)
    {
        if (shieldStatusUI != null)
            shieldStatusUI.gameObject.SetActive(visible);
    }
}