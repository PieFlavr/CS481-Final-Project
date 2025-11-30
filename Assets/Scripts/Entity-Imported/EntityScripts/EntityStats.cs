using UnityEngine;

/// <summary>
/// Runtime stat tracking for entities.
/// Handles stat modifications from buffs/debuffs/equipment.
/// </summary>
[System.Serializable]
public class EntityStats
{
    // Base stats (from EntityData)
    private float baseWeight;
    private float baseMoveSpeed;

    // Runtime modifiers (for buffs/debuffs to modify later)
    private float weightModifier = 1f;
    private float moveSpeedModifier = 1f;

    // The 'real' stats after applying modifiers are obtain via thse properties

    public float Weight => baseWeight * weightModifier;
    public float MoveSpeed => baseMoveSpeed * moveSpeedModifier;

    /// <summary>
    /// Initializes stats from EntityData.
    /// </summary>
    public EntityStats(EntityData data)
    {
        baseWeight = data.baseWeight;
        baseMoveSpeed = data.baseMoveSpeed;
    }

    /// <summary>
    /// Modifies the weight multiplier (for status effects/buffs).
    /// </summary>
    public void ModifyWeight(float multiplier)
    {
        weightModifier = Mathf.Max(0.1f, multiplier); // Prevent negative/zero weight
    }

    /// <summary>
    /// Modifies the move speed multiplier (for status effects/buffs).
    /// </summary>
    public void ModifyMoveSpeed(float multiplier)
    {
        moveSpeedModifier = Mathf.Max(0f, multiplier); // Allow zero (frozen) but not negative
    }

    /// <summary>
    /// Resets all modifiers to default values.
    /// </summary>
    public void ResetModifiers()
    {
        weightModifier = 1f;
        moveSpeedModifier = 1f;
    }

    /// <summary>
    /// Gets debug string of current stats.
    /// </summary>
    public override string ToString()
    {
        return $"Weight: {Weight:F2} (base: {baseWeight}, mod: {weightModifier:F2}), " +
               $"MoveSpeed: {MoveSpeed:F2} (base: {baseMoveSpeed}, mod: {moveSpeedModifier:F2})";
    }
}