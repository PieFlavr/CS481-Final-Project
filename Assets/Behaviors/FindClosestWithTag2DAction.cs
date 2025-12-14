using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Find Closest With Tag 2D",
    description: "Finds the closest GameObject with the given tag.",
    story: "Find [Target] closest to [Agent] with tag: [Tag]",
    category: "Action/Find",
    id: "25e2bf2d384c501a2de642da78feadcb")]
internal partial class FindClosestWithTagAction : Unity.Behavior.Action
{
    [Tooltip("[Out Value] If a target is found, the field is assigned with it.")]
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<string> Tag;

    protected override Status OnStart()
    {
        if (Agent.Value == null)
        {
            LogFailure("No agent provided.");
            return Status.Failure;
        }

        Vector3 agentPosition = Agent.Value.transform.position;

        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag(Tag.Value);
        float closestDistanceSq = Mathf.Infinity;
        GameObject closestGameObject = null;
        foreach (GameObject gameObject in gameObjects)
        {
            if (gameObject == Agent.Value)
                continue;

            float distanceSq = Vector2.Distance(agentPosition, gameObject.transform.position);
            if (closestGameObject == null || distanceSq < closestDistanceSq)
            {
                closestDistanceSq = distanceSq;
                closestGameObject = gameObject;
            }
        }

        Target.Value = closestGameObject;
        return Target.Value == null ? Status.Failure : Status.Success;
    }
}
