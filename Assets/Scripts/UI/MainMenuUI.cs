using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 메인메뉴 씬: 게임 시작 / 옵션 / 게임 종료 버튼 처리.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    public Button startButton;
    public Button optionsButton;
    public Button quitButton;
    public OptionsPanel optionsPanel;

    void Awake()
    {
        startButton.onClick.AddListener(SceneLoader.LoadInGame);
        optionsButton.onClick.AddListener(optionsPanel.Open);
        quitButton.onClick.AddListener(SceneLoader.QuitGame);
    }

    void Update()
    {
        // 옵션 패널이 열려 있을 때 일시정지 키(기본 ESC)로 닫기
        if (OptionsPanel.IsRebinding || Time.frameCount == OptionsPanel.LastRebindFrame)
            return;
        if (optionsPanel.gameObject.activeSelf && KeyBindings.GetKeyDown(GameAction.Pause))
            optionsPanel.Close();
    }
}
