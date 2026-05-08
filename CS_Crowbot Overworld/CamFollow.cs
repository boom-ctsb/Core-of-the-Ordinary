using System.Collections.Generic;
using UnityEngine;

// กระชับ: CamFollow ที่รองรับทั้งแบบถูกกำหนดด้วย BoxCollider2D เดียว (boundsBox)
// หรือค้นหา BoxCollider2D หลายตัวใน Scene (tag) และเลือกตัวที่ครอบตำแหน่ง player (หรือใกล้สุดเป็น fallback).
// - ถ้าต้องการบังคับใช้ box เฉพาะ ให้ตั้ง boundsBox (override).
// - เรียก RefreshBoxes() เพื่อรีโหลดรายการ bounds จาก tag หรือ RegisterBox/UnregisterBox เพื่อจัดการ runtime.
// - เรียก RefreshBounds() เมื่อเปลี่ยน boundsBox เพื่อให้คำนวณ min/max ใหม่ทันที.
[RequireComponent(typeof(Camera))]
public class CamFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;            // ให้ลาก Player.transform หรือหาโดย tag "Player"
    public Vector2 offset = Vector2.zero;

    [Header("Follow")]
    public float smoothTime = 0.12f;
    public bool followX = true;
    public bool followY = false;

    [Header("Bounds")]
    public BoxCollider2D boundsBox;     // manual override (single)
    public bool autoFindBoxesByTag = true;
    public string boxTag = "LevelRoot"; // tag ของ root แต่ละห้อง/scene ที่มี BoxCollider2D

    [Header("Behavior")]
    public bool snapOnBoundChange = true; // ถ้าเปลี่ยน bounds ให้ snap กล้องไปตำแหน่ง clamp ทันที

    // internal
    Camera cam;
    Vector3 velocity = Vector3.zero;

    List<BoxCollider2D> boxes = new List<BoxCollider2D>();
    BoxCollider2D activeBox = null;
    Vector2 minBounds, maxBounds;
    bool hasBounds = false;
    BoxCollider2D lastAssignedBoundsBox = null;

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

        if (autoFindBoxesByTag) RefreshBoxes();
        UpdateActiveBox(true);
    }

    void LateUpdate()
    {
        if (target == null) return;

        // If manual override was changed externally, detect & refresh
        if (boundsBox != lastAssignedBoundsBox)
        {
            UpdateActiveBox(true);
        }
        else
        {
            UpdateActiveBox(false);
        }

        Vector3 desired = (Vector3)target.position + (Vector3)offset;
        if (!followX) desired.x = transform.position.x;
        if (!followY) desired.y = transform.position.y;

        if (hasBounds)
            desired = GetClampedCameraPos(desired);

        transform.position = Vector3.SmoothDamp(transform.position,
            new Vector3(desired.x, desired.y, transform.position.z),
            ref velocity, Mathf.Max(0.0001f, smoothTime));
    }

    // public api
    public void RefreshBoxes()
    {
        boxes.Clear();
        if (!string.IsNullOrEmpty(boxTag))
        {
            var roots = GameObject.FindGameObjectsWithTag(boxTag);
            foreach (var r in roots)
            {
                if (r == null) continue;
                var b = r.GetComponent<BoxCollider2D>() ?? r.GetComponentInChildren<BoxCollider2D>();
                if (b != null) boxes.Add(b);
            }
        }
    }

    public void RegisterBox(BoxCollider2D box)
    {
        if (box == null) return;
        if (!boxes.Contains(box)) boxes.Add(box);
    }

    public void UnregisterBox(BoxCollider2D box)
    {
        if (box == null) return;
        boxes.Remove(box);
    }

    public void RefreshBounds()
    {
        UpdateActiveBox(true);
    }

    // choose active box: priority = boundsBox override -> any box that contains target -> nearest center -> none
    void UpdateActiveBox(bool forceSnapIfChanged)
    {
        BoxCollider2D chosen = null;

        if (boundsBox != null)
        {
            chosen = boundsBox;
        }
        else
        {
            if (boxes != null && boxes.Count > 0 && target != null)
            {
                Vector2 p = target.position;
                // prefer box that contains player
                foreach (var b in boxes)
                {
                    if (b == null) continue;
                    if (b.bounds.Contains(p)) { chosen = b; break; }
                }

                // fallback nearest center
                if (chosen == null)
                {
                    float best = float.MaxValue;
                    foreach (var b in boxes)
                    {
                        if (b == null) continue;
                        float d = Vector2.SqrMagnitude((Vector2)b.bounds.center - p);
                        if (d < best) { best = d; chosen = b; }
                    }
                }
            }
        }

        bool changed = (chosen != activeBox);
        activeBox = chosen;
        lastAssignedBoundsBox = boundsBox;

        if (activeBox != null)
        {
            Bounds bb = activeBox.bounds;
            minBounds = new Vector2(bb.min.x, bb.min.y);
            maxBounds = new Vector2(bb.max.x, bb.max.y);
            hasBounds = true;
        }
        else
        {
            hasBounds = false;
        }

        if (changed && snapOnBoundChange && hasBounds && target != null)
        {
            // Snap camera immediately to clamped position to avoid being stuck outside bounds
            Vector3 snapPos = GetClampedCameraPos(target.position + (Vector3)offset);
            snapPos.z = transform.position.z;
            transform.position = snapPos;
            velocity = Vector3.zero;
            // small allowance: reset smoothing so camera doesn't glide from far away
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