using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLauncher : MonoBehaviour
{
    // 新游戏按钮调用的方法
    public void StartNewGame()
    {
        // 调用静态类的重置逻辑
        SaveManager.StartNewGame();

        // 加载第一个游戏场景（请替换成你实际的第一关场景名或索引）
        SceneManager.LoadScene(0); 
    }

    // 继续游戏按钮调用的方法（可选）
    public void ContinueGame()
    {
        SaveManager.LoadGame();  // 无参重载：自动加载最新存档
    }
}