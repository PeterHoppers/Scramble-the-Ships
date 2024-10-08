using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem;

public class PreviewInputHandler : MonoBehaviour
{
    InputSystemUIInputModule _inputSystem;
    private void OnEnable()
    {
        _inputSystem = EventSystem.current.gameObject.GetComponent<InputSystemUIInputModule>();
        _inputSystem.middleClick.action.performed += (InputAction.CallbackContext context) =>
        {
            print(context);
        };
        _inputSystem.leftClick.action.performed += OnOnePlayerButtonPress;
        _inputSystem.rightClick.action.performed += OnTwoPlayerButtonPress;
        _inputSystem.submit.action.performed += (InputAction.CallbackContext context) =>
        {
            print(context);
        };        
    }

    private void OnDisable()
    {
        _inputSystem.leftClick.action.performed -= OnOnePlayerButtonPress;
        _inputSystem.rightClick.action.performed -= OnTwoPlayerButtonPress;
    }

    private void OnOnePlayerButtonPress(InputAction.CallbackContext context)
    {
        print(context);
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