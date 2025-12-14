using UnityEngine;

[System.Serializable]
public class UtilityCurve
{
    public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);

    /// <summary>
    /// Input should be normalized 0–1 before being passed in.
    /// Output is clamped 0–1.
    /// </summary>
    public float Evaluate01(float normalizedInput)
    {
        return Mathf.Clamp01(curve.Evaluate(Mathf.Clamp01(normalizedInput)));
    }
}
