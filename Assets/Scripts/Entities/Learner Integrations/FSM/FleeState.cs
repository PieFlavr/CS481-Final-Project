using UnityEngine;

/// <summary>
/// Flee state: retreat from threats when health is critical.
/// Moves away from detected targets.
/// Transitions to Idle when safe, Chase when health recovers.
/// </summary>
public class FleeState : IState
{
    private readonly FSMComponent fsmComponent;
    private readonly EnemyEntity owner;
    private readonly Transform transform;
    private readonly Rigidbody2D rigidbody;

    public FleeState(FSMComponent fsmComponent, EnemyEntity owner)
    {
        this.fsmComponent = fsmComponent;
        this.owner = owner;
        this.transform = owner.transform;
        this.rigidbody = owner.GetComponent<Rigidbody2D>();
    }

    public void Enter()
    {
        Debug.Log($"[{owner.ArchetypeId}] Flee: Enter");
    }

    public void Tick()
    {
        // Find closest threat
        if (!TryFindClosestThreat(out Transform threat, out float distance))
        {
            // No threats detected, stand still
            if (rigidbody != null)
                rigidbody.linearVelocity = Vector2.zero;
            return;
        }

        // Move away from threat
        Vector3 fleeDirection = (transform.position - threat.position).normalized;
        float fleespeed = owner.Stats.Speed * 1.5f; // Flee faster

        if (rigidbody != null)
        {
            rigidbody.linearVelocity = (Vector2)fleeDirection * fleespeed;
        }
        else
        {
            transform.position += fleeDirection * fleespeed * Time.deltaTime;
        }

        // Face flee direction
        if (fleeDirection != Vector3.zero)
        {
            float angle = Mathf.Atan2(fleeDirection.y, fleeDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    public void Exit()
    {
        Debug.Log($"[{owner.ArchetypeId}] Flee: Exit");
        if (rigidbody != null)
        {
            rigidbody.linearVelocity = Vector2.zero;
        }
    }

    private bool TryFindClosestThreat(out Transform threat, out float distance)
    {
        threat = null;
        distance = float.MaxValue;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            fsmComponent.RuntimeParameters.aggroDistance.Value,
            owner.ArchetypeData.TargetLayer
        );

        if (hits.Length == 0) return false;

        foreach (var hit in hits)
        {
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < distance)
            {
                distance = dist;
                threat = hit.transform;
            }
        }

        return threat != null;
    }
}
