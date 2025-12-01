using UnityEngine;

/// <summary>
/// Fight state: attack target in range.
/// Executes attack actions with cooldown management.
/// Transitions to Flee when health critical, Chase when target moves out of range.
/// </summary>
public class FightState : IState
{
    private readonly FSMComponent fsmComponent;
    private readonly EnemyEntity owner;
    private readonly Transform transform;
    private Transform currentTarget;
    private float lastAttackTime;

    public FightState(FSMComponent fsmComponent, EnemyEntity owner)
    {
        this.fsmComponent = fsmComponent;
        this.owner = owner;
        this.transform = owner.transform;
    }

    public void Enter()
    {
        Debug.Log($"[{owner.ArchetypeId}] Fight: Enter");
        FindTarget();
    }

    public void Tick()
    {
        if (currentTarget == null)
        {
            FindTarget();
            if (currentTarget == null) return;
        }

        // Face target
        Vector3 direction = (currentTarget.position - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // Attack if cooldown elapsed
        float timeSinceLastAttack = Time.time - lastAttackTime;
        if (timeSinceLastAttack >= fsmComponent.RuntimeParameters.attackCooldown.Value)
        {
            ExecuteAttack();
            lastAttackTime = Time.time;
        }
    }

    public void Exit()
    {
        Debug.Log($"[{owner.ArchetypeId}] Fight: Exit");
        currentTarget = null;
    }

    private void FindTarget()
    {
        float attackRange = 2f; // TODO: Make this a learnable parameter
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            attackRange,
            owner.ArchetypeData.TargetLayer
        );

        if (hits.Length > 0)
        {
            currentTarget = hits[0].transform;
        }
    }

    private void ExecuteAttack()
    {
        if (currentTarget == null) return;

        Debug.Log($"[{owner.ArchetypeId}] Attacking {currentTarget.name}");

        // Apply damage to target if it has stats
        var targetStats = currentTarget.GetComponent<StatsComponent>();
        if (targetStats != null)
        {
            targetStats.TakeDamage(owner.Stats.Damage);
        }
    }
}

