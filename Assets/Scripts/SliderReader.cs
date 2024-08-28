using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SliderReader : MonoBehaviour
{
    public Slider slider;
    public float stepFactor;
    public TextMeshProUGUI sliderDisplay;
    public bool isMoney = false;

    public delegate void SliderChange(float baseValue, float convertedValue, string renderedText);
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
        var textDisplay = SetText(convertedValue);
        OnSliderChange?.Invoke(sliderValue, convertedValue, textDisplay);
    }

    string SetText(float valueToPrint)
    {
        if (isMoney)
        {
            decimal moneyNumber = (decimal)valueToPrint;
            sliderDisplay.text = String.Format("{0:C}", moneyNumber);
        }
        else
        {
            sliderDisplay.text = $"{valueToPrint}";
        }

        return sliderDisplay.text;
    }

    public void SetValueToRead(float baseValue)
    { 
        var convertedValue = baseValue /= stepFactor;
        SetText(convertedValue);
        slider.value = baseValue;
    }

    public float GetSliderValue()
    {
        return slider.value * stepFactor;
    }

}
