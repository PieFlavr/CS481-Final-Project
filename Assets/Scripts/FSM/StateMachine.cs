using UnityEngine;

public class StateMachine : MonoBehaviour
{
    public delegate float Evaluator(IState state);

    private IState currentState;
    private IState[] states;
    private Evaluator evaluator;

    public IState CurrentState => currentState;

    public StateMachine(Evaluator evaluator, IState[] states)
    {
        this.evaluator = evaluator;
        this.states = states;
    }

    public void SetEvaluator(Evaluator evaluator)
    {
        this.evaluator = evaluator;
    }

    private IState GetNextState()
    {
        float maxUtility = float.MinValue;
        IState bestState = null;

        foreach (var state in states)
        {
            float utility = this.evaluator(state);
            if (utility > maxUtility) // FIXED: was maxUtility > utility
            {
                bestState = state;
                maxUtility = utility;
            }
        }
        return bestState;
    }

    public void ChangeToState(IState state)
    {
        this.currentState?.Exit();
        this.currentState = state;
        this.currentState?.Enter();
    }

    public void Tick()
    {
        IState nextState = GetNextState();
        if (nextState != this.currentState)
            this.ChangeToState(nextState);

        this.currentState?.Tick();
    }
}
