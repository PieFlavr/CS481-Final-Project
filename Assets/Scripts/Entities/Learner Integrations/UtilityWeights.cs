using UnityEngine;

/// <summary>
/// Weights for per-target-class utility evaluation.
/// Learner tunes these to prioritize different target types.
/// </summary>
[System.Serializable]
public struct UtilityWeights
{
    [SerializeField] private float threat;
    [SerializeField] private float distance;
    [SerializeField] private float priority;

    public float Threat => threat;
    public float Distance => distance;
    public float Priority => priority;

    public UtilityWeights(float threat, float distance, float priority)
    {
        this.threat = threat;
        this.distance = distance;
        this.priority = priority;
    }

    /// <summary>
    /// Compute utility for a target given normalized inputs.
    /// </summary>
    public float ComputeUtility(float normalizedThreat, float normalizedDistance, float normalizedPriority)
    {
        return (threat * normalizedThreat) 
             + (distance * normalizedDistance) 
             + (priority * normalizedPriority);
    }

    /// <summary>
    /// Set new weights (for learner mutation).
    /// </summary>
    public void SetWeights(float newThreat, float newDistance, float newPriority)
    {
        threat = newThreat;
        distance = newDistance;
        priority = newPriority;
    }
}
