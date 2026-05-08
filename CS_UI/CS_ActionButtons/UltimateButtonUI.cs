using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UltimateButtonUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Button ultimateButton;
    [SerializeField] private Image buttonImage;
    [SerializeField] private TextMeshProUGUI cooldownText;

    [Header("Tier Gauge (3 ช่อง)")]
    [SerializeField] private Transform tierGaugeRoot;
    [SerializeField] private Image[] tierGaugeImages = new Image[3];

    [Header("Colors")]
    [SerializeField] private Color readyColor = Color.white;
    [SerializeField] private Color notReadyColor = new Color(0.35f, 0.35f, 0.35f, 1f);

    [Header("Text")]
    [SerializeField] private bool hideTextWhenReady = true;
    [SerializeField] private bool showSecondsRemaining = true;

    [Header("Gauge Visibility")]
    [SerializeField] private bool hideGaugeWhenTierIsZero = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private BattleCharacter owner;
    private UltimateRuntime runtime;

    private Color buttonBaseColor = Color.white;
    private bool hasBaseColor = false;

    private void Awake()
    {
        AutoCollectTierImagesIfNeeded();
        ForceHideAllTierImages();
        CacheButtonBaseColor();
    }

    private void OnEnable()
    {
        ForceHideAllTierImages();
        CacheButtonBaseColor();
    }

    private void CacheButtonBaseColor()
    {
        if (buttonImage == null && ultimateButton != null)
            buttonImage = ultimateButton.GetComponent<Image>();

        if (buttonImage != null && !hasBaseColor)
        {
            buttonBaseColor = buttonImage.color;
            hasBaseColor = true;
        }
    }

    public void Bind(BattleCharacter ownerCharacter)
    {
        Unbind();

        owner = ownerCharacter;

        if (ultimateButton == null)
            ultimateButton = GetComponent<Button>();

        if (buttonImage == null && ultimateButton != null)
            buttonImage = ultimateButton.GetComponent<Image>();

        CacheButtonBaseColor();
        AutoCollectTierImagesIfNeeded();

        if (owner == null)
        {
            ForceHideAllTierImages();
            ApplyState(isCooldown: true, text: "");
            if (ultimateButton != null) ultimateButton.interactable = false;
            return;
        }

        runtime = owner.GetComponent<UltimateRuntime>();
        if (runtime == null)
        {
            Debug.LogWarning($"⚠️ UltimateButtonUI: '{owner.CharacterName}' ไม่มี UltimateRuntime", this);
            ForceHideAllTierImages();
            ApplyState(isCooldown: true, text: "—");
            if (ultimateButton != null) ultimateButton.interactable = false;
            return;
        }

        runtime.OnChanged += HandleChanged;

        if (ultimateButton != null)
        {
            ultimateButton.onClick.RemoveListener(OnPressed);
            ultimateButton.onClick.AddListener(OnPressed);
        }

        if (debugLogs)
            Debug.Log($"[UltimateButtonUI] Bind owner={owner.CharacterName}", this);

        Refresh();
    }

    public void Unbind()
    {
        if (runtime != null)
            runtime.OnChanged -= HandleChanged;

        runtime = null;
        owner = null;
    }

    private void OnDisable()
    {
        Unbind();
    }

    private void Update()
    {
        if (runtime != null && runtime.HasSkill)
            Refresh();
    }

    private void HandleChanged(UltimateRuntime rt)
    {
        Refresh();
    }

    private void OnPressed()
    {
        if (runtime == null) return;

        string reason;
        if (!runtime.CanUse(out reason))
        {
            if (debugLogs) Debug.Log($"Ultimate not ready: {reason}", this);
            Refresh();
            return;
        }

        runtime.StartCooldown();
        runtime.ConsumeChargeByCurrentTier();

        if (debugLogs) Debug.Log("Ultimate used (demo)", this);
        Refresh();
    }

    public void Refresh()
    {
        if (runtime == null || !runtime.HasSkill)
        {
            ForceHideAllTierImages();
            ApplyState(isCooldown: true, text: "—");
            if (ultimateButton != null) ultimateButton.interactable = false;
            return;
        }

        int tier = runtime.Tier;
        UpdateTierImages(tier);

        string reason;
        bool canUse = runtime.CanUse(out reason);

        if (ultimateButton != null)
            ultimateButton.interactable = canUse;

        // ✅ คูลดาวน์เช็คเสมอ (ไม่ดู costType แล้ว)
        bool isCooldown = runtime.CooldownRemaining > 0f;

        string text = "";
        if (!canUse && isCooldown && showSecondsRemaining)
        {
            int sec = Mathf.CeilToInt(runtime.CooldownRemaining);
            text = sec > 0 ? sec.ToString() : "0";
        }
        else if (!hideTextWhenReady)
        {
            text = "";
        }

        ApplyState(isCooldown: isCooldown, text: text);
    }

    private void AutoCollectTierImagesIfNeeded()
    {
        if (tierGaugeImages != null && tierGaugeImages.Length > 0 && tierGaugeImages[0] != null)
            return;

        if (tierGaugeRoot == null) return;

        List<Image> list = new List<Image>();
        tierGaugeRoot.GetComponentsInChildren(true, list);
        tierGaugeImages = list.ToArray();
    }

    private void ForceHideAllTierImages()
    {
        if (tierGaugeImages == null) return;
        for (int i = 0; i < tierGaugeImages.Length; i++)
            if (tierGaugeImages[i] != null) tierGaugeImages[i].gameObject.SetActive(false);
    }

    private void UpdateTierImages(int tier)
    {
        if (tierGaugeImages == null || tierGaugeImages.Length == 0) return;

        int t = Mathf.Clamp(tier, 0, tierGaugeImages.Length);

        if (hideGaugeWhenTierIsZero && t <= 0)
        {
            ForceHideAllTierImages();
            return;
        }

        for (int i = 0; i < tierGaugeImages.Length; i++)
        {
            Image img = tierGaugeImages[i];
            if (img == null) continue;

            bool show = i < t;
            img.gameObject.SetActive(show);
        }
    }

    private void ApplyState(bool isCooldown, string text)
    {
        if (buttonImage != null)
        {
            Color baseCol = hasBaseColor ? buttonBaseColor : buttonImage.color;
            Color mul = isCooldown ? notReadyColor : readyColor;

            buttonImage.color = new Color(
                baseCol.r * mul.r,
                baseCol.g * mul.g,
                baseCol.b * mul.b,
                baseCol.a * mul.a
            );
        }

        if (cooldownText != null)
        {
            cooldownText.text = text;
            cooldownText.gameObject.SetActive(!(hideTextWhenReady && !isCooldown));
        }
    }
}