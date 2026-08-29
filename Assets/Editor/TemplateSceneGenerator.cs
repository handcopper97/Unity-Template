using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 메뉴: Tools > 템플릿 > 씬 생성.
/// MainMenu / InGame 씬을 UI(TextMeshPro) 와 스크립트 연결까지 완성된 상태로 생성하고
/// 빌드 설정에 등록한다. 다시 실행하면 씬을 덮어쓴다.
/// 텍스트는 TMP Settings 의 기본 폰트 에셋을 사용한다.
/// </summary>
public static class TemplateSceneGenerator
{
    const string ScenesDir = "Assets/Scenes";
    const string MainMenuPath = ScenesDir + "/MainMenu.unity";
    const string InGamePath = ScenesDir + "/InGame.unity";

    static readonly Color WindowColor = new Color(0.13f, 0.14f, 0.18f, 0.98f);
    static readonly Color DimColor = new Color(0f, 0f, 0f, 0.65f);
    static readonly Color LabelColor = new Color(0.92f, 0.92f, 0.92f, 1f);
    static readonly Color SectionColor = new Color(1f, 0.85f, 0.45f, 1f);

    [MenuItem("Tools/템플릿/씬 생성 (MainMenu + InGame)")]
    public static void Generate()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        if (!AssetDatabase.IsValidFolder(ScenesDir))
            AssetDatabase.CreateFolder("Assets", "Scenes");

        CreateMainMenuScene();
        CreateInGameScene();
        SetupBuildSettings();

        EditorSceneManager.OpenScene(MainMenuPath);
        EditorUtility.DisplayDialog("템플릿 생성 완료",
            "MainMenu / InGame 씬을 생성하고 빌드 설정에 등록했습니다.\n" +
            "MainMenu 씬에서 플레이 버튼을 눌러 확인하세요.", "확인");
    }

    // =========================================================
    // 씬 구성
    // =========================================================

    static void CreateMainMenuScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateCamera(new Color(0.09f, 0.09f, 0.16f), true);
        CreateEventSystem();
        GameObject canvas = CreateCanvas();

        // 타이틀
        TextMeshProUGUI title = CreateText(canvas.transform, "Title", "GAME TITLE", 80f,
            TextAlignmentOptions.Center, Color.white, FontStyles.Bold);
        RectTransform titleRt = title.rectTransform;
        titleRt.anchorMin = titleRt.anchorMax = new Vector2(0.5f, 1f);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = new Vector2(0f, -140f);
        titleRt.sizeDelta = new Vector2(1400f, 120f);

        // 메뉴 버튼 3개
        var buttonRoot = new GameObject("MenuButtons", typeof(RectTransform));
        buttonRoot.transform.SetParent(canvas.transform, false);
        var buttonRootRt = (RectTransform)buttonRoot.transform;
        buttonRootRt.anchorMin = buttonRootRt.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRootRt.pivot = new Vector2(0.5f, 0.5f);
        buttonRootRt.anchoredPosition = new Vector2(0f, -80f);
        buttonRootRt.sizeDelta = new Vector2(380f, 300f);
        var vlg = buttonRoot.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 24f;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        Button startButton = CreateMenuButton(buttonRoot.transform, "StartButton", "게임 시작");
        Button optionsButton = CreateMenuButton(buttonRoot.transform, "OptionsButton", "옵션");
        Button quitButton = CreateMenuButton(buttonRoot.transform, "QuitButton", "게임 종료");

        // 옵션 패널 (가장 위에 그려지도록 마지막에 추가)
        OptionsPanel optionsPanel = BuildOptionsPanel(canvas.transform);

        var mainMenu = canvas.AddComponent<MainMenuUI>();
        mainMenu.startButton = startButton;
        mainMenu.optionsButton = optionsButton;
        mainMenu.quitButton = quitButton;
        mainMenu.optionsPanel = optionsPanel;

        EditorSceneManager.SaveScene(scene, MainMenuPath);
    }

    static void CreateInGameScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Camera cam = CreateCamera(new Color(0.2f, 0.25f, 0.35f), false);
        cam.transform.position = new Vector3(0f, 2.5f, -6f);
        cam.transform.rotation = Quaternion.Euler(15f, 0f, 0f);

        var lightGo = new GameObject("Directional Light", typeof(Light));
        var light = lightGo.GetComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1f;
        lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // 플레이스홀더 월드
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.name = "Ground";
        plane.transform.localScale = new Vector3(2f, 1f, 2f);
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "PlaceholderCube";
        cube.transform.position = new Vector3(0f, 0.5f, 0f);

        CreateEventSystem();
        GameObject canvas = CreateCanvas();

        // HUD 안내 텍스트
        TextMeshProUGUI hud = CreateText(canvas.transform, "HudText", "인게임 씬  |  [ESC] 일시정지", 30f,
            TextAlignmentOptions.TopLeft, Color.white, FontStyles.Normal);
        RectTransform hudRt = hud.rectTransform;
        hudRt.anchorMin = hudRt.anchorMax = new Vector2(0f, 1f);
        hudRt.pivot = new Vector2(0f, 1f);
        hudRt.anchoredPosition = new Vector2(30f, -30f);
        hudRt.sizeDelta = new Vector2(900f, 50f);

        // 일시정지 패널
        GameObject pauseRoot = CreateOverlay(canvas.transform, "PausePanel");
        Image window = CreatePanel(pauseRoot.transform, "Window", WindowColor);
        RectTransform windowRt = window.rectTransform;
        SetCenter(windowRt, new Vector2(480f, 560f));

        TextMeshProUGUI pauseTitle = CreateText(window.transform, "Title", "일시정지", 44f,
            TextAlignmentOptions.Center, Color.white, FontStyles.Bold);
        RectTransform pauseTitleRt = pauseTitle.rectTransform;
        pauseTitleRt.anchorMin = pauseTitleRt.anchorMax = new Vector2(0.5f, 1f);
        pauseTitleRt.pivot = new Vector2(0.5f, 1f);
        pauseTitleRt.anchoredPosition = new Vector2(0f, -30f);
        pauseTitleRt.sizeDelta = new Vector2(400f, 60f);

        var pauseButtons = new GameObject("Buttons", typeof(RectTransform));
        pauseButtons.transform.SetParent(window.transform, false);
        var pauseButtonsRt = (RectTransform)pauseButtons.transform;
        pauseButtonsRt.anchorMin = pauseButtonsRt.anchorMax = new Vector2(0.5f, 0.5f);
        pauseButtonsRt.pivot = new Vector2(0.5f, 0.5f);
        pauseButtonsRt.anchoredPosition = new Vector2(0f, -40f);
        pauseButtonsRt.sizeDelta = new Vector2(340f, 380f);
        var pvlg = pauseButtons.AddComponent<VerticalLayoutGroup>();
        pvlg.spacing = 20f;
        pvlg.childControlWidth = true;
        pvlg.childControlHeight = true;
        pvlg.childForceExpandWidth = true;
        pvlg.childForceExpandHeight = false;

        Button resumeButton = CreateMenuButton(pauseButtons.transform, "ResumeButton", "이어서 하기");
        Button pauseOptionsButton = CreateMenuButton(pauseButtons.transform, "OptionsButton", "옵션");
        Button mainMenuButton = CreateMenuButton(pauseButtons.transform, "MainMenuButton", "메인 메뉴로");
        Button pauseQuitButton = CreateMenuButton(pauseButtons.transform, "QuitButton", "게임 종료");

        pauseRoot.SetActive(false);

        // 옵션 패널
        OptionsPanel optionsPanel = BuildOptionsPanel(canvas.transform);

        var pauseManager = canvas.AddComponent<PauseManager>();
        pauseManager.pausePanel = pauseRoot;
        pauseManager.resumeButton = resumeButton;
        pauseManager.optionsButton = pauseOptionsButton;
        pauseManager.mainMenuButton = mainMenuButton;
        pauseManager.quitButton = pauseQuitButton;
        pauseManager.optionsPanel = optionsPanel;

        EditorSceneManager.SaveScene(scene, InGamePath);
    }

    static void SetupBuildSettings()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MainMenuPath, true),
            new EditorBuildSettingsScene(InGamePath, true),
        };
    }

    // =========================================================
    // 옵션 패널
    // =========================================================

    static OptionsPanel BuildOptionsPanel(Transform canvasParent)
    {
        GameObject root = CreateOverlay(canvasParent, "OptionsPanel");
        Image window = CreatePanel(root.transform, "Window", WindowColor);
        SetCenter(window.rectTransform, new Vector2(860f, 930f));

        TextMeshProUGUI title = CreateText(window.transform, "Title", "옵션", 40f,
            TextAlignmentOptions.Center, Color.white, FontStyles.Bold);
        RectTransform titleRt = title.rectTransform;
        titleRt.anchorMin = titleRt.anchorMax = new Vector2(0.5f, 1f);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = new Vector2(0f, -24f);
        titleRt.sizeDelta = new Vector2(300f, 50f);

        // 내용 영역 (세로 레이아웃)
        var content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(window.transform, false);
        var contentRt = (RectTransform)content.transform;
        contentRt.anchorMin = Vector2.zero;
        contentRt.anchorMax = Vector2.one;
        contentRt.offsetMin = new Vector2(36f, 24f);
        contentRt.offsetMax = new Vector2(-36f, -84f);
        var vlg = content.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 10f;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // ----- 화면 -----
        CreateSectionLabel(content.transform, "화면");

        RectTransform resRow = CreateRow(content.transform, "ResolutionRow", 48f);
        CreateRowLabel(resRow, "해상도", 180f);
        TMP_Dropdown resolutionDropdown = CreateDropdown(resRow);
        Button applyButton = CreateButton(resRow, "ApplyButton", "적용", 22f);
        SetRowControlSize(applyButton.gameObject, 140f, 40f);

        RectTransform fullRow = CreateRow(content.transform, "FullscreenRow", 40f);
        CreateRowLabel(fullRow, "전체 화면", 180f);
        Toggle fullscreenToggle = CreateToggle(fullRow);

        // ----- 사운드 -----
        CreateSectionLabel(content.transform, "사운드");

        Slider masterSlider = CreateVolumeRow(content.transform, "MasterRow", "전체 음량");
        Slider bgmSlider = CreateVolumeRow(content.transform, "BgmRow", "배경음 음량");
        Slider sfxSlider = CreateVolumeRow(content.transform, "SfxRow", "효과음 음량");

        // ----- 키 설정 -----
        CreateSectionLabel(content.transform, "키 설정");

        var keyContainer = new GameObject("KeyBindContainer", typeof(RectTransform));
        keyContainer.transform.SetParent(content.transform, false);
        var keyVlg = keyContainer.AddComponent<VerticalLayoutGroup>();
        keyVlg.spacing = 6f;
        keyVlg.childControlWidth = true;
        keyVlg.childControlHeight = true;
        keyVlg.childForceExpandWidth = true;
        keyVlg.childForceExpandHeight = false;
        var keyLe = keyContainer.AddComponent<LayoutElement>();
        keyLe.preferredHeight = 330f;

        GameObject keyRowTemplate = CreateKeyRowTemplate(keyContainer.transform);

        // ----- 하단 버튼 -----
        RectTransform bottomRow = CreateRow(content.transform, "BottomRow", 56f);
        var bottomHlg = bottomRow.GetComponent<HorizontalLayoutGroup>();
        bottomHlg.childAlignment = TextAnchor.MiddleCenter;
        bottomHlg.spacing = 24f;
        Button resetKeysButton = CreateButton(bottomRow, "ResetKeysButton", "기본 키로 초기화", 22f);
        SetRowControlSize(resetKeysButton.gameObject, 250f, 50f);
        Button closeButton = CreateButton(bottomRow, "CloseButton", "닫기", 22f);
        SetRowControlSize(closeButton.gameObject, 250f, 50f);

        // 컴포넌트 연결
        var panel = root.AddComponent<OptionsPanel>();
        panel.resolutionDropdown = resolutionDropdown;
        panel.fullscreenToggle = fullscreenToggle;
        panel.applyResolutionButton = applyButton;
        panel.masterSlider = masterSlider;
        panel.bgmSlider = bgmSlider;
        panel.sfxSlider = sfxSlider;
        panel.keyBindContainer = keyContainer.transform;
        panel.keyBindRowTemplate = keyRowTemplate;
        panel.resetKeysButton = resetKeysButton;
        panel.closeButton = closeButton;

        root.SetActive(false);
        return panel;
    }

    static Slider CreateVolumeRow(Transform parent, string rowName, string label)
    {
        RectTransform row = CreateRow(parent, rowName, 36f);
        CreateRowLabel(row, label, 180f);
        GameObject sliderGo = DefaultControls.CreateSlider(Res());
        sliderGo.transform.SetParent(row, false);
        var le = sliderGo.AddComponent<LayoutElement>();
        le.flexibleWidth = 1f;
        var slider = sliderGo.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 1f;
        return slider;
    }

    static GameObject CreateKeyRowTemplate(Transform parent)
    {
        var row = new GameObject("KeyRowTemplate", typeof(RectTransform));
        row.transform.SetParent(parent, false);
        var hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 12f;
        hlg.childControlWidth = true;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        var le = row.AddComponent<LayoutElement>();
        le.preferredHeight = 42f;

        TextMeshProUGUI label = CreateText(row.transform, "Label", "액션", 24f,
            TextAlignmentOptions.Left, LabelColor, FontStyles.Normal);
        var labelLe = label.gameObject.AddComponent<LayoutElement>();
        labelLe.preferredWidth = 320f;
        labelLe.flexibleWidth = 1f;
        label.rectTransform.sizeDelta = new Vector2(320f, 32f);

        Button keyButton = CreateButton(row.transform, "KeyButton", "Key", 20f);
        SetRowControlSize(keyButton.gameObject, 240f, 38f);

        row.SetActive(false);
        return row;
    }

    // =========================================================
    // 공용 헬퍼
    // =========================================================

    static DefaultControls.Resources Res()
    {
        var res = new DefaultControls.Resources();
        res.standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        res.background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        res.inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd");
        res.knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        res.checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd");
        res.dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/DropdownArrow.psd");
        res.mask = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd");
        return res;
    }

    static TMP_DefaultControls.Resources TmpRes()
    {
        var res = new TMP_DefaultControls.Resources();
        res.standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        res.background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        res.inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd");
        res.knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        res.checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd");
        res.dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/DropdownArrow.psd");
        res.mask = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd");
        return res;
    }

    static Camera CreateCamera(Color background, bool solidColor)
    {
        var go = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        go.tag = "MainCamera";
        var cam = go.GetComponent<Camera>();
        if (solidColor)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = background;
        }
        return cam;
    }

    static void CreateEventSystem()
    {
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    static GameObject CreateCanvas()
    {
        var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        return go;
    }

    /// <summary>화면 전체를 덮는 반투명 오버레이(뒤쪽 클릭 차단).</summary>
    static GameObject CreateOverlay(Transform parent, string name)
    {
        GameObject go = DefaultControls.CreatePanel(Res());
        go.name = name;
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = DimColor;
        img.sprite = null;
        return go;
    }

    static Image CreatePanel(Transform parent, string name, Color color)
    {
        GameObject go = DefaultControls.CreatePanel(Res());
        go.name = name;
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = color;
        return img;
    }

    static void SetCenter(RectTransform rt, Vector2 size)
    {
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = size;
    }

    static TextMeshProUGUI CreateText(Transform parent, string name, string content, float fontSize,
        TextAlignmentOptions alignment, Color color, FontStyles style)
    {
        GameObject go = TMP_DefaultControls.CreateText(TmpRes());
        go.name = name;
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<TextMeshProUGUI>();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.fontStyle = style;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Overflow;
        return text;
    }

    static Button CreateButton(Transform parent, string name, string label, float fontSize)
    {
        GameObject go = TMP_DefaultControls.CreateButton(TmpRes());
        go.name = name;
        go.transform.SetParent(parent, false);
        var text = go.GetComponentInChildren<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = fontSize;
        return go.GetComponent<Button>();
    }

    static Button CreateMenuButton(Transform parent, string name, string label)
    {
        Button button = CreateButton(parent, name, label, 30f);
        var le = button.gameObject.AddComponent<LayoutElement>();
        le.preferredHeight = 72f;
        return button;
    }

    static RectTransform CreateRow(Transform parent, string name, float height)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var hlg = go.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 12f;
        hlg.childControlWidth = true;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = height;
        return (RectTransform)go.transform;
    }

    static TextMeshProUGUI CreateRowLabel(Transform row, string content, float width)
    {
        TextMeshProUGUI label = CreateText(row, "Label", content, 24f,
            TextAlignmentOptions.Left, LabelColor, FontStyles.Normal);
        var le = label.gameObject.AddComponent<LayoutElement>();
        le.preferredWidth = width;
        label.rectTransform.sizeDelta = new Vector2(width, 32f);
        return label;
    }

    static void CreateSectionLabel(Transform parent, string content)
    {
        TextMeshProUGUI label = CreateText(parent, "Section_" + content, content, 26f,
            TextAlignmentOptions.Left, SectionColor, FontStyles.Bold);
        var le = label.gameObject.AddComponent<LayoutElement>();
        le.preferredHeight = 34f;
    }

    static TMP_Dropdown CreateDropdown(Transform row)
    {
        GameObject go = TMP_DefaultControls.CreateDropdown(TmpRes());
        go.transform.SetParent(row, false);
        var le = go.AddComponent<LayoutElement>();
        le.flexibleWidth = 1f;
        var rt = (RectTransform)go.transform;
        rt.sizeDelta = new Vector2(rt.sizeDelta.x, 40f);
        var dropdown = go.GetComponent<TMP_Dropdown>();
        dropdown.ClearOptions(); // 기본 Option A/B/C 제거 (런타임에 해상도 목록으로 채움)
        dropdown.captionText.fontSize = 22f;
        if (dropdown.itemText != null)
            dropdown.itemText.fontSize = 20f;
        return dropdown;
    }

    static Toggle CreateToggle(Transform row)
    {
        GameObject go = DefaultControls.CreateToggle(Res());
        go.transform.SetParent(row, false);
        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth = 40f;
        var rt = (RectTransform)go.transform;
        rt.sizeDelta = new Vector2(40f, 28f);

        // 토글 자체 라벨은 제거하고 체크박스를 키운다
        var toggle = go.GetComponent<Toggle>();
        Transform labelTr = go.transform.Find("Label");
        if (labelTr != null)
            Object.DestroyImmediate(labelTr.gameObject);
        var bg = (RectTransform)go.transform.Find("Background");
        if (bg != null)
        {
            bg.sizeDelta = new Vector2(28f, 28f);
            var check = (RectTransform)bg.Find("Checkmark");
            if (check != null)
                check.sizeDelta = new Vector2(24f, 24f);
        }
        return toggle;
    }

    static void SetRowControlSize(GameObject go, float width, float height)
    {
        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth = width;
        var rt = (RectTransform)go.transform;
        rt.sizeDelta = new Vector2(width, height);
    }
}
