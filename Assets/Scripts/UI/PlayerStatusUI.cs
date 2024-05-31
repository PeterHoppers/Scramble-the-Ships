using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStatusUI : MonoBehaviour
{
    public TextMeshProUGUI joinMessage;
    public FireUI fireUI;
    Player _player;
    
    void Awake()
    {
        joinMessage.gameObject.SetActive(true);
        fireUI.gameObject.SetActive(false);
    }

    public void AddPlayerReference(Player player)
    {
        _player = player;
        joinMessage.gameObject.SetActive(false);
        fireUI.gameObject.SetActive(true);
        fireUI.SetFirableState(true);
        fireUI.SetFireControlSprite(player.GetSpriteForInput(InputValue.Fire));
        player.AddInputRenderer(InputValue.Fire, fireUI.fireControlRenderer);
    }

    public void GainedCondition(Condition condition)
    { 
        if (condition == null) 
        {
            return;        
        }

        if (condition.GetType() == typeof(ShootingDisable))
        {
            fireUI.SetFirableState(false);
        }
    }
}
