using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioMusicStart : MonoBehaviour
{
    public AudioSource[] musicTracks;

    void Awake()
    {
        foreach(AudioSource m in musicTracks)
        {
            m.Play();
            Debug.Log("Playing " + m.name + " track!");
        }
    }
}
