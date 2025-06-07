extern alias GSD1;
extern alias GSD2;

using System;
using System.Collections.Generic;
using BepInEx;
using Il2CppInterop.Runtime.Injection;
using Suikoden_Fix.Tools.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

namespace Suikoden_Fix;

public sealed class ModComponent : MonoBehaviour
{
    public enum Game
    {
        None,
        GSD1,
        GSD2
    }

    private enum Chapter
    {
        Unknown,
        Title,
        Map,
        Battle,
        War,
        Duel,
        Minigame,
        GameOver,
        Gallery
    }

    public enum CommandType
    {
        SaveAnywhere,
        SpeedHack,
        BattleSpeed,
        ExitApplication,
        ResetApplication,
        PauseGame,
        SkipMovie
    }

    public static ModComponent Instance { get; private set; }
    private bool _isDisabled;

    private readonly Dictionary<CommandType, Command> _commands = new()
    {
        { CommandType.SaveAnywhere, new Command([ GamepadButton.Select ], [ Key.F1 ], []) },
        { CommandType.SpeedHack, new Command([ GamepadButton.RightTrigger ], [ Key.T ], []) },
        { CommandType.BattleSpeed, new Command([], [], [ GRInputManager.Type.BattleSpeed ]) },
        { CommandType.ExitApplication, new Command([ GamepadButton.Start ], [ Key.Escape ], []) },
        { 
            CommandType.ResetApplication, new Command(
                [ GamepadButton.Start, GamepadButton.LeftShoulder, GamepadButton.RightShoulder ],
                [ Key.Escape, Key.R],
                [], true
            )
        },
        { CommandType.PauseGame, new Command([ GamepadButton.Start ], [ Key.Tab ], []) },
        { CommandType.SkipMovie, new Command([ GamepadButton.Select ], [ Key.Escape ], []) }
    };

    private bool _speedHackToggle = false;
    private int _battleSpeed = 0;

    private Chapter _chapter = Chapter.Unknown;
    private Chapter _prevChapter = Chapter.Unknown;

    private GUIStyle _guiCenteredStyle;
    private GUIStyle _guiTopRightStyle;
    private Texture2D _backgroundTexture;

    /******* Values manipulated by patches ******/

    // Read only
    public Game ActiveGame { get; private set; } = Game.None;
    public Color? WindowBGColor { get; private set; } = null;
    public int GameTimerMultiplier { get; private set; } = 1;
    public int GameSpeed { get; private set; } = 1;
    public bool IsMenuOpened { get; private set; } = false;
    public bool IsMessageBoxOpened { get; private set; } = false;
    public bool ResetOnExit { get; private set; } = false;
    public bool GamePaused { get; private set; } = false;
    public bool SkipScene { get; private set; } = false;

    // Read-Write
    public GSDTitleSelect.State PrevTitleSelectStep = GSDTitleSelect.State.NONE;
    public GSDTitleSelect.State TitleSelectStep = GSDTitleSelect.State.NONE;
    public bool IsInSpecialMenu = false;
    public bool IsInGameEvent = false;
    public bool IsInDanceMinigame = false;
    public bool IsInMovieGallery = false;

    /********************************************/

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

    private void Awake()
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

            _backgroundTexture = new Texture2D(1, 1);
            _backgroundTexture.hideFlags = HideFlags.HideAndDontSave;
            _backgroundTexture.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.5f));
            _backgroundTexture.Apply();

            _guiCenteredStyle = new GUIStyle()
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 48,
                fontStyle = FontStyle.Normal,
                normal = new GUIStyleState()
                {
                    textColor = Color.white
                }
            };

            _guiTopRightStyle = new GUIStyle()
            {
                alignment = TextAnchor.UpperRight,
                fontSize = 30,
                fontStyle = FontStyle.Normal,
                normal = new GUIStyleState()
                {
                    textColor = Color.white
                }
            };

            Plugin.Log.LogInfo($"[{nameof(ModComponent)}].{nameof(Awake)}: Processed successfully.");
        }
        catch (Exception e)
        {
            _isDisabled = true;
            Plugin.Log.LogError($"[{nameof(ModComponent)}].{nameof(Awake)}(): {e}");
        }
    }

    private void Update()
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

    private void LateUpdate()
    {
        try
        {
            if (_isDisabled)
            {
                return;
            }

            UpdateExitApplication();
            UpdateResetApplication();
            UpdateSaveAnywhere();
            UpdateWindowOpened();
            UpdateGameSpeed();
            UpdatePauseGame(); // needs to be after UpdateGameSpeed to have the game timer correctly paused
            UpdateSkipScene();

            _prevChapter = _chapter;
        }
        catch (Exception e)
        {
            _isDisabled = true;
            Plugin.Log.LogError($"[{nameof(ModComponent)}].{nameof(LateUpdate)}(): {e}");
        }
    }

    public void OnGUI()
    {
        if (GamePaused)
        {
            // Display "PAUSED" in the middle of the screen
            var screenRect = new Rect(0, 0, Screen.width, Screen.height);
            GUI.DrawTexture(screenRect, _backgroundTexture);
            GUI.Label(screenRect, "PAUSED", _guiCenteredStyle);

            if (IsSkippableScene())
            {
                const int Offset = 10;
                var marginRect = new Rect(screenRect.x + Offset, screenRect.y + Offset, screenRect.width - Offset * 2, screenRect.height - Offset * 2);

                var input = GRInputManager.LastInputDeviceType == GRInputManager.InputDeviceType.GamePad ? "Select button" : "Escape key";
                GUI.Label(marginRect, $"{input} : Skip", _guiTopRightStyle);
            }
        }
    }

    private void UpdateExitApplication()
    {
        if (!_commands[CommandType.ExitApplication].IsOn ||
            PrevTitleSelectStep != TitleSelectStep ||
            TitleSelectStep != GSDTitleSelect.State.SelectContent)
        {
            return;
        }

        Application.Quit();
    }

    private void UpdateResetApplication()
    {
        if (!Plugin.Config.ResetGame.Value ||
            !_commands[CommandType.ResetApplication].IsOn ||
            _chapter == Chapter.Title ||
            _chapter == Chapter.Unknown)
        {
            return;
        }

        if (GamePaused)
        {
            ResumeGame();
        } 

        if (ActiveGame == Game.GSD1)
        {
            if (IsGalleryMode())
            {
                Omake.EndEvent<GSD1.ExitChapter>();
            }
            else
            {
                ResetOnExit = true;
                Framework.Chapter.Request<GSD1.ExitChapter>();
            }
        }
        else if (ActiveGame == Game.GSD2)
        {
            if (IsGalleryMode())
            {
                Omake.EndEvent<GSD2.ExitChapter>();
            }
            else
            {
                ResetOnExit = true;
                Framework.Chapter.Request<GSD2.ExitChapter>();
            }
        }
    }

    private bool IsGalleryMode()
    {
        var omake = Omake.Instance;
        if (omake == null)
        {
            return false;
        }

        return omake.g_ending_mode != 0 || omake.g_staff_mode != 0 || omake.g_memory_mode != 0;
    }

    private bool IsGalleryMovie()
    {
        return IsInMovieGallery && IsMovieShown();
    }

    private bool IsMovieShown()
    {
        return SoundManager.IsCri() != null;
    }

    private bool IsSkippableScene()
    {
        return IsGalleryMode() || IsMovieShown();
    }

    private void PauseGame()
    {
        GamePaused = true;
        SoundManager.PauseBGM(-1, 0f);
        SoundManager.PauseSE(-1, 0f);
        SoundManager.IsCri()?.Pause(true); // Pause movie

        Time.timeScale = 0f;
    }

    private void ResumeGame()
    {
        GamePaused = false;
        SoundManager.ResumeBGM(-1, 0f);
        SoundManager.ResumeSE(-1, 0f);
        SoundManager.IsCri()?.Pause(false); // Resume movie

        // It's otherwise restored in ChapterManager.Update() with the frameSkip value
        if (ActiveGame == Game.None)
        {
            Time.timeScale = 1f;
        }
    }

    private void UpdatePauseGame()
    {
        if (!Plugin.Config.PauseGame.Value ||
            !_commands[CommandType.PauseGame].IsOn ||
            _commands[CommandType.ResetApplication].IsOn ||
            IsInDanceMinigame) // start button is used in the dance minigame to skip the practice
        {
            return;
        }

        if (!GamePaused && // for safety but shouldn't be needed, allow unpausing in any chapter
            (_chapter == Chapter.Title) ||
            (_chapter == Chapter.Unknown && !IsGalleryMovie())) // allow pause movies in the gallery
        {
            return;
        }

        if (GamePaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    private void UpdateSkipScene()
    {
        SkipScene = false;

        if (!_commands[CommandType.SkipMovie].IsOn ||
            _commands[CommandType.PauseGame].IsOn ||
            _commands[CommandType.ResetApplication].IsOn ||
            !GamePaused ||
            !IsSkippableScene())
        {
            return;
        }

        SkipScene = true;
        ResumeGame();
    }

    private void UpdateGameState()
    {
        var scene = SceneManager.GetActiveScene();
        var sceneName = scene.name;
        var prevChapter = _chapter;

        if (sceneName == "GSD1")
        {
            ActiveGame = Game.GSD1;
        }
        else if (sceneName == "GSD2")
        {
            ActiveGame = Game.GSD2;
        }
        else if (sceneName == "Main")
        {
            ActiveGame = Game.None;
        }

        if (ActiveGame == Game.GSD1)
        {
            var chapter = GSD1.ChapterManager.GR1Instance?.activeChapter;
            if (chapter != null)
            {
                if (chapter.TryCast<GSD1.MapChapter>() != null || chapter.TryCast<GSD1.FieldChapter>() != null)
                {
                    _chapter = Chapter.Map;
                }
                else if (chapter.TryCast<GSD1.BattleChapter>() != null)
                {
                    _chapter = Chapter.Battle;
                }
                else if (chapter.TryCast<GSD1.WarChapter>() != null)
                {
                    _chapter = Chapter.War;
                }
                else if (chapter.TryCast<GSD1.MinigameChapter>() != null)
                {
                    _chapter = Chapter.Minigame;
                }
                else if (chapter.TryCast<GSD1.TitleChapter>() != null)
                {
                    _chapter = Chapter.Title;
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
        else if (ActiveGame == Game.GSD2)
        {
            var chapter = GSD2.GRChapterManager.GRInstance?.activeChapter;
            if (chapter != null)
            {
                if (chapter.TryCast<GSD2.MapChapter>() != null)
                {
                    _chapter = Chapter.Map;
                }
                else if (chapter.TryCast<GSD2.BattleChapter>() != null)
                {
                    _chapter = Chapter.Battle;
                }
                else if (chapter.TryCast<GSD2.WarChapter>() != null)
                {
                    _chapter = Chapter.War;
                }
                else if (chapter.TryCast<GSD2.IkkiChapter>() != null)
                {
                    _chapter = Chapter.Duel;
                }
                else if (chapter.TryCast<GSD2.TitleChapter>() != null)
                {
                    _chapter = Chapter.Title;
                }
                else if (chapter.TryCast<GSD2.HanModeChapter>() != null)
                {
                    _chapter = Chapter.Gallery;
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
            _chapter = Chapter.Unknown;
        }

        // Init values
        if (_chapter == Chapter.Title && _chapter != prevChapter)
        {
            IsInSpecialMenu = false;
            GameTimerMultiplier = 1;
            GameSpeed = 1;
            IsMenuOpened = false;
            IsMessageBoxOpened = false;
            IsInGameEvent = false;
            IsInDanceMinigame = false;
            ResetOnExit = false;
        }
    }

    private void UpdateInputs()
    {
        foreach (var command in _commands.Values)
        {
            command.Update();
        }
    }

    private void UpdateSaveAnywhere()
    {
        // The dance minigame is the only thing that use the select button in the game
        if (!Plugin.Config.SaveAnywhere.Value ||
            !_commands[CommandType.SaveAnywhere].IsOn ||
            GamePaused ||
            IsInDanceMinigame)
        {
            return;
        }

        const int slot = 16; // last slot
        var success = false;

        if (_chapter == Chapter.Map && !IsInGameEvent)
        {
            if (ActiveGame == Game.GSD1)
            {
                var village = GSD1.GlobalWork.Instance?.village_c;
                var partyData = village?.fm_party_data;
                var player = village?.player;
                bool saveCurrentPosition = partyData != null && player != null;

                int playerX = 0;
                int playerY = 0;

                if (saveCurrentPosition)
                {
                    Plugin.Log.LogInfo("Save current position.");

                    playerX = partyData.x;
                    playerY = partyData.y;
                    partyData.x = player.map_x;
                    partyData.y = player.map_y;
                }

                GSD1.UISaveLoad1.Save(slot, null);

                if (saveCurrentPosition)
                {
                    partyData.x = playerX;
                    partyData.y = playerY;
                }

                success = true; 
            }
            else if (ActiveGame == Game.GSD2)
            {
                GSD2.UISaveLoad2.Save(slot, null);
                success = true;
            }
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

    private int GetGameSpeed()
    {
        int speed = 1;

        if (ActiveGame == Game.GSD1)
        {
            var gr1Instance = GSD1.ChapterManager.GR1Instance;
            if (gr1Instance != null)
            {
                speed = gr1Instance.frameSkip;
            }
        }
        else if (ActiveGame == Game.GSD2)
        {
            var grInstance = GSD2.GRChapterManager.GRInstance;
            if (grInstance != null)
            {
                speed = grInstance.BattleFrameSkip;
            }
        }

        return speed;
    }

    private void SetFrameSkip(int factor)
    {
        if (ActiveGame == Game.GSD1)
        {
            var gr1Instance = GSD1.ChapterManager.GR1Instance;
            if (gr1Instance != null)
            {
                gr1Instance.frameSkip = factor;
            }
        }
        else if (ActiveGame == Game.GSD2)
        {
            var grInstance = GSD2.GRChapterManager.GRInstance;
            if (grInstance != null)
            {
                grInstance.BattleFrameSkip = factor;
            }
        }

        GameSpeed = factor;
    }

    private void SetSpeedIcon(int speed)
    {
        // Show speed icon in bottom right
        if (ActiveGame == Game.GSD1)
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
        else if (ActiveGame == Game.GSD2)
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

    private bool IsStallionUnlocked()
    {
        if (Plugin.Config.StallionBoons.Value)
        {
            return true;
        }

        if (ActiveGame == Game.GSD1)
        {
            const int StallionId = 75;

            var memberFlags = GSD1.OldSrcBase.game_work?.member_flag;
            if (memberFlags != null && memberFlags.Count > StallionId)
            {
                return (memberFlags[StallionId] & 1) != 0;
            }
        }
        else if (ActiveGame == Game.GSD2)
        {
            const int StallionId = 67;

            return GSD2.G2_SYS.G2_cha_flag(2, StallionId) == 1;
        }

        return false;
    }

    private void UpdateGameSpeed()
    {
        if (ActiveGame == Game.None)
        {
            return;
        }

        bool speedHackEnabled = Plugin.Config.SpeedHackFactor.Value > 1;
        if (!speedHackEnabled && !Plugin.Config.RememberBattleSpeed.Value && !Plugin.Config.StallionBoons.Value)
        {
            GameSpeed = GetGameSpeed();
            SetGameTimerMultiplier(1);
            return;
        }

        bool speedHackChange = _commands[CommandType.SpeedHack].IsOn && !GamePaused;
        bool battleSpeedChange = _commands[CommandType.BattleSpeed].IsOn && !GamePaused;

        int factor = 1;
        var pitchType = SoundManager.PitchType.x1;
        var speedIcon = 0;

        if (IsSpeedHackAllowed())
        {
            if (_chapter == Chapter.Battle &&
                (!speedHackEnabled || Plugin.Config.NoSpeedHackInBattle.Value || Plugin.Config.RememberBattleSpeed.Value))
            {
                if (_chapter != _prevChapter && !Plugin.Config.RememberBattleSpeed.Value)
                {
                    _battleSpeed = 0;
                }
                else
                {
                    var isStallionUnlocked = IsStallionUnlocked();

                    // Can happen if you load a save with stallion then load a save without him
                    if (!isStallionUnlocked && _battleSpeed > 1)
                    {
                        _battleSpeed = 1;
                    }

                    if (battleSpeedChange)
                    {
                        int nbSpeed = isStallionUnlocked ? 3 : 2;
                        _battleSpeed = (_battleSpeed + 1) % nbSpeed;
                    }
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
            else if (speedHackEnabled)
            {
                if (speedHackChange || (_chapter == Chapter.Battle && battleSpeedChange))
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
        }

        SetFrameSkip(factor);
        SoundManager.SetPitchType(pitchType);
        SetSpeedIcon(speedIcon);
        SetGameTimerMultiplier(factor);
    }

    private void UpdateWindowOpened()
    {
        bool menuOpened = false;
        bool messageBoxOpened = false;

        if (ActiveGame == Game.GSD1)
        {
            var windowManager = GSD1.WindowManager.Instance;
            if (windowManager != null)
            {
                var menuWindow = windowManager.GetMenuWindow();
                var minimapPanel = windowManager.GetMiniMapPanel();

                if (windowManager.GetIsOpen())
                {
                    messageBoxOpened = true;
                }

                if ((minimapPanel != null && minimapPanel.IsWholeMapShow) ||
                    (menuWindow != null && menuWindow.IsOpen))
                {
                    menuOpened = true;
                }
            }
        }
        else if (ActiveGame == Game.GSD2)
        {
            var windowManager = GSD2.WindowManager.Instance;
            if (windowManager != null)
            {
                var menuWindow = windowManager.GetMenuWindow();
                var minimapPanel = windowManager.GetMiniMapPanel();

                if (windowManager.isUseMessageWindow)
                {
                    messageBoxOpened = true;
                }

                if ((minimapPanel != null && minimapPanel.IsWholeMapShow) ||
                    (menuWindow != null && menuWindow.IsOpen))
                {
                    menuOpened = true;
                }
            }
        }

        IsMenuOpened = menuOpened;
        IsMessageBoxOpened = messageBoxOpened;
    }

    private bool IsSpeedHackAllowed()
    {
        if (_chapter == Chapter.Title ||
            _chapter == Chapter.GameOver ||
            (IsMessageBoxOpened && !Plugin.Config.SpeedHackAffectsMessageBoxes.Value) ||
            IsMenuOpened ||
            (IsInSpecialMenu && !Plugin.Config.SpeedHackAffectsSpecialMenus.Value))
        {
            return false;
        }

        return true;
    }

    private void SetGameTimerMultiplier(int factor)
    {
        if (GamePaused)
        {
            GameTimerMultiplier = 0;
        }
        // don't speedup the game timer during battle like the base game
        else if (!Plugin.Config.SpeedHackAffectsGameTimer.Value ||
                  _chapter == Chapter.Battle ||
                  _chapter == Chapter.War ||
                  _chapter == Chapter.Duel)
        {
            GameTimerMultiplier = 1;
        }
        else
        {
            GameTimerMultiplier = factor;
        }
    }
}
