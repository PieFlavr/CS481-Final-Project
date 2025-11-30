using UnityEngine;
using System.Collections.Generic;

public enum EntityFaction
{
    Neutral,
    Player,
    Enemy,
    Ally,
}

/// <summary>
/// ScriptableObject template for entity configuration.
/// Defines identity, visuals, base stats, and default behaviors.
/// Serves as a reusable template for spawning entities.
/// </summary>
[CreateAssetMenu(fileName = "New EntityData", menuName = "Scriptable Objects/EntityData")]
public class EntityData : ScriptableObject
{
    #region Identity

    [Header("Identity")]
    [Tooltip("Display name of this entity")]
    public string entityName = "Unnamed Entity";

    [Tooltip("Faction determines targeting and interactions")]
    public EntityFaction faction = EntityFaction.Neutral;

    #endregion

    #region Visuals

    [Header("Visual Prefabs")]
    [Tooltip("Main visual prefab (sprite, model, etc.)")]
    public GameObject visualPrefab;

    [Tooltip("Optional death/destruction effect")]
    public GameObject deathEffectPrefab;

    [Tooltip("Optional spawn/entry effect")]
    public GameObject spawnEffectPrefab;

    #endregion

    #region Base Stats

    [Header("Base Resources")]
    [Tooltip("Maximum health")]
    public float baseHealth = 100f;

    [Tooltip("Maximum mana")]
    public float baseMana = 100f;

    [Tooltip("Health regeneration rate (per second)")]
    public float healthRegenRate = 0f;

    [Tooltip("Mana regeneration rate (per second)")]
    public float manaRegenRate = 10f;

    [Header("Base Stats")]
    [Tooltip("Weight (affects physics interactions)")]
    public float baseWeight = 1f;

    [Tooltip("Movement speed")]
    public float baseMoveSpeed = 5f;

    #endregion

    #region Behavior Configuration

    [Header("Default Behaviors (Optional)")]
    [Tooltip("Behaviors automatically added when this entity spawns")]
    [SerializeReference]
    public List<ConditionalBehavior> defaultBehaviors = new List<ConditionalBehavior>();

    #endregion
}