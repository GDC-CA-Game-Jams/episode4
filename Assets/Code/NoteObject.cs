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
    [Tooltip("specifies grading threshhold in meters from center of button to center of note")]
    public float gradingThreshhold = 10f; //grading threshhold between perfect, excellent, good, poor

    [Header("Reference Variables")]
    public Rigidbody2D rb;

    //private variables
    private bool canBeActivated; //indicates whether the note is in the "zone" of a button
    private bool falseStart = false; //used to catch if the player had the button held down before the note entered "the zone"
    private InputControls targetControls = null; //used to snatch a controls reference once the note enters the "zone" of a button
    private Collider2D buttonTrigger;

    private void Update()
    {
        if(canBeActivated) //if note is in active "zone" of a button
        {
            if (targetControls.GetPressStatus()) //if key button is pressed
            {
                if (!falseStart) //provided the button wasn't held prior to "zone" entry
                {
                    GradeArrowPop();
                    //ServiceLocator.Instance.Get<EventManager>().OnHit?.Invoke(); //might pull this if we don't add more into it
                    Destroy(gameObject);
                }
            }
            else if (falseStart) //resets falseStart to false if the player disengages the active button
            {
                falseStart = false;
            }

        }
    }

    private void GradeArrowPop()
    {
        //Pulls distance btween arrow & the button popping it
        float distance = Mathf.Abs(buttonTrigger.transform.position.x - this.transform.position.x); //TO UPDATE, currently horizontal exclusive

        float awardedPointValue = pointValue;

        //figures out what threshhold for quality the arrow pop is in
        if (distance < gradingThreshhold) //perfect
        {
            Debug.Log("Perfect!");
            ServiceLocator.Instance.Get<EventManager>().OnPerfect?.Invoke();
        }
        else if (distance < gradingThreshhold * 2) // excellent
        {
            awardedPointValue *= 0.9f;
            Debug.Log("Excellent!");
            ServiceLocator.Instance.Get<EventManager>().OnExcellent?.Invoke();
        }
        else if (distance < gradingThreshhold * 3) //good
        {
            awardedPointValue *= 0.8f;
            Debug.Log("Good!");
            ServiceLocator.Instance.Get<EventManager>().OnGood?.Invoke();
        }
        else if (distance < gradingThreshhold * 4) //fair
        {
            awardedPointValue *= 0.7f;
            Debug.Log("Fair!");
            ServiceLocator.Instance.Get<EventManager>().OnGood?.Invoke();
        }
        else
        {
            awardedPointValue *= 0.6f;
            Debug.Log("Poor!");
            ServiceLocator.Instance.Get<EventManager>().OnPoor?.Invoke();
        }
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Activator"))
        {
            targetControls = other.GetComponent<InputControls>(); //grabs a control reference once to be able to check button status
            canBeActivated = true;
            buttonTrigger = other;

            //below statement means if you were already pressing when the note entered the zone you won't auto score
            if (targetControls.GetPressStatus())
            {
                falseStart = true;
            }
        }

        if(other.CompareTag("Destroyer"))
        {
            ServiceLocator.Instance.Get<DiscoMeterService>().ChangeValue(missValue);
            ServiceLocator.Instance.Get<EventManager>().OnMiss?.Invoke(); //might pull this if we don't add things into it
            Destroy(gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.CompareTag("Activator"))
        {
            canBeActivated = false;
        }
    }
}
