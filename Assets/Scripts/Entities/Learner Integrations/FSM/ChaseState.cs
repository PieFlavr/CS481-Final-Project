using UnityEngine;

/// <summary>
/// Chase state: pursue detected target.
/// Moves toward target while maintaining distance thresholds.
/// Transitions to Fight when in attack range, Flee when health critical.
/// </summary>
public class ChaseState : IState
{
    private readonly FSMComponent fsmComponent;
    private readonly EnemyEntity owner;
    private readonly Transform transform;
    private readonly Rigidbody2D rigidbody;
    private Transform currentTarget;
    private float chaseStartTime;

    public ChaseState(FSMComponent fsmComponent, EnemyEntity owner)
    {
        this.fsmComponent = fsmComponent;
        this.owner = owner;
        this.transform = owner.transform;
        this.rigidbody = owner.GetComponent<Rigidbody2D>();
    }

    public void Enter()
    {
        Debug.Log($"[{owner.ArchetypeId}] Chase: Enter");
        chaseStartTime = Time.time;
        FindTarget();
    }

    public void Tick()
    {
        if (currentTarget == null)
        {
            FindTarget();
            if (currentTarget == null) return;
        }

        // Check chase duration timeout
        float chaseDuration = Time.time - chaseStartTime;
        if (chaseDuration > fsmComponent.RuntimeParameters.chaseDurationMax.Value)
        {
            Debug.Log($"[{owner.ArchetypeId}] Chase timeout, losing target");
            currentTarget = null;
            return;
        }

        // Move toward target
        Vector3 direction = (currentTarget.position - transform.position).normalized;
        
        if (rigidbody != null)
        {
            rigidbody.linearVelocity = (Vector2)direction * owner.Stats.Speed;
        }
        else
        {
            transform.position += direction * owner.Stats.Speed * Time.deltaTime;
        }

        // Face target
        if (direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    public void Exit()
    {
        Debug.Log($"[{owner.ArchetypeId}] Chase: Exit");
        if (rigidbody != null)
        {
            rigidbody.linearVelocity = Vector2.zero;
        }
        currentTarget = null;
    }

    private void FindTarget()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            fsmComponent.RuntimeParameters.aggroDistance.Value,
            owner.ArchetypeData.TargetLayer
        );

        if (hits.Length > 0)
        {
            currentTarget = hits[0].transform;
        }
    }
}
