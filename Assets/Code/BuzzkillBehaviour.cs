using System;
using System.Collections;
using System.Collections.Generic;
using Services;
using UnityEngine;
using Random = System.Random;

public class BuzzkillBehaviour : MonoBehaviour
{

    [SerializeField] private Animator anim;
    
    private Random rand;


    private void Start()
    {
        rand = new Random();
    }
    private void OnEnable()
    {
        EventManager em = ServiceLocator.Instance.Get<EventManager>();
        em.OnMiss += OnMiss;
        em.OnMissObstacle += OnMiss;
        em.OnPerfect += OnHit;
        em.OnExcellent += OnHit;
        em.OnGood += OnHit;
        em.OnPoor += OnHit;
    }

    private void OnDisable()
    {
        EventManager em = ServiceLocator.Instance.Get<EventManager>();
        em.OnMiss -= OnMiss;
        em.OnMissObstacle -= OnMiss;
        em.OnPerfect -= OnHit;
        em.OnExcellent -= OnHit;
        em.OnGood -= OnHit;
        em.OnPoor -= OnHit;
    }

    private void OnMiss()
    {
        
    }

    private void OnHit()
    {
        if (rand.Next(0, 2) == 0)
        {
            anim.SetTrigger("React1");
        }
        else
        {
            anim.SetTrigger("React2");
        }
    }
}
