using System.Collections.Generic;
using UnityEngine;

/// <summary>키 설정으로 관리되는 게임 액션 목록.</summary>
public enum GameAction
{
    Pause,
    MoveUp,
    MoveDown,
    MoveLeft,
    MoveRight,
    Jump,
    Interact,
}

/// <summary>
/// 액션 → 키 매핑을 관리하고 PlayerPrefs 에 저장/로드하는 정적 클래스.
/// 게임플레이 코드에서는 Input.GetKey 대신 KeyBindings.GetKey(GameAction.Jump) 식으로 사용한다.
/// </summary>
public static class KeyBindings
{
    const string PrefPrefix = "keybind_";

    static readonly Dictionary<GameAction, KeyCode> Defaults = new Dictionary<GameAction, KeyCode>
    {
        { GameAction.Pause,     KeyCode.Escape },
        { GameAction.MoveUp,    KeyCode.W },
        { GameAction.MoveDown,  KeyCode.S },
        { GameAction.MoveLeft,  KeyCode.A },
        { GameAction.MoveRight, KeyCode.D },
        { GameAction.Jump,      KeyCode.Space },
        { GameAction.Interact,  KeyCode.E },
    };

    static readonly Dictionary<GameAction, KeyCode> Current = new Dictionary<GameAction, KeyCode>(Defaults);

    public static IEnumerable<GameAction> AllActions => Defaults.Keys;

    public static KeyCode Get(GameAction action) => Current[action];

    public static void Set(GameAction action, KeyCode key) => Current[action] = key;

    public static bool GetKey(GameAction action) => Input.GetKey(Current[action]);

    public static bool GetKeyDown(GameAction action) => Input.GetKeyDown(Current[action]);

    public static bool GetKeyUp(GameAction action) => Input.GetKeyUp(Current[action]);

    public static string DisplayName(GameAction action)
    {
        switch (action)
        {
            case GameAction.Pause:     return "일시정지";
            case GameAction.MoveUp:    return "위로 이동";
            case GameAction.MoveDown:  return "아래로 이동";
            case GameAction.MoveLeft:  return "왼쪽 이동";
            case GameAction.MoveRight: return "오른쪽 이동";
            case GameAction.Jump:      return "점프";
            case GameAction.Interact:  return "상호작용";
            default:                   return action.ToString();
        }
    }

    public static void ResetToDefaults()
    {
        foreach (var pair in Defaults)
            Current[pair.Key] = pair.Value;
        Save();
    }

    public static void Load()
    {
        foreach (var pair in Defaults)
        {
            int saved = PlayerPrefs.GetInt(PrefPrefix + pair.Key, (int)pair.Value);
            Current[pair.Key] = (KeyCode)saved;
        }
    }

    public static void Save()
    {
        foreach (var pair in Current)
            PlayerPrefs.SetInt(PrefPrefix + pair.Key, (int)pair.Value);
        PlayerPrefs.Save();
    }
}
