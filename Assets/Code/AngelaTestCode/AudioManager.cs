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
    public AudioMixer masterMixer;

    [SerializeField] AudioFilterControl audioFilterController;

    public AudioMixerSnapshot defaultSnapshot;

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

    public void SetSFXLevel(float sfxLvl)
    {
        masterMixer.SetFloat("sfxVol", sfxLvl);
    }

    public void SetMUSLevel(float musLvl)
    {
        masterMixer.SetFloat("musicVol", musLvl);
    }

    public void MuteAllOnPlayerDeath()
    {
        float muteLevel = -80f;
        SetMUSLevel(muteLevel);
        SetSFXLevel(muteLevel);
    }

}
