using UnityEngine;
using UnityEngine.InputSystem;
using System;   

/// <summary>
/// Manages player input using the new Unity Input System.
/// Unwraps input actions and provides events for other systems to subscribe to.
/// </summary>
public class InputManager : MonoBehaviour
{
    #region Fields and Properties
    public static InputManager Instance { get; private set; }
    
    private InputSystem_Actions inputActions;

    #region Events
    // public event Action<Vector2> OnMove; //TODO: Implement movement input w/ player controller. -L
    // public event Action<Vector2> OnLook;
    // public event Action OnDash;
    public event Action OnInteract;
    public event Action OnPause;
    public event Action OnBack;

    #endregion Events

    // Public Properties
    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }

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
            Destroy(gameObject);
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
    /// </summary>
    private void SubscribeToInputs()
    {
        // Add input subscriptions here... (preferably group em' too)
        inputActions.Player.Interact.performed += context => OnInteract?.Invoke();
        inputActions.Player.Pause.performed += context => OnPause?.Invoke();

        inputActions.UI.Back.performed += context => OnBack?.Invoke();
    }

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

    #region Cleanup
    private void OnDestroy()
    {
        inputActions.Player.Interact.performed -= context => OnInteract?.Invoke();
        inputActions.Player.Pause.performed -= context => OnPause?.Invoke();

        inputActions.UI.Back.performed -= context => OnBack?.Invoke();
    }
    #endregion Cleanup
}