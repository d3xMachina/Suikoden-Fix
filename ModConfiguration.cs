using BepInEx.Configuration;

namespace Suikoden_Fix;

public sealed class ModConfiguration
{
    private ConfigFile _config;
    public ConfigEntry<int> FPS;
    public ConfigEntry<int> Vsync;
    public ConfigEntry<bool> DisableVignette;

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
    }
}
