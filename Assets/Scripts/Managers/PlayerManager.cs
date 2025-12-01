using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance {get; private set;}
    [SerializeField] private PlayerEntity player;
    private bool playerDead = false;
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
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene().name == "Level1" && player != null)
        {
            if (player.Stats != null && player.Stats.IsDead())
            {
                Debug.Log("[PlayerManager] Player has died, reloading start scene.");
                if (!this.playerDead)
                {
                    this.playerDead = true;
                    SceneManager.LoadScene("StartScene");
                }
                else
                {
                    this.playerDead = false;
                    player.Stats.ResetHealth();
                }
            }
        }
    }

    public void Restart() 
    {
        this.playerDead = false;
    }

    public bool GetPlayerDead() 
    {
        return this.playerDead;
    }

    
}
