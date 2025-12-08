using UnityEngine;

/// <summary>
/// Simple interactable button that starts a night when activated.
/// Example implementation of Interactable.
/// </summary>
public class NightStartButton : Interactable
{
    [SerializeField] private bool autoStartOnInteract = true;

    protected override void ExecuteInteraction()
    {
        if (!autoStartOnInteract) return;

        if (NightManager.Instance != null)
        {
            NightManager.Instance.StartNight();
            Debug.Log("[NightStartButton] Night started via button interaction.");
        }
        else
        {
            Debug.LogError("[NightStartButton] NightManager instance not found!");
        }
    }
}
