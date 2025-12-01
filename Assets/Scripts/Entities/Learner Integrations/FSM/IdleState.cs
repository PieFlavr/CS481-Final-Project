using UnityEngine;

/// <summary>
/// Idle state: patrol, wander, or stand guard.
/// Transitions to Chase when target detected within aggro range.
/// </summary>
public class IdleState : IState
{
    private readonly FSMComponent fsmComponent;
    private readonly EnemyEntity owner;
    private readonly Transform transform;

    public IdleState(FSMComponent fsmComponent, EnemyEntity owner)
    {
        this.fsmComponent = fsmComponent;
        this.owner = owner;
        this.transform = owner.transform;
    }

    public void Enter()
    {
        Debug.Log($"[{owner.ArchetypeId}] Idle: Enter");
    }

    public void Tick()
    {
        
        // Idle behavior: stand guard or simple patrol
        // State machine handles transition evaluation via utility
    }

    public void Exit()
    {
        Debug.Log($"[{owner.ArchetypeId}] Idle: Exit");
    }
}
