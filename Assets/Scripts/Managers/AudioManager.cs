// AudioManager.cs
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("音频源")]
    public AudioSource musicSource;    // 背景音乐
    public AudioSource sfxSource;      // 音效

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 播放背景音乐
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    // 播放音效
    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    // 更新音量（配合设置面板）
    public void UpdateVolumes()
    {
        musicSource.volume = SettingsManager.MusicVolume;
        sfxSource.volume = SettingsManager.SEVolume;
    }
}