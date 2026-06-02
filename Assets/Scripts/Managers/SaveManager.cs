using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

//这里用static表明整个类可以被外部引用
public static class SaveManager
{
    //Application.persistentDataPath: unity提供的跨平台持久化数据存储路径，
    //在Windows上是 C:\Users\用户名\AppData\LocalLow\公司名\项目名，打包后存档文件会保存在这里。
    private static string SavePath => Application.persistentDataPath + "/saves";
    private static string AutoSavePath => SavePath + "/autosave.json";
    //存档数据存储,文件名 + GameData对象
    private static Dictionary<string, GameData> saveCache = new Dictionary<string, GameData>();
    //游戏数据
    public static GameData CurrentData { get; private set; }

    //初始化（游戏启动时调用）
    public static void Initialize()
    {
        CurrentData = new GameData();
        //确保存档文件夹存在
        if (!Directory.Exists(SavePath))
            Directory.CreateDirectory(SavePath);
    }

    //保存游戏到存档位——————————————————？Json
    //slotIndex: 1，2，3...
    public static void SaveGame(int slotIndex)
    {
        string path = SavePath + "save_" + slotIndex + ".json";
        CurrentData.saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        //IsonUtility.ToJson: 将C#对象转为JSON字符串
        string json = JsonUtility.ToJson(CurrentData, true);
        File.WriteAllText(path, json);

        Debug.Log($"游戏已保存到档位{slotIndex}:{path}");
    }

    //从指定档位读取存档
    public static void LoadGame(int slotIndex)
    {
        string path = SavePath + "save_" + slotIndex + ".json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            //JsonUtility.FromJson: 将JSON字符串转回C#对象
            CurrentData = JsonUtility.FromJson<GameData>(json);
            //加载对应场景
            UnityEngine.SceneManagement.SceneManager.LoadScene(CurrentData.currentSceneIndex);
        }
    }
    // 加载最新存档（先查自动存档，没有再找槽位1～5中修改时间最新的）
    public static void LoadGame()
    {
        // 优先加载自动存档
        string autoPath = SavePath + "autosave.json";
        if (File.Exists(autoPath))
        {
            string json = File.ReadAllText(autoPath);
            CurrentData = JsonUtility.FromJson<GameData>(json);
            SceneManager.LoadScene(CurrentData.currentSceneIndex);
            return;
        }

        // 否则查找保存时间最新的手动存档
        string latestFile = null;
        System.DateTime latestTime = System.DateTime.MinValue;
        for (int i = 1; i <= 5; i++)
        {
            string path = SavePath + "save_" + i + ".json";
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                GameData tempData = JsonUtility.FromJson<GameData>(json);
                System.DateTime tempTime = System.DateTime.Parse(tempData.saveTime);
                if (tempTime > latestTime)
                {
                    latestTime = tempTime;
                    latestFile = path;
                }
            }
        }

        if (latestFile != null)
        {
            string json = File.ReadAllText(latestFile);
            CurrentData = JsonUtility.FromJson<GameData>(json);
            SceneManager.LoadScene(CurrentData.currentSceneIndex);
        }
        else
        {
            Debug.Log("没有找到任何存档！");
        }
    }

    //自动存档（退出游戏/结局触发时调用）
    public static void AutoSave()
    {
        CurrentData.saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string json = JsonUtility.ToJson(CurrentData, true);
        File.WriteAllText(AutoSavePath, json);
    }

    //检查是否存在存档
    public static bool HasSaveData()
    {
        return File.Exists(AutoSavePath) || File.Exists(SavePath + "save_1.json");
    }

    //获取存档信息列表（用于存档选择界面UI展示）
    public static List<SaveSlotInfo> GetSaveSlots()
    {
        List<SaveSlotInfo> slots = new List<SaveSlotInfo>();
        for (int i = 1; i <= 5; ++i)
        {
            string path = SavePath + "save_" + i + ".json";
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                GameData data = JsonUtility.FromJson<GameData>(json);
                slots.Add(new SaveSlotInfo
                {
                    slotIndex = i,
                    saveTime = data.saveTime,
                    sceneName = "Scene_" + data.currentSceneIndex
                });
            }
            else
            {
                slots.Add(new SaveSlotInfo { slotIndex = i, isEmpty = true });
            }
        }
        return slots;
    }

    public static void StartNewGame()
    {
        // 重置当前数据
        CurrentData = new GameData();
        // 可选：删除自动存档文件
        if (File.Exists(AutoSavePath))
            File.Delete(AutoSavePath);
    }

}

//存档位信息
public class SaveSlotInfo
{
    public int slotIndex;
    public string saveTime;
    public string sceneName;
    public bool isEmpty = true;
}