using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Navigate Away From Target 2D",
    description: "Navigates a GameObject away from another GameObject using its Rigidbody2D." +
    "\nIf Rigidbody2D is not available on the [Agent] or its children, moves the Agent using its transform.",
    story: "[Agent] navigates away from [Target]",
    category: "Action/Navigation",
     id: "01572d38b08b5d9357eeb494faaecef6")]
public partial class NavigateAwayFromTarget2DAction : Action
{
    public enum TargetPositionMode
    {
        ClosestPointOnAnyCollider,      // Use the closest point on any collider, including child objects
        ClosestPointOnTargetCollider,   // Use the closest point on the target's own collider only
        ExactTargetPosition             // Use the exact position of the target, ignoring colliders
    }

    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<float> Speed = new BlackboardVariable<float>(1.0f);
    [SerializeReference] public BlackboardVariable<float> DistanceThreshold = new BlackboardVariable<float>(0.2f);
    [SerializeReference] public BlackboardVariable<string> AnimatorSpeedParam = new BlackboardVariable<string>("SpeedMagnitude");

    // This will only be used in movement without a Rigidbody.
    [SerializeReference] public BlackboardVariable<float> SlowDownDistance = new BlackboardVariable<float>(1.0f);
    [Tooltip("Defines how the target position is determined for navigation:" +
        "\n- ClosestPointOnAnyCollider: Use the closest point on any collider, including child objects" +
        "\n- ClosestPointOnTargetCollider: Use the closest point on the target's own collider only" +
        "\n- ExactTargetPosition: Use the exact position of the target, ignoring colliders. Default if no collider is found.")]
    [SerializeReference] public BlackboardVariable<TargetPositionMode> m_TargetPositionMode = new(TargetPositionMode.ClosestPointOnAnyCollider);

    private Rigidbody2D m_Rigidbody;
    private Animator m_Animator;
    private Vector3 m_ColliderAdjustedTargetPosition;
    [CreateProperty] private float m_OriginalStoppingDistance = -1f;
    [CreateProperty] private Vector2? m_OriginalVelocity = null;
    private float m_ColliderOffset;
    private float m_CurrentSpeed;

    protected override Status OnStart()
    {
        if (Agent.Value == null || Target.Value == null)
        {
            return Status.Failure;
        }

        return Initialize();
    }

    protected override Status OnUpdate()
    {
        if (Agent.Value == null || Target.Value == null)
        {
            return Status.Failure;
        }

        m_ColliderAdjustedTargetPosition = GetPositionColliderAdjusted();
        float distance = Vector2.Distance(Agent.Value.transform.position, Target.Value.transform.position);
        bool tooFar = distance > (DistanceThreshold + m_ColliderOffset);

        if (tooFar)
        {
            return Status.Success;
        }
        else
        {
            m_CurrentSpeed = SimpleMoveAwayFromLocation(Agent.Value.transform, m_ColliderAdjustedTargetPosition, Speed, distance, SlowDownDistance);
        }

        UpdateAnimatorSpeed();

        return Status.Running;
    }

    protected override void OnEnd()
    {
        UpdateAnimatorSpeed(0f);
        m_Rigidbody = null;
        m_Animator = null;
    }

    protected override void OnDeserialize()
    {
        // Reset rigidbody velocity.
        m_Rigidbody = Agent.Value.GetComponent<Rigidbody2D>();
        if (m_Rigidbody != null)
        {
            if (m_Rigidbody != null)
                m_Rigidbody.linearVelocity = m_OriginalVelocity.Value;
        }

        Initialize();
    }

    private Status Initialize()
    {
        m_ColliderAdjustedTargetPosition = GetPositionColliderAdjusted();

        // Add the extents of the colliders to the stopping distance.
        m_ColliderOffset = 0.0f;
        if (Agent.Value.TryGetComponent<Collider>(out var agentCollider))
        {
            Vector3 colliderExtents = agentCollider.bounds.extents;
            m_ColliderOffset += Mathf.Max(colliderExtents.x, colliderExtents.z);
        }

        float distance = Vector2.Distance(Agent.Value.transform.position, m_ColliderAdjustedTargetPosition);
        if (distance >= (DistanceThreshold + m_ColliderOffset))
        {
            return Status.Success;
        }

        m_Animator = Agent.Value.GetComponent<Animator>();
        UpdateAnimatorSpeed(0f);

        return Status.Running;
    }

    private Vector3 GetPositionColliderAdjusted()
    {
        switch (m_TargetPositionMode.Value)
        {
            case TargetPositionMode.ClosestPointOnAnyCollider:
                Collider anyCollider = Target.Value.GetComponent<Collider>();
                if (anyCollider == null || anyCollider.enabled == false)
                    break;
                return anyCollider.ClosestPoint(Agent.Value.transform.position);
            case TargetPositionMode.ClosestPointOnTargetCollider:
                Collider targetCollider = Target.Value.GetComponent<Collider>();
                if (targetCollider == null || targetCollider.enabled == false)
                    break;
                return targetCollider.ClosestPoint(Agent.Value.transform.position);
        }

        // Default to target position.
        return Target.Value.transform.position;
    }

    private void UpdateAnimatorSpeed(float explicitSpeed = -1)
    {
        UpdateAnimatorSpeed(m_Animator, AnimatorSpeedParam, m_Rigidbody, m_CurrentSpeed, explicitSpeed: explicitSpeed);
    }

    /// <summary>
    /// Updates an animator parameter based on agent movement speed
    /// </summary>
    /// <param name="animator">The animator component to update</param>
    /// <param name="speedParameterName">Name of the speed parameter in the animator</param>
    /// <param name="rigidbody2D">Optional Rigidbody2D component</param>
    /// <param name="currentSpeed">Current calculated speed (used when navMeshAgent is null)</param>
    /// <param name="minSpeedThreshold">The minimum speed threshold - any calculated speed at or below this value will be set to zero.
    /// This helps eliminate animator jitter when the agent is nearly stationary or making very minor adjustments.</param>
    /// <param name="explicitSpeed">Optional explicit speed value to set (-1 means use movement speed)</param>
    /// <returns>True if animator was updated, false otherwise</returns>
    private bool UpdateAnimatorSpeed(Animator animator, string speedParameterName, Rigidbody2D rigidbody2D, float currentSpeed, float minSpeedThreshold = 0.1f, float explicitSpeed = -1f)
    {
        if (animator == null || string.IsNullOrEmpty(speedParameterName))
        {
            return false;
        }

        float speedValue;
        if (explicitSpeed >= 0)
            speedValue = explicitSpeed;

        else if (rigidbody2D != null)
            speedValue = rigidbody2D.linearVelocity.magnitude;

        else
            speedValue = currentSpeed;

        if (speedValue <= minSpeedThreshold)
            speedValue = 0;

        animator.SetFloat(speedParameterName, speedValue);
        return true;
    }

    /// <summary>
    /// Moves a transform away from a target position with optional slowdown near destination
    /// </summary>
    /// <param name="agentTransform">The transform to move</param>
    /// <param name="avoidLocation">The target position to move away from</param>
    /// <param name="speed">Maximum movement speed</param>
    /// <param name="distance">Current distance to target</param>
    /// <param name="slowDownDistance">Distance at which to begin slowing down (0 for no slowdown)</param>
    /// <param name="minSpeedRatio">Minimum speed ratio when slowing down (0.1 = 10% of max speed)</param>
    /// <returns>Actual speed used for movement</returns>
    private float SimpleMoveAwayFromLocation(
        Transform agentTransform,
        Vector2 avoidLocation,
        float speed,
        float distance,
        float slowDownDistance = 0.0f,
        float minSpeedRatio = 0.1f)
    {
        if (agentTransform == null)
            return 0f;

        Vector2 agentPosition = agentTransform.position;
        float movementSpeed = speed;

        // Slowdown
        if (slowDownDistance > 0.0f && distance < slowDownDistance)
        {
            float ratio = distance / slowDownDistance;
            movementSpeed = Mathf.Max(speed * minSpeedRatio, speed * ratio);
        }

        Vector2 toDestination = agentPosition - avoidLocation;
        if (toDestination.sqrMagnitude > 0.0001f)
        {
            toDestination.Normalize();

            // Apply movement
            agentPosition += toDestination * (movementSpeed * Time.deltaTime);
            agentTransform.position = agentPosition;
        }

        return movementSpeed;
    }
}

