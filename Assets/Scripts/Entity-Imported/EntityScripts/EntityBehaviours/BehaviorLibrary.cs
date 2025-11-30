using UnityEngine;

/// <summary>
/// Library of common behaviors and conditions for entities.
/// Behaviors inherit from EntityBehaviorBase (abstract class).
/// Conditions implement IBehaviorCondition (interface).
/// </summary>

#region Behaviors

[System.Serializable]
public class ChasePlayerBehavior : EntityBehaviorBase
{
    [Tooltip("Maximum detection range")]
    public float detectionRadius = 10f;

    [Tooltip("Minimum distance to maintain")]
    public float minDistance = 2f;

    public override void Execute(Entity entity, EntityController controller, float deltaTime)
    {
        if (cachedTarget == null) return;

        float distance = Vector2.Distance(entity.transform.position, cachedTarget.transform.position);

        // Only chase if within range and not too close
        if (distance > minDistance && distance < detectionRadius)
        {
            Vector2 direction = (cachedTarget.transform.position - entity.transform.position).normalized;
            controller.Move(direction);
        }
    }
}

[System.Serializable]
public class FleeFromPlayerBehavior : EntityBehaviorBase
{
    [Tooltip("Distance to flee")]
    public float fleeDistance = 10f;

    public override void Execute(Entity entity, EntityController controller, float deltaTime)
    {
        if (cachedTarget == null) return;

        Vector2 direction = (entity.transform.position - cachedTarget.transform.position).normalized;
        controller.Move(direction);
    }
}

[System.Serializable]
public class ShootSpellBehavior : EntityBehaviorBase
{
    [Tooltip("Which spell slot to cast")]
    public int spellSlot = 0;

    public override void Execute(Entity entity, EntityController controller, float deltaTime)
    {
        if (cachedTarget != null)
            controller.CastSpellAtTarget(spellSlot, cachedTarget);
    }
}

[System.Serializable]
public class IdleBehavior : EntityBehaviorBase
{
    public override void Execute(Entity entity, EntityController controller, float deltaTime)
    {
        controller.StopMovement();
    }
}

#endregion

#region Conditions

[System.Serializable]
public class DistanceCondition : IBehaviorCondition
{
    [Tooltip("Minimum distance (-1 = no min)")]
    public float minDistance = -1f;

    [Tooltip("Maximum distance (-1 = no max)")]
    public float maxDistance = -1f;

    private Entity target;

    public void OnInitialize(Entity entity)
    {
        target = EntityManager.Instance?.GetPlayer();
    }

    public bool IsMet(Entity entity, EntityController controller)
    {
        if (target == null) return false;

        float distance = Vector2.Distance(entity.transform.position, target.transform.position);

        if (minDistance >= 0 && distance < minDistance) return false;
        if (maxDistance >= 0 && distance > maxDistance) return false;

        return true;
    }

    public IBehaviorCondition Clone()
    {
        return new DistanceCondition
        {
            minDistance = this.minDistance,
            maxDistance = this.maxDistance
        };
    }
}

[System.Serializable]
public class HealthCondition : IBehaviorCondition
{
    [Tooltip("Minimum health % (0-1, -1 = no min)")]
    [Range(-1f, 1f)]
    public float minHealthPercent = -1f;

    [Tooltip("Maximum health % (0-1, -1 = no max)")]
    [Range(-1f, 1f)]
    public float maxHealthPercent = -1f;

    public void OnInitialize(Entity entity) { }

    public bool IsMet(Entity entity, EntityController controller)
    {
        float hp = entity.Resources.HealthPercent;

        if (minHealthPercent >= 0 && hp < minHealthPercent) return false;
        if (maxHealthPercent >= 0 && hp > maxHealthPercent) return false;

        return true;
    }

    public IBehaviorCondition Clone()
    {
        return new HealthCondition
        {
            minHealthPercent = this.minHealthPercent,
            maxHealthPercent = this.maxHealthPercent
        };
    }
}

[System.Serializable]
public class CooldownCondition : IBehaviorCondition
{
    [Tooltip("Seconds between activations")]
    public float cooldownSeconds = 1f;

    private float lastTriggerTime = -999f;

    public void OnInitialize(Entity entity) { }

    public bool IsMet(Entity entity, EntityController controller)
    {
        bool ready = Time.time - lastTriggerTime >= cooldownSeconds;

        if (ready)
            lastTriggerTime = Time.time;

        return ready;
    }

    public IBehaviorCondition Clone()
    {
        return new CooldownCondition
        {
            cooldownSeconds = this.cooldownSeconds
            // Don't copy lastTriggerTime - runtime state!
        };
    }
}

[System.Serializable]
public class AlwaysTrueCondition : IBehaviorCondition
{
    public void OnInitialize(Entity entity) { }
    public bool IsMet(Entity entity, EntityController controller) => true;
    public IBehaviorCondition Clone() { return new AlwaysTrueCondition(); }
}

#endregion