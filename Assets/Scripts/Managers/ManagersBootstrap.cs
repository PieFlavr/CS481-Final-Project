using UnityEngine;

/// <summary>
/// Bootstrap script for a Managers prefab.
/// </summary>
public class ManagersBootstrap : MonoBehaviour
{
    private static ManagersBootstrap instance;

    void Awake()
    {
        Debug.Log("[ManagersBootstrap] Hello World!");
        if (instance != null && instance != this)
        {
            Debug.LogWarning("[ManagersBootstrap] Duplicate managers prefab detected. Destroying new instance.");
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

}

