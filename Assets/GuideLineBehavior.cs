using System;
using Services;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuideLineBehavior : MonoBehaviour
{
    private void OnEnable()
    {
        ServiceLocator.Instance.Get<EventManager>().OnClearNotes += OnClearNotes;
    }

    private void OnDisable()
    {
        ServiceLocator.Instance.Get<EventManager>().OnClearNotes -= OnClearNotes;
    }

    void Start()
    {
        transform.SetAsFirstSibling();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {

        if (other.CompareTag("Destroyer"))
        {
            Destroy(gameObject);
        }
    }

    private void OnClearNotes()
    {
        Destroy(gameObject);
    }
}
