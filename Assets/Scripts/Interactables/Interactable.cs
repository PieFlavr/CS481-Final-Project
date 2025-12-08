using UnityEngine;
using System;

/// <summary>
/// Base class for interactable objects (buttons, terminals, cameras, sensors, mines, traps, etc).
/// Self-manages proximity detection. Emits events when target comes in range and on interaction.
/// Target-agnostic: works with player, enemies, or any entity type via IsValidTarget() override.
/// Behavior components listen to OnTargetInRange/OutOfRange/OnInteract events.
/// </summary>
public abstract class Interactable : MonoBehaviour
{
    #region Fields And Properties

    [SerializeField] private float proximityRange = 2f;
    [Tooltip("Optional: if assigned, uses this collider for proximity checks. Otherwise gets component on Start.")]
    [SerializeField] private Collider2D triggerCollider;
    [Tooltip("Text displayed in the interaction prompt when target is in range.")]
    [SerializeField] private string interactionPrompt = "Interact";

    #region Events
    public event Action OnTargetInRange;
    public event Action OnTargetOutOfRange;
    public event Action OnInteract;
    #endregion Events

    private bool isTargetInRange = false;
    public bool IsTargetInRange => isTargetInRange;
    public string InteractionPrompt => interactionPrompt;

    #endregion Fields And Properties

    #region Unity Methods

    protected virtual void Start()
    {
        if (triggerCollider == null)
            triggerCollider = GetComponent<Collider2D>();

        if (triggerCollider == null)
            Debug.LogWarning($"[Interactable] {gameObject.name} has no Collider2D assigned or found on Start.", gameObject);
    }

    protected virtual void OnEnable()
    {
        if (PlayerInteractionManager.Instance != null)
            PlayerInteractionManager.Instance.RegisterInteractable(this);
    }

    protected virtual void OnDisable()
    {
        if (PlayerInteractionManager.Instance != null)
            PlayerInteractionManager.Instance.UnregisterInteractable(this);
    }

    #endregion Unity Methods

    #region Proximity Detection

    /// <summary>
    /// Called by InteractionManager each frame for relevant target types.
    /// Override to implement custom detection logic (cone, motion-detection, etc).
    /// </summary>
    public virtual void CheckProximity(Transform targetTransform)
    {
        if (targetTransform == null || !IsValidTarget(targetTransform)) return;

        float distance = Vector2.Distance(transform.position, targetTransform.position);
        bool inRange = distance <= proximityRange;

        UpdateProximityState(inRange);
    }

    /// <summary>
    /// Override to filter which target types can trigger this interactable.
    /// Default: any target is valid.
    /// </summary>
    protected virtual bool IsValidTarget(Transform targetTransform)
    {
        return true;
    }

    /// <summary>
    /// Updates proximity state and emits events.
    /// Override if you need custom state update logic.
    /// </summary>
    protected virtual void UpdateProximityState(bool inRange)
    {
        if (inRange && !isTargetInRange)
        {
            isTargetInRange = true;
            OnTargetInRange?.Invoke();
        }
        else if (!inRange && isTargetInRange)
        {
            isTargetInRange = false;
            OnTargetOutOfRange?.Invoke();
        }
    }

    #endregion Proximity Detection

    #region Interaction

    /// <summary>
    /// Called when a target attempts to interact. Called by InteractionManager.TryInteract().
    /// </summary>
    public void Interact()
    {
        if (!isTargetInRange)
        {
            Debug.LogWarning($"[Interactable] Attempted to interact with {gameObject.name} but target is out of range.", gameObject);
            return;
        }

        OnInteract?.Invoke();
        ExecuteInteraction();
    }

    /// <summary>
    /// Override in subclasses to define specific interaction behavior.
    /// Called after OnInteract event is emitted.
    /// </summary>
    protected abstract void ExecuteInteraction();

    #endregion Interaction
}
