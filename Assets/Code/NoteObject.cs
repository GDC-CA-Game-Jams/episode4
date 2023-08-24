using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using Services;

using UnityEngine;

public class NoteObject : MonoBehaviour
{
    //public (serialized) variables
    [Header("Difficulty Variables")]
    [Tooltip("Point worth of note on a hit (modified by grade on hit)")]
    public float pointValue = 10f;
    [Tooltip("specifies how much a miss costs the player in terms of glamour")]
    public float missValue = -10f;

    //public (non-serialized) variables
    [NonSerialized] public Rigidbody2D rb;

    //private variables
    private bool bLongNote = false;
    private bool bObstacleNote = false;

    private void OnEnable()
    {
        ServiceLocator.Instance.Get<EventManager>().OnClearNotes += Remove;
    }

    private void OnDisable()
    {
        ServiceLocator.Instance.Get<EventManager>().OnClearNotes -= Remove;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {

        if (other.CompareTag("Destroyer"))
        {
            ServiceLocator.Instance.Get<EventManager>().OnMiss?.Invoke(); //might pull this if we don't add things into it
            Destroy(gameObject);
        }
    }

    //getters & setters
    public float getRawScoreWorth()
    {
        return pointValue;
    }

    public float getRawMissWorth()
    {
        return missValue;
    }

    public bool GetFlagIsLongNote()
    {
        return bLongNote;
    }

    public void SetFlagIsLongNote(bool flag)
    {
        bLongNote = flag;
    }

    public bool GetFlagIsObstacleNote()
    {
        return bObstacleNote;
    }

    public void SetFlagIsObstacleNote(bool flag)
    {
        bObstacleNote = flag;
    }

    private void Remove()
    {
        Destroy(gameObject);
    }
}
