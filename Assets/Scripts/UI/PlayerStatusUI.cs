using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using AYellowpaper.SerializedCollections;

public class PlayerStatusUI : MonoBehaviour
{
    public ActionButtonUI actionButtonUI;
    Player _player;

    private void OnDisable()
    {
        if (_player != null)
        {
            _player.OnScrambledInputsChanged -= OnScrambledInputChanged;
        }
    }

    public void AddPlayerReference(Player player)
    {
        _player = player;
        _player.OnScrambledInputsChanged += OnScrambledInputChanged;

        actionButtonUI.SetUIActiveState(true);
        
        actionButtonUI.SetActionSprite(player.GetSpriteForInput(InputValue.Fire));
        player.AddButtonRenderer(ButtonValue.Action, actionButtonUI.actionRenderer);
        actionButtonUI.SetButtonSprite(player.GetActionButtonSprite());
    }

    public void RemovePlayerReference(Player player) 
    {
        player.OnScrambledInputsChanged -= OnScrambledInputChanged;
    }

    private void OnScrambledInputChanged(SerializedDictionary<ButtonValue, PlayerAction> scrambledActions)
    {
        var isButtonNeededForInput = scrambledActions.ContainsKey(ButtonValue.Action);
        actionButtonUI.SetActiveState(isButtonNeededForInput);
    }
}
