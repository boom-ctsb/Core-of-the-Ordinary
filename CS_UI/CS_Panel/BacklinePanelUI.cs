using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BacklinePanelUI : MonoBehaviour
{
    [Header("Portrait")]
    [SerializeField] private Image portraitImage;
    [SerializeField] private Image portraitEmptyOverlay;

    [Header("Timebar")]
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private DynamicBar timeBar;

    [Header("Condition Icon (Radial Fill)")]
    [SerializeField] private StatusEffectIconUI conditionIconUI;
    [SerializeField] private GameObject conditionIconRoot;

    private BacklinePassiveUnit unit;

    public void Bind(BacklinePassiveUnit u)
    {
        unit = u;
        Refresh();
    }

    private void Update()
    {
        if (unit != null)
            Refresh();
    }

    public void Refresh()
    {
        if (unit == null)
        {
            SetPortrait(null);
            SetTimebar(0f, 1f, false);
            SetConditionIcon(false, null, 1f, 0f);
            return;
        }

        Sprite portrait = unit.GetComponent<BacklinePassiveUnit>() != null && unit.GetComponent<BacklinePassiveUnit>() != null
            ? (unit.GetComponent<BacklinePassiveUnit>() != null ? unit.GetComponent<BacklinePassiveUnit>().GetConditionIcon() : null)
            : null;

        // portrait ใช้จาก data
        Sprite p = null;
        var data = unit.GetComponent<BacklinePassiveUnit>() != null ? unit.GetComponent<BacklinePassiveUnit>() : null;
        if (data != null && data.GetComponent<BacklinePassiveUnit>() != null) { }

        // ดึง portrait จาก data
        if (unit != null)
        {
            var d = unit.GetComponent<BacklinePassiveUnit>();
            if (d != null)
            {
                var field = d.GetType().GetField("data", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    var dataObj = field.GetValue(d) as AllyBacklineData;
                    if (dataObj != null) p = dataObj.portraitSprite;
                }
            }
        }

        SetPortrait(p);

        float cur = unit.Timebar;
        float max = unit.TimebarMax;

        SetTimebar(cur, max, true);

        bool active = unit.IsFillWindowActive;
        if (active)
        {
            Sprite icon = unit.GetConditionIcon();
            SetConditionIcon(true, icon, unit.FillWindowDuration, unit.FillWindowRemaining);
        }
        else
        {
            SetConditionIcon(false, null, 1f, 0f);
        }
    }

    private void SetPortrait(Sprite sprite)
    {
        bool has = sprite != null;

        if (portraitEmptyOverlay != null)
            portraitEmptyOverlay.gameObject.SetActive(!has);

        if (portraitImage != null)
        {
            portraitImage.gameObject.SetActive(has);
            portraitImage.sprite = sprite;
        }
    }

    private void SetTimebar(float current, float max, bool show)
    {
        if (!show)
        {
            if (timeText != null) timeText.text = "";
            if (timeBar != null) timeBar.SetFillAmount(0f, 1f);
            return;
        }

        if (timeText != null)
        {
            int cur = Mathf.RoundToInt(current);
            int m = Mathf.RoundToInt(max);
            timeText.text = $"{cur}/{m}";
        }

        if (timeBar != null)
            timeBar.SetFillAmount(current, max);
    }

    private void SetConditionIcon(bool show, Sprite icon, float duration, float remaining)
    {
        if (conditionIconRoot != null)
            conditionIconRoot.SetActive(show);

        if (!show || conditionIconUI == null) return;

        conditionIconUI.SetData(icon, duration, remaining);
    }
}