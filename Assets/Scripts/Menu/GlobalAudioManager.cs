using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class GlobalAudioManager : MonoBehaviour
{
    public AudioMixer globalAudioMixer;
    [Space]
    public AudioSource sfxSource;
    public AudioSource musicSource;
    [Space]
    public AudioClip mainMenuMusic;
    public AudioClip cutsceneMusic;

    private const string MUSIC_VOLUME = "MusicVolume";
    private const string SFX_VOLUME = "SFXVolume";

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

    void Start()
    {
        OptionsManager.Instance.OnParametersChanged += UpdateMixerSettings;
        GlobalGameStateManager.Instance.OnStateChange += UpdateBackgroundMusic;
        UpdateMixerSettings(OptionsManager.Instance.gameSettingParameters, OptionsManager.Instance.systemSettingParameters);
        PlayMusic(mainMenuMusic);
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
        if (musicSource.clip == null)
        {
            PlayMusic(clipToPlay); 
            return;
        }

        StartCoroutine(TransitionBetweenTracks(clipToPlay, transitionDuration));
    }

    public void PlayAudioSFX(AudioClip clipToPlay)
    {
        sfxSource.clip = clipToPlay;
        sfxSource.Play();
    }

    void PlayMusic(AudioClip clipToPlay)
    {
        musicSource.clip = clipToPlay;
        musicSource.Play();
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
    void UpdateMixerSettings(GameSettingParameters _, SystemSettingParameters systemSettingParameters)
    {
        SetMixerVolumeLevel(globalAudioMixer, MUSIC_VOLUME, systemSettingParameters.musicVolume);
        SetMixerVolumeLevel(globalAudioMixer, SFX_VOLUME, systemSettingParameters.sfxVolume);
    }

    void SetMixerVolumeLevel(AudioMixer mixer, string variableName, float soundLevel)
    {
        if (soundLevel == 0)
        {
            soundLevel = .0001f; //It’s important to set the min value to 0.001, otherwise dropping it all the way to zero breaks the calculation and puts the volume up again.
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
