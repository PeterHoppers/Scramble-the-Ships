using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelProgressUI : MonoBehaviour
{
    private int _screensRemaining;
    public int ScreensRemaining
    { 
        get 
        {
            return _screensRemaining;
        }
        set
        {
            _screensRemaining = value;
            SetColorOfScreenImages(_screensRemaining);
        }
    }

    private int _maxScreens;

    [SerializeField]
    private GameObject _screenHolder;
    [SerializeField]
    private Image _screenImage;
    [SerializeField]
    private Color _unvisitedColor;
    [SerializeField]
    private Color _visitedColor;

    private List<Image> _screenImages = new List<Image>();

    public void SetupScreenUI(int maxScreens)
    {
        if (_maxScreens == maxScreens)
        {
            return;
        }

        _maxScreens = maxScreens;

        for (int index  = 0; index < _maxScreens; index++) 
        {
            GameObject screenImageGameObject = Instantiate(_screenImage.gameObject, _screenHolder.transform);
            _screenImages.Add(screenImageGameObject.GetComponent<Image>());
        }
    }

    void SetColorOfScreenImages(int screenIndex)
    { 
        for (int index = 0; index < _screenImages.Count; index++) 
        { 
            var targetImage = _screenImages[index];

            if (index <= screenIndex)
            {
                targetImage.color = _visitedColor;
            }
            else
            {
                targetImage.color = _unvisitedColor;
            }
        }
    }
}
