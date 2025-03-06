using BepInEx.Configuration;

namespace Suikoden_Fix;

public sealed class ModConfiguration
{
    private ConfigFile _config;
    public ConfigEntry<bool> DisableVignette;

    public ModConfiguration(ConfigFile config)
    {
        _config = config;
    }

    public void Init()
    {
        DisableVignette = _config.Bind(
             "Disable vignette",
             "DisableVignette",
             false,
             "Disable the vignette effect that is displayed in some scenes."
        );
    }
}
