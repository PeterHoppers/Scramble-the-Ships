using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class PlayerStatusUI : MonoBehaviour
{
    public ActionButtonUI fireUI;
    Player _player;

    const int BUTTON_UI_NEEDED = 5;
    
    void Awake()
    {
        fireUI.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (_player != null)
        {
            _player.OnPossibleInputs -= OnPossibleInputChanged;
        }
    }

    public void AddPlayerReference(Player player)
    {
        _player = player;
        fireUI.gameObject.SetActive(true);
        fireUI.SetButtonControlSprite(player.GetSpriteForInput(InputValue.Fire));
        player.AddButtonRenderer(ButtonValue.Action, fireUI.buttonControlRenderer);
        
        _player.OnPossibleInputs += OnPossibleInputChanged;
    }

    private void OnPossibleInputChanged(List<PlayerAction> possibleActions)
    {
        var isButtonNeededForInput = (possibleActions.Count >= BUTTON_UI_NEEDED);
        fireUI.SetActiveState(isButtonNeededForInput);        
    }    
}
