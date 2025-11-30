using UnityEngine;

/// <summary>
/// Base component for all interactive world objects.
/// Handles core functionality: stats, resources, visuals, collisions.
/// Behavior is delegated to EntityController.
/// </summary>
public class Entity : MonoBehaviour
{
    #region Configuration

    [Header("Entity Configuration")]
    [SerializeField] private EntityData entityData;

    #endregion

    #region Runtime Components

    private EntityStats stats;
    private EntityResources resources;
    private EntityController controller;
    // private EntitySpellCaster spellCaster;

    #endregion

    #region Visual Components

    private GameObject visualInstance;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    #endregion

    #region Public Properties

    public EntityData Data => entityData;
    public EntityStats Stats => stats;
    public EntityResources Resources => resources;
    public EntityController Controller => controller;
    // public EntitySpellCaster SpellCaster => spellCaster;
    public string EntityName => entityData != null ? entityData.entityName : "Unknown Entity";
    public SpriteRenderer SpriteRenderer => spriteRenderer;
    public Animator Animator => animator;
    public GameObject VisualInstance => visualInstance;

    #endregion

    #region Lifecycle

    private void Awake()
    {
        // Validate configuration
        if (entityData == null)
        {
            Debug.LogError($"Entity: No EntityData assigned on {name}!");
            return;
        }

        // Initialize components
        InitializeVisuals();
        InitializeStats();
        InitializeResources();
        InitializeReferences();

        // Register with manager
        RegisterEntity();
    }

    private void OnDestroy()
    {
        UnregisterEntity();
        UnsubscribeEvents();
    }

    #endregion

    #region Initialization

    private void InitializeVisuals()
    {
        if (entityData.visualPrefab == null)
        {
            Debug.LogWarning($"Entity: No visual prefab for {entityData.entityName}");
            return;
        }

        visualInstance = Instantiate(entityData.visualPrefab, transform);
        visualInstance.name = $"{entityData.entityName}_Visual";

        spriteRenderer = visualInstance.GetComponent<SpriteRenderer>();
        animator = visualInstance.GetComponent<Animator>();

        // Spawn effect
        if (entityData.spawnEffectPrefab != null)
        {
            GameObject fx = Instantiate(entityData.spawnEffectPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 3f);
        }
    }

    private void InitializeStats()
    {
        stats = new EntityStats(entityData);
    }

    private void InitializeResources()
    {
        resources = new EntityResources(
            entityData.baseHealth,
            // entityData.baseMana,
            entityData.healthRegenRate//,
            // entityData.manaRegenRate
        );
        resources.OnHealthDepleted += HandleDeath;
        resources.OnHealthChanged += HandleHealthChanged;
    }

    private void InitializeReferences()
    {
        controller = GetComponent<EntityController>();
        // spellCaster = GetComponent<EntitySpellCaster>();
    }

    private void RegisterEntity()
    {
        EntityManager.Instance?.RegisterEntity(this);
    }

    private void UnregisterEntity()
    {
        EntityManager.Instance?.UnregisterEntity(this);
    }

    private void UnsubscribeEvents()
    {
        if (resources != null)
        {
            resources.OnHealthDepleted -= HandleDeath;
            resources.OnHealthChanged -= HandleHealthChanged;
        }
    }

    #endregion

    #region Update Loop

    private void Update()
    {
        // Regenerate health and mana every frame
        resources?.Regenerate(Time.deltaTime);
    }

    #endregion

    #region Health System

    /// <summary>
    /// Applies damage to this entity.
    /// </summary>
    public void TakeDamage(float damage)
    {
        resources.ModifyHealth(-damage);
        OnDamaged(damage);
    }

    /// <summary>
    /// Heals this entity.
    /// </summary>
    public void Heal(float amount)
    {
        resources.ModifyHealth(amount);
        OnHealed(amount);
    }

    /// <summary>
    /// Called when damaged. Override for custom damage responses.
    /// </summary>
    protected virtual void OnDamaged(float damage)
    {
        // Trigger animation
        animator?.SetTrigger("Damaged");
    }

    /// <summary>
    /// Called when healed. Override for custom heal responses.
    /// </summary>
    protected virtual void OnHealed(float amount)
    {
        // Override in subclasses if needed
    }

    private void HandleHealthChanged(float current, float max)
    {
        // Update UI for player faction
        if (entityData.faction == EntityFaction.Player)
        {
            // UIManager.Instance?.UpdateHealthDisplay((int)current, (int)max);
        }
    }

    private void HandleDeath()
    {
        OnDeath();
    }

    /// <summary>
    /// Called when entity dies. Override for custom death behavior.
    /// </summary>
    protected virtual void OnDeath()
    {
        // Spawn death effect
        if (entityData.deathEffectPrefab != null)
        {
            GameObject fx = Instantiate(entityData.deathEffectPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 5f);
        }

        // Trigger animation
        animator?.SetTrigger("Death");

        // Handle player death separately
        if (entityData.faction == EntityFaction.Player)
        {
            HandlePlayerDeath();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void HandlePlayerDeath()
    {
        // if (RunManager.Instance != null)
        // {
        //     RunManager.Instance.RecordPlayerDeath("Defeated in combat");
        // }
        // else
        // {
        //     GameManager.Instance?.GameOver();
        // }
    }

    #endregion

    #region Collision System (Extensible)

    /// <summary>
    /// Handles trigger enter events.
    /// Routes to controller for behavior-specific responses.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Notify controller (behaviors can handle specific collision types)
        controller?.OnCollisionDetected(collision);
    }

    /// <summary>
    /// Handles trigger stay events.
    /// Routes to controller for behavior-specific responses.
    /// </summary>
    private void OnTriggerStay2D(Collider2D collision)
    {
        // Notify controller for continuous collision handling
        controller?.OnCollisionDetected(collision);
    }

    #endregion
}