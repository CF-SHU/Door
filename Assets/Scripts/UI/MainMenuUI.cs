using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("按钮引用")]
    public Button startButton;
    public Button continueButton;
    public Button settingsButton;
    public Button characterButton;
    public Button loadButton;
    public Button quitButton;
    // Start is called before the first frame update
    void Start()
    {
        //绑定按钮点击事件
        startButton.onClick.AddListener(OnStartGame);
        continueButton.onClick.AddListener(OnContinue);
        settingsButton.onClick.AddListener(OnSettings);
        characterButton.onClick.AddListener(OnCharacter);
        quitButton.onClick.AddListener(OnQuit);

        //没有存档，禁用continueButton
        if (!SaveManager.HasSaveData())
        {
            continueButton.interactable = false;
        }
    }
    //开始游戏，第一个场景ClassRoom
    void OnStartGame()
    {
        SceneManager.LoadScene("ClassRoomScene");
    }
    //继续游戏，加载存档
    void OnContinue()
    {
        SaveManager.LoadGame();
    }
    //显示————设置面板
    void OnSettings()
    {
        // 从父级 Canvas（或根Canvas）上获取 UIPanelManager
        GetComponentInParent<UIPanelManager>()?.ShowPanel("SettingsPanel");
    }
    //显示————人物资料面板
    void OnCharacter()
    {
        GetComponentInParent<UIPanelManager>()?.ShowPanel("CharacterPanel");
    }
    //退出游戏
    void OnQuit()
    {
        SaveManager.AutoSave();
        Application.Quit();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
