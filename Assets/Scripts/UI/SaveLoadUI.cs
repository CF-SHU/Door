using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;   // 使用 TextMeshPro

public class SaveLoadUI : MonoBehaviour
{
    [Header("存档槽按钮")]
    public Button[] saveSlotButtons;

    [Header("读档模式 Toggle")]
    public Toggle readModeToggle;   // 拖入名为 "Read" 的 Toggle，勾选=读档模式

    private bool isSaveMode = true;   // true=存档模式, false=读档模式

    void Start()
    {
        if (saveSlotButtons == null || saveSlotButtons.Length == 0)
        {
            Debug.LogError("SaveLoadUI: 没有指定存档槽按钮！");
            return;
        }

        if (readModeToggle != null)
        {
            // 初始化：Toggle 勾选状态 = 读档模式 = isSaveMode = false
            readModeToggle.isOn = !isSaveMode;   // 假如默认是存档模式，则 Toggle 未勾选
            readModeToggle.onValueChanged.AddListener(OnReadToggleChanged);
        }
        else
        {
            Debug.LogWarning("SaveLoadUI: 未绑定 Read Toggle，将无法切换模式");
        }

        RefreshSlots();
    }

    // Toggle 值改变时调用：勾选 -> 读档模式 (isSaveMode = false)
    private void OnReadToggleChanged(bool isOn)
    {
        // isOn 为 true 表示 Read 被勾选 → 读档模式
        isSaveMode = !isOn;   // 勾选时 isSaveMode = false，未勾选时 isSaveMode = true
        RefreshSlots();
    }

    public void RefreshSlots()
    {
        if (saveSlotButtons == null) return;

        List<SaveSlotInfo> slots = SaveManager.GetSaveSlots();

        for (int i = 0; i < saveSlotButtons.Length; i++)
        {
            Button btn = saveSlotButtons[i];
            if (btn == null) continue;

            // 获取文本组件（支持旧版 Text 和 TMP）
            Text textLegacy = btn.GetComponentInChildren<Text>();
            TMP_Text textTMP = btn.GetComponentInChildren<TMP_Text>();
            if (textLegacy == null && textTMP == null)
            {
                Debug.LogError($"按钮[{i}] 缺少文本组件！");
                continue;
            }

            bool hasSlot = (i < slots.Count);
            SaveSlotInfo slot = hasSlot ? slots[i] : new SaveSlotInfo { isEmpty = true, slotIndex = i, saveTime = "" };

            string displayText = "";
            bool interactable = false;

            if (!slot.isEmpty)
            {
                // 有存档数据
                displayText = $"存档 {slot.slotIndex}\n{slot.saveTime}";
                interactable = true;   // 有数据的槽在两种模式下都可用
            }
            else
            {
                // 空槽
                displayText = $"存档 {i + 1}\n[空]";
                interactable = isSaveMode;   // 只有存档模式下空槽才可点（新建存档）
            }

            if (textLegacy != null) textLegacy.text = displayText;
            else if (textTMP != null) textTMP.text = displayText;

            btn.interactable = interactable;
        }
    }

    public void OnSlotClicked(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= saveSlotButtons.Length)
        {
            Debug.LogError($"无效槽位索引 {slotIndex}");
            return;
        }

        if (isSaveMode)
        {
            // 存档模式
            SaveManager.SaveGame(slotIndex);
        }
        else
        {
            // 读档模式：确保槽位非空
            List<SaveSlotInfo> slots = SaveManager.GetSaveSlots();
            if (slotIndex < slots.Count && !slots[slotIndex].isEmpty)
                SaveManager.LoadGame(slotIndex);
            else
                Debug.LogWarning($"槽位 {slotIndex} 为空，无法读档");
        }
        RefreshSlots();
    }

    // 可选：外部手动设置模式（例如用按钮调用）
    public void SetSaveMode(bool save)
    {
        isSaveMode = save;
        if (readModeToggle != null)
            readModeToggle.isOn = !save;   // 同步 Toggle 显示：存档模式 -> 未勾选
        else
            RefreshSlots();
    }
}