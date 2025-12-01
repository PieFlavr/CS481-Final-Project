using UnityEngine;

/// <summary>
/// Player entity that integrates with the entity system.
/// Thin wrapper around `BaseEntity` that owns a `StatsComponent` and player-specific data.
/// </summary>
public class PlayerEntity : BaseEntity
{
    [SerializeField] private PlayerData playerData;

    private StatsComponent statsComponent;

    public override EntityType Type => EntityType.Player;
    public PlayerData PlayerData => playerData;
    public StatsComponent Stats => statsComponent;

    protected override void Awake()
    {
        base.Awake();

        statsComponent = GetComponent<StatsComponent>();
        if (statsComponent == null)
            statsComponent = gameObject.AddComponent<StatsComponent>();

        if (playerData != null)
        {
            statsComponent.InitializeFromEntityData(playerData);
        }
    }

    public override void Initialize(EntityData data)
    {
        if (data is PlayerData pd)
        {
            playerData = pd;
            if (statsComponent == null)
                statsComponent = GetComponent<StatsComponent>() ?? gameObject.AddComponent<StatsComponent>();

            statsComponent.InitializeFromEntityData(pd);
        }
    }

    public override void Die()
    {
        if (!IsAlive) return;
        // Player-specific death handling could go here (respawn, UI, etc.)
        base.Die();
    }

    public void ResetPlayerState()
    {
        statsComponent?.ResetHealth();
    }
}
