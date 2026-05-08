using UnityEngine;
using UnityEngine.UI;

public class AllyPortraitWidget : MonoBehaviour
{
    [Header("Dim only portrait")]
    public Graphic portraitGraphic; // ลาก Image ของ “รูป” เท่านั้น

    [Range(0f, 1f)] public float normalAlpha = 1f;
    [Range(0f, 1f)] public float dimmedAlpha = 0.35f;

    public void SetDimmed(bool dimmed)
    {
        if (portraitGraphic == null) return;

        Color c = portraitGraphic.color;
        c.a = dimmed ? dimmedAlpha : normalAlpha;
        portraitGraphic.color = c;
    }
}