using System.Collections.Generic;
using UnityEngine;

// CamFollow ที่รองรับหลาย BoxCollider2D (zones).
// - ถ้ามี multipleBoxes ให้เลือก box ที่ contains(player) หากมีหลายตัวจะเลือกตัวแรกที��พบ
// - หากไม่มี box ที่ contains player จะเลือก nearest box (center-to-player distance) เป็น fallback
// - ยังรองรับ manual override: activeBox (single) ที่จะใช้ถ้า set ไว้
[RequireComponent(typeof(Camera))]
public class CamFollowMulti : MonoBehaviour
{
    [Header("Target")]
    public Transform target;                // ลาก Player.transform หรือให้ค้นหา tag "Player"
    public Vector2 offset = Vector2.zero;

    [Header("Follow")]
    public float smoothTime = 0.12f;
    public bool followX = true;
    public bool followY = false;

    [Header("Bounds Collection")]
    public List<BoxCollider2D> multipleBoxes = new List<BoxCollider2D>(); // ใส่ทุก bounds ที่จะให้ camera ควบคุม
    public BoxCollider2D activeBoxOverride; // ถ้าต้องการบังคับใช้ box ตัวนี้เสมอ
    public bool autoFindBoxesByTag = false; // หา BoxCollider2D ใน scene ที่มี tag (optional)
    public string boxTag = "LevelRoot";

    Camera cam;
    Vector3 velocity = Vector3.zero;

    // cached computed min/max for chosen active box
    public Vector2 minBounds;
    public Vector2 maxBounds;
    bool hasBounds = false;

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null) cam = Camera.main;
    }

    void Start()
    {
        if (target == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) target = go.transform;
        }

        if (autoFindBoxesByTag)
        {
            RefreshBoxesFromTag();
        }

        UpdateActiveBoxAndRefresh();
        if (target != null) transform.position = GetClampedCameraPos(target.position + (Vector3)offset);
    }

    void LateUpdate()
    {
        if (target == null) return;

        // choose best active box each frame (handles dynamic situations)
        UpdateActiveBoxAndRefresh();

        Vector3 desired = target.position + (Vector3)offset;
        if (!followX) desired.x = transform.position.x;
        if (!followY) desired.y = transform.position.y;

        if (hasBounds) desired = GetClampedCameraPos(desired);

        transform.position = Vector3.SmoothDamp(transform.position,
            new Vector3(desired.x, desired.y, transform.position.z),
            ref velocity, Mathf.Max(0.0001f, smoothTime));
    }

    // public: add/remove box runtime
    public void RegisterBox(BoxCollider2D box)
    {
        if (box == null) return;
        if (!multipleBoxes.Contains(box)) multipleBoxes.Add(box);
    }
    public void UnregisterBox(BoxCollider2D box)
    {
        if (box == null) return;
        multipleBoxes.Remove(box);
    }

    void RefreshBoxesFromTag()
    {
        multipleBoxes.Clear();
        var roots = GameObject.FindGameObjectsWithTag(boxTag);
        foreach (var r in roots)
        {
            var box = r.GetComponent<BoxCollider2D>() ?? r.GetComponentInChildren<BoxCollider2D>();
            if (box != null) multipleBoxes.Add(box);
        }
    }

    // เลือก active box ตามลำดับ: override -> contains target -> nearest -> none
    void UpdateActiveBoxAndRefresh()
    {
        BoxCollider2D chosen = null;
        if (activeBoxOverride != null) chosen = activeBoxOverride;
        else
        {
            if (multipleBoxes != null && multipleBoxes.Count > 0)
            {
                Vector2 p = target != null ? (Vector2)target.position : Vector2.zero;

                // first prefer any box that contains the player
                foreach (var b in multipleBoxes)
                {
                    if (b == null) continue;
                    if (b.bounds.Contains(p)) { chosen = b; break; }
                }

                // fallback: nearest center
                if (chosen == null)
                {
                    float bestDist = float.MaxValue;
                    foreach (var b in multipleBoxes)
                    {
                        if (b == null) continue;
                        float d = Vector2.SqrMagnitude((Vector2)b.bounds.center - p);
                        if (d < bestDist) { bestDist = d; chosen = b; }
                    }
                }
            }
        }

        if (chosen != null)
        {
            Bounds b = chosen.bounds;
            minBounds = new Vector2(b.min.x, b.min.y);
            maxBounds = new Vector2(b.max.x, b.max.y);
            hasBounds = true;
        }
        else
        {
            hasBounds = false;
        }
    }

    Vector3 GetClampedCameraPos(Vector3 desiredWorldPos)
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return desiredWorldPos;

        float camHalfHeight = cam.orthographicSize;
        float camHalfWidth = cam.orthographicSize * cam.aspect;

        float boundsWidth = maxBounds.x - minBounds.x;
        float boundsHeight = maxBounds.y - minBounds.y;

        Vector3 result = desiredWorldPos;

        if (boundsWidth <= camHalfWidth * 2f)
            result.x = (minBounds.x + maxBounds.x) / 2f;
        else
        {
            float minX = minBounds.x + camHalfWidth;
            float maxX = maxBounds.x - camHalfWidth;
            result.x = Mathf.Clamp(desiredWorldPos.x, minX, maxX);
        }

        if (boundsHeight <= camHalfHeight * 2f)
            result.y = (minBounds.y + maxBounds.y) / 2f;
        else
        {
            float minY = minBounds.y + camHalfHeight;
            float maxY = maxBounds.y - camHalfHeight;
            result.y = Mathf.Clamp(desiredWorldPos.y, minY, maxY);
        }

        result.z = transform.position.z;
        return result;
    }
}