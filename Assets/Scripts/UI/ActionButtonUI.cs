using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActionButtonUI : MonoBehaviour
{
    public GameObject holderUI;
    public InputRenderer actionRenderer;
    public InputRenderer buttonRenderer;

    public void SetUIActiveState(bool isActive)
    { 
        gameObject.SetActive(isActive);
    }

    public void SetActiveState(bool isActive)
    {
        holderUI.SetActive(isActive);
    }

    public void SetActionSprite(Sprite sprite)
    {
        actionRenderer.SetSprite(sprite);
    }

    public void SetButtonSprite(Sprite sprite) 
    {
        buttonRenderer.SetSprite(sprite);
    }
}
