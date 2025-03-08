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

    public static ModComponent Instance { get; private set; }
    private bool _isDisabled;

    private bool _isSaveAnywhere = false;
    private bool _wasSelectPressed = false;
    private bool _wasSpeedHackPressed = false;
    private bool _speedHackToggle = false;
    private bool _speedHackEnabled = true;

    private string _sceneName = "";
    private Game _activeGame = Game.None;

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
        }
        catch (Exception e)
        {
            _isDisabled = true;
            Plugin.Log.LogError($"[{nameof(ModComponent)}].{nameof(LateUpdate)}(): {e}");
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
        
        bool isPressed = GRInputManager.IsPress(GRInputManager.Type.R2) || GRInputManager.IsKeyPress(Key.T);
        if (isPressed && !_wasSpeedHackPressed)
        {
            _speedHackToggle = !_speedHackToggle;
        }

        _wasSpeedHackPressed = isPressed;
    }

    private void UpdateSaveAnywhere()
    {
        if (!Plugin.Config.SaveAnywhere.Value || !_isSaveAnywhere)
        {
            return;
        }

        const int slot = 16; // last slot
        var success = false;

        if (_sceneName == "GSD1")
        {
            var partyData = GSD1.GlobalWork.Instance?.game_work?.party_data;

            if (partyData != null)
            {
                var areaId = GSD1.VillageManager.GetAreaIndex(partyData.area_no, partyData.vil_no);

                if (areaId != 10) // Title screen, TODO: change this, it is the starting area castle
                {
                    GSD1.UISaveLoad1.Save(slot, null);
                    success = true;
                }
            }
                
        }
        else if (_sceneName == "GSD2")
        {
            var machicon = GSD2.GAME_WORK.Instance?.sys_work?.mcon;

            if (machicon != null)
            {
                //var areaId = GSD2.MachiLoader.GetGlobalMapID(machicon.ano, machicon.vno, machicon.mno);
                GSD2.UISaveLoad2.Save(slot, null);
                success = true;
            }
        }
        else
        {
            SoundManager.PlaySE("SD_BUZZER");
            Plugin.Log.LogWarning("Cannot save in this scene!");
        }

        if (success)
        {
            SoundManager.PlaySE("SD_WOP");
            Plugin.Log.LogInfo("Game saved!");
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
            SetFrameSkip(factor);
            SoundManager.SetPitchType(pitchType);
            SetSpeedIcon(factor > 1);
        }
    }

    private void UpdateSpeedHackState()
    {
        _speedHackEnabled = true;

        // Avoid skipping frames on menus to avoid skipped inputs

        if (_activeGame == Game.GSD1)
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

            var chapterManager = GSD1.ChapterManager.GR1Instance;
            if (chapterManager != null)
            {
                var chapter = chapterManager.activeChapter;
                if (chapter != null && (chapter is GSD1.TitleChapter))
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

            var chapterManager = GSD2.GRChapterManager.GRInstance;
            if (chapterManager != null)
            {
                var chapter = chapterManager.activeChapter;
                if (chapter != null && chapter.TryCast<GSD2.TitleChapter>() != null)
                {
                    _speedHackEnabled = false;
                }
            }
        }
    }
}
