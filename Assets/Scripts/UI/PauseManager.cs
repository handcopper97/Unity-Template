using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 인게임 씬 일시정지 관리.
/// - 일시정지 키(기본 ESC)로 패널 열기/닫기, Time.timeScale 0/1 전환
/// - 이어하기 / 옵션 / 메인 메뉴 / 게임 종료 버튼 처리
/// - 옵션 패널이 열려 있으면 ESC 는 옵션 닫기로 동작
/// </summary>
public class PauseManager : MonoBehaviour
{
    public GameObject pausePanel;
    public Button resumeButton;
    public Button optionsButton;
    public Button mainMenuButton;
    public Button quitButton;
    public OptionsPanel optionsPanel;

    public static bool IsPaused { get; private set; }

    void Awake()
    {
        resumeButton.onClick.AddListener(Resume);
        optionsButton.onClick.AddListener(OpenOptions);
        mainMenuButton.onClick.AddListener(SceneLoader.LoadMainMenu);
        quitButton.onClick.AddListener(SceneLoader.QuitGame);
        optionsPanel.Closed += OnOptionsClosed;

        pausePanel.SetActive(false);
        SetPaused(false);
    }

    void OnDestroy()
    {
        optionsPanel.Closed -= OnOptionsClosed;
        // 씬이 내려갈 때 시간 정지가 남지 않도록 보장
        SetPaused(false);
    }

    void Update()
    {
        // 키 리바인딩 중이거나, 리바인딩을 방금 끝낸 프레임의 ESC 는 무시
        if (OptionsPanel.IsRebinding || Time.frameCount == OptionsPanel.LastRebindFrame)
            return;
        if (!KeyBindings.GetKeyDown(GameAction.Pause))
            return;

        if (optionsPanel.gameObject.activeSelf)
        {
            optionsPanel.Close(); // Closed 콜백에서 일시정지 패널로 복귀
            return;
        }

        if (IsPaused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        SetPaused(true);
        pausePanel.SetActive(true);
    }

    public void Resume()
    {
        SetPaused(false);
        pausePanel.SetActive(false);
    }

    void OpenOptions()
    {
        pausePanel.SetActive(false);
        optionsPanel.Open();
    }

    void OnOptionsClosed()
    {
        if (IsPaused)
            pausePanel.SetActive(true);
    }

    static void SetPaused(bool paused)
    {
        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;
        AudioListener.pause = paused;
    }
}
