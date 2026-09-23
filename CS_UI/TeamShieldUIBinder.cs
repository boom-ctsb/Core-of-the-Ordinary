using UnityEngine;

public class TeamShieldUIBinder : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;
    [SerializeField] private ShieldStatusUI shieldStatusUI;

    private void Awake()
    {
        if (root == null)
            root = gameObject;

        if (shieldStatusUI == null)
            shieldStatusUI = GetComponentInChildren<ShieldStatusUI>(true);
    }

    public void SetShieldValue(float shieldValue)
    {
        bool hasShield = shieldValue > 0f;

        if (root != null)
            root.SetActive(hasShield);

        if (hasShield && shieldStatusUI != null)
            shieldStatusUI.SetValue(shieldValue);
    }
}

