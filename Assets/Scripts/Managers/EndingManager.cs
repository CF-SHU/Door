// EndingManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingManager : MonoBehaviour
{
    [Header("结局设置")]
    public string endingID;         // 结局ID（如"good_ending", "bad_ending"）
    public string endingName;       // 结局名称
    [TextArea(3, 5)]
    public string endingDescription; // 结局描述

    // 触发结局
    public void TriggerEnding()
    {
        // 记录达成的结局
        if (!SaveManager.CurrentData.triggeredFlags.Contains(endingID))
        {
            SaveManager.CurrentData.triggeredFlags.Add(endingID);
        }

        // 自动存档
        SaveManager.AutoSave();

        // 显示结局画面（可以通过UI面板展示）
        StartCoroutine(ShowEndingAndReturn());
    }

    System.Collections.IEnumerator ShowEndingAndReturn()
    {
        // 显示结局画面，等待几秒
        yield return new WaitForSeconds(5f);

        // 返回主菜单
        SceneManager.LoadScene("MainMenu");
    }
}