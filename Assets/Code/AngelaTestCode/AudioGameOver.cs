using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioGameOver : MonoBehaviour
{

    [SerializeField] AudioMixerGroup audioMixerSFX;
    [SerializeField] AudioMixerGroup audioMixerMUS;
    public AudioMixer masterMixer;
    public AudioMixerSnapshot audioResetSnapshot;
    //private AudioManager audioManager;

    void Start()
    {
        //audioManager = FindObjectOfType<AudioManager>();
        //audioManager.StopGuitarFadeOut();
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
        masterMixer.SetFloat("sfxLowPass", 22000f);
        masterMixer.SetFloat("musicLowPass", 22000f);
    }



    public void SetSFXLevel(float sfxLvl)
    {
        masterMixer.SetFloat("sfxVol", sfxLvl);
        Debug.Log("resetting audio mixer levels on game over");
    }

    public void SetMUSLevel(float musLvl)
    {
        masterMixer.SetFloat("musicVol", musLvl);
    }
}
