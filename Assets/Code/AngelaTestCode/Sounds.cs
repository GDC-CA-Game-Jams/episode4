using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]

// audio sources into array
public class Sounds
{
    public string name;

    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume;
    [Range(.3f, 2f)]
    public float pitch;

    public bool loop;

    [HideInInspector]
    public AudioSource source;
}
