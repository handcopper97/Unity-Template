using UnityEngine;

/// <summary>
/// 어떤 씬에서 시작하든 게임 실행 시 설정 로드/적용과 매니저 생성을 보장한다.
/// </summary>
public static class GameBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        GameSettings.Load();
        GameSettings.ApplySavedResolution();
        GameSettings.ApplyVolume();
        AudioManager.EnsureExists();
    }
}
