using UnityEngine;

public class ShieldUIBinder : MonoBehaviour
{
    [Header("Prefab / Anchor")]
    [SerializeField] private GameObject shieldPrefab;

    [Tooltip("จุดที่ให้ prefab โผล่ (ใช้ empty object นี้ได้เลย)")]
    [SerializeField] private Transform shieldAnchor;

    private BattleCharacter boundCharacter;

    private GameObject instanceGO;
    private ShieldStatusUI instanceUI;

    private void Awake()
    {
        if (shieldAnchor == null)
            shieldAnchor = transform;
    }

    private void OnDisable()
    {
        Unbind();
    }

    public void Bind(BattleCharacter character)
    {
        Unbind();

        boundCharacter = character;
        if (boundCharacter == null) return;

        boundCharacter.OnShieldChanged += HandleShieldChanged;

        // sync ทันที
        HandleShieldChanged(boundCharacter, boundCharacter.ShieldHP);
    }

    public void Unbind()
    {
        if (boundCharacter != null)
            boundCharacter.OnShieldChanged -= HandleShieldChanged;

        boundCharacter = null;

        if (instanceGO != null)
            Destroy(instanceGO);

        instanceGO = null;
        instanceUI = null;
    }

    private void HandleShieldChanged(BattleCharacter character, float shieldHP)
    {
        if (shieldHP <= 0f)
        {
            if (instanceGO != null)
            {
                Destroy(instanceGO);
                instanceGO = null;
                instanceUI = null;
            }
            return;
        }

        if (instanceGO == null)
        {
            if (shieldPrefab == null)
            {
                Debug.LogError("❌ ShieldUIBinder: shieldPrefab is null", this);
                return;
            }

            Transform parent = (shieldAnchor != null) ? shieldAnchor : transform;
            instanceGO = Instantiate(shieldPrefab, parent);

            // reset transform
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

            instanceUI = instanceGO.GetComponent<ShieldStatusUI>();
            if (instanceUI == null)
                Debug.LogWarning("⚠️ ShieldUIBinder: prefab ไม่มี ShieldStatusUI component", this);
        }

        if (instanceUI != null)
            instanceUI.SetValue(shieldHP);
    }
}