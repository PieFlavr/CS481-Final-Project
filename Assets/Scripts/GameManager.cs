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
            Debug.LogWarning("[GameManager] Duplicate instance detected -- annihilatin it now!");
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        Initialize();
    }

    void Initialize()
    {
        State = GameState.Boot;
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
}

public enum GameState
{
    Boot,
    MainMenu,
    Loading, 
    Playing,
    Paused,
}