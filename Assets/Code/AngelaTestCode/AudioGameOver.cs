using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioGameOver : MonoBehaviour
{

    [SerializeField] AudioMixerGroup audioMixerSFX;
    [SerializeField] AudioMixerGroup audioMixerMUS;
    public AudioMixer masterMixer;
    public AudioMixerSnapshot audioResetSnapshot;

    void Awake()
    {
        ResetAudioMixerLevels();
        ResetAudioFilterLevels();
    }

    public void ResetAudioMixerLevels()
    {
        SetMUSLevel(-10f);
        SetSFXLevel(-7f);
    }

    public void ResetAudioFilterLevels()
    {
        // audioResetSnapshot.TransitionTo(.1f);
        masterMixer.SetFloat("sfxLowPass", 22000f);
        masterMixer.SetFloat("musicLowPass", 22000f);
    }

    public void SetSFXLevel(float sfxLvl)
    {
        masterMixer.SetFloat("sfxVol", sfxLvl);
    }

    public void SetMUSLevel(float musLvl)
    {
        masterMixer.SetFloat("musicVol", musLvl);
    }
}
