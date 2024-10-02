using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActionButtonUI : MonoBehaviour
{
    public GameObject holderUI;
    public InputRenderer buttonControlRenderer;

    public void SetActiveState(bool isFirable)
    {
        holderUI.SetActive(isFirable);
    }

    public void SetButtonControlSprite(Sprite sprite)
    {
        buttonControlRenderer.SetSprite(sprite);
    }
}
