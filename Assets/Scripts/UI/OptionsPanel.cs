using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 옵션 패널(해상도/전체화면, 음량, 키 설정).
/// 메인메뉴 씬과 인게임 일시정지 메뉴에서 공용으로 사용한다.
/// </summary>
public class OptionsPanel : MonoBehaviour
{
    [Header("화면")]
    public TMP_Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    public Button applyResolutionButton;

    [Header("사운드")]
    public Slider masterSlider;
    public Slider bgmSlider;
    public Slider sfxSlider;

    [Header("키 설정")]
    public Transform keyBindContainer;
    public GameObject keyBindRowTemplate; // 비활성 템플릿: "Label"(Text) + "KeyButton"(Button)
    public Button resetKeysButton;

    [Header("공통")]
    public Button closeButton;

    /// <summary>패널이 닫힐 때 호출됨(일시정지 메뉴 복귀 등에 사용).</summary>
    public event Action Closed;

    /// <summary>키 입력 대기(리바인딩) 중이면 true. 이때 일시정지 토글 등은 무시해야 한다.</summary>
    public static bool IsRebinding { get; private set; }

    /// <summary>리바인딩이 끝난(확정/취소) 프레임. 같은 프레임의 Escape 입력 중복 처리를 막는다.</summary>
    public static int LastRebindFrame { get; private set; } = -1;

    readonly List<Resolution> resolutions = new List<Resolution>();
    readonly Dictionary<GameAction, TMP_Text> keyButtonLabels = new Dictionary<GameAction, TMP_Text>();
    static readonly Array AllKeyCodes = Enum.GetValues(typeof(KeyCode));

    GameAction rebindTarget;
    TMP_Text rebindLabel;
    bool initialized;

    void Awake()
    {
        Init();
    }

    void Init()
    {
        if (initialized)
            return;
        initialized = true;

        BuildResolutionList();
        BuildKeyRows();

        masterSlider.onValueChanged.AddListener(v => { GameSettings.MasterVolume = v; GameSettings.ApplyVolume(); });
        bgmSlider.onValueChanged.AddListener(v => { GameSettings.BgmVolume = v; GameSettings.ApplyVolume(); });
        sfxSlider.onValueChanged.AddListener(v => { GameSettings.SfxVolume = v; GameSettings.ApplyVolume(); });

        applyResolutionButton.onClick.AddListener(ApplyResolution);
        resetKeysButton.onClick.AddListener(ResetKeys);
        closeButton.onClick.AddListener(Close);
    }

    public void Open()
    {
        Init();
        RefreshUI();
        gameObject.SetActive(true);
    }

    public void Close()
    {
        CancelRebind();
        GameSettings.Save();
        gameObject.SetActive(false);
        Closed?.Invoke();
    }

    void Update()
    {
        if (!IsRebinding || !Input.anyKeyDown)
            return;

        // Escape 는 리바인딩 취소 키
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelRebind();
            return;
        }

        foreach (KeyCode key in AllKeyCodes)
        {
            if (key == KeyCode.None || key == KeyCode.Escape)
                continue;
            // 마우스 클릭은 UI 조작과 겹치므로 바인딩에서 제외
            if (key >= KeyCode.Mouse0 && key <= KeyCode.Mouse6)
                continue;
            if (Input.GetKeyDown(key))
            {
                CompleteRebind(key);
                return;
            }
        }
    }

    // ---------- 화면 ----------

    void BuildResolutionList()
    {
        resolutions.Clear();
        foreach (Resolution res in Screen.resolutions)
        {
            bool exists = false;
            foreach (Resolution r in resolutions)
            {
                if (r.width == res.width && r.height == res.height)
                {
                    exists = true;
                    break;
                }
            }
            if (!exists)
                resolutions.Add(res);
        }
        if (resolutions.Count == 0)
        {
            resolutions.Add(new Resolution { width = Screen.width, height = Screen.height });
        }

        resolutionDropdown.ClearOptions();
        var options = new List<string>();
        foreach (Resolution res in resolutions)
            options.Add($"{res.width} x {res.height}");
        resolutionDropdown.AddOptions(options);
    }

    int CurrentResolutionIndex()
    {
        for (int i = 0; i < resolutions.Count; i++)
        {
            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
                return i;
        }
        return resolutions.Count - 1;
    }

    void ApplyResolution()
    {
        Resolution res = resolutions[Mathf.Clamp(resolutionDropdown.value, 0, resolutions.Count - 1)];
        GameSettings.SetResolution(res.width, res.height, fullscreenToggle.isOn);
    }

    // ---------- 키 설정 ----------

    void BuildKeyRows()
    {
        foreach (GameAction action in KeyBindings.AllActions)
        {
            GameObject row = Instantiate(keyBindRowTemplate, keyBindContainer);
            row.name = "KeyRow_" + action;
            row.SetActive(true);

            row.transform.Find("Label").GetComponent<TMP_Text>().text = KeyBindings.DisplayName(action);

            var button = row.transform.Find("KeyButton").GetComponent<Button>();
            TMP_Text buttonLabel = button.GetComponentInChildren<TMP_Text>();
            keyButtonLabels[action] = buttonLabel;

            GameAction captured = action;
            button.onClick.AddListener(() => StartRebind(captured, buttonLabel));
        }
        RefreshKeyLabels();
    }

    void StartRebind(GameAction action, TMP_Text label)
    {
        if (IsRebinding)
            return;
        IsRebinding = true;
        rebindTarget = action;
        rebindLabel = label;
        label.text = "아무 키나 누르세요...";
    }

    void CompleteRebind(KeyCode key)
    {
        // 이미 다른 액션에 할당된 키면 서로 교환한다
        foreach (GameAction other in KeyBindings.AllActions)
        {
            if (other != rebindTarget && KeyBindings.Get(other) == key)
            {
                KeyBindings.Set(other, KeyBindings.Get(rebindTarget));
                break;
            }
        }
        KeyBindings.Set(rebindTarget, key);
        KeyBindings.Save();
        EndRebind();
    }

    void CancelRebind()
    {
        if (!IsRebinding)
            return;
        EndRebind();
    }

    void EndRebind()
    {
        IsRebinding = false;
        LastRebindFrame = Time.frameCount;
        rebindLabel = null;
        RefreshKeyLabels();
    }

    void ResetKeys()
    {
        CancelRebind();
        KeyBindings.ResetToDefaults();
        RefreshKeyLabels();
    }

    void RefreshKeyLabels()
    {
        foreach (var pair in keyButtonLabels)
            pair.Value.text = KeyBindings.Get(pair.Key).ToString();
    }

    // ---------- 공통 ----------

    void RefreshUI()
    {
        masterSlider.SetValueWithoutNotify(GameSettings.MasterVolume);
        bgmSlider.SetValueWithoutNotify(GameSettings.BgmVolume);
        sfxSlider.SetValueWithoutNotify(GameSettings.SfxVolume);
        fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
        resolutionDropdown.SetValueWithoutNotify(CurrentResolutionIndex());
        resolutionDropdown.RefreshShownValue();
        RefreshKeyLabels();
    }
}
