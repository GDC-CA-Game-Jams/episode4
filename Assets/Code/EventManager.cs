using System;
using System.Collections;
using System.Collections.Generic;
using Services;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : IService
{    
    public UnityEvent OnHit = new();
    public UnityEvent OnMiss = new();

    public UnityEvent OnDeath = new();

    private float discoMeterHitIncreaseAmount = 10;
    private float discoMeterMissDecreaseAmount = -10;

    public void HitListener()
    {
        Debug.Log("Hit!");
        ServiceLocator.Instance.Get<DiscoMeterService>().ChangeValue(discoMeterHitIncreaseAmount);
    }

    public void MissListener()
    {
        Debug.Log("Miss!");
        ServiceLocator.Instance.Get<DiscoMeterService>().ChangeValue(discoMeterMissDecreaseAmount);
    }

    private void DeathListener()
    {
        Debug.Log("Player died!");
    }
    
    public void Init()
    {
        OnHit.AddListener(HitListener);
        OnMiss.AddListener(MissListener);
        OnDeath.AddListener(DeathListener);
    }
}
