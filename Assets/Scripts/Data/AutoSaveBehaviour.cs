using UnityEngine;

public class AutoSaveBehaviour : MonoBehaviour
{
    private static bool hasContinued = false;
    private void Awake()
    {
        // 确保该物体在场景切换时不被销毁，以持续响应退出事件
        DontDestroyOnLoad(gameObject);
        if (!hasContinued)
        {
            // 游戏启动时，自动加载最近的存档（如果有）
            hasContinued = true;
            TryContinueGame();
        }
    }

    private void TryContinueGame()
    {
        // 检查是否有任何存档（自动存档或手动存档）
        if (SaveManager.HasSaveData())
        {
            Debug.Log("检测到存档，自动继续游戏...");
            SaveManager.LoadGame();  // 无参重载：优先自动存档，否则最新手动存档
        }
        else
        {
            Debug.Log("没有找到存档，开始新游戏。");
            // 这里可以加载新游戏场景，或停留在主菜单让玩家选择
        }
    }

    private void OnApplicationQuit()
    {
        Debug.Log("游戏退出，执行自动存档...");
        SaveManager.AutoSave();
    }
}