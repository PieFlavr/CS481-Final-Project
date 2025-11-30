using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Unified entity controller with behavior system.
/// Works for ALL entity types (enemies, hazards, NPCs, etc.).
/// Only subclass when you need custom logic (Player input, Boss phases).
/// </summary>
public class EntityController : MonoBehaviour
{
    #region Core References

    protected Entity entity;
    protected Rigidbody2D body;
    protected EntitySpellCaster spellCaster;

    #endregion

    #region Behavior System

    [Header("Behaviors")]
    [Tooltip("Behaviors executed by this entity (configured in Inspector or EntityData)")]
    public List<ConditionalBehavior> behaviors = new List<ConditionalBehavior>();

    #endregion

    #region Targeting & Detection

    [Header("Targeting")]
    [Tooltip("Layer mask for targetable entities")]
    [SerializeField] protected LayerMask targetableLayer = -1;

    [Tooltip("Layer mask for hazards")]
    [SerializeField] protected LayerMask hazardLayer;

    protected Entity currentTarget;

    [Header("Auto-Targeting (Optional)")]
    [Tooltip("Automatically target player?")]
    [SerializeField] protected bool autoTargetPlayer = false;

    [Tooltip("Re-target interval (seconds)")]
    [SerializeField] protected float retargetInterval = 0.5f;

    private float lastRetargetTime;

    #endregion

    #region Visual

    [Header("Visual")]
    [Tooltip("Automatically flip sprite based on movement?")]
    [SerializeField] protected bool autoFlipSprite = false;

    protected SpriteRenderer spriteRenderer;
    protected bool isFacingRight = true;

    #endregion

    #region Debug

    [Header("Debug")]
    [Tooltip("Show debug visualization")]
    [SerializeField] protected bool showDebug = false;

    #endregion

    #region Lifecycle

    protected virtual void Awake()
    {
        entity = GetComponent<Entity>();
        body = GetComponent<Rigidbody2D>();
        spellCaster = GetComponent<EntitySpellCaster>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (entity == null)
            Debug.LogError($"EntityController: No Entity on {name}!");
    }

    protected virtual void Start()
    {
        Initialize();
    }

    protected virtual void Update()
    {
        UpdateBehavior();
        UpdateVisuals();
    }

    protected virtual void Initialize()
    {
        // Load behaviors from EntityData if available
        if (entity?.Data != null && entity.Data.defaultBehaviors != null)
        {
            // DEEP COPY each behavior instead of sharing reference
            foreach (var behaviorTemplate in entity.Data.defaultBehaviors)
            {
                if (behaviorTemplate != null)
                {
                    // Create a new instance (deep copy)
                    ConditionalBehavior instancedBehavior = behaviorTemplate.CreateInstance();
                    behaviors.Add(instancedBehavior);
                }
            }
        }

        // Initialize all behaviors
        foreach (var behavior in behaviors)
            behavior?.OnInitialize(entity);

        // Initial targeting
        UpdateTarget();
    }

    private void OnDestroy()
    {
        // Cleanup behaviors
        foreach (var behavior in behaviors)
            behavior?.OnCleanup(entity);
    }

    #endregion

    #region Behavior Execution

    /// <summary>
    /// Executes behaviors by priority. Override for custom logic.
    /// </summary>
    protected virtual void UpdateBehavior()
    {
        // Update target if auto-targeting enabled
        if (autoTargetPlayer)
            UpdateTarget();

        // Execute behaviors
        ExecuteBehaviors();
    }

    /// <summary>
    /// Executes all behaviors in priority order.
    /// </summary>
    protected void ExecuteBehaviors()
    {
        var sorted = behaviors.OrderByDescending(b => b.priority);
        bool blocked = false;

        foreach (var behavior in sorted)
        {
            if (behavior == null || blocked)
                continue;

            if (behavior.ShouldExecute(entity, this))
            {
                behavior.Execute(entity, this, Time.deltaTime);

                if (behavior.blockOthers)
                    blocked = true;
            }
        }
    }

    #endregion

    #region Runtime Behavior Modification (For Status Effects)

    /// <summary>
    /// Adds a behavior at runtime (for status effects, spells, etc.).
    /// </summary>
    public void AddBehavior(ConditionalBehavior behavior)
    {
        if (behavior == null) return;

        behaviors.Add(behavior);
        behavior.OnInitialize(entity);

        if (showDebug)
            Debug.Log($"{entity.EntityName}: Added behavior at runtime");
    }

    /// <summary>
    /// Removes a specific behavior instance.
    /// </summary>
    public bool RemoveBehavior(ConditionalBehavior behavior)
    {
        if (behavior == null) return false;

        behavior.OnCleanup(entity);
        bool removed = behaviors.Remove(behavior);

        if (removed && showDebug)
            Debug.Log($"{entity.EntityName}: Removed behavior");

        return removed;
    }

    /// <summary>
    /// Temporarily replaces all behaviors with a new set (for Confuse, Charm, etc.).
    /// Returns the original behaviors for restoration.
    /// </summary>
    public List<ConditionalBehavior> ReplaceAllBehaviors(List<ConditionalBehavior> newBehaviors)
    {
        // Save originals
        List<ConditionalBehavior> original = new List<ConditionalBehavior>(behaviors);

        // Cleanup old
        foreach (var b in behaviors)
            b?.OnCleanup(entity);

        // Set new
        behaviors = newBehaviors;

        // Initialize new
        foreach (var b in behaviors)
            b?.OnInitialize(entity);

        return original;
    }

    /// <summary>
    /// Restores behaviors (after Charm/Confuse expires).
    /// </summary>
    public void RestoreBehaviors(List<ConditionalBehavior> savedBehaviors)
    {
        // Cleanup current
        foreach (var b in behaviors)
            b?.OnCleanup(entity);

        // Restore
        behaviors = savedBehaviors;

        // Re-initialize
        foreach (var b in behaviors)
            b?.OnInitialize(entity);
    }

    #endregion

    #region Targeting System

    /// <summary>
    /// Gets current target.
    /// </summary>
    public virtual Entity GetTarget()
    {
        return currentTarget;
    }

    /// <summary>
    /// Sets current target.
    /// </summary>
    public virtual void SetTarget(Entity target)
    {
        currentTarget = target;
    }

    /// <summary>
    /// Updates auto-targeting (if enabled).
    /// </summary>
    protected virtual void UpdateTarget()
    {
        if (!autoTargetPlayer)
            return;

        if (Time.time - lastRetargetTime < retargetInterval)
            return;

        lastRetargetTime = Time.time;
        currentTarget = EntityManager.Instance?.GetPlayer();
    }

    /// <summary>
    /// Finds nearest entity of faction within radius.
    /// </summary>
    protected Entity FindNearestEntity(float radius, EntityFaction targetFaction)
    {
        var candidates = EntityManager.Instance?.GetEntitiesByFaction(targetFaction);
        if (candidates == null || candidates.Count == 0)
            return null;

        Entity nearest = null;
        float nearestDist = radius;

        foreach (var candidate in candidates)
        {
            if (candidate == null || candidate == entity)
                continue;

            float dist = Vector2.Distance(transform.position, candidate.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = candidate;
            }
        }

        return nearest;
    }

    /// <summary>
    /// Raycasts to find entity at position.
    /// </summary>
    protected Entity GetEntityAtPosition(Vector2 worldPosition)
    {
        RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero, 0f, targetableLayer);

        if (hit.collider != null)
        {
            Entity hitEntity = hit.collider.GetComponent<Entity>();
            if (hitEntity != null && hitEntity != entity)
                return hitEntity;
        }

        return null;
    }

    #endregion

    #region Movement System

    /// <summary>
    /// Moves entity in direction.
    /// </summary>
    public virtual void Move(Vector2 direction)
    {
        if (body == null || entity == null)
            return;

        body.linearVelocity = direction.normalized * entity.Stats.MoveSpeed;
    }

    /// <summary>
    /// Stops movement.
    /// </summary>
    public virtual void StopMovement()
    {
        if (body != null)
            body.linearVelocity = Vector2.zero;
    }

    /// <summary>
    /// Gets current velocity.
    /// </summary>
    public Vector2 GetVelocity()
    {
        return body != null ? body.linearVelocity : Vector2.zero;
    }

    /// <summary>
    /// Is entity moving?
    /// </summary>
    public bool IsMoving()
    {
        return body != null && body.linearVelocity.sqrMagnitude > 0.01f;
    }

    #endregion

    #region Hazard Detection

    /// <summary>
    /// Is position safe (no hazards)?
    /// </summary>
    public bool IsPositionSafe(Vector2 position, float checkRadius = 0.3f)
    {
        if (hazardLayer == 0)
            return true;

        Collider2D hazard = Physics2D.OverlapCircle(position, checkRadius, hazardLayer);
        return hazard == null;
    }

    /// <summary>
    /// Is path clear of hazards?
    /// </summary>
    public bool IsPathSafe(Vector2 from, Vector2 to, float checkRadius = 0.3f)
    {
        if (hazardLayer == 0)
            return true;

        Vector2 direction = (to - from).normalized;
        float distance = Vector2.Distance(from, to);

        RaycastHit2D hit = Physics2D.CircleCast(from, checkRadius, direction, distance, hazardLayer);
        return hit.collider == null;
    }

    #endregion

    #region Pathfinding Stub

    /// <summary>
    /// Gets path to target (stub for future A*).
    /// </summary>
    public virtual Vector2[] GetPathTo(Vector2 targetPosition)
    {
        // TODO: Implement when PathfindingService exists
        return null;
    }

    #endregion

    #region Spell Casting

    /// <summary>
    /// Casts spell at target.
    /// </summary>
    public bool CastSpellAtTarget(int slotIndex, Entity target)
    {
        if (spellCaster == null || target == null)
            return false;

        bool success = spellCaster.CastSpell(slotIndex, target);

        if (showDebug)
        {
            if (success)
                Debug.Log($"{name}: Cast spell {slotIndex} on {target.EntityName}");
            else
                Debug.LogWarning($"{name}: Failed to cast spell {slotIndex}");
        }

        return success;
    }

    /// <summary>
    /// Casts spell at position.
    /// </summary>
    public bool CastSpellAtPosition(int slotIndex, Vector2 position)
    {
        if (spellCaster == null)
            return false;

        bool success = spellCaster.CastSpellAtPosition(slotIndex, position);

        if (showDebug)
        {
            if (success)
                Debug.Log($"{name}: Cast spell {slotIndex} at {position}");
            else
                Debug.LogWarning($"{name}: Failed to cast spell {slotIndex}");
        }

        return success;
    }

    /// <summary>
    /// Can spell be cast?
    /// </summary>
    public bool CanCastSpell(int slotIndex)
    {
        return spellCaster != null && spellCaster.CanCastSpell(slotIndex);
    }

    #endregion

    #region Visual Updates

    protected virtual void UpdateVisuals()
    {
        if (autoFlipSprite)
            UpdateSpriteFlip();
    }

    protected void UpdateSpriteFlip()
    {
        if (spriteRenderer == null || body == null)
            return;

        if (body.linearVelocity.x < -0.01f && !isFacingRight)
            Flip();
        else if (body.linearVelocity.x > 0.01f && isFacingRight)
            Flip();
    }

    protected void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    #endregion

    #region Collision Notifications

    /// <summary>
    /// Called by Entity when collision detected.
    /// </summary>
    public virtual void OnCollisionDetected(Collider2D collision)
    {
        // Override if needed
    }

    #endregion

    #region Debug

    protected virtual void OnDrawGizmos()
    {
        if (!showDebug)
            return;

        // Draw target line
        if (currentTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, currentTarget.transform.position);
        }
    }

    #endregion
}