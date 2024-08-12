using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

[RequireComponent(typeof(InputSystemUIInputModule))]
public class PreviewInputHandler : MonoBehaviour
{
    InputSystemUIInputModule _inputSystemUIInputModule;

    private void Awake()
    {
        _inputSystemUIInputModule = GetComponent<InputSystemUIInputModule>();
        _inputSystemUIInputModule.middleClick.action.performed += SelectedOnePlayer;
        _inputSystemUIInputModule.rightClick.action.performed += SelectedOnePlayer;
    }

    private void SelectedOnePlayer(InputAction.CallbackContext obj)
    {
        GlobalGameStateManager.Instance.PlayerCount = 1;
        GlobalGameStateManager.Instance.GlobalGameStateStatus = GlobalGameStateStatus.LevelSelect;
    }

    private void SelectedTwoPlayers(InputAction.CallbackContext obj)
    {
        GlobalGameStateManager.Instance.PlayerCount = 2;
        GlobalGameStateManager.Instance.GlobalGameStateStatus = GlobalGameStateStatus.LevelSelect;
    }
}
