// CharacterCardUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterCardUI : MonoBehaviour
{
    [Header("UI引用")]
    public GameObject characterListPanel;    // 角色列表
    public GameObject characterDetailPanel; // 角色详情
    public Transform characterListParent;   // 列表项的父对象
    public GameObject characterListItemPrefab; // 列表项预制体

    [Header("详情面板")]
    public Image portraitImage;
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    public TMP_Text affectionText;

    [Header("动画")]
    public Animator cardAnimator;           // 卡片动画器

    [Header("角色数据")]
    public CharacterData[] characters;

    void Start()
    {
        BuildCharacterList();
    }

    // 构建角色列表
    void BuildCharacterList()
    {
        foreach (var character in characters)
        {
            // Instantiate：克隆一个GameObject（预制体）
            GameObject item = Instantiate(characterListItemPrefab, characterListParent);
            item.GetComponentInChildren<Image>().sprite = character.characterPortrait;
            item.GetComponentInChildren<TMP_Text>().text = character.characterName;

            // 绑定点击事件
            item.GetComponent<Button>().onClick.AddListener(() =>
            {
                ShowCharacterDetail(character);
            });
        }
    }

    // 显示角色详情（带动画转场）
    void ShowCharacterDetail(CharacterData character)
    {
        characterListPanel.SetActive(false);
        characterDetailPanel.SetActive(true);

        // 触发转场动画
        cardAnimator?.SetTrigger("FlipCard");

        // 更新详情信息
        portraitImage.sprite = character.fullBodyImage;
        nameText.text = character.characterName;
        descriptionText.text = character.description;
        affectionText.text = $"好感度: {character.affectionLevel}";
    }

    // 返回角色列表
    public void BackToList()
    {
        characterDetailPanel.SetActive(false);
        characterListPanel.SetActive(true);
    }
}