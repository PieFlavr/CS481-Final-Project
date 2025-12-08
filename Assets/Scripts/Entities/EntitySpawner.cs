using UnityEngine;

/// <summary>
/// Spawns entity prefabs at night based on SwarmSpawner configuration.
/// Registers entities with EntityManager.
/// </summary>
public class EntitySpawner : MonoBehaviour
{
    [Header("Spawn Configuration")]
    [SerializeField] private BaseEntity prefabToSpawn;
    [SerializeField] private int spawnCount = 5;
    [SerializeField] private float spawnRadius = 2f;

    /// <summary>
    /// Spawn configured prefab entities around this spawner's position.
    /// Called by SwarmSpawner during OnNightStart.
    /// </summary>
    public void SpawnWave()
    {
        Debug.Log($"[EntitySpawner] Spawning wave at {transform.position}.");
        if (prefabToSpawn == null)
        {
            Debug.LogWarning($"[EntitySpawner] {gameObject.name} has no prefab configured. Skipping spawn.");
            return;
        }

        SpawnEntities(prefabToSpawn, spawnCount, transform.position, spawnRadius);
        Debug.Log($"[EntitySpawner] Spawned {spawnCount} entities.");
    }

    /// <summary>
    /// Spawn multiple entities from the same prefab.
    /// </summary>
    public void SpawnEntities(BaseEntity prefab, int count, Vector3 spawnCenter, float spawnRadius)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 randomOffset = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = spawnCenter + randomOffset;
            Instantiate(prefab, spawnPos, Quaternion.identity);
        }
    }

    /// <summary>
    /// Spawn a single entity from a prefab at a position.
    /// </summary>
    public BaseEntity SpawnEntity(BaseEntity prefab, Vector3 position, Quaternion rotation = default)
    {
        if (prefab == null)
        {
            Debug.LogError("[EntitySpawner] Cannot spawn entity without prefab!");
            return null;
        }

        return Instantiate(prefab, position, rotation);
    }
}

