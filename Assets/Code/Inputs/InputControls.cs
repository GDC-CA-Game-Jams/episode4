using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputControls : MonoBehaviour
{
    //enum used to hold cardinal direction of inputButton this is attached to.
    enum Direction { Up, Down, Left, Right };
    [SerializeField] Direction arrowDirection;

    private CustomInput input = null;
    //isPressed stores whether the input button is currently held down or not in a publicly grabbable way
    private bool isPressed = false;

    private void Awake()
    {
        input = new CustomInput();
    }

    private void OnEnable()
    {
        input.Enable();
        //switch case adds only the input handling for the appropriate direction.
        switch (arrowDirection)
        {
            case Direction.Up:
                input.Player.DiscoUp.performed += DiscoInput_performed;
                input.Player.DiscoUp.canceled += DiscoInput_canceled;
                break;
            case Direction.Down:
                input.Player.DiscoDown.performed += DiscoInput_performed;
                input.Player.DiscoDown.canceled += DiscoInput_canceled;
                break;
            case Direction.Left:
                input.Player.DiscoLeft.performed += DiscoInput_performed;
                input.Player.DiscoLeft.canceled += DiscoInput_canceled;
                break;
            case Direction.Right:
                input.Player.DiscoRight.performed += DiscoInput_performed;
                input.Player.DiscoRight.canceled += DiscoInput_canceled;
                break;
        }
    }

    private void OnDisable()
    {
        input.Disable();
        switch (arrowDirection)
        {
            case Direction.Up:
                input.Player.DiscoUp.performed -= DiscoInput_performed;
                input.Player.DiscoUp.canceled -= DiscoInput_canceled;
                break;
            case Direction.Down:
                input.Player.DiscoDown.performed -= DiscoInput_performed;
                input.Player.DiscoDown.canceled -= DiscoInput_canceled;
                break;
            case Direction.Left:
                input.Player.DiscoLeft.performed -= DiscoInput_performed;
                input.Player.DiscoLeft.canceled -= DiscoInput_canceled;
                break;
            case Direction.Right:
                input.Player.DiscoRight.performed -= DiscoInput_performed;
                input.Player.DiscoRight.canceled -= DiscoInput_canceled;
                break;
        }
    }

    private void DiscoInput_performed(InputAction.CallbackContext obj)
    {
        isPressed = true;
    }
    private void DiscoInput_canceled(InputAction.CallbackContext obj)
    {
        isPressed = false;
    }

    /// <summary>
    /// Returns a boolean indicating whether the input of a button is currently being actuated
    /// </summary>
    /// <returns></returns>
    public bool GetPressStatus()
    {
        return isPressed;
    }
}
