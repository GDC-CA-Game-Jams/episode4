using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //private Vector2 discoMotion, playerMotion, playerJump;

    //[SerializeField] InputActionReference controlRefDiscoMove, controlRefPlayerMove, controlRefPlayerJump;

    private InputControls inputControl;

    private void Awake()
    {
        inputControl = GetComponent<InputControls>();
    }

    private void Update()
    {
        Debug.Log("Disco Motion - " + inputControl.GetDiscoMotion());
        Debug.Log("Player Motion - " + inputControl.GetPlayerMotion());
        Debug.Log("Jumping? - " + inputControl.GetInputJump());
    }

}


