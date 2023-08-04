using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopingScroll : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 1;

    [SerializeField] private float loopPoint;

    [SerializeField] private float respawnPoint;
    
    private Camera cam;

    private Vector3 moveVec;
    
    // Start is called before the first frame update
    void Start()
    {
        loopPoint *= -1;
        scrollSpeed *= -1;
        respawnPoint *= -1;
        cam = Camera.main;
        moveVec = new Vector2(scrollSpeed, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.x <= loopPoint)
        {
            Debug.Log("Looping!");
            transform.position = new Vector2(-loopPoint, transform.position.y);
        }

        transform.position += moveVec * Time.deltaTime;
    }
}
