using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SoundMixerManager : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    public string[] trackNames; // Names of the audio mixer tracks (e.g., "Drums", "Synth", "Piano", "Piano2")
    public float minLerpTime = 2f;
    public float maxLerpTime = 5f;

    private Coroutine lerpCoroutine;

    private static bool hasInstance = false;
    private bool drums = false;

    private void Awake()
    {
        if (!hasInstance)
        {
            // If this is the first instance, mark it as created and make it persistent
            hasInstance = true;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // If another instance is found, destroy it
            Destroy(gameObject);
        }
    }

    private void StartRandomVolumeLerp()
    {
        // Pick a random track
        string randomTrackName = trackNames[Random.Range(0, trackNames.Length)];

        // Determine the target volume (-80 to 0 or 0 to -80)
        float targetVolume = Random.Range(0f, 1f) > 0.5f ? 0f : -80f;

        // Calculate a random lerp time
        float lerpTime = Random.Range(minLerpTime, maxLerpTime);

        // Start lerping the volume
        lerpCoroutine = StartCoroutine(LerpVolume(randomTrackName, targetVolume, lerpTime));
    }

    private void Update()
    {
        // Check if we are in the target scene
        if (!drums && SceneManager.GetActiveScene().name == "Game Scene")
        {
            // Set the drums' volume to 0
            StartCoroutine(LerpVolume("Drums", 0f, 5f));
            StartCoroutine(LerpVolume("Synth", 0f, 5f));
            StartCoroutine(LerpVolume("BottomPiano", 0f, 5f));
            drums = true;
        }
    }

    private IEnumerator LerpVolume(string trackName, float targetVolume, float lerpTime)
    {
        float startVolume;
        audioMixer.GetFloat(trackName, out startVolume);

        float currentTime = 0f;

        while (currentTime < lerpTime)
        {
            currentTime += Time.deltaTime;
            float t = currentTime / lerpTime;
            float volume = Mathf.Lerp(startVolume, targetVolume, t);
            audioMixer.SetFloat(trackName, volume);
            yield return null;
        }

        // Ensure the volume reaches the exact target value
        audioMixer.SetFloat(trackName, targetVolume);
    }

    public void SetMasterVolume(float level)
    {
        audioMixer.SetFloat("MasterVolume", level);
    }

    public void SetSoundFXVolume(float level)
    {
        audioMixer.SetFloat("SoundFXVolume", level);
    }

    public void SetMusicVolume(float level)
    {
        audioMixer.SetFloat("MusicVolume", level);
    }

}
