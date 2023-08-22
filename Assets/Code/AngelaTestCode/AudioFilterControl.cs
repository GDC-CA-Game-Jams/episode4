using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioFilterControl : MonoBehaviour
{
    public float maxThreshold = 50;
    public AudioMixer filterMixer;
    public AudioMixerSnapshot[] filterSnapshots;
    public float[] weights;
    public bool atMaxHealth = false;
    [SerializeField] AudioManager audioManager;

    public void BlendSnapshots(float playerHealth)
    {
        weights[0] = playerHealth;
        weights[1] = maxThreshold - playerHealth;
        // TransitionToSnapshots(AudioMixerSnapshot[] snapshots, float[] weights, float timeToReach)
        // -- (set of snapshots to be mixed, mix weights for snapshots specified, time after which mix should be reached from any current state)
        filterMixer.TransitionToSnapshots(filterSnapshots, weights, .1f);
    }

    public void PlayerMaxHealth(bool maxHealth)
    {
        // if true, turn up guitar track
        // if false, do not play
        atMaxHealth = maxHealth;

        if (atMaxHealth)
        {
            audioManager.MusicGuitarFadeIn();
        }
        else
        {
            audioManager.MusicGuitarFadeOut();
        }
    }
}
