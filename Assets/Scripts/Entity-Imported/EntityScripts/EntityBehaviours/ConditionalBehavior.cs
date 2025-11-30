using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Wrapper combining a behavior and its conditions.
/// Each entity gets its own deep copy to prevent shared state issues.
/// </summary>
[System.Serializable]
public class ConditionalBehavior
{
    [Header("What Does This Do?")]
    [Tooltip("The action this behavior performs")]
    [SerializeReference]
    public EntityBehaviorBase behavior;

    [Header("When Should It Happen?")]
    [Tooltip("ALL of these conditions must be true (AND logic)")]
    [SerializeReference]
    public List<IBehaviorCondition> conditions = new List<IBehaviorCondition>();

    [Header("Execution Settings")]
    [Tooltip("Higher priority = executes first")]
    [Range(0, 100)]
    public int priority = 50;

    [Tooltip("Prevent lower-priority behaviors from running?")]
    public bool blockOthers = false;

    /// <summary>
    /// Creates a deep copy of this conditional behavior for a new entity instance.
    /// Ensures each entity has its own behavior state (cooldowns, timers, cached targets).
    /// </summary>
    public ConditionalBehavior CreateInstance()
    {
        ConditionalBehavior copy = new ConditionalBehavior
        {
            priority = this.priority,
            blockOthers = this.blockOthers
        };

        // Deep copy behavior using inherited Clone()
        if (behavior != null)
        {
            copy.behavior = behavior.Clone();
        }

        // Deep copy all conditions
        copy.conditions = new List<IBehaviorCondition>();
        foreach (var condition in conditions)
        {
            if (condition != null)
            {
                copy.conditions.Add(condition.Clone());
            }
        }

        return copy;
    }

    public void OnInitialize(Entity entity)
    {
        behavior?.OnInitialize(entity);
        foreach (var condition in conditions)
            condition?.OnInitialize(entity);
    }

    public bool ShouldExecute(Entity entity, EntityController controller)
    {
        foreach (var condition in conditions)
        {
            if (condition != null && !condition.IsMet(entity, controller))
                return false;
        }
        return true;
    }

    public void Execute(Entity entity, EntityController controller, float deltaTime)
    {
        behavior?.Execute(entity, controller, deltaTime);
    }

    public void OnCleanup(Entity entity)
    {
        behavior?.OnCleanup(entity);
    }
}