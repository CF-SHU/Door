using UnityEngine;
using UnityEngine.SceneManagement;
using Fungus;
using System.Reflection;

public class GameBootstrap : MonoBehaviour
{
    void Awake()
    {
        SaveManager.Initialize();
        DontDestroyOnLoad(gameObject);

        // 首次启动时应用一次设置
        ApplySettingsToAllWriters();

        // 场景加载后延时一帧再应用（等待 Fungus UI 生成）
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(ApplyWriterSpeedNextFrame());
    }

    System.Collections.IEnumerator ApplyWriterSpeedNextFrame()
    {
        yield return null; // 等一帧
        ApplySettingsToAllWriters();
    }

    void ApplySettingsToAllWriters()
    {
        float secondsPerChar = SettingsManager.TextSpeed;
        if (secondsPerChar < 0.001f) secondsPerChar = 0.05f;
        float charsPerSecond = 1.0f / secondsPerChar;

        Writer[] writers = Object.FindObjectsOfType<Writer>();
        var field = typeof(Writer).GetField("writingSpeed",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (field == null)
        {
            Debug.LogError("反射获取 writingSpeed 失败");
            return;
        }

        foreach (Writer w in writers)
        {
            field.SetValue(w, charsPerSecond);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}