using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class TeleportTriggerInScene : MonoBehaviour
{
    public enum FallbackMode { None, UseZoneGroupSpawn, CenterOfTargetBg, EdgeOffsetFromTargetBg, PreserveRelativeX }

    [Header("Primary target (highest priority)")]
    public Transform targetSpawn;            // ถ้าตั้งไว้: teleport ไปตำแหน่งนี้

    [Header("Optional group/bg fallback")]
    public ZoneGroup zoneGroup;              // ถ้าใช้ ZoneGroup: จะใช้ left/right spawn ถ้า targetSpawn ว่าง
    public ZoneGroup.Side targetSide = ZoneGroup.Side.Right;
    public GameObject targetBg;              // ถ้าไม่มี zoneGroup แต่มี BG root ให้ใช้คำนวณ bounds

    [Header("Fallback behaviour")]
    public FallbackMode fallback = FallbackMode.CenterOfTargetBg;
    public float edgeOffset = 0.8f;          // ถ้าใช้ EdgeOffsetFromTargetBg
    public float yMargin = 0.2f;             // margin เมื่อ clamp Y ภายใน bounds

    [Header("Activation")]
    public bool requireButton = false;       // ถ้า true ต้องกด interactKey ภายใน trigger
    public KeyCode interactKey = KeyCode.F;
    public bool autoTriggerOnEnter = false;  // ถ้า true จะ teleport ทันที on enter เมื่อ requireButton=false
    public bool disableAfterUse = false;
    public float cooldown = 0.25f;

    [Header("Teleport behaviour")]
    public bool resetVelocity = true;
    public bool faceTowardTarget = true;
    public bool preserveZ = true;

    [Header("Optional")]
    public FadeController fadeController;    // ถ้ามี ใส่เพื่อ fade (optional)
    public float fadeWait = 0.03f;

    [Header("Events / Debug")]
    public UnityEvent onTeleported;
    public bool debugLogs = false;

    bool playerInZone = false;
    Transform playerTransform;
    float lastTeleport = -999f;

    void Reset()
    {
        var c = GetComponent<Collider2D>();
        if (c != null) c.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInZone = true;
        playerTransform = other.transform;
        if (debugLogs) Debug.Log("[TeleportTriggerInScene] Player entered trigger.");

        if (!requireButton && autoTriggerOnEnter && Time.time - lastTeleport >= cooldown)
        {
            TryTeleport();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInZone = false;
        playerTransform = null;
        if (debugLogs) Debug.Log("[TeleportTriggerInScene] Player left trigger.");
    }

    void Update()
    {
        if (!playerInZone || !requireButton) return;
        if (Time.time - lastTeleport < cooldown) return;
        if (Input.GetKeyDown(interactKey)) TryTeleport();
    }

    void TryTeleport()
    {
        if (playerTransform == null)
        {
            if (debugLogs) Debug.LogWarning("[TeleportTriggerInScene] No player to teleport.");
            return;
        }

        if (Time.time - lastTeleport < cooldown) return;
        lastTeleport = Time.time;

        // Compute destination
        Vector3 dest;
        bool ok = ComputeDestination(out dest);
        if (!ok)
        {
            Debug.LogWarning("[TeleportTriggerInScene] No valid destination computed.");
            return;
        }

        StartCoroutine(TeleportCoroutine(dest));
    }

    bool ComputeDestination(out Vector3 outPos)
    {
        outPos = playerTransform.position;

        // 1) Highest priority: explicit targetSpawn
        if (targetSpawn != null)
        {
            outPos = targetSpawn.position;
            if (preserveZ) outPos.z = playerTransform.position.z;
            return true;
        }

        // 2) If ZoneGroup provided and has spawn for side
        if (zoneGroup != null)
        {
            Transform spawn = (targetSide == ZoneGroup.Side.Left) ? zoneGroup.leftSpawn : zoneGroup.rightSpawn;
            if (spawn != null)
            {
                outPos = spawn.position;
                if (preserveZ) outPos.z = playerTransform.position.z;
                return true;
            }
        }

        // 3) Bounds-based fallback requires a targetBg with BoxCollider2D
        GameObject bg = null;
        if (zoneGroup != null)
        {
            bg = (targetSide == ZoneGroup.Side.Left) ? zoneGroup.leftBg : zoneGroup.rightBg;
        }
        if (bg == null) bg = targetBg;

        if (bg == null)
        {
            // nothing else available
            return fallback == FallbackMode.None ? false : false;
        }

        var box = bg.GetComponent<BoxCollider2D>() ?? bg.GetComponentInChildren<BoxCollider2D>();
        if (box == null)
        {
            if (debugLogs) Debug.LogWarning("[TeleportTriggerInScene] targetBg has no BoxCollider2D.");
            return false;
        }

        Bounds b = box.bounds;

        switch (fallback)
        {
            case FallbackMode.CenterOfTargetBg:
                outPos = new Vector3(b.center.x, b.center.y, preserveZ ? playerTransform.position.z : b.center.z);
                return true;

            case FallbackMode.EdgeOffsetFromTargetBg:
                {
                    float x = (targetSide == ZoneGroup.Side.Left) ? b.min.x + edgeOffset : b.max.x - edgeOffset;
                    float y = Mathf.Clamp(playerTransform.position.y, b.min.y + yMargin, b.max.y - yMargin);
                    outPos = new Vector3(x, y, preserveZ ? playerTransform.position.z : b.center.z);
                    return true;
                }

            case FallbackMode.PreserveRelativeX:
                {
                    // Attempt to use this trigger's parent BoxCollider2D as source bounds if present
                    var sourceBox = GetComponentInParent<BoxCollider2D>();
                    Bounds sb = sourceBox != null ? sourceBox.bounds : b;
                    float sbWidth = Mathf.Max(0.0001f, sb.max.x - sb.min.x);
                    float rel = (playerTransform.position.x - sb.min.x) / sbWidth;
                    rel = Mathf.Clamp01(rel);
                    float tbWidth = Mathf.Max(0.0001f, b.max.x - b.min.x);
                    float x = b.min.x + rel * tbWidth;
                    float y = Mathf.Clamp(playerTransform.position.y, b.min.y + yMargin, b.max.y - yMargin);
                    outPos = new Vector3(x, y, preserveZ ? playerTransform.position.z : b.center.z);
                    return true;
                }

            case FallbackMode.None:
            default:
                return false;
        }
    }

    IEnumerator TeleportCoroutine(Vector3 dest)
    {
        // optional fade out
        if (fadeController != null) yield return fadeController.FadeOutCoroutine();

        // reset velocity
        if (resetVelocity)
        {
            var rb = playerTransform.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = Vector2.zero;
        }

        // move player
        playerTransform.position = dest;

        // small wait
        if (fadeWait > 0f) yield return new WaitForSecondsRealtime(fadeWait);

        // optional fade in
        if (fadeController != null) yield return fadeController.FadeInCoroutine();

        // face toward center of target bg if requested
        if (faceTowardTarget)
        {
            Vector3 toward = dest;
            GameObject bg = null;
            if (zoneGroup != null) bg = (targetSide == ZoneGroup.Side.Left) ? zoneGroup.leftBg : zoneGroup.rightBg;
            if (bg == null) bg = targetBg;
            if (bg != null)
            {
                var box = bg.GetComponent<BoxCollider2D>() ?? bg.GetComponentInChildren<BoxCollider2D>();
                if (box != null) toward = box.bounds.center;
            }
            bool faceRight = (toward.x >= playerTransform.position.x);
            var pc = playerTransform.GetComponent<PlayerController>();
            if (pc != null) pc.SetFacingDirection(faceRight);
            else
            {
                var sr = playerTransform.GetComponentInChildren<SpriteRenderer>();
                if (sr != null) sr.flipX = !faceRight;
            }
        }

        onTeleported?.Invoke();

        // optionally switch BG if zoneGroup defined
        if (zoneGroup != null)
        {
            var bg = (targetSide == ZoneGroup.Side.Left) ? zoneGroup.leftBg : zoneGroup.rightBg;
            if (bg != null && BgSwitcher.Instance != null) BgSwitcher.Instance.SwitchTo(bg);
        }

        if (disableAfterUse)
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
            enabled = false;
        }

        if (debugLogs) Debug.Log("[TeleportTriggerInScene] Teleport completed.");
        yield return null;
    }
}