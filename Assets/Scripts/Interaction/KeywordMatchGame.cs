using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro; // 如果用 TextMeshPro 请保留，否则删除并改用普通 Text
using UnityEngine.Events;

/// <summary>
/// 关键词拼贴小游戏：将打乱的词条按正确顺序组成一条或多条规则/句子
/// </summary>
public class KeywordMatchGame : MonoBehaviour
{
    [System.Serializable]
    public class Rule
    {
        [TextArea] public string ruleDescription;  // 规则描述（例如“上课必须：”）
        public string[] correctPhrases;            // 正确顺序的词条（如["抬头","看黑板"]）
    }

    [Header("规则配置")]
    public List<Rule> rules;                      // 所有需要完成的规则（按顺序进行）

    [Header("UI 组件")]
    public GameObject gamePanel;                  // 整个小游戏面板
    public TextMeshProUGUI ruleText;              // 显示当前规则文本
    public Transform phrasesContainer;             // 打乱词条按钮的父物体
    public Transform answerContainer;              // 当前答案区域的父物体
    public Button resetButton;                     // 重置按钮
    public Button submitButton;                    // 提交按钮
    public Button exitButton;                      // 退出按钮
    public TextMeshProUGUI messageText;            // 提示信息（错误/正确）

    [Header("事件回调 (供 Fungus 调用)")]
    public UnityEvent onGameWin;    // 胜利时触发（可执行 Flowchart 的某个 Block）
    public UnityEvent onGameLose;   // 失败时触发（可选）
    public UnityEvent onGameExit;   // 退出时触发

    // 内部状态
    private int currentRuleIndex = 0;
    private List<string> currentAvailablePhrases;   // 当前未被选的词条
    private List<string> currentAnswer;             // 当前已选答案（按顺序）

    private void Awake()
    {
        // 初始隐藏面板，等 Fungus 调用 OpenGame
        gamePanel.SetActive(false);
        if (resetButton) resetButton.onClick.AddListener(ResetCurrentRule);
        if (submitButton) submitButton.onClick.AddListener(Submit);
        if (exitButton) exitButton.onClick.AddListener(ExitGame);
    }

    /// <summary>Fungus 调用此方法打开小游戏</summary>
    public void OpenGame()
    {
        Debug.Log("KeywordMatchGame: OpenGame called");
        currentRuleIndex = 0;
        gamePanel.SetActive(true);
        Debug.Log($"gamePanel active: {gamePanel.activeSelf}");
        LoadRule(currentRuleIndex);
    }

    /// <summary>关闭面板（退出小游戏）</summary>
    public void ExitGame()
    {
        gamePanel.SetActive(false);
        onGameExit?.Invoke();
    }

    /// <summary>加载指定索引的规则</summary>
    private void LoadRule(int index)
    {
        if (rules == null || rules.Count == 0)
        {
            Debug.LogError("KeywordMatchGame: rules list is empty or not assigned.");
            return;
        }

        if (index < 0 || index >= rules.Count)
        {
            // 所有规则完成 → 胜利
            WinGame();
            return;
        }

        Rule rule = rules[index];
        if (rule == null)
        {
            Debug.LogError($"KeywordMatchGame: rule at index {index} is null.");
            return;
        }

        if (ruleText == null)
        {
            Debug.LogError("KeywordMatchGame: ruleText is not assigned.");
            return;
        }

        ruleText.text = rule.ruleDescription;

        // 初始化打乱词条列表（随机顺序）
        currentAvailablePhrases = new List<string>(rule.correctPhrases);
        Shuffle(currentAvailablePhrases);
        currentAnswer = new List<string>();

        // 刷新 UI
        RefreshPhrasesButtons();
        RefreshAnswerButtons();

        if (messageText != null)
        {
            messageText.text = "";
        }
    }

    /// <summary>刷新打乱词条区域的按钮</summary>
    private void RefreshPhrasesButtons()
    {
        if (phrasesContainer == null)
        {
            Debug.LogError("KeywordMatchGame: phrasesContainer is not assigned.");
            return;
        }

        if (currentAvailablePhrases == null)
        {
            Debug.LogError("KeywordMatchGame: currentAvailablePhrases is null.");
            return;
        }

        // 清除旧按钮
        foreach (Transform child in phrasesContainer) Destroy(child.gameObject);

        // 为每个可用词条生成按钮
        foreach (string phrase in currentAvailablePhrases)
        {
            Button btn = CreateGameButton("PhraseBtn", phrasesContainer, phrase);
            if (btn == null) continue;
            btn.onClick.AddListener(() => OnPhraseClicked(phrase));
        }
    }

    /// <summary>刷新答案区域的按钮</summary>
    private void RefreshAnswerButtons()
    {
        if (answerContainer == null)
        {
            Debug.LogError("KeywordMatchGame: answerContainer is not assigned.");
            return;
        }

        if (currentAnswer == null)
        {
            Debug.LogError("KeywordMatchGame: currentAnswer is null.");
            return;
        }

        foreach (Transform child in answerContainer) Destroy(child.gameObject);

        for (int i = 0; i < currentAnswer.Count; i++)
        {
            string phrase = currentAnswer[i];
            Button btn = CreateGameButton("AnswerBtn", answerContainer, phrase);
            if (btn == null) continue;
            int idx = i; // 捕获索引
            btn.onClick.AddListener(() => OnAnswerClicked(idx));
        }
    }

    /// <summary>点击词条：添加到答案末尾</summary>
    private void OnPhraseClicked(string phrase)
    {
        currentAvailablePhrases.Remove(phrase);
        currentAnswer.Add(phrase);
        RefreshPhrasesButtons();
        RefreshAnswerButtons();
    }

    private Button CreateGameButton(string name, Transform parent, string labelText)
    {
        GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(Button), typeof(Image));
        btnObj.transform.SetParent(parent, false);

        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(180, 40);

        Image btnImage = btnObj.GetComponent<Image>();
        btnImage.color = new Color(0.95f, 0.95f, 0.95f, 1f);
        btnImage.type = Image.Type.Sliced;

        Button btn = btnObj.GetComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.95f, 0.95f, 0.95f, 1f);
        colors.highlightedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        colors.selectedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        colors.disabledColor = new Color(0.7f, 0.7f, 0.7f, 0.5f);
        colors.colorMultiplier = 1f;
        btn.colors = colors;
        btn.transition = Selectable.Transition.ColorTint;
        btn.targetGraphic = btnImage;

        GameObject labelObj = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        labelObj.transform.SetParent(btnObj.transform, false);
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        Text text = labelObj.GetComponent<Text>();
        if (text == null)
        {
            Debug.LogError("KeywordMatchGame: failed to add Text component to " + name + ".");
            return null;
        }

        text.text = labelText;
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 20;
        text.color = Color.black;
        text.raycastTarget = false;

        return btn;
    }

    /// <summary>点击答案中的词条：移除并放回可用列表</summary>
    private void OnAnswerClicked(int index)
    {
        string phrase = currentAnswer[index];
        currentAnswer.RemoveAt(index);
        currentAvailablePhrases.Add(phrase);
        RefreshPhrasesButtons();
        RefreshAnswerButtons();
    }

    /// <summary>重置当前规则（清空答案，恢复初始打乱词条）</summary>
    private void ResetCurrentRule()
    {
        if (currentRuleIndex >= rules.Count) return;
        Rule rule = rules[currentRuleIndex];
        currentAvailablePhrases = new List<string>(rule.correctPhrases);
        Shuffle(currentAvailablePhrases);
        currentAnswer = new List<string>();
        RefreshPhrasesButtons();
        RefreshAnswerButtons();
        messageText.text = "";
    }

    /// <summary>提交答案，检查是否匹配正确顺序</summary>
    private void Submit()
    {
        if (currentRuleIndex >= rules.Count) return;
        Rule rule = rules[currentRuleIndex];

        // 检查词条数量和顺序
        if (currentAnswer.Count != rule.correctPhrases.Length)
        {
            ShowMessage("数量不对，请把所有词条按正确顺序排列！", false);
            return;
        }

        for (int i = 0; i < currentAnswer.Count; i++)
        {
            if (currentAnswer[i] != rule.correctPhrases[i])
            {
                ShowMessage("顺序错误，请重新排列！", false);
                return;
            }
        }

        // 正确 → 进入下一条规则
        ShowMessage("正确！进入下一规则。", true);
        currentRuleIndex++;
        LoadRule(currentRuleIndex);
    }

    private void WinGame()
    {
        gamePanel.SetActive(false);
        messageText.text = "恭喜完成所有规则！";
        onGameWin?.Invoke();
    }

    private void ShowMessage(string msg, bool isGood)
    {
        messageText.text = msg;
        messageText.color = isGood ? Color.green : Color.red;
        // 可选：2秒后自动清除提示
        CancelInvoke("ClearMessage");
        Invoke("ClearMessage", 2f);
    }

    private void ClearMessage()
    {
        messageText.text = "";
    }

    /// <summary>Fisher-Yates 洗牌算法</summary>
    private void Shuffle<T>(IList<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }
}