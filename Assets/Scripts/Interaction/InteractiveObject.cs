// InteractiveObject.cs
using UnityEngine;
using UnityEngine.Events;

public class InteractiveObject : MonoBehaviour
{
    [Header("互动条件")]
    public string requiredItemID;       // 需要的道具ID（空表示不需要）
    public bool isDone;                 // 互动是否已完成
    public string interactionFlag;       // 互动完成的标记ID

    [Header("互动反馈")]
    public string hoverMessage;          // 鼠标悬停提示
    //public DialogueLine[] dialogueBefore; // 互动前的对话
    //public DialogueLine[] dialogueAfter;  // 互动后的对话

    [Header("互动事件")]
    public UnityEvent onInteractSuccess;  // 互动成功时触发的事件

    // 被点击时调用
    public virtual void OnInteract()
    {
        // 检查是否需要特定道具
        if (!string.IsNullOrEmpty(requiredItemID))
        {
            if (!SaveManager.CurrentData.collectedItems.Contains(requiredItemID))
            {
                // 没有所需道具，显示提示对话
                Debug.Log($"需要道具：{requiredItemID}");
                //if (dialogueBefore.Length > 0)
                //    DialogueSystem.Instance.StartDialogue(dialogueBefore);
                return;
            }
        }

        // 互动成功
        if (!isDone)
        {
            isDone = true;
            if (!string.IsNullOrEmpty(interactionFlag))
            {
                SaveManager.CurrentData.triggeredFlags.Add(interactionFlag);
            }
            onInteractSuccess?.Invoke();
            //if (dialogueAfter.Length > 0)
            //    DialogueSystem.Instance.StartDialogue(dialogueAfter);
        }
        else
        {
            // 已经互动过了，显示后续对话
            //if (dialogueAfter.Length > 0)
            //    DialogueSystem.Instance.StartDialogue(dialogueAfter);
        }
    }

    // 鼠标点击检测
    void OnMouseDown()
    {
        // 发射一条射线，检测是否点击到了自己
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        if (hit.collider != null && hit.collider.gameObject == gameObject)
        {
            OnInteract();
        }
    }

    // 鼠标悬停时改变光标
    void OnMouseEnter()
    {
        // 可以在这里改变鼠标样式
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}