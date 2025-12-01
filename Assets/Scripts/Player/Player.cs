using UnityEngine;

/// <summary>
/// Legacy player MonoBehaviour converted to integrate with the entity system.
/// Now inherits `PlayerEntity` (thin wrapper) so the prefab/scene can keep `Player` script attached.
/// Removes singleton behavior; use `PlayerManager` for global game logic.
/// </summary>
public class Player : PlayerEntity
{
    // Compatibility helpers so other code can still call old methods.
    public void DoDamage(float damage)
    {
        Stats?.TakeDamage(damage);
    }

    public float GetHealth()
    {
        return Stats != null ? Stats.CurrentHealth : 0f;
    }

    public void ResetHealth()
    {
        Stats?.ResetHealth();
    }

    // Keep Awake/Start behavior minimal; PlayerEntity/ BaseEntity handle registration and initialization.
    protected override void Awake()
    {
        base.Awake();
    }
}
