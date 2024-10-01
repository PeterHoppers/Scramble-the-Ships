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

    public GameObject optionsCanvas;
    [Space]
    public GameSettingParameters gameSettingParameters;

    [Header("UI Components for Game")]
    public TMP_Dropdown amountScrambledDropdown;
    public TMP_Dropdown noInputScrambleDropdown;
    public TMP_Dropdown multiplayerResultDropdown;
    public SliderReader tickDurationSlider;
    public SliderReader tickEndDurationSlider;
    public TMP_Dropdown moveOnInputDropdown;
    public SliderReader maxEnergySlider;
    public Slider energyPerMoveSlider;
    public Slider energyPerShotSlider;
    public Slider energyPerDeathSlider;

    [Space]

    public SystemSettingParameters systemSettingParameters;
    [Header("UI Components for System")]
    public SliderReader musicVolumeSlider;
    public SliderReader sfxVolumeSlider;
    public TMP_Dropdown modeTypeDropdown;
    public TMP_Dropdown attractAudioDropdown;
    public SliderReader creditsForPlaySlider;

    public delegate void ParametersChanged(GameSettingParameters gameSettings, SystemSettingParameters systemSettingParameters);
    public ParametersChanged OnParametersChanged;

    TabbedPanelUI _panelUI;
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
        _panelUI = optionsCanvas.GetComponentInChildren<TabbedPanelUI>();
    }

    void Start()
    {
        SetupSettings();
    }

    void SetupSettings()
    {
        var dropdownScrambledOptions = amountScrambledDropdown.options.Select(option => option.text).ToList();
        amountScrambledDropdown.value = dropdownScrambledOptions.IndexOf(gameSettingParameters.amountControlsScrambled.ToString());
        amountScrambledDropdown.onValueChanged.AddListener((int newValue) => 
        {
            var dropdownOptions = amountScrambledDropdown.options.Select(option => option.text).ToList();
            gameSettingParameters.amountControlsScrambled = int.Parse(dropdownOptions[newValue]);
        });

        noInputScrambleDropdown.value = BoolToDropdownIndex(gameSettingParameters.doesScrambleOnNoInput);
        noInputScrambleDropdown.onValueChanged.AddListener((int newSelection) =>
        {
            gameSettingParameters.doesScrambleOnNoInput = DropdownValueToBool(newSelection);
        });

        multiplayerResultDropdown.value = BoolToDropdownIndex(gameSettingParameters.isMultiplayerScrambleSame);
        multiplayerResultDropdown.onValueChanged.AddListener((int newSelection) =>
        {
            gameSettingParameters.isMultiplayerScrambleSame = DropdownValueToBool(newSelection);
        });

        tickDurationSlider.SetValueToRead(gameSettingParameters.tickDuration);
        tickDurationSlider.OnSliderChange += (float baseValue, float convertedValue, string _) => 
        {
            gameSettingParameters.tickDuration = convertedValue;
        };

        tickEndDurationSlider.SetValueToRead(gameSettingParameters.tickEndDuration);
        tickEndDurationSlider.OnSliderChange += (float baseValue, float convertedValue, string _) =>
        {
            gameSettingParameters.tickEndDuration = convertedValue;
        };

        moveOnInputDropdown.value = BoolToDropdownIndex(gameSettingParameters.doesMoveOnInput);
        moveOnInputDropdown.onValueChanged.AddListener((int newSelection) =>
        {
            gameSettingParameters.doesMoveOnInput = DropdownValueToBool(newSelection);
        });

        maxEnergySlider.SetValueToRead(gameSettingParameters.maxEnergy);
        maxEnergySlider.OnSliderChange += (float baseValue, float convertedValue, string _) =>
        {
            gameSettingParameters.maxEnergy = (int)convertedValue;
        };

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

        attractAudioDropdown.value = BoolToDropdownIndex(systemSettingParameters.isAttractAudioEnabled);
        attractAudioDropdown.onValueChanged.AddListener((int newSelection) =>
        {
            systemSettingParameters.isAttractAudioEnabled = DropdownValueToBool(newSelection);
        });

        creditsForPlaySlider.SetValueToRead(systemSettingParameters.coinsPerPlay, true);
        creditsForPlaySlider.OnSliderChange += (float baseValue, float convertedValue, string renderedText) =>
        {
            systemSettingParameters.coinsPerPlay = (int)baseValue;
            systemSettingParameters.creditDisplay = renderedText;
        };

        musicVolumeSlider.SetValueToRead(systemSettingParameters.musicVolume, true);
        musicVolumeSlider.OnSliderChange += (float baseValue, float _, string _) =>
        {
            systemSettingParameters.musicVolume = baseValue;
        };

        sfxVolumeSlider.SetValueToRead(systemSettingParameters.sfxVolume, true);
        sfxVolumeSlider.OnSliderChange += (float baseValue, float _, string _) =>
        {
            systemSettingParameters.sfxVolume = baseValue;
        };

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
        var toggledActiveState = !optionsCanvas.activeSelf;
        optionsCanvas.SetActive(toggledActiveState);

        if (toggledActiveState)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
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
public enum ScrambleType
{ 
    None = 0,
    Movement = 1,
    All = 2,
    Rotation = 3,
}

[System.Serializable]
public struct GameSettingParameters
{
    public int amountControlsScrambled;
    public ScrambleType scrambleType;
    public bool isMultiplayerScrambleSame;
    public bool doesScrambleOnNoInput;
    public float tickDuration;
    public float tickEndDuration;
    public bool doesMoveOnInput;
    public int maxEnergy;
    public int energyPerMove;
    public int energyPerShot;
    public int energyPerDeath;
}

[System.Serializable]
public struct SystemSettingParameters
{
    public float sfxVolume;
    public float musicVolume;
    public bool isAttractAudioEnabled;
    public bool isFreeplay;
    public int coinsPerPlay;
    [HideInInspector]
    public string creditDisplay; //we hide it since it is based upon coins per play, so we only expose the variable once. Plus, money conversion stuff
}
