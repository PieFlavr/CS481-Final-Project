using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }
    [SerializeField] private PlayerEntity player;

    public static event Action PlayerDied;
    public static event Action PlayerWon;
    private bool playerDead = false;
    private bool playerWon = false;
    private bool hasSeenEnemies = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Debug.Log("[PlayerManager] Hello World!");
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[PlayerManager] Duplicate instance detected -- annihilating it now!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        // Ensure we detect player instances when scenes load
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Try to find a PlayerEntity in the newly loaded scene (if any)
        if (player == null)
        {
            var found = FindAnyObjectByType<PlayerEntity>();
            if (found != null)
            {
                player = found;
                Debug.Log($"[PlayerManager] Assigned PlayerEntity from scene: {found.name}");

            }
        }

        Restart();
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene().name == "Level1" && player != null)
        {
            if (player.Stats != null && player.Stats.IsDead())
            {
                Debug.Log("[PlayerManager] Player has died.");
                if (!this.playerDead)
                {
                    this.playerDead = true;
                    Time.timeScale = 0.0f;
                    player.Stats.ResetHealth();
                    PlayerDied?.Invoke();
                }
            }

            CheckForWinCondition();
        }
    }

    public void Restart()
    {
        this.playerDead = false;
        this.playerWon = false;
        this.hasSeenEnemies = false;
    }

    public bool GetPlayerDead()
    {
        return this.playerDead;
    }

    private void CheckForWinCondition()
    {
        if (playerWon || playerDead) return;
        if (EntityManager.Instance == null) return;

        int aliveEnemies = EntityManager.Instance.GetEntityCount(EntityType.Enemy);

        if (aliveEnemies > 0)
        {
            hasSeenEnemies = true;
            return;
        }

        if (!hasSeenEnemies) return; // Avoid false positives before any enemies spawn

        Debug.Log("[PlayerManager] All enemies defeated. Player has won.");
        playerWon = true;
        Time.timeScale = 0.0f;
        PlayerWon?.Invoke();
        this.Restart();
    }


}
