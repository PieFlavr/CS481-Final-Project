using System;
using Unity.Behavior;
using UnityEngine;

[RequireComponent(typeof(BehaviorGraphAgent))]
[RequireComponent(typeof(EnemyEntity))]
public class AgentUtilityHandler : MonoBehaviour
{
    public AgentPerception perception;

    [Header("Attack Base Utility")]
    public UtilityCurve attackDistanceCurve;
    public UtilityCurve attackConfidenceCurve;     // health + allies
    public UtilityCurve attackPlayerThreatPenalty; // inverse curve

    [Header("Flee Base Utility")]
    public UtilityCurve fleeLowHealthCurve;
    public UtilityCurve fleeThreatCurve;           // player attack or danger
    public UtilityCurve fleeIsolationCurve;

    [Header("Seek Base Utility")]
    public UtilityCurve seekDistanceCurve;         // desire to get closer
    public UtilityCurve seekConfidenceCurve;       // more confidence = less need to seek

    [Header("Idle Base Utility")]
    public UtilityCurve idleBaseCurve;             // usually small / constant

    // ---- Base Utility Outputs ----
    public float AttackBase { get; private set; }
    public float FleeBase { get; private set; }
    public float SeekBase { get; private set; }
    public float IdleBase { get; private set; }

    [SerializeField] private LayerMask allyLayer;

    private BehaviorGraphAgent agent;
    private EnemyEntity entity;
    private BlackboardVariable<EnemyState> blackboardState;
    private EnemyState state;
    public EnemyState State => this.state;
    private static readonly EnemyState[] states = new EnemyState[]
    {
        EnemyState.Idle,
        EnemyState.Flee,
        EnemyState.Attack,
    };

    void Awake()
    {
        this.agent = GetComponent<BehaviorGraphAgent>();
        this.entity = GetComponent<EnemyEntity>();
        this.agent.BlackboardReference.GetVariable("State", out this.blackboardState);
    }

    public void DetermineState()
    {
        float bestUtility = 0.0f;
        EnemyState bestState = EnemyState.Idle;
        foreach (var state in states)
        {
            float utility = this.GetUtility(state);
            if (utility > bestUtility)
            {
                bestState = state;
                bestUtility = utility;
            }
        }
    }

    private float GetUtility(EnemyState state)
    {
        throw new NotImplementedException();
    }

    public void GetUtility()
    {
        // ---- Helper inputs ----
        float distanceScore = perception.Distance01;     // closer → higher
        float healthScore = perception.Health01;
        float alliesScore = perception.Allies01;
        float threatScore = perception.playerIsAttacking ? 1f : 0f;
        float lowHealth = 1f - healthScore;
        float isolation = 1f - alliesScore;

        // ---- ATTACK ----
        float attackDistance = attackDistanceCurve.Evaluate01(distanceScore);
        float attackConfidence = attackConfidenceCurve.Evaluate01(
            Mathf.Clamp01(0.5f * healthScore + 0.5f * alliesScore)
        );
        float attackThreatPenalty = attackPlayerThreatPenalty.Evaluate01(threatScore);

        AttackBase = attackDistance * attackConfidence * (1f - attackThreatPenalty);

        // ---- FLEE ----
        float fleeLowHealth = fleeLowHealthCurve.Evaluate01(lowHealth);
        float fleeThreat = fleeThreatCurve.Evaluate01(threatScore);
        float fleeIsolation = fleeIsolationCurve.Evaluate01(isolation);

        // Flee is "any danger is enough", so we use max
        FleeBase = Mathf.Max(fleeLowHealth, fleeThreat, fleeIsolation);

        // ---- SEEK ----
        float seekDistance = seekDistanceCurve.Evaluate01(1f - distanceScore); // want to close distance if far
        float seekConfidence = seekConfidenceCurve.Evaluate01(1f - healthScore);

        // Seek is strongest when the enemy is healthy AND far away
        SeekBase = seekDistance * seekConfidence;

        // ---- IDLE ----
        IdleBase = idleBaseCurve.Evaluate01(0.5f); // constant-ish
    }

    public void SetState(EnemyState value)
    {
        this.state = value;
        this.blackboardState.ObjectValue = value;
    }
}
