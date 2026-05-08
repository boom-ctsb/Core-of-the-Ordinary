using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterPanel : MonoBehaviour
{
    [Header("UI Portrait (แบบเดียวกับ PartySlots)")]
    [SerializeField] private Image portraitImage;
    [SerializeField] private Image portraitEmptyOverlay;

    [Header("UI เลือดและเวลา")]
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private DynamicBar hpBar;

    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private DynamicBar timeBar;

    [Tooltip("กล่อง Charge ทั้ง 10 ช่อง")]
    [SerializeField] private Image[] chargeBoxes = new Image[10];

    [Header("UI สเตตัส")]
    [SerializeField] private TextMeshProUGUI atkText;
    [SerializeField] private TextMeshProUGUI defText;
    [SerializeField] private TextMeshProUGUI spdText;

    [Header("ตั้งค่าสีของ Charge Box")]
    [SerializeField] private Color chargeFullColor = Color.green;
    [SerializeField] private Color chargeEmptyColor = new Color(0.2f, 0.2f, 0.2f);

    [Header("Ultimate Cooldown (Fill Only)")]
    [SerializeField] private Image ultimateCooldownFill;

    [Tooltip("ถ้า true: ตอนพร้อมใช้งาน (คูลดาวน์หมด) ให้หลอดเต็ม 1")]
    [SerializeField] private bool showFullWhenReady = true;

    [Header("Buff Icons (Temporary Buff)")]
    [SerializeField] private Transform buffIconsAnchor;
    [SerializeField] private GameObject buffIconPrefab;
    [SerializeField] private int maxBuffIcons = 6;
    [SerializeField] private float buffIconSpacing = 28f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    public BattleCharacter character { get; private set; }

    private bool wasATBFull = false;

    private readonly List<TimedBuffInstance> trackedBuffs = new List<TimedBuffInstance>();
    private readonly List<GameObject> buffIconGOs = new List<GameObject>();
    private readonly List<StatusEffectIconUI> buffIconUIs = new List<StatusEffectIconUI>();

    private UltimateRuntime ultimateRuntime;

    public void Initialize(BattleCharacter character)
    {
        UnbindBuffEvents();

        this.character = character;
        wasATBFull = false;

        if (character == null)
        {
            RefreshPortraitLikePartySlots();
            ClearBuffIcons();
            ultimateRuntime = null;
            UpdateUltimateCooldownFill();
            return;
        }

        ultimateRuntime = character.GetComponent<UltimateRuntime>();

        RefreshPortraitLikePartySlots();
        UpdatePanel();

        BindBuffEvents();
        RebuildBuffIcons();
    }

    private void OnDisable()
    {
        UnbindBuffEvents();
    }

    private void Update()
    {
        if (character != null)
        {
            UpdatePanel();
            UpdateBuffIconsPerFrame();
            UpdateUltimateCooldownFill();
        }
    }

    public void UpdatePanel()
    {
        if (character == null || character.Stats == null) return;

        float currentHP = character.Stats.CurrentHP;
        float maxHP = character.Stats.MaxHP;

        if (hpText != null) hpText.text = $"Hp : {currentHP:F0}/{maxHP:F0}";
        if (hpBar != null) hpBar.SetFillAmount(currentHP, maxHP);

        float currentATB = character.ATBTimer;
        float maxATB = character.ATBMaxTime;

        bool isFull = character.IsATBFull;

        if (isFull && !wasATBFull)
        {
            wasATBFull = true;
            if (debugLogs) Debug.Log($"⏹️ {character.CharacterName} TimeBar เต็ม!");
        }
        else if (!isFull)
        {
            wasATBFull = false;
        }

        if (timeText != null)
        {
            int cur = Mathf.RoundToInt(currentATB);
            int max = Mathf.RoundToInt(maxATB);
            timeText.text = isFull ? $"{max}/{max}" : $"{cur}/{max}";
        }

        if (timeBar != null)
        {
            if (isFull) timeBar.SetFillAmount(maxATB, maxATB);
            else timeBar.SetFillAmount(currentATB, maxATB);
        }

        UpdateChargeBoxes();

        if (atkText != null) atkText.text = $"Atk : {character.Stats.Attack:F0}";
        if (defText != null) defText.text = $"Def : {character.Stats.Defense:F0}";
        if (spdText != null) spdText.text = $"Spd : {character.Stats.Speed:F0}";
    }

    private void UpdateChargeBoxes()
    {
        if (character == null) return;
        if (chargeBoxes == null || chargeBoxes.Length == 0) return;

        float chargePerBox = 100f / chargeBoxes.Length;
        float currentCharge = character.ChargeTimer;

        for (int i = 0; i < chargeBoxes.Length; i++)
        {
            Image box = chargeBoxes[i];
            if (box == null) continue;

            bool filled = currentCharge >= (i + 1) * chargePerBox;
            box.color = filled ? chargeFullColor : chargeEmptyColor;
        }
    }

    private void UpdateUltimateCooldownFill()
    {
        if (ultimateCooldownFill == null) return;

        if (character == null)
        {
            ultimateCooldownFill.fillAmount = 0f;
            return;
        }

        if (ultimateRuntime == null)
            ultimateRuntime = character.GetComponent<UltimateRuntime>();

        if (ultimateRuntime == null || !ultimateRuntime.HasSkill)
        {
            ultimateCooldownFill.fillAmount = 0f;
            return;
        }

        float p = ultimateRuntime.GetCooldownPercent01();

        if (showFullWhenReady && ultimateRuntime.CooldownRemaining <= 0f)
            p = 1f;

        ultimateCooldownFill.fillAmount = Mathf.Clamp01(p);
    }

    private void RefreshPortraitLikePartySlots()
    {
        bool has = character != null;

        Sprite sprite = null;
        if (has && character.CharacterData != null)
            sprite = character.CharacterData.portraitSprite;

        if (portraitEmptyOverlay != null)
            portraitEmptyOverlay.gameObject.SetActive(!has);

        if (portraitImage != null)
        {
            portraitImage.gameObject.SetActive(has);
            portraitImage.sprite = sprite;
        }
    }

    public void Highlight()
    {
        Image panelBorder = GetComponent<Image>();
        if (panelBorder != null) panelBorder.color = Color.yellow;
    }

    public void Normalize()
    {
        Image panelBorder = GetComponent<Image>();
        if (panelBorder != null) panelBorder.color = new Color(0.8f, 0.8f, 0.8f);
    }

    private void BindBuffEvents()
    {
        if (character == null) return;
        character.OnOwnedBuffIconListChanged += HandleBuffListChanged;
    }

    private void UnbindBuffEvents()
    {
        if (character == null) return;
        character.OnOwnedBuffIconListChanged -= HandleBuffListChanged;
    }

    private void HandleBuffListChanged(BattleCharacter owner)
    {
        RebuildBuffIcons();
    }

    private void RebuildBuffIcons()
    {
        ClearBuffIcons();
        if (character == null || buffIconsAnchor == null || buffIconPrefab == null) return;

        IReadOnlyList<TimedBuffInstance> buffs = character.GetOwnedBuffIconsReadOnly();
        if (buffs == null) return;

        List<TimedBuffInstance> list = new List<TimedBuffInstance>();
        for (int i = 0; i < buffs.Count; i++)
        {
            TimedBuffInstance b = buffs[i];
            if (b == null) continue;
            if (b.IsExpired) continue;
            if (b.IsDebuff) continue;
            if (b.IconSprite == null) continue;
            list.Add(b);
        }

        list.Sort((a, b) => b.TimeCreated.CompareTo(a.TimeCreated));

        int cap = Mathf.Clamp(maxBuffIcons, 0, 32);

        for (int i = 0; i < list.Count && trackedBuffs.Count < cap; i++)
        {
            TimedBuffInstance b = list[i];

            StatusEffectIconUI ui;
            GameObject go = CreateBuffIcon(b, out ui);
            if (go == null) continue;

            trackedBuffs.Add(b);
            buffIconGOs.Add(go);
            buffIconUIs.Add(ui);
        }

        LayoutBuffIcons();
        UpdateBuffIconsPerFrame();
    }

    private void ClearBuffIcons()
    {
        for (int i = 0; i < buffIconGOs.Count; i++)
            if (buffIconGOs[i] != null) Destroy(buffIconGOs[i]);

        trackedBuffs.Clear();
        buffIconGOs.Clear();
        buffIconUIs.Clear();
    }

    private void UpdateBuffIconsPerFrame()
    {
        if (trackedBuffs.Count == 0) return;

        for (int i = trackedBuffs.Count - 1; i >= 0; i--)
        {
            TimedBuffInstance b = trackedBuffs[i];
            if (b == null || b.IsExpired)
            {
                if (i >= 0 && i < buffIconGOs.Count && buffIconGOs[i] != null)
                    Destroy(buffIconGOs[i]);

                trackedBuffs.RemoveAt(i);
                buffIconGOs.RemoveAt(i);
                buffIconUIs.RemoveAt(i);

                LayoutBuffIcons();
                continue;
            }

            StatusEffectIconUI ui = buffIconUIs[i];
            if (ui != null)
                ui.SetData(b.IconSprite, b.DurationSeconds, b.TimeRemaining);
        }
    }

    private void LayoutBuffIcons()
    {
        RectTransform anchorRT = buffIconsAnchor as RectTransform;

        float startX = 0f;
        if (anchorRT != null)
            startX = -anchorRT.rect.width * anchorRT.pivot.x;

        for (int i = 0; i < buffIconGOs.Count; i++)
        {
            GameObject go = buffIconGOs[i];
            if (go == null) continue;

            float x = startX + (i * buffIconSpacing);

            RectTransform rt = go.GetComponent<RectTransform>();
            if (rt != null) rt.anchoredPosition = new Vector2(x, 0f);
            else go.transform.localPosition = new Vector3(x, 0f, 0f);
        }
    }

    private GameObject CreateBuffIcon(TimedBuffInstance buff, out StatusEffectIconUI ui)
    {
        ui = null;

        GameObject prefab = buff.IconPrefabOverride != null ? buff.IconPrefabOverride : buffIconPrefab;
        if (prefab == null)
            return null;

        GameObject go = Instantiate(prefab, buffIconsAnchor);

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

        ui = go.GetComponent<StatusEffectIconUI>();
        return go;
    }
}