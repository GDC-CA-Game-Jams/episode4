using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }
    
    public UnityEvent OnHit;

    public UnityEvent OnMiss;


    public void HitListener()
    {
        Debug.Log("Hit!");
    }

    public void MissListener()
    {
        Debug.Log("Miss!");
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }

    private void Start()
    {
        OnHit.AddListener(HitListener);
        OnMiss.AddListener(MissListener);
    }
}
