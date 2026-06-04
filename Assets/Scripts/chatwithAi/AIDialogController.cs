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
    //新增：挂载你做好的中文字体资源
    public TMP_FontAsset chineseFont;
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
            //角色名称优先中文字体
            if (chineseFont != null)
                charNameText.font = chineseFont;
            else if (englishFont != null)
                charNameText.font = englishFont;
        }

        // 欢迎语改成中文
        if (CurrentChar != null)
            AddAIMessage($"你好，我是{CurrentChar.charName}");
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
        //【核心：强制角色只用中文说话】
        string systemPrompt = $@"
你将扮演{CurrentChar.charName}，**全程只使用中文说话，禁止任何英文**，绝不跳出人物设定。
人物性格：{CurrentChar.personality}
人物背景：{CurrentChar.background}

硬性规则：
1. 严格按照人设回答用户问题，不能跑题，全程只用简体中文。
2. 仅输出**单独一句日常口语中文**，不许分成多句话，不能出现英文字母、单词。
3. 语句通顺自然，无重复字词、无语病，贴合人物性格。
4. 不要多余解释、备注、标点以外的附加内容，只保留回答正文。
5. 严禁出现任何英文、数字缩写、外来词汇。

用户提问：{userMsg}
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
                AddAIMessage("网络连接出错了。");
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
            return "抱歉，我暂时无法回答这个问题。";
        }
    }

    //玩家消息气泡（靠右）
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
            //优先中文字体
            if (chineseFont != null)
                txt.font = chineseFont;
            else if (englishFont != null)
                txt.font = englishFont;
        }

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 0.5f);
        rt.anchorMax = new Vector2(1, 0.5f);
        rt.pivot = new Vector2(1, 0.5f);
        rt.anchoredPosition = Vector2.zero;

        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
        StartCoroutine(ScrollToBottom());
    }

    //AI消息气泡（靠左）
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
            //优先中文字体
            if (chineseFont != null)
                txt.font = chineseFont;
            else if (englishFont != null)
                txt.font = englishFont;
        }

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
    //【新增】中文字体，挂载你配置好的思源宋体FontAsset
    public TMP_FontAsset chineseFont;
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
            //角色名称优先中文
            if (chineseFont != null)
                charNameText.font = chineseFont;
            else if (englishFont != null)
                charNameText.font = englishFont;
        }

        // 角色有效时发送欢迎语（改为中文欢迎）
        if (CurrentChar != null)
            AddAIMessage($"你好，我是{CurrentChar.charName}");
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
        //【重点修改：Prompt全部改成中文约束，强制AI中文回复】
        string systemPrompt = $@"
你现在扮演角色：{CurrentChar.charName}
绝对不能跳出人设，不能脱离角色身份。

角色性格：{CurrentChar.personality}
角色背景：{CurrentChar.background}

必须严格遵守的硬性规则：
1. 完全看懂用户提问，紧扣问题直接回答，不跑题、不闲聊无关内容。
2. 禁止重复措辞、结巴、啰嗦冗长。
3. 仅输出**一句通顺自然的中文日常口语句子**，不要多句。
4. 无病句、无重复字词、没有相邻重复汉字（如不不、好好）。
5. 说话风格贴合你角色的性格设定，生活化日常对话。
6. 不额外解释、不附带多余说明，只输出回答正文。

用户提问：{userMsg}
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
                AddAIMessage("网络连接出错了。");
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
            return "抱歉，我暂时无法回答这个问题。";
        }
    }

    // ==============================
    // 玩家消息 → 最右边
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
            //【修改：优先中文字体】
            if (chineseFont != null)
                txt.font = chineseFont;
            else if (englishFont != null)
                txt.font = englishFont;
        }

        // 靠右对齐
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 0.5f);
        rt.anchorMax = new Vector2(1, 0.5f);
        rt.pivot = new Vector2(1, 0.5f);
        rt.anchoredPosition = Vector2.zero;

        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
        StartCoroutine(ScrollToBottom());
    }

    // ==============================
    // AI消息 → 最左边
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
            //【修改：优先中文字体】
            if (chineseFont != null)
                txt.font = chineseFont;
            else if (englishFont != null)
                txt.font = englishFont;
        }

        //靠左对齐
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
}*/