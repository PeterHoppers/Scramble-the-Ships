using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GlobalAudioManager : MonoBehaviour
{
    public AudioMixer globalAudioMixer;
    [Space]
    public AudioSource sfxSource;
    public AudioSource musicSource;
    [Space]
    public AudioClip mainMenuMusic;
    public AudioClip cutsceneMusic;
    [Space]
    public AudioClip menuSFX;
    public AudioClip clickSFX;

    private const string MUSIC_VOLUME = "MusicVolume";
    private const string SFX_VOLUME = "SFXVolume";

    private GameObject _previousSelectedObject;
    private bool _isPreviewAudioEnabled = true;
    private GlobalGameStateStatus _currentGameState = GlobalGameStateStatus.Preview;
    private float _currentMusicVolume = 0f;
    private float _currentSFXVolume = 0f;

    private static GlobalAudioManager instance;
    public static GlobalAudioManager Instance
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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += GetFirstSelectedObject;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= GetFirstSelectedObject;
    }

    void Start()
    {
        OptionsManager.Instance.OnParametersChanged += OnOptionParametersChanged;
        GlobalGameStateManager.Instance.OnStateChange += OnGlobalStateChange;

        OnOptionParametersChanged(OptionsManager.Instance.gameSettingParameters, OptionsManager.Instance.systemSettingParameters);
        PlayMusic(mainMenuMusic);
    }

    void GetFirstSelectedObject(Scene _, LoadSceneMode mode)
    {
        _previousSelectedObject = EventSystem.current.firstSelectedGameObject;
    }

    private void Update()
    {
        var currentObject = EventSystem.current.currentSelectedGameObject;

        if (currentObject != null && _previousSelectedObject != currentObject)
        {
            _previousSelectedObject = currentObject;
            PlayAudioSFX(menuSFX);
        }
    }

    void OnGlobalStateChange(GlobalGameStateStatus newState)
    {
        if (!_isPreviewAudioEnabled)
        {
            //if we're entering the preview state, mute the game
            if (newState == GlobalGameStateStatus.Preview)
            {
                if (_currentGameState != GlobalGameStateStatus.Preview)
                {
                    musicSource.Stop();
                    UpdateMixerSettings(0, 0);
                }
            }
            else
            //if we're leaving the preview state, unmute the game
            {
                if (_currentGameState == GlobalGameStateStatus.Preview)
                {
                    UpdateMixerSettings(_currentMusicVolume, _currentSFXVolume);
                    musicSource.Stop();
                    musicSource.Play();
                }                
            }
        }

        UpdateBackgroundMusic(newState);
        _currentGameState = newState;
    }

    void UpdateBackgroundMusic(GlobalGameStateStatus newState)
    {
        switch (newState)
        {
            case GlobalGameStateStatus.Preview:
                TransitionSongs(mainMenuMusic);
                break;
            case GlobalGameStateStatus.Cutscene:
                TransitionSongs(cutsceneMusic);
                break;
            default:
                break;
        }
    }

    public void TransitionSongs(AudioClip clipToPlay, float transitionDuration = .25f)
    {
        if (clipToPlay == null || clipToPlay == musicSource.clip) 
        {
            return;
        }

        globalAudioMixer.GetFloat(MUSIC_VOLUME, out var currentVol);

        if (currentVol == 0f)
        {
            return;
        }

        if (musicSource.clip == null)
        {
            PlayMusic(clipToPlay); 
            return;
        }

        StartCoroutine(TransitionBetweenTracks(clipToPlay, transitionDuration));
    }

    public void PlayClickSFX()
    { 
        PlayAudioSFX(clickSFX);
    }

    public void PlayAudioSFX(AudioClip clipToPlay, bool isLoop = false)
    {
        PlayClip(sfxSource, clipToPlay, isLoop);
    }

    public void StopAudioSFX()
    { 
        sfxSource?.Stop();
    }

    public void PlayMusic(AudioClip clipToPlay, bool isLoop = true)
    {
        PlayClip(musicSource, clipToPlay, isLoop);
    }

    void PlayClip(AudioSource source, AudioClip clip, bool isLoop)
    {
        source.clip = clip;
        source.loop = isLoop;
        source.Play();
    }

    IEnumerator TransitionBetweenTracks(AudioClip clipToPlay, float duration)
    {
        globalAudioMixer.GetFloat(MUSIC_VOLUME, out var currentVol);
        StartCoroutine(StartFade(globalAudioMixer, MUSIC_VOLUME, duration, 0));
        yield return new WaitForSeconds(duration);
        globalAudioMixer.SetFloat(MUSIC_VOLUME, currentVol);
        PlayMusic(clipToPlay);
    }

    //from: https://discussions.unity.com/t/changing-audio-mixer-group-volume-with-ui-slider/567394/11
    void OnOptionParametersChanged(GameSettingParameters _, SystemSettingParameters systemSettingParameters)
    {
        _isPreviewAudioEnabled = systemSettingParameters.isAttractAudioEnabled; 
        _currentMusicVolume = systemSettingParameters.musicVolume;
        _currentSFXVolume = systemSettingParameters.sfxVolume;

        if (!_isPreviewAudioEnabled && _currentGameState == GlobalGameStateStatus.Preview)
        {
            UpdateMixerSettings(0, 0);
        }
        else
        {            
            UpdateMixerSettings(_currentMusicVolume, _currentSFXVolume);
        }        
    }

    void UpdateMixerSettings(float musicVolume, float sfxVolume)
    {
        SetMixerVolumeLevel(globalAudioMixer, MUSIC_VOLUME, musicVolume);
        SetMixerVolumeLevel(globalAudioMixer, SFX_VOLUME, sfxVolume);
    }

    void SetMixerVolumeLevel(AudioMixer mixer, string variableName, float soundLevel)
    {
        soundLevel /= 100;
        if (soundLevel == 0)
        {
            soundLevel = .001f; //It’s important to set the min value to 0.001, otherwise dropping it all the way to zero breaks the calculation and puts the volume up again.
        }

        mixer.SetFloat(variableName, Mathf.Log(soundLevel) * 20);
    }

    //from: https://johnleonardfrench.com/how-to-fade-audio-in-unity-i-tested-every-method-this-ones-the-best/
    IEnumerator StartFade(AudioMixer audioMixer, string exposedParam, float duration, float targetVolume)
    {
        float currentTime = 0;
        float currentVol;
        audioMixer.GetFloat(exposedParam, out currentVol);
        currentVol = Mathf.Pow(10, currentVol / 20);
        float targetValue = Mathf.Clamp(targetVolume, 0.0001f, 1);
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float newVol = Mathf.Lerp(currentVol, targetValue, currentTime / duration);
            audioMixer.SetFloat(exposedParam, Mathf.Log10(newVol) * 20);
            yield return null;
        }
        yield break;
    }
}
