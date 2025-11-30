using UnityEngine;

/// <summary>
/// Base class for all entity behaviors.
/// Provides shared state, lifecycle management, and automatic cloning.
/// Use this instead of IEntityBehavior for most behaviors.
/// </summary>
[System.Serializable]
public abstract class EntityBehaviorBase
{
    #region Shared State

    /// <summary>
    /// Cached target entity (usually player).
    /// Automatically initialized in OnInitialize().
    /// </summary>
    protected Entity cachedTarget;

    #endregion

    #region Lifecycle

    /// <summary>
    /// Called when behavior is first added to an entity.
    /// Override to add custom initialization.
    /// </summary>
    public virtual void OnInitialize(Entity entity)
    {
        // Default: Cache player as target
        cachedTarget = EntityManager.Instance?.GetPlayer();
    }

    /// <summary>
    /// Called when behavior is removed from an entity.
    /// Override to add cleanup logic.
    /// </summary>
    public virtual void OnCleanup(Entity entity)
    {
        cachedTarget = null;
    }

    #endregion

    #region Execution

    /// <summary>
    /// Executes this behavior's logic every frame.
    /// MUST be implemented by subclasses.
    /// </summary>
    public abstract void Execute(Entity entity, EntityController controller, float deltaTime);

    #endregion

    #region Cloning

    /// <summary>
    /// Creates a shallow copy of this behavior.
    /// Works automatically for most behaviors using MemberwiseClone.
    /// Override if you need deep cloning of complex fields (lists, arrays, etc.).
    /// </summary>
    public virtual EntityBehaviorBase Clone()
    {
        // MemberwiseClone creates shallow copy of all fields
        return (EntityBehaviorBase)this.MemberwiseClone();
    }

    #endregion
}