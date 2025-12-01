using UnityEngine;

/// <summary>
/// ScriptableObject defining a learnable swarm archetype.
/// Extends EntityData with FSM-specific configuration and parameters.
/// Works for any learnable archetype: Tank, Rusher, Sniper, etc.
/// </summary>
[CreateAssetMenu(fileName = "Archetype_", menuName = "Entities/Archetype", order = 3)]
public class ArchetypeData : EntityData
{
    [Header("Archetype Identity")]
    [SerializeField] private string archetypeId = "generic"; // "tank", "rusher", "sniper", etc.

    [Header("FSM Configuration")]
    [SerializeField] private FSMParameterSet fsmParameters = new FSMParameterSet();
    
    [Header("Target Detection")]
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private float sensorRange = 30f;

    public string ArchetypeId => archetypeId;
    public FSMParameterSet FSMParameters => fsmParameters;
    public LayerMask TargetLayer => targetLayer;
    public float SensorRange => sensorRange;

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(archetypeId)) archetypeId = "generic";
        if (sensorRange < 0f) sensorRange = 0f;
    }

    /// <summary>
    /// Clone FSM parameters for runtime mutation.
    /// Learner mutates the clone, not the asset.
    /// </summary>
    public FSMParameterSet CloneFSMParameters()
    {
        // Deep copy to prevent asset mutation
        var clone = new FSMParameterSet();
        clone.FromArray(fsmParameters.ToArray());
        return clone;
    }
}
