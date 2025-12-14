using System;
using Unity.Behavior;
using UnityEngine;

[RequireComponent(typeof(BehaviorGraphAgent))]
[RequireComponent(typeof(StatsComponent))]
public class AgentUtilityHandler : MonoBehaviour
{
    public AgentPerception perception;
    private BehaviorGraphAgent agent;
    private StatsComponent stats;
    private EnemyState state;
    public EnemyState State => this.state;
    private static readonly EnemyState[] states = new EnemyState[]
    {
        EnemyState.Idle,
        EnemyState.Flee,
        EnemyState.Attack,
        EnemyState.Seek,
    };

    void Start()
    {
        this.agent = GetComponent<BehaviorGraphAgent>();
        this.stats = GetComponent<StatsComponent>();

        this.agent.Init();
    }

    void Update()
    {
        var nextState = DetermineState();
        this.agent.GetVariable("State", out BlackboardVariable<EnemyState> state);
        if ((EnemyState)state.ObjectValue != nextState)
            this.SetState(nextState);
    }

    public EnemyState DetermineState()
    {
        float bestUtility = 0.0f;
        EnemyState bestState = EnemyState.Idle;
        foreach (var state in states)
        {
            this.perception.UpdateParameters(this.stats);
            float utility = this.perception.GetUtility(state);
            if (utility > bestUtility)
            {
                bestState = state;
                bestUtility = utility;
            }
        }
        return bestState;
    }

    public void SetState(EnemyState value)
    {
        this.state = value;
        this.agent.SetVariableValue("State", value);
    }
}
