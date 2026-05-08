using UnityEngine;

// Put this on an empty GameObject that marks where player should appear.
// Set a unique spawnId (string).
public class SpawnPoint : MonoBehaviour
{
    [Tooltip("Unique id for this spawn point. SceneTeleportTrigger uses this id.")]
    public string spawnId = "default_spawn";
}