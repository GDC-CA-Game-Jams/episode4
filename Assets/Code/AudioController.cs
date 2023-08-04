using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AudioController : MonoBehaviour
{
    // beats per minute of the song
    [SerializeField] private float _bpm;

    // The "beat"- controls when events will be fired (ie every beat, every half beat, every quarter beat etc)
    [SerializeField] private Interval[] _intervals;

    [SerializeField] AudioSource m_MyAudioSource;

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
    public void jumpAudioBack(int beats)
    {
        //right now just jumps back "beats" sec in - needs fixed
        m_MyAudioSource.time = beats;
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
            Debug.Log("Beat");
            //instead of debug.log do an update in the screen- change text to 0,1,2
        }
    }
}