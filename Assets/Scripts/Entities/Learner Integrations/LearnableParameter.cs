using UnityEngine;

/// <summary>
/// Wraps a single learnable parameter with bounds, activation state, and metadata.
/// Used by Hill Climber to mutate FSM behavior without knowing semantic meaning.
/// </summary>
[System.Serializable]
public struct LearnableParameter
{
    [SerializeField] private float value;
    [SerializeField] private float min;
    [SerializeField] private float max;
    [SerializeField] private bool isActive;
    [SerializeField, TextArea] private string description;

    public float Value => value;
    public float Min => min;
    public float Max => max;
    public bool IsActive => isActive;
    public string Description => description;

    public LearnableParameter(float value, float min, float max, bool isActive = true, string description = "")
    {
        this.value = Mathf.Clamp(value, min, max);
        this.min = min;
        this.max = max;
        this.isActive = isActive;
        this.description = description;
    }

    /// <summary>
    /// Set a new value, clamped to bounds.
    /// </summary>
    public void SetValue(float newValue)
    {
        value = Mathf.Clamp(newValue, min, max);
    }

    /// <summary>
    /// Enable or disable this parameter (for capability unlocks).
    /// </summary>
    public void SetActive(bool active)
    {
        isActive = active;
    }

    /// <summary>
    /// Reset to default value (useful when capability unlocks).
    /// </summary>
    public void ResetToDefault(float defaultValue)
    {
        value = Mathf.Clamp(defaultValue, min, max);
    }
}
