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


    public void HitListener()
    {
        Debug.Log("Hit!");
    }

    public void MissListener()
    {
        Debug.Log("Miss!");
    }

    public void Init()
    {
        OnHit.AddListener(HitListener);
        OnMiss.AddListener(MissListener);
    }
}
