// EndingManager.cs
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Fungus;

public class EndingManager : MonoBehaviour
{
    [Header("Ending UI")]
    public GameObject endingPanelPrefab;
    public GameObject endingPanel;
    public CanvasGroup endingCanvasGroup;
    public TextMeshProUGUI endingTitle;
    public TextMeshProUGUI endingDesc;
    public TextMeshProUGUI endingHint;
    public Button restartButton;
    public Button menuButton;
    public Button quitButton;

    [Header("Fade Settings")]
    public float fadeDuration = 0.4f;

    [Header("Fungus Variables")]
    public Flowchart flowchart;
    public string endingIDVariable = "endingID";
    public string titleVariable = "title";
    public string descVariable = "desc";

    [Header("New Game Settings")]
    public int newGameSceneIndex = 0;
    public bool useAutoSaveNamedEnding = true;

    private void Awake()
    {
        if (endingPanel == null && endingPanelPrefab != null)
        {
            endingPanel = Instantiate(endingPanelPrefab, transform);
        }

        if (endingPanel != null)
        {
            endingPanel.SetActive(false);
            if (endingCanvasGroup == null)
                endingCanvasGroup = endingPanel.GetComponent<CanvasGroup>();

            if (endingCanvasGroup != null)
                endingCanvasGroup.alpha = 0f;
        }
        else if (endingPanelPrefab == null)
        {
            Debug.LogWarning("[EndingManager] 请在 Inspector 中分配 endingPanelPrefab 或 endingPanel。结局面板未配置。", this);
        }
    }

    // 触发结局
    public void TriggerEnding(string endingID, string title, string desc)
    {
        if (SaveManager.CurrentData == null)
            SaveManager.Initialize();

        if (!SaveManager.CurrentData.triggeredFlags.Contains(endingID))
        {
            SaveManager.CurrentData.triggeredFlags.Add(endingID);
        }

        SaveManager.AutoSave(useAutoSaveNamedEnding ? endingID : null);
        ShowEndingPanel(title, desc);
    }

    public void TriggerEndingFromFungus()
    {
        if (flowchart == null)
        {
            flowchart = GetComponent<Flowchart>();
            if (flowchart == null)
                flowchart = FindObjectOfType<Flowchart>();
        }

        if (flowchart == null)
        {
            Debug.LogError("[EndingManager] flowchart is not assigned and no Flowchart was found in the scene. Cannot read Fungus variables.");
            return;
        }

        string endingID = flowchart.GetStringVariable(endingIDVariable);
        string title = flowchart.GetStringVariable(titleVariable);
        string desc = flowchart.GetStringVariable(descVariable);
        TriggerEnding(endingID, title, desc);
    }

    public void ShowEndingPanel(string title, string desc)
    {
        if (endingPanel == null)
        {
            Debug.LogError("[EndingManager] endingPanel or endingPanelPrefab is not assigned.");
            return;
        }

        if (endingTitle != null)
            endingTitle.text = title;

        if (endingDesc != null)
            endingDesc.text = desc;

        if (endingHint != null)
            endingHint.text = "结局达成！请选择：-->>";

        endingPanel.SetActive(true);
        if (endingCanvasGroup != null)
        {
            endingCanvasGroup.alpha = 0f;
            endingCanvasGroup.interactable = false;
            endingCanvasGroup.blocksRaycasts = false;
            StopAllCoroutines();
            StartCoroutine(FadeCanvas(0f, 1f));
        }
    }

    public void HideEndingPanel()
    {
        if (endingPanel == null)
            return;

        if (endingCanvasGroup != null)
        {
            StopAllCoroutines();
            StartCoroutine(FadeCanvas(1f, 0f, () => endingPanel.SetActive(false)));
        }
        else
        {
            endingPanel.SetActive(false);
        }
    }

    private IEnumerator FadeCanvas(float from, float to, System.Action onComplete = null)
    {
        float timer = 0f;
        if (endingCanvasGroup != null)
        {
            endingCanvasGroup.alpha = from;
            while (timer < fadeDuration)
            {
                timer += Time.unscaledDeltaTime;
                endingCanvasGroup.alpha = Mathf.Lerp(from, to, timer / fadeDuration);
                yield return null;
            }
            endingCanvasGroup.alpha = to;
            endingCanvasGroup.interactable = to > 0.5f;
            endingCanvasGroup.blocksRaycasts = to > 0.5f;
        }

        onComplete?.Invoke();
    }

    public void StartNewGame()
    {
        SaveManager.StartNewGame();
        SceneManager.LoadScene(newGameSceneIndex);
    }

    public void RestartCurrentScene()
    {
        HideEndingPanel();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMenu()
    {
        SaveManager.AutoSave();
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}