using System;
using System.Collections.Generic;

//这个标记告诉unity这个类可以被序列化成JSON格式存储
//List & Dictionary都是可序列化的
[Serializable]
public class GameData
{
    //当前场景索引，用于读档时加载场景
    public int currentSceneIndex;
    //当前对话ID，用于读档时恢复对话进度
    public string currentDialogueID;
    //当前已收集到的道具
    public List<string> collectedItems = new List<string>();
    //当前已触发的剧情flag
    public List<string> triggeredFlags = new List<string>();
    //特殊属性面板值
    public Dictionary<string, int> attributes = new Dictionary<string, int>();
    //存档时间
    public string saveTime;
    //存档的文件名
    public string screenshotName;
}
