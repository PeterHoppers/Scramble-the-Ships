using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderReader : MonoBehaviour
{
    public Slider slider;
    public float stepFactor;
    TextMeshProUGUI _sliderDisplay;

    // Start is called before the first frame update
    void Start()
    {
        _sliderDisplay = GetComponent<TextMeshProUGUI>();
        OnSliderUpdate();
        slider.onValueChanged.AddListener(delegate { OnSliderUpdate(); });
    }

    void OnSliderUpdate()
    {
        var sliderValue = slider.value / stepFactor;
        _sliderDisplay.text = sliderValue.ToString();
    }

}
