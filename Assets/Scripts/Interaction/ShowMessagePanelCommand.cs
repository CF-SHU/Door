using UnityEngine;
using Fungus;

[CommandInfo("Custom", 
             "Show Message Panel", 
             "显示一个 ScrollView 消息面板，展示长文本，关闭后继续")]
[AddComponentMenu("")]
public class ShowMessagePanelCommand : Command
{
    [Tooltip("消息面板预制体（或场景中的实例）")]
    public MessagePanelController messagePanelPrefab;

    [Tooltip("要显示的文本内容，支持变量")]
    [TextArea(5, 10)]
    public string content;

    // 如果面板需要从场景中获取（而不是实例化预制体），可以用这个
    public bool useExistingInstance = false;

    private MessagePanelController currentPanel;

    public override void OnEnter()
    {
        if (messagePanelPrefab == null)
        {
            Debug.LogError("ShowMessagePanelCommand: 没有指定消息面板预制体！");
            Continue();
            return;
        }

        if (useExistingInstance)
        {
            // 假设场景中已经有一个激活的实例（可能手动放置）
            currentPanel = FindObjectOfType<MessagePanelController>();
            if (currentPanel == null)
            {
                Debug.LogError("场景中未找到 MessagePanelController 实例！");
                Continue();
                return;
            }
        }
        else
        {
            // 实例化预制体，并设置其父级为 Canvas（需要找到 Canvas）
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("场景中没有 Canvas！");
                Continue();
                return;
            }
            currentPanel = Instantiate(messagePanelPrefab, canvas.transform);
        }

        // 显示面板，传入内容，并在关闭时调用 Continue
        currentPanel.Show(content, () => {
            // 如果不是使用场景中的实例，则销毁实例
            if (!useExistingInstance && currentPanel != null)
                Destroy(currentPanel.gameObject);
            Continue();
        });
    }

    public override string GetSummary()
    {
        if (messagePanelPrefab == null)
            return "Error: 未绑定消息面板预制体";
        return "显示: " + (content.Length > 30 ? content.Substring(0, 30) + "..." : content);
    }
}