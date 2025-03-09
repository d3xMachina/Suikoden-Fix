extern alias GSD1;
extern alias GSD2;

using System;
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

    private bool _isSaveAnywhere = false;
    private bool _wasSelectPressed = false;
    private bool _wasSpeedHackPressed = false;
    private bool _speedHackToggle = false;
    private bool _speedHackEnabled = true;

    private string _sceneName = "";
    private Game _activeGame = Game.None;
    private Chapter _chapter = Chapter.None;
    private Chapter _prevChapter = Chapter.None;

    public bool InvertDash = false;
    public bool LastDash = false;

    public uint LastPadData = 0;
    public uint LastPadDataSanitized = 0;

    public Patches.TransitionState transition = Patches.TransitionState.None;

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
            UpdateSpeedHack();

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
        if (isSelectPressed && !_wasSelectPressed)
        {
            _isSaveAnywhere = true;
        }
        else
        {
            _isSaveAnywhere = false;
        }

        _wasSelectPressed = isSelectPressed;
        
        if (!Plugin.Config.NoSpeedHackInBattle.Value || _prevChapter != Chapter.Battle)
        {
            bool isPressed =  (gamepad?.rightTrigger.isPressed ?? false) || GRInputManager.IsKeyPress(Key.T);
            if (isPressed && !_wasSpeedHackPressed)
            {
                _speedHackToggle = !_speedHackToggle;
            }

            _wasSpeedHackPressed = isPressed;
        }
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

    private void SetSpeedIcon(bool show)
    {
        // Show speed icon in bottom right
        if (_activeGame == Game.GSD1)
        {
            var uiBattleManager = GSD1.UIBattleManager.Instance;
            if (uiBattleManager != null)
            {
                if (show)
                {
                    uiBattleManager.ShowSpeedIconUI(1);
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
                if (show)
                {
                    uiBattleManager.ShowSpeedIconUI(1);
                }
                else
                {
                    uiBattleManager.HideSpeedIconUI();
                }
            }
        }
    }

    private void UpdateSpeedHack()
    {
        if (Plugin.Config.SpeedHackFactor.Value <= 1)
        {
            return;
        }

        UpdateSpeedHackState();

        int factor = 1;
        var pitchType = SoundManager.PitchType.x1;

        if (_speedHackEnabled)
        {
            if (_speedHackToggle)
            {
                factor = Plugin.Config.SpeedHackFactor.Value;
                pitchType = SoundManager.PitchType.x3;
            }
            else
            {
                factor = 1;
                pitchType = SoundManager.PitchType.x1;
            }
        }

        if (_activeGame != Game.None)
        {
            // Reset to x1 when entering battle with NoSpeedHackInBattle
            if (!Plugin.Config.NoSpeedHackInBattle.Value ||
                _chapter != Chapter.Battle ||
                _prevChapter != Chapter.Battle)
            {
                SetFrameSkip(factor);
                SoundManager.SetPitchType(pitchType);
                SetSpeedIcon(factor > 1);
            }
        }
    }

    private void UpdateSpeedHackState()
    {
        _speedHackEnabled = true;

        // Avoid skipping frames on menus to avoid skipped inputs

        if ((Plugin.Config.NoSpeedHackInBattle.Value && _chapter == Chapter.Battle) ||
            _chapter == Chapter.Title ||
            _chapter == Chapter.GameOver)
        {
            _speedHackEnabled = false;
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
                    _speedHackEnabled = false;
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
                    _speedHackEnabled = false;
                }
            }
        }
    }
}
