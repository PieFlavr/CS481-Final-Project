using UnityEngine;

/// <summary>
/// ScriptableObject defining player configuration.
/// Extends EntityData with player-specific progression fields.
/// Not learnable—player upgrades are deterministic choices.
/// </summary>
[CreateAssetMenu(fileName = "PlayerData", menuName = "Entities/Player Data", order = 2)]
public class PlayerData : EntityData
{
    [Header("Player-Specific")]
    // NOTE(L): This is 1000% placeholder, just needed to test it. 
    [SerializeField] private int startingResources = 100;
    [SerializeField] private float resourceRegenPerNight = 10f;

    public int StartingResources => startingResources;
    public float ResourceRegenPerNight => resourceRegenPerNight;
}
