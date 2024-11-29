using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance; // Singleton instance

    public AudioSource musicSource; // Audio source for background music
    public AudioSource sfxSource;   // Audio source for sound effects
    public AudioSource walkingSource;

    public AudioClip walking;

    [Header("Music")]
    public List<AudioClip> musicClips;  // List of background music clips
    private Dictionary<string, AudioClip> musicDictionary; // Dictionary for easy lookup by name

    [Header("Sound Effects")]
    public List<AudioClip> sfxClips;  // List of sound effect clips
    private Dictionary<string, AudioClip> sfxDictionary; // Dictionary for easy lookup by name

    private bool isMuted = false;

    private void Awake()
    {
        // Make the SoundManager persistent across scenes
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist between scenes
        } else {
            Destroy(gameObject); // Destroy duplicate SoundManager
        }

        InitializeDictionary();
    }

    private void Start()
    {
        SetMusicVolume(0.5f); // Set to a mid-range volume
        musicSource.mute = GameSettings.mute;
    }

    private void Update()
    {
        if (GameObject.Find("MusicSlider") != null) {
            Slider slider = GameObject.Find("MusicSlider").GetComponent<Slider>();
            if (slider != null) {
                SetMusicVolume(slider.value);
            }
        }
    }

    // Initialize the dictionary with music and sound effect names and clips
    private void InitializeDictionary()
    {
        sfxDictionary = new Dictionary<string, AudioClip>();
        musicDictionary = new Dictionary<string, AudioClip>();

        foreach (AudioClip clip in sfxClips) {
            // Use the clip's name as the key; you can change this if you want to use custom keys
            sfxDictionary[clip.name] = clip;
        }

        foreach (AudioClip clip in musicClips) {
            // Use the clip's name as the key; you can change this if you want to use custom keys
            musicDictionary[clip.name] = clip;
        }
    }

    // Play a specific music clip by name
    public void PlayMusic(string musicName, bool loop)
    {
        if (musicDictionary.TryGetValue(musicName, out AudioClip clip)) {
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        } else {
            Debug.LogWarning("Music not found: " + musicName);
        }
    }

    // Play a specific sound effect by name
    public void PlaySFX(string sfxName, bool loop)
    {
        if (sfxDictionary.TryGetValue(sfxName, out AudioClip clip)) {
            if (loop) {
                sfxSource.clip = clip;
                sfxSource.loop = loop;
                sfxSource.Play();
            } else {
                sfxSource.PlayOneShot(clip);
            }
        } else {
            Debug.LogWarning("Sound effect not found: " + sfxName);
        }
    }

    // Stop the currently playing music
    public void StopMusic()
    {
        musicSource.Stop();
    }

    // Stop the currently playing sound effect
    public void StopSFX()
    {
        sfxSource.Stop();
    }

    public void StopWalking()
    {
        if (walkingSource.isPlaying) {
            walkingSource.Stop();
        }
    }

    public void StartWalking()
    {
        // Check if the walking sound is not already playing
        if (!walkingSource.isPlaying) {
            walkingSource.clip = walking;
            walkingSource.loop = true; // Enable looping
            walkingSource.Play();
        }
    }

    // Mute or unmute all audio
    public void ToggleMute()
    {
        GameSettings.mute = !GameSettings.mute;
        AudioListener.volume = GameSettings.mute ? 0 : 1; // Mute or unmute all sounds globally
    }

    public void ToggleController()
    {
        GameSettings.usingController = !GameSettings.usingController;
    }

    // Check if sound is currently muted
    public bool IsMuted()
    {
        return isMuted;
    }

    // Set specific volume for music and sound effects
    public void SetMusicVolume(float volume)
    {
        musicSource.volume = Mathf.Clamp01(volume); // Clamp between 0 and 1
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = Mathf.Clamp01(volume); // Clamp between 0 and 1
    }
}
