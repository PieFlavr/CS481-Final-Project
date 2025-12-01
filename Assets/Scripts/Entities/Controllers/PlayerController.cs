using UnityEngine;

/// <summary>
/// Controls player movement, rotation, and animation based on input from InputManager.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    #region Serialized Fields
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [Header("Combat")]
    [SerializeField] private float attackRange = 1.0f;
    [SerializeField] private float attackRadius = 0.5f;
    #endregion

    #region Private Fields
    private Rigidbody2D rb;
    private Animator animator;
    
    private InputManager inputManager;
    private PlayerEntity playerEntity;
    
    // Animator parameter names
    private const string XParam = "X";
    private const string YParam = "Y";
    private const string LastXParam = "LastX";
    private const string LastYParam = "LastY";
    private const string IsMovingParam = "isMoving";
    private const string IsAttackingParam = "isAttacking";
    #endregion

    #region Unity Methods
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerEntity = GetComponent<PlayerEntity>();
        
    }

    private void Start()
    {
        inputManager = InputManager.Instance;
        if (inputManager == null)
        {
            Debug.LogError("[PlayerController] InputManager instance not found!");
            enabled = false;
            return;
        }

        // Subscribe to input events
        inputManager.OnMove += HandleMovement;
        inputManager.OnAttack += HandleAttack;

        Debug.Log("[PlayerController] Initialized successfully.");
    }

    private void FixedUpdate()
    {
        // Handle movement in FixedUpdate for physics
        if (!inputManager.IsAttacking && inputManager.IsMoving)
        {
            float speed = moveSpeed;
            if (playerEntity != null && playerEntity.Stats != null)
                speed = playerEntity.Stats.Speed;

            Vector2 movement = inputManager.MoveInput * speed;
            rb.linearVelocity = movement;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void Update()
    {
    UpdateAnimations();
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (inputManager != null)
        {
            inputManager.OnMove -= HandleMovement;
            inputManager.OnAttack -= HandleAttack;
        }
    }
    #endregion

    #region Movement & Animation
    private void HandleMovement(Vector2 moveInput)
    {
        // Movement is handled in FixedUpdate
        // This is just for any additional logic if needed
    }

    private void UpdateAnimations()
    {
        // Update animator with values from InputManager
        animator.SetFloat(XParam, inputManager.X);
        animator.SetFloat(YParam, inputManager.Y);
        animator.SetBool(IsMovingParam, inputManager.IsMoving);
        animator.SetBool(IsAttackingParam, inputManager.IsAttacking);

        // LastX/LastY should represent where the character is facing
        if (inputManager.IsMoving)
        {
            // When moving, follow the movement input
            animator.SetFloat(LastXParam, inputManager.LastX);
            animator.SetFloat(LastYParam, inputManager.LastY);
        }
        else
        {
            // When idle, face the mouse cursor
            Vector2 lookDir = inputManager.LookDirection;
            if (lookDir.sqrMagnitude > 0.001f)
            {
                animator.SetFloat(LastXParam, lookDir.x);
                animator.SetFloat(LastYParam, lookDir.y);
            }
        }
    }
    #endregion

    #region Combat
    private void HandleAttack()
    {
        Debug.Log("[PlayerController] Attack triggered!");
        if (playerEntity == null)
        {
            Debug.LogWarning("[PlayerController] No PlayerEntity found; cannot deal damage.");
            return;
        }

        // Determine damage amount from player stats if available
        float damage = 1f;
        if (playerEntity.Stats != null)
            damage = playerEntity.Stats.Damage;

        // Compute attack origin in the look direction (fallback to facing last animation direction)
        Vector2 lookDir = inputManager != null ? inputManager.LookDirection : Vector2.zero;
        if (lookDir.sqrMagnitude < 0.001f)
        {
            // Fallback: use animator last facing values
            float lx = animator.GetFloat(LastXParam);
            float ly = animator.GetFloat(LastYParam);
            lookDir = new Vector2(lx, ly);
            if (lookDir.sqrMagnitude < 0.001f)
                lookDir = Vector2.up; // default forward
        }

        // Prefer stats-provided range/radius when available
        float range = attackRange;
        float radius = attackRadius;
        if (playerEntity.Stats != null)
        {
            if (playerEntity.Stats.AttackRange > 0f) range = playerEntity.Stats.AttackRange;
            if (playerEntity.Stats.AttackRadius > 0f) radius = playerEntity.Stats.AttackRadius;
        }

        Vector2 origin = (Vector2)transform.position + lookDir.normalized * range;

        // Detect colliders in attack area
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, radius);
        bool hitAny = false;
        foreach (var c in hits)
        {
            if (c == null) continue;

            // Try to get EnemyEntity from collider or its parents
            EnemyEntity enemy = c.GetComponentInParent<EnemyEntity>();
            if (enemy != null && enemy.IsAlive)
            {
                enemy.TakeDamage(damage);
                hitAny = true;
                Debug.Log($"[PlayerController] Hit enemy {enemy.EntityID} for {damage} damage.");
            }
        }

        if (!hitAny)
            Debug.Log("[PlayerController] Attack hit nothing.");
    }
    #endregion
}
