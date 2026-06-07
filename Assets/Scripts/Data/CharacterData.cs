// CharacterData.cs
using UnityEngine;

[System.Serializable]
public class CharacterData
{
    public string characterID;          
    public string characterName;        
    public Sprite characterPortrait;    
    public Sprite fullBodyImage;        
    public string description;          
    public string[] relationshipQuotes;  

    public int affectionLevel;
}