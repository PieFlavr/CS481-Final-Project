using UnityEngine;

/// <summary>
/// Base ScriptableObject for all entity configuration.
/// Defines core stats that all entities share.
/// Inherited by ArchetypeData (swarm), PlayerData (player), etc.
/// </summary>
public abstract class EntityData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] protected string displayName = "Generic Entity";

    [Header("Prefab")]
    [SerializeField] protected BaseEntity prefab;

    [Header("Base Stats")]
    [SerializeField] protected float maxHealth = 10f;
    [SerializeField] protected float speed = 3f;
    [SerializeField] protected float damage = 1f;

    public string DisplayName => displayName;
    public float MaxHealth => maxHealth;
    public float Speed => speed;
    public float Damage => damage;
    public BaseEntity Prefab => prefab;

    private void OnValidate()
    {
        if (maxHealth < 0f) maxHealth = 0f;
        if (speed < 0f) speed = 0f;
        if (damage < 0f) damage = 0f;

        // Prefab null is allowed (fallback to spawner default), but warn designers.
        if (prefab == null)
        {
            // Don't spam logs in the editor unless explicitly set up.
            // Debug.LogWarning($"[EntityData] {name} has no prefab assigned.");
        }
    }
}
