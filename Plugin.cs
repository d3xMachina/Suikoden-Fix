using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Suikoden_Fix.Patches;
using HarmonyLib;
using System;

namespace Suikoden_Fix;

[BepInPlugin("d3xMachina.suikoden_fix", "Suikoden Fix", "1.0.0")]
public partial class Plugin : BasePlugin
{
    public static new ManualLogSource Log;
    public static new ModConfiguration Config;

    public override void Load()
    {
        Log = base.Log;

        Log.LogInfo("Loading...");

        Config = new ModConfiguration(base.Config);
        Config.Init();
        if (ModComponent.Inject())
        {
            ApplyPatches();
        }
    }

    private void ApplyPatches()
    {
        if (Config.FPS.Value >= 0 || Config.Vsync.Value >= 0)
        {
            ApplyPatch(typeof(FrameratePatch));
        }
        if (Config.DisableVignette.Value)
        {
            ApplyPatch(typeof(DisableVignettePatch));
        }

        if (Config.ToggleDash.Value)
        {
            ApplyPatch(typeof(ToggleDashPatch));
        }

        if (Config.DisableFootStepSound.Value)
        {
            ApplyPatch(typeof(DisableFootStepSoundPatch));
        }
        Log.LogInfo("Patches applied!");
    }

    private void ApplyPatch(Type type)
    {
        Log.LogInfo($"Patching {type.Name}...");

        Harmony.CreateAndPatchAll(type);
    }
}
