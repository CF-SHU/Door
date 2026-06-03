using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoleDetailUI : MonoBehaviour
{
    [Header("UI绑定")]
    public Image bigAvatarImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public Button chatButton;
    public Button returnButton;

    private RoleData _currentRole;

    void Awake()
    {
        if(chatButton != null)
        {
            chatButton.onClick.AddListener(OnChatClicked);
            Debug.Log("✅聊天按钮监听注册成功");
        }
        else
        {
            Debug.LogError("❌chatButton为空");
        }

        if(returnButton != null)
            returnButton.onClick.AddListener(OnReturnClicked);
    }
    // 
    // 外部设置角色数据
    public void SetRoleData(RoleData role)
    {
        _currentRole = role;
        if (_currentRole == null)
        {
            Debug.LogError("RoleDetailUI：传入角色为 null！");
            return;
        }

        nameText.text = role.charName;
        descText.text = role.personality + "\n" + role.background;
        bigAvatarImage.sprite = role.bigAvatar;

        Debug.Log("RoleDetailUI 收到角色：" + role.charName);
    }

    // 进入对话
    void OnChatClicked()
    {
        if (_currentRole == null)
        {
            Debug.LogError("RoleDetailUI：当前角色为空，无法进入聊天！");
            return;
        }

        UIManager.Instance.ShowChatPanel(_currentRole);
    }

    // 返回列表
    void OnReturnClicked()
    {
        UIManager.Instance.ShowRoleList();
    }
}