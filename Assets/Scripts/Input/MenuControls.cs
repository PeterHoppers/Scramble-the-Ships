using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuControls : MonoBehaviour
{
    public PlayerInput menuPlayer;
    PlayerInputManager playerInputManager;

    bool _hasSelectedPlayers = false;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_hasSelectedPlayers) 
        {
            SceneManager.LoadScene(1);
        }

        if (Input.GetKeyUp(KeyCode.Alpha1) || Input.GetKeyUp(KeyCode.Keypad1)) 
        {
            GlobalGameState.Instance.PlayerCount = 1;
            _hasSelectedPlayers = true;
        }

        if (Input.GetKeyUp(KeyCode.Alpha2) || Input.GetKeyUp(KeyCode.Keypad2))
        {
            GlobalGameState.Instance.PlayerCount = 2;
            _hasSelectedPlayers = true;            
        }
    }
}
