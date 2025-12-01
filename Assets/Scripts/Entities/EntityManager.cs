using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Singleton manager for all entities in the game.
/// Enables efficient queries by type and does some entity lifecycle tracking.
/// </summary>
public class EntityManager : MonoBehaviour
{
    #region Fields And Properties
    public static EntityManager Instance { get; private set; }

    private Dictionary<int, IEntity> entities = new();
    private Dictionary<EntityType, List<IEntity>> entitiesByType = new();
    private List<IEntity> entitiesToRemove = new();
    #endregion Fields And Properties



    #region Unity Methods
    private void Awake()
    {
        Debug.Log("[EntityManager] Hello World!");
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[EntityManager] Duplicate instance detected -- annihilating it now!");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Initialize type buckets
        foreach (EntityType type in System.Enum.GetValues(typeof(EntityType)))
        {
            entitiesByType[type] = new List<IEntity>();
        }
    }

    private void Update()
    {
        // Process deferred removals at frame end to avoid iteration issues
        if (entitiesToRemove.Count > 0)
        {
            foreach (var entity in entitiesToRemove)
            {
                RemoveEntityImmediate(entity);
            }
            entitiesToRemove.Clear();
        }
    }
    #endregion Unity Methods



    #region Entity Lifecycle Methods
    /// <summary>
    /// Register a new entity with the manager.
    /// Should be called automatically by entities in their Start().
    /// </summary>
    public void RegisterEntity(IEntity entity)
    {
        Debug.Log($"[EntityManager] Registering entity ID: {entity.EntityID}, Type: {entity.Type}");
        if (entities.ContainsKey(entity.EntityID))
        {
            Debug.LogWarning($"[EntityManager] Entity {entity.EntityID} already registered!");
            return;
        }

        entities[entity.EntityID] = entity;
        entitiesByType[entity.Type].Add(entity);

        //Debug.Log($"[EntityManager] Registered {entity.Type} entity (ID: {entity.EntityID})");
    }

    /// <summary>
    /// Unregister an entity from the manager.
    /// Called automatically by entities in their OnDestroy().
    /// </summary>
    public void UnregisterEntity(IEntity entity)
    {
        if (!entities.Remove(entity.EntityID))
        {
            Debug.LogWarning($"[EntityManager] Tried to unregister unknown entity {entity.EntityID}");
            return;
        }

        entitiesByType[entity.Type].Remove(entity);
        //Debug.Log($"[EntityManager] Unregistered {entity.Type} entity (ID: {entity.EntityID})");
    }

    /// <summary>
    /// Mark entity for deferred removal (safe during iteration).
    /// Actually removed at frame end.
    /// </summary>
    public void MarkForRemoval(IEntity entity)
    {
        if (!entitiesToRemove.Contains(entity))
        {
            entitiesToRemove.Add(entity);
        }
    }
    #endregion Entity Lifecycle Methods



    #region Query Methods
    /// <summary>
    /// Get all entities of a specific type.
    /// Learner uses this to query all enemies, hazards, etc.
    /// </summary>
    public List<IEntity> GetEntitiesByType(EntityType type)
    {
        return new List<IEntity>(entitiesByType[type]);
    }

    /// <summary>
    /// Generic helper to get entities of a specific runtime type filtered by EntityType.
    /// Returns only live entities.
    /// </summary>
    public List<T> GetEntitiesByType<T>(EntityType type) where T : class, IEntity
    {
        return entitiesByType[type]
            .OfType<T>()
            .Where(e => e.IsAlive)
            .ToList();
    }

    /// <summary>
    /// Get all enemies currently alive.
    /// Convenience method for learner.
    /// </summary>
    public List<EnemyEntity> GetAllEnemies()
    {
        return GetEntitiesByType<EnemyEntity>(EntityType.Enemy);
    }

    /// <summary>
    /// Get all enemies of a specific archetype.
    /// Learner uses this to group enemies by archetype for learning.
    /// </summary>
    public List<EnemyEntity> GetEnemiesByArchetype(string archetypeId)
    {
            return GetEntitiesByType<EnemyEntity>(EntityType.Enemy)
                .Where(e => e.ArchetypeId == archetypeId)
                .ToList();
    }

    /// <summary>
    /// Get entities within a radius of a position.
    /// Useful for behavior queries, targeting, hazard detection.
    /// </summary>
    public List<IEntity> GetEntitiesInRadius(Vector3 position, float radius, EntityType? filterType = null)
    {
        var result = new List<IEntity>();

        var entiesToCheck = filterType.HasValue 
            ? entitiesByType[filterType.Value]
            : entities.Values.ToList();

        foreach (var entity in entiesToCheck)
        {
            if (entity.IsAlive && Vector3.Distance(entity.Position, position) <= radius)
            {
                result.Add(entity);
            }
        }

        return result;
    }

    /// <summary>
    /// Get entities within a cone/sector from a position.
    /// Useful for ranged attack/ability targeting.
    /// </summary>
    public List<IEntity> GetEntitiesInCone(Vector3 position, Vector3 direction, float radius, float angleInDegrees, EntityType? filterType = null)
    {
        var result = new List<IEntity>();
        float halfAngle = angleInDegrees * 0.5f;

        var entiesToCheck = filterType.HasValue 
            ? entitiesByType[filterType.Value]
            : entities.Values.ToList();

        foreach (var entity in entiesToCheck)
        {
            if (!entity.IsAlive) continue;

            Vector3 toEntity = (entity.Position - position).normalized;
            float distance = Vector3.Distance(entity.Position, position);

            if (distance <= radius)
            {
                float angle = Vector3.Angle(direction, toEntity);
                if (angle <= halfAngle)
                {
                    result.Add(entity);
                }
            }
        }

        return result;
    }
    #endregion Query Methods



    #region Stats Methods
    /// <summary>
    /// Get count of entities by type.
    /// Learner uses this for reward/adaptation calculations.
    /// </summary>
    public int GetEntityCount(EntityType type)
    {
        return entitiesByType[type].Count(e => e.IsAlive);
    }

    /// <summary>
    /// Get total count of all live entities.
    /// </summary>
    public int GetTotalEntityCount()
    {
        return entities.Count(kvp => kvp.Value.IsAlive);
    }

    private void RemoveEntityImmediate(IEntity entity)
    {
        entity.Despawn();
    }
    #endregion Stats Method    
}

