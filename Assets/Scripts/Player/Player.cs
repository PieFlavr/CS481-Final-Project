using UnityEngine;

public class Player : MonoBehaviour
{
    private static Player Instance {get; set;}
    [SerializeField] private int health; //Do value in inspector
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("[Player] Hello World!");
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[Player] Duplicate instance detected -- annihilating it now!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void DoDamage(int damage) 
    {
        this.health -= damage;
    }

    public int GetHealth() 
    {
        return this.health;
    }

    public void ResetHealth() 
    {
        this.health = 10;
    }
}
