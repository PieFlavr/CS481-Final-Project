using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

/// <summary>
/// Handles player-specific input and movement.
/// Replaces functionality from old PlayerControl script.
/// </summary>
public class PlayerController : EntityController
{
    [Header("Player Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float dashSpeed = 22f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private float dashCooldown = 0.25f;

    private Vector2 moveInput;
    private Vector2 lastMove;
    private float dashDurationTimer = 0f;
    private float dashCooldownTimer = 0f;

    private Camera mainCamera;

    // Track spell hold states
    private bool[] spellHeldStates = new bool[4];

    protected override void Awake()
    {
        base.Awake();
        mainCamera = Camera.main;
        lastMove = new UnityEngine.Vector2(1, 0);

        if (mainCamera == null)
        {
            Debug.LogError("PlayerController: No main camera found!");
        }
    }

    protected override void Initialize()
    {
        base.Initialize();
        Debug.Log("PlayerController: Player initialized");
    }

    protected override void UpdateBehavior()
    {
        // Apply movement (normal or dashing)
        if (dashDurationTimer <= 0)
        {
            // Normal movement
            body.linearVelocity = moveInput * moveSpeed;
        }
        // During dash, velocity is already set, don't override

        // Handle sprite flipping based on velocity
        if (body.linearVelocityX > 0 && !isFacingRight)
        {
            Flip(2);
        }
        else if (body.linearVelocityX < 0 && isFacingRight)
        {
            Flip(0);
        }

        // Update held spells
        UpdateHeldSpells();
    }

    /// <summary>
    /// Called by Input System when move input changes.
    /// </summary>
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        UnityEngine.Vector2 val = context.ReadValue<UnityEngine.Vector2>();
		if (val != new UnityEngine.Vector2(0,0))
		{
            lastMove = context.ReadValue<UnityEngine.Vector2>();
        }
    }

    /// <summary>
    /// Called by Input System when dash input is pressed.
    /// </summary>
    public void OnDash(InputAction.CallbackContext context)
    {
        // Only dash if moving and not already dashing/on cooldown
        if (context.performed && moveInput != Vector2.zero)
        {
            if (dashDurationTimer <= 0 && dashCooldownTimer <= 0)
            {
                // Start dash
                dashDurationTimer = dashDuration;
                dashCooldownTimer = dashDuration + dashCooldown;
                body.linearVelocity = moveInput * dashSpeed;

				if (moveInput == new UnityEngine.Vector2(0, 0))
				{
					body.linearVelocity = lastMove * dashSpeed;
				} else
                {
                    body.linearVelocity = moveInput * dashSpeed;
                }

                StartCoroutine(StopDash());

                Debug.Log("PlayerController: Dash activated");
            }
        }

        if (moveInput == Vector2.zero)
        {
            if (dashDurationTimer <= 0 && dashCooldownTimer <= 0)
            {
                // Start dash
                dashDurationTimer = dashDuration;
                dashCooldownTimer = dashDuration + dashCooldown;
				body.linearVelocity = lastMove * dashSpeed;

                StartCoroutine(StopDash());

                Debug.Log("PlayerController: Dash activated");
            }
        }
    }

    private void Flip(int direction)
    {
        body.transform.localScale = new Vector2(direction - 1, 1);
        isFacingRight = !isFacingRight;
    }

    private IEnumerator StopDash()
    {
        yield return new WaitForSeconds(dashDurationTimer);
        dashDurationTimer = 0;
        yield return new WaitForSeconds(dashCooldown);
        dashCooldownTimer = 0;
    }

    public void slow(bool trap)
    {
        if (trap)
        {
            moveSpeed = 1.5f;
            dashSpeed = 8f;
        } else
        {
            moveSpeed = 5f;
            dashSpeed = 22f;
        }
    }

    #region Player Spell Casting

    /// <summary>
    /// Called by Input System when spell 1 input changes state.
    /// </summary>
    public void OnCastSpell1(InputAction.CallbackContext context)
    {
        HandleSpellInputState(0, context);
    }

    /// <summary>
    /// Called by Input System when spell 2 input changes state.
    /// </summary>
    public void OnCastSpell2(InputAction.CallbackContext context)
    {
        HandleSpellInputState(1, context);
    }

    /// <summary>
    /// Called by Input System when spell 3 input changes state.
    /// </summary>
    public void OnCastSpell3(InputAction.CallbackContext context)
    {
        HandleSpellInputState(2, context);
    }

    /// <summary>
    /// Called by Input System when spell 4 input changes state.
    /// </summary>
    public void OnCastSpell4(InputAction.CallbackContext context)
    {
        HandleSpellInputState(3, context);
    }

    /// <summary>
    /// Handles spell input state transitions (started, performed, canceled).
    /// </summary>
    private void HandleSpellInputState(int slotIndex, InputAction.CallbackContext context)
    {
        if (spellCaster == null || mainCamera == null)
        {
            Debug.LogWarning("PlayerController: SpellCaster or MainCamera is null");
            return;
        }

        // Get spell to check if it requires holding
        Spell spell = spellCaster.GetSpell(slotIndex);
        if (spell == null)
        {
            Debug.LogWarning($"PlayerController: No spell in slot {slotIndex}");
            return;
        }

        // Handle different input phases
        if (context.started)
        {
            // Button pressed down
            OnSpellInputStarted(slotIndex, spell);
        }
        else if (context.performed)
        {
            // Button fully pressed (for instant cast spells)
            if (!spell.requiresHold)
            {
                OnSpellInputPerformed(slotIndex, spell);
            }
        }
        else if (context.canceled)
        {
            // Button released
            OnSpellInputCanceled(slotIndex, spell);
        }
    }

    /// <summary>
    /// Called when spell button is first pressed down.
    /// </summary>
    private void OnSpellInputStarted(int slotIndex, Spell spell)
    {
        Debug.Log($"PlayerController: Spell {slotIndex} input started");
        spellHeldStates[slotIndex] = true;

        // For instant cast spells, cast immediately on button down
        if (!spell.requiresHold)
        {
            CastSpellFromInput(slotIndex);
        }
    }

    /// <summary>
    /// Called when spell button is fully pressed (performed phase).
    /// Used for instant cast spells.
    /// </summary>
    private void OnSpellInputPerformed(int slotIndex, Spell spell)
    {
        Debug.Log($"PlayerController: Spell {slotIndex} input performed");
        // Instant cast spells already cast in OnSpellInputStarted
    }

    /// <summary>
    /// Called when spell button is released.
    /// </summary>
    private void OnSpellInputCanceled(int slotIndex, Spell spell)
    {
        Debug.Log($"PlayerController: Spell {slotIndex} input canceled");
        spellHeldStates[slotIndex] = false;

        // For hold-to-cast spells, cast when button is released
        if (spell.requiresHold)
        {
            CastSpellFromInput(slotIndex);
        }
    }

    /// <summary>
    /// Updates held spells every frame (for channeling/charging spells).
    /// </summary>
    private void UpdateHeldSpells()
    {
        if (spellCaster == null)
            return;

        for (int i = 0; i < spellHeldStates.Length; i++)
        {
            if (spellHeldStates[i])
            {
                Spell spell = spellCaster.GetSpell(i);
                if (spell != null && spell.updateWhileHeld)
                {
                    // Call spell's OnHeldUpdate for channeling/charging logic
                    spell.OnHeldUpdate(entity, Time.deltaTime);
                }
            }
        }
    }

    /// <summary>
    /// Casts a spell at the specified slot using mouse position for targeting.
    /// </summary>
    private void CastSpellFromInput(int slotIndex)
    {
        if (spellCaster == null || mainCamera == null)
        {
            Debug.LogWarning("PlayerController: SpellCaster or MainCamera is null");
            return;
        }

        // Get mouse position in world space (using new Input System)
        Vector2 mouseWorldPos = GetMouseWorldPosition();

        // Try to find entity at mouse position
        Entity targetEntity = GetEntityAtPosition(mouseWorldPos);

        // Debug visualization
        if (showDebug && targetEntity != null)
        {
            Debug.DrawLine(transform.position, targetEntity.transform.position, Color.green, 0.1f);
        }

        // Cast the spell
        if (targetEntity != null)
        {
            CastSpellAtTarget(slotIndex, targetEntity);
        }
        else
        {
            CastSpellAtPosition(slotIndex, mouseWorldPos);
        }
    }

    /// <summary>
    /// Gets the mouse position in world space using the new Input System.
    /// </summary>
    private Vector2 GetMouseWorldPosition()
    {
        if (mainCamera == null)
            return Vector2.zero;

        // Use new Input System
        if (Mouse.current == null)
            return Vector2.zero;

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        return mainCamera.ScreenToWorldPoint(mouseScreenPos);
    }

    /// <summary>
    /// Checks if a spell is currently being held.
    /// </summary>
    public bool IsSpellHeld(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= spellHeldStates.Length)
            return false;

        return spellHeldStates[slotIndex];
    }

    /// <summary>
    /// Draws debug gizmos for player targeting visualization.
    /// </summary>
    protected override void OnDrawGizmos()
    {
        if (!showDebug || mainCamera == null || Mouse.current == null)
            return;

        // Draw mouse position
        Vector2 mouseWorldPos = GetMouseWorldPosition();
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(mouseWorldPos, 0.3f);

        // Draw targeting line to entity under mouse
        Entity target = GetEntityAtPosition(mouseWorldPos);
        if (target != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, target.transform.position);
            Gizmos.DrawWireSphere(target.transform.position, 0.5f);
        }

        // Draw indicators for held spells
        for (int i = 0; i < spellHeldStates.Length; i++)
        {
            if (spellHeldStates[i])
            {
                Gizmos.color = Color.cyan;
                Vector3 offset = new Vector3(0, 1f + (i * 0.3f), 0);
                Gizmos.DrawWireSphere(transform.position + offset, 0.2f);
            }
        }
    }

    #endregion
}