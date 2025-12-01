using UnityEngine;

/// <summary>
/// DEPRECATED: Legacy EnemyController.
/// FSM logic has been moved to FSMComponent + EnemyEntity.
/// This file remains for backward compatibility but should not be used for new enemies.
/// Use EnemyEntity with FSMComponent instead.
/// </summary>
[System.Obsolete("Use EnemyEntity + FSMComponent instead")]
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
        Debug.LogWarning("[EnemyController] DEPRECATED: Use EnemyEntity + FSMComponent instead!");
    }
}
