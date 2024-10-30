using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewUIManager : MonoBehaviour
{
    [SerializeField]
    private List<PreviewInfo> _previewUIs;
    [SerializeField]
    private float _transitionDuration;

    private GlitchAdapter _glitchAdapter;
    private int _displayIndex = 0;

    private void Awake()
    {
        _glitchAdapter = Camera.main.GetComponent<GlitchAdapter>();
    }

    private void OnEnable()
    {
        DisplayUI();
        StartCoroutine(IterateThroughDisplays());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator IterateThroughDisplays()
    {
        var halfTransitionDuration = _transitionDuration / 2;
        var displayDuration = _previewUIs[_displayIndex].duration;
        yield return new WaitForSeconds(displayDuration - halfTransitionDuration);
        _glitchAdapter.PerformDefaultGlitchTransitionEffect();
        yield return new WaitForSeconds(halfTransitionDuration);
        _displayIndex++;

        if (_displayIndex >= _previewUIs.Count)
        {
            GlobalGameStateManager.Instance.PlayPreviewLevel();
        }
        else
        {
            DisplayUI();
            StartCoroutine(IterateThroughDisplays());
        }

        yield return new WaitForSeconds(halfTransitionDuration);
        _glitchAdapter.ClearGlitchEffects();
    }

    void DisplayUI()
    {
        for (int index = 0; index < _previewUIs.Count; index++)
        {
            _previewUIs[index].UI.SetActive(index == _displayIndex);
        }
    }
}

[Serializable]
public struct PreviewInfo
{
    public GameObject UI;
    public float duration;
}
