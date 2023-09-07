using System;
using System.Collections;
using System.Collections.Generic;
using Services;
using UnityEngine;

public class ObstacleBehaviour : MonoBehaviour
{

    [SerializeField] private Rigidbody2D obstacleArt;
    
    [SerializeField] private Rigidbody2D rbCover;

    public int startBeat;
    
    public float beatTempo;
    
    public AudioClip sfx;
    
    private Vector3 artStart;
    private Vector3 coverStart;

    private AudioController ac;
    private GameManager gm;
    
    private void Awake()
    {
        gm = ServiceLocator.Instance.Get<GameManager>();
        ac = GameObject.FindWithTag("AudioController").GetComponent<AudioController>();
        //artStart = obstacleArt.transform.localPosition;
        coverStart = rbCover.transform.localPosition;
    }
    
    private void OnEnable()
    {
        //obstacleArt.transform.localPosition = artStart;
        rbCover.transform.localPosition = coverStart;
        rbCover.velocity = new Vector2(beatTempo * -1, 0f);
        obstacleArt.velocity = new Vector2(beatTempo * -1, 0f);
    }

    private void OnDisable()
    {
        ServiceLocator.Instance.Get<EventManager>().OnMiss -= OnMiss;
        rbCover.velocity = Vector3.zero;
        obstacleArt.velocity = Vector3.zero;
    }

    public void OnMiss()
    {
        gm.beatCount = startBeat - 2;
        ac.jumpAudioToNumBeats(startBeat - 2);
        ServiceLocator.Instance.Get<EventManager>().OnClearNotes?.Invoke();
        ServiceLocator.Instance.Get<EventManager>().OnMissObstacle?.Invoke();
        gameObject.SetActive(false);
    }
}
