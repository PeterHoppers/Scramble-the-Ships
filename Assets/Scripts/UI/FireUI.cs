using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FireUI : MonoBehaviour
{
    public GameObject unableToFireUI;
    public GameObject ableToFireUI;
    public InputRenderer fireControlRenderer;
    public Image bulletImage;
    public TextMeshProUGUI bulletsRemainingText;

    public void SetFirableState(bool isFirable)
    {
        unableToFireUI.SetActive(!isFirable);
        ableToFireUI.SetActive(isFirable);
    }

    public void SetFireControlSprite(Sprite sprite)
    {
        fireControlRenderer.SetSprite(sprite);
    }

    public void SetBulletSprite(Sprite sprite) 
    { 
        bulletImage.sprite = sprite;
    }

    public void UpdateBulletUI(int bulletsRemaining)
    {
        bulletsRemainingText.text = $"x{bulletsRemaining}";
    }
}
