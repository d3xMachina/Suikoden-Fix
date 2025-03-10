﻿extern alias GSD1;
extern alias GSD2;

using System;
using BepInEx;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Suikoden_Fix;

public sealed class ModComponent : MonoBehaviour
{
    private enum Game
    {
        None,
        GSD1,
        GSD2
    }

    private enum Chapter
    {
        None,
        Unknown,
        Title,
        Map,
        Battle,
        GameOver
    }

    public static ModComponent Instance { get; private set; }
    private bool _isDisabled;

    private bool _wasSelectPressed = false;
    private bool _wasSpeedHackPressed = false;
    private bool _wasBattleSpeedPressed = false;

    private bool _isSaveAnywhere = false;
    private bool _speedHackToggle = false;
    private bool _speedHackChange = false;
    private bool _battleSpeedChange = false;
    private int _battleSpeed = 0;

    private string _sceneName = "";
    private Game _activeGame = Game.None;
    private Chapter _chapter = Chapter.None;
    private Chapter _prevChapter = Chapter.None;

    public bool InvertDash = false;
    public bool LastDash = false;
    public uint LastPadData = 0;
    public uint LastPadDataSanitized = 0;

    public Patches.TransitionState transition = Patches.TransitionState.None;
    public bool IsEventMsgMWOpen = false;
    public bool IsInShop = false;

    public Color? WindowBGColor = null;

    public ModComponent(IntPtr ptr) : base(ptr) { }

    public static bool Inject()
    {
        ClassInjector.RegisterTypeInIl2Cpp<ModComponent>();
        var name = typeof(ModComponent).FullName;

        Plugin.Log.LogInfo($"Initializing game object {name}");
        var modObject = new GameObject(name)
        {
            hideFlags = HideFlags.HideAndDontSave
        };
        GameObject.DontDestroyOnLoad(modObject);

        Plugin.Log.LogInfo($"Adding {name} to game object...");
        ModComponent component = modObject.AddComponent<ModComponent>();
        if (component == null)
        {
            GameObject.Destroy(modObject);
            Plugin.Log.LogError($"The game object is missing the required component: {name}");
            return false;
        }

        return true;
    }

    public void Awake()
    {
        try
        {
            Instance = this;

            var colorStr = Plugin.Config.WindowBGColor.Value;
            if (!colorStr.IsNullOrWhiteSpace())
            {
                if (ColorUtility.TryParseHtmlString(colorStr, out var color))
                {
                    WindowBGColor = color;
                }
                else
                {
                    Plugin.Log.LogWarning($"Invalid color for WindowBGColor: {colorStr}");
                }
            }

            Plugin.Log.LogInfo($"[{nameof(ModComponent)}].{nameof(Awake)}: Processed successfully.");
        }
        catch (Exception e)
        {
            _isDisabled = true;
            Plugin.Log.LogError($"[{nameof(ModComponent)}].{nameof(Awake)}(): {e}");
        }
    }

    public void Update()
    {
        try
        {
            if (_isDisabled)
            {
                return;
            }

            UpdateGameState();
            UpdateInputs();
        }
        catch (Exception e)
        {
            _isDisabled = true;
            Plugin.Log.LogError($"[{nameof(ModComponent)}].{nameof(Update)}(): {e}");
        }
    }

    public void LateUpdate()
    {
        try
        {
            if (_isDisabled)
            {
                return;
            }

            UpdateSaveAnywhere();
            UpdateGameSpeed();

            _prevChapter = _chapter;
        }
        catch (Exception e)
        {
            _isDisabled = true;
            Plugin.Log.LogError($"[{nameof(ModComponent)}].{nameof(LateUpdate)}(): {e}");
        }
    }

    private void UpdateGameState()
    {
        var scene = SceneManager.GetActiveScene();
        _sceneName = scene.name;

        if (_sceneName == "GSD1")
        {
            _activeGame = Game.GSD1;
        }
        else if (_sceneName == "GSD2")
        {
            _activeGame = Game.GSD2;
        }
        else if (_sceneName == "Main")
        {
            _activeGame = Game.None;
        }

        if (_activeGame == Game.GSD1)
        {
            var chapter = GSD1.ChapterManager.GR1Instance?.activeChapter;
            if (chapter != null)
            {
                if (chapter.TryCast<GSD1.TitleChapter>() != null)
                {
                    _chapter = Chapter.Title;
                }
                else if (chapter.TryCast<GSD1.BattleChapter>() != null || chapter.TryCast<GSD1.WarChapter>() != null)
                {
                    _chapter = Chapter.Battle;
                }
                else if (chapter.TryCast<GSD1.MapChapter>() != null || chapter.TryCast<GSD1.FieldChapter>() != null)
                {
                    _chapter = Chapter.Map;
                }
                else if (chapter.TryCast<GSD1.GameOverChapter>() != null)
                {
                    _chapter = Chapter.GameOver;
                }
                else
                {
                    _chapter = Chapter.Unknown;
                }
            }
        }
        else if (_activeGame == Game.GSD2)
        {
            var chapter = GSD2.GRChapterManager.GRInstance?.activeChapter;
            if (chapter != null)
            {
                if (chapter.TryCast<GSD2.TitleChapter>() != null)
                {
                    _chapter = Chapter.Title;
                }
                else if (chapter.TryCast<GSD2.BattleChapter>() != null || chapter.TryCast<GSD2.WarChapter>() != null)
                {
                    _chapter = Chapter.Battle;
                }
                else if (chapter.TryCast<GSD2.MapChapter>() != null)
                {
                    _chapter = Chapter.Map;
                }
                else if (chapter.TryCast<GSD2.GameOverChapter>() != null)
                {
                    _chapter = Chapter.GameOver;
                }
                else
                {
                    _chapter = Chapter.Unknown;
                }
            }
        }
        else
        {
            _chapter = Chapter.None;
        }
    }

    private void UpdateInputs()
    {
        var gamepad = Gamepad.current;

        bool isSelectPressed = (gamepad?.selectButton.isPressed ?? false) || GRInputManager.IsKeyPress(Key.F1);
        _isSaveAnywhere = isSelectPressed && !_wasSelectPressed;
        _wasSelectPressed = isSelectPressed;
        
        bool isSpeedHackPressed =  (gamepad?.rightTrigger.isPressed ?? false) || GRInputManager.IsKeyPress(Key.T);
        _speedHackChange = isSpeedHackPressed && !_wasSpeedHackPressed;
        _wasSpeedHackPressed = isSpeedHackPressed;

        bool isBattleSpeedPressed = GRInputManager.IsPress(GRInputManager.Type.BattleSpeed);
        _battleSpeedChange = isBattleSpeedPressed && !_wasBattleSpeedPressed;
        _wasBattleSpeedPressed = isBattleSpeedPressed;
    }

    private void UpdateSaveAnywhere()
    {
        if (!Plugin.Config.SaveAnywhere.Value || !_isSaveAnywhere)
        {
            return;
        }

        const int slot = 16; // last slot
        var success = false;

        if (_activeGame == Game.GSD1 && _chapter == Chapter.Map)
        {
            GSD1.UISaveLoad1.Save(slot, null);
            success = true;
                
        }
        else if (_activeGame == Game.GSD2 && _chapter == Chapter.Map)
        {
            GSD2.UISaveLoad2.Save(slot, null);
            success = true;
        }
        
        if (success)
        {
            SoundManager.PlaySE("SD_WOP");
            Plugin.Log.LogInfo("Game saved!");
        }
        else
        {
            SoundManager.PlaySE("SD_BUZZER");
            Plugin.Log.LogWarning("Cannot save here!");
        }
    }

    private void SetFrameSkip(int factor)
    {
        if (_activeGame == Game.GSD1)
        {
            var gr1Instance = GSD1.ChapterManager.GR1Instance;
            if (gr1Instance != null)
            {
                gr1Instance.frameSkip = factor;
            }
        }
        else if (_activeGame == Game.GSD2)
        {
            var grInstance = GSD2.GRChapterManager.GRInstance;
            if (grInstance != null)
            {
                grInstance.BattleFrameSkip = factor;
            }
        }
    }

    private void SetSpeedIcon(int speed)
    {
        // Show speed icon in bottom right
        if (_activeGame == Game.GSD1)
        {
            var uiBattleManager = GSD1.UIBattleManager.Instance;
            if (uiBattleManager != null)
            {
                if (speed > 0)
                {
                    uiBattleManager.ShowSpeedIconUI(speed);
                }
                else
                {
                    uiBattleManager.HideSpeedIconUI();
                }
            }
        }
        else if (_activeGame == Game.GSD2)
        {
            var uiBattleManager = GSD2.UIBattleManager.Instance;
            if (uiBattleManager != null)
            {
                if (speed > 0)
                {
                    uiBattleManager.ShowSpeedIconUI(speed);
                }
                else
                {
                    uiBattleManager.HideSpeedIconUI();
                }
            }
        }
    }

    private void UpdateGameSpeed()
    {
        bool speedHackEnabled = Plugin.Config.SpeedHackFactor.Value > 1;

        if ((!speedHackEnabled && !Plugin.Config.RememberBattleSpeed.Value) ||
            _activeGame == Game.None)
        {
            return;
        }

        int factor = 1;
        var pitchType = SoundManager.PitchType.x1;
        var speedIcon = 0;

        if (_chapter == Chapter.Battle &&
            (!speedHackEnabled || Plugin.Config.NoSpeedHackInBattle.Value || Plugin.Config.RememberBattleSpeed.Value))
        {
            if (_chapter != _prevChapter && !Plugin.Config.RememberBattleSpeed.Value)
            {
                _battleSpeed = 0;
            }
            else if (_battleSpeedChange)
            {
                _battleSpeed = (_battleSpeed + 1) % 3;
                // TODO: check when you can use battle speed 2
            }

            switch (_battleSpeed)
            {
                default:
                    factor = 1;
                    break;
                case 1:
                    factor = 2;
                    break;
                case 2:
                    factor = 4;
                    break;
            }

            pitchType = (SoundManager.PitchType)_battleSpeed;
            speedIcon = _battleSpeed;
        }
        else if (speedHackEnabled && IsSpeedHackSafe())
        {
            if (_speedHackChange || (_chapter == Chapter.Battle && _battleSpeedChange))
            {
                _speedHackToggle = !_speedHackToggle;
            }

            if (_speedHackToggle)
            {
                factor = Plugin.Config.SpeedHackFactor.Value;
                pitchType = SoundManager.PitchType.x3;
                speedIcon = 1;
            }
        }

        SetFrameSkip(factor);
        SoundManager.SetPitchType(pitchType);
        SetSpeedIcon(speedIcon);
    }

    private bool IsSpeedHackSafe()
    {
        bool safe = true;

        // Avoid skipping frames on menus to avoid skipped inputs

        if (_chapter == Chapter.Title ||
            _chapter == Chapter.GameOver ||
            IsInShop)
        {
            safe = false;
        }
        else if (_activeGame == Game.GSD1)
        {
            var windowManager = GSD1.WindowManager.Instance;
            if (windowManager != null)
            {
                var menuWindow = windowManager.GetMenuWindow();
                var minimapPanel = windowManager.GetMiniMapPanel();

                if (windowManager.GetIsOpen() ||
                    (minimapPanel != null && minimapPanel.IsWholeMapShow) ||
                    (menuWindow != null && menuWindow.IsOpen))
                {
                    safe = false;
                }
            }
        }
        else if (_activeGame == Game.GSD2)
        {
            var windowManager = GSD2.WindowManager.Instance;
            if (windowManager != null)
            {
                var menuWindow = windowManager.GetMenuWindow();
                var minimapPanel = windowManager.GetMiniMapPanel();

                if (windowManager.isUseMessageWindow ||
                    (minimapPanel != null && minimapPanel.IsWholeMapShow) ||
                    (menuWindow != null && menuWindow.IsOpen))
                {
                    safe = false;
                }
            }
        }

        return safe;
    }
}
