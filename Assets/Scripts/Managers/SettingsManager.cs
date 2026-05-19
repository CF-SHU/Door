// SettingsManager.cs
using UnityEngine;

public static class SettingsManager
{
    // PlayerPrefs的Key常量
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SE_VOLUME_KEY = "SoundEffectVolume";
    private const string TEXT_SPEED_KEY = "TextSpeed";

    // 音乐音量（0~1的浮点数）
    public static float MusicVolume
    {
        // PlayerPrefs.GetFloat(键名, 默认值)
        get => PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0.8f);
        set
        {
            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, value);
            PlayerPrefs.Save(); // 立即写入磁盘
        }
    }

    // 音效音量
    public static float SEVolume
    {
        get => PlayerPrefs.GetFloat(SE_VOLUME_KEY, 1f);
        set
        {
            PlayerPrefs.SetFloat(SE_VOLUME_KEY, value);
            PlayerPrefs.Save();
        }
    }

    // 文本速度（每个字的显示间隔，单位：秒）
    public static float TextSpeed
    {
        get => PlayerPrefs.GetFloat(TEXT_SPEED_KEY, 0.05f);
        set
        {
            PlayerPrefs.SetFloat(TEXT_SPEED_KEY, value);
            PlayerPrefs.Save();
        }
    }
}