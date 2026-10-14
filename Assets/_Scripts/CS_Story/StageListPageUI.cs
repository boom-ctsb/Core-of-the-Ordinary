using UnityEngine;
using TMPro;

public class StageListPageUI : MonoBehaviour
{
    [Header("Stage Slots — ลากตามลำดับ")]
    [SerializeField] private StageSlotUI[] stageSlots;

    [Header("Counter")]
    [SerializeField] private TextMeshProUGUI mainCountText;
    [SerializeField] private TextMeshProUGUI subCountText;

    [Header("All Stages")]
    [SerializeField] private StageData[] allStages;

    // ─────────────────────────────────────────

    public void Open()
    {
        gameObject.SetActive(true);
        SetupSlots();
        RefreshCount();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────

    private void SetupSlots()
    {
        for (int i = 0; i < stageSlots.Length; i++)
        {
            if (i < allStages.Length)
            {
                stageSlots[i].gameObject.SetActive(true);
                stageSlots[i].Setup(allStages[i]);
            }
            else
            {
                stageSlots[i].gameObject.SetActive(false);
            }
        }
    }

    private void RefreshCount()
    {
        int mainTotal = 0, mainDone = 0;
        int subTotal = 0, subDone = 0;

        foreach (var stage in allStages)
        {
            foreach (var ep in stage.episodes)
            {
                if (ep.isMain) { mainTotal++; if (ep.isCompleted) mainDone++; }
                else { subTotal++; if (ep.isCompleted) subDone++; }
            }
        }

        if (mainCountText != null) mainCountText.text = $"Main: {mainDone}/{mainTotal}";
        if (subCountText != null) subCountText.text = $"Sub: {subDone}/{subTotal}";
    }
}