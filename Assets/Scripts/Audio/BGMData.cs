using System;
using UnityEngine;

// TODO: Add categories/tags later? -L

// NOTE (L): Why not have a base AudioData class?
// Was thinking of having these two be inherited from a common AudioData SO base class...
// ...but decided against it for simplicity's sake. Not like we have voicelines or anything fancy. 
// Was also worried about type safety shenanigans soooooo yeah. -L

[CreateAssetMenu(fileName = "BGMData", menuName = "Audio/BGMData")]
public class BGMData : ScriptableObject
{
    public string id; 
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    private void OnValidate()
    {
        if(String.IsNullOrWhiteSpace(id))
        {
            Debug.LogWarning("[BGMData] Audio ID is empty or whitespace! Assigning default ID based on filename.");
            id = name;
        }
    }
}