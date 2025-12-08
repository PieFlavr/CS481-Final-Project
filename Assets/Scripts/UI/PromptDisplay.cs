using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Displays interaction prompts when the associated Interactable is in range.
/// Listens to Interactable events and manages prompt UI visibility/position.
/// Attach to the same GameObject as an Interactable component.
/// </summary>
public class PromptDisplay : MonoBehaviour
{
    private static bool globalPromptsEnabled = true;
    private static List<PromptDisplay> activePrompts = new List<PromptDisplay>();

    [SerializeField] private Interactable interactable;
    [SerializeField] private Canvas promptCanvas;

    /// <summary>
    /// Globally suppress or enable all prompts (e.g., during pause/menus).
    /// </summary>
    public static void SetGlobalPromptsEnabled(bool enabled)
    {
        globalPromptsEnabled = enabled;
        foreach (var prompt in activePrompts)
        {
            if (prompt.promptCanvas != null)
                prompt.promptCanvas.enabled = enabled;
        }
    }

    private void OnEnable()
    {
        Debug.Log("[PromptDisplay] Enabled and registering with Interactable.");
        if (interactable == null)
            interactable = GetComponent<Interactable>();

        if (interactable != null)
        {
            interactable.OnTargetInRange += ShowPrompt;
            interactable.OnTargetOutOfRange += HidePrompt;
        }

        if (!activePrompts.Contains(this))
            activePrompts.Add(this);
    }

    private void OnDisable()
    {
        if (interactable != null)
        {
            interactable.OnTargetInRange -= ShowPrompt;
            interactable.OnTargetOutOfRange -= HidePrompt;
        }

        activePrompts.Remove(this);
    }

    private void ShowPrompt()
    {
        if (promptCanvas != null)
            promptCanvas.enabled = globalPromptsEnabled;
    }

    private void HidePrompt()
    {
        if (promptCanvas != null)
            promptCanvas.enabled = false;
    }

    /// <summary>
    /// Update prompt text dynamically (optional).
    /// </summary>
    public void SetPromptText(string text)
    {
        if (promptCanvas == null) return;
        
        var textComponent = promptCanvas.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (textComponent != null)
            textComponent.text = text;
    }
}
