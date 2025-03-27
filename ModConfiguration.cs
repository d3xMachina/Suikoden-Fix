using BepInEx.Configuration;

namespace Suikoden_Fix;

public sealed class ModConfiguration
{
    private ConfigFile _config;
    public ConfigEntry<int> FPS;
    public ConfigEntry<int> Vsync;
    public ConfigEntry<int> Width;
    public ConfigEntry<int> Height;
    public ConfigEntry<int> Fullscreen;
    public ConfigEntry<bool> SkipSplashscreens;
    public ConfigEntry<bool> SkipMovies;
    public ConfigEntry<float> LoadingTransitionFactor;
    public ConfigEntry<float> TitleMenuTransitionFactor;
    public ConfigEntry<float> ZoneTransitionFactor;
    public ConfigEntry<bool> DisableVignette;
    public ConfigEntry<bool> DisableMaskedVignette;
    public ConfigEntry<bool> DisableDiagonalMovements;
    public ConfigEntry<bool> ToggleDash;
    public ConfigEntry<bool> DisableFootStepSound;
    public ConfigEntry<bool> SaveAnywhere;
    public ConfigEntry<int> SpeedHackFactor;
    public ConfigEntry<bool> SpeedHackAffectsGameTimer;
    public ConfigEntry<bool> SpeedHackAffectsSpecialMenus;
    public ConfigEntry<bool> SpeedHackAffectsMessageBoxes;
    public ConfigEntry<bool> NoSpeedHackInBattle;
    public ConfigEntry<int> SpedUpMusic;
    public ConfigEntry<int> SpedUpSoundEffect;
    public ConfigEntry<bool> DisableMessageWindowSound;
    public ConfigEntry<bool> DisableStartledSound;
    public ConfigEntry<string> WindowBGColor;
    public ConfigEntry<bool> DisableAutoSaveNotification;
    public ConfigEntry<bool> RememberBattleSpeed;
    public ConfigEntry<bool> ClassicMode;
    public ConfigEntry<bool> ExitApplication;
    public ConfigEntry<bool> ResetGame;
    public ConfigEntry<bool> EditSave;
    public ConfigEntry<float> PlayerDamageMultiplier;
    public ConfigEntry<float> MonsterDamageMultiplier;
    public ConfigEntry<float> MonsterHealthMultiplier;
    public ConfigEntry<float> ExperienceMultiplier;
    public ConfigEntry<float> LootMultiplier;
    public ConfigEntry<float> EncounterRateMultiplier;
    public ConfigEntry<bool> RecoverAfterBattle;
    public ConfigEntry<float> MoneyMultiplier;
    public ConfigEntry<bool> InstantMessage;
    public ConfigEntry<bool> UncapMoney;
    public ConfigEntry<bool> InstantRichmondInvestigation;
    public ConfigEntry<bool> FixKeyboardBindings;

    public ModConfiguration(ConfigFile config)
    {
        _config = config;
    }

    public void Init()
    {
        FPS = _config.Bind(
             "Display",
             "FPS",
             -1,
             "Set the FPS limit. Set to 0 to uncap, a positive value, or -1 to use the default behavior."
        );

        Vsync = _config.Bind(
             "Display",
             "Vsync",
             -1,
             "Set to 0 to disable VSync, 1 to enable VSync or -1 to use the default behavior. When on, the framerate will match your monitor refresh rate."
        );

        Width = _config.Bind(
             "Display",
             "Width",
             -1,
             "Set to your desired width resolution."
        );

        Height = _config.Bind(
             "Display",
             "Height",
             -1,
             "Set to your desired height resolution."
        );

        Fullscreen = _config.Bind(
             "Display",
             "Fullscreen",
             -1,
             "Set to 0 for windowed mode or 1 for fullscreen mode."
        );

        SkipSplashscreens = _config.Bind(
             "Skip",
             "SkipSplashscreens",
             true,
             "Skip the intro splashscreens."
        );

        SkipMovies = _config.Bind(
             "Skip",
             "SkipMovies",
             false,
             "Skip the intro movie."
        );

        LoadingTransitionFactor = _config.Bind(
             "Skip",
             "LoadingTransitionFactor",
             -1f,
             "Change the speed of fade in/out on the loading screen. Set to 0 for instant transition, a positive value to speed up or -1 for the default behavior."
        );

        TitleMenuTransitionFactor = _config.Bind(
             "Skip",
             "TitleMenuTransitionFactor",
             -1f,
             "Change the speed of fade in/out in the main menu. Set to 0 for instant transition, a positive value to speed up or -1 for the default behavior."
        );

        ZoneTransitionFactor = _config.Bind(
             "Skip",
             "ZoneTransitionFactor",
             -1f,
             "(Might cause some events to not trigger in Suikoden 2!!!) Change the speed of fade in/out when changing zone. Set to 0 for instant transition, a positive value to speed up or -1 for the default behavior."
        );

        DisableVignette = _config.Bind(
             "Visual",
             "DisableVignette",
             false,
             "Disable the vignette effect (darkened corners) that is displayed in some scenes."
        );

        DisableMaskedVignette = _config.Bind(
             "Visual",
             "DisableMaskedVignette",
             false,
             "Disable the vignette effect (darkened corners) for vignette with a custom appearance that is displayed in some scenes (in the night time intro scene of Suikoden 2 for example)."
        );

        DisableDiagonalMovements = _config.Bind(
             "Movement",
             "DisableDiagonalMovements",
             false,
             "Restrict the movements to 4 directions instead of 8, except on the worldmap."
        );

        ToggleDash = _config.Bind(
             "Movement",
             "ToggleDash",
             false,
             "Make the dash command a toggle instead of having to hold it."
        );

        DisableFootStepSound = _config.Bind(
             "Audio",
             "DisableFootStepSound",
             false,
             "Disable the sound of foot steps."
        );

        SaveAnywhere = _config.Bind(
             "Misc",
             "SaveAnywhere",
             false,
             "Allows you to save anywhere to the last save slot using the select button or the F1 key. Be careful to not softlock yourself!"
        );

        SpeedHackFactor = _config.Bind(
             "Skip",
             "SpeedHackFactor",
             1,
             "Increase the game speed by X when the R2 button or the T key is pressed. Minimum value of 1."
        );

        SpeedHackAffectsGameTimer = _config.Bind(
             "Skip",
             "SpeedHackAffectsGameTimer",
             true,
             "The speedhack will change the game timer (used for events) accordingly. It will not take effect in battle. Only relevant when using SpeedHackFactor."
        );

        SpeedHackAffectsSpecialMenus = _config.Bind(
             "Skip",
             "SpeedHackAffectsSpecialMenus",
             false,
             "The speedhack will take effect in shop menus and some special menus like tablet, investigation, teleport..."
        );

        SpeedHackAffectsMessageBoxes = _config.Bind(
             "Skip",
             "SpeedHackAffectsMessageBoxes",
             false,
             "The speedhack will take effect in dialogues when a message box appears"
        );

        NoSpeedHackInBattle = _config.Bind(
             "Skip",
             "NoSpeedHackInBattle",
             false,
             "Disable the speedhack in battle. Only relevant when using SpeedHackFactor."
        );

        SpedUpMusic = _config.Bind(
             "Audio",
             "SpedUpMusic",
             -1,
             "Change the pitch or speed of the music when you change the game speed. Set 0 to have the sounds remain the same, 1 to have the sounds played at a higher pitch, 2 to have the sounds played faster at specific speeds, 3 to have the sounds played faster matching the game speed, or -1 for the default behavior."
        );

        SpedUpSoundEffect = _config.Bind(
             "Audio",
             "SpedUpSoundEffect",
             -1,
             "Change the pitch or speed of sound effects when you change the game speed. Set 0 to have the sounds remain the same, 1 to have the sounds played at a higher pitch, 2 to have the sounds played faster at specific speeds, 3 to have the sounds played faster matching the game speed or -1 for the default behavior."
        );

        DisableMessageWindowSound = _config.Bind(
             "Audio",
             "DisableMessageWindowSound",
             false,
             "Don't play a sound effect when a message window appears."
        );

        DisableStartledSound = _config.Bind(
             "Audio",
             "DisableStartledSound",
             false,
             "Don't play a sound effect when a character is startled (with the icon above the head) in Suikoden 2."
        );

        WindowBGColor = _config.Bind(
             "Visual",
             "WindowBGColor",
             "",
             "Change the background color of most windows instead of the default black. Use the hex format (#000C7A for example)"
        );

        DisableAutoSaveNotification = _config.Bind(
             "Visual",
             "DisableAutoSaveNotification",
             false,
             "Don't show the autosave notification window."
        );

        RememberBattleSpeed = _config.Bind(
             "Misc",
             "RememberBattleSpeed",
             false,
             "Remember the battle speed between battles. It will also disable the speedhack in battle like NoSpeedHackInBattle."
        );

        ClassicMode = _config.Bind(
             "Advanced",
             "ClassicMode",
             false,
             "Active the classic mode (PS1 visuals). This is a proof of concept, it is unfinished and many assets are missings."
        );

        ExitApplication = _config.Bind(
             "Misc",
             "ExitApplication",
             true,
             "Allow you to exit the application by pressing the start button or the escape key on the title selection screen."
        );

        ResetGame = _config.Bind(
             "Misc",
             "ResetGame",
             true,
             "Allow you to go back to the title screen of the game by pressing start + left shoulder + right shoulder on the gamepad or Escape + R on the keyboard."
        );
        
        FixKeyboardBindings = _config.Bind(
             "Misc",
             "FixKeyboardBindings",
             true,
             "Bind the key q to the action L1 and the key e to the action R1 which are necessary for the dance minigame."
        );

        EditSave = _config.Bind(
             "Advanced",
             "EditSave",
             false,
             "Allow you to edit your saves. Make sure to backup your saves for safety. After saving, go to your game folder and you will have the save files in the json format (filenames start with \"_decrypted\". Modify the content of the file then load your save again. You can save again to have the changes persist and disable this option."
        );

        PlayerDamageMultiplier = _config.Bind(
             "Cheat",
             "PlayerDamageMultiplier",
             1f,
             "Multiply the damage of your party members in battle. (Damage capped at 9999 before bonus)"
        );

        MonsterDamageMultiplier = _config.Bind(
             "Cheat",
             "MonsterDamageMultiplier",
             1f,
             "Multiply the damage of enemies in battle. (Damage capped at 9999 before bonus)"
        );
        
        MonsterHealthMultiplier = _config.Bind(
             "Cheat",
             "MonsterHealthMultiplier",
             1f,
             "Multiply the HP of enemies in battle. (HP capped at 32767)"
        );

        ExperienceMultiplier = _config.Bind(
             "Cheat",
             "ExperienceMultiplier",
             1f,
             "Multiply the experience gained after a battle."
        );

        MoneyMultiplier = _config.Bind(
             "Cheat",
             "MoneyMultiplier",
             1f,
             "Multiply the money gained after a battle."
        );

        LootMultiplier = _config.Bind(
             "Cheat",
             "LootMultiplier",
             1f,
             "Multiply the chance to loot an item after a battle. A value of 100 will guarantee the monster will drop its items (if it has items it can drop)."
        );

        EncounterRateMultiplier = _config.Bind(
             "Cheat",
             "EncounterRateMultiplier",
             1f,
             "Multiply the chances to get into a battle when walking. A value of 0 will disable encounters and 512 will guarantee you the highest probability. Encounters are checked every few steps mostly."
        );

        RecoverAfterBattle = _config.Bind(
             "Cheat",
             "RecoverAfterBattle",
             false,
             "Your party members recover their HP and MP after a battle."
        );

        InstantMessage = _config.Bind(
             "Skip",
             "InstantMessage",
             false,
             "Display messages instantly."
        );

        InstantRichmondInvestigation = _config.Bind(
             "Skip",
             "InstantRichmondInvestigation",
             false,
             "(Suikoden 2 only) No need to wait for the Richmond investigations."
        );

        UncapMoney = _config.Bind(
             "Misc",
             "UncapMoney",
             false,
             "Increase the money cap from 999,999 to 999,999,999."
        );
    }
}
