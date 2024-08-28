using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderReader : MonoBehaviour
{
    public Slider slider;
    public float stepFactor;
    public string textPrepender;
    public TextMeshProUGUI sliderDisplay;

    public delegate void SliderChange(float baseValue, float convertedValue);
    public SliderChange OnSliderChange;

    // Start is called before the first frame update
    void Start()
    {
        slider.onValueChanged.AddListener((float newValue) => OnSliderUpdate(newValue));
        OnSliderUpdate(slider.value);
    }

    void OnSliderUpdate(float sliderValue)
    {
        var convertedValue = sliderValue * stepFactor;
        sliderDisplay.text = $"{textPrepender}{convertedValue}";
        OnSliderChange?.Invoke(sliderValue, convertedValue);
    }

    public void SetTextValue(float baseValue)
    { 
        var convertedValue = baseValue /= stepFactor;
        sliderDisplay.text = $"{textPrepender}{convertedValue}";
        slider.value = baseValue;
    }

    public float GetSliderValue()
    {
        return slider.value * stepFactor;
    }

}
