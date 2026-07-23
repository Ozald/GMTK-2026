using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public EventInstance currentBGMusic;
    public bool playMusicOnStart = true;

    public EventReference testBGM;

    //private FMOD.Studio.Bus musicBus;
    //private FMOD.Studio.Bus sfxBus;
    //private FMOD.Studio.Bus masterBus;

    void Awake()
    {
        if (instance == null)
            instance = this;

        //musicBus = FMODUnity.RuntimeManager.GetBus("bus:/Music");
        //sfxBus = FMODUnity.RuntimeManager.GetBus("bus:/SFX");
        //masterBus = FMODUnity.RuntimeManager.GetBus("bus:/");
    }

    void Start()
    {
        if (playMusicOnStart)
            PlayBGMusic(testBGM);

        //if (!PlayerPrefs.HasKey("MusicVolume"))
        //    PlayerPrefs.SetFloat("MusicVolume", 0.5f);

        //if (!PlayerPrefs.HasKey("SFXVolume"))
        //    PlayerPrefs.SetFloat("SFXVolume", 0.5f);

        //if (!PlayerPrefs.HasKey("MasterVolume"))
        //    PlayerPrefs.SetFloat("MasterVolume", 1f);

        //PlayerPrefs.Save();
    }

    void Update()
    {
        
    }

/******************************** AUDIO SETTINGS ***********************************/

    //public static void SetMusicVolume(float volume)
    //{
    //    instance.musicBus.setVolume(volume);
    //    PlayerPrefs.SetFloat("MusicVolume", volume);
    //    PlayerPrefs.Save();
    //}
    //public static void SetSFXVolume(float volume)
    //{
    //    if (volume % 0.02f < 0.001f)
    //        PlayOneShot(instance.audioData.sfxSliderChange);

    //    instance.sfxBus.setVolume(volume);
    //    PlayerPrefs.SetFloat("SFXVolume", volume);
    //    PlayerPrefs.Save();

    //}
    //public static void SetMasterVolume(float volume)
    //{
    //    instance.masterBus.setVolume(volume);
    //    PlayerPrefs.SetFloat("MasterVolume", volume);
    //    PlayerPrefs.Save();
    //}

/******************************** PUBLIC METHODS ***********************************/

    public static void PlayOneShot(EventReference sound)
    {
        RuntimeManager.PlayOneShot(sound, Vector3.zero);
    }

    public static void PlayOneShot(EventReference sound, float delay)
    {
        instance.StartCoroutine(instance.DelayedOneShot(sound, delay));
    }

    public static void PlayBGMusic(EventReference music)
    {
        instance.currentBGMusic = RuntimeManager.CreateInstance(music);
        instance.currentBGMusic.start();
    }

    public static void StopCurrentBGMusic()
    {
        instance.currentBGMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

/************************* PRIVATE HELPER METHODS ****************************/

    private IEnumerator DelayedOneShot(EventReference sound, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        RuntimeManager.PlayOneShot(sound, Vector3.zero);
    }
}
