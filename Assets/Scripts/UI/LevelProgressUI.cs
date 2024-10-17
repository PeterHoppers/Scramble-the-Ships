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
    private float _levelProgressDuration;
    [SerializeField]
    private GameObject _screenHolder;
    [SerializeField]
    private LevelProgressNode _levelProgressNode;

    private List<LevelProgressNode> _progressNodes = new List<LevelProgressNode>();

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void SetupScreenUI(int maxScreens)
    {
        if (_maxScreens == maxScreens)
        {
            return;
        }

        gameObject.SetActive(true);
        _maxScreens = maxScreens;

        for (int index  = 0; index < _maxScreens; index++) 
        {
            GameObject screenImageGameObject = Instantiate(_levelProgressNode.gameObject, _screenHolder.transform);
            _progressNodes.Add(screenImageGameObject.GetComponent<LevelProgressNode>());
        }
    }

    void SetColorOfScreenImages(int screenCount)
    {
        int screenIndex = screenCount - 1; //1th to 0th
        for (int index = 0; index < _progressNodes.Count; index++) 
        { 
            var targetNode = _progressNodes[index];

            if (index <= screenIndex)
            {
                targetNode.ActivateNode(_levelProgressDuration);
            }
            else
            {
                targetNode.DeactivateNode();
            }
        }
    }
}
