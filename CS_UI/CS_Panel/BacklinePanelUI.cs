using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BacklinePanelUI : MonoBehaviour
{
    [Header("Portrait")]
    [SerializeField] private Image portraitImage;
    [SerializeField] private Image portraitEmptyOverlay;

    [Header("Trigger Icon Display")]
    [SerializeField] private Image triggerIconImage;

    [Header("Timebar")]
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private DynamicBar timeBar;

    [Header("Condition Icon (Radial Fill)")]
    [SerializeField] private StatusEffectIconUI conditionIconUI;
    [SerializeField] private GameObject conditionIconRoot;

    [Header("Buff Icons")]
    [SerializeField] private Transform buffIconsAnchor;
    [SerializeField] private GameObject buffIconPrefab;
    [SerializeField] private int maxBuffIcons = 6;
    [SerializeField] private float buffIconSpacing = 28f;

    // ─── runtime ───
    private BacklinePassiveUnit unit;

    private Sprite cachedPortrait = null;
    private Sprite cachedIcon = null;
    private bool isDirty = true;

    // dirty-check cache
    private float lastTimebar = -1f;
    private bool lastFilling = false;
    private float lastFillRemaining = -1f;
    private float lastTickProgress = -1f;

    // buff icon lists
    private readonly List<TimedBuffInstance> trackedBuffs = new List<TimedBuffInstance>();
    private readonly List<GameObject> buffIconGOs = new List<GameObject>();
    private readonly List<StatusEffectIconUI> buffIconUIs = new List<StatusEffectIconUI>();

    // ─────────────────────────────────────
    public void Bind(BacklinePassiveUnit u)
    {
        // unsubscribe เดิม
        if (unit != null)
            unit.OnOwnedBuffIconListChanged -= HandleBuffListChanged;

        unit = u;
        isDirty = true;

        if (unit != null)
            unit.OnOwnedBuffIconListChanged += HandleBuffListChanged;

        // portrait
        Sprite p = unit != null ? unit.PortraitSprite : null;
        if (p != cachedPortrait)
        {
            cachedPortrait = p;
            SetPortrait(p);
        }

        // trigger icon
        Sprite icon = unit != null ? unit.GetConditionIcon() : null;
        if (icon != cachedIcon)
        {
            cachedIcon = icon;
            SetTriggerIconImage(icon);
        }

        if (conditionIconUI != null)
            conditionIconUI.SetData(cachedIcon, 1f, 1f);

        RebuildBuffIcons();
        Refresh();
    }

    private void OnDestroy()
    {
        if (unit != null)
            unit.OnOwnedBuffIconListChanged -= HandleBuffListChanged;
    }

    // ─────────────────────────────────────
    private void Update()
    {
        if (unit == null) return;

        float curTimebar = unit.Timebar;
        bool curFilling = unit.IsFillWindowActive;
        float curRemaining = unit.FillWindowRemaining;
        float curTickProgress = unit.PassiveTickProgress01;

        bool changed = isDirty
            || !Mathf.Approximately(curTimebar, lastTimebar)
            || curFilling != lastFilling
            || !Mathf.Approximately(curRemaining, lastFillRemaining)
            || !Mathf.Approximately(curTickProgress, lastTickProgress);

        if (!changed)
        {
            // ✅ อัปเดต radial fill ขอ�� buff icons ทุก frame แม้ไม่ dirty
            UpdateBuffIconsPerFrame();
            return;
        }

        lastTimebar = curTimebar;
        lastFilling = curFilling;
        lastFillRemaining = curRemaining;
        lastTickProgress = curTickProgress;
        isDirty = false;

        RefreshTimebar(curTimebar, unit.TimebarMax);
        RefreshConditionIcon(curFilling, curRemaining, curTickProgress);
        UpdateBuffIconsPerFrame();
    }

    public void Refresh()
    {
        if (unit == null)
        {
            SetPortrait(null);
            SetTriggerIconImage(null);
            RefreshTimebar(0f, 1f);
            if (conditionIconRoot != null) conditionIconRoot.SetActive(false);
            return;
        }

        isDirty = true;
    }

    // ─────────────────────────────────────
    // Portrait / Trigger icon
    // ─────────────────────────────────────
    private void SetTriggerIconImage(Sprite icon)
    {
        if (triggerIconImage == null) return;
        bool has = icon != null;
        triggerIconImage.gameObject.SetActive(has);
        if (has) triggerIconImage.sprite = icon;
    }

    private void SetPortrait(Sprite sprite)
    {
        bool has = sprite != null;

        if (portraitEmptyOverlay != null)
            portraitEmptyOverlay.gameObject.SetActive(!has);

        if (portraitImage != null)
        {
            portraitImage.gameObject.SetActive(has);
            if (portraitImage.sprite != sprite)
                portraitImage.sprite = sprite;
        }
    }

    // ─────────────────────────────────────
    // Timebar
    // ─────────────────────────────────────
    private void RefreshTimebar(float current, float max)
    {
        if (timeText != null)
        {
            int cur = Mathf.RoundToInt(current);
            int m = Mathf.RoundToInt(max);
            timeText.text = $"{cur}/{m}";
        }

        if (timeBar != null)
            timeBar.SetFillAmount(current, max);
    }

    // ─────────────────────────────────────
    // Condition Icon (radial fill ตาม trigger window)
    // ─────────────────────────────────────
    private void RefreshConditionIcon(bool isFillWindowActive, float fillWindowRemaining, float tickProgress01)
    {
        bool hasIcon = cachedIcon != null;

        if (conditionIconRoot != null)
            conditionIconRoot.SetActive(hasIcon);

        if (!hasIcon || conditionIconUI == null || unit == null) return;

        float duration;
        float remaining;

        switch (unit.TriggerCondition)
        {
            case BacklineTrigger.OnEnemyStunned:
            case BacklineTrigger.OnAllyHit:
            case BacklineTrigger.OnAllyUsedSkill:
                duration = unit.FillWindowDuration;
                remaining = isFillWindowActive ? fillWindowRemaining : duration;
                break;

            case BacklineTrigger.PassiveTimer:
                duration = 1f;
                remaining = 1f - tickProgress01;
                break;

            default:
                duration = 1f;
                remaining = 1f;
                break;
        }

        conditionIconUI.SetData(cachedIcon, duration, remaining);
    }

    // ─────────────────────────────────────
    // Buff Icons — เหมือน CharacterPanel
    // ─────────────────────────────────────
    private void HandleBuffListChanged(BacklinePassiveUnit owner)
    {
        RebuildBuffIcons();
    }

    private void RebuildBuffIcons()
    {
        ClearBuffIcons();

        if (unit == null || buffIconsAnchor == null || buffIconPrefab == null) return;

        IReadOnlyList<TimedBuffInstance> buffs = unit.GetOwnedBuffIconsReadOnly();
        if (buffs == null) return;

        List<TimedBuffInstance> list = new List<TimedBuffInstance>();
        for (int i = 0; i < buffs.Count; i++)
        {
            TimedBuffInstance b = buffs[i];
            if (b == null) continue;
            if (b.IsExpired) continue;
            if (b.IsDebuff) continue;  // ✅ กรองเฉพาะ Buff เท่านั้น
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

    // ✅ แค่อัปเดต radial fill — ไม่ Tick เอง (BacklinePassiveUnit Tick ให้แล้ว)
    private void UpdateBuffIconsPerFrame()
    {
        if (trackedBuffs.Count == 0) return;

        for (int i = trackedBuffs.Count - 1; i >= 0; i--)
        {
            TimedBuffInstance b = trackedBuffs[i];

            // ✅ ถ้าหมดเวลา → ลบ icon ออก (BacklinePassiveUnit ลบออกจาก list แล้ว event จะเรียก RebuildBuffIcons)
            if (b == null || b.IsExpired)
            {
                if (i < buffIconGOs.Count && buffIconGOs[i] != null)
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
        if (prefab == null) return null;

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