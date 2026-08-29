# Unity 기초 템플릿 프로젝트

Unity **2022.3.62f2** (LTS) 기준. 메인메뉴 + 인게임 씬, 옵션(해상도/음량/키 설정), 일시정지 기능이 포함된 기초 템플릿입니다.

## 시작하기

1. Unity Hub → **Add** → `UnityTemplate` 폴더 선택 → 2022.3.62f2 로 열기
2. (씬이 없다면) 상단 메뉴 **Tools > 템플릿 > 씬 생성 (MainMenu + InGame)** 클릭
   - `Assets/Scenes/MainMenu.unity`, `Assets/Scenes/InGame.unity` 가 생성되고 빌드 설정에 자동 등록됩니다.
   - 다시 실행하면 씬을 초기 상태로 덮어씁니다.
3. `MainMenu` 씬을 열고 플레이.

## 기능

### 메인메뉴 씬 (MainMenu)
- **게임 시작**: 인게임 씬으로 이동
- **옵션**: 옵션 패널 열기 (ESC 로 닫기 가능)
- **게임 종료**: 게임 종료 (에디터에서는 플레이 모드 종료)

### 인게임 씬 (InGame)
- **ESC** (일시정지 키, 변경 가능): 일시정지 패널 열기/닫기. 열리면 `Time.timeScale = 0` + 오디오 일시정지, 닫히면 복구.
- 일시정지 패널 버튼: **이어서 하기 / 옵션 / 메인 메뉴로 / 게임 종료**
- 옵션 패널이 열린 상태에서 ESC 를 누르면 옵션만 닫히고 일시정지 패널로 돌아갑니다.

### 옵션 패널 (공용)
- **화면**: 해상도 드롭다운 + 전체 화면 토글 + 적용 버튼
- **사운드**: 전체/배경음/효과음 음량 슬라이더 (즉시 반영)
- **키 설정**: 각 액션의 키 버튼 클릭 → 새 키 입력으로 변경. ESC 는 입력 취소 키. 이미 사용 중인 키를 지정하면 서로 교환됩니다. "기본 키로 초기화" 지원.
- 모든 설정은 `PlayerPrefs` 에 저장되어 재시작 후에도 유지됩니다.

## 코드 구조

| 파일 | 역할 |
|---|---|
| `Scripts/Core/GameSettings.cs` | 음량·해상도 설정 저장/로드/적용 |
| `Scripts/Core/KeyBindings.cs` | 액션→키 매핑, 저장/로드. 게임플레이에서 `KeyBindings.GetKeyDown(GameAction.Jump)` 식으로 사용 |
| `Scripts/Core/AudioManager.cs` | BGM/SFX 재생 싱글턴 (`AudioManager.Instance.PlaySfx(clip)`) |
| `Scripts/Core/GameBootstrap.cs` | 게임 시작 시 설정 로드·적용, 매니저 자동 생성 |
| `Scripts/Core/SceneLoader.cs` | 씬 전환/종료 헬퍼 (전환 전 타임스케일 복구 보장) |
| `Scripts/UI/MainMenuUI.cs` | 메인메뉴 버튼 처리 |
| `Scripts/UI/PauseManager.cs` | 일시정지 토글·패널·버튼 처리 |
| `Scripts/UI/OptionsPanel.cs` | 옵션 패널 (해상도/음량/키 리바인딩) |
| `Editor/TemplateSceneGenerator.cs` | 씬 자동 생성 에디터 메뉴 |

## 확장 포인트

- **액션 추가**: `KeyBindings.cs` 의 `GameAction` enum 과 `Defaults` 딕셔너리, `DisplayName` 에 항목 추가 → 옵션 키 설정 UI 에 자동 반영.
- **BGM 넣기**: 오디오 클립을 `AudioManager.Instance.PlayBgm(clip)` 으로 재생.
- **UI 폰트 교체**: UI 는 TextMeshPro 기반이며, TMP Settings 의 기본 폰트 에셋(Default Font Asset)을 바꾸면 씬 재생성 시 전체에 적용됩니다.

## 참고

- 입력은 구(Legacy) Input Manager 기반입니다 (`Input.GetKeyDown`).
- ESC 는 키 리바인딩 취소 키로 예약되어 있어 다른 액션에 할당할 수 없습니다.
