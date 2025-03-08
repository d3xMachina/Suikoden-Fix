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
    public static ModComponent Instance { get; private set; }
    private bool _isDisabled;

    private bool _isGamepadSelectPressed = false;
    private bool _isGamepadSelectDown = false;
    private bool _wasSpeedHackPressed = false;
    private bool _speedHackToggle = false;

    private string _sceneName = "";

    public bool InvertDash = false;
    public bool LastDash = false;

    public uint LastPadData = 0;
    public uint LastPadDataSanitized = 0;

    public Patches.TransitionState transition = Patches.TransitionState.None;
    public bool FrameSkip = true;

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

    public void LateUpdate()
    {
        try
        {
            if (_isDisabled)
            {
                return;
            }

            var scene = SceneManager.GetActiveScene();
            _sceneName = scene.name;

            UpdateInputs();
            UpdateSaveAnywhere();
            UpdateFrameSkip();
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

        if (gamepad == null || !gamepad.selectButton.isPressed)
        {
            _isGamepadSelectDown = false;
            _isGamepadSelectPressed = false;
        }
        else
        {
            if (!_isGamepadSelectPressed)
            {
                _isGamepadSelectDown = true;
                _isGamepadSelectPressed = true;
            }
            else
            {
                _isGamepadSelectDown = false;
            }
        }
        
    }

    private void UpdateSaveAnywhere()
    {
        if (!Plugin.Config.SaveAnywhere.Value)
        {
            return;
        }

        const int slot = 16; // last slot

        if (_isGamepadSelectDown || GRInputManager.IsKeyDown(Key.F1))
        {
            var success = false;

            if (_sceneName == "GSD1")
            {
                var partyData = GSD1.GlobalWork.Instance?.game_work?.party_data;

                if (partyData != null)
                {
                    var areaId = GSD1.VillageManager.GetAreaIndex(partyData.area_no, partyData.vil_no);

                    if (areaId != 10) // Title screen
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
                Plugin.Log.LogWarning("Cannot save in this scene!");
            }

            if (success)
            {
                SoundManager.PlaySE("SD_WOP");
                Plugin.Log.LogInfo("Game saved!");
            }
        }
    }

    private void SetFrameSkip(int factor)
    {
        if (Plugin.Config.SpeedHackFactor.Value <= 1)
        {
            return;
        }

        if (_sceneName == "GSD1")
        {
            var gr1Instance = GSD1.ChapterManager.GR1Instance;
            if (gr1Instance != null)
            {
                gr1Instance.frameSkip = factor;
            }
        }
        else if (_sceneName == "GSD2")
        {
            var grInstance = GSD2.GRChapterManager.GRInstance;
            if (grInstance != null)
            {
                grInstance.BattleFrameSkip = factor;
            }
        }
    }

    private void UpdateSpeedHack()
    {
        if (Plugin.Config.SpeedHackFactor.Value <= 1)
        {
            return;
        }

        int factor = 1;

        if (FrameSkip)
        {
            bool isPressed = GRInputManager.IsPress(GRInputManager.Type.R2) || GRInputManager.IsKeyPress(Key.T);

            if (isPressed && !_wasSpeedHackPressed)
            {
                _speedHackToggle = !_speedHackToggle;
            }

            factor = _speedHackToggle ? Plugin.Config.SpeedHackFactor.Value : 1;
            _wasSpeedHackPressed = isPressed;
        }

        SetFrameSkip(factor);

        // Show speed icon in bottom right
        if (_sceneName == "GSD1")
        {
            var uiBattleManager = GSD1.UIBattleManager.Instance;
            if (uiBattleManager != null)
            {
                if (factor > 1)
                {
                    uiBattleManager.ShowSpeedIconUI(1);
                }
                else
                {
                    uiBattleManager.HideSpeedIconUI();
                }
            }
        }
        else if (_sceneName == "GSD2")
        {
            var uiBattleManager = GSD2.UIBattleManager.Instance;
            if (uiBattleManager != null)
            {
                if (factor > 1)
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

    private void UpdateFrameSkip()
    {
        FrameSkip = true;

        // Avoid skipping frames on menus to avoid skipped inputs

        if (_sceneName == "GSD1")
        {
            var windowManager = GSD1.WindowManager.Instance;
            if (windowManager != null)
            {
                var menuWindow = windowManager.GetMenuWindow();
                if (menuWindow != null && menuWindow.IsOpen)
                {
                    FrameSkip = false;
                }
            }

            var chapterManager = GSD1.ChapterManager.GR1Instance;
            if (chapterManager != null)
            {
                var chapter = chapterManager.activeChapter;
                if (chapter != null && (chapter is GSD1.TitleChapter))
                {
                    FrameSkip = false;
                }
            }
        }
        else if (_sceneName == "GSD2")
        {
            var windowManager = GSD2.WindowManager.Instance;
            if (windowManager != null)
            {
                var menuWindow = windowManager.GetMenuWindow();
                if (menuWindow != null && menuWindow.IsOpen)
                {
                    FrameSkip = false;
                }
            }

            var chapterManager = GSD2.GRChapterManager.GRInstance;
            if (chapterManager != null)
            {
                var chapter = chapterManager.activeChapter;
                if (chapter != null && chapter.TryCast<GSD2.TitleChapter>() != null)
                {
                    FrameSkip = false;
                }
            }
        }
    }
}
