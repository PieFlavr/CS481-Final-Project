using UnityEngine;

/// <summary>
/// Button that decreases the first variable in a BehaviorVariableController.
/// </summary>
public class DecreaseVariableButton : Interactable
{
    [SerializeField] private BehaviorGraphVariableController variableController;

    protected override void ExecuteInteraction()
    {
        if (variableController == null)
        {
            Debug.LogError("[DecreaseVariableButton] BehaviorVariableController not assigned!", gameObject);
            return;
        }

        variableController.DecreaseVariable(0);
        Debug.Log("[DecreaseVariableButton] Decreased variable at index 0.");
    }
}
