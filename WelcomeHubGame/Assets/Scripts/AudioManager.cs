using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

// this part is just so that it runs Awake() before most other scripts
[DefaultExecutionOrder(-100)]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer")] 
    public AudioMixer mixer;
    
    [Header("Sound Library")]
    [Tooltip("Shared lookup table of soundID -> AudioClip, so callers don't need separate clip references")]
    public SoundLibrary soundLibrary;
    
    [Header("Music Library")]
    [Tooltip("Shared lookup table of musicID -> AudioClip, so callers don't need separate clip references")]
    public MusicLibrary musicLibrary;

    // ill change this to enums or something later... probably...
    // these have to match the EXACT exposed parameter names set in the Audio Mixer window
    public string masterVolumeParam = "MasterVolume";
    public string musicVolumeParam = "MusicVolume";
    public string sfxVolumeParam = "SFXVolume";
    public string uiVolumeParam = "UIVolume";

    [Header("Mixer Groups")] 
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup sfxGroup;
    public AudioMixerGroup uiGroup;

    [Header("Music")]
    [Tooltip("Default crossfade duration in seconds when none is specified per-call")]
    public float defaultFadeDuration = 1.5f;

    private AudioSource _musicSourceA;
    private AudioSource _musicSourceB;
    private AudioSource _activeMusicSource;
    private Coroutine _musicFadeRoutine;
    
    private AudioSource _sfxSource;
    private AudioSource _uiSource;

    private const string MasterVolumeKey = "AudioManager_MasterVolume";
    private const string MusicVolumeKey = "AudioManager_MusicVolume";
    private const string SFXVolumeKey = "AudioManager_SFXVolume";
    private const string UIVolumeKey = "AudioManager_UIVolume";

    private void Awake()
    {
        if (!Instance) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SetupAudioSources();
        LoadVolumes();
    }

    private void SetupAudioSources()
    {
        _musicSourceA = CreateSource("MusicSourceA", musicGroup, loop: true);
        _musicSourceB = CreateSource("MusicSourceB", musicGroup, loop: true);
        _activeMusicSource = _musicSourceA;
        
        _sfxSource = CreateSource("SFXSource", sfxGroup, loop: false);
        _uiSource = CreateSource("UISource", uiGroup, loop: false);
    }

    private AudioSource CreateSource(string name, AudioMixerGroup group, bool loop)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform);
        var src = go.AddComponent<AudioSource>();
        src.outputAudioMixerGroup = group;
        src.loop = loop;
        src.playOnAwake = false;
        return src;
    }

    public void PlayMusic(AudioClip clip, float fadeDuration = -1f)
    {
        if (!clip) return;
        if (_activeMusicSource.clip == clip && _activeMusicSource.isPlaying) return;
        
        float duration = fadeDuration >= 0f ? fadeDuration : defaultFadeDuration;

        if (_musicFadeRoutine != null) StopCoroutine(_musicFadeRoutine);
        _musicFadeRoutine = StartCoroutine(CrossfadeMusic(clip, duration));
    }

    private void PlayMusic(MusicID musicID, float fadeDuration = -1f)
    {
        if (musicID == MusicID.None)
        {
            StopMusic(fadeDuration);
            return;
        }

        if (!musicLibrary)
        {
            Debug.LogWarning("AudioManager: no MusicLibrary assigned");
            return;
        }
        PlayMusic(musicLibrary.Get(musicID), fadeDuration);
    }

    public void StopMusic(float fadeDuration = -1f)
    {
        float duration = fadeDuration >= 0f ? fadeDuration : defaultFadeDuration;
        if (_musicFadeRoutine != null) StopCoroutine(_musicFadeRoutine);
        _musicFadeRoutine = StartCoroutine(FadeOut(_activeMusicSource, duration));
    }

    private IEnumerator CrossfadeMusic(AudioClip newClip, float duration)
    {
        AudioSource fadeIn = (_activeMusicSource == _musicSourceA) ? _musicSourceB : _activeMusicSource;
        AudioSource fadeOut = _activeMusicSource;
        
        fadeIn.clip = newClip;
        fadeIn.volume = 0f;
        fadeIn.Play();
        
        float startVolOut = fadeOut.volume;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = duration <= 0f ? 1f : t / duration;
            fadeIn.volume = Mathf.Lerp(0f, 1f, p);
            fadeOut.volume = Mathf.Lerp(startVolOut, 0f, p);
            yield return null;
        }

        fadeIn.volume = 1f;
        fadeOut.Stop();
        fadeOut.volume = 1f;

        _activeMusicSource = fadeIn;
        _musicFadeRoutine = null;
    }

    private IEnumerator FadeOut(AudioSource source, float duration)
    {
        float startVol = source.volume;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(startVol, 0f, t / duration);
            yield return null;
        }
        source.Stop();
        source.volume = 1f;
        _musicFadeRoutine = null;
    }

    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (!clip) return;
        _sfxSource.pitch = pitch;
        _sfxSource.PlayOneShot(clip, volume);
    }

    public void PlaySFX(SoundID soundID, float volume = 1f, float pitch = 1f)
    {
        if (!soundLibrary)
        {
            Debug.LogWarning("AudioManager: no SoundLibrary assigned");
            return;
        }
        PlaySFX(soundLibrary.Get(soundID), volume, pitch);
    }

    public void PlayUISound(AudioClip clip, float volume = 1f)
    {
        if (!clip) return;
        _uiSource.PlayOneShot(clip, volume);
    }

    public void PlayUISound(SoundID soundID, float volume = 1f)
    {
        if (!soundLibrary)
        {
            Debug.LogWarning("AudioManager: no SoundLibrary assigned");
            return;
        }
        PlayUISound(soundLibrary.Get(soundID), volume);
    }

    public void SetMasterVolume01(float value01) => SetVolume(masterVolumeParam, MasterVolumeKey, value01);
    public void SetMusicVolume01(float value01) => SetVolume(musicVolumeParam, MusicVolumeKey, value01);
    public void SetSFXVolume01(float value01) => SetVolume(sfxVolumeParam, SFXVolumeKey, value01);
    public void SetUIVolume01(float value01) => SetVolume(uiVolumeParam, UIVolumeKey, value01);
    
    public float GetMasterVolume01() => PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
    public float GetMusicVolume01() => PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
    public float GetSFXVolume01() => PlayerPrefs.GetFloat(SFXVolumeKey, 1f);
    public float GetUIVolume01() => PlayerPrefs.GetFloat(UIVolumeKey, 1f);

    private void SetVolume(string exposedParam, string prefsKey, float value01)
    {
        ApplyMixerVolume(exposedParam, value01);
        PlayerPrefs.SetFloat(prefsKey, value01);
    }
    
    private void ApplyMixerVolume(string exposedParam, float value01)
    {
        float dB = value01 <= 0.0001f ? -80f : Mathf.Log10(value01) * 20f;
        mixer.SetFloat(exposedParam, dB);
    }

    private void LoadVolumes()
    {
        ApplyMixerVolume(masterVolumeParam, PlayerPrefs.GetFloat(MasterVolumeKey, 1f));
        ApplyMixerVolume(musicVolumeParam, PlayerPrefs.GetFloat(MusicVolumeKey, 1f));
        ApplyMixerVolume(sfxVolumeParam, PlayerPrefs.GetFloat(SFXVolumeKey, 1f));
        ApplyMixerVolume(uiVolumeParam, PlayerPrefs.GetFloat(UIVolumeKey, 1f));
    }
}
