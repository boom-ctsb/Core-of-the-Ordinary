using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

// Editor/runtime utility: คำนวณ world bounds จาก child SpriteRenderers/Tilemaps/Collider2D
// แล้วสร้างหรืออัปเดต BoxCollider2D บน GameObject นี้ (Level root).
// ใช้ [ContextMenu("Auto Set Bounds")] ใน Inspector เพื่อเรียก หรือเปิด assignToCamFollow เพื่อตั้งให้ CamFollow อัตโนมัติ.
[ExecuteAlways]
public class LevelBoundsAuto : MonoBehaviour
{
    [Header("Auto bounds settings")]
    public float padding = 0.5f;
    public bool includeInactiveChildren = false;
    public LayerMask excludeLayers = 0;
    public List<string> excludeTags = new List<string>();
    public bool assignToCamFollow = false;
    public bool debugLogs = true;

    [ContextMenu("Auto Set Bounds")]
    public void AutoSetBounds()
    {
        Bounds computed = ComputeWorldBounds();
        if (computed.size == Vector3.zero)
        {
            if (debugLogs) Debug.LogWarning($"[LevelBoundsAuto] {name}: no renderers/colliders found.");
            return;
        }

        computed.Expand(padding * 2f);

        BoxCollider2D box = GetComponent<BoxCollider2D>();
#if UNITY_EDITOR
        if (box == null) box = UnityEditor.Undo.AddComponent<BoxCollider2D>(gameObject);
#else
        if (box == null) box = gameObject.AddComponent<BoxCollider2D>();
#endif

        Vector3 worldCenter = computed.center;
        Vector3 localCenter = transform.InverseTransformPoint(worldCenter);
        Vector3 lossy = transform.lossyScale;
        Vector2 localSize = new Vector2(
            computed.size.x / (Mathf.Approximately(lossy.x, 0f) ? 1f : lossy.x),
            computed.size.y / (Mathf.Approximately(lossy.y, 0f) ? 1f : lossy.y)
        );

        box.offset = new Vector2(localCenter.x, localCenter.y);
        box.size = localSize;

        if (debugLogs) Debug.Log($"[LevelBoundsAuto] {name}: BoxCollider2D set offset={box.offset}, size={box.size}");

        if (assignToCamFollow)
        {
            var cam = Camera.main;
            if (cam != null)
            {
                var camFollow = cam.GetComponent<CamFollow>();
                if (camFollow != null)
                {
                    camFollow.boundsBox = box;
                    camFollow.RefreshBounds();
                    if (debugLogs) Debug.Log("[LevelBoundsAuto] Assigned BoxCollider2D to CamFollow.");
                }
                else if (debugLogs) Debug.LogWarning("[LevelBoundsAuto] CamFollow not found on Main Camera.");
            }
            else if (debugLogs) Debug.LogWarning("[LevelBoundsAuto] Main Camera not found.");
        }
    }

    Bounds ComputeWorldBounds()
    {
        bool found = false;
        Bounds total = new Bounds();

        System.Func<Transform, bool> isExcluded = (t) =>
        {
            if (excludeTags != null && excludeTags.Count > 0)
            {
                if (!string.IsNullOrEmpty(t.tag) && excludeTags.Contains(t.tag)) return true;
            }
            if (excludeLayers != 0)
            {
                int layerMask = 1 << t.gameObject.layer;
                if ((excludeLayers.value & layerMask) != 0) return true;
            }
            return false;
        };

        var srs = includeInactiveChildren ? GetComponentsInChildren<SpriteRenderer>(true) : GetComponentsInChildren<SpriteRenderer>(false);
        foreach (var r in srs)
        {
            if (r == null || r.gameObject == this.gameObject) continue;
            if (isExcluded(r.transform)) continue;
            if (!found) { total = r.bounds; found = true; } else total.Encapsulate(r.bounds);
        }

        var tms = includeInactiveChildren ? GetComponentsInChildren<TilemapRenderer>(true) : GetComponentsInChildren<TilemapRenderer>(false);
        foreach (var tr in tms)
        {
            if (tr == null) continue;
            if (isExcluded(tr.transform)) continue;
            var tilemap = tr.GetComponent<Tilemap>();
            if (tilemap != null)
            {
                var tmLocal = tilemap.localBounds;
                var centerWorld = tr.transform.TransformPoint(tmLocal.center);
                var sizeWorld = Vector3.Scale(tmLocal.size, tr.transform.lossyScale);
                var b = new Bounds(centerWorld, sizeWorld);
                if (!found) { total = b; found = true; } else total.Encapsulate(b);
            }
            else
            {
                if (!found) { total = tr.bounds; found = true; } else total.Encapsulate(tr.bounds);
            }
        }

        var cols = includeInactiveChildren ? GetComponentsInChildren<Collider2D>(true) : GetComponentsInChildren<Collider2D>(false);
        foreach (var c in cols)
        {
            if (c == null || c.gameObject == this.gameObject) continue;
            if (isExcluded(c.transform)) continue;
            if (!found) { total = c.bounds; found = true; } else total.Encapsulate(c.bounds);
        }

        if (!found)
        {
            var children = includeInactiveChildren ? GetComponentsInChildren<Transform>(true) : GetComponentsInChildren<Transform>(false);
            foreach (var t in children)
            {
                if (t == transform) continue;
                if (isExcluded(t)) continue;
                var p = t.position;
                var b = new Bounds(p, Vector3.zero);
                if (!found) { total = b; found = true; } else total.Encapsulate(b);
            }
        }

        if (!found) return new Bounds(Vector3.zero, Vector3.zero);
        return total;
    }
}