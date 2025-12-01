using UnityEngine;

/// <summary>
/// Placeholder utility entity for traps, turrets, or other non-enemy/player entities.
/// Extend as needed later.
/// </summary>
public class UtilityEntity : BaseEntity
{
    public override EntityType Type => EntityType.Hazard;

    // Add utility-specific fields or components later.
}
