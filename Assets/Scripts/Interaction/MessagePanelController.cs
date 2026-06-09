using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MessagePanelController : MonoBehaviour
{
    [Header("UI 组件")]
    public TMP_Text messageText;          // 拖入 Content 下的 TMP_Text
    public Button closeButton;            // 拖入关闭按钮
    public CanvasGroup canvasGroup;       // 可选，用于淡入淡出效果

    private System.Action onCloseCallback;

    void Awake()
    {
        // 绑定关闭按钮事件
        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        
        // 初始隐藏
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 显示消息面板
    /// </summary>
    /// <param name="content">要显示的文本</param>
    /// <param name="onClose">面板关闭时调用的回调（用于 Fungus 继续）</param>
    public void Show(string content, System.Action onClose)
    {
        onCloseCallback = onClose;
        if (messageText != null)
            messageText.text = content;
        
        gameObject.SetActive(true);
        
        // 可选：淡入效果
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            // 使用 DOTween 或简单协程实现淡入
            StartCoroutine(FadeIn());
        }
    }

    private void OnCloseButtonClicked()
    {
        // 可选：淡出效果
        if (canvasGroup != null)
            StartCoroutine(FadeOutAndClose());
        else
            CloseImmediately();
    }

    private void CloseImmediately()
    {
        gameObject.SetActive(false);
        onCloseCallback?.Invoke();
    }

    private System.Collections.IEnumerator FadeIn()
    {
        float duration = 0.2f;
        float elapsed = 0;
        while (elapsed < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(0, 1, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1;
    }

    private System.Collections.IEnumerator FadeOutAndClose()
    {
        float duration = 0.2f;
        float elapsed = 0;
        while (elapsed < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(1, 0, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0;
        CloseImmediately();
    }
}