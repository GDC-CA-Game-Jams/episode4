using System.Collections;
using System.Collections.Generic;
using Services;
using UnityEngine;

public class ObstacleCoverBehaviour : MonoBehaviour
{

    [SerializeField] private ObstacleBehaviour ob;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Activator"))
        {
            Debug.Log("Entering activator!");
            ServiceLocator.Instance.Get<EventManager>().OnMiss += ob.OnMiss;
        }
    }
    
    private void OnDisable()
    {
        ServiceLocator.Instance.Get<EventManager>().OnMiss -= ob.OnMiss;
    }
}
