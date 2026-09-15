using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// จัดการ Image ทุกชิ้นในฉาก Dialogue
/// - สร้าง Image จาก initialElements ตอนเปิดฉาก (ตำแหน่งและ sortOrder ตั้งครั้งเดียว)
/// - Swap รูปและ visibility ตาม SceneElementChange[] ใน Beat
/// - ลบทิ้งตอนปิดฉาก
/// </summary>
public class DialogueSceneDirector : MonoBehaviour
{
    [Header("Container")]
    [Tooltip("Parent ที่จะสร้าง Image ทั้งหมดลงไป")]
    [SerializeField] private RectTransform elementContainer;

    [Header("Background")]
    [Tooltip("Image พื้นหลังของฉาก — ตั้งค่าจาก defaultBackground ใน DialogueData")]
    [SerializeField] private Image backgroundImage;

    // dict: id → Image ที่สร้างไว้
    private readonly Dictionary<string, Image> elementImages = new Dictionary<string, Image>();

    // ─────────────────────────────────────────────────────
    // เปิดฉาก: สร้าง Image ทุกชิ้น + ตั้ง bg
    // ─────────────────────────────────────────────────────

    public void BuildScene(DialogueData data)
    {
        ClearScene();

        // ตั้ง background (ครั้งเดียวตลอด dialogue)
        if (backgroundImage != null && data.defaultBackground != null)
            backgroundImage.sprite = data.defaultBackground;

        if (data.initialElements == null) return;

        foreach (SceneElementData el in data.initialElements)
        {
            if (string.IsNullOrEmpty(el.id)) continue;

            // สร้าง GameObject + Image
            GameObject go = new GameObject(el.id, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(elementContainer, false);

            // ตั้ง RectTransform (ตำแหน่งและขนาด กำหนดตอนนี้ครั้งเดียว)
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = el.position;
            rt.sizeDelta = el.size;

            // ตั้ง Image
            Image img = go.GetComponent<Image>();
            img.sprite = el.sprite;
            img.preserveAspect = true;

            // ตั้ง visibility เริ่มต้น
            go.SetActive(el.visibleOnStart);

            elementImages[el.id] = img;
        }

        // เรียง layer ตาม sortOrder (ทำครั้งเดียว ไม่เปลี่ยนอีก)
        SortElementsBySortOrder(data.initialElements);
    }

    // ─────────────────────────────────────────────────────
    // Apply การเปลี่ยนแปลงจาก Beat
    // เรียกก่อนแสดงข้อความ — swap รูปทุกชิ้นพร้อมกันใน frame เดียว
    // ─────────────────────────────────────────────────────

    public void ApplyChanges(DialogueBeat beat)
    {
        if (beat.changes == null || beat.changes.Length == 0) return;

        foreach (SceneElementChange change in beat.changes)
        {
            if (string.IsNullOrEmpty(change.targetId)) continue;

            if (!elementImages.TryGetValue(change.targetId, out Image img))
            {
                Debug.LogWarning($"[DialogueSceneDirector] ไม่เจอ element id='{change.targetId}' — ตรวจสอบ id ใน initialElements");
                continue;
            }

            // ✅ swap sprite (รูปหน้า/ท่าทาง)
            if (change.newSprite != null)
                img.sprite = change.newSprite;

            // ✅ เปลี่ยน visibility (แสดง/ซ่อน)
            if (change.useNewVisible)
                img.gameObject.SetActive(change.newVisible);
        }
    }

    // ─────────────────────────────────────────────────────
    // ปิดฉาก: ลบ Image ทั้งหมด
    // ─────────────────────────────────────────────────────

    public void ClearScene()
    {
        foreach (var kvp in elementImages)
        {
            if (kvp.Value != null)
                Destroy(kvp.Value.gameObject);
        }
        elementImages.Clear();
    }

    // ─────────────────────────────────────────────────────
    // Helper: เรียง sibling index ตาม sortOrder (น้อย→มาก = ข้างหลัง→ข้างหน้า)
    // ─────────────────────────────────────────────────────

    private void SortElementsBySortOrder(SceneElementData[] elements)
    {
        List<(int order, string id)> sorted = new List<(int, string)>();

        foreach (SceneElementData el in elements)
            sorted.Add((el.sortOrder, el.id));

        sorted.Sort((a, b) => a.order.CompareTo(b.order));

        for (int i = 0; i < sorted.Count; i++)
        {
            if (elementImages.TryGetValue(sorted[i].id, out Image img))
                img.transform.SetSiblingIndex(i);
        }
    }
}