using System.Collections.Generic;
using UnityEngine;

public class CharacterFactory : MonoBehaviour
{
    [Header("Database (Separated)")]
    [Tooltip("ใส่เฉพาะ AllyCharacterData เท่านั้น")]
    [SerializeField] private List<AllyCharacterData> allyDatabase = new List<AllyCharacterData>();

    [Tooltip("ใส่เฉพาะ EnemyCharacterData เท่านั้น")]
    [SerializeField] private List<EnemyCharacterData> enemyDatabase = new List<EnemyCharacterData>();

    [Header("Battle Spawn (Use Spawn Points)")]
    [SerializeField] private Transform[] allySpawnPoints = new Transform[8];
    [SerializeField] private Transform enemySpawnPoint;

    [Header("Idle Display (Single Anchor Point)")]
    [SerializeField] private Transform idleDisplayAnchor;
    [SerializeField] private float idleDisplaySpacing = 1.5f;
    [SerializeField] private Transform idleDisplayParent;

    [Header("Debug")]
    [SerializeField] private bool debugIdleSpawn = true;

    // =======================
    // Find Data
    // =======================
    public AllyCharacterData FindAllyData(string characterName)
    {
        if (allyDatabase == null) return null;

        for (int i = 0; i < allyDatabase.Count; i++)
        {
            var d = allyDatabase[i];
            if (d != null && d.characterName == characterName)
                return d;
        }
        return null;
    }

    public EnemyCharacterData FindEnemyData(string characterName)
    {
        if (enemyDatabase == null) return null;

        for (int i = 0; i < enemyDatabase.Count; i++)
        {
            var d = enemyDatabase[i];
            if (d != null && d.characterName == characterName)
                return d;
        }
        return null;
    }

    public BaseCharacterData FindAnyData(string characterName)
    {
        var a = FindAllyData(characterName);
        if (a != null) return a;

        var e = FindEnemyData(characterName);
        if (e != null) return e;

        return null;
    }

    // =======================
    // Battle Spawn
    // =======================
    public BattleCharacter CreateAllyBySelectedSlot(BaseCharacterData data, int selectedSlotIndex)
    {
        AllyCharacterData allyData = data as AllyCharacterData;
        if (allyData == null)
        {
            Debug.LogError("❌ CreateAllyBySelectedSlot: data ต้องเป็น AllyCharacterData", this);
            return null;
        }

        if (allySpawnPoints == null || allySpawnPoints.Length != 8)
        {
            Debug.LogError("❌ allySpawnPoints ต้องมีขนาด 8", this);
            return null;
        }

        if (selectedSlotIndex < 0 || selectedSlotIndex >= allySpawnPoints.Length)
        {
            Debug.LogError($"❌ selectedSlotIndex out of range: {selectedSlotIndex}", this);
            return null;
        }

        Transform spawn = allySpawnPoints[selectedSlotIndex];
        if (spawn == null)
        {
            Debug.LogError($"❌ allySpawnPoints[{selectedSlotIndex}] is null", this);
            return null;
        }

        GameObject prefab = allyData.characterPrefab != null
            ? allyData.characterPrefab
            : new GameObject(allyData.characterName + "_EmptyBattle");

        GameObject instance = Instantiate(prefab, spawn.position, Quaternion.identity, spawn);
        instance.name = allyData.characterName;

        BattleCharacter bc = instance.GetComponent<BattleCharacter>();
        if (bc == null)
        {
            bc = instance.AddComponent<AllyBattleCharacter>();
        }

        bc.SetupData(allyData);
        bc.SlotIndex = selectedSlotIndex;
        bc.InitializeVisuals();

        // ✅ ใช้ UltimateRuntime ที่อยู่ใน prefab เท่านั้น
        EnsureUltimateRuntimeForAlly(bc, allyData);

        return bc;
    }

    private void EnsureUltimateRuntimeForAlly(BattleCharacter bc, AllyCharacterData allyData)
    {
        if (bc == null) return;

        UltimateRuntime rt = bc.GetComponent<UltimateRuntime>();
        if (rt == null)
        {
            Debug.LogWarning("⚠️ UltimateRuntime missing on prefab (โปรดใส่ใน prefab)", this);
            return;
        }

        // เซ็ต ultimate skill
        if (allyData != null)
            rt.SetSkill(allyData.ultimateSkill, resetCooldown: true);
        else
            rt.SetSkill(null, resetCooldown: true);
    }

    public BattleCharacter CreateEnemy(string characterName)
    {
        EnemyCharacterData enemyData = FindEnemyData(characterName);
        if (enemyData == null)
        {
            Debug.LogError($"❌ CreateEnemy: ไม่พบ EnemyCharacterData ของ '{characterName}'", this);
            return null;
        }

        Transform spawn = enemySpawnPoint != null ? enemySpawnPoint : transform;

        GameObject prefab = enemyData.characterPrefab != null
            ? enemyData.characterPrefab
            : new GameObject(enemyData.characterName + "_EmptyEnemy");

        GameObject instance = Instantiate(prefab, spawn.position, Quaternion.identity, spawn);
        instance.name = enemyData.characterName;

        BattleCharacter bc = instance.GetComponent<BattleCharacter>();
        if (bc == null)
        {
            bc = instance.AddComponent<Dysnorma>();
        }

        bc.SetupData(enemyData);
        bc.SlotIndex = -1;
        bc.InitializeVisuals();

        return bc;
    }

    // =======================
    // Idle Display (Centered from Single Anchor)
    // =======================
    public List<GameObject> CreateIdleDisplayCentered(IReadOnlyList<BaseCharacterData> characters)
    {
        List<GameObject> result = new List<GameObject>();
        if (characters == null) return result;

        if (idleDisplayAnchor == null)
        {
            Debug.LogError("❌ CreateIdleDisplayCentered: idleDisplayAnchor is null (assign in Inspector)", this);
            return result;
        }

        int count = 0;
        for (int i = 0; i < characters.Count; i++)
            if (characters[i] != null) count++;

        if (count == 0) return result;

        float centerOffset = (count - 1) * 0.5f;
        Transform parent = idleDisplayParent != null ? idleDisplayParent : idleDisplayAnchor;

        int k = 0;
        for (int i = 0; i < characters.Count; i++)
        {
            BaseCharacterData cd = characters[i];
            if (cd == null) continue;

            GameObject prefab = cd.idleDisplayPrefab != null ? cd.idleDisplayPrefab : cd.characterPrefab;
            if (prefab == null)
            {
                Debug.LogWarning($"⚠️ CreateIdleDisplayCentered: '{cd.characterName}' ไม่มี idleDisplayPrefab และไม่มี characterPrefab", this);
                k++;
                continue;
            }

            float xOffset = (k - centerOffset) * idleDisplaySpacing;
            Vector3 spawnPos = idleDisplayAnchor.position + idleDisplayAnchor.right * xOffset;

            GameObject idleInstance = Instantiate(prefab, spawnPos, Quaternion.identity, parent);
            idleInstance.name = $"{cd.characterName}_IdleDisplay";

            if (debugIdleSpawn)
                Debug.Log($"✅ IdleDisplay: '{cd.characterName}' ({k + 1}/{count}) offset={xOffset:F1} pos={spawnPos}", this);

            result.Add(idleInstance);
            k++;
        }

        return result;
    }

    public GameObject CreateIdleDisplaySingle(BaseCharacterData data)
    {
        if (data == null)
        {
            Debug.LogError("❌ CreateIdleDisplaySingle: data is null", this);
            return null;
        }

        if (idleDisplayAnchor == null)
        {
            Debug.LogError("❌ CreateIdleDisplaySingle: idleDisplayAnchor is null (assign in Inspector)", this);
            return null;
        }

        GameObject prefab = data.idleDisplayPrefab != null ? data.idleDisplayPrefab : data.characterPrefab;
        if (prefab == null)
        {
            Debug.LogError($"❌ CreateIdleDisplaySingle: '{data.characterName}' ไม่มี idleDisplayPrefab และไม่มี characterPrefab", this);
            return null;
        }

        Transform parent = idleDisplayParent != null ? idleDisplayParent : idleDisplayAnchor;

        GameObject idleInstance = Instantiate(prefab, idleDisplayAnchor.position, Quaternion.identity, parent);
        idleInstance.name = $"{data.characterName}_IdleDisplay_Single";

        if (debugIdleSpawn)
            Debug.Log($"✅ IdleDisplaySingle: '{data.characterName}' pos={idleDisplayAnchor.position}", this);

        return idleInstance;
    }
}