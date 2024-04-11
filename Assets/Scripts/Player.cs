using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    ControlsManager _manager;
    PlayerInput _playerInput;
    int _playerId = 0;

    public void InitPlayer(ControlsManager manager, PlayerInput playerInput, int id)
    {
        _manager = manager;
        _playerInput = playerInput;
        _playerId = id;
    }

    public void OnPlayerMove(InputAction.CallbackContext context)
    {
        Vector2 playerMovement = context.ReadValue<Vector2>();

        _manager.TranslatePlayerInput(_playerId, playerMovement);
    }

    public void OnPlayerFire(InputAction.CallbackContext context)
    {
        var fired = context.ReadValueAsButton();
    }
}
