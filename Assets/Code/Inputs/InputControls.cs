using JetBrains.Annotations;
using Services;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InputControls : MonoBehaviour
{
    // --- Enum Declarations --- //
    enum Direction { Up, Down, Left, Right };
    enum PopQuality { Perfect, Excellent, Good, Poor, Miss } //to be used for FX triggering

    // --- Serialized Variable Declarations --- //
    [Header("Defining Variables")]
    [Tooltip("Sets the direction this arrow is \"Pointing\"")]
    [SerializeField] Direction arrowDirection;
    [Tooltip("specifies grading threshhold in meters from center of button to center of note")]
    [SerializeField] private float gradingThreshhold = 10f; //grading threshhold between perfect, excellent, good, poor
    [SerializeField] private Color[] hitColorCycle;
    [SerializeField] private ParticleSystem[] perfectParticles;

    // --- Private Variable Declarations --- //
    private CustomInput input = null;                               // Input reference for handling purposes
    private Queue<NoteObject> noteQueue = new Queue<NoteObject>();  // Stores notes currently "in range" of clicking
    private SpriteRenderer img;                                     
    private Animator hitAnim;

    // Play-State Variables
    private bool isInLongPress = false;         // Flags whether note track is currently in a long-press region
    private bool isFailingLongPress = false;    // Flags whether note track is currently in a long-press fail-state
    private int colorIndex;                     // Controls color cycling for hitColorCycling CHECK FOR VALIDITY
    
    private void Awake()
    {
        input = new CustomInput();
        Physics2D.callbacksOnDisable = false; // Bad place for this, but fixes a bug where popping a note double triggers dequeueing
        img = gameObject.GetComponentInChildren<SpriteRenderer>();
        hitAnim = gameObject.GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        ServiceLocator.Instance.Get<EventManager>().OnClearNotes += stateReset;
        input.Enable();
        // Switch case adds only the input handling for the appropriate direction.
        switch (arrowDirection)
        {
            case Direction.Up:
                input.Player.DiscoUp.performed += DiscoInput_performed;
                input.Player.DiscoUp.canceled += DiscoInput_canceled;
                break;
            case Direction.Down:
                input.Player.DiscoDown.performed += DiscoInput_performed;
                input.Player.DiscoDown.canceled += DiscoInput_canceled;
                break;
            case Direction.Left:
                input.Player.DiscoLeft.performed += DiscoInput_performed;
                input.Player.DiscoLeft.canceled += DiscoInput_canceled;
                break;
            case Direction.Right:
                input.Player.DiscoRight.performed += DiscoInput_performed;
                input.Player.DiscoRight.canceled += DiscoInput_canceled;
                break;
        }
    }

    private void OnDisable()
    {
        ServiceLocator.Instance.Get<EventManager>().OnClearNotes -= stateReset;
        input.Disable();
        // Switch case removes the previously subscribed input
        switch (arrowDirection)
        {
            case Direction.Up:
                input.Player.DiscoUp.performed -= DiscoInput_performed;
                input.Player.DiscoUp.canceled -= DiscoInput_canceled;
                break;
            case Direction.Down:
                input.Player.DiscoDown.performed -= DiscoInput_performed;
                input.Player.DiscoDown.canceled -= DiscoInput_canceled;
                break;
            case Direction.Left:
                input.Player.DiscoLeft.performed -= DiscoInput_performed;
                input.Player.DiscoLeft.canceled -= DiscoInput_canceled;
                break;
            case Direction.Right:
                input.Player.DiscoRight.performed -= DiscoInput_performed;
                input.Player.DiscoRight.canceled -= DiscoInput_canceled;
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("NoteObject"))
        {
            noteQueue.Enqueue(collision.GetComponent<NoteObject>());
        }
        else if (collision.CompareTag("LongBody"))
        {
            Destroy(collision.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("NoteObject"))
        {
            NoteObject objNote = noteQueue.Dequeue();
            PointScoreUpdate(objNote.getRawMissWorth());
            if (objNote.GetFlagIsLongNote())
            {
                if (!isInLongPress)
                {
                    isInLongPress = true;
                    isFailingLongPress = true;
                    Debug.Log("missed the start note");
                }
                else
                {
                    isFailingLongPress = false;
                    isInLongPress = false;
                    Debug.Log("missed the end note");
                }
            }
        }
    }

    private void PointScoreUpdate(float pointAmount)
    {
        if(pointAmount < 0)
        {
            

        }
        else
        {
            
        }
    }

    private void GradeArrowPop(NoteObject note)
    {
        //Pulls distance btween arrow & the button popping it
        float distance = Mathf.Abs(this.transform.position.x - note.transform.position.x); //TO UPDATE, currently horizontal exclusive

        float awardedPointValue = note.getRawScoreWorth();

        //figures out what threshhold for quality the arrow pop is in
        if (distance < gradingThreshhold) //perfect
        {
            ServiceLocator.Instance.Get<EventManager>().OnPerfect?.Invoke();
            perfectParticles[Random.Range(0, 3)].Play();
            hitAnim.SetTrigger("OnPerfect");
        }
        else if (distance < gradingThreshhold * 2) // excellent
        {
            awardedPointValue *= 0.9f;
            Debug.Log("Hit - Excellent!");
            ServiceLocator.Instance.Get<EventManager>().OnExcellent?.Invoke();
            hitAnim.SetTrigger("OnGood");
        }
        else if (distance < gradingThreshhold * 3) //good
        {
            awardedPointValue *= 0.8f;
            Debug.Log("Hit - Good!");
            ServiceLocator.Instance.Get<EventManager>().OnGood?.Invoke();
            hitAnim.SetTrigger("OnGood");
        }
        else if (distance < gradingThreshhold * 4) //fair
        {
            awardedPointValue *= 0.7f;
            Debug.Log("Hit - Fair!");
            ServiceLocator.Instance.Get<EventManager>().OnPoor?.Invoke();
            hitAnim.SetTrigger("OnPoor");
        }
        else
        {
            awardedPointValue *= 0.6f;
            Debug.Log("Hit - Poor!");
            ServiceLocator.Instance.Get<EventManager>().OnPoor?.Invoke();
            hitAnim.SetTrigger("OnPoor");
        }
        //ServiceLocator.Instance.Get<DiscoMeterService>().ChangeValue(awardedPointValue);
    }

    private void DiscoInput_performed(InputAction.CallbackContext obj)
    {
        if (noteQueue.Count > 0) 
        { 
            if (!isFailingLongPress)
            {
                NoteObject objNote = noteQueue.Dequeue();
                GradeArrowPop(objNote);
                if (objNote.GetFlagIsLongNote())
                {
                    if (!isInLongPress && !isFailingLongPress)
                    {
                        isInLongPress = true;
                    }
                }

                Destroy(objNote.gameObject);
            }
        }
        else
        {
            ServiceLocator.Instance.Get<EventManager>().OnMiss?.Invoke();
        }
        img.color = hitColorCycle[colorIndex];
        colorIndex = (colorIndex + 1) % hitColorCycle.Length;
    }
    private void DiscoInput_canceled(InputAction.CallbackContext obj)
    {
        if (isInLongPress)
        {
            if (noteQueue.Count > 0)
            {
                NoteObject objNote = noteQueue.Peek();
                if (objNote.GetFlagIsLongNote())
                {
                    if (!isFailingLongPress)
                    {
                        GradeArrowPop(objNote);
                        noteQueue.Dequeue();
                        isInLongPress = false;
                        Destroy(objNote.gameObject);
                        Debug.Log("STUCK THE LANDING");
                    }
                }
            }
            else
            {
                isFailingLongPress = true;
                ServiceLocator.Instance.Get<EventManager>().OnMiss?.Invoke();
            }
        }

        ResetColor();
    }

    private void stateReset()
    {
        isInLongPress = false;
        isFailingLongPress = false;
        noteQueue.Clear();
    }

    private void ResetColor()
    {
        img.color = Color.white;
    }

}
