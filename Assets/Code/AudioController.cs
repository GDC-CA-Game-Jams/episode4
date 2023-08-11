using System.Collections;
using System.Collections.Generic;
using Services;
using UnityEngine;
using UnityEngine.Events;

public class AudioController : MonoBehaviour
{
    // beats per minute of the song
    [SerializeField] private float _bpm;

    // The "beat"- controls when events will be fired (ie every beat, every half beat, every quarter beat etc)
    [SerializeField] private Interval[] _intervals;

    [SerializeField] AudioSource m_MyAudioSource;

    [SerializeField] AudioManager m_MyAudioManager;

    private IEnumerator coroutine;

    // Update is called once per frame
    void Update()
    {
        //timeElapsedinIntervals is essentially the time elapsed in beats
        //you can change the interval, so it could be time elapsed in beats, half beats, quarter beats etc.
        //for example, assuming 1 interval = 1 beat, if timeElapsedinIntervals = 5, then we are 5 beats into the song
        //if if timeElapsedinIntervals = 5.0006, then we are 5 beats into the song (it rounds down)
        foreach (Interval interval in _intervals)
        {
            float timeElapsedinIntervals = ((m_MyAudioSource.timeSamples) / (m_MyAudioSource.clip.frequency * interval.GetIntervalLength(_bpm)));
            interval.CheckForNewInterval(timeElapsedinIntervals);
        }
    }


    //jumps the audio back a given amount of beats
    public void jumpAudioBackInBeats(int beats)
    {
        coroutine = playRewindStop(1.0f);

        // play RewindStart
        m_MyAudioManager.PlaySFX("RewindStart");
        StartCoroutine(coroutine);
        //calculate secondsToRewind based on beats and bpm
        float secondsToRewind = beats*(60/_bpm);

        float currentTime = m_MyAudioSource.time;
        float rewindTime = currentTime - secondsToRewind;
        m_MyAudioSource.time = rewindTime;
    }

    //jumps to X beats, regardless of where in the audio file the song is when this function is called
    //ie if beats is 4, it will jump to 4 beats after the song starts
    public void jumpAudioToNumBeats(int beats)
    {
        coroutine = playRewindStop(1.0f);

        // play RewindStart
        m_MyAudioManager.PlaySFX("RewindStart");
        StartCoroutine(coroutine);
        m_MyAudioSource.time = beats * (60 / _bpm);
    }

    //Probably will not be used
    //jumps the audio back a given amount of seconds
    public void jumpAudioBackInSeconds(float seconds)
    {
        float currentTime = m_MyAudioSource.time;
        float rewindTime = currentTime - seconds;
        m_MyAudioSource.time = rewindTime;
    }
    public float GetBPM()
    {
        return _bpm;
    }

    public void jumpAudioBackFourMeasures()
    {
        coroutine = playRewindStop(1.0f);

        // go back 16 beats in music

        // float secondsToRewind = 16*(60/_bpm);
        // float currentTime = m_MyAudioSource.time;
        // float rewindTime = currentTime - secondsToRewind;

        // pause time for 2 measures (8 beats)

        // play RewindStart
        m_MyAudioManager.PlaySFX("RewindStart");
        // m_MyAudioSource.Stop();
        // stop m_MyAudioSource
        StartCoroutine(coroutine);
        // play RewindStop
        // play m_MyAudioSource
        // m_MyAudioSource.time = rewindTime;
        Debug.Log("done");
    }

    private IEnumerator playRewindStop(float waitTime)
    {
        Debug.Log("pausing time");
        
        float secondsToRewind = 16*(60/_bpm);
        float currentTime = m_MyAudioSource.time;
        float rewindTime = currentTime - secondsToRewind;
        
        m_MyAudioSource.Stop();

        yield return new WaitForSeconds(waitTime);
        yield return new WaitForSeconds(waitTime);
        m_MyAudioManager.PlaySFX("RewindStop");
        yield return new WaitForSeconds(waitTime);
        m_MyAudioManager.StopPlaying("RewindStart");
        Debug.Log("Stop rewind begin sfx");
        yield return new WaitForSeconds(waitTime);
        m_MyAudioSource.Play();
        m_MyAudioSource.time = rewindTime;
        Debug.Log("done rewinding");
    }

}

//Intervals are like beats, but more complex
//you can have an interval every beat, but you could also have one every 2 beats, or every half beat, every quarter beat etc.
[System.Serializable] //this class is visible in the inspector
public class Interval
{
    // the _steps variable controls when events will be fired(ie every beat, every half beat, every quarter beat etc)
    // 1 is every beat, 0.5 is every 2 beats, 2 is every half beat, 4 is every quarter beat 
    [SerializeField] private float _steps;

    [SerializeField] private UnityEvent _trigger;

    private int _lastInterval;

    public float GetIntervalLength(float bpm)
    {
        return 60f / (bpm * _steps);
            //60f / bpm gives us the number of beats per second
            //_steps controls if the interval is every beat, every 2 beats, every half beat etc.
    }

    public void CheckForNewInterval(float interval)
    {
        //a new interval ocurrs every NEW whole number
        //We round down to the nearest whole number
        //Might not hit exact whole numbers- ie 5.0004 could count as a "whole number"
        if(Mathf.FloorToInt(interval) != _lastInterval)
        {
            _lastInterval = Mathf.FloorToInt(interval);
            _trigger.Invoke(); //Fire the event
            //Debug.Log("Beat");
            //instead of debug.log do an update in the screen- change text to 0,1,2
        }
    }
}