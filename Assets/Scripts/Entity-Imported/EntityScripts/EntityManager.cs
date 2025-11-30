using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all entities in the game.
/// Handles registration, tracking, querying, and cleanup of entities.
/// </summary>
public class EntityManager : MonoBehaviour
{
    #region Singleton

    public static EntityManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("EntityManager: Singleton instance created");
        }
        else
        {
            Debug.LogWarning("EntityManager: Duplicate instance found. Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    #endregion

    #region Fields

    [Header("Entity Tracking")]
    [SerializeField] private List<Entity> allEntities = new List<Entity>();
    [SerializeField] private Entity playerEntity; // Keep special track of the player

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true; 
    [SerializeField] private int totalEntitiesSpawned = 0;
    [SerializeField] private int totalEntitiesDestroyed = 0;

    // Faction tracking
    private Dictionary<EntityFaction, List<Entity>> entitiesByFaction = new Dictionary<EntityFaction, List<Entity>>();

    #endregion

    #region Unity Methods

    private void Start()
    {
        // Initialize faction dictionaries
        foreach (EntityFaction faction in System.Enum.GetValues(typeof(EntityFaction)))
        {
            entitiesByFaction[faction] = new List<Entity>();
        }
    }

    private void Update()
    {
        // Clean up null references (entities destroyed outside of UnregisterEntity)
        CleanupNullEntities();
    }

    #endregion

    #region Registration

    /// <summary>
    /// Registers an entity with the manager.
    /// Called automatically by Entity.Awake() or manually for dynamic spawning.
    /// </summary>
    public void RegisterEntity(Entity entity)
    {
        if (entity == null)
        {
            Debug.LogWarning("EntityManager: Attempted to register null entity!");
            return;
        }

        if (allEntities.Contains(entity))
        {
            Debug.LogWarning($"EntityManager: Entity {entity.Data.entityName} is already registered!");
            return;
        }

        // Add to main list
        allEntities.Add(entity);

        // Add to faction-specific list
        EntityFaction faction = entity.Data.faction;
        if (!entitiesByFaction.ContainsKey(faction))
        {
            entitiesByFaction[faction] = new List<Entity>();
        }
        entitiesByFaction[faction].Add(entity);

        // Track player separately
        if (faction == EntityFaction.Player)
        {
            playerEntity = entity;
            Debug.Log("EntityManager: Player entity registered");
        }

        totalEntitiesSpawned++;

        if (showDebugInfo)
        {
            Debug.Log($"EntityManager: Registered {entity.Data.entityName} ({faction}). Total: {allEntities.Count}");
        }
    }

    /// <summary>
    /// Unregisters an entity from the manager.
    /// Called automatically by Entity.OnDestroy() or manually before destruction.
    /// </summary>
    public void UnregisterEntity(Entity entity)
    {
        if (entity == null) return;

        // Remove from main list
        allEntities.Remove(entity);

        // Remove from faction list
        EntityFaction faction = entity.Data.faction;
        if (entitiesByFaction.ContainsKey(faction))
        {
            entitiesByFaction[faction].Remove(entity);
        }

        // Clear player reference if this was the player
        if (entity == playerEntity)
        {
            playerEntity = null;
            Debug.Log("EntityManager: Player entity unregistered");
        }

        totalEntitiesDestroyed++;

        if (showDebugInfo)
        {
            Debug.Log($"EntityManager: Unregistered {entity.Data.entityName}. Total: {allEntities.Count}");
        }
    }

    #endregion

    #region Queries

    /// <summary>
    /// Gets all registered entities.
    /// </summary>
    public List<Entity> GetAllEntities()
    {
        return new List<Entity>(allEntities); // Return a copy to prevent external modification
    }

    /// <summary>
    /// Gets all entities of a specific faction.
    /// </summary>
    public List<Entity> GetEntitiesByFaction(EntityFaction faction)
    {
        if (entitiesByFaction.ContainsKey(faction))
        {
            return new List<Entity>(entitiesByFaction[faction]);
        }
        return new List<Entity>();
    }

    /// <summary>
    /// Gets the player entity.
    /// </summary>
    public Entity GetPlayer()
    {
        return playerEntity;
    }

    /// <summary>
    /// Gets all enemy entities.
    /// </summary>
    public List<Entity> GetEnemies()
    {
        return GetEntitiesByFaction(EntityFaction.Enemy);
    }

    /// <summary>
    /// Gets all entities within a radius of a position.
    /// </summary>
    public List<Entity> GetEntitiesInRadius(Vector2 position, float radius, EntityFaction faction = EntityFaction.Enemy)
    {
        List<Entity> result = new List<Entity>();
        List<Entity> candidates = GetEntitiesByFaction(faction);

        foreach (Entity entity in candidates)
        {
            if (entity != null && Vector2.Distance(position, entity.transform.position) <= radius)
            {
                result.Add(entity);
            }
        }

        return result;
    }

    /// <summary>
    /// Gets the closest entity of a specific faction to a position.
    /// </summary>
    public Entity GetClosestEntity(Vector2 position, EntityFaction faction = EntityFaction.Enemy)
    {
        List<Entity> candidates = GetEntitiesByFaction(faction);
        Entity closest = null;
        float closestDistance = float.MaxValue;

        foreach (Entity entity in candidates)
        {
            if (entity != null)
            {
                float distance = Vector2.Distance(position, entity.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = entity;
                }
            }
        }

        return closest;
    }

    /// <summary>
    /// Gets the total count of entities.
    /// </summary>
    public int GetEntityCount()
    {
        return allEntities.Count;
    }

    /// <summary>
    /// Gets the count of entities in a specific faction.
    /// </summary>
    public int GetEntityCount(EntityFaction faction)
    {
        return entitiesByFaction.ContainsKey(faction) ? entitiesByFaction[faction].Count : 0;
    }

    #endregion

    #region Cleanup

    /// <summary>
    /// Removes null references from entity lists (cleanup from external destroys).
    /// </summary>
    private void CleanupNullEntities()
    {
        // Clean main list
        allEntities.RemoveAll(e => e == null);

        // Clean faction lists
        foreach (var kvp in entitiesByFaction)
        {
            kvp.Value.RemoveAll(e => e == null);
        }

        // Clear player reference if destroyed
        if (playerEntity == null || playerEntity.gameObject == null)
        {
            playerEntity = null;
        }
    }

    /// <summary>
    /// Clears all entity references (use when changing scenes/restarting).
    /// </summary>
    public void ClearAllEntities()
    {
        allEntities.Clear();
        foreach (var kvp in entitiesByFaction)
        {
            kvp.Value.Clear();
        }
        playerEntity = null;

        Debug.Log("EntityManager: All entities cleared");
    }

    /// <summary>
    /// Destroys all entities of a specific faction.
    /// </summary>
    public void DestroyAllEntities(EntityFaction faction)
    {
        List<Entity> toDestroy = GetEntitiesByFaction(faction);

        foreach (Entity entity in toDestroy)
        {
            if (entity != null)
            {
                Destroy(entity.gameObject);
            }
        }

        Debug.Log($"EntityManager: Destroyed all {faction} entities ({toDestroy.Count} total)");
    }

    #endregion

    #region Debug

    /// <summary>
    /// Prints debug information about all registered entities.
    /// </summary>
    [ContextMenu("Debug: Print All Entities")]
    public void DebugPrintAllEntities()
    {
        Debug.Log($"=== EntityManager Debug ===");
        Debug.Log($"Total Entities: {allEntities.Count}");
        Debug.Log($"Total Spawned: {totalEntitiesSpawned}");
        Debug.Log($"Total Destroyed: {totalEntitiesDestroyed}");
        Debug.Log($"");

        foreach (EntityFaction faction in System.Enum.GetValues(typeof(EntityFaction)))
        {
            int count = GetEntityCount(faction);
            if (count > 0)
            {
                Debug.Log($"{faction}: {count}");
                foreach (Entity entity in GetEntitiesByFaction(faction))
                {
                    if (entity != null)
                    {
                        Debug.Log($"  - {entity.Data.entityName} ({entity.Resources.CurrentHealth}/{entity.Resources.MaxHealth} HP)");
                    }
                }
            }
        }
    }

    #endregion
}