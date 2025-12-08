using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages night cycles, coordinating spawners and emitting lifecycle events.
/// Extensible: spawners are configurable, and event signals allow loose coupling with other systems.
/// Singleton: only one active at a time, but reinitializes on new scene/save load.
/// </summary>
public class NightManager : MonoBehaviour
{
    #region Fields And Properties

    private static NightManager instance;
    public static NightManager Instance => instance;

    [Header("Night Configuration")]
    [SerializeField] private float nightDurationSeconds = 300f; // 5 minutes default

    [Header("Spawners")]
    [SerializeField] private List<SwarmSpawner> swarmSpawners = new List<SwarmSpawner>();

    #region Events
    public event Action OnNightStart;
    public event Action OnNightEnd;
    public event Action<float> OnNightProgress; // 0-1 normalized progress
    #endregion Events

    private float nightTimer = 0f;
    private bool isNightActive = false;

    public List<SwarmSpawner> GetSwarmSpawners() => swarmSpawners;
    public float GetNightElapsedTime() => nightTimer;
    public bool IsNightActive => isNightActive;

    #endregion Fields And Properties



    #region Unity Methods

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning("[NightManager] Instance already exists. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        instance = this;
    }
    
    private void Update()
    {
        if (!isNightActive) return;

        nightTimer += Time.deltaTime;

        // Emit progress event
        OnNightProgress?.Invoke(GetNightProgress());

        // Check if night has ended
        if (nightTimer >= nightDurationSeconds)
        {
            EndNight();
        }
    }

    #endregion Unity Methods



    #region Accessors

    /// <summary>
    /// Add a swarm spawner dynamically.
    /// </summary>
    public void AddSwarmSpawner(SwarmSpawner swarmSpawner)
    {
        if (swarmSpawner == null) return;
        swarmSpawners.Add(swarmSpawner);
    }

    /// <summary>
    /// Remove a swarm spawner dynamically.
    /// </summary>
    public void RemoveSwarmSpawner(SwarmSpawner swarmSpawner)
    {
        if (swarmSpawner == null) return;
        swarmSpawners.Remove(swarmSpawner);
    }

    public float GetNightProgress() => Mathf.Clamp01(nightTimer / nightDurationSeconds);
    
    #endregion Accessors



    #region Night Control

    /// <summary>
    /// Start a new night.
    /// Notifies all swarm spawners to prepare for the night.
    /// </summary>
    public void StartNight()
    {
        if (isNightActive)
        {
            Debug.LogWarning("[NightManager] Attempted to start night while one is already active.");
            return;
        }

        nightTimer = 0f;
        isNightActive = true;

        // Notify all swarm spawners
        foreach (var swarmSpawner in swarmSpawners)
        {
            swarmSpawner?.OnNightStart();
        }

        OnNightStart?.Invoke();
        Debug.Log($"[NightManager] Night started. Duration: {nightDurationSeconds}s");
    }

    /// <summary>
    /// End the current night immediately.
    /// Notifies all swarm spawners to clean up.
    /// </summary>
    public void EndNight()
    {
        if (!isNightActive)
        {
            Debug.LogWarning("[NightManager] Attempted to end night when none is active.");
            return;
        }

        isNightActive = false;

        // Notify all swarm spawners
        foreach (var swarmSpawner in swarmSpawners)
        {
            swarmSpawner?.OnNightEnd();
        }

        OnNightEnd?.Invoke();
        Debug.Log("[NightManager] Night ended.");
    }

    /// <summary>
    /// Pause the night timer (for scene transitions, menus, etc).
    /// </summary>
    public void PauseNight()
    {
        isNightActive = false;
    }

    /// <summary>
    /// Resume the night timer from where it was paused.
    /// </summary>
    public void ResumeNight()
    {
        if (nightTimer < nightDurationSeconds)
        {
            isNightActive = true;
        }
    }

    /// <summary>
    /// Set night duration (useful for testing or dynamic difficulty).
    /// </summary>
    public void SetNightDuration(float seconds)
    {
        nightDurationSeconds = Mathf.Max(0.1f, seconds);
    }

    #endregion Night Control

}
