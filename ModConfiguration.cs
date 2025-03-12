using BepInEx.Configuration;

namespace Suikoden_Fix;

public sealed class ModConfiguration
{
    private ConfigFile _config;
    public ConfigEntry<int> FPS;
    public ConfigEntry<int> Vsync;
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
    public ConfigEntry<bool> NoSpeedHackInBattle;
    public ConfigEntry<bool> NoHighPitchMusic;
    public ConfigEntry<bool> DisableMessageWindowSound;
    public ConfigEntry<string> WindowBGColor;
    public ConfigEntry<bool> RememberBattleSpeed;
    public ConfigEntry<bool> ClassicMode;
    public ConfigEntry<bool> ExitApplication;
    public ConfigEntry<bool> EditSave;
    public ConfigEntry<float> PlayerDamageMultiplier;
    public ConfigEntry<float> MonsterDamageMultiplier;
    public ConfigEntry<float> ExperienceMultiplier;
    public ConfigEntry<float> MoneyMultiplier;
    public ConfigEntry<bool> InstantMessage;

    public ModConfiguration(ConfigFile config)
    {
        _config = config;
    }

    public void Init()
    {
        FPS = _config.Bind(
             "Framerate",
             "FPS",
             -1,
             "Set the FPS limit. Set to 0 to uncap, a positive value, or -1 to use the default behavior."
        );

        Vsync = _config.Bind(
             "Framerate",
             "Vsync",
             -1,
             "Set to 0 to disable VSync, 1 to enable VSync or -1 to use the default behavior. When on, the framerate will match your monitor refresh rate."
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
             "Change the speed of fade in/out when changing zone. Set to 0 for instant transition, a positive value to speed up or -1 for the default behavior."
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

        NoSpeedHackInBattle = _config.Bind(
             "Skip",
             "NoSpeedHackInBattle",
             false,
             "Disable the speedhack in battle. Only relevant when using SpeedHackFactor."
        );

        NoHighPitchMusic = _config.Bind(
             "Audio",
             "NoHighPitchMusic",
             false,
             "Prevent the music from speeding up when you change the game speed."
        );

        DisableMessageWindowSound = _config.Bind(
             "Audio",
             "DisableMessageWindowSound",
             false,
             "Don't play a sound effect when a message window appears."
        );

        WindowBGColor = _config.Bind(
             "Visual",
             "WindowBGColor",
             "",
             "Change the background color of most windows instead of the default black. Use the hex format (#000C7A for example)"
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
             "Multiply the damage of your party members in battle."
        );

        MonsterDamageMultiplier = _config.Bind(
             "Cheat",
             "MonsterDamageMultiplier",
             1f,
             "Multiply the damage of enemies in battle."
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

        InstantMessage = _config.Bind(
             "Skip",
             "InstantMessage",
             false,
             "Display messages instantly."
        );
    }
}
