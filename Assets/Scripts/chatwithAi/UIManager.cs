using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    // 动态查找的面板，不再通过 Inspector 拖拽
    private GameObject roleListPanel;
    private GameObject roleDetailPanel;
    private GameObject chatPanel;

    private RoleData _currentSelectedRole;

    // 解决AI聊天的角色数据bug
    public RoleData GetCurrentSelectedRole() => _currentSelectedRole;
    public void SetCurrentSelectedRole(RoleData role) => _currentSelectedRole = role;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 注册场景加载完成事件，每次换场景后重新查找面板
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 场景加载完成后调用，修复ai聊天bug-角色数据为空
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindPanels();

        // 【关键修复】如果当前有选中的角色，且聊天面板存在，则自动恢复角色数据
        if (_currentSelectedRole != null)
        {
            // 重新设置角色详情面板的数据
            if (roleDetailPanel != null)
            {
                RoleDetailUI detail = roleDetailPanel.GetComponent<RoleDetailUI>();
                if (detail != null)
                    detail.SetRoleData(_currentSelectedRole);
            }

            // 重新设置聊天面板的数据（如果聊天面板当前是激活状态）
            if (chatPanel != null && chatPanel.activeSelf)
            {
                AIDialogController chat = chatPanel.GetComponent<AIDialogController>();
                if (chat != null)
                    chat.SetRoleData(_currentSelectedRole);
            }
        }
    }

    // 动态查找面板（根据你场景中的实际命名和层级）
    private void FindPanels()
    {
        // 找到 Canvas（所有 UI 面板的父物体）
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas == null)
        {
            Debug.LogError("UIManager: 场景中找不到 Canvas 物体！");
            return;
        }

        // 根据实际面板名称查找（请与 Hierarchy 中的名字完全一致）
        roleListPanel = canvas.transform.Find("CharacterPanel")?.gameObject;
        roleDetailPanel = canvas.transform.Find("CharacterDetail")?.gameObject;
        chatPanel = canvas.transform.Find("ChatPanel")?.gameObject;

        if (roleListPanel == null) Debug.LogError("UIManager: 找不到 CharacterPanel");
        if (roleDetailPanel == null) Debug.LogError("UIManager: 找不到 CharacterDetail");
        if (chatPanel == null) Debug.LogError("UIManager: 找不到 ChatPanel");
    }

    // 确保面板引用有效，否则重新查找
    private bool EnsurePanelsValid()
    {
        if (roleListPanel == null || roleDetailPanel == null || chatPanel == null)
        {
            FindPanels();
        }
        return (roleListPanel != null && roleDetailPanel != null && chatPanel != null);
    }

    // 打开角色列表
    public void ShowRoleList()
    {
        if (!EnsurePanelsValid()) return;
        roleListPanel.SetActive(true);
        roleDetailPanel.SetActive(false);
        chatPanel.SetActive(false);
    }

    // 打开详情页，并传入角色
    public void ShowRoleDetail(RoleData role)
    {
        if (!EnsurePanelsValid()) return;
        _currentSelectedRole = role;

        roleListPanel.SetActive(false);
        roleDetailPanel.SetActive(true);
        chatPanel.SetActive(false);

        RoleDetailUI detail = roleDetailPanel.GetComponent<RoleDetailUI>();
        if (detail != null)
            detail.SetRoleData(role);
        else
            Debug.LogError("UIManager: roleDetailPanel 上没有 RoleDetailUI 组件");
    }

    // 打开聊天界面
    public void ShowChatPanel(RoleData role)
    {
        if (role == null)
        {
            Debug.LogError("UIManager：传入聊天的角色为 null！");
            return;
        }
        if (!EnsurePanelsValid()) return;

        _currentSelectedRole = role;

        roleListPanel.SetActive(false);
        roleDetailPanel.SetActive(false);
        chatPanel.SetActive(true);

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