using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewUIManager : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> _previewUIs;
    [SerializeField]
    private float _displayDuration;
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
        yield return new WaitForSeconds(_displayDuration - halfTransitionDuration);
        StartTransitionEffects();
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
            _previewUIs[index].SetActive(index == _displayIndex);
        }
    }

    void StartTransitionEffects()
    {
        _glitchAdapter.SetScanLineJitterIntensity(.5F);
        _glitchAdapter.SetColorDriftIntensity(.35f);
        _glitchAdapter.SetHorizontalShakeIntensity(.45f);
        _glitchAdapter.SetVerticalJumpIntensity(.025f);
    }
}
