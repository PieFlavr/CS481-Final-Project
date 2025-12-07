using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Fields and Properties
    public static GameManager Instance { get; private set; } // Singleton instance
    public GameState State { get; private set; }
    #endregion Fields and Properties



    #region Unity Methods
    void Awake()
    {
        Debug.Log("[GameManager] Hello World!");
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[GameManager] Duplicate instance detected -- annihilating it now!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Initialize();
    }

    void Start()
    {
        SubscribeToInputs();
        AudioManager.Instance.PlayBGM("TestBGM");
    }

    void Initialize()
    {
        State = GameState.Playing;
        // TODO: Add other initialization logic here -L
    }
    
    #endregion Unity Methods
    


    #region State Management
    /// <summary>
    /// Changes the current game state to the specified new state.
    /// </summary>
    /// <remarks>If the current state is already equal to <paramref name="newState"/>, no action is taken.
    /// Otherwise, the state is updated, and the <see cref="OnGameStateChanged"/> event is invoked with the new
    /// state.</remarks>
    /// <param name="newState">The new game state to transition to.</param>
    public void ChangeState(GameState newState)
    {
        if (State != newState) { 
            Debug.Log("[GameManager] Changing state from " + State + " to " + newState + "...");
            State = newState;
            OnGameStateChanged?.Invoke(newState);
        }
        
    }

    /// <summary>
    /// Occurs when the game state changes.
    /// </summary>
    /// <remarks>Subscribe to this event to be notified whenever the game state transitions to a new value. 
    /// The event provides the new <see cref="GameState"/> as its parameter.</remarks>
    public event Action<GameState> OnGameStateChanged;  // Discovered this recently its awesome -L
    #endregion State Management



    #region Input Handling

    private void SubscribeToInputs()
    {
        InputManager.Instance.OnBack += HandleBackInput;
        InputManager.Instance.OnPause += HandlePauseInput;
    }

    private void HandleBackInput()
    {
        Debug.Log("[GameManager] Back input received.");
        if (UIManager.Instance.HasOpenOverlay())
        {
            Debug.Log("[GameManager] Closing top overlay panel.");
            UIManager.Instance.GoBack();

            // NOTE (L): Repeated twice in HandlePauseInput() for a reason
            // This is because in the case "Back" and "Pause" are different binds :P. -L
            
            if (State == GameState.Paused && !UIManager.Instance.IsPanelOpen("PausePanel"))
            {
                ChangeState(GameState.Playing);
            }
        }
    }

    private void HandlePauseInput()
    {
        Debug.Log("[GameManager] Pause input received.");
        if (State == GameState.Playing)
        {
            UIManager.Instance.OpenPanel("PausePanel");
            ChangeState(GameState.Paused);
        } 
        else if (State == GameState.Paused && UIManager.Instance.IsPanelOpen("PausePanel"))
        {
            UIManager.Instance.ClosePanel("PausePanel");
            ChangeState(GameState.Playing);
        }
    }

    #endregion Input Handling
}

public enum GameState
{
    Boot,
    MainMenu,
    Loading, 
    Playing,
    Paused,
}