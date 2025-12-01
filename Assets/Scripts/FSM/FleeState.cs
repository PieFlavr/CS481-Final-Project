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

    public FleeState(FSMComponent fsmComponent, EnemyEntity owner)
    {
        this.fsmComponent = fsmComponent;
        this.owner = owner;
        this.transform = owner.transform;
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
            return;
        }

        // Move away from threat
        Vector3 fleeDirection = (transform.position - threat.position).normalized;
        transform.position += fleeDirection * owner.Stats.Speed * 1.5f * Time.deltaTime; // Flee faster

        // Face flee direction
        if (fleeDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(fleeDirection);
        }
    }

    public void Exit()
    {
        Debug.Log($"[{owner.ArchetypeId}] Flee: Exit");
    }

    private bool TryFindClosestThreat(out Transform threat, out float distance)
    {
        threat = null;
        distance = float.MaxValue;

        Collider[] hits = Physics.OverlapSphere(
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

