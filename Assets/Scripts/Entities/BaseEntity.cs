using UnityEngine;

/// <summary>
/// Abstract base class for scene entities.
/// Implements `IEntity` and centralizes common lifecycle, ID, and registration with `EntityManager`.
/// Subclasses should override `Type` and can override lifecycle stuff like `Initialize`/`Die`/`Despawn`.
/// </summary>
public abstract class BaseEntity : MonoBehaviour, IEntity
{
    #region Fields and Properties
    protected int entityId;
    protected bool isAlive = true;

    public int EntityID => entityId;
    public abstract EntityType Type { get; }
    public Vector3 Position => transform.position;
    public bool IsAlive => isAlive;
    #endregion Fields and Properties



    #region Unity Methods
    protected virtual void Awake()
    {
        entityId = GetInstanceID();
    }

    protected virtual void Start()
    {
        EntityManager.Instance?.RegisterEntity(this);
    }

    protected virtual void OnDestroy()
    {
        EntityManager.Instance?.UnregisterEntity(this);
    }
    #endregion Unity Methods



    #region IEntity Implementations
    /// <summary>
    /// Optional initialization for other systems/data uses.
    /// Subclasses should override and call base.Initialize(data) if they need base behavior.
    /// </summary>
    public virtual void Initialize(EntityData data)
    {
        // Default: nothing. Provided for explicit initialization instead of heavy Awake logic.
    }

    /// <summary>
    /// Despawn hook for managers. Default behavior calls Die().
    /// </summary>
    public virtual void Despawn()
    {
        Die();
    }

    /// <summary>
    /// Default Die implementat: mark not alive and then destroy GameObject.
    /// Subclasses should override to play VFX/animations or other logic before calling base.Die().
    /// </summary>
    public virtual void Die()
    {
        if (!isAlive) return;
        isAlive = false;
        Destroy(gameObject);
    }
    #endregion IEntity Implementations
}
