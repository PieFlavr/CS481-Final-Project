using UnityEngine;

public class AgentPerception : MonoBehaviour
{
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
    public LayerMask allyLayer;

    [Header("Idle Base Utility")]
    public float idleUtility; // constant

    [Header("Normalization")]
    public float maxConsideredDistance = 12f;

    private float healthScore; // highest when health is high
    private float alliesScore; // highest when many allies nearby
    private float distanceScore; // highest when closer to player
    private float threatScore; // highest when player is attacking

    public void UpdateParameters(StatsComponent stats)
    {
        // Get health score.
        this.healthScore = stats.GetHealthPercent();

        // Get ally score.
        var enemies = EntityManager.Instance.GetAllEnemies();
        int maxAllies = enemies.Count - 1;
        int alliesNearby = 0;
        foreach (var enemy in enemies)
        {
            if (enemy.Stats == stats)
                continue;
            if (Vector2.Distance(enemy.transform.position, this.transform.position) <= this.maxConsideredDistance)
                alliesNearby++;
        }
        this.alliesScore = Mathf.Clamp01(alliesNearby / (float)maxAllies);

        // Get distance score.
        var player = GameObject.FindGameObjectWithTag("Player");
        var distanceToPlayer = Vector2.Distance(player.transform.position, this.transform.position);
        this.distanceScore = 1f - Mathf.Clamp01(distanceToPlayer / maxConsideredDistance);     // closer → higher

        // Get threat score.
        this.threatScore = InputManager.Instance.IsAttacking ? 1f : 0f;
    }

    public float GetUtility(EnemyState state)
    {
        if (state == EnemyState.Attack)
        {
            float attackConfidence = attackConfidenceCurve.Evaluate01(Mathf.Clamp01(0.5f * healthScore + 0.5f * alliesScore));
            float attackDistance = attackDistanceCurve.Evaluate01(distanceScore);
            float attackThreatPenalty = attackPlayerThreatPenalty.Evaluate01(threatScore);
            return Mathf.Max(attackDistance, attackConfidence) * (1f - attackThreatPenalty);
        }

        if (state == EnemyState.Flee)
        {
            float fleeLowHealth = fleeLowHealthCurve.Evaluate01(1 - healthScore);
            float fleeThreat = fleeThreatCurve.Evaluate01(threatScore);
            float fleeIsolation = fleeIsolationCurve.Evaluate01(1 - alliesScore);

            return Mathf.Max(fleeLowHealth, fleeThreat, fleeIsolation);
        }

        if (state == EnemyState.Seek)
        {
            float seekDistance = seekDistanceCurve.Evaluate01(1f - distanceScore); // want to close distance if far
            float seekConfidence = seekConfidenceCurve.Evaluate01(1f - healthScore);

            return seekDistance * seekConfidence;
        }

        return idleUtility;
    }
}
