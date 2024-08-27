using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;
using UnityEngine.EventSystems;

public class OptionsManager : MonoBehaviour, IManager, IDataPersistence
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

    public GameSettingParameters gameSettingParameters;

    [Header("UI Components for Game")]
    public TMP_Dropdown amountScrambledDropdown;
    public Slider tickDurationSlider;
    public Slider tickScrambleSlider;
    public TMP_Dropdown moveOnInputDropdown;
    public TMP_Dropdown shootingEnabledDropdown;
    public Slider maxEnergySlider;
    public Slider energyPerMoveSlider;
    public Slider energyPerShotSlider;
    public Slider energyPerDeathSlider;

    [Space]

    public SystemSettingParameters systemSettingParameters;
    [Header("UI Components for System")]
    public TMP_Dropdown modeTypeDropdown;
    public Slider creditsForPlaySlider;

    public delegate void ParametersChanged(GameSettingParameters gameSettings, SystemSettingParameters systemSettingParameters);
    public ParametersChanged OnParametersChanged;

    GameManager _gameManager;

    public void InitManager(GameManager manager)
    {
        _gameManager = manager;
    }

    public void AfterInitManager()
    { 
        OnParametersChanged?.Invoke(gameSettingParameters, systemSettingParameters);
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

        maxEnergySlider.value = gameSettingParameters.maxEnergy / 10;
        maxEnergySlider.onValueChanged.AddListener((float newValue) =>
        {
            gameSettingParameters.maxEnergy = (int)newValue * 10;
        });

        energyPerMoveSlider.value = gameSettingParameters.energyPerMove;
        energyPerMoveSlider.onValueChanged.AddListener((float newValue) => 
        {
            gameSettingParameters.energyPerMove = (int)newValue;
        });

        energyPerShotSlider.value = gameSettingParameters.energyPerShot;
        energyPerShotSlider.onValueChanged.AddListener((float newValue) =>
        {
            gameSettingParameters.energyPerShot = (int)newValue;
        });

        energyPerDeathSlider.value = gameSettingParameters.energyPerDeath;
        energyPerDeathSlider.onValueChanged.AddListener((float newValue) =>
        {
            gameSettingParameters.energyPerDeath = (int)newValue;
        });

        modeTypeDropdown.value = BoolToDropdownIndex(systemSettingParameters.isFreeplay);
        modeTypeDropdown.onValueChanged.AddListener((int newSelection) =>
        {
            systemSettingParameters.isFreeplay = DropdownValueToBool(newSelection);
        });

        creditsForPlaySlider.value = systemSettingParameters.coinsPerPlay;
        creditsForPlaySlider.onValueChanged.AddListener((float newValue) =>
        {
            systemSettingParameters.coinsPerPlay = (int)newValue;
        });

        transform.GetChild(0).gameObject.SetActive(false);
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
            OnParametersChanged?.Invoke(gameSettingParameters, systemSettingParameters);
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

    public void LoadData(SaveData data)
    {
        if (data == null)
        {
            return;
        }

        //if the data is empty, use the information found in the inspector
        if (data.gameSettingParameters.tickDuration == 0 || data.gameSettingParameters.maxEnergy == 0)
        {
            return;
        }

        gameSettingParameters = data.gameSettingParameters;
        systemSettingParameters = data.systemSettingParameters;
    }

    public void SaveData(SaveData data)
    {
        data.gameSettingParameters = gameSettingParameters;
        data.systemSettingParameters = systemSettingParameters;
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
    public int maxEnergy;
    public int energyPerMove;
    public int energyPerShot;
    public int energyPerDeath;
}

[System.Serializable]
public struct SystemSettingParameters
{
    public bool isFreeplay;
    public int coinsPerPlay;
}
