using UnityEngine;

[CreateAssetMenu(menuName = "TextAdventure/InputActions/GoBack")]
public class GoBack : InputAction
{
    public override void RespondToInput(GameController controller, string[] separatedInputWords)
    {
        Debug.Log("GoBack RespondToInput called!");
        // 调用房间导航，尝试去“back”方向
        controller.roomNavigation.AttemptToChangeRooms("back");
        // AttemptToChangeRooms 方法内部查找 exitDictionary 的键
        // 所以我们的出口配置中必须包含 "front" 和 "back" 键。
    }
}
