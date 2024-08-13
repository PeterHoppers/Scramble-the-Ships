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

    [Header("UI Components")]
    public TMP_Dropdown amountScrambledDropdown;
    public Slider tickDurationSlider;
    public Slider tickScrambleSlider;
    public TMP_Dropdown moveOnInputDropdown;
    public TMP_Dropdown shootingEnabledDropdown;
    public Slider livesPerSlider;

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
        amountScrambledDropdown.onValueChanged.AddListener(delegate { OnScrambleDropdownUpdate(); });

        tickDurationSlider.value = gameSettingParameters.tickDuration * 10; //eww, I know, but there's no good way of forcing a slider to do steps on non whole numbers
        tickDurationSlider.onValueChanged.AddListener(delegate { OnTickDurationUpdate(); });

        moveOnInputDropdown.value = (gameSettingParameters.doesMoveOnInput) ? 1 : 0;
        moveOnInputDropdown.onValueChanged.AddListener(delegate { OnMoveInputUpdate(); });

        tickScrambleSlider.value = gameSettingParameters.amountTickPerScramble;
        tickScrambleSlider.onValueChanged.AddListener(delegate { OnTickScrambleUpdate(); });

        shootingEnabledDropdown.value = (gameSettingParameters.isShootingEnabled) ? 1 : 0;
        shootingEnabledDropdown.onValueChanged.AddListener(delegate { OnShootingEnabledUpdate(); });

        livesPerSlider.value = gameSettingParameters.amountLivesPerPlayer;
        livesPerSlider.onValueChanged.AddListener(delegate { OnLivesAmountUpdate(); });

        transform.GetChild(0).gameObject.SetActive(false);
    }

    void InvokeCurrentOptions()
    {
        _effectsSystem.OnScrambleAmountChanged?.Invoke(gameSettingParameters.amountControlsScrambled);
        _effectsSystem.OnTickDurationChanged?.Invoke(gameSettingParameters.tickDuration);
        _effectsSystem.OnTicksUntilScrambleChanged?.Invoke(gameSettingParameters.amountTickPerScramble);
        _effectsSystem.OnMoveOnInputChanged?.Invoke(gameSettingParameters.doesMoveOnInput);
        _effectsSystem.OnShootingChanged?.Invoke(!gameSettingParameters.isShootingEnabled);
    }

    void OnScrambleDropdownUpdate()
    {
        var dropdownOptions = amountScrambledDropdown.options.Select(option => option.text).ToList();
        gameSettingParameters.amountControlsScrambled = int.Parse(dropdownOptions[amountScrambledDropdown.value]);

        if (_effectsSystem)
        {
            _effectsSystem.OnScrambleAmountChanged?.Invoke(gameSettingParameters.amountControlsScrambled);
        }
    }

    void OnTickDurationUpdate()
    {
        gameSettingParameters.tickDuration = tickDurationSlider.value / 10;
        if (_effectsSystem)
        {
            _effectsSystem.OnTickDurationChanged?.Invoke(gameSettingParameters.tickDuration);
        }
    }

    void OnMoveInputUpdate()
    {
        gameSettingParameters.doesMoveOnInput = (moveOnInputDropdown.value == 1);
        if (_effectsSystem)
        {
            _effectsSystem.OnMoveOnInputChanged?.Invoke(gameSettingParameters.doesMoveOnInput);
        }
    }

    void OnTickScrambleUpdate()
    {
        gameSettingParameters.amountTickPerScramble = (int)tickScrambleSlider.value;
        if (_effectsSystem)
        {
            _effectsSystem.OnTicksUntilScrambleChanged?.Invoke(gameSettingParameters.amountTickPerScramble);
        }
    }

    void OnLivesAmountUpdate()
    {
        gameSettingParameters.amountLivesPerPlayer = (int)livesPerSlider.value;
    }

    void OnShootingEnabledUpdate()
    {
        gameSettingParameters.isShootingEnabled = (shootingEnabledDropdown.value == 1);
        if (_effectsSystem)
        {
            _effectsSystem.OnShootingChanged?.Invoke(!gameSettingParameters.isShootingEnabled);
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
public struct GameSettingParameters
{
    public int amountControlsScrambled;
    public float tickDuration;
    public int amountTickPerScramble;
    public bool doesMoveOnInput;
    public bool isShootingEnabled;
    public int amountLivesPerPlayer;
}

public struct SystemSettingParameters
{
    public bool isFreeplay;
}
