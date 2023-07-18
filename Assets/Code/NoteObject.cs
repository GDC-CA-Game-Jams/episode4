using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteObject : MonoBehaviour
{
    public bool canBePressed;

    public Rigidbody2D rb;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.S))
        {
            if(canBePressed)
            {
                EventManager.Instance.OnHit?.Invoke();
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Activator"))
        {
            canBePressed = true;
        }

        if(other.CompareTag("Destroyer"))
        {
            EventManager.Instance.OnMiss?.Invoke();
            Destroy(gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.CompareTag("Activator"))
        {
            canBePressed = false;
        }
    }
}
