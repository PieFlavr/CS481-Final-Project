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

    public string ArchetypeId => archetypeId;
    
    //Add FSM configuration fields here as needed

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(archetypeId)) archetypeId = "generic";
    }
}
