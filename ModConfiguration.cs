using BepInEx.Configuration;

namespace Suikoden_Fix;

public sealed class ModConfiguration
{
    private ConfigFile _config;
    public ConfigEntry<int> FPS;
    public ConfigEntry<int> Vsync;
    public ConfigEntry<bool> DisableVignette;
    public ConfigEntry<bool> DisableDiagonalMovements;
    public ConfigEntry<bool> ToggleDash;
    public ConfigEntry<bool> DisableFootStepSound;

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
        DisableVignette = _config.Bind(
             "Disable vignette",
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
    }
}
