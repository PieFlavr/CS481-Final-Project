using UnityEngine;

/// <summary>
/// Button that increases the first variable in a BehaviorVariableController.
/// </summary>
public class IncreaseVariableButton : Interactable
{
    [SerializeField] private BehaviorGraphVariableController variableController;

    protected override void ExecuteInteraction()
    {
        if (variableController == null)
        {
            Debug.LogError("[IncreaseVariableButton] BehaviorVariableController not assigned!", gameObject);
            return;
        }

        variableController.IncreaseVariable(0);
        Debug.Log("[IncreaseVariableButton] Increased variable at index 0.");
    }
}
