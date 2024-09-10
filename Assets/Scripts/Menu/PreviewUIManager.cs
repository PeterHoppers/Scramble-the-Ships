using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreviewUIManager : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> _previewUIs;
    [SerializeField]
    private float _displayDuration;

    private int _displayIndex = 0;

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
        yield return new WaitForSeconds(_displayDuration);
        _displayIndex++;

        if (_displayIndex >= _previewUIs.Count)
        { 
            _displayIndex = 0;
        }

        DisplayUI();
        StartCoroutine(IterateThroughDisplays());
    }

    void DisplayUI()
    {
        for (int index = 0; index < _previewUIs.Count; index++)
        {
            _previewUIs[index].SetActive(index == _displayIndex);
        }
    }
}
