using UnityEngine;

/// <summary>
/// FSM component for enemy entities.
/// Manages state machine lifecycle and provides learner integration points.
/// Clones archetype FSM parameters at runtime for learner mutation without affecting assets.
/// </summary>
public class FSMComponent : MonoBehaviour
{
    private StateMachine stateMachine;
    private FSMParameterSet runtimeParameters;
    private ArchetypeData archetypeData;
    private EnemyEntity owner;
    
    // State instances
    private IdleState idleState;
    private ChaseState chaseState;
    private FightState fightState;
    private FleeState fleeState;

    /// <summary>
    /// Runtime-mutable parameters (cloned from archetype config).
    /// Learner mutates these without touching the asset.
    /// </summary>
    public FSMParameterSet RuntimeParameters => runtimeParameters;

    /// <summary>
    /// Initialize FSM with archetype data.
    /// Called by EnemyEntity after Awake.
    /// </summary>
    public void Initialize(EnemyEntity enemyEntity, ArchetypeData data)
    {
        if (data == null)
        {
            Debug.LogError("[FSMComponent] Cannot initialize with null ArchetypeData!");
            return;
        }

        owner = enemyEntity;
        archetypeData = data;
        
        // Clone parameters for runtime mutation
        runtimeParameters = data.CloneFSMParameters();
        
        // Create state instances
        idleState = new IdleState(this, owner);
        chaseState = new ChaseState(this, owner);
        fightState = new FightState(this, owner);
        fleeState = new FleeState(this, owner);
        
        // Initialize state machine with all states
        stateMachine = new StateMachine(
            EvaluateState,
            new IState[] { idleState, chaseState, fightState, fleeState }
        );
        
        // Start in idle
        stateMachine.ChangeToState(idleState);
    }

    private void Update()
    {
        if (stateMachine != null && owner != null && owner.IsAlive)
        {
            stateMachine.Tick();
        }
    }

    /// <summary>
    /// Utility-based state evaluation.
    /// Returns utility score for each state based on runtime parameters.
    /// Highest utility state becomes active.
    /// </summary>
    private float EvaluateState(IState state)
    {
        if (owner == null || !owner.IsAlive) return float.MinValue;

        return state switch
        {
            IdleState => EvaluateIdle(),
            ChaseState => EvaluateChase(),
            FightState => EvaluateFight(),
            FleeState => EvaluateFlee(),
            _ => 0f
        };
    }

    private float EvaluateIdle()
    {
        // Idle has low baseline utility
        // Increases when no threats detected
        float targetDetected = DetectAnyTarget() ? 0f : 1f;
        return 0.1f + (targetDetected * 0.3f);
    }

    private float EvaluateChase()
    {
        // Chase utility based on target detection and distance
        if (!TryGetClosestTarget(out Transform target, out float distance))
            return 0f;
        
        float normalizedDistance = Mathf.Clamp01(distance / archetypeData.SensorRange);
        float inAggroRange = distance < runtimeParameters.aggroDistance.Value ? 1f : 0f;
        
        return inAggroRange * (1f - normalizedDistance) * 0.7f;
    }

    private float EvaluateFight()
    {
        // Fight utility based on being in attack range and health
        if (!TryGetClosestTarget(out Transform target, out float distance))
            return 0f;
        
        float attackRange = 2f; // TODO: Make this a parameter
        float inAttackRange = distance < attackRange ? 1f : 0f;
        float healthPercent = owner.Stats.GetHealthPercent();
        
        return inAttackRange * healthPercent * 0.9f;
    }

    private float EvaluateFlee()
    {
        // Flee utility based on low health
        float healthPercent = owner.Stats.GetHealthPercent();
        float shouldFlee = healthPercent < runtimeParameters.fleeHealthThreshold.Value ? 1f : 0f;
        
        return shouldFlee * (1f - healthPercent) * 0.8f;
    }

    /// <summary>
    /// Detect if any target exists in sensor range.
    /// </summary>
    private bool DetectAnyTarget()
    {
        Collider[] hits = Physics.OverlapSphere(
            owner.Position,
            archetypeData.SensorRange,
            archetypeData.TargetLayer
        );
        return hits.Length > 0;
    }

    /// <summary>
    /// Get the closest target within sensor range.
    /// </summary>
    private bool TryGetClosestTarget(out Transform target, out float distance)
    {
        target = null;
        distance = float.MaxValue;

        Collider[] hits = Physics.OverlapSphere(
            owner.Position,
            archetypeData.SensorRange,
            archetypeData.TargetLayer
        );

        if (hits.Length == 0) return false;

        foreach (var hit in hits)
        {
            float dist = Vector3.Distance(owner.Position, hit.transform.position);
            if (dist < distance)
            {
                distance = dist;
                target = hit.transform;
            }
        }

        return target != null;
    }

    /// <summary>
    /// Force a specific state (for testing/debugging).
    /// </summary>
    public void ForceState(IState state)
    {
        stateMachine?.ChangeToState(state);
    }

    /// <summary>
    /// Get current active state (for debugging).
    /// </summary>
    public IState GetCurrentState()
    {
        return stateMachine?.CurrentState;
    }
}