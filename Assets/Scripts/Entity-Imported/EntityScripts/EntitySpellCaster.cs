using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages spell casting for an entity.
/// Handles spell loadout, casting, cooldowns, and mana consumption.
/// </summary>
public class EntitySpellCaster : MonoBehaviour
{
    [Header("Spell Loadout")]
    [Tooltip("Maximum number of spells this entity can have equipped")]
    public int maxSpellSlots = 4;

    [Tooltip("Currently equipped spells")]
    public List<Spell> equippedSpells = new List<Spell>();

    [Header("Casting State")]
    [Tooltip("Can this entity cast spells right now?")]
    private bool canCast = true;

    [Tooltip("Reference to the entity this spell caster belongs to")]
    private Entity entity;

    [Header("Spell Holders (Optional)")]
    [Tooltip("SpellHolder components to load spells from on Start")]
    public SpellHolder[] spellHolders;

    private void Awake()
    {
        entity = GetComponent<Entity>();
        if (entity == null)
        {
            Debug.LogError("EntitySpellCaster: No Entity component found!");
        }
    }

    private void Start()
    {
        // Load spells from SpellHolders first
        if (spellHolders != null && spellHolders.Length > 0)
        {
            for (int i = 0; i < spellHolders.Length && i < maxSpellSlots; i++)
            {
                if (spellHolders[i] != null)
                {
                    Spell spell = spellHolders[i].GetSpell();
                    EquipSpell(spell, i);
                    Debug.Log($"EntitySpellCaster: Loaded spell '{spell.spellName}' from SpellHolder into slot {i}");
                }
            }
        }

        // Then equip any manually assigned spells
        foreach (var spell in equippedSpells)
        {
            if (spell != null)
            {
                spell.OnEquip(entity);
            }
        }
    }

    private void Update()
    {
        // Update all equipped spells (for duration/passive catalysts)
        foreach (var spell in equippedSpells)
        {
            if (spell != null && spell.isEquipped)
            {
                spell.OnUpdate(entity, Time.deltaTime);
            }
        }
    }

    #region Spell Casting

    /// <summary>
    /// Casts a spell by index on a target.
    /// </summary>
    public bool CastSpell(int spellIndex, Entity target = null)
    {
        if (!CanCastSpell(spellIndex))
            return false;

        Spell spell = equippedSpells[spellIndex];
        return spell.Cast(entity, target);
    }

    /// <summary>
    /// Casts a spell by index at a world position.
    /// </summary>
    public bool CastSpellAtPosition(int spellIndex, Vector2 position)
    {
        if (!CanCastSpell(spellIndex))
            return false;

        Spell spell = equippedSpells[spellIndex];
        return spell.CastAtPosition(entity, position);
    }

    /// <summary>
    /// Casts a specific spell on a target.
    /// </summary>
    public bool CastSpell(Spell spell, Entity target = null)
    {
        if (!canCast || spell == null)
            return false;

        return spell.Cast(entity, target);
    }

    /// <summary>
    /// Casts a specific spell at a world position.
    /// </summary>
    public bool CastSpellAtPosition(Spell spell, Vector2 position)
    {
        if (!canCast || spell == null)
            return false;

        return spell.CastAtPosition(entity, position);
    }

    /// <summary>
    /// Checks if a spell at the given index can be cast.
    /// </summary>
    public bool CanCastSpell(int spellIndex)
    {
        if (!canCast)
            return false;

        if (spellIndex < 0 || spellIndex >= equippedSpells.Count)
            return false;

        Spell spell = equippedSpells[spellIndex];
        if (spell == null)
            return false;

        return spell.CanCast(entity);
    }

    #endregion

    #region Spell Management

    /// <summary>
    /// Equips a spell in the specified slot.
    /// </summary>
    public bool EquipSpell(Spell spell, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxSpellSlots)
        {
            Debug.LogWarning($"EntitySpellCaster: Invalid slot index {slotIndex}");
            return false;
        }

        // Ensure list is large enough
        while (equippedSpells.Count <= slotIndex)
        {
            equippedSpells.Add(null);
        }

        // Unequip old spell if present
        if (equippedSpells[slotIndex] != null)
        {
            equippedSpells[slotIndex].OnUnequip(entity);
        }

        // Equip new spell
        equippedSpells[slotIndex] = spell;
        if (spell != null)
        {
            spell.OnEquip(entity);
        }

        return true;
    }

    /// <summary>
    /// Unequips a spell from the specified slot.
    /// </summary>
    public void UnequipSpell(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equippedSpells.Count)
            return;

        if (equippedSpells[slotIndex] != null)
        {
            equippedSpells[slotIndex].OnUnequip(entity);
            equippedSpells[slotIndex] = null;
        }
    }

    /// <summary>
    /// Swaps two spell slots.
    /// </summary>
    public void SwapSpells(int slotA, int slotB)
    {
        if (slotA < 0 || slotA >= equippedSpells.Count ||
            slotB < 0 || slotB >= equippedSpells.Count)
            return;

        Spell temp = equippedSpells[slotA];
        equippedSpells[slotA] = equippedSpells[slotB];
        equippedSpells[slotB] = temp;
    }

    /// <summary>
    /// Gets the spell at the specified slot.
    /// </summary>
    public Spell GetSpell(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equippedSpells.Count)
            return null;

        return equippedSpells[slotIndex];
    }

    #endregion

    #region Casting State

    /// <summary>
    /// Enables or disables spell casting.
    /// </summary>
    public void SetCanCast(bool value)
    {
        canCast = value;
    }

    /// <summary>
    /// Disables casting for a duration (e.g., stun effects).
    /// </summary>
    public void DisableCastingFor(float duration)
    {
        canCast = false;
        Invoke(nameof(EnableCasting), duration);
    }

    private void EnableCasting()
    {
        canCast = true;
    }

    #endregion

    #region Utility

    /// <summary>
    /// Gets all equipped spells.
    /// </summary>
    public List<Spell> GetAllSpells()
    {
        return new List<Spell>(equippedSpells);
    }

    /// <summary>
    /// Gets the number of equipped spells.
    /// </summary>
    public int GetEquippedSpellCount()
    {
        int count = 0;
        foreach (var spell in equippedSpells)
        {
            if (spell != null)
                count++;
        }
        return count;
    }

    #endregion
}