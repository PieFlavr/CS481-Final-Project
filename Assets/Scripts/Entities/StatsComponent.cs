using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

/// <summary>
/// Holds and manages entity stats (health, damage, speed).
/// Broadcasts stat changes to EntityEventManager for learner observation.
/// Works with any entity type (enemies, player, turrets, etc.).
/// Includes damage/heal queue to prevent mid-frame state inconsistencies.
/// </summary>
public class StatsComponent : MonoBehaviour
{
    private HealthMeter healthStat = new HealthMeter();
    private CombatStats combatStats = new CombatStats();
    private IEntity entity;
    private Action onDiedHandler;

    // Telemetry events exposed for systems and learner integrations
    public event Action<float, float> OnHealthChanged; // old, new
    public event Action<float> OnHealthPercentChanged; // normalized 0..1
    public event Action OnDied;

    [Header("Designer Events")]
    [SerializeField] private UnityEvent onDiedUnityEvent;
    [SerializeField] private UnityEvent<float> onHealthPercentChangedEvent;

    public float MaxHealth => healthStat.MaxHealth;
    public float CurrentHealth => healthStat.CurrentHealth;
    public float Speed => combatStats.Speed;
    public float Damage => combatStats.BaseDamage;

    private void Awake()
    {
        entity = GetComponent<IEntity>();
        if (entity == null)
        {
            Debug.LogError("[StatsComponent] Attached to GameObject without IEntity component!");
        }

        // When the HealthMeter signals death, invoke UnityEvents, public events, then call entity.Die()
        healthStat.OnDied += () => {
            try { onDiedUnityEvent?.Invoke(); } catch (Exception) { }
            try { OnDied?.Invoke(); } catch (Exception) { }

            // Finally call entity Die hook if available
            if (entity is BaseEntity be)
            {
                try { be.Die(); } catch (Exception) { }
            }
        };

        // Forward health change events
        healthStat.OnHealthChanged += (oldVal, newVal) => {
            try { OnHealthChanged?.Invoke(oldVal, newVal); } catch (Exception) { }
            try { onHealthPercentChangedEvent?.Invoke(healthStat.GetHealthPercent()); } catch (Exception) { }
            try { OnHealthPercentChanged?.Invoke(healthStat.GetHealthPercent()); } catch (Exception) { }
        };
    }

    private void OnDestroy()
    {
        if (onDiedHandler != null)
            healthStat.OnDied -= onDiedHandler;
    }

    private void LateUpdate()
    {
        // Process queued health changes at frame end
        if (healthStat.HasQueuedChanges)
        {
            healthStat.ProcessHealthQueue();
        }
    }

    /// <summary>
    /// Initialize stats from entity data.
    /// Called by spawners after instantiation.
    /// </summary>
    public void InitializeFromArchetype(ArchetypeData archetypeData)
    {
        if (archetypeData == null)
        {
            Debug.LogError("[StatsComponent] Cannot initialize with null archetype!");
            return;
        }

        healthStat.Initialize(archetypeData.MaxHealth);
        combatStats.Initialize(archetypeData.Speed, archetypeData.Damage);
    }

    /// <summary>
    /// Initialize stats from any entity data (player, archetype, utility, etc).
    /// </summary>
    public void InitializeFromEntityData(EntityData entityData)
    {
        if (entityData == null)
        {
            Debug.LogError("[StatsComponent] Cannot initialize with null entity data!");
            return;
        }

        healthStat.Initialize(entityData.MaxHealth);
        combatStats.Initialize(entityData.Speed, entityData.Damage);
    }

    /// <summary>
    /// Queue a health change (positive for heal, negative for damage).
    /// Applied at frame end to prevent mid-frame inconsistencies.
    /// Checks for death between changes.
    /// </summary>
    public void QueueHealthChange(float amount)
    {
        healthStat.QueueHealthChange(amount);
    }

    /// <summary>
    /// Apply damage immediately (bypasses queue).
    /// Use sparingly—prefer QueueHealthChange for gameplay logic.
    /// </summary>
    public void TakeDamage(float damageAmount)
    {
        healthStat.TakeDamage(damageAmount);
    }

    /// <summary>
    /// Apply healing immediately (bypasses queue).
    /// Use sparingly—prefer QueueHealthChange for gameplay logic.
    /// </summary>
    public void Heal(float healAmount)
    {
        healthStat.Heal(healAmount);
    }

    /// <summary>
    /// Process all queued health changes.
    /// Broadcasts stat changes and checks for death after each change.
    /// </summary>
    private void ProcessHealthQueue()
    {
        // Forward to health model for compatibility.
        if (healthStat.HasQueuedChanges)
            healthStat.ProcessHealthQueue();
    }

    /// <summary>
    /// Modify max health (e.g., via upgrade).
    /// Broadcasts stat change event.
    /// </summary>
    public void SetMaxHealth(float newMaxHealth)
    {
        healthStat.SetMaxHealth(newMaxHealth);
    }

    /// <summary>
    /// Modify speed (e.g., via upgrade or buff).
    /// Broadcasts stat change event.
    /// </summary>
    public void SetSpeed(float newSpeed)
    {
        combatStats.SetSpeed(newSpeed);
    }

    /// <summary>
    /// Modify damage (e.g., via upgrade).
    /// Broadcasts stat change event.
    /// </summary>
    public void SetDamage(float newDamage)
    {
        combatStats.SetDamage(newDamage);
    }

    /// <summary>
    /// Check if this entity is dead.
    /// </summary>
    public bool IsDead()
    {
        return healthStat.IsDead;
    }

    /// <summary>
    /// Get health as percentage (0 to 1).
    /// Useful for UI or FSM decisions.
    /// </summary>
    public float GetHealthPercent()
    {
        return healthStat.GetHealthPercent();
    }

    /// <summary>
    /// Reset health to full.
    /// Called between rounds or on respawn.
    /// </summary>
    public void ResetHealth()
    {
        healthStat.ResetHealth();
    }
}
