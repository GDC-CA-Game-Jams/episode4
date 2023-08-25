using System.Collections;
using System.Collections.Generic;
using Services;
using UnityEngine;

public class HideOnGameEnd : MonoBehaviour
{
    private void OnEnable()
    {
        ServiceLocator.Instance.Get<EventManager>().OnDeath += Hide;
        ServiceLocator.Instance.Get<EventManager>().OnSongComplete += Hide;
    }
    
    private void OnDisable()
    {
        ServiceLocator.Instance.Get<EventManager>().OnDeath -= Hide;
        ServiceLocator.Instance.Get<EventManager>().OnSongComplete -= Hide;
    }
    
    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
