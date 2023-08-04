using System;
using System.Collections;
using System.Collections.Generic;

using Services;

using UnityEngine;

public class NoteObject : MonoBehaviour
{
    [Header("Difficulty Variables")]
    [Tooltip("Point worth of note on a hit (modified by grade on hit)")]
    public float pointValue = 10f;
    [Tooltip("specifies how much a miss costs the player in terms of glamour")]
    public float missValue = -10f;

    [Header("Reference Variables")]
    public Rigidbody2D rb;

    //private variables
    private bool canBeActivated; //indicates whether the note is in the "zone" of a button

    private void OnTriggerEnter2D(Collider2D other)
    {

        if(other.CompareTag("Destroyer"))
        {
            ServiceLocator.Instance.Get<DiscoMeterService>().ChangeValue(missValue);
            ServiceLocator.Instance.Get<EventManager>().OnMiss?.Invoke(); //might pull this if we don't add things into it
            Destroy(gameObject);
        }
    }

    public float getRawScoreWorth()
    {
        return pointValue;
    }

    public float getRawMissWorth()
    {
        return missValue;
    }
}
