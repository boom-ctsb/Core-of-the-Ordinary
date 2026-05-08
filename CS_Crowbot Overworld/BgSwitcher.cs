using System.Collections.Generic;
using UnityEngine;

// BgSwitcher (grouped)
// - Manage background "rooms" organized as groups (e.g., Hall01) where each group has a left and right child scene.
// - You can still use SwitchTo(GameObject) for single BG roots, or use SwitchToGroup(groupName, Side).
// - When switching, BgSwitcher will SetActive the correct side, update currentActive and currentGroup,
//   and update CamFollow.boundsBox (if Main Camera has CamFollow).
[DisallowMultipleComponent]
public class BgSwitcher : MonoBehaviour
{
    public static BgSwitcher Instance { get; private set; }

    public enum Side { Left = 0, Right = 1 }

    [System.Serializable]
    public class RoomGroup
    {
        public string groupName;
        public GameObject leftBg;
        public GameObject rightBg;
        public Side startSide = Side.Left;
        public bool autoRegister = true; // when true, register both BGs into the flat backgrounds list (if used)
    }

    [Header("Flat list (optional)")]
    [Tooltip("Flat list of background roots. You can also rely on Groups below.")]
    public List<GameObject> backgrounds = new List<GameObject>();

    [Header("Grouped rooms")]
    [Tooltip("Define room groups where each group contains left/right background roots.")]
    public List<RoomGroup> groups = new List<RoomGroup>();

    [Header("Start")]
    [Tooltip("If set, this GameObject will be active at Start. If empty, BgSwitcher tries groups' startSide.")]
    public GameObject startActive;

    // runtime
    GameObject currentActive;
    RoomGroup currentGroup;
    Side currentGroupSide;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // build flat backgrounds list if groups provided and flat list is empty (optional convenience)
        if ((backgrounds == null || backgrounds.Count == 0) && groups != null && groups.Count > 0)
        {
            backgrounds = new List<GameObject>();
            foreach (var g in groups)
            {
                if (g == null) continue;
                if (g.leftBg != null) backgrounds.Add(g.leftBg);
                if (g.rightBg != null) backgrounds.Add(g.rightBg);
            }
        }

        // If startActive explicitly provided and exists in backgrounds/groups, use it.
        if (startActive != null && (backgrounds.Contains(startActive) || FindGroupContaining(startActive) != null))
        {
            SwitchTo(startActive);
            return;
        }

        // Otherwise, if groups defined, activate the first group's startSide
        if (groups != null && groups.Count > 0)
        {
            var g = groups[0];
            SwitchToGroup(g, g.startSide);
            return;
        }

        // fallback: if flat backgrounds exist, activate first
        if (backgrounds != null && backgrounds.Count > 0)
        {
            SetActiveInstant(backgrounds[0]);
        }
    }

    // Switch by GameObject (legacy/simple)
    public void SwitchTo(GameObject target)
    {
        if (target == null) { Debug.LogWarning("BgSwitcher: target is null."); return; }

        // If target belongs to a group, set group state accordingly
        var grp = FindGroupContaining(target);
        if (grp != null)
        {
            Side s = (target == grp.leftBg) ? Side.Left : Side.Right;
            SwitchToGroup(grp, s);
            return;
        }

        // Otherwise treat as a single BG root
        if (backgrounds == null || !backgrounds.Contains(target))
        {
            Debug.LogWarning("BgSwitcher: target is not in backgrounds list. Adding it automatically.");
            if (backgrounds == null) backgrounds = new List<GameObject>();
            backgrounds.Add(target);
        }

        if (currentActive == target) return;

        foreach (var bg in backgrounds)
        {
            if (bg == null) continue;
            bg.SetActive(bg == target);
        }

        currentActive = target;
        currentGroup = null;
        // update camera bounds
        UpdateCameraBoundsFor(target);
    }

    // Switch to a side in a named group
    public void SwitchToGroup(string groupName, Side side)
    {
        var g = FindGroupByName(groupName);
        if (g == null) { Debug.LogWarning($"BgSwitcher: group '{groupName}' not found."); return; }
        SwitchToGroup(g, side);
    }

    // Switch to a specific group's side (primary API for groups)
    public void SwitchToGroup(RoomGroup group, Side side)
    {
        if (group == null) return;

        GameObject left = group.leftBg;
        GameObject right = group.rightBg;

        // Deactivate all backgrounds that belong to this group then activate the chosen side
        if (left != null) left.SetActive(side == Side.Left);
        if (right != null) right.SetActive(side == Side.Right);

        // Optionally, keep other groups as-is (we assume groups are disjoint). If you prefer exclusive activation,
        // uncomment the following to deactivate all other group's children:
        /*
        foreach (var g in groups)
        {
            if (g == group) continue;
            if (g.leftBg != null) g.leftBg.SetActive(false);
            if (g.rightBg != null) g.rightBg.SetActive(false);
        }
        foreach (var b in backgrounds) if (b != null && !IsPartOfAnyGroup(b)) b.SetActive(false);
        */

        // Update flat backgrounds list entries for these group items (if backgrounds list used)
        if (backgrounds == null) backgrounds = new List<GameObject>();
        if (group.autoRegister)
        {
            if (group.leftBg != null && !backgrounds.Contains(group.leftBg)) backgrounds.Add(group.leftBg);
            if (group.rightBg != null && !backgrounds.Contains(group.rightBg)) backgrounds.Add(group.rightBg);
        }

        currentGroup = group;
        currentGroupSide = side;
        currentActive = (side == Side.Left) ? group.leftBg : group.rightBg;

        // update camera bounds
        UpdateCameraBoundsFor(currentActive);
    }

    // Set active instantly among the flat backgrounds only (legacy helper)
    public void SetActiveInstant(GameObject target)
    {
        if (backgrounds == null) backgrounds = new List<GameObject>();
        foreach (var bg in backgrounds)
        {
            if (bg == null) continue;
            bg.SetActive(bg == target);
        }
        currentActive = target;
        currentGroup = FindGroupContaining(target);
        if (currentGroup != null)
            currentGroupSide = (target == currentGroup.leftBg) ? Side.Left : Side.Right;
        UpdateCameraBoundsFor(target);
    }

    // Helpers
    RoomGroup FindGroupByName(string name)
    {
        if (groups == null) return null;
        foreach (var g in groups)
        {
            if (g == null) continue;
            if (!string.IsNullOrEmpty(g.groupName) && g.groupName == name) return g;
        }
        return null;
    }

    RoomGroup FindGroupContaining(GameObject bg)
    {
        if (groups == null || bg == null) return null;
        foreach (var g in groups)
        {
            if (g == null) continue;
            if (g.leftBg == bg || g.rightBg == bg) return g;
        }
        return null;
    }

    bool IsPartOfAnyGroup(GameObject bg)
    {
        return FindGroupContaining(bg) != null;
    }

    // Update camera bounds by finding BoxCollider2D on target
    void UpdateCameraBoundsFor(GameObject target)
    {
        if (target == null) return;
        var cam = Camera.main;
        if (cam == null) return;
        var camFollow = cam.GetComponent<CamFollow>();
        if (camFollow == null) return;

        // prefer BoxCollider2D on root, else children
        var box = target.GetComponent<BoxCollider2D>() ?? target.GetComponentInChildren<BoxCollider2D>();
        if (box != null)
        {
            camFollow.boundsBox = box;
            camFollow.RefreshBounds();
        }
    }

    // Public getters for other systems
    public GameObject GetCurrentActive() => currentActive;
    public RoomGroup GetCurrentGroup() => currentGroup;
    public Side GetCurrentGroupSide() => currentGroupSide;
}