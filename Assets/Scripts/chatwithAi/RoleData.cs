/*using UnityEngine;

[CreateAssetMenu(fileName = "NewRole", menuName = "RoleConfig/RoleData")]
public class RoleData : ScriptableObject
{
    [Header("Basic Info")]
    public string roleName;
    public Sprite smallAvatar;
    public Sprite bigAvatar;

    [TextArea(2, 6)]
    public string roleDescription;

    [Header("AI Prompt")]
    [TextArea(4, 12)]
    public string aiSystemPrompt;
}*/
/*
using UnityEngine;

// 右键菜单：快速创建角色配置文件
[CreateAssetMenu(fileName = "角色_", menuName = "角色配置/角色数据")]
public class RoleData : ScriptableObject
{
    [Header("角色基础信息")]
    public string roleName;         // 角色名称
    public Sprite smallAvatar;      // 主界面小头像
    public Sprite bigAvatar;        // 详情页大图
    [TextArea(2,6)]
    public string roleDescription;  // 角色简介

    [Header("AI对话人设（决定性格/语气）")]
    [TextArea(4,12)]
    public string aiSystemPrompt;   // AI角色设定
}*/
/*using UnityEngine;

[CreateAssetMenu(fileName = "NewRole", menuName = "RoleConfig/RoleData")]
public class RoleData : ScriptableObject
{
    [Header("Basic Info")]
    public string charName;  // 改成和你脚本里一致的charName
    public Sprite smallAvatar;
    public Sprite bigAvatar;

    [TextArea(2, 6)]
    public string roleDescription;

    [Header("AI Settings")]
    [TextArea(4, 12)]
    public string personality;
    [TextArea(4, 12)]
    public string background;
}*/
using UnityEngine;

[CreateAssetMenu(fileName = "NewRole", menuName = "RoleConfig/RoleData")]
public class RoleData : ScriptableObject
{
    [Header("Basic Info")]
    public string charName;
    public Sprite smallAvatar;
    public Sprite bigAvatar;

    [TextArea(2,6)]
    public string roleDescription;

    [Header("AI Settings")]
    [TextArea(4,12)]
    public string personality;
    [TextArea(4,12)]
    public string background;
}