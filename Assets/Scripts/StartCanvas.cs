using UnityEngine;

public class StartCanvas : MonoBehaviour
{
    public static StartCanvas Instance {get; private set;}
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
        DontDestroyOnLoad(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
