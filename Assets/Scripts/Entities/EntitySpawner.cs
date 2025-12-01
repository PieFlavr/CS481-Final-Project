using UnityEngine;

/// <summary>
/// Generic spawner for entities from EntityData ScriptableObjects.
/// Configures stats, FSM, and registers with EntityManager.
/// Works with any entity data: archetypes, player, utilities, etc.
/// </summary>
public class EntitySpawner : MonoBehaviour
{
    [Header("Default Prefab")]
    [SerializeField] private BaseEntity defaultPrefab;

    [Header("Demo / Temporary Spawn (remove for production)")]
    [Tooltip("If enabled, the spawner will immediately spawn `demoSpawnCount` of `demoArchetype` on Start and then destroy itself. Temporary for testing.")]
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private ArchetypeData demoArchetype;
    [SerializeField] private int demoSpawnCount = 5;
    [SerializeField] private float demoSpawnRadius = 2f;

    /// <summary>
    /// Spawn a single entity from entity data.
    /// </summary>
    public BaseEntity SpawnEntity(EntityData entityData, Vector3 position, Quaternion rotation = default, BaseEntity prefab = null)
    {
        if (entityData == null)
        {
            Debug.LogError("[EntitySpawner] Cannot spawn entity without entity data!");
            return null;
        }

        // Choose prefab: explicit parameter > entity data prefab > defaultPrefab
        BaseEntity prefabToUse = prefab ?? entityData?.Prefab ?? defaultPrefab;
        if (prefabToUse == null)
        {
            Debug.LogError("[EntitySpawner] No entity prefab available to spawn!");
            return null;
        }

        // Instantiate entity
        var entity = Instantiate(prefabToUse, position, rotation);
        entity.name = $"{entityData.DisplayName}";

        // Configure from entity data
        ConfigureEntityFromData(entity, entityData);

        return entity;
    }

    /// <summary>
    /// Spawn multiple entities of the same archetype.
    /// </summary>
    public void SpawnEntities(ArchetypeData archetypeData, int count, Vector3 spawnCenter, float spawnRadius)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 randomOffset = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = spawnCenter + randomOffset;
            SpawnEntity(archetypeData, spawnPos, default, archetypeData?.Prefab);
        }
    }

    private void Start()
    {
        // Temporary demo behavior: spawn a small wave then self-destruct so this spawner doesn't persist.
        if (!spawnOnStart) return;

        if (demoArchetype == null)
        {
            Debug.LogWarning("[EntitySpawner] spawnOnStart is true but no demoArchetype is assigned. Skipping demo spawn.");
            return;
        }

        SpawnEntities(demoArchetype, Mathf.Max(1, demoSpawnCount), transform.position, Mathf.Max(0.01f, demoSpawnRadius));
        Debug.Log($"[EntitySpawner] Demo spawned {demoSpawnCount} of {demoArchetype.DisplayName}. Destroying spawner (temporary).");

        // Destroy this GameObject so the demo behaviour is easy to remove and won't repeat.
        Destroy(gameObject);
    }

    /// <summary>
    /// Generic, type-safe spawn helper.
    /// </summary>
    public T SpawnEntity<T>(T prefab, EntityData entityData, Vector3 position, Quaternion rotation = default) where T : BaseEntity
    {
        if (prefab == null) return null;
        var instance = Instantiate(prefab, position, rotation);
        instance.name = $"{entityData.DisplayName}";
        ConfigureEntityFromData(instance, entityData);
        return instance;
    }

    private void ConfigureEntityFromData(BaseEntity entity, EntityData entityData)
    {
        // Prefer explicit initialization contract on the entity instead of reflection.
        // Enemy implements `Initialize(EntityData)` (inherited from BaseEntity), so call it.
        try
        {
            entity.Initialize(entityData);
        }
        catch (System.Exception ex)
        {
            // Fallback: if for some reason the entity doesn't implement Initialize as expected,
            // still attempt to set stats from the data directly.
            Debug.LogWarning($"[EntitySpawner] Initialize failed on entity {entity.name}: {ex.Message}");
        }

        // Ensure StatsComponent exists and is configured
        var statsComponent = entity.GetComponent<StatsComponent>();
        if (statsComponent == null)
        {
            statsComponent = entity.gameObject.AddComponent<StatsComponent>();
        }
        statsComponent.InitializeFromEntityData(entityData);

        // TODO: Configure animator, visuals, colliders based on entity data
    }
}
