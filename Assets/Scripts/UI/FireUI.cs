using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FireUI : MonoBehaviour
{
    public GameObject unableToFireUI;
    public GameObject ableToFireUI;
    public InputRenderer fireControlRenderer;

    bool _isFirable;

    public void SetFirableState(bool isFirable)
    {
        _isFirable = isFirable;
        unableToFireUI.SetActive(!isFirable);
        ableToFireUI.SetActive(isFirable);
    }

    public void SetFireControlSprite(Sprite sprite)
    {
        fireControlRenderer.SetSprite(sprite);
    }
}
