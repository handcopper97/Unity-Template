using UnityEngine;

/// <summary>
/// 게임 전역 설정(음량, 해상도)을 PlayerPrefs 에 저장/로드하고 적용하는 정적 클래스.
/// </summary>
public static class GameSettings
{
    const string KeyMaster = "opt_master_volume";
    const string KeyBgm    = "opt_bgm_volume";
    const string KeySfx    = "opt_sfx_volume";
    const string KeyResW   = "opt_res_width";
    const string KeyResH   = "opt_res_height";
    const string KeyFull   = "opt_fullscreen";

    public static float MasterVolume = 1f;
    public static float BgmVolume    = 1f;
    public static float SfxVolume    = 1f;

    public static void Load()
    {
        MasterVolume = PlayerPrefs.GetFloat(KeyMaster, 1f);
        BgmVolume    = PlayerPrefs.GetFloat(KeyBgm, 1f);
        SfxVolume    = PlayerPrefs.GetFloat(KeySfx, 1f);
        KeyBindings.Load();
    }

    public static void Save()
    {
        PlayerPrefs.SetFloat(KeyMaster, MasterVolume);
        PlayerPrefs.SetFloat(KeyBgm, BgmVolume);
        PlayerPrefs.SetFloat(KeySfx, SfxVolume);
        KeyBindings.Save();
        PlayerPrefs.Save();
    }

    /// <summary>마스터 볼륨은 AudioListener, BGM/SFX 는 AudioManager 소스에 반영.</summary>
    public static void ApplyVolume()
    {
        AudioListener.volume = MasterVolume;
        if (AudioManager.Instance != null)
            AudioManager.Instance.RefreshVolumes();
    }

    /// <summary>저장된 해상도가 있으면 적용. 없으면 현재 해상도 유지.</summary>
    public static void ApplySavedResolution()
    {
        if (!PlayerPrefs.HasKey(KeyResW) || !PlayerPrefs.HasKey(KeyResH))
            return;

        int w = PlayerPrefs.GetInt(KeyResW);
        int h = PlayerPrefs.GetInt(KeyResH);
        bool fullscreen = PlayerPrefs.GetInt(KeyFull, Screen.fullScreen ? 1 : 0) == 1;
        if (w > 0 && h > 0)
            Screen.SetResolution(w, h, fullscreen);
    }

    public static void SetResolution(int width, int height, bool fullscreen)
    {
        PlayerPrefs.SetInt(KeyResW, width);
        PlayerPrefs.SetInt(KeyResH, height);
        PlayerPrefs.SetInt(KeyFull, fullscreen ? 1 : 0);
        PlayerPrefs.Save();
        Screen.SetResolution(width, height, fullscreen);
    }
}
