using System.Xml.Serialization;
using UnityEngine;

[CreateAssetMenu(menuName = "TextAdventure/InputActions/TakeKey")]
public class TakeKey : InputAction
{
    public override void RespondToInput(GameController controller, string[] separatedInputWords)
    {
        //检查当前房间是否是"Room_Start"
        if (controller.roomNavigation.currentRoom.roomName != "ClassRoom")
        {
            controller.LogStringWithReturn("There is nothing to take here.");
            controller.DisplayLoggedText();
            return;
        }
        //检查是否已经拿过钥匙
        if(controller.GetFlag("hasKey"))
        {
            controller.LogStringWithReturn("You already took the key, It's in your pocket.");
            controller.DisplayLoggedText();
            return;
        }
        //拾取钥匙
        controller.SetFlag("hasKey", true);
        controller.LogStringWithReturn("You picked up the rusty old key.");

        //触发npc对话
        TriggerTeacherDialogue(controller);
        controller.DisplayLoggedText();
    }

    private void TriggerTeacherDialogue(GameController controller)
    {
        //检查是否已经触发过对话
        if (controller.GetFlag("老师对话")) return;
        controller.SetFlag("老师对话",true);
        string dialogue = "老师注意到了你，大声说：";
        controller.LogStringWithReturn(dialogue);
    }
}
