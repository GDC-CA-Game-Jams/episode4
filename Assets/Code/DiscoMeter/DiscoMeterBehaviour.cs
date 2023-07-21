using System;
using System.Collections;
using System.Collections.Generic;
using Services;
using UnityEngine;
using UnityEngine.UI;

public class DiscoMeterBehaviour : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Slider deathSlider;
    
    [SerializeField] private float initialValue;
    [SerializeField] private float maxValue;
    [SerializeField] private float killValue;
    
    // Start is called before the first frame update
    void Start()
    {
        // Init the disco meter service. Setting as a service so other scripts can access more easily
        ServiceLocator.Instance.Register(new DiscoMeterService());
        ServiceLocator.Instance.Get<DiscoMeterService>().Init(this, slider, deathSlider, initialValue, maxValue, killValue);
    }

    // Update is called once per frame
    private void OnDestroy()
    {
        // When the disco meter object itself gets destroyed, unregister the service as it no longer needs to exist
        ServiceLocator.Instance.Unregister<DiscoMeterService>();
    }
}
