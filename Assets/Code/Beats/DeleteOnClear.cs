using System;
using System.Collections;
using System.Collections.Generic;
using Services;
using UnityEngine;

public class DeleteOnClear : MonoBehaviour
{
    private void OnEnable()
    {
        ServiceLocator.Instance.Get<EventManager>().OnClearNotes += Clear;
    }
    
    private void OnDisable()
    {
        ServiceLocator.Instance.Get<EventManager>().OnClearNotes -= Clear;
    }

    private void Clear()
    {
        Destroy(gameObject);
    }
}
