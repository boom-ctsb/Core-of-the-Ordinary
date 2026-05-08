using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyPanel : MonoBehaviour
{
    [Header("UI รูปภาพ")]
    [SerializeField] private Image portraitImage;

    [Header("UI เลือดและเวลา")]
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private DynamicBar hpBar;

    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private DynamicBar timeBar;

    [Header("UI สเตตัส")]
    [SerializeField] private TextMeshProUGUI atkText;
    [SerializeField] private TextMeshProUGUI defText;
    [SerializeField] private TextMeshProUGUI spdText;

    [Header("Charge (optional)")]
    [SerializeField] private Image[] chargeBoxes = new Image[10];
    [SerializeField] private Color chargeFullColor = Color.green;
    [SerializeField] private Color chargeEmptyColor = new Color(0.2f, 0.2f, 0.2f);

    [Header("Combo Gauge (10 hits)")]
    [SerializeField] private Image[] comboBoxes = new Image[10];
    [SerializeField] private Color comboLowColor = Color.green;
    [SerializeField] private Color comboMidColor = Color.yellow;
    [SerializeField] private Color comboHighColor = new Color(1f, 0.55f, 0f);
    [SerializeField] private Color comboMaxColor = Color.red;
    [SerializeField] private Color comboEmptyColor = new Color(0.2f, 0.2f, 0.2f);

    [Header("Debuff Icons")]
    [Tooltip("Empty UI anchor บน EnemyPanel สำหรับแสดง Debuff")]
    [SerializeField] private Transform debuffIconsAnchor;

    [Tooltip("Prefab icon (มี StatusEffectIconUI) ใช้อันเดียวกับ buff icon ได้")]
    [SerializeField] private GameObject debuffIconPrefab;

    [SerializeField] private int maxDebuffIcons = 6;
    [SerializeField] private float debuffIconSpacing = 28f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    public Dysnorma enemy { get; private set; }

    private bool wasATBFull = false;

    // runtime debuff icon cache
    private readonly List<TimedBuffInstance> trackedDebuffs = new List<TimedBuffInstance>();
    private readonly List<GameObject> debuffIconGOs = new List<GameObject>();
    private readonly List<StatusEffectIconUI> debuffIconUIs = new List<StatusEffectIconUI>();

    public void Initialize(Dysnorma enemy)
    {
        UnbindEnemyEvents();

        this.enemy = enemy;

        UpdatePortrait();
        UpdatePanel();

        BindEnemyEvents();
        RebuildDebuffIcons();
    }

    private void OnDisable()
    {
        UnbindEnemyEvents();
    }

    private void Update()
    {
        if (enemy != null)
        {
            UpdatePanel();
            UpdateDebuffIconsPerFrame();
        }
    }

    public void UpdatePanel()
    {
        if (enemy == null || enemy.Stats == null) return;

        // HP
        float currentHP = enemy.Stats.CurrentHP;
        float maxHP = enemy.Stats.MaxHP;

        if (hpText != null) hpText.text = $"HP : {currentHP:F0}/{maxHP:F0}";
        if (hpBar != null) hpBar.SetFillAmount(currentHP, maxHP);

        // ATB
        float currentATB = enemy.ATBTimer;
        float maxATB = enemy.ATBMaxTime;

        bool isFull = enemy.IsATBFull;

        if (isFull && !wasATBFull) wasATBFull = true;
        else if (!isFull) wasATBFull = false;

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

        // Charge (optional)
        UpdateChargeBoxes();

        // Combo
        UpdateComboBoxes();

        // Stats
        if (atkText != null) atkText.text = $"Atk : {enemy.Stats.Attack:F0}";
        if (defText != null) defText.text = $"Def : {enemy.Stats.Defense:F0}";
        if (spdText != null) spdText.text = $"Spd : {enemy.Stats.Speed:F0}";
    }

    private void UpdateChargeBoxes()
    {
        if (chargeBoxes == null || chargeBoxes.Length == 0) return;

        float chargePerBox = 100f / chargeBoxes.Length;
        float currentCharge = enemy.ChargeTimer;

        for (int i = 0; i < chargeBoxes.Length; i++)
        {
            Image box = chargeBoxes[i];
            if (box == null) continue;

            bool filled = currentCharge >= (i + 1) * chargePerBox;
            box.color = filled ? chargeFullColor : chargeEmptyColor;
        }
    }

    private void UpdateComboBoxes()
    {
        if (comboBoxes == null || comboBoxes.Length == 0) return;

        int max = Mathf.Max(1, enemy.ComboMax);
        int count = Mathf.Clamp(enemy.ComboCount, 0, max);

        for (int i = 0; i < comboBoxes.Length; i++)
        {
            Image box = comboBoxes[i];
            if (box == null) continue;

            bool filled = (i < count);
            if (!filled)
            {
                box.color = comboEmptyColor;
                continue;
            }

            float t = (float)(i + 1) / max;
            if (t < 0.4f) box.color = comboLowColor;
            else if (t < 0.7f) box.color = comboMidColor;
            else if (t < 0.95f) box.color = comboHighColor;
            else box.color = comboMaxColor;
        }
    }

    private void UpdatePortrait()
    {
        if (portraitImage == null) return;
        if (enemy == null) return;

        portraitImage.sprite = enemy.CharacterData != null ? enemy.CharacterData.portraitSprite : null;
    }

    // =======================
    // Enemy Debuff Icons
    // =======================
    private void BindEnemyEvents()
    {
        if (enemy == null) return;
        enemy.OnBuffListChanged += HandleEnemyBuffChanged;
    }

    private void UnbindEnemyEvents()
    {
        if (enemy == null) return;
        enemy.OnBuffListChanged -= HandleEnemyBuffChanged;
    }

    private void HandleEnemyBuffChanged(BattleCharacter owner)
    {
        RebuildDebuffIcons();
    }

    private void RebuildDebuffIcons()
    {
        ClearDebuffIcons();

        if (enemy == null) return;

        if (debuffIconsAnchor == null)
        {
            if (debugLogs) Debug.LogWarning("[EnemyPanel] debuffIconsAnchor null", this);
            return;
        }

        if (debuffIconPrefab == null)
        {
            if (debugLogs) Debug.LogWarning("[EnemyPanel] debuffIconPrefab null", this);
            return;
        }

        IReadOnlyList<TimedBuffInstance> buffs = enemy.GetActiveBuffsReadOnly();
        if (buffs == null) return;

        // เฉพาะ Debuff + มี icon + ยังไม่หมด
        List<TimedBuffInstance> list = new List<TimedBuffInstance>();
        for (int i = 0; i < buffs.Count; i++)
        {
            TimedBuffInstance b = buffs[i];
            if (b == null) continue;
            if (b.IsExpired) continue;
            if (!b.IsDebuff) continue;
            if (b.IconSprite == null) continue;

            list.Add(b);
        }

        // ใหม่สุดอยู่ซ้าย (เหมือน buff ฝั่ง ally)
        list.Sort((a, b) => b.TimeCreated.CompareTo(a.TimeCreated));

        int cap = Mathf.Clamp(maxDebuffIcons, 0, 32);
        for (int i = 0; i < list.Count && trackedDebuffs.Count < cap; i++)
        {
            TimedBuffInstance d = list[i];

            GameObject prefab = d.IconPrefabOverride != null ? d.IconPrefabOverride : debuffIconPrefab;
            if (prefab == null) continue;

            GameObject go = Instantiate(prefab, debuffIconsAnchor);

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

            trackedDebuffs.Add(d);
            debuffIconGOs.Add(go);
            debuffIconUIs.Add(go.GetComponent<StatusEffectIconUI>());
        }

        LayoutDebuffIcons();
        UpdateDebuffIconsPerFrame();
    }

    private void ClearDebuffIcons()
    {
        for (int i = 0; i < debuffIconGOs.Count; i++)
            if (debuffIconGOs[i] != null) Destroy(debuffIconGOs[i]);

        trackedDebuffs.Clear();
        debuffIconGOs.Clear();
        debuffIconUIs.Clear();
    }

    private void UpdateDebuffIconsPerFrame()
    {
        if (trackedDebuffs.Count == 0) return;

        for (int i = trackedDebuffs.Count - 1; i >= 0; i--)
        {
            TimedBuffInstance b = trackedDebuffs[i];
            if (b == null || b.IsExpired)
            {
                if (i >= 0 && i < debuffIconGOs.Count && debuffIconGOs[i] != null)
                    Destroy(debuffIconGOs[i]);

                trackedDebuffs.RemoveAt(i);
                debuffIconGOs.RemoveAt(i);
                debuffIconUIs.RemoveAt(i);

                LayoutDebuffIcons();
                continue;
            }

            StatusEffectIconUI ui = debuffIconUIs[i];
            if (ui != null)
                ui.SetData(b.IconSprite, b.DurationSeconds, b.TimeRemaining);
        }
    }
    private void LayoutDebuffIcons()
    {
        // ✅ นับจากขวาไปซ้าย:
        // index สุดท้าย (ใหม่สุด) อยู่ขวาสุด x=0
        // ตัวก่อนหน้าอยู่ซ้าย x=-spacing, -2*spacing, ...
        for (int i = 0; i < debuffIconGOs.Count; i++)
        {
            GameObject go = debuffIconGOs[i];
            if (go == null) continue;

            int fromRightIndex = (debuffIconGOs.Count - 1) - i;
            float x = -fromRightIndex * debuffIconSpacing;

            RectTransform rt = go.GetComponent<RectTransform>();
            if (rt != null) rt.anchoredPosition = new Vector2(x, 0f);
            else go.transform.localPosition = new Vector3(x, 0f, 0f);
        }
    }
}