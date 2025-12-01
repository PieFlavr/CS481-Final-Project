using UnityEngine;

/// <summary>
/// Concrete enemy entity implementation for the entity system.
/// Move any enemy-specific behaviour here. Legacy `Enemy` class will remain as a thin wrapper.
/// </summary>
public class EnemyEntity : BaseEntity
{
    [SerializeField] private ArchetypeData archetypeData;

    private StatsComponent statsComponent;
    private FSMComponent fsmComponent;

    public override EntityType Type => EntityType.Enemy;
    public string ArchetypeId => archetypeData?.ArchetypeId ?? "unknown";
    public ArchetypeData ArchetypeData => archetypeData;
    public StatsComponent Stats => statsComponent;
    public FSMComponent FSM => fsmComponent;

    protected override void Awake()
    {
        base.Awake();

        // Get or add StatsComponent
        statsComponent = GetComponent<StatsComponent>();
        if (statsComponent == null)
        {
            statsComponent = gameObject.AddComponent<StatsComponent>();
        }

        // Get or add FSMComponent
        fsmComponent = GetComponent<FSMComponent>();
        if (fsmComponent == null)
        {
            fsmComponent = gameObject.AddComponent<FSMComponent>();
        }

        // Initialize stats from archetype
        if (archetypeData != null)
        {
            statsComponent.InitializeFromArchetype(archetypeData);
        }
    }

    protected override void Start()
    {
        base.Start();

        // Initialize FSM after all components are ready
        if (archetypeData != null && fsmComponent != null)
        {
            fsmComponent.Initialize(this, archetypeData);
        }
    }

    /// <summary>
    /// Explicit initialization used by factories/spawners. Accepts any EntityData.
    /// </summary>
    public override void Initialize(EntityData data)
    {
        if (data is ArchetypeData ad)
        {
            archetypeData = ad;
            if (statsComponent == null)
                statsComponent = GetComponent<StatsComponent>() ?? gameObject.AddComponent<StatsComponent>();

            if (fsmComponent == null)
                fsmComponent = GetComponent<FSMComponent>() ?? gameObject.AddComponent<FSMComponent>();

            statsComponent.InitializeFromArchetype(ad);
            fsmComponent.Initialize(this, ad);
        }
    }

    public override void Die()
    {
        if (!IsAlive) return;

        // TODO: Play death animation, disable collision, etc.
        base.Die();
    }

    public override void Despawn()
    {
        Die();
    }

    public void TakeDamage(float damage)
    {
        if (!IsAlive || statsComponent == null) return;
        statsComponent.TakeDamage(damage);
    }

    public void Heal(float amount)
    {
        if (!IsAlive || statsComponent == null) return;
        statsComponent.Heal(amount);
    }

    public float GetSpeed()
    {
        return statsComponent != null ? statsComponent.Speed : 0f;
    }

    public float GetDamage()
    {
        return statsComponent != null ? statsComponent.Damage : 0f;
    }

    public float GetAttackRange()
    {
        return statsComponent != null ? statsComponent.AttackRange : 0f;
    }

    public float GetAttackRadius()
    {
        return statsComponent != null ? statsComponent.AttackRadius : 0f;
    }
}
