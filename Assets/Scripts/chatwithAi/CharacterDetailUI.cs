using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterDetailUI : MonoBehaviour
{
    // 这里设置当前查看的角色人设（你可以从存档/配置读取）
    public CharacterData2 currentCharacter;

    void Start()
    {
        // 示例：给当前角色赋值（你项目里从数据里读）
        currentCharacter = new CharacterData2()
        {
            charId = "1001",
            charName = "林晚星",
            personality = "高冷、话少、内心温柔、不擅长表达",
            background = "孤独的天才少女，喜欢星空，不喜欢热闹"
        };
    }

    /// <summary>
    /// 点击AI对话按钮调用
    /// </summary>
    public void OnClickAIDialog()
    {
        // 把角色数据传给对话场景
        PlayerPrefs.SetString("CurrentCharId", currentCharacter.charId);
        PlayerPrefs.SetString("CurrentCharName", currentCharacter.charName);
        PlayerPrefs.SetString("CurrentCharPersonality", currentCharacter.personality);
        PlayerPrefs.SetString("CurrentCharBackground", currentCharacter.background);
        
        // 跳转到AI对话场景
        SceneManager.LoadScene("AIDialog");
    }
}