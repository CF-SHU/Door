//GameSaveBootstrap.cs
//创建一个持久化的gameObject来初始化存档系统
using UnityEngine;

public class GameSaveBootstrap : MonoBehaviour
{
    //Awake在Start前执行，适合初始化工作
    private void Awake()
    {
        SaveManager.Initialize();

        //DontDestroyOnLoad: 让这个gameObject在切换场景时不被销毁
        //全局管理
        DontDestroyOnLoad(gameObject);
    }
    // use this for initialization
    void Start()
    {

    }

    // update is called once per frame
    void Update()
    {

    }
}

