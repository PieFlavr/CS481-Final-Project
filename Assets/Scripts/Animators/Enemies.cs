using UnityEngine;

/// <summary>
/// Controls enemy animation states through an Animator component.
/// Attach this script to any enemy prefab that has an Animator with the following parameters:
/// - isDead (bool): Triggers death animation (transitions to Exit)
/// - IsHurt (trigger): Triggers hit/damage animation
/// - IsAttack (trigger): Triggers attack animation
/// - SpeedMagnitude (float): Controls movement animations (0 = stand, 0-0.5 = walk, >0.5 = run)
/// 
/// This script automatically updates the SpeedMagnitude based on the Rigidbody2D velocity.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemiesAnimator : MonoBehaviour
{
    /// <summary>
    /// Reference to the Animator component. Automatically fetched on Start if not assigned.
    /// </summary>
    [Tooltip("The Animator component controlling this enemy's animations. Auto-assigned if left empty.")]
    public Animator animator;

    /// <summary>
    /// Reference to the Rigidbody2D component for reading velocity.
    /// </summary>
    private Rigidbody2D rb;

    /// <summary>
    /// Cached reference to the player transform for facing direction.
    /// </summary>
    private Transform playerTransform;

    // Animator parameter names - must match exactly what's in the Animator Controller
    private static readonly string PARAM_IS_DEAD = "isDead";
    private static readonly string PARAM_IS_HURT = "IsHurt";
    private static readonly string PARAM_IS_ATTACK = "IsAttack";
    private static readonly string PARAM_SPEED_MAGNITUDE = "SpeedMagnitude";

    /// <summary>
    /// Called once before the first frame update.
    /// Automatically fetches the Animator and Rigidbody2D components if not assigned.
    /// </summary>
    void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (animator == null)
        {
            Debug.LogError("EnemiesAnimator: No Animator component found on " + gameObject.name);
        }

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("EnemiesAnimator: No Rigidbody2D component found on " + gameObject.name);
        }

        // Find the player
        var player = FindAnyObjectByType<PlayerEntity>();
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    /// <summary>
    /// Updates the SpeedMagnitude parameter based on the enemy's velocity every frame.
    /// This automatically handles stand/walk/run transitions.
    /// Also flips the sprite to face the player.
    /// </summary>
    void Update()
    {
        if (animator == null || rb == null || IsDead()) return;

        // Calculate speed magnitude from velocity
        float speedMagnitude = rb.linearVelocity.magnitude;
        animator.SetFloat(PARAM_SPEED_MAGNITUDE, speedMagnitude);

        // Face the player (flip sprite based on relative position)
        if (playerTransform != null)
        {
            float directionToPlayer = playerTransform.position.x - transform.position.x;
            
            // Flip sprite: if player is to the left (negative direction), flip to face left
            if (directionToPlayer < 0)
            {
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            // If player is to the right (positive direction), face right
            else if (directionToPlayer > 0)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
    }

    /// <summary>
    /// Triggers the death animation. This is a terminal state that transitions to Exit.
    /// Call this when the enemy's health reaches zero.
    /// Can be triggered from any state.
    /// Maintains current facing direction by clearing player reference.
    /// </summary>
    public void SetDead()
    {
        if (animator == null) return;

        animator.SetFloat(PARAM_SPEED_MAGNITUDE, 0);
        animator.SetBool(PARAM_IS_DEAD, true);
        
        // Stop facing the player when dead
        playerTransform = null;
    }

    /// <summary>
    /// Triggers the hit/damage animation.
    /// Call this when the enemy takes damage but is not killed.
    /// Can be triggered from any state (via Any State transition).
    /// </summary>
    public void SetHurt()
    {
        if (animator == null || IsDead()) return;
        animator.SetTrigger(PARAM_IS_HURT);
    }

    /// <summary>
    /// Triggers the attack animation.
    /// Call this when the enemy performs an attack action.
    /// </summary>
    public void SetAttack()
    {
        if (animator == null || IsDead()) return;
        animator.SetTrigger(PARAM_IS_ATTACK);
    }

    /// <summary>
    /// Checks if the enemy is currently in the dead state.
    /// </summary>
    /// <returns>True if the isDead parameter is set, false otherwise.</returns>
    public bool IsDead()
    {
        return animator != null && animator.GetBool(PARAM_IS_DEAD);
    }

    /// <summary>
    /// Gets the current speed magnitude value from the animator.
    /// </summary>
    /// <returns>Current speed magnitude (0 = standing, 0-0.5 = walking, >0.5 = running).</returns>
    public float GetSpeedMagnitude()
    {
        return animator != null ? animator.GetFloat(PARAM_SPEED_MAGNITUDE) : 0f;
    }
}
