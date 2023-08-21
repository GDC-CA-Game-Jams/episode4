using System;
using System.Collections;
using System.Collections.Generic;
using Services;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = System.Random;

public class PlayerBehaviour : MonoBehaviour
{
    [SerializeField] private Animator anim;

    [Tooltip("Minimum amount of time to stay in one dance before swapping")]
    [SerializeField] private int minDanceTime;

    private int beatsDanced;

    private Random rand;

    private CustomInput input;

    private void Awake()
    {
        rand = new Random();
        input = new CustomInput();
    }
    
    private void OnEnable()
    {
        EventManager em = ServiceLocator.Instance.Get<EventManager>();
        em.OnMissObstacle += OnMissObstacle;
        em.OnRewindComplete += OnRewindComplete;
        em.OnDeath += OnDeath;
        input.Enable();
        input.Player.Pause.performed += OnPressPause;
    }

    private void OnDisable()
    {
        EventManager em = ServiceLocator.Instance.Get<EventManager>();
        em.OnMissObstacle += OnMissObstacle;
        em.OnDeath += OnDeath;
        input.Player.Pause.performed -= OnPressPause;
    }

    private void OnRewindComplete()
    {
        anim.SetTrigger("Respawn");
    }
    
    private void OnMissObstacle()
    {
        anim.SetTrigger("CheckpointFailed");
    }

    private void OnDeath()
    {
        anim.SetTrigger("Death");
    }

    public void OnBeat()
    {
        if (beatsDanced < minDanceTime)
        {
            ++beatsDanced;
            return;
        }
        
        if (rand.Next(10) >= 5)
        {
            anim.SetTrigger("SwapDance");
            beatsDanced = 0;
        }
    }
    
    private void OnPressPause(InputAction.CallbackContext obj)
    {
        ServiceLocator.Instance.Get<GameManager>().TogglePause("Pause");
    }
}
