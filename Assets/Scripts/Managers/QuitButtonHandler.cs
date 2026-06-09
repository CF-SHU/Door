using UnityEngine;
using UnityEngine.SceneManagement;

public class QuitButtonHandler : MonoBehaviour
{
    // Quit without saving (useful for debug / instant exit)
    public void Quit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // Save current data (autosave) then quit
    public void SaveAndQuit()
    {
        // Use a named autosave so the reason is recorded
        try
        {
            SaveManager.AutoSave("quit");
        }
        catch
        {
            // swallow exceptions to ensure quit still proceeds in case SaveManager isn't ready
        }

        Quit();
    }

    // Save and return to main menu
    public void SaveAndBackToMenu()
    {
        try
        {
            SaveManager.AutoSave("menu");
        }
        catch { }

        SceneManager.LoadScene("MainMenu");
    }
}
