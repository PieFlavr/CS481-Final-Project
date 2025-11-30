using UnityEngine;
using System;

/// <summary>
/// Manages health and mana for entities.
/// Should replace the old HealthManager!
/// Easily extensible for additional resources later.
/// Supports regeneration rates for health and mana.
/// </summary>
[System.Serializable]
public class EntityResources
{
    // Health
    [SerializeField] private float maxHealth;
    [SerializeField] private float currentHealth;

    // Mana
    [SerializeField] private float maxMana;
    [SerializeField] private float currentMana;

    // Regeneration
    [SerializeField] private float healthRegenRate = 0f; // Health per second
    [SerializeField] private float manaRegenRate = 10f;  // Mana per second

    // Events
    public event Action<float, float> OnHealthChanged; // (current, max)
    public event Action<float, float> OnManaChanged;   // (current, max)
    public event Action OnHealthDepleted;

    // Properties
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public float MaxMana => maxMana;
    public float CurrentMana => currentMana;
    public float HealthRegenRate => healthRegenRate;
    public float ManaRegenRate => manaRegenRate;

    /// <summary>
    /// Gets the health percentage (0-1).
    /// </summary>
    public float HealthPercent => maxHealth > 0 ? currentHealth / maxHealth : 0f;

    /// <summary>
    /// Gets the mana percentage (0-1).
    /// </summary>
    public float ManaPercent => maxMana > 0 ? currentMana / maxMana : 0f;

    /// <summary>
    /// Initializes resources with base values from EntityData.
    /// </summary>
    public EntityResources(float baseHealth, float baseMana, float healthRegenRate = 0f, float manaRegenRate = 10f)
    {
        maxHealth = baseHealth;
        currentHealth = baseHealth;
        maxMana = baseMana;
        currentMana = baseMana;
        this.healthRegenRate = healthRegenRate;
        this.manaRegenRate = manaRegenRate;
    }

    /// <summary>
    /// Modifies health by the specified amount (positive = heal, negative = damage).
    /// </summary>
    public void ModifyHealth(float amount)
    {
        float previousHealth = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);

        // Only fire events if health actually changed
        if (!Mathf.Approximately(currentHealth, previousHealth))
        {
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0)
            {
                OnHealthDepleted?.Invoke();
            }
        }
    }

    /// <summary>
    /// Modifies mana by the specified amount (positive = restore, negative = consume).
    /// </summary>
    public void ModifyMana(float amount)
    {
        float previousMana = currentMana;
        currentMana = Mathf.Clamp(currentMana + amount, 0, maxMana);

        // Only fire events if mana actually changed
        if (!Mathf.Approximately(currentMana, previousMana))
        {
            OnManaChanged?.Invoke(currentMana, maxMana);
        }
    }

    /// <summary>
    /// Sets the maximum health and optionally adjusts current health.
    /// </summary>
    public void SetMaxHealth(float newMax, bool adjustCurrent = true)
    {
        maxHealth = Mathf.Max(1, newMax); // Minimum 1 HP

        if (adjustCurrent)
        {
            currentHealth = Mathf.Min(currentHealth, maxHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }

    /// <summary>
    /// Sets the maximum mana and optionally adjusts current mana.
    /// </summary>
    public void SetMaxMana(float newMax, bool adjustCurrent = true)
    {
        maxMana = Mathf.Max(0, newMax);

        if (adjustCurrent)
        {
            currentMana = Mathf.Min(currentMana, maxMana);
            OnManaChanged?.Invoke(currentMana, maxMana);
        }
    }

    /// <summary>
    /// Fully restores health and mana.
    /// </summary>
    public void FullRestore()
    {
        ModifyHealth(maxHealth - currentHealth);
        ModifyMana(maxMana - currentMana);
    }

    /// <summary>
    /// Regenerates health and mana over time.
    /// Call this once per frame, passing deltaTime.
    /// </summary>
    public void Regenerate(float deltaTime)
    {
        if (healthRegenRate > 0f && currentHealth < maxHealth)
        {
            ModifyHealth(healthRegenRate * deltaTime);
        }
        if (manaRegenRate > 0f && currentMana < maxMana)
        {
            ModifyMana(manaRegenRate * deltaTime);
        }
    }

    /// <summary>
    /// Gets debug string of current resources.
    /// </summary>
    public override string ToString()
    {
        return $"Health: {currentHealth:F1}/{maxHealth:F1} ({HealthPercent:P0}), " +
               $"Mana: {currentMana:F1}/{maxMana:F1} ({ManaPercent:P0})";
    }
}