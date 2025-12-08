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

    [SerializeField] private Collider2D proximityCollider;
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
        if (proximityCollider == null)
            proximityCollider = GetComponent<Collider2D>();

        if (proximityCollider == null)
            Debug.LogWarning($"[Interactable] {gameObject.name} has no Collider2D assigned or found on Start.", gameObject);
        else if (!proximityCollider.isTrigger)
            Debug.LogWarning($"[Interactable] {gameObject.name}'s Collider2D is not set as a trigger. Set it to trigger for proximity detection.", gameObject);
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
        if (targetTransform == null || proximityCollider == null || !IsValidTarget(targetTransform))
        {
            UpdateProximityState(false);
            return;
        }

        Collider2D targetCollider = targetTransform.GetComponent<Collider2D>();
        if (targetCollider == null)
        {
            UpdateProximityState(false);
            return;
        }

        // Check if target collider overlaps or is touching proximity collider
        bool inRange = proximityCollider.IsTouching(targetCollider) || Physics2D.GetContacts(proximityCollider, new Collider2D[1]) > 0;
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
