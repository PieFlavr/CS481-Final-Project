using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manager for player interactable proximity detection and UI integration.
/// Tracks nearest player-targetable interactable (buttons, doors, terminals, etc) for UI prompts.
/// Interactables self-manage proximity checks; this manager provides nearest-target queries for UI.
/// Singleton pattern.
/// </summary>
public class PlayerInteractionManager : MonoBehaviour
{
    private static PlayerInteractionManager instance;
    public static PlayerInteractionManager Instance => instance;

    #region Fields And Properties

    [SerializeField] private Transform playerTransform;
    private List<Interactable> registeredInteractables = new List<Interactable>();
    private Interactable nearestInteractable;

    #endregion Fields And Properties

    #region Unity Methods

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning("[PlayerInteractionManager] Instance already exists. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void Start()
    {
        if (playerTransform == null)
        {
            // Auto-find player if not assigned
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
            else
                Debug.LogError("[PlayerInteractionManager] Player not found! Assign manually or tag player with 'Player'.");
        }

        // Auto-discover all interactables in scene
        Interactable[] allInteractables = FindObjectsByType<Interactable>(FindObjectsSortMode.None);
        foreach (var interactable in allInteractables)
        {
            RegisterInteractable(interactable);
        }

        Debug.Log($"[PlayerInteractionManager] Found and registered {registeredInteractables.Count} player interactables.");
    }

    private void Update()
    {
        if (playerTransform == null) return;

        UpdateProximity();
    }

    #endregion Unity Methods

    #region Registration

    /// <summary>
    /// Register a new interactable (called automatically by Interactable.OnEnable or manually).
    /// </summary>
    public void RegisterInteractable(Interactable interactable)
    {
        if (interactable == null || registeredInteractables.Contains(interactable)) return;
        registeredInteractables.Add(interactable);
    }

    /// <summary>
    /// Unregister an interactable (called automatically by Interactable.OnDisable or manually).
    /// </summary>
    public void UnregisterInteractable(Interactable interactable)
    {
        if (interactable == null) return;
        registeredInteractables.Remove(interactable);

        if (nearestInteractable == interactable)
            nearestInteractable = null;
    }

    #endregion Registration

    #region Proximity Detection

    /// <summary>
    /// Update proximity for all interactables, track the nearest one.
    /// </summary>
    private void UpdateProximity()
    {
        Interactable newNearest = null;
        float closestDistance = float.MaxValue;

        foreach (var interactable in registeredInteractables)
        {
            if (interactable == null) continue;

            interactable.CheckProximity(playerTransform);

            float distance = Vector2.Distance(playerTransform.position, interactable.transform.position);
            if (distance < closestDistance && interactable.IsTargetInRange)
            {
                closestDistance = distance;
                newNearest = interactable;
            }
        }

        // Update nearest interactable if changed
        if (newNearest != nearestInteractable)
        {
            nearestInteractable = newNearest;
            OnNearestInteractableChanged();
        }
    }

    /// <summary>
    /// Called when the nearest interactable changes. Override or use events as needed.
    /// </summary>
    private void OnNearestInteractableChanged()
    {
        // UI can hook here to update which prompt is showing
        Debug.Log($"[PlayerInteractionManager] Nearest interactable: {(nearestInteractable != null ? nearestInteractable.gameObject.name : "None")}");
    }

    /// <summary>
    /// Get the currently nearest interactable in range.
    /// </summary>
    public Interactable GetNearestInteractable() => nearestInteractable;

    #endregion Proximity Detection

    #region Interaction

    /// <summary>
    /// Attempt to interact with the nearest interactable.
    /// Call this from InputManager when player presses interact key.
    /// </summary>
    public void TryInteract()
    {
        if (nearestInteractable == null)
        {
            Debug.LogWarning("[PlayerInteractionManager] No interactable in range to interact with.");
            return;
        }

        nearestInteractable.Interact();
    }

    #endregion Interaction
}
