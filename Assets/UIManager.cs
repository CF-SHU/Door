using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("三个界面")]
    public GameObject roleListPanel;
    public GameObject roleDetailPanel;
    public GameObject chatPanel;

    private RoleData _currentSelectedRole;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 打开角色列表
    public void ShowRoleList()
    {
        roleListPanel.SetActive(true);
        roleDetailPanel.SetActive(false);
        chatPanel.SetActive(false);
    }

    // 打开详情页，并传入角色
    public void ShowRoleDetail(RoleData role)
    {
        _currentSelectedRole = role;

        roleListPanel.SetActive(false);
        roleDetailPanel.SetActive(true);
        chatPanel.SetActive(false);

        // 刷新详情页
        RoleDetailUI detail = roleDetailPanel.GetComponent<RoleDetailUI>();
        if (detail != null)
            detail.SetRoleData(role);
    }

    // 打开聊天界面，并传入角色（关键加固）
    public void ShowChatPanel(RoleData role)
    {
        if (role == null)
        {
            Debug.LogError("UIManager：传入聊天的角色为 null！");
            return;
        }

        _currentSelectedRole = role;

        roleListPanel.SetActive(false);
        roleDetailPanel.SetActive(false);
        chatPanel.SetActive(true);

        // 刷新聊天界面，强制调用 SetRoleData
        AIDialogController chat = chatPanel.GetComponent<AIDialogController>();
        if (chat != null)
        {
            chat.SetRoleData(role);
            Debug.Log("UIManager 成功传给聊天：" + role.charName);
        }
        else
        {
            Debug.LogError("UIManager：chatPanel 上没有 AIDialogController！");
        }
    }

    // 返回：聊天 → 详情
    public void BackToDetail()
    {
        if (_currentSelectedRole != null)
            ShowRoleDetail(_currentSelectedRole);
    }
}