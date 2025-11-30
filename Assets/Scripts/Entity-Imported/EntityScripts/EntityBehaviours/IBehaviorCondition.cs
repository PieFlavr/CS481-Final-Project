using UnityEngine;

/// <summary>
/// Interface for behavior conditions.
/// Determines WHEN a behavior should execute.
/// Conditions are combined with AND logic.
/// </summary>
public interface IBehaviorCondition
{
    /// <summary>
    /// Checks if this condition is currently met.
    /// </summary>
    bool IsMet(Entity entity, EntityController controller);

    /// <summary>
    /// Called when condition is first initialized.
    /// </summary>
    void OnInitialize(Entity entity);

    /// <summary>
    /// Creates a deep copy of this condition.
    /// Must copy all serialized fields but NOT runtime state.
    /// </summary>
    IBehaviorCondition Clone();
}