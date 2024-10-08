using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class PlayerStatusUI : MonoBehaviour
{
    public ActionButtonUI actionButtonUI;
    Player _player;

    const int BUTTON_UI_NEEDED = 5;

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
        actionButtonUI.SetUIActiveState(true);
        
        actionButtonUI.SetActionSprite(player.GetSpriteForInput(InputValue.Fire));
        player.AddButtonRenderer(ButtonValue.Action, actionButtonUI.actionRenderer);
        actionButtonUI.SetButtonSprite(player.GetActionButtonSprite());
        
        _player.OnPossibleInputs += OnPossibleInputChanged;
    }

    private void OnPossibleInputChanged(List<PlayerAction> possibleActions)
    {
        var isButtonNeededForInput = (possibleActions.Count >= BUTTON_UI_NEEDED);
        actionButtonUI.SetActiveState(isButtonNeededForInput);       
    }    
}
