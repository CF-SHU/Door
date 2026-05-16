using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ButtonCommand : MonoBehaviour
{
    public InputAction commandAction; //拖入一个InputAction资产(eg:GoFront)
    public string commandArg; //eg: front

    private GameController controller;

    // Use this for initialization
    void Start()
    {
        controller = FindObjectOfType<GameController>();
        /*
        GetComponent<Button>().onClick.AddListener(() =>
        {
            string[] fakeInput = new string[] { commandAction.keyWord, commandArg };
            commandAction.RespondToInput(controller, fakeInput);
        });
        */


        Debug.Log("ButtonCommand initialized on " + gameObject.name);
        var btn = GetComponent<Button>();
        if (btn == null)
        {
            Debug.LogError("No Button component found on " + gameObject.name);
            return;
        }
        btn.onClick.AddListener(() =>
        {
            Debug.Log("Button clicked: " + commandAction?.name);
            if (commandAction == null)
            {
                Debug.LogError("commandAction is null on " + gameObject.name);
                return;
            }
            string[] fakeInput = new string[] { commandAction.keyWord, commandArg };
            commandAction.RespondToInput(controller, fakeInput);
        });
    }

}