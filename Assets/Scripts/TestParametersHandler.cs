using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;

public class TestParametersHandler : MonoBehaviour
{
    private static TestParametersHandler instance;
    public static TestParametersHandler Instance
    {
        get
        {
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        instance = this;
    }

    public TestParameters testParameters;

    [Header("UI Components")]
    public TMP_Dropdown amountScrambledDropdown;
    public Slider tickDurationSlider;
    public TMP_Dropdown moveOnInputDropdown;
    public Slider tickScrambleSlider;

    public delegate void ParametersChanged(TestParameters newParameters);
    public ParametersChanged OnParametersChanged;

    void Start() 
    {
        var dropdownOptions = amountScrambledDropdown.options.Select(option => option.text).ToList();
        amountScrambledDropdown.value = dropdownOptions.IndexOf(testParameters.amountControlsScrambled.ToString());
        amountScrambledDropdown.onValueChanged.AddListener(delegate { OnScrambleDropdownUpdate(); });

        tickDurationSlider.value = testParameters.tickDuration * 10; //eww, I know, but there's no good way of forcing a slider to do steps on non whole numbers
        tickDurationSlider.onValueChanged.AddListener(delegate { OnTickDurationUpdate(); });

        moveOnInputDropdown.value = (testParameters.doesMoveOnInput) ? 1 : 0;
        moveOnInputDropdown.onValueChanged.AddListener(delegate { OnMoveInputUpdate(); });

        tickScrambleSlider.value = testParameters.amountTickPerScramble;
        tickScrambleSlider.onValueChanged.AddListener(delegate { OnTickScrambleUpdate(); });

        OnParametersChanged?.Invoke(testParameters);
    }

    void OnScrambleDropdownUpdate()
    {
        var dropdownOptions = amountScrambledDropdown.options.Select(option => option.text).ToList();
        testParameters.amountControlsScrambled = int.Parse(dropdownOptions[amountScrambledDropdown.value]);
        OnParametersChanged?.Invoke(testParameters);
    }

    void OnTickDurationUpdate()
    {
        testParameters.tickDuration = tickDurationSlider.value / 10;
        OnParametersChanged?.Invoke(testParameters);
    }

    void OnMoveInputUpdate()
    {
        testParameters.doesMoveOnInput = (moveOnInputDropdown.value == 1);
        OnParametersChanged?.Invoke(testParameters);
    }

    void OnTickScrambleUpdate()
    {
        testParameters.amountTickPerScramble = (int)tickScrambleSlider.value;
        OnParametersChanged?.Invoke(testParameters);
    }
}

[System.Serializable]
public struct TestParameters
{
    public int amountControlsScrambled;
    public float tickDuration;
    public int amountTickPerScramble;
    public bool doesMoveOnInput;
}
