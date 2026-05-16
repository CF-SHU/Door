using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public Text displayText;
    public InputAction[] inputActions;
    // 道具/对话是否被触发，状态检测
    public Dictionary<string, bool> gameFlags = new Dictionary<string, bool>();

    [HideInInspector] public RoomNavigation roomNavigation;
    [HideInInspector] public List<string> interactionDescriptionsInRoom = new List<string>();

    List<string> actionLog = new List<string>();

    // 设置/获取标记
    public void SetFlag(string flag, bool value)
    {
        gameFlags[flag] = value;
    }
    public bool GetFlag(string flag)
    {
        return gameFlags.ContainsKey(flag) ? gameFlags[flag] : false;
    }

    // Use this for initialization
    void Awake()
    {
        roomNavigation = GetComponent<RoomNavigation>();
    }

    void Start()
    {
        if (displayText != null)
            displayText.text = "GameController is alive!";
        else
            Debug.LogError("displayText is null!");

        if (roomNavigation.currentRoom != null)
        {
            displayText.text = roomNavigation.currentRoom.description;
        }
        else
        {
            displayText.text = "currentRoom is null!";
            Debug.LogError("No current room assigned!");
        }

        DisplayRoomText();
        DisplayLoggedText();
    }

    public void DisplayLoggedText()
    {
        string logAsText = string.Join("\n", actionLog.ToArray());

        displayText.text = logAsText;
    }

    public void DisplayRoomText()
    {
        ClearCollectionsForNewRoom();

        UnpackRoom();

        string joinedInteractionDescriptions = string.Join("\n", interactionDescriptionsInRoom.ToArray());

        string combinedText = roomNavigation.currentRoom.description + "\n" + joinedInteractionDescriptions;

        LogStringWithReturn(combinedText);
    }

    void UnpackRoom()
    {
        roomNavigation.UnpackExitsInRoom();
    }

    void ClearCollectionsForNewRoom()
    {
        interactionDescriptionsInRoom.Clear();
        roomNavigation.ClearExits();
    }

    public void LogStringWithReturn(string stringToAdd)
    {
        actionLog.Add(stringToAdd + "\n");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
