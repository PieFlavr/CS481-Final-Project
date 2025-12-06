using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Attack",
    description: "Performs and animates an attack on two bodies using their StatsComponent. Both bodies must have a StatsComponent.",
    story: "[Agent] attacks [Target]",
    category: "Action/Combat",
    id: "43fbac606f739aaa14a60735dd53a980")]
public partial class AttackAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    [SerializeReference] public BlackboardVariable<string> HurtAnimatorTriggerName = new BlackboardVariable<string>();
    [SerializeReference] public BlackboardVariable<string> AttackAnimatorTriggerName = new BlackboardVariable<string>();

    private Animator agentAnimator;
    private Animator targetAnimator;
    private StatsComponent agentStats;
    private StatsComponent targetStats;

    protected override Status OnStart()
    {
        if (Agent.Value == null || Target.Value == null
            || !Target.Value.TryGetComponent(out targetStats)
            || !Agent.Value.TryGetComponent(out agentStats))
        {
            return Status.Failure;
        }

        // Animate.
        agentAnimator = Agent.Value.GetComponent<Animator>();
        targetAnimator = Target.Value.GetComponent<Animator>();

        if (agentAnimator)
            agentAnimator.SetTrigger(AttackAnimatorTriggerName);

        if (targetAnimator)
            targetAnimator.SetTrigger(HurtAnimatorTriggerName);

        // Do damage.
        targetStats.QueueHealthChange(-agentStats.Damage);

        return Status.Running;
    }
}

