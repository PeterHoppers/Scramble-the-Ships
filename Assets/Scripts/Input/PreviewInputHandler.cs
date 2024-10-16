using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem;

public class PreviewInputHandler : MonoBehaviour
{
    [SerializeField]
    private InputActionAsset _actions;

    private InputActionMap _uiActionMap;
    private InputAction _onePlayerAction;
    private InputAction _twoPlayerAction;

    private void OnEnable()
    {
        _uiActionMap = _actions.FindActionMap("ui");
        _uiActionMap.Enable();
        _onePlayerAction = _uiActionMap.FindAction("OnePlayerSelection");
        _twoPlayerAction = _uiActionMap.FindAction("TwoPlayerSelection");

        _onePlayerAction.performed += OnOnePlayerButtonPress;
        _twoPlayerAction.performed += OnTwoPlayerButtonPress;
    }

    private void OnDisable()
    {
        _onePlayerAction.performed -= OnOnePlayerButtonPress;
        _twoPlayerAction.performed -= OnTwoPlayerButtonPress;

        _uiActionMap.Disable();
    }

    private void OnOnePlayerButtonPress(InputAction.CallbackContext context)
    {
        if (GlobalGameStateManager.Instance.GlobalGameStateStatus != GlobalGameStateStatus.Preview)
        {
            return;
        }

        if (GlobalGameStateManager.Instance.CanPlay(1))
        {
            SelectedPlayerCount(1);
        }
    }

    private void OnTwoPlayerButtonPress(InputAction.CallbackContext context)
    {
        if (GlobalGameStateManager.Instance.GlobalGameStateStatus != GlobalGameStateStatus.Preview)
        {
            return;
        }

        if (GlobalGameStateManager.Instance.CanPlay(2))
        {
            SelectedPlayerCount(2);
        }
    }

    private void SelectedPlayerCount(int playerCount)
    {
        GlobalGameStateManager.Instance.SetPlayerCount(playerCount);
        GlobalGameStateManager.Instance.StartGame();
    }
}