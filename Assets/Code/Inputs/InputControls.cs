using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputControls : MonoBehaviour
{
    private CustomInput input = null;
    private Vector2 discoMotion = Vector2.zero;
    private Vector2 playerMotion = Vector2.zero;
    private bool isJumpInputting = false;

    private void Awake()
    {
        input = new CustomInput();
    }

    private void OnEnable()
    {
        input.Enable();
        input.Player.DiscoMotion.performed += DiscoMotion_performed;
        input.Player.DiscoMotion.canceled += DiscoMotion_canceled;
        input.Player.PlayerMotion.performed += PlayerMotion_performed;
        input.Player.PlayerMotion.canceled += PlayerMotion_canceled;
        input.Player.Jump.performed += Jump_performed;
        input.Player.Jump.canceled += Jump_cancelled;
    }

    private void OnDisable()
    {
        input.Disable();
        input.Player.DiscoMotion.performed -= DiscoMotion_performed;
        input.Player.DiscoMotion.canceled -= DiscoMotion_canceled;
        input.Player.PlayerMotion.performed -= PlayerMotion_performed;
        input.Player.PlayerMotion.canceled -= PlayerMotion_performed;
        input.Player.Jump.performed -= Jump_performed;
        input.Player.Jump.canceled -= Jump_performed;
    }

    private void PlayerMotion_performed(InputAction.CallbackContext obj) { playerMotion = obj.ReadValue<Vector2>(); }
    private void PlayerMotion_canceled(InputAction.CallbackContext obj) { playerMotion = Vector2.zero; }
    private void DiscoMotion_canceled(InputAction.CallbackContext obj) { discoMotion = Vector2.zero; }
    private void DiscoMotion_performed(InputAction.CallbackContext obj) { discoMotion = obj.ReadValue<Vector2>(); }
    private void Jump_performed(InputAction.CallbackContext obj) { isJumpInputting = true; }
    private void Jump_cancelled(InputAction.CallbackContext obj) { isJumpInputting = false; }
    public Vector2 GetDiscoMotion() { return discoMotion; }
    public Vector2 GetPlayerMotion() { return playerMotion; }
    public bool GetInputJump() { return isJumpInputting; }
}
