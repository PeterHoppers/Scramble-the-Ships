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
        private set 
        {
            instance = value;
        }
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

    public GameSettingParameters gameSettingParameters;

    [Header("UI Components for Game")]
    public TMP_Dropdown amountScrambledDropdown;
    public Slider tickDurationSlider;
    public Slider tickScrambleSlider;
    public TMP_Dropdown moveOnInputDropdown;
    public TMP_Dropdown shootingEnabledDropdown;
    public Slider livesPerSlider;

    [Space]

    public SystemSettingParameters systemSettingParameters;
    [Header("UI Components for System")]
    public TMP_Dropdown modeTypeDropdown;
    public Slider creditsForPlaySlider;

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
        amountScrambledDropdown.value = dropdownOptions.IndexOf(gameSettingParameters.amountControlsScrambled.ToString());
        amountScrambledDropdown.onValueChanged.AddListener((int newValue) => 
        {
            var dropdownOptions = amountScrambledDropdown.options.Select(option => option.text).ToList();
            gameSettingParameters.amountControlsScrambled = int.Parse(dropdownOptions[newValue]);
        });

        tickDurationSlider.value = gameSettingParameters.tickDuration * 10; //eww, I know, but there's no good way of forcing a slider to do steps on non whole numbers
        tickDurationSlider.onValueChanged.AddListener((float newValue) => 
        {
            gameSettingParameters.tickDuration = newValue / 10;
        });

        moveOnInputDropdown.value = BoolToDropdownIndex(gameSettingParameters.doesMoveOnInput);
        moveOnInputDropdown.onValueChanged.AddListener((int newSelection) =>
        {
            gameSettingParameters.doesMoveOnInput = DropdownValueToBool(newSelection);
        });

        tickScrambleSlider.value = gameSettingParameters.amountTickPerScramble;
        tickScrambleSlider.onValueChanged.AddListener((float newValue) =>
        {
            gameSettingParameters.amountTickPerScramble = (int)newValue;
        });

        shootingEnabledDropdown.value = BoolToDropdownIndex(gameSettingParameters.isShootingEnabled);
        shootingEnabledDropdown.onValueChanged.AddListener((int newSelection) =>
        {
            gameSettingParameters.isShootingEnabled = DropdownValueToBool(newSelection);
        });

        livesPerSlider.value = gameSettingParameters.amountLivesPerPlayer;
        livesPerSlider.onValueChanged.AddListener((float newValue) => 
        {
            gameSettingParameters.amountLivesPerPlayer = (int)newValue;
        });

        modeTypeDropdown.value = BoolToDropdownIndex(systemSettingParameters.isFreeplay);
        modeTypeDropdown.onValueChanged.AddListener((int newSelection) =>
        {
            systemSettingParameters.isFreeplay = DropdownValueToBool(newSelection);
        });

        transform.GetChild(0).gameObject.SetActive(false);
    }

    void InvokeCurrentOptions()
    {
        if (!_effectsSystem)
        {
            return;
        }

        _effectsSystem.OnScrambleAmountChanged?.Invoke(gameSettingParameters.amountControlsScrambled);
        _effectsSystem.OnTickDurationChanged?.Invoke(gameSettingParameters.tickDuration);
        _effectsSystem.OnTicksUntilScrambleChanged?.Invoke(gameSettingParameters.amountTickPerScramble);
        _effectsSystem.OnMoveOnInputChanged?.Invoke(gameSettingParameters.doesMoveOnInput);
        _effectsSystem.OnShootingChanged?.Invoke(!gameSettingParameters.isShootingEnabled);
    }

    bool DropdownToBool(TMP_Dropdown dropdown)
    {
        return DropdownValueToBool(dropdown.value);
    }

    bool DropdownValueToBool(int dropdownValue)
    {
        return (dropdownValue == 1);
    }

    int BoolToDropdownIndex(bool isSelected)
    {
        return (isSelected) ? 1 : 0;
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
        else
        {
            InvokeCurrentOptions();
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
public struct GameSettingParameters
{
    public int amountControlsScrambled;
    public float tickDuration;
    public int amountTickPerScramble;
    public bool doesMoveOnInput;
    public bool isShootingEnabled;
    public int amountLivesPerPlayer;
}

[System.Serializable]
public struct SystemSettingParameters
{
    public bool isFreeplay;
    public int coinsPerPlay;
}
