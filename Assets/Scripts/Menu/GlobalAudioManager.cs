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
        UpdateMixerSettings(OptionsManager.Instance.gameSettingParameters, OptionsManager.Instance.systemSettingParameters);
    }

    public void PlayAudioSFX(AudioClip clipToPlay)
    {
        sfxSource.clip = clipToPlay;
        sfxSource.Play();
    }

    //from: https://discussions.unity.com/t/changing-audio-mixer-group-volume-with-ui-slider/567394/11
    void UpdateMixerSettings(GameSettingParameters _, SystemSettingParameters systemSettingParameters)
    {
        SetMixerVolumeLevel(globalAudioMixer, "MusicVolume", systemSettingParameters.musicVolume);
        SetMixerVolumeLevel(globalAudioMixer, "SFXVolume", systemSettingParameters.sfxVolume);
    }

    void SetMixerVolumeLevel(AudioMixer mixer, string variableName, float soundLevel)
    {
        if (soundLevel == 0)
        {
            soundLevel = .0001f; //It’s important to set the min value to 0.001, otherwise dropping it all the way to zero breaks the calculation and puts the volume up again.
        }

        mixer.SetFloat(variableName, Mathf.Log(soundLevel) * 20);
    }
}
