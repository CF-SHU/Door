// CharacterData.cs
using UnityEngine;

[System.Serializable]
public class CharacterData
{
    public string characterID;          // ��ɫID
    public string characterName;        // ��ɫ����
    public Sprite characterPortrait;    // ��ɫ����/ͷ��
    public Sprite fullBodyImage;        // ȫ����
    public string description;          // ��ɫ����
    public string[] relationshipQuotes;  // ��ϵ��¼

    // �øжȵ���ֵ
    public int affectionLevel;
}