using UnityEngine;

/// <summary>
/// BGM/SFX 재생용 싱글턴. 씬 전환에도 유지된다(DontDestroyOnLoad).
/// GameBootstrap 이 게임 시작 시 자동 생성하므로 씬에 미리 배치할 필요 없음.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    AudioSource bgmSource;
    AudioSource sfxSource;

    public static void EnsureExists()
    {
        if (Instance != null)
            return;
        var go = new GameObject("AudioManager");
        go.AddComponent<AudioManager>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;

        RefreshVolumes();
    }

    public void RefreshVolumes()
    {
        bgmSource.volume = GameSettings.BgmVolume;
        sfxSource.volume = GameSettings.SfxVolume;
    }

    public void PlayBgm(AudioClip clip)
    {
        if (clip == null || bgmSource.clip == clip)
            return;
        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void StopBgm() => bgmSource.Stop();

    public void PlaySfx(AudioClip clip)
    {
        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }
}
