using UnityEngine;

/// <summary>
/// Example interactable door that can be opened/closed on interaction.
/// Demonstrates custom Interactable behavior.
/// </summary>
public class DoorInteractable : Interactable
{
    [SerializeField] private bool isOpen = false;
    [SerializeField] private float openDuration = 0.5f;
    [SerializeField] private Vector3 openPosition = Vector3.up * 2f;

    private Vector3 closedPosition;
    private float animationTimer = 0f;
    private bool isAnimating = false;

    protected override void Start()
    {
        base.Start();
        closedPosition = transform.position;
    }

    protected override void ExecuteInteraction()
    {
        if (isAnimating) return;
        
        isOpen = !isOpen;
        isAnimating = true;
        animationTimer = 0f;
        
        Debug.Log($"[DoorInteractable] Door {(isOpen ? "opening" : "closing")}.");
    }

    private void Update()
    {
        if (!isAnimating) return;

        animationTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(animationTimer / openDuration);

        Vector3 targetPos = isOpen ? closedPosition + openPosition : closedPosition;
        transform.position = Vector3.Lerp(transform.position, targetPos, progress);

        if (progress >= 1f)
        {
            isAnimating = false;
        }
    }
}
