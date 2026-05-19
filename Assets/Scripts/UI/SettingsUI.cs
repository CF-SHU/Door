using UnityEngine;
using UnityEngine.UI;

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
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
