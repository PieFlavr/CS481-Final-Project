/// <summary>
/// Entity type enumeration for manager queries and filtering.
/// </summary>
public enum EntityType
{
    Player,
    Enemy,
    Hazard,
    EffectArea
}

/// <summary>
/// Core interface for all trackable entities in the game.
/// Minimal contract: identity, position, lifecycle.
/// All other data should be accessed via GetComponent<T>() or specific accessors.
/// </summary>
public interface IEntity
{
    /// <summary>
    /// Unique identifier for this entity instance.
    /// </summary>
    int EntityID { get; }

    /// <summary>
    /// Type of entity (Player, Enemy, Hazard, etc.).
    /// Used for EntityManager filtering.
    /// </summary>
    EntityType Type { get; }

    /// <summary>
    /// World position of this entity.
    /// </summary>
    UnityEngine.Vector3 Position { get; }

    /// <summary>
    /// Whether this entity is currently alive/active.
    /// </summary>
    bool IsAlive { get; }

    /// <summary>
    /// Despawn this entity (called by EntityManager or cleanup systems).
    /// </summary>
    void Despawn();
}
