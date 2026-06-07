// UIPanelManager.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;  // 如果需要用 FirstOrDefault，可以加上

public class UIPanelManager : MonoBehaviour
{
    private Dictionary<string, GameObject> panels = new Dictionary<string, GameObject>();
    private Stack<GameObject> panelHistory = new Stack<GameObject>();
    private GameObject currentPanel;

    void Start()
    {
        // 1. 注册所有直接子对象
        foreach (Transform child in transform)
        {
            panels[child.name] = child.gameObject;
        }

        // 2. 先把所有面板都关掉
        foreach (var panel in panels.Values)
        {
            panel.SetActive(false);
        }

        // 3. 找到主菜单面板并激活它（假设名字是 "MainMenuPanel"）
        if (panels.ContainsKey("MainMenuPanel"))
        {
            currentPanel = panels["MainMenuPanel"];
            currentPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("找不到 MainMenuPanel！请检查 Hierarchy 中的面板命名。");
        }
    }

    public void ShowPanel(string panelName)
    {
        if (!panels.ContainsKey(panelName))
        {
            Debug.LogError($"面板 '{panelName}' 不存在！");
            return;
        }

        // 隐藏当前面板，并压入历史栈
        if (currentPanel != null)
        {
            currentPanel.SetActive(false);
            panelHistory.Push(currentPanel);
        }

        // 显示新面板
        currentPanel = panels[panelName];
        currentPanel.SetActive(true);
    }

    public void GoBack()
    {
        if (panelHistory.Count > 0)
        {
            if (currentPanel != null)
                currentPanel.SetActive(false);

            currentPanel = panelHistory.Pop();
            currentPanel.SetActive(true);
        }
        else
        {
            Debug.Log("已经是最上层面板，无法返回。");
        }
        Debug.Log("GoBack 被调用，当前栈深度：" + panelHistory.Count);

    }
}