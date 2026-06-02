// CharacterData.cs
using UnityEngine;

[System.Serializable]
public class CharacterData
{
    public string characterID;          // 角色ID
    public string characterName;        // 角色名称
    public Sprite characterPortrait;    // 角色立绘/头像
    public Sprite fullBodyImage;        // 全身像
    public string description;          // 角色描述
    public string[] relationshipQuotes;  // 关系语录

    // 好感度等数值
    public int affectionLevel;
}