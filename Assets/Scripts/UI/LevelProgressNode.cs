using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RendererTransition))]
[RequireComponent (typeof(Image))]
public class LevelProgressNode : MonoBehaviour
{
    RendererTransition _rendererTransition;
    Image _levelProgressImage;

    bool _isActivated = false;

    private void Awake()
    {
        _rendererTransition = GetComponent<RendererTransition>();
        _levelProgressImage = GetComponent<Image>();
    }

    public void ActivateNode(float duration)
    {
        if (_isActivated)
        {
            return;
        }

        _isActivated = true;
        _rendererTransition.AnimateFill(_levelProgressImage, 0, 1, duration);
    }

    public void DeactivateNode() 
    {
        _levelProgressImage.fillAmount = 0;
    }
}
