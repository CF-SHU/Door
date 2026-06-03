/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Text;
using TMPro;


public class AIDialogController : MonoBehaviour
{
    [Header("⚠️ 必须拖入的UI（TMP版本）")]
    public Transform content;
    public TMP_InputField inputField;
    public Button sendBtn;

    [Header("可选：角色名Text（TMP）")]
    public TextMeshProUGUI charNameText;

    [Header("气泡预设（不拖会自动创建）")]
    public GameObject bubblePlayer;
    public GameObject bubbleAI;

   // private CharacterData currentChar;
    private RoleData currentChar; // 改成这个

    // 中文字体
    public TMP_FontAsset chineseFont;
   
    public ScrollRect scrollRect;
    public Scrollbar verticalScrollbar;


    void Start()
    {
        // 给角色名设置字体
        if (charNameText != null && chineseFont != null)
        {
            charNameText.font = chineseFont;
        }

        // 1. 兜底：如果气泡为空，自动创建
        EnsureBubblesExist();

        // 2. 读取角色数据
        currentChar = new CharacterData()
        {
            charId = PlayerPrefs.GetString("CurrentCharId"),
            charName = PlayerPrefs.GetString("CurrentCharName"),
            personality = PlayerPrefs.GetString("CurrentCharPersonality"),
            background = PlayerPrefs.GetString("CurrentCharBackground")
        };

        // 3. 显示角色名（如果有）
        if (charNameText != null)
            charNameText.text = currentChar.charName;

        // 4. 绑定发送按钮
        if (sendBtn != null)
            sendBtn.onClick.AddListener(SendMessage);
        else
            Debug.LogError("❌ 请把SendBtn拖到脚本上！");

        // 5. 安全发送欢迎语
        if (content != null && bubbleAI != null)
            AddAIMessage("你好，我是 " + currentChar.charName);
    }

    // 兜底：确保气泡一定存在
    void EnsureBubblesExist()
    {
        if (bubblePlayer == null)
        {
            bubblePlayer = CreateBasicBubble(Color.white, TextAlignmentOptions.Right);
            Debug.Log("✅ 自动创建了Player气泡");
        }
        if (bubbleAI == null)
        {
            bubbleAI = CreateBasicBubble(Color.gray, TextAlignmentOptions.Left);
            Debug.Log("✅ 自动创建了AI气泡");
        }
    }

    // 创建气泡（自带中文字体）
    GameObject CreateBasicBubble(Color bgColor, TextAlignmentOptions align)
    {
        GameObject obj = new GameObject("BasicBubble");
        obj.AddComponent<RectTransform>();
        
        Image img = obj.AddComponent<Image>();
        img.color = bgColor;

        TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
        text.color = Color.black;
        text.alignment = align;
        text.enableAutoSizing = true;

        // 自动设置中文字体
        if (chineseFont != null)
        {
            text.font = chineseFont;
        }

        return obj;
    }

    public void SendMessage()
    {
        // 【修复】启动时不会自动调用空消息
        if (inputField == null || string.IsNullOrEmpty(inputField.text.Trim()))
        {
            return;
        }

        string msg = inputField.text.Trim();

        Debug.Log("📤 发送消息: " + msg);

        AddPlayerMessage(msg);
        inputField.text = "";
        RequestAIReply(msg);
    }

    void RequestAIReply(string userMsg)
    {
        string systemPrompt = $@"
你现在要扮演一个角色，严格按照设定回复，绝对不能OOC。

角色名称：{currentChar.charName}
性格特点：{currentChar.personality}
背景故事：{currentChar.background}

要求：
1. 只回复角色说的话，不要解释
2. 回复简短自然，符合文游对话
3. 严格贴合性格，不崩坏
4. 不要表情符号

用户说：{userMsg}
";

        StartCoroutine(RequestZhipuAI(systemPrompt));
    }

    // 硅基流动API（已修正模型名）
    IEnumerator RequestZhipuAI(string prompt)
    {
        string url = "https://api.siliconflow.cn/v1/chat/completions";
        string apiKey = "sk-ljjdlppjgidehmjhgadyhlwsygkmqluuivfpzaclybehsooc";
        
        // 【修复1】正确的免费模型名称（必须小写开头）
        string model = "qwen/Qwen2.5-7B-Instruct";

        ChatRequest requestData = new ChatRequest
        {
            model = model,
            messages = new List<ChatMessage>
            {
                new ChatMessage { role = "user", content = prompt }
            }
        };

        string json = JsonUtility.ToJson(requestData);
        byte[] postData = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(postData);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string reply = ParseReply(www.downloadHandler.text);
                AddAIMessage(reply);
            }
            else
            {
                AddAIMessage("（连接失败，请检查API Key是否正确）");
                Debug.LogError("错误：" + www.error + "\n返回内容：" + www.downloadHandler.text);
            }
        }
    }

    string ParseReply(string json)
    {
        try
        {
            int start = json.IndexOf("\"content\":\"") + 11;
            int end = json.IndexOf("\"", start);
            return json.Substring(start, end - start).Replace("\\n", "\n");
        }
        catch
        {
            Debug.LogWarning("JSON解析失败：" + json);
            return "（我暂时不知道说什么）";
        }
    }

    // 玩家消息（靠右显示）
    void AddPlayerMessage(string text)
    {
        if (bubblePlayer == null || content == null)
        {
            Debug.LogError("❌ AddPlayerMessage: bubblePlayer 或 content 为空！");
            return;
        }

        GameObject go = Instantiate(bubblePlayer, content);
        TextMeshProUGUI txt = go.GetComponentInChildren<TextMeshProUGUI>();
        
        if (txt != null)
        {
            txt.text = text;
            txt.alignment = TextAlignmentOptions.Right;
            if (chineseFont != null)
                txt.font = chineseFont;
        }

        RectTransform rt = go.GetComponent<RectTransform>();
        // 关键设置：锚点和轴心强制设为右侧中心
        rt.anchorMin = new Vector2(1, 0.5f);
        rt.anchorMax = new Vector2(1, 0.5f);
        rt.pivot = new Vector2(1, 0.5f);
        rt.anchoredPosition = Vector2.zero;

        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
                   if (scrollRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
            scrollRect.verticalNormalizedPosition = 0;
        }
        
    }

    // AI消息（靠左显示）
    void AddAIMessage(string text)
    {
        if (bubbleAI == null || content == null)
        {
            Debug.LogError("❌ AddAIMessage: bubbleAI 或 content 为空！");
            return;
        }

        GameObject go = Instantiate(bubbleAI, content);
        TextMeshProUGUI txt = go.GetComponentInChildren<TextMeshProUGUI>();
        
        if (txt != null)
        {
            txt.text = text;
            txt.alignment = TextAlignmentOptions.Left;
            if (chineseFont != null)
                txt.font = chineseFont;
        }

        RectTransform rt = go.GetComponent<RectTransform>();
        // 关键设置：锚点和轴心强制设为左侧中心
        rt.anchorMin = new Vector2(0, 0.5f);
        rt.anchorMax = new Vector2(0, 0.5f);
        rt.pivot = new Vector2(0, 0.5f);
        rt.anchoredPosition = Vector2.zero;

        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
              // 刷新布局并滚动到底部
        if (scrollRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
            scrollRect.verticalNormalizedPosition = 0;
        }
           
    } 
}


[Serializable]
public class ChatRequest
{
    public string model;
    public List<ChatMessage> messages;
}

[Serializable]
public class ChatMessage
{
    public string role;
    public string content;
}*/
/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Text;
using TMPro;

public class AIDialogController : MonoBehaviour
{
    [Header("⚠️ 必须拖入的UI（TMP版本）")]
    public Transform content;
    public TMP_InputField inputField;
    public Button sendBtn;

    [Header("可选：角色名Text（TMP）")]
    public TextMeshProUGUI charNameText;

    [Header("气泡预设（不拖会自动创建）")]
    public GameObject bubblePlayer;
    public GameObject bubbleAI;

    private RoleData currentChar;

    // 中文字体
    public TMP_FontAsset chineseFont;
    public ScrollRect scrollRect;
    public Scrollbar verticalScrollbar;

    [Header("返回按钮")]
    public Button returnButton;

    void Awake()
    {
        if (returnButton != null)
            returnButton.onClick.AddListener(() => UIManager.Instance.BackToDetail());
    }

    // 外部设置角色数据（关键！）
    public void SetRoleData(RoleData role)
    {
        currentChar = role;

        // 清空旧聊天
        ClearAllChat();

        // 设置角色名
        if (charNameText != null)
            charNameText.text = currentChar.charName;

        // 欢迎语
        AddAIMessage("Hi, I'm " + currentChar.charName);
    }

    void ClearAllChat()
    {
        foreach (Transform t in content)
            Destroy(t.gameObject);
    }

    void Start()
    {
        if (charNameText != null && chineseFont != null)
            charNameText.font = chineseFont;

        EnsureBubblesExist();

        if (sendBtn != null)
            sendBtn.onClick.AddListener(SendMessage);
    }

    void EnsureBubblesExist()
    {
        if (bubblePlayer == null)
        {
            bubblePlayer = CreateBasicBubble(Color.white, TextAlignmentOptions.Right);
        }
        if (bubbleAI == null)
        {
            bubbleAI = CreateBasicBubble(Color.gray, TextAlignmentOptions.Left);
        }
    }

    GameObject CreateBasicBubble(Color bgColor, TextAlignmentOptions align)
    {
        GameObject obj = new GameObject("BasicBubble");
        obj.AddComponent<RectTransform>();
        Image img = obj.AddComponent<Image>();
        img.color = bgColor;

        TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
        text.color = Color.black;
        text.alignment = align;
        text.enableAutoSizing = true;

        if (chineseFont != null) text.font = chineseFont;
        return obj;
    }

    public void SendMessage()
    {
        if (inputField == null || string.IsNullOrEmpty(inputField.text.Trim())) return;

        string msg = inputField.text.Trim();
        AddPlayerMessage(msg);
        inputField.text = "";
        RequestAIReply(msg);
    }

    void RequestAIReply(string userMsg)
    {
        if (currentChar == null) return;

        string systemPrompt = $@"
Your name is {currentChar.charName}.
Personality: {currentChar.personality}
Background: {currentChar.background}

Rules:
1. You must reply ONLY in ENGLISH.
2. Keep your responses short, natural, and conversational.
3. Do not use any Chinese characters or emojis.
4. Act like a real person chatting, not a robot.

User: {userMsg}
";

        StartCoroutine(RequestZhipuAI(systemPrompt));
    }

    IEnumerator RequestZhipuAI(string prompt)
    {
        string url = "https://api.siliconflow.cn/v1/chat/completions";
        string apiKey = "sk-ljjdlppjgidehmjhgadyhlwsygkmqluuivfpzaclybehsooc";
        string model = "qwen/Qwen2.5-7B-Instruct";

        ChatRequest requestData = new ChatRequest
        {
            model = model,
            messages = new List<ChatMessage>
            {
                new ChatMessage { role = "user", content = prompt }
            }
        };

        string json = JsonUtility.ToJson(requestData);
        byte[] postData = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(postData);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string reply = ParseReply(www.downloadHandler.text);
                AddAIMessage(reply);
            }
            else
            {
                AddAIMessage("Connection error.");
            }
        }
    }

    string ParseReply(string json)
    {
        try
        {
            int start = json.IndexOf("\"content\":\"") + 11;
            int end = json.IndexOf("\"", start);
            return json.Substring(start, end - start).Replace("\\n", "\n");
        }
        catch
        {
            return "Sorry, I can't answer that.";
        }
    }

    void AddPlayerMessage(string text)
    {
        if (bubblePlayer == null || content == null) return;
        GameObject go = Instantiate(bubblePlayer, content);
        TextMeshProUGUI txt = go.GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null)
        {
            txt.text = text;
            txt.alignment = TextAlignmentOptions.Right;
            if (chineseFont != null) txt.font = chineseFont;
        }

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 0.5f);
        rt.anchorMax = new Vector2(1, 0.5f);
        rt.pivot = new Vector2(1, 0.5f);
        rt.anchoredPosition = Vector2.zero;

        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
        if (scrollRect != null) scrollRect.verticalNormalizedPosition = 0;
    }

    void AddAIMessage(string text)
    {
        if (bubbleAI == null || content == null) return;
        GameObject go = Instantiate(bubbleAI, content);
        TextMeshProUGUI txt = go.GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null)
        {
            txt.text = text;
            txt.alignment = TextAlignmentOptions.Left;
            if (chineseFont != null) txt.font = chineseFont;
        }

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0.5f);
        rt.anchorMax = new Vector2(0, 0.5f);
        rt.pivot = new Vector2(0, 0.5f);
        rt.anchoredPosition = Vector2.zero;

        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
        Canvas.ForceUpdateCanvases();
        if (scrollRect != null) scrollRect.verticalNormalizedPosition = 0;
    }
}

[Serializable]
public class ChatRequest
{
    public string model;
    public List<ChatMessage> messages;
}

[Serializable]
public class ChatMessage
{
    public string role;
    public string content;
}*/
/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Text;
using TMPro;

public class AIDialogController : MonoBehaviour
{
    [Header("⚠️ 必须拖入的UI（TMP版本）")]
    public Transform content;
    public TMP_InputField inputField;
    public Button sendBtn;

    [Header("可选：角色名Text（TMP）")]
    public TextMeshProUGUI charNameText;

    [Header("气泡预设（不拖会自动创建）")]
    public GameObject bubblePlayer;
    public GameObject bubbleAI;

    private RoleData currentChar; // 已修复

    // 中文字体
    public TMP_FontAsset chineseFont;

    public ScrollRect scrollRect;
    public Scrollbar verticalScrollbar;

    void Start()
    {
        // 给角色名设置字体
        if (charNameText != null && chineseFont != null)
        {
            charNameText.font = chineseFont;
        }

        // 1. 兜底：如果气泡为空，自动创建
        EnsureBubblesExist();

        // 2. 读取角色数据（已修复，不再使用CharacterData）
        string charId = PlayerPrefs.GetString("CurrentCharId");
        string charName = PlayerPrefs.GetString("CurrentCharName");
        string personality = PlayerPrefs.GetString("CurrentCharPersonality");
        string background = PlayerPrefs.GetString("CurrentCharBackground");

        // 创建临时RoleData，保证你的逻辑完全不变
        currentChar = ScriptableObject.CreateInstance<RoleData>();
        currentChar.charName = charName;
        currentChar.personality = personality;
        currentChar.background = background;

        // 3. 显示角色名（如果有）
        if (charNameText != null)
            charNameText.text = currentChar.charName;

        // 4. 绑定发送按钮
        if (sendBtn != null)
            sendBtn.onClick.AddListener(SendMessage);
        else
            Debug.LogError("❌ 请把SendBtn拖到脚本上！");

        // 5. 安全发送欢迎语
        if (content != null && bubbleAI != null)
            AddAIMessage("你好，我是 " + currentChar.charName);
    }

    // 兜底：确保气泡一定存在
    void EnsureBubblesExist()
    {
        if (bubblePlayer == null)
        {
            bubblePlayer = CreateBasicBubble(Color.white, TextAlignmentOptions.Right);
            Debug.Log("✅ 自动创建了Player气泡");
        }
        if (bubbleAI == null)
        {
            bubbleAI = CreateBasicBubble(Color.gray, TextAlignmentOptions.Left);
            Debug.Log("✅ 自动创建了AI气泡");
        }
    }

    // 创建气泡（自带中文字体）
    GameObject CreateBasicBubble(Color bgColor, TextAlignmentOptions align)
    {
        GameObject obj = new GameObject("BasicBubble");
        obj.AddComponent<RectTransform>();

        Image img = obj.AddComponent<Image>();
        img.color = bgColor;

        TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
        text.color = Color.black;
        text.alignment = align;
        text.enableAutoSizing = true;

        // 自动设置中文字体
        if (chineseFont != null)
        {
            text.font = chineseFont;
        }

        return obj;
    }

    public void SendMessage()
    {
        // 【修复】启动时不会自动调用空消息
        if (inputField == null || string.IsNullOrEmpty(inputField.text.Trim()))
        {
            return;
        }

        string msg = inputField.text.Trim();

        Debug.Log("📤 发送消息: " + msg);

        AddPlayerMessage(msg);
        inputField.text = "";
        RequestAIReply(msg);
    }

    void RequestAIReply(string userMsg)
    {
        string systemPrompt = $@"
你现在要扮演一个角色，严格按照设定回复，绝对不能OOC。

角色名称：{currentChar.charName}
性格特点：{currentChar.personality}
背景故事：{currentChar.background}

要求：
1. 只回复角色说的话，不要解释
2. 回复简短自然，符合文游对话
3. 严格贴合性格，不崩坏
4. 不要表情符号

用户说：{userMsg}
";

        StartCoroutine(RequestZhipuAI(systemPrompt));
    }

    // 硅基流动API（已修正模型名）
    IEnumerator RequestZhipuAI(string prompt)
    {
        string url = "https://api.siliconflow.cn/v1/chat/completions";
        string apiKey = "sk-ljjdlppjgidehmjhgadyhlwsygkmqluuivfpzaclybehsooc";

        // 【修复1】正确的免费模型名称（必须小写开头）
        string model = "qwen/Qwen2.5-7B-Instruct";

        ChatRequest requestData = new ChatRequest
        {
            model = model,
            messages = new List<ChatMessage>
            {
                new ChatMessage { role = "user", content = prompt }
            }
        };

        string json = JsonUtility.ToJson(requestData);
        byte[] postData = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(postData);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string reply = ParseReply(www.downloadHandler.text);
                AddAIMessage(reply);
            }
            else
            {
                AddAIMessage("（连接失败，请检查API Key是否正确）");
                Debug.LogError("错误：" + www.error + "\n返回内容：" + www.downloadHandler.text);
            }
        }
    }

    string ParseReply(string json)
    {
        try
        {
            int start = json.IndexOf("\"content\":\"") + 11;
            int end = json.IndexOf("\"", start);
            return json.Substring(start, end - start).Replace("\\n", "\n");
        }
        catch
        {
            Debug.LogWarning("JSON解析失败：" + json);
            return "（我暂时不知道说什么）";
        }
    }

    // 玩家消息（靠右显示）
    void AddPlayerMessage(string text)
    {
        if (bubblePlayer == null || content == null)
        {
            Debug.LogError("❌ AddPlayerMessage: bubblePlayer 或 content 为空！");
            return;
        }

        GameObject go = Instantiate(bubblePlayer, content);
        TextMeshProUGUI txt = go.GetComponentInChildren<TextMeshProUGUI>();

        if (txt != null)
        {
            txt.text = text;
            txt.alignment = TextAlignmentOptions.Right;
            if (chineseFont != null)
                txt.font = chineseFont;
        }

        RectTransform rt = go.GetComponent<RectTransform>();
        // 关键设置：锚点和轴心强制设为右侧中心
        rt.anchorMin = new Vector2(1, 0.5f);
        rt.anchorMax = new Vector2(1, 0.5f);
        rt.pivot = new Vector2(1, 0.5f);
        rt.anchoredPosition = Vector2.zero;

        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
        if (scrollRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
            scrollRect.verticalNormalizedPosition = 0;
        }

    }

    // AI消息（靠左显示）
    void AddAIMessage(string text)
    {
        if (bubbleAI == null || content == null)
        {
            Debug.LogError("❌ AddAIMessage: bubbleAI 或 content 为空！");
            return;
        }

        GameObject go = Instantiate(bubbleAI, content);
        TextMeshProUGUI txt = go.GetComponentInChildren<TextMeshProUGUI>();

        if (txt != null)
        {
            txt.text = text;
            txt.alignment = TextAlignmentOptions.Left;
            if (chineseFont != null)
                txt.font = chineseFont;
        }

        RectTransform rt = go.GetComponent<RectTransform>();
        // 关键设置：锚点和轴心强制设为左侧中心
        rt.anchorMin = new Vector2(0, 0.5f);
        rt.anchorMax = new Vector2(0, 0.5f);
        rt.pivot = new Vector2(0, 0.5f);
        rt.anchoredPosition = Vector2.zero;

        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
        // 刷新布局并滚动到底部
        if (scrollRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
            scrollRect.verticalNormalizedPosition = 0;
        }

    }
}

[Serializable]
public class ChatRequest
{
    public string model;
    public List<ChatMessage> messages;
}

[Serializable]
public class ChatMessage
{
    public string role;
    public string content;
}*/
/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Text;
using TMPro;

public class AIDialogController : MonoBehaviour
{
    [Header("⚠️ 必须拖入的UI（TMP版本）")]
    public Transform content;
    public TMP_InputField inputField;
    public Button sendBtn;

    [Header("可选：角色名Text（TMP）")]
    public TextMeshProUGUI charNameText;

    [Header("气泡预设（不拖会自动创建）")]
    public GameObject bubblePlayer;
    public GameObject bubbleAI;

    private RoleData currentChar;

    // 统一用英文字体（LiberationSans SDF）
    public TMP_FontAsset englishFont;
    public ScrollRect scrollRect;

    [Header("返回按钮")]
    public Button returnButton;

    void Awake()
    {
        if (returnButton != null)
            returnButton.onClick.AddListener(() => UIManager.Instance.BackToDetail());
    }

    // 外部唯一入口：设置角色数据
    public void SetRoleData(RoleData role)
    {
        Debug.Log("==== SetRoleData 被调用：" + (role != null ? role.charName : "null"));
        currentChar = role;

        // 清空聊天记录
        ClearAllChat();

        // 更新角色名
        if (charNameText != null && currentChar != null)
        {
            charNameText.text = currentChar.charName;
            if (englishFont != null)
                charNameText.font = englishFont;
        }

        // 角色有效时发送欢迎语
        if (currentChar != null)
            AddAIMessage("Hi, I'm " + currentChar.charName);
    }

    // 清空所有气泡
    void ClearAllChat()
    {
        foreach (Transform t in content)
            Destroy(t.gameObject);
    }

    void Start()
    {
        EnsureBubblesExist();

        if (sendBtn != null)
            sendBtn.onClick.AddListener(SendMessage);
    }

    void EnsureBubblesExist()
    {
        if (bubblePlayer == null)
            bubblePlayer = CreateBasicBubble(Color.white, TextAlignmentOptions.Right);
        if (bubbleAI == null)
            bubbleAI = CreateBasicBubble(Color.gray, TextAlignmentOptions.Left);
    }

    // 创建基础气泡（兼容旧版Unity，只保留基础换行设置）
    GameObject CreateBasicBubble(Color bgColor, TextAlignmentOptions align)
    {
        GameObject obj = new GameObject("Bubble");
        obj.AddComponent<RectTransform>();
        Image img = obj.AddComponent<Image>();
        img.color = bgColor;

        TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
        text.color = Color.black;
        text.alignment = align;
        text.enableAutoSizing = true;
        text.enableWordWrapping = true; // 只保留这一行，强制换行

        if (englishFont != null)
            text.font = englishFont;

        return obj;
    }

    public void SendMessage()
    {
        // 1. 输入框为空直接返回
        if (inputField == null || string.IsNullOrEmpty(inputField.text.Trim()))
        {
            Debug.LogError("输入框为空，无法发送！");
            return;
        }

        // 2. 双重判断：必须有角色数据才发送
        if (currentChar == null)
        {
            Debug.LogError("角色数据为空！请重新进入对话界面。");
            AddAIMessage("Error: Please re-enter the chat interface.");
            return;
        }

        string msg = inputField.text.Trim();
        AddPlayerMessage(msg);
        inputField.text = "";
        RequestAIReply(msg);
    }

    void RequestAIReply(string userMsg)
    {
        // 发送请求前再次判断
        if (currentChar == null)
        {
            Debug.LogError("发送AI请求时，角色数据为空！");
            return;
        }

        string systemPrompt = $@"
Your name is {currentChar.charName}.
Personality: {currentChar.personality}
Background: {currentChar.background}

Rules:
1. You must reply ONLY in ENGLISH.
2. Keep responses short, natural, and conversational.
3. No Chinese characters or emojis.
4. Act like a real person chatting.

User: {userMsg}
";

        StartCoroutine(RequestZhipuAI(systemPrompt));
    }

    IEnumerator RequestZhipuAI(string prompt)
    {
        string url = "https://api.siliconflow.cn/v1/chat/completions";
        string apiKey = "sk-ljjdlppjgidehmjhgadyhlwsygkmqluuivfpzaclybehsooc";
        string model = "qwen/Qwen2.5-7B-Instruct";

        ChatRequest requestData = new ChatRequest
        {
            model = model,
            messages = new List<ChatMessage>
            {
                new ChatMessage { role = "user", content = prompt }
            }
        };

        string json = JsonUtility.ToJson(requestData);
        byte[] postData = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(postData);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string reply = ParseReply(www.downloadHandler.text);
                AddAIMessage(reply);
            }
            else
            {
                AddAIMessage("Connection error.");
                Debug.LogError("API错误：" + www.error);
            }
        }
    }

    // 解析JSON：清理所有转义字符，解决换行乱码
    string ParseReply(string json)
    {
        try
        {
            int start = json.IndexOf("\"content\":\"") + 11;
            int end = json.IndexOf("\"", start);
            string reply = json.Substring(start, end - start);
            reply = reply.Replace("\\n", "\n").Replace("\\r", "").Replace("\\\"", "\"");
            return reply;
        }
        catch
        {
            Debug.LogError("解析JSON失败：" + json);
            return "Sorry, I can't answer that.";
        }
    }

    // 添加玩家消息
    void AddPlayerMessage(string text)
    {
        if (bubblePlayer == null || content == null) return;
        GameObject go = Instantiate(bubblePlayer, content);
        TextMeshProUGUI txt = go.GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null)
        {
            txt.text = text;
            txt.alignment = TextAlignmentOptions.Right;
            txt.enableWordWrapping = true;
            if (englishFont != null) txt.font = englishFont;
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
        StartCoroutine(ScrollToBottom());
    }

    // 添加AI消息
    void AddAIMessage(string text)
    {
        if (bubbleAI == null || content == null) return;
        GameObject go = Instantiate(bubbleAI, content);
        TextMeshProUGUI txt = go.GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null)
        {
            txt.text = text;
            txt.alignment = TextAlignmentOptions.Left;
            txt.enableWordWrapping = true;
            if (englishFont != null) txt.font = englishFont;
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
        StartCoroutine(ScrollToBottom());
    }

    // 滚到底部
    IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 0f;
    }
}

[Serializable]
public class ChatRequest
{
    public string model;
    public List<ChatMessage> messages;
}

[Serializable]
public class ChatMessage
{
    public string role;
    public string content;
}*/
/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Text;
using TMPro;

public class AIDialogController : MonoBehaviour
{
    [Header("必须拖入的UI")]
    public Transform content;
    public TMP_InputField inputField;
    public Button sendBtn;
    public Button returnButton;

    [Header("角色名和气泡")]
    public TextMeshProUGUI charNameText;
    public GameObject bubblePlayer;
    public GameObject bubbleAI;
    public TMP_FontAsset englishFont;
    public ScrollRect scrollRect;

    // 角色数据：只在SetRoleData里赋值，只读保护
    private RoleData _currentChar;
    public RoleData CurrentChar
    {
        get { return _currentChar; }
        private set { _currentChar = value; }
    }

    void Awake()
    {
        if (returnButton != null)
            returnButton.onClick.AddListener(() => UIManager.Instance.BackToDetail());
    }

    // 外部唯一入口：设置角色数据
    public void SetRoleData(RoleData role)
    {
        Debug.Log("==== SetRoleData 被调用：" + (role != null ? role.charName : "null"));
        CurrentChar = role;

        // 清空聊天记录
        ClearAllChat();

        // 更新角色名
        if (charNameText != null && CurrentChar != null)
        {
            charNameText.text = CurrentChar.charName;
            if (englishFont != null)
                charNameText.font = englishFont;
        }

        // 角色有效时发送欢迎语
        if (CurrentChar != null)
            AddAIMessage("Hi, I'm " + CurrentChar.charName);
    }

    // 清空所有气泡
    void ClearAllChat()
    {
        foreach (Transform t in content)
            Destroy(t.gameObject);
    }

    void Start()
    {
        if (sendBtn != null)
            sendBtn.onClick.AddListener(SendMessage);
    }

    public void SendMessage()
    {
        // 1. 输入框为空直接返回
        if (inputField == null || string.IsNullOrEmpty(inputField.text.Trim()))
        {
            Debug.LogError("输入框为空，无法发送！");
            return;
        }

        // 2. 关键：如果角色数据为空，只打日志，不添加错误气泡
        if (CurrentChar == null)
        {
            Debug.LogError("角色数据为空！请重新进入对话界面。");
            return;
        }

        string msg = inputField.text.Trim();
        AddPlayerMessage(msg);
        inputField.text = "";
        RequestAIReply(msg);
    }

    void RequestAIReply(string userMsg)
    {
        if (CurrentChar == null)
        {
            Debug.LogError("发送AI请求时，角色数据为空！");
            return;
        }

        string systemPrompt = $@"
Your name is {CurrentChar.charName}.
Personality: {CurrentChar.personality}
Background: {CurrentChar.background}

Rules:
1. You must reply ONLY in ENGLISH.
2. Keep responses short, natural, and conversational.
3. No Chinese characters or emojis.
4. Act like a real person chatting.

User: {userMsg}
";

        StartCoroutine(RequestZhipuAI(systemPrompt));
    }

    IEnumerator RequestZhipuAI(string prompt)
    {
        string url = "https://api.siliconflow.cn/v1/chat/completions";
        string apiKey = "sk-ljjdlppjgidehmjhgadyhlwsygkmqluuivfpzaclybehsooc";
        string model = "qwen/Qwen2.5-7B-Instruct";

        ChatRequest requestData = new ChatRequest
        {
            model = model,
            messages = new List<ChatMessage>
            {
                new ChatMessage { role = "user", content = prompt }
            }
        };

        string json = JsonUtility.ToJson(requestData);
        byte[] postData = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(postData);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string reply = ParseReply(www.downloadHandler.text);
                AddAIMessage(reply);
            }
            else
            {
                AddAIMessage("Connection error.");
                Debug.LogError("API错误：" + www.error);
            }
        }
    }

    // 解析JSON：清理转义字符
    string ParseReply(string json)
    {
        try
        {
            int start = json.IndexOf("\"content\":\"") + 11;
            int end = json.IndexOf("\"", start);
            string reply = json.Substring(start, end - start);
            reply = reply.Replace("\\n", "\n").Replace("\\r", "").Replace("\\\"", "\"");
            return reply;
        }
        catch
        {
            Debug.LogError("解析JSON失败：" + json);
            return "Sorry, I can't answer that.";
        }
    }

    // 添加玩家消息
    void AddPlayerMessage(string text)
    {
        if (bubblePlayer == null || content == null)
        {
            Debug.LogError("bubblePlayer 未拖入！");
            return;
        }
        GameObject go = Instantiate(bubblePlayer, content);
        TextMeshProUGUI txt = go.GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null)
        {
            txt.text = text;
            txt.enableWordWrapping = true;
            if (englishFont != null) txt.font = englishFont;
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
        StartCoroutine(ScrollToBottom());
    }

    // 添加AI消息
    void AddAIMessage(string text)
    {
        if (bubbleAI == null || content == null)
        {
            Debug.LogError("bubbleAI 未拖入！");
            return;
        }
        GameObject go = Instantiate(bubbleAI, content);
        TextMeshProUGUI txt = go.GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null)
        {
            txt.text = text;
            txt.enableWordWrapping = true;
            if (englishFont != null) txt.font = englishFont;
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
        StartCoroutine(ScrollToBottom());
    }

    // 滚到底部
    IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 0f;
    }
}

[Serializable]
public class ChatRequest
{
    public string model;
    public List<ChatMessage> messages;
}

[Serializable]
public class ChatMessage
{
    public string role;
    public string content;
}*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Text;
using TMPro;

public class AIDialogController : MonoBehaviour
{
    [Header("必须拖入的UI")]
    public Transform content;
    public TMP_InputField inputField;
    public Button sendBtn;
    public Button returnButton;

    [Header("角色名和气泡")]
    public TextMeshProUGUI charNameText;
    public GameObject bubblePlayer;
    public GameObject bubbleAI;
    public TMP_FontAsset englishFont;
    public ScrollRect scrollRect;

    // 角色数据：只在SetRoleData里赋值，只读保护
    private RoleData _currentChar;
    public RoleData CurrentChar
    {
        get { return _currentChar; }
        private set { _currentChar = value; }
    }

    void Awake()
    {
        if (returnButton != null)
            returnButton.onClick.AddListener(() => UIManager.Instance.BackToDetail());
    }

    // 外部唯一入口：设置角色数据
    public void SetRoleData(RoleData role)
    {
        Debug.Log("==== SetRoleData 被调用：" + (role != null ? role.charName : "null"));
        CurrentChar = role;

        // 清空聊天记录
        ClearAllChat();

        // 更新角色名
        if (charNameText != null && CurrentChar != null)
        {
            charNameText.text = CurrentChar.charName;
            if (englishFont != null)
                charNameText.font = englishFont;
        }

        // 角色有效时发送欢迎语
        if (CurrentChar != null)
            AddAIMessage("Hi, I'm " + CurrentChar.charName);
    }

    // 清空所有气泡
    void ClearAllChat()
    {
        foreach (Transform t in content)
            Destroy(t.gameObject);
    }

    void Start()
    {
        if (sendBtn != null)
            sendBtn.onClick.AddListener(SendMessage);
    }

    public void SendMessage()
    {
        // 1. 输入框为空直接返回
        if (inputField == null || string.IsNullOrEmpty(inputField.text.Trim()))
        {
            Debug.LogError("输入框为空，无法发送！");
            return;
        }

        // 2. 关键：如果角色数据为空，只打日志，不添加错误气泡
        if (CurrentChar == null)
        {
            Debug.LogError("角色数据为空！请重新进入对话界面。");
            return;
        }

        string msg = inputField.text.Trim();
        AddPlayerMessage(msg);
        inputField.text = "";
        RequestAIReply(msg);
    }

    void RequestAIReply(string userMsg)
    {
        if (CurrentChar == null)
        {
            Debug.LogError("发送AI请求时，角色数据为空！");
            return;
        }

        string systemPrompt = $@"
You are role-playing as {CurrentChar.charName}.
Do NOT break character. Do NOT ignore the user's question.


Personality: {CurrentChar.personality}
Background: {CurrentChar.background}


IMMUTABLE RULES TO FOLLOW 100%:
1. Fully understand user's question, answer directly to his words, never off-topic.
2. Do NOT repeat words, stutter, or ramble.
3. Output ONLY ONE grammatically perfect, natural English sentence.
4. No repeated words, no broken grammar, no messy wording, no stuttering.
5. Keep the sentence casual for daily chatting, match your character's personality.
6. no extra explanation, no redundant content.
7. Always speak with correct English grammar in casual daily sentences
8. FORBID repeating the same English word consecutively in your sentence, no duplicated adjacent words like 'do do', 'how how'.

User: {userMsg}
";

        StartCoroutine(RequestZhipuAI(systemPrompt));
    }

    IEnumerator RequestZhipuAI(string prompt)
    {
        string url = "https://api.siliconflow.cn/v1/chat/completions";
        string apiKey = "sk-ljjdlppjgidehmjhgadyhlwsygkmqluuivfpzaclybehsooc";
        string model = "qwen/Qwen2.5-7B-Instruct";

        ChatRequest requestData = new ChatRequest
        {
            model = model,
            messages = new List<ChatMessage>
            {
                new ChatMessage { role = "user", content = prompt }
            }
        };

        string json = JsonUtility.ToJson(requestData);
        byte[] postData = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(postData);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string reply = ParseReply(www.downloadHandler.text);
                AddAIMessage(reply);
            }
            else
            {
                AddAIMessage("Connection error.");
                Debug.LogError("API错误：" + www.error);
            }
        }
    }

    // 解析JSON：清理转义字符
    string ParseReply(string json)
    {
        try
        {
            int start = json.IndexOf("\"content\":\"") + 11;
            int end = json.IndexOf("\"", start);
            string reply = json.Substring(start, end - start);
            reply = reply.Replace("\\n", "\n").Replace("\\r", "").Replace("\\\"", "\"");
            return reply;
        }
        catch
        {
            Debug.LogError("解析JSON失败：" + json);
            return "Sorry, I can't answer that.";
        }
    }

    // ==============================
    // 【修正】你的消息 → 最右边
    // ==============================
    void AddPlayerMessage(string text)
    {
        if (bubblePlayer == null || content == null)
        {
            Debug.LogError("bubblePlayer 未拖入！");
            return;
        }
        GameObject go = Instantiate(bubblePlayer, content);
        TextMeshProUGUI txt = go.GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null)
        {
            txt.text = text;
            txt.enableWordWrapping = true;
            txt.alignment = TextAlignmentOptions.Right;
            if (englishFont != null) txt.font = englishFont;
        }

        // 【关键修正】靠右对齐
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 0.5f);
        rt.anchorMax = new Vector2(1, 0.5f);
        rt.pivot = new Vector2(1, 0.5f);
        rt.anchoredPosition = Vector2.zero;

        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
        StartCoroutine(ScrollToBottom());
    }

    // ==============================
    // 【修正】AI消息 → 最左边
    // ==============================
    void AddAIMessage(string text)
    {
        if (bubbleAI == null || content == null)
        {
            Debug.LogError("bubbleAI 未拖入！");
            return;
        }
        GameObject go = Instantiate(bubbleAI, content);
        TextMeshProUGUI txt = go.GetComponentInChildren<TextMeshProUGUI>();
        if (txt != null)
        {
            txt.text = text;
            txt.enableWordWrapping = true;
            txt.alignment = TextAlignmentOptions.Left;
            if (englishFont != null) txt.font = englishFont;
        }

        // 【关键修正】靠左对齐
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0.5f);
        rt.anchorMax = new Vector2(0, 0.5f);
        rt.pivot = new Vector2(0, 0.5f);
        rt.anchoredPosition = Vector2.zero;

        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
        StartCoroutine(ScrollToBottom());
    }

    // 滚到底部
    IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 0f;
    }
}

[Serializable]
public class ChatRequest
{
    public string model;
    public List<ChatMessage> messages;
}

[Serializable]
public class ChatMessage
{
    public string role;
    public string content;
}