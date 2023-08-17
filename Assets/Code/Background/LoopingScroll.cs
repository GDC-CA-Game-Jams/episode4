using System;
using System.Collections;
using System.Collections.Generic;
using Services;
using UnityEngine;

public class LoopingScroll : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 1;

    [SerializeField] private float loopPoint;

    [SerializeField] private float respawnPoint;

    private float startScrollSpeed;

    private bool isRewinding;
    
    private Camera cam;

    private Vector3 moveVec;

    private void OnEnable()
    {
        EventManager em = ServiceLocator.Instance.Get<EventManager>();
        em.OnMissObstacle += OnMissObstacle;
        em.OnRewindComplete += OnRewindComplete;
    }

    private void OnDisable()
    {
        EventManager em = ServiceLocator.Instance.Get<EventManager>();
        em.OnMissObstacle -= OnMissObstacle;
        em.OnRewindComplete -= OnRewindComplete;
    }

    // Start is called before the first frame update
    void Start()
    {
        loopPoint *= -1;
        scrollSpeed *= -1;
        startScrollSpeed = scrollSpeed;
        respawnPoint *= -1;
        cam = Camera.main;
        moveVec = new Vector2(scrollSpeed, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isRewinding)
        {
            if (transform.position.x <= loopPoint)
            {
                transform.position = new Vector2(-loopPoint, transform.position.y);
            }
        }
        else
        {
            if (transform.position.x >= loopPoint)
            {
                transform.position = new Vector2(-loopPoint, transform.position.y);
            }
        }

        transform.position += moveVec * Time.deltaTime;
    }

    private void OnMissObstacle()
    {
        loopPoint *= -1;
        scrollSpeed *= -4;
        respawnPoint *= -1;
        moveVec *= -4;
        isRewinding = true;
    }

    private void OnRewindComplete()
    {
        loopPoint *= -1;
        scrollSpeed *= -0.25f;
        respawnPoint *= -1;
        moveVec *= -0.25f;
        isRewinding = false;
    }
}
