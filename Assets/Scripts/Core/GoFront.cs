using UnityEngine;

[CreateAssetMenu(menuName = "TextAdventure/InputActions/GoFront")]
public class GoFront : InputAction
{
    public override void RespondToInput(GameController controller, string[] separatedInputWords)
    {
        //Debug.Log("GoFront RespondToInput called!");
        Debug.Log("GoFront RespondToInput called with arg: " + (separatedInputWords.Length > 1 ? separatedInputWords[1] : "none"));
        controller.roomNavigation.AttemptToChangeRooms(separatedInputWords[1]);
        // 调用房间导航，尝试去“front”方向
        controller.roomNavigation.AttemptToChangeRooms("front");
        // AttemptToChangeRooms 方法内部查找 exitDictionary 的键
        // 所以我们的出口配置中必须包含 "front" 和 "back" 键。
    }
}
