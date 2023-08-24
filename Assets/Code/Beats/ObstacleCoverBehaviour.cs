using System.Collections;
using System.Collections.Generic;
using Services;
using UnityEngine;

public class ObstacleCoverBehaviour : MonoBehaviour
{

    [SerializeField] private ObstacleBehaviour ob;
    private bool active;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Activator") && !active)
        {
            ServiceLocator.Instance.Get<EventManager>().OnMiss += ob.OnMiss;
            active = true;
        }
    }
    
    private void OnDisable()
    {
        ServiceLocator.Instance.Get<EventManager>().OnMiss -= ob.OnMiss;
        active = false;
    }
}
