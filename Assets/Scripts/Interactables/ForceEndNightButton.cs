using UnityEngine;

/// <summary>
/// Interactable button that force-ends the current night when activated.
/// Example implementation of Interactable.
/// </summary>
public class ForceEndNightButton : Interactable
{
    [SerializeField] private bool autoEndOnInteract = true;

    protected override void ExecuteInteraction()
    {
        if (!autoEndOnInteract) return;

        if (NightManager.Instance != null)
        {
            NightManager.Instance.EndNight();
            Debug.Log("[ForceEndNightButton] Night force-ended via button interaction.");
        }
        else
        {
            Debug.LogError("[ForceEndNightButton] NightManager instance not found!");
        }
    }
}
