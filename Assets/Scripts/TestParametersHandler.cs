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
        instance = this;
    }

    public TestParameters testParameters;

    [Header("UI Components")]
    public GameObject testParamsHolder;
    public TMP_Dropdown amountScrambledDropdown;
    public Slider tickDurationSlider;
    public Slider tickScrambleSlider;
    public TMP_Dropdown moveOnInputDropdown;
    public TMP_Dropdown shootingEnabledDropdown;

    public delegate void ParametersChanged(TestParameters newParameters);
    public ParametersChanged OnParametersChanged;

    void Start()
    {
        SetupSettings();
    }

    void SetupSettings()
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

        shootingEnabledDropdown.value = (testParameters.isShootingEnabled) ? 1 : 0;
        shootingEnabledDropdown.onValueChanged.AddListener(delegate { OnShootingEnabledUpdate(); });

        OnParametersChanged?.Invoke(testParameters);

        if (testParamsHolder.gameObject != null)
        {
            testParamsHolder.gameObject.SetActive(false);
        }
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

    void OnShootingEnabledUpdate()
    {
        testParameters.isShootingEnabled = (shootingEnabledDropdown.value == 1);
        OnParametersChanged?.Invoke(testParameters);
    }

    public void ToggleOptions()
    {
        var isActive = testParamsHolder.activeSelf;
        testParamsHolder.gameObject.SetActive(!isActive);
    }
}

[System.Serializable]
public struct TestParameters
{
    public int amountControlsScrambled;
    public float tickDuration;
    public int amountTickPerScramble;
    public bool doesMoveOnInput;
    public bool isShootingEnabled;
}
