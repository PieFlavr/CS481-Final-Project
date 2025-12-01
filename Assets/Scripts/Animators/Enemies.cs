using UnityEngine;

/// <summary>
/// Controls enemy animation states through an Animator component.
/// Attach this script to any enemy prefab that has an Animator with the following boolean parameters:
/// - isDead: Triggers death animation (transitions to Exit)
/// - isDamage: Triggers hit/damage animation
/// - isRunning: Triggers running animation
/// - isWalking: Triggers walking animation
/// - isAttacking: Triggers attack animation
/// - isStanding: Triggers idle/standing animation (default state)
/// 
/// MOVEMENT STATE TRANSITIONS:
/// - Standing ↔ Walking (both directions allowed)
/// - Walking ↔ Running (both directions allowed)
/// - Running → Standing (can skip walking when stopping)
/// - Standing → Running (NOT allowed - must go Standing → Walking → Running)
/// </summary>
public class EnemiesAnimator : MonoBehaviour
{
    /// <summary>
    /// Reference to the Animator component. Automatically fetched on Start if not assigned.
    /// </summary>
    [Tooltip("The Animator component controlling this enemy's animations. Auto-assigned if left empty.")]
    public Animator animator;

    // Animator parameter names - must match exactly what's in the Animator Controller
    private static readonly string PARAM_IS_DEAD = "isDead";
    private static readonly string PARAM_IS_DAMAGE = "isDamage";
    private static readonly string PARAM_IS_RUNNING = "isRunning";
    private static readonly string PARAM_IS_WALKING = "isWalking";
    private static readonly string PARAM_IS_ATTACKING = "isAttacking";
    private static readonly string PARAM_IS_STANDING = "isStanding";

    /// <summary>
    /// Called once before the first frame update.
    /// Automatically fetches the Animator component if not assigned in the Inspector.
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
    }

    /// <summary>
    /// Clears movement-related states (standing, walking, running).
    /// Used internally before setting a new movement state.
    /// </summary>
    private void ClearMovementStates()
    {
        if (animator == null) return;

        animator.SetBool(PARAM_IS_RUNNING, false);
        animator.SetBool(PARAM_IS_WALKING, false);
        animator.SetBool(PARAM_IS_STANDING, false);
    }

    /// <summary>
    /// Triggers the death animation. This is a terminal state that transitions to Exit.
    /// Call this when the enemy's health reaches zero.
    /// Can be triggered from any state.
    /// </summary>
    public void SetDead()
    {
        if (animator == null) return;

        ClearMovementStates();
        animator.SetBool(PARAM_IS_DAMAGE, false);
        animator.SetBool(PARAM_IS_ATTACKING, false);
        animator.SetBool(PARAM_IS_DEAD, true);
    }

    /// <summary>
    /// Triggers the hit/damage animation.
    /// Call this when the enemy takes damage but is not killed.
    /// Can be triggered from any state (via Any State transition).
    /// </summary>
    public void SetDamaged()
    {
        if (animator == null) return;

        ClearMovementStates();
        animator.SetBool(PARAM_IS_ATTACKING, false);
        animator.SetBool(PARAM_IS_DAMAGE, true);
    }

    /// <summary>
    /// Triggers the running animation.
    /// Call this when the enemy is moving at full speed (e.g., chasing the player).
    /// REQUIREMENT: Must currently be walking. Cannot go directly from standing or damaged to running.
    /// </summary>
    /// <returns>True if transition was successful, false if not allowed (not currently walking).</returns>
    public bool SetRunning()
    {
        if (animator == null) return false;

        // Can only transition to running from walking (not from standing or damaged)
        if (!IsWalking())
        {
            Debug.LogWarning("EnemiesAnimator: Cannot run - must be walking first. Cannot run from standing or damaged state.");
            return false;
        }

        ClearMovementStates();
        animator.SetBool(PARAM_IS_RUNNING, true);
        return true;
    }

    /// <summary>
    /// Triggers the walking animation.
    /// Call this when the enemy is moving at normal/patrol speed.
    /// Can transition from standing, running, OR damaged states.
    /// </summary>
    /// <returns>True if transition was successful, false if not allowed.</returns>
    public bool SetWalking()
    {
        if (animator == null) return false;

        // Can transition to walking from standing, running, or damaged
        if (!IsStanding() && !IsRunning() && !IsDamaged())
        {
            Debug.LogWarning("EnemiesAnimator: Cannot walk - must be standing, running, or damaged first.");
            return false;
        }

        ClearMovementStates();
        animator.SetBool(PARAM_IS_DAMAGE, false);
        animator.SetBool(PARAM_IS_WALKING, true);
        return true;
    }

    /// <summary>
    /// Triggers the attack animation.
    /// Call this when the enemy performs an attack action.
    /// Can be triggered from standing, walking, or running states.
    /// </summary>
    public void SetAttacking()
    {
        if (animator == null) return;

        ClearMovementStates();
        animator.SetBool(PARAM_IS_DAMAGE, false);
        animator.SetBool(PARAM_IS_ATTACKING, true);
    }

    /// <summary>
    /// Triggers the standing/idle animation.
    /// Call this when the enemy is stationary and not performing any action.
    /// This is the default state the animator enters from Entry.
    /// Can be set from ANY movement state (walking, running) OR from damaged state.
    /// </summary>
    public void SetStanding()
    {
        if (animator == null) return;

        ClearMovementStates();
        animator.SetBool(PARAM_IS_ATTACKING, false);
        animator.SetBool(PARAM_IS_DAMAGE, false);
        animator.SetBool(PARAM_IS_STANDING, true);
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
    /// Checks if the enemy is currently playing the damage animation.
    /// </summary>
    /// <returns>True if the isDamage parameter is set, false otherwise.</returns>
    public bool IsDamaged()
    {
        return animator != null && animator.GetBool(PARAM_IS_DAMAGE);
    }

    /// <summary>
    /// Checks if the enemy is currently running.
    /// </summary>
    /// <returns>True if the isRunning parameter is set, false otherwise.</returns>
    public bool IsRunning()
    {
        return animator != null && animator.GetBool(PARAM_IS_RUNNING);
    }

    /// <summary>
    /// Checks if the enemy is currently walking.
    /// </summary>
    /// <returns>True if the isWalking parameter is set, false otherwise.</returns>
    public bool IsWalking()
    {
        return animator != null && animator.GetBool(PARAM_IS_WALKING);
    }

    /// <summary>
    /// Checks if the enemy is currently attacking.
    /// </summary>
    /// <returns>True if the isAttacking parameter is set, false otherwise.</returns>
    public bool IsAttacking()
    {
        return animator != null && animator.GetBool(PARAM_IS_ATTACKING);
    }

    /// <summary>
    /// Checks if the enemy is currently standing/idle.
    /// </summary>
    /// <returns>True if the isStanding parameter is set, false otherwise.</returns>
    public bool IsStanding()
    {
        return animator != null && animator.GetBool(PARAM_IS_STANDING);
    }
}
