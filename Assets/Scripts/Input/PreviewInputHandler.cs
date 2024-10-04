using UnityEngine;

public class PreviewInputHandler : MonoBehaviour
{
    private void Update()
    {
        if (GlobalGameStateManager.Instance.GlobalGameStateStatus != GlobalGameStateStatus.Preview)
        {
            return;
        }

        if (Input.GetKeyUp(KeyCode.Alpha1) || Input.GetKeyUp(KeyCode.Keypad1))
        {
            if (GlobalGameStateManager.Instance.CanPlay(1))
            {
                SelectedOnePlayer();
            }
        }

        if (Input.GetKeyUp(KeyCode.Alpha2) || Input.GetKeyUp(KeyCode.Keypad2))
        {
            if (GlobalGameStateManager.Instance.CanPlay(2))
            {
                SelectedTwoPlayers();
            }
        }
    }

    private void SelectedOnePlayer()
    {
        GlobalGameStateManager.Instance.SetPlayerCount(1);
        GlobalGameStateManager.Instance.StartGame();
    }

    private void SelectedTwoPlayers()
    {
        GlobalGameStateManager.Instance.SetPlayerCount(2);
        GlobalGameStateManager.Instance.StartGame();
    }    
}