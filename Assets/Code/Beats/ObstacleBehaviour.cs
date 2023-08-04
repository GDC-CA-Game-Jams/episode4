using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleBehaviour : MonoBehaviour
{

    [SerializeField] private Rigidbody2D obstacleArt;
    
    [SerializeField] private Rigidbody2D rbCover;

    public float beatTempo;

    private Vector3 artStart;

    private void Awake()
    {
        artStart = obstacleArt.position;
    }
    
    private void OnEnable()
    {
        obstacleArt.position = artStart;
        rbCover.velocity = new Vector2(beatTempo * -1, 0f);
        obstacleArt.velocity = new Vector2(beatTempo * -1, 0f);
    }

    private void OnDisable()
    {
        rbCover.velocity = Vector3.zero;
        obstacleArt.velocity = Vector3.zero;
    }
}
