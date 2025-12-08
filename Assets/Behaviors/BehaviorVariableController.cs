using UnityEngine;
using Unity.Behavior;

public class BehaviorGraphVariableController : MonoBehaviour
{
    [System.Serializable]
    public class GraphVariableControl
    {
        [Tooltip("The behavior graph asset (not the agent)")]
        public BehaviorGraph behaviorGraph;
        
        [Tooltip("Exact name of the variable in the graph")]
        public string variableName;
        
        [Tooltip("Amount to increment/decrement")]
        public float changeAmount = 1f;
        
        [Tooltip("Minimum value (optional)")]
        public float minValue = 0f;
        
        [Tooltip("Maximum value (optional)")]
        public float maxValue = 0f;
        
        [Tooltip("Use clamping for min/max values")]
        public bool useClamp = false;
    }
    
    [SerializeField] private GraphVariableControl[] graphVariables;
    
    /// <summary>
    /// Increase a variable by index
    /// </summary>
    public void IncreaseVariable(int index)
    {
        if (!IsValidIndex(index)) return;
        
        GraphVariableControl control = graphVariables[index];
        ModifyGraphVariable(control, control.changeAmount);
    }
    
    /// <summary>
    /// Decrease a variable by index
    /// </summary>
    public void DecreaseVariable(int index)
    {
        if (!IsValidIndex(index)) return;
        
        GraphVariableControl control = graphVariables[index];
        ModifyGraphVariable(control, -control.changeAmount);
    }
    
    /// <summary>
    /// Set a variable to a specific value by index
    /// </summary>
    public void SetVariable(int index, float value)
    {
        if (!IsValidIndex(index)) return;
        
        GraphVariableControl control = graphVariables[index];
        SetGraphVariableValue(control, value);
    }
    
    /// <summary>
    /// Get current value of a variable by index
    /// </summary>
    public float GetVariable(int index)
    {
        if (!IsValidIndex(index)) return 0f;
        
        GraphVariableControl control = graphVariables[index];
        if (control.behaviorGraph == null) return 0f;
        
        BlackboardReference blackboard = control.behaviorGraph.BlackboardReference;
        if (blackboard == null) return 0f;
        
        if (blackboard.GetVariableValue(control.variableName, out float value))
        {
            return value;
        }
        
        return 0f;
    }
    
    private void ModifyGraphVariable(GraphVariableControl control, float delta)
    {
        if (control.behaviorGraph == null)
        {
            Debug.LogWarning($"Behavior Graph not assigned for variable: {control.variableName}");
            return;
        }
        
        BlackboardReference blackboard = control.behaviorGraph.BlackboardReference;
        
        if (blackboard == null)
        {
            Debug.LogWarning($"Blackboard Reference is null for graph. Make sure the graph has a blackboard.");
            return;
        }
        
        // Try float first
        if (blackboard.GetVariableValue(control.variableName, out float floatValue))
        {
            float newValue = floatValue + delta;
            
            if (control.useClamp)
            {
                newValue = Mathf.Clamp(newValue, control.minValue, control.maxValue);
            }
            else if (control.minValue != 0 || control.maxValue != 0)
            {
                if (control.minValue != 0) newValue = Mathf.Max(control.minValue, newValue);
                if (control.maxValue != 0) newValue = Mathf.Min(control.maxValue, newValue);
            }
            
            blackboard.SetVariableValue(control.variableName, newValue);
            
            // Mark the graph as dirty so Unity saves the changes
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(control.behaviorGraph);
            #endif
            
            Debug.Log($"Modified {control.variableName} in graph: {floatValue} -> {newValue}");
            return;
        }
        
        // Try int
        if (blackboard.GetVariableValue(control.variableName, out int intValue))
        {
            int newValue = intValue + Mathf.RoundToInt(delta);
            
            if (control.useClamp)
            {
                newValue = Mathf.Clamp(newValue, Mathf.RoundToInt(control.minValue), Mathf.RoundToInt(control.maxValue));
            }
            else if (control.minValue != 0 || control.maxValue != 0)
            {
                if (control.minValue != 0) newValue = Mathf.Max(Mathf.RoundToInt(control.minValue), newValue);
                if (control.maxValue != 0) newValue = Mathf.Min(Mathf.RoundToInt(control.maxValue), newValue);
            }
            
            blackboard.SetVariableValue(control.variableName, newValue);
            
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(control.behaviorGraph);
            #endif
            
            Debug.Log($"Modified {control.variableName} in graph: {intValue} -> {newValue}");
            return;
        }
        
        // Try bool (toggle)
        if (blackboard.GetVariableValue(control.variableName, out bool boolValue))
        {
            bool newValue = !boolValue;
            blackboard.SetVariableValue(control.variableName, newValue);
            
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(control.behaviorGraph);
            #endif
            
            Debug.Log($"Toggled {control.variableName} in graph: {boolValue} -> {newValue}");
            return;
        }
        
        Debug.LogWarning($"Variable '{control.variableName}' not found or unsupported type in graph");
    }
    
    private void SetGraphVariableValue(GraphVariableControl control, float value)
    {
        if (control.behaviorGraph == null)
        {
            Debug.LogWarning($"Behavior Graph not assigned for variable: {control.variableName}");
            return;
        }
        
        BlackboardReference blackboard = control.behaviorGraph.BlackboardReference;
        
        if (blackboard == null)
        {
            Debug.LogWarning($"Blackboard Reference is null for graph.");
            return;
        }
        
        if (control.useClamp)
        {
            value = Mathf.Clamp(value, control.minValue, control.maxValue);
        }
        
        // Try to determine type and set appropriately
        if (blackboard.GetVariableValue(control.variableName, out float _))
        {
            blackboard.SetVariableValue(control.variableName, value);
        }
        else if (blackboard.GetVariableValue(control.variableName, out int _))
        {
            blackboard.SetVariableValue(control.variableName, Mathf.RoundToInt(value));
        }
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(control.behaviorGraph);
        #endif
        
        Debug.Log($"Set {control.variableName} in graph to: {value}");
    }
    
    private bool IsValidIndex(int index)
    {
        if (graphVariables == null || index < 0 || index >= graphVariables.Length)
        {
            Debug.LogWarning($"Invalid variable index: {index}");
            return false;
        }
        return true;
    }
}