using UnityEngine;

public class FightState : IState
{
    private readonly EnemyController owner;
    private readonly Transform transform;
    private readonly LayerMask targetLayer;
    private readonly System.Action onRun;
    private readonly Collider[] targetColliders = new Collider[1];

    public FightState(EnemyController owner, LayerMask targetLayer, System.Action onRun)
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
        var hits = Physics.OverlapSphereNonAlloc(
            position,
            radius: radius,
            this.targetColliders,
            layerMask: this.targetLayer);

        target = hits > 0 ? this.targetColliders[0].transform : null;
        return target != null;
    }
}
