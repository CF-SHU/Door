using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 命令模式的接口
public abstract class InputAction : ScriptableObject
{
    public string keyWord;

    public abstract void RespondToInput(GameController controller, string[] separatedInputWords);
}
