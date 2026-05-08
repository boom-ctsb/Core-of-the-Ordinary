using UnityEngine;

// วางบน GameObject ที่มี Collider2D (isTrigger = true).
// ใช้งานร่วมกับ ZoneGroup (leftBg/rightBg + optional spawn).
// - กดปุ่มเดียว (switchKey) เพื่อสลับฝั่ง
// - ย้าย player ไป spawn (ถ้ามี) และหันหน้าไปด้านที่สลับ
[RequireComponent(typeof(Collider2D))]
public class ZoneSwitcher : MonoBehaviour
{
    [Header("References")]
    public ZoneGroup zoneGroup;                     // ชี้ไปที่ ZoneGroup (ต้องมี left/right BG)
    public KeyCode switchKey = KeyCode.F;           // ปุ่มกดเพื่อสลับ (single-button)
    public string promptText = "กด F เพื่อสลับมุมมอง"; // ข้อความ prompt (ใช้ UIManager ถ้ามี)

    [Header("Behavior")]
    public bool toggleIfOnSameGroup = true;         // ถ้าปัจจุบันเป็นฝั่งหนึ่ง ให้สลับไปอีกฝั่ง
    public ZoneGroup.Side defaultSide = ZoneGroup.Side.Right; // ถาไม่รู้ current ให้สลับไปฝั่งนี้
    public bool movePlayerToSpawn = true;           // ย้าย player ไป spawn ถ้ามี
    public bool faceTowardSideAfterSwitch = true;   // ให้ตัวละครหันไปทาง side ใหม่

    [Header("Timing")]
    public float cooldown = 0.5f;                   // ป้องกันสลับซ้ำ

    [Header("Debug")]
    public bool debugLogs = false;

    bool playerInZone = false;
    Transform playerTransform = null;
    float lastSwitchAt = -999f;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInZone = true;
        playerTransform = other.transform;
        UIManager.Instance?.ShowPrompt(promptText);
        if (debugLogs) Debug.Log($"[ZoneSwitcher] Player entered zone '{name}'.");
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInZone = false;
        playerTransform = null;
        UIManager.Instance?.HidePrompt();
        if (debugLogs) Debug.Log($"[ZoneSwitcher] Player exited zone '{name}'.");
    }

    void Update()
    {
        if (!playerInZone) return;
        if (Time.time - lastSwitchAt < cooldown) return;
        if (zoneGroup == null) return;

        if (Input.GetKeyDown(switchKey))
        {
            ZoneGroup.Side target = DetermineTargetSide();
            DoSwitch(target);
        }
    }

    ZoneGroup.Side DetermineTargetSide()
    {
        if (toggleIfOnSameGroup && BgSwitcher.Instance != null)
        {
            var current = BgSwitcher.Instance.GetCurrentActive();
            if (current != null)
            {
                if (current == zoneGroup.leftBg) return ZoneGroup.Side.Right;
                if (current == zoneGroup.rightBg) return ZoneGroup.Side.Left;
            }
        }
        return defaultSide;
    }

    void DoSwitch(ZoneGroup.Side side)
    {
        if (zoneGroup == null) return;

        zoneGroup.SwitchToSide(side, playerTransform, movePlayerToSpawn);

        if (faceTowardSideAfterSwitch && playerTransform != null)
        {
            bool faceRight = (side == ZoneGroup.Side.Right);
            ApplyPlayerFacing(playerTransform, faceRight);
        }

        UIManager.Instance?.HidePrompt();
        lastSwitchAt = Time.time;

        if (debugLogs) Debug.Log($"[ZoneSwitcher] Switched group '{(zoneGroup.groupName ?? zoneGroup.name)}' to {side}");
    }

    void ApplyPlayerFacing(Transform player, bool faceRight)
    {
        if (player == null) return;

        var pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.SetFacingDirection(faceRight);
            return;
        }

        var sr = player.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.flipX = !faceRight; // assumes sprite faces right by default
        }

        // Fallback: set Animator bool "FacingRight" if that parameter exists.
        // We cannot rely on an external AnimatorExtensions, so check parameters directly.
        var anim = player.GetComponentInChildren<Animator>();
        if (anim != null)
        {
            var pars = anim.parameters;
            for (int i = 0; i < pars.Length; i++)
            {
                if (pars[i].name == "FacingRight" && pars[i].type == AnimatorControllerParameterType.Bool)
                {
                    anim.SetBool("FacingRight", faceRight);
                    break;
                }
            }
        }
    }
}