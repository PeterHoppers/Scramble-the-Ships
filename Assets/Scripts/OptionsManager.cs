using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;
using UnityEngine.EventSystems;

public class OptionsManager : MonoBehaviour, IManager
{
    private static OptionsManager instance;
    public static OptionsManager Instance
    {
        get
        {
            return instance;
        }
        private set { }
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public TestParameters testParameters;

    [Header("UI Components")]
    public TMP_Dropdown amountScrambledDropdown;
    public Slider tickDurationSlider;
    public Slider tickScrambleSlider;
    public TMP_Dropdown moveOnInputDropdown;
    public TMP_Dropdown shootingEnabledDropdown;

    GameManager _gameManager;
    EffectsSystem _effectsSystem;

    public void InitManager(GameManager manager)
    {
        _gameManager = manager;
        _effectsSystem = manager.EffectsSystem;
        InvokeCurrentOptions();
    }

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

        transform.GetChild(0).gameObject.SetActive(false);
    }

    void InvokeCurrentOptions()
    {
        _effectsSystem.OnScrambleAmountChanged?.Invoke(testParameters.amountControlsScrambled);
        _effectsSystem.OnTickDurationChanged?.Invoke(testParameters.tickDuration);
        _effectsSystem.OnTicksUntilScrambleChanged?.Invoke(testParameters.amountTickPerScramble);
        _effectsSystem.OnMoveOnInputChanged?.Invoke(testParameters.doesMoveOnInput);
        _effectsSystem.OnShootingChanged?.Invoke(!testParameters.isShootingEnabled);
    }

    void OnScrambleDropdownUpdate()
    {
        var dropdownOptions = amountScrambledDropdown.options.Select(option => option.text).ToList();
        testParameters.amountControlsScrambled = int.Parse(dropdownOptions[amountScrambledDropdown.value]);

        if (_effectsSystem)
        {
            _effectsSystem.OnScrambleAmountChanged?.Invoke(testParameters.amountControlsScrambled);
        }
    }

    void OnTickDurationUpdate()
    {
        testParameters.tickDuration = tickDurationSlider.value / 10;
        if (_effectsSystem)
        {
            _effectsSystem.OnTickDurationChanged?.Invoke(testParameters.tickDuration);
        }
    }

    void OnMoveInputUpdate()
    {
        testParameters.doesMoveOnInput = (moveOnInputDropdown.value == 1);
        if (_effectsSystem)
        {
            _effectsSystem.OnMoveOnInputChanged?.Invoke(testParameters.doesMoveOnInput);
        }
    }

    void OnTickScrambleUpdate()
    {
        testParameters.amountTickPerScramble = (int)tickScrambleSlider.value;
        if (_effectsSystem)
        {
            _effectsSystem.OnTicksUntilScrambleChanged?.Invoke(testParameters.amountTickPerScramble);
        }
    }

    void OnShootingEnabledUpdate()
    {
        testParameters.isShootingEnabled = (shootingEnabledDropdown.value == 1);
        if (_effectsSystem)
        {
            _effectsSystem.OnShootingChanged?.Invoke(!testParameters.isShootingEnabled);
        }
    }

    public void ToggleOptions()
    {
        var holder = transform.GetChild(0);
        var toggledActiveState = !holder.gameObject.activeSelf;
        holder.gameObject.SetActive(toggledActiveState);

        if (toggledActiveState)
        {
            EventSystem.current.SetSelectedGameObject(amountScrambledDropdown.gameObject);
        }
    }

    void Update() 
    { 
        if (Input.GetKeyUp(KeyCode.Escape)) 
        {
            if (_gameManager)
            {
                _gameManager.PauseGame();
            }
            ToggleOptions();
        }
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
