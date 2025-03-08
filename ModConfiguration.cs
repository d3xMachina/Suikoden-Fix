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
    public ConfigEntry<bool> DisableDiagonalMovements;
    public ConfigEntry<bool> ToggleDash;
    public ConfigEntry<bool> DisableFootStepSound;
    public ConfigEntry<bool> SaveAnywhere;
    public ConfigEntry<int> SpeedHackFactor;
    public ConfigEntry<bool> NoHighPitchMusic;

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
             "Only works on Suikoden I. Change the speed of fade in/out on the loading screen. Set to 0 for instant transition, a positive value to speed up or -1 for the default behavior."
        );

        TitleMenuTransitionFactor = _config.Bind(
             "Skip",
             "TitleMenuTransitionFactor",
             -1f,
             "Only works on Suikoden I. Change the speed of fade in/out in the main menu. Set to 0 for instant transition, a positive value to speed up or -1 for the default behavior."
        );

        ZoneTransitionFactor = _config.Bind(
             "Skip",
             "ZoneTransitionFactor",
             -1f,
             "Only works on Suikoden I. Change the speed of fade in/out when changing zone. Set to 0 for instant transition, a positive value to speed up or -1 for the default behavior."
        );

        DisableVignette = _config.Bind(
             "Visual",
             "DisableVignette",
             false,
             "Disable the vignette effect that is displayed in some scenes."
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

        NoHighPitchMusic = _config.Bind(
             "Misc",
             "NoHighPitchMusic",
             false,
             "Prevent the music from speeding up when you change the game speed."
        );
    }
}
