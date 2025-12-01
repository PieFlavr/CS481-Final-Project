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
    #endregion

    #region Private Fields
    private Rigidbody2D rb;
    private Animator animator;
    private InputManager inputManager;
    
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
    }

    private void FixedUpdate()
    {
        // Handle movement in FixedUpdate for physics
        if (!inputManager.IsAttacking && inputManager.IsMoving)
        {
            Vector2 movement = inputManager.MoveInput * moveSpeed;
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
        // TODO: Implement attack logic here
        // For now, the animation will play via the isAttacking parameter
    }
    #endregion
}
