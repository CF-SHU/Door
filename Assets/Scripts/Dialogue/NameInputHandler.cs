using UnityEngine;
using UnityEngine.UI;
using Fungus; // 务必引入Fungus命名空间

public class NameInputHandler : MonoBehaviour
{
    [Header("UI References")]
    public InputField playerNameInput; // 从Inspector拖入InputField
    public GameObject nameInputPanel;  // 从Inspector拖入NameInputPanel对象

    [Header("Fungus References")]
    public Flowchart flowchart;        // 从Inspector拖入Flowchart对象
    public string fungusVariableName = "playerName"; // 与Fungus变量名严格一致
    public string blockToExecute = "AfterNameInput"; // 输入后要执行的Block名字

    public void OnConfirmName()
    {
        // 1. 获取并存储玩家输入的名字
        string playerName = playerNameInput.text;
        if (string.IsNullOrEmpty(playerName))
            playerName = "佚名"; // 防止空输入，可自行修改

        // 2. 将名字存到Fungus的String变量中
        flowchart.SetStringVariable(fungusVariableName, playerName);

        // 3. 隐藏输入面板
        nameInputPanel.SetActive(false);

        // 4. 通知Fungus继续执行后续的Block
        flowchart.ExecuteBlock(blockToExecute);
    }

    void OnEnable()
    {
        // 如果这个脚本所在对象被激活，会打印日志（可选）
        Debug.Log("NameInputHandler 被激活，检查是谁激活了它的 GameObject");
    }

    public void ShowInputPanel()
    {
        Debug.Log("ShowInputPanel 被调用，调用栈：" + StackTraceUtility.ExtractStackTrace());
        nameInputPanel.SetActive(true);
    }
}