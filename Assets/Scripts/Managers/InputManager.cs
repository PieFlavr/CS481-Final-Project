using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages player input using the new Unity Input System.
/// Unwraps input actions and provides events for other systems to subscribe to.
/// </summary>
public class InputManager : MonoBehaviour
{
    #region Fields and Properties
    public static InputManager Instance { get; private set; }
    
    private InputSystem_Actions inputActions;
    [SerializeField, Min(0f)] private float attackDuration = 0.3f;
    private Coroutine attackRoutine;

    #region Events
    public event Action<Vector2> OnMove;
    public event Action OnAttack;
    public event Action OnInteract;
    public event Action OnPause;
    public event Action OnBack;

    #endregion Events

    // Public Properties
    public Vector2 MoveInput { get; private set; }
    public Vector2 LookDirection { get; private set; }
    
    // Animation Variables
    public float X { get; private set; }
    public float Y { get; private set; }
    public float LastX { get; private set; }
    public float LastY { get; private set; }
    public bool IsMoving { get; private set; }
    public bool IsAttacking { get; private set; }

    // Rebinding
    //private const string RebindsKey = "InputRebinds"; //TODO: Implement input rebinding system. -L
    #endregion Fields and Properties

    #region Unity Methods
    private void Awake()
    {
        Debug.Log("[InputManager] Hello World!");
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[InputManager] Duplicate instance detected -- annihilating it now!");
            if(gameObject != null) 
            {
                Destroy(gameObject);
            }
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize & Setup Input Actions
        inputActions = new InputSystem_Actions();
        SubscribeToInputs();
    }

    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();
    #endregion Unity Methods

    #region Input Subscriptions
    /// <summary>
    /// Subscribes to input action events.
    /// Note: Move and Attack are now handled by PlayerInput component callbacks.
    /// </summary>
    private void SubscribeToInputs()
    {
        // Interact & UI (these are still manually subscribed)
        inputActions.Player.Interact.performed += context => OnInteract?.Invoke();
        inputActions.Player.Pause.performed += context => OnPause?.Invoke();

        inputActions.UI.Back.performed += context => OnBack?.Invoke();
    }

    /// <summary>
    /// Updates movement input and animation variables.
    /// Prevents movement while attacking.
    /// </summary>
    private void UpdateMovement(Vector2 moveInput)
    {
        MoveInput = moveInput;
        X = moveInput.x;
        Y = moveInput.y;

        // Update last direction if moving
        if (moveInput != Vector2.zero)
        {
            LastX = X;
            LastY = Y;
        }

        bool hasInput = moveInput != Vector2.zero;
        IsMoving = hasInput && !IsAttacking;

        OnMove?.Invoke(moveInput);
    }

    /// <summary>
    /// Handles attack input. Can be expanded when attack logic is implemented.
    /// </summary>
    private void HandleAttack()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
        }

        attackRoutine = StartCoroutine(AttackRoutine());
    }

    private void Update()
    {
        // Update look direction to face mouse position
        UpdateMouseLookDirection();
    }

    /// <summary>
    /// Updates the character's look direction based on mouse position.
    /// </summary>
    private void UpdateMouseLookDirection()
    {
        Vector2 mouseScreenPos = Pointer.current.position.ReadValue();
        Vector3 mousePos = mouseScreenPos;
        mousePos.z = 10f; // Distance from camera
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);
        
        // Calculate direction from character to mouse
        LookDirection = (worldMousePos - transform.position).normalized;
    }

    #region Public Input Methods (for PlayerInput component binding)
    /// <summary>
    /// Public method for Move action that can be bound in the PlayerInput component.
    /// </summary>
    public void OnMoveAction(InputAction.CallbackContext context)
    {
        UpdateMovement(context.ReadValue<Vector2>());
    }

    /// <summary>
    /// Public method for Attack action that can be bound in the PlayerInput component.
    /// </summary>
    public void OnAttackAction(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            HandleAttack();
        }
    }
    #endregion Public Input Methods

    public void EnableGameplayInput()
    {
        inputActions.Player.Enable();
        inputActions.UI.Disable();
    }

    public void EnableUIInput()
    {
        inputActions.Player.Disable();
        inputActions.UI.Enable();
    }
    #endregion Input Subscriptions

    private IEnumerator AttackRoutine()
    {
        IsAttacking = true;
        IsMoving = false;
        OnAttack?.Invoke();

        yield return new WaitForSeconds(attackDuration);

        IsAttacking = false;
        IsMoving = MoveInput != Vector2.zero;
        attackRoutine = null;
    }

    #region Cleanup
    private void OnDestroy()
    {
        if(inputActions != null) {
            inputActions.Player.Interact.performed -= context => OnInteract?.Invoke();
            inputActions.Player.Pause.performed -= context => OnPause?.Invoke();

            inputActions.UI.Back.performed -= context => OnBack?.Invoke();
        }
    }
    #endregion Cleanup
}