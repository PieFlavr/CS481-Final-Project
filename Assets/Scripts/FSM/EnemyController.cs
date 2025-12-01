using System;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Enemy Parameters")]
    [SerializeField, Tooltip("The distance from a target at which this enemy will run away.")]
    private float disengageDistance;

    [SerializeField, Tooltip("The distance at which this enemy can see targets to attack.")]
    private float aggroDistance;

    [SerializeField, Tooltip("The layer that this enemy searches for targets to attack.")]
    private LayerMask targetLayer;

    private StateMachine stateMachine;

    public float DisengageDistance => this.disengageDistance;
    public float AggroDistance => this.aggroDistance;

    void Start()
    {
        var idle
        var fight = new FightState(this, this.targetLayer, () => this.stateMachine.ChangeToState());

        this.stateMachine = new(this.Evaluate, new IState[]
        {

        });
    }

    private float Evaluate(IState state)
    {
        throw new NotImplementedException();
    }
}