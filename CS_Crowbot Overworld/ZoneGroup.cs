using UnityEngine;

// ZoneGroup: เก็บ BG สองฝั่ง (Left/Right) และ spawn points ของแต่ละฝั่ง
// เมธอด SwitchToSide จะสั่ง BgSwitcher.SwitchTo(bg) และย้าย player ไปยัง spawn ถ้ามี
public class ZoneGroup : MonoBehaviour
{
    public enum Side { Left = 0, Right = 1 }

    [Header("Group identity")]
    public string groupName;

    [Header("Backgrounds")]
    public GameObject leftBg;
    public GameObject rightBg;

    [Header("Spawn points (optional)")]
    public Transform leftSpawn;   // ถ้าไม่ใส่ จะไม่ย้าย player
    public Transform rightSpawn;

    [Header("Options")]
    public bool autoRegisterToBgSwitcher = true; // ถ้า true จะเพิ่ม left/right BG เข้า BgSwitcher.backgrounds ถ้ายังไม่ม��
    public bool debugLogs = false;

    void Start()
    {
        if (autoRegisterToBgSwitcher && BgSwitcher.Instance != null)
        {
            TryRegisterBg(leftBg);
            TryRegisterBg(rightBg);
        }
    }

    void TryRegisterBg(GameObject bg)
    {
        if (bg == null) return;
        if (BgSwitcher.Instance == null) return;
        if (!BgSwitcher.Instance.backgrounds.Contains(bg))
        {
            BgSwitcher.Instance.backgrounds.Add(bg);
            if (debugLogs) Debug.Log($"[ZoneGroup] Registered BG '{bg.name}' to BgSwitcher.");
        }
    }

    // Switch to a side; optionally move the player to the side's spawn if provided
    public void SwitchToSide(Side side, Transform player = null, bool movePlayerToSpawn = true)
    {
        GameObject targetBg = (side == Side.Left) ? leftBg : rightBg;
        Transform spawn = (side == Side.Left) ? leftSpawn : rightSpawn;

        if (targetBg == null)
        {
            if (debugLogs) Debug.LogWarning($"[ZoneGroup:{groupName}] No target bg for side {side}.");
            return;
        }

        // Switch background via BgSwitcher (will also update camera bounds if implemented)
        if (BgSwitcher.Instance != null)
        {
            BgSwitcher.Instance.SwitchTo(targetBg);
        }
        else
        {
            // fallback: just activate target and deactivate siblings in this group
            if (debugLogs) Debug.LogWarning("[ZoneGroup] BgSwitcher.Instance is null — switching by SetActive fallback.");
            SetActiveFallback(targetBg);
        }

        // optionally move player to spawn
        if (movePlayerToSpawn && player != null && spawn != null)
        {
            // try to stop player's velocity if it has Rigidbody2D
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = Vector2.zero;

            // set position (preserve z)
            Vector3 pos = spawn.position;
            pos.z = player.position.z;
            player.position = pos;
        }
    }

    void SetActiveFallback(GameObject target)
    {
        if (leftBg != null) leftBg.SetActive(leftBg == target);
        if (rightBg != null) rightBg.SetActive(rightBg == target);
    }

    // Convenience methods
    public void SwitchToLeft(Transform player = null, bool movePlayer = true) => SwitchToSide(Side.Left, player, movePlayer);
    public void SwitchToRight(Transform player = null, bool movePlayer = true) => SwitchToSide(Side.Right, player, movePlayer);
}