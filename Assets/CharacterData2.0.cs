/// <summary>
/// 角色数据（性格、背景，AI不OOC核心）
/// </summary>
[System.Serializable]
public class CharacterData2
{
    public string charId;      // 角色唯一ID
    public string charName;    // 角色名
    public string personality; // 性格（例如：高冷、傲娇、温柔、毒舌）
    public string background;  // 背景故事
}