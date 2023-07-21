using System;
using System.Collections;
using System.Collections.Generic;

using Services;

using UnityEngine;

public class NoteObject : MonoBehaviour
{
    private bool canBeActivated; //indicates whether the note is in the "zone" of a button
    private bool falseStart = false; //used to catch if the player had the button held down before the note entered "the zone"
    private InputControls targetControls = null; //used to snatch a controls reference once the note enters the "zone" of a button

    public Rigidbody2D rb; //afaik currently unused, not sure what it will be used for -BMH
    /// </summary>

    private void Update()
    {
        if(canBeActivated) //if note is in active "zone" of a button
        {
            if (targetControls.GetPressStatus()) //if key button is pressed
            {
                if (!falseStart) //provided the button wasn't held prior to "zone" entry
                {
                    ServiceLocator.Instance.Get<EventManager>().OnHit?.Invoke();
                    Destroy(gameObject);
                }
            }
            else if (falseStart) //resets falseStart to false if the player disengages the active button
            {
                falseStart = false;
            }

        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Activator"))
        {
            targetControls = other.GetComponent<InputControls>(); //grabs a control reference once to be able to check button status
            canBeActivated = true;

            //below statement means if you were already pressing when the note entered the zone you won't auto score
            if (targetControls.GetPressStatus())
            {
                falseStart = true;
            }
        }

        if(other.CompareTag("Destroyer"))
        {
            ServiceLocator.Instance.Get<EventManager>().OnMiss?.Invoke();
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
