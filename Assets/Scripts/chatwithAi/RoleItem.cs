/*using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoleItem : MonoBehaviour
{
    [Header("UI Bind")]
    public Image avatarImage;
    public TextMeshProUGUI nameText;

    public RoleData roleData;

    void Start()
    {
        RefreshUI();
    }

    void RefreshUI()
    {
        if (roleData == null) return;
        avatarImage.sprite = roleData.smallAvatar;
        // 这里改成charName
        nameText.text = roleData.charName;
    }

    public void OnClickRole()
    {
        if (roleData == null) return;*/

        // 保存角色信息
       /* PlayerPrefs.SetString("CurrentCharId", roleData.name);
        PlayerPrefs.SetString("CurrentCharName", roleData.charName);
        PlayerPrefs.SetString("CurrentCharPersonality", roleData.personality);
        PlayerPrefs.SetString("CurrentCharBackground", roleData.background);
        PlayerPrefs.Save();*/
         // 关键：这里必须用当前roleData的数据覆盖写入
    /*    PlayerPrefs.SetString("CurrentCharName", roleData.charName);
        PlayerPrefs.SetString("CurrentCharPersonality", roleData.personality);
        PlayerPrefs.SetString("CurrentCharBackground", roleData.background);
        PlayerPrefs.Save();

        Debug.Log("已切换角色：" + roleData.charName);

        // 打开详情界面
        UIManager.Instance.ShowRoleDetail();
    }
}*/
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoleItem : MonoBehaviour
{
    [Header("UI绑定")]
    public Image avatarImage;
    public TextMeshProUGUI nameText;

    [Header("角色配置文件")]
    public RoleData roleData;

    void Start()
    {
        RefreshUI();
    }

    void RefreshUI()
    {
        if (roleData == null) return;
        avatarImage.sprite = roleData.smallAvatar;
        nameText.text = roleData.charName;
    }

    // 点击卡片 → 打开详情
    public void OnClickRole()
    {
        if (roleData == null) return;
        UIManager.Instance.ShowRoleDetail(roleData);
    }
}