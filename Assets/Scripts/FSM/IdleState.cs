using UnityEngine;

public class IdleState : IState
{
    private readonly EnemyController owner;
    private readonly Transform transform;
    private readonly LayerMask targetLayer;
    private readonly System.Action onRun;

    public IdleState(EnemyController owner, LayerMask targetLayer, System.Action onRun)
    {
        this.owner = owner;
        this.transform = owner.transform;
        this.targetLayer = targetLayer;
        this.onRun = onRun;
    }

    public void Enter() => Debug.Log("Fight: Enter");

    public void Tick()
    {
        Vector3 position = this.transform.position;
        if (!this.TryFindTargetInRange(position, this.owner.AggroDistance, out Transform target))
            return;

        // Target is too close, run away.
        if (Vector3.Distance(position, target.position) < this.owner.DisengageDistance)
        {
            onRun?.Invoke();
            return;
        }

        // Target can safely be hit, attack.
        // target.GetComponent<>();
    }

    public void Exit() => Debug.Log("Fight: Exit");

    private bool TryFindTargetInRange(Vector3 position, float radius, out Transform target)
    {
        bool hit = Physics.SphereCast(
            origin: position,
            radius: radius,
            direction: Vector3.zero,
            out RaycastHit hitInfo,
            maxDistance: radius,
            layerMask: this.targetLayer
        );

        target = hit ? hitInfo.collider.transform : null;
        return target != null;
    }
}
