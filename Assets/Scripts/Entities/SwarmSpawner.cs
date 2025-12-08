using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Container for a swarm configuration.
/// Holds a collection of EntitySpawners and manages spawning for a logical swarm unit.
/// Persists across nights; gets reconfigured by the learner for each night cycle.
/// </summary>
public class SwarmSpawner : MonoBehaviour
{
    #region Fields And Properties 

    [Tooltip("List of entity spawners that are part of this swarm. If left empty, will auto-populate from child components. Otherwise, only the assigned spawners will be used.")]
    [SerializeField] private List<EntitySpawner> entitySpawners = new List<EntitySpawner>();

    public List<EntitySpawner> GetEntitySpawners() => entitySpawners;
    #endregion Fields And Properties



    #region Unity Methods
    private void Awake()
    {
        // Auto-populate spawners from children if not manually assigned
        if (entitySpawners.Count == 0)
        {
            GetComponentsInChildren<EntitySpawner>(entitySpawners);
            Debug.Log($"[SwarmSpawner] Auto-populated {entitySpawners.Count} EntitySpawners from children.");
        }
    }
    #endregion Unity Methods



    #region Entity Spawner Management
    /// <summary>
    /// Add an entity spawner to this swarm dynamically.
    /// </summary>
    public void AddEntitySpawner(EntitySpawner spawner)
    {
        if (spawner == null) return;
        entitySpawners.Add(spawner);
    }

    /// <summary>
    /// Remove an entity spawner from this swarm dynamically.
    /// </summary>
    public void RemoveEntitySpawner(EntitySpawner spawner)
    {
        if (spawner == null) return;
        entitySpawners.Remove(spawner);
    }
    #endregion Entity Spawner Management



    #region Night Lifecycle
    /// <summary>
    /// Called by NightManager when the night begins.
    /// Learner can hook here to configure swarm behavior.
    /// </summary>
    public virtual void OnNightStart()
    {
        // Default: nothing. Override in subclasses or use events.
    }

    /// <summary>
    /// Called by NightManager when the night ends.
    /// Use for cleanup, state reset, etc.
    /// </summary>
    public virtual void OnNightEnd()
    {
        // Default: nothing. Override in subclasses or use events.
    }
    #endregion Night Lifecycle
}
