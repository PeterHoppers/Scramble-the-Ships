using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalAudioManager : MonoBehaviour
{
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

    public void PlayAudioSFX(AudioClip clipToPlay)
    {
        sfxSource.clip = clipToPlay;
        sfxSource.Play();
    }
}
