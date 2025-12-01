using System;
using System.Collections.Generic;

/// <summary>
/// Responsible for health state and queued health changes.
/// Isolated from other stats for clarity.
/// </summary>
public class HealthMeter
{
    #region Fields and Properties

    public float MaxHealth { get; private set; }
    public float CurrentHealth { get; private set; }

    // NOTE (L): Queue fixes some issue in my experience from an older Entity System.
    // If health changes (damage/heal) are applied mid-frame, goofy race conditions happen and heals/damages get negated.
    // This in theory should fix that by queuing health changes and applying them at frame end as opposed to immediately.
    private Queue<float> healthChangeQueue = new Queue<float>(); // positive = heal, negative = damage

    public bool IsDead => CurrentHealth <= 0f;

    #region Events
    public event Action<float, float> OnHealthChanged; // old, new
    public event Action OnDied;
    #endregion Events

    public bool HasQueuedChanges => healthChangeQueue.Count > 0;

    #endregion Fields and Properties



    #region Utility Methods
    public void Initialize(float maxHealth)
    {
        MaxHealth = maxHealth;
        CurrentHealth = MaxHealth;
    }
    #endregion Utility Methods



    #region Health Methods
    public void QueueHealthChange(float amount)
    {
        healthChangeQueue.Enqueue(amount);
    }

    public void TakeDamage(float damageAmount)
    {
        QueueHealthChange(-damageAmount);
    }

    public void Heal(float healAmount)
    {
        QueueHealthChange(healAmount);
    }

    /// <summary>
    /// Process queued health changes. Returns true if entity died during processing.
    /// </summary>
    public bool ProcessHealthQueue()
    {
        while (healthChangeQueue.Count > 0)
        {
            if (CurrentHealth <= 0f)
                break;

            float change = healthChangeQueue.Dequeue();
            float old = CurrentHealth;
            float next = CurrentHealth + change;
            if (next > MaxHealth) next = MaxHealth;
            if (next < 0f) next = 0f;
            CurrentHealth = next;

            OnHealthChanged?.Invoke(old, CurrentHealth);

            if (CurrentHealth <= 0f)
            {
                OnDied?.Invoke();
                return true;
            }
        }

        return false;
    }

    public void SetMaxHealth(float newMax)
    {
        MaxHealth = newMax;
        if (CurrentHealth > MaxHealth) CurrentHealth = MaxHealth;
    }

    public float GetHealthPercent()
    {
        return MaxHealth > 0f ? CurrentHealth / MaxHealth : 0f;
    }

    public void ResetHealth()
    {
        CurrentHealth = MaxHealth;
    }
    #endregion Health Methods
}
