using UnityEngine;

/// <summary>
/// Complete set of learnable FSM parameters for a single archetype.
/// Stores state thresholds and target utility weights.
/// Learner mutates these via IParameterVector interface.
/// Designer-friendly: all params visible in Inspector when attached to ArchetypeData.
/// </summary>
[System.Serializable]
public class FSMParameterSet
{
    [Header("State Thresholds")]
    [Tooltip("Distance at which enemy begins aggression toward targets")]
    public LearnableParameter aggroDistance = new LearnableParameter(15f, 5f, 30f, true, "Aggro detection range");
    
    [Tooltip("Health % threshold below which enemy flees")]
    public LearnableParameter fleeHealthThreshold = new LearnableParameter(0.3f, 0.1f, 0.9f, true, "Flee when health drops below this %");
    
    [Tooltip("Maximum duration to chase a target before giving up")]
    public LearnableParameter chaseDurationMax = new LearnableParameter(8f, 2f, 30f, true, "Max chase time in seconds");
    
    [Tooltip("Distance from group center before unit returns to swarm")]
    public LearnableParameter distanceFromGroupThreshold = new LearnableParameter(15f, 5f, 25f, true, "Return to group beyond this distance");
    
    [Tooltip("Cooldown between attacks")]
    public LearnableParameter attackCooldown = new LearnableParameter(1.5f, 0.5f, 3f, true, "Attack cooldown in seconds");

    [Header("Target Utility Weights")]
    [Tooltip("Utility weights for evaluating player as target")]
    public UtilityWeights playerWeights = new UtilityWeights(0.6f, 0.3f, 0.1f);
    
    [Tooltip("Utility weights for ranged threats (turrets, drones)")]
    public UtilityWeights rangedWeights = new UtilityWeights(0.8f, 0.2f, 0.0f);
    
    [Tooltip("Utility weights for melee threats (traps)")]
    public UtilityWeights meleeWeights = new UtilityWeights(0.5f, 0.4f, 0.1f);
    
    [Tooltip("Utility weight for returning to group locus")]
    public UtilityWeights groupLocusWeights = new UtilityWeights(0.0f, 1.0f, 0.0f);

    /// <summary>
    /// Convert all parameters to a flat array for learner mutation.
    /// Order must match FromArray() implementation.
    /// </summary>
    public float[] ToArray()
    {
        return new float[]
        {
            // State thresholds
            aggroDistance.Value,
            fleeHealthThreshold.Value,
            chaseDurationMax.Value,
            distanceFromGroupThreshold.Value,
            attackCooldown.Value,
            
            // Player weights
            playerWeights.Threat,
            playerWeights.Distance,
            playerWeights.Priority,
            
            // Ranged weights
            rangedWeights.Threat,
            rangedWeights.Distance,
            rangedWeights.Priority,
            
            // Melee weights
            meleeWeights.Threat,
            meleeWeights.Distance,
            meleeWeights.Priority,
            
            // Group locus weights
            groupLocusWeights.Threat,
            groupLocusWeights.Distance,
            groupLocusWeights.Priority
        };
    }

    /// <summary>
    /// Apply learner-mutated values from flat array.
    /// Only updates active parameters.
    /// </summary>
    public void FromArray(float[] values)
    {
        if (values.Length < 17)
        {
            Debug.LogError("[FSMParameterSet] FromArray() requires at least 17 values!");
            return;
        }

        int idx = 0;
        
        // State thresholds
        if (aggroDistance.IsActive) aggroDistance.SetValue(values[idx]);
        idx++;
        
        if (fleeHealthThreshold.IsActive) fleeHealthThreshold.SetValue(values[idx]);
        idx++;
        
        if (chaseDurationMax.IsActive) chaseDurationMax.SetValue(values[idx]);
        idx++;
        
        if (distanceFromGroupThreshold.IsActive) distanceFromGroupThreshold.SetValue(values[idx]);
        idx++;
        
        if (attackCooldown.IsActive) attackCooldown.SetValue(values[idx]);
        idx++;
        
        // Player weights
        playerWeights.SetWeights(values[idx], values[idx + 1], values[idx + 2]);
        idx += 3;
        
        // Ranged weights
        rangedWeights.SetWeights(values[idx], values[idx + 1], values[idx + 2]);
        idx += 3;
        
        // Melee weights
        meleeWeights.SetWeights(values[idx], values[idx + 1], values[idx + 2]);
        idx += 3;
        
        // Group locus weights
        groupLocusWeights.SetWeights(values[idx], values[idx + 1], values[idx + 2]);
    }

    /// <summary>
    /// Reset blinded (inactive) parameters to defaults.
    /// Called when capabilities unlock to prevent cargo cult gradients.
    /// </summary>
    public void ResetInactiveParameters()
    {
        if (!aggroDistance.IsActive) aggroDistance.ResetToDefault(15f);
        if (!fleeHealthThreshold.IsActive) fleeHealthThreshold.ResetToDefault(0.3f);
        if (!chaseDurationMax.IsActive) chaseDurationMax.ResetToDefault(8f);
        if (!distanceFromGroupThreshold.IsActive) distanceFromGroupThreshold.ResetToDefault(15f);
        if (!attackCooldown.IsActive) attackCooldown.ResetToDefault(1.5f);
    }
}
