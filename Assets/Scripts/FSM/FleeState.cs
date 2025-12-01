using UnityEngine;

public class FleeState : IState
{
    private readonly EnemyController owner;
    private readonly Transform transform;
    private readonly LayerMask targetLayer;
    private readonly System.Action onIdle;
    private readonly Collider[] targetColliders = new Collider[1];

    public FleeState(EnemyController owner, LayerMask targetLayer, System.Action onIdle)
    {
        this.owner = owner;
        this.transform = owner.transform;
        this.targetLayer = targetLayer;
        this.onIdle = onIdle;
    }

    public void Enter() => Debug.Log("Fight: Enter");

    public void Tick()
    {
        Vector3 position = this.transform.position;
        if (!this.TryFindTargetInRange(position, this.owner.AggroDistance, out Collider target))
            return;

        var targetVelocity = target.attachedRigidbody.linearVelocity;
        var desiredHeading = -targetVelocity.normalized;

        // Target can safely be hit, attack.
        // target.GetComponent<>();
    }

    public void Exit() => Debug.Log("Fight: Exit");

    private bool TryFindTargetInRange(Vector3 position, float radius, out Collider target)
    {
        var hits = Physics.OverlapSphereNonAlloc(
            position,
            radius: radius,
            this.targetColliders,
            layerMask: this.targetLayer);

        target = hits > 0 ? this.targetColliders[0] : null;
        return target != null;
    }
}
