using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance {get; private set;}
    [SerializeField] private Player player;
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
    }

    // Update is called once per frame
    void Update()
    {
        if(SceneManager.GetActiveScene().name == "Level1" && this.player.GetHealth() <= 0) 
        {
            if(!this.playerDead) 
            {
                this.playerDead = true;
                SceneManager.LoadScene("StartScene");
            }
            else 
            {
                this.playerDead = false;
                this.player.ResetHealth();
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
