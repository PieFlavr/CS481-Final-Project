using UnityEngine;

public class AgentPerception : MonoBehaviour
{
    [Header("Self")]
    public float currentHealth;
    public float maxHealth = 100f;

    [Header("Environment")]
    public float distanceToPlayer;
    public int alliesNearby;
    public bool playerIsAttacking;

    [Header("Normalization")]
    public float maxConsideredDistance = 12f;
    public int maxAllies = 5;

    public float Health01 => Mathf.Clamp01(currentHealth / maxHealth);
    public float Distance01 => 1f - Mathf.Clamp01(distanceToPlayer / maxConsideredDistance);
    public float Allies01 => Mathf.Clamp01(alliesNearby / (float)maxAllies);
}
