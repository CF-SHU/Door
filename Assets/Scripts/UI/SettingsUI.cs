using UnityEngine;
using UnityEngine.UI;
using Fungus;
using System.Reflection;   // 反射命名空间

public class SettingsUI : MonoBehaviour
{
    [Header("设置音量/音效/速度")]
    public Slider musicSlider;
    public Slider seSlider;
    public Slider textSpeedSlider;

    [Header("返回按钮")]
    public Button returnButton;

    void Start()
    {
        musicSlider.value = SettingsManager.MusicVolume;
        seSlider.value = SettingsManager.SEVolume;
        textSpeedSlider.value = SettingsManager.TextSpeed;

        // AddListener：注册一个回调函数
        musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        seSlider.onValueChanged.AddListener(OnSEVolumeChanged);
        textSpeedSlider.onValueChanged.AddListener(OnTextSpeedChanged);

        returnButton.onClick.AddListener(() => { GetComponent<UIPanelManager>()?.GoBack(); });
    }
    void OnMusicVolumeChanged(float value)
    {
        SettingsManager.MusicVolume = value;
        // AudioListener.volume控制全局音量
        // 也可以用AudioMixer实现更精细的控制
        AudioListener.volume = value;
    }
    void OnSEVolumeChanged(float value)
    {
        SettingsManager.SEVolume = value;
    }
    void OnTextSpeedChanged(float value)
    {
        SettingsManager.TextSpeed = value;
        ApplyTextSpeedToAllWriters(value);
    }

    void ApplyTextSpeedToAllWriters(float secondsPerChar)
    {
        // secondsPerChar 是“每个字符等待秒数”，需要转成 Fungus 的“每秒字符数”
        if (secondsPerChar < 0.001f)
            secondsPerChar = 0.05f; // 防止除零

        float charsPerSecond = 1.0f / secondsPerChar;

        Writer[] writers = Object.FindObjectsOfType<Writer>();
        foreach (Writer w in writers)
        {
            SetWritingSpeed(w, charsPerSecond);
        }
    }

    /// <summary>
    /// 通过反射修改 Writer 的 protected 字段 writingSpeed
    /// </summary>
    void SetWritingSpeed(Writer writer, float charsPerSecond)
    {
        var field = typeof(Writer).GetField("writingSpeed",
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(writer, charsPerSecond);
        }
        else
        {
            Debug.LogError("未找到 Writer.writingSpeed 字段，请检查 Fungus 版本");
        }
    }

// Update is called once per frame
void Update()
    {
        
    }
}
