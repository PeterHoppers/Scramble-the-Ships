using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

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

    [SerializeField]
    private InputActionAsset _actions;
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
    public TMP_Dropdown energyRestoreDropdown;

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

    GameManager _gameManager;
    GameSettingParameters _defaultGameSettings;
    SystemSettingParameters _defaultSystemSettings;
    bool _hasBeenLoaded; 
    private InputActionMap _uiActionMap;
    private InputAction _cancelActionMap;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (!_hasBeenLoaded)
        {
            _defaultGameSettings = gameSettingParameters;
            _defaultSystemSettings = systemSettingParameters;
        }
    }

    void Start()
    {
        SetupSettings();
        AddEventListeners();
        SetOptionsCanvasVisibilty(false);
    }

    public void InitManager(GameManager manager)
    {
        _gameManager = manager;
    }

    public void AfterInitManager()
    { 
        OnParametersChanged?.Invoke(gameSettingParameters, systemSettingParameters);
    }

    private void OnEnable()
    {
        _uiActionMap = _actions.FindActionMap("ui");
        _uiActionMap.Enable();
        _cancelActionMap = _uiActionMap.FindAction("Cancel");

        _cancelActionMap.performed += OnCancelPerformed;
    }

    private void OnDisable()
    {
        _cancelActionMap.performed -= OnCancelPerformed;
    }

    private void OnCancelPerformed(InputAction.CallbackContext context)
    {
        if (_gameManager)
        {
            _gameManager.PauseGame();
        }
        ToggleOptions();
    }    

    public void ResetSettings()
    {
        gameSettingParameters = _defaultGameSettings;
        systemSettingParameters = _defaultSystemSettings;
        SetupSettings();
    }

    void SetupSettings()
    {
        var dropdownScrambledOptions = amountScrambledDropdown.options.Select(option => option.text).ToList();
        amountScrambledDropdown.value = dropdownScrambledOptions.IndexOf(gameSettingParameters.amountControlsScrambled.ToString());      

        noInputScrambleDropdown.value = BoolToDropdownIndex(gameSettingParameters.doesScrambleOnNoInput);
        multiplayerResultDropdown.value = BoolToDropdownIndex(gameSettingParameters.isMultiplayerScrambleSame);
        tickDurationSlider.SetValueToRead(gameSettingParameters.tickDuration);
        tickEndDurationSlider.SetValueToRead(gameSettingParameters.tickEndDuration);
        moveOnInputDropdown.value = (int)gameSettingParameters.inputMoveStyle;
        maxEnergySlider.SetValueToRead(gameSettingParameters.maxEnergy);
        energyPerMoveSlider.value = gameSettingParameters.energyPerMove;
        energyPerShotSlider.value = gameSettingParameters.energyPerShot;
        energyPerDeathSlider.value = gameSettingParameters.energyPerDeath;
        energyRestoreDropdown.value = BoolToDropdownIndex(gameSettingParameters.isEnergyRestoredToStartOnDeath);
        modeTypeDropdown.value = BoolToDropdownIndex(systemSettingParameters.isFreeplay);
        attractAudioDropdown.value = BoolToDropdownIndex(systemSettingParameters.isAttractAudioEnabled);
        creditsForPlaySlider.SetValueToRead(systemSettingParameters.coinsPerPlay, true);
        musicVolumeSlider.SetValueToRead(systemSettingParameters.musicVolume, true);
        sfxVolumeSlider.SetValueToRead(systemSettingParameters.sfxVolume, true);        
    }

    void AddEventListeners()
    {
        amountScrambledDropdown.onValueChanged.AddListener((int newValue) =>
        {
            var dropdownOptions = amountScrambledDropdown.options.Select(option => option.text).ToList();
            gameSettingParameters.amountControlsScrambled = int.Parse(dropdownOptions[newValue]);
        });

        noInputScrambleDropdown.onValueChanged.AddListener((int newSelection) =>
        {
            gameSettingParameters.doesScrambleOnNoInput = DropdownValueToBool(newSelection);
        });

        multiplayerResultDropdown.onValueChanged.AddListener((int newSelection) =>
        {
            gameSettingParameters.isMultiplayerScrambleSame = DropdownValueToBool(newSelection);
        });

        tickDurationSlider.OnSliderChange += (float baseValue, float convertedValue, string _) =>
        {
            gameSettingParameters.tickDuration = convertedValue;
        };

        tickEndDurationSlider.OnSliderChange += (float baseValue, float convertedValue, string _) =>
        {
            gameSettingParameters.tickEndDuration = convertedValue;
        };

        moveOnInputDropdown.onValueChanged.AddListener((int newValue) =>
        {
            gameSettingParameters.inputMoveStyle = (InputMoveStyle)newValue;
        });

        maxEnergySlider.OnSliderChange += (float baseValue, float convertedValue, string _) =>
        {
            gameSettingParameters.maxEnergy = (int)convertedValue;
        };

        energyPerMoveSlider.onValueChanged.AddListener((float newValue) =>
        {
            gameSettingParameters.energyPerMove = (int)newValue;
        });

        energyPerShotSlider.onValueChanged.AddListener((float newValue) =>
        {
            gameSettingParameters.energyPerShot = (int)newValue;
        });

        energyPerDeathSlider.onValueChanged.AddListener((float newValue) =>
        {
            gameSettingParameters.energyPerDeath = (int)newValue;
        });

        energyRestoreDropdown.onValueChanged.AddListener((int newSelection) =>
        {
            gameSettingParameters.isEnergyRestoredToStartOnDeath = DropdownValueToBool(newSelection);
        });

        modeTypeDropdown.onValueChanged.AddListener((int newSelection) =>
        {
            systemSettingParameters.isFreeplay = DropdownValueToBool(newSelection);
        });

        attractAudioDropdown.onValueChanged.AddListener((int newSelection) =>
        {
            systemSettingParameters.isAttractAudioEnabled = DropdownValueToBool(newSelection);
        });

        creditsForPlaySlider.OnSliderChange += (float baseValue, float convertedValue, string renderedText) =>
        {
            systemSettingParameters.coinsPerPlay = (int)baseValue;
            systemSettingParameters.creditDisplay = renderedText;
        };

        musicVolumeSlider.OnSliderChange += (float baseValue, float _, string _) =>
        {
            systemSettingParameters.musicVolume = baseValue;
        };

        sfxVolumeSlider.OnSliderChange += (float baseValue, float _, string _) =>
        {
            systemSettingParameters.sfxVolume = baseValue;
        };
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
        SetOptionsCanvasVisibilty(toggledActiveState);        
    }

    public void SetOptionsCanvasVisibilty(bool isVisible)
    {
        optionsCanvas.SetActive(isVisible);

        if (isVisible)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
            OnParametersChanged?.Invoke(gameSettingParameters, systemSettingParameters);
            DataPersistenceManager.Instance.SaveGame();
        }
    }

    public void LoadData(SaveData data)
    {
        _defaultGameSettings = gameSettingParameters;
        _defaultSystemSettings = systemSettingParameters;

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

        _hasBeenLoaded = true;
    }

    public void SaveData(SaveData data)
    {
        data.gameSettingParameters = gameSettingParameters;
        data.systemSettingParameters = systemSettingParameters;
    }
}

[System.Serializable]
public enum GameInputProgression
{
    SimpleMovement = 0,
    ScrambledMovement = 1,
    MoveAndShooting = 2,
    ScrambledShooting = 3,
    Rotation = 4,
    DummyShipDefault = 100,
    CrossScrambleShooting = 200,
}

[System.Serializable]
public enum InputMoveStyle
{ 
    OnInputStart = 0,
    OnInputEnd = 1,
    OnTickEnd = 2
}

[System.Serializable]
public struct GameSettingParameters
{
    public int amountControlsScrambled;
    public bool isMultiplayerScrambleSame;
    public bool doesScrambleOnNoInput;
    public float tickDuration;
    public float tickEndDuration;
    public InputMoveStyle inputMoveStyle;
    public int maxEnergy;
    public int energyPerMove;
    public int energyPerShot;
    public int energyPerDeath;
    public bool isEnergyRestoredToStartOnDeath;
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
