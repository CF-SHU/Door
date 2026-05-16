// UIPanelManager.cs
using UnityEngine;
using System.Collections.Generic;

public class UIPanelManager : MonoBehaviour
{
    // 存储所有面板
    private Dictionary<string, GameObject> panels = new Dictionary<string, GameObject>();
    private Stack<GameObject> panelHistory = new Stack<GameObject>();
    private GameObject currentPanel;

    void Start()
    {
        // 注册所有子面板（假设它们都是本GameObject的直接子对象）
        foreach (Transform child in transform)
        {
            panels[child.name] = child.gameObject;
        }
    }

    // 显示指定面板
    public void ShowPanel(string panelName)
    {
        if (currentPanel != null)
        {
            currentPanel.SetActive(false);
            panelHistory.Push(currentPanel);
        }

        if (panels.ContainsKey(panelName))
        {
            currentPanel = panels[panelName];
            currentPanel.SetActive(true);
        }
    }

    // 返回上一个面板
    public void GoBack()
    {
        if (panelHistory.Count > 0)
        {
            if (currentPanel != null)
                currentPanel.SetActive(false);
            currentPanel = panelHistory.Pop();
            currentPanel.SetActive(true);
        }
    }
}