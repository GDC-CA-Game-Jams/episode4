using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    AudioSource m_MyAudioSource;
    // Start is called before the first frame update
    void Start()
    {
        //Fetch the AudioSource from the GameObject
        m_MyAudioSource = GetComponent<AudioSource>();
        m_MyAudioSource.Play();
    }



    // Update is called once per frame
    void Update()
    {
        Debug.Log(m_MyAudioSource.timeSamples);
    }


    //jumps the audio back a given amount of beats
    public void jumpAudioBack(int beats)
    {
        //right now just jumps back "beats" sec in - needs fixed
        m_MyAudioSource.time = beats;
    }
}
