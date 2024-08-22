using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LivesUI : MonoBehaviour
{
    public Image baseLifeDisplay;
    List<Image> shipImages = new List<Image>();

    public void SetupLives(Sprite sprite, int lives)
    {
        foreach (var image in shipImages)
        {
            Destroy(image.gameObject);
        }

        shipImages.Clear();

        for (int index = 0; index < lives; index++)
        { 
            var newImage = Instantiate(baseLifeDisplay, transform);
            newImage.sprite = sprite;
            shipImages.Add(newImage);
        }
    }

    public void LossLife(int livesRemaining)
    {
        //shipImages[livesRemaining].gameObject.SetActive(false);
    }
}
