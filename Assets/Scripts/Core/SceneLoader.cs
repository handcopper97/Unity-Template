using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬 전환/게임 종료 헬퍼. 씬 전환 전에 항상 타임스케일을 복구한다.
/// </summary>
public static class SceneLoader
{
    public const string MainMenuScene = "MainMenu";
    public const string InGameScene = "InGame";

    public static void LoadMainMenu()
    {
        ResetTimeState();
        SceneManager.LoadScene(MainMenuScene);
    }

    public static void LoadInGame()
    {
        ResetTimeState();
        SceneManager.LoadScene(InGameScene);
    }

    public static void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    static void ResetTimeState()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }
}
