using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public Sounds[] SFX;
    public Sounds[] Music;

    [SerializeField] AudioSource m_GuitarMusicLayer;

    public static AudioManager instance;
    [SerializeField] AudioMixerGroup audioMixerSFX;
    [SerializeField] AudioMixerGroup audioMixerMUS;

    [SerializeField] AudioFilterControl audioFilterController;

    void Awake()
    {
        // only room in this town for one AudioManager
        if (instance == null)
        {
            instance = this;
        }
        else 
        {
            Destroy(gameObject);
            return;
        }

        foreach (Sounds s in SFX)
        {
            // add gameobject on AudioManager for each sound source, inherit values

            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.outputAudioMixerGroup = audioMixerSFX;
        }

        // foreach (Sounds m in Music)
        // {
        //     //Debug.Log("playing audio clip " + m.name);
        //     m.source = gameObject.AddComponent<AudioSource>();
        //     m.source.clip = m.clip;

        //     m.source.volume = m.volume;
        //     m.source.pitch = m.pitch;
        //     m.source.loop = m.loop;
        //     m.source.outputAudioMixerGroup = audioMixerMUS;
        // }

        // PlayMUS("MusicBaseLayer");
        // PlayMUS("MusicPianoLayer");
        // PlayMUS("MusicSynthLayer");
        //PlayMUS("MusicGuitarLayer");
    }

    public void PlaySFX(string name)
    {
        // search through Array of SFX, play the one that matches name
        Sounds s = Array.Find(SFX, sound => sound.name == name);
        if(s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found");
            return;
        }
        s.source.Play();
    }

    // public void PlayMUS(string name)
    // {
    //     // search through Array of Music, play the one that matches name
    //     Sounds m = Array.Find(Music, sound => sound.name == name);
    //     if(m == null)
    //     {
    //         Debug.LogWarning("Sound: " + name + " not found");
    //         return;
    //     }
    //     m.source.Play();
    //}

    public void StopPlaying(string name)
    {
        Sounds s = Array.Find(SFX, sound => sound.name == name);
        if(s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found");
            return;
        }

        s.source.Stop();      
    }

    public void MusicGuitarFadeIn()
    {
        //Debug.Log("fading in guitar layer");
        StartCoroutine(FadeAudioSource.StartFade(m_GuitarMusicLayer, 3f, .75f));
    }

    public void MusicGuitarFadeOut()
    {
        //Debug.Log("fading out guitar layer");
        StartCoroutine(FadeAudioSource.StartFade(m_GuitarMusicLayer, 1f, 0f));
    }

}
