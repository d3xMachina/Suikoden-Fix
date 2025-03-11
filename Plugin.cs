using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Suikoden_Fix.Patches;
using HarmonyLib;
using System;

namespace Suikoden_Fix;

[BepInPlugin("d3xMachina.suikoden_fix", "Suikoden Fix", "1.3.1")]
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
        bool fastTransition = false;
        bool disableBattleSpeedChange = false;

        if (Config.FPS.Value >= 0 || Config.Vsync.Value >= 0)
        {
            ApplyPatch(typeof(FrameratePatch));
        }

        if (Config.SkipSplashscreens.Value || Config.SkipMovies.Value)
        {
            ApplyPatch(typeof(SkipIntroPatch));
        }

        if (Config.DisableVignette.Value || Config.DisableMaskedVignette.Value)
        {
            ApplyPatch(typeof(DisableVignettePatch));
        }

        if (Config.ToggleDash.Value || Config.DisableDiagonalMovements.Value)
        {
            ApplyPatch(typeof(InputMovementsPatch));
        }

        if (Config.DisableFootStepSound.Value)
        {
            ApplyPatch(typeof(DisableFootStepSoundPatch));
        }

        if (Config.ZoneTransitionFactor.Value >= 0f)
        {
            ApplyPatch(typeof(FastZoneTransitionPatch));
            fastTransition = true;
        }

        if (Config.LoadingTransitionFactor.Value >= 0f)
        {
            ApplyPatch(typeof(FastLoadingTransitionPatch));
            fastTransition = true;
        }

        if (Config.TitleMenuTransitionFactor.Value >= 0f)
        {
            ApplyPatch(typeof(FastTitleMenuTransitionPatch));
            fastTransition = true;
        }

        if (fastTransition)
        {
            ApplyPatch(typeof(FastTransitionPatch));
        }

        if (Config.NoHighPitchMusic.Value)
        {
            ApplyPatch(typeof(NoHighPitchMusicPatch));
        }

        if (Config.SpeedHackFactor.Value > 1)
        {
            ApplyPatch(typeof(SpeedHackPatch));
            disableBattleSpeedChange = true;
        }

        if (Config.RememberBattleSpeed.Value)
        {
            disableBattleSpeedChange = true;
        }

        if (disableBattleSpeedChange)
        {
            ApplyPatch(typeof(DisableBattleSpeedChangePatch));
        }

        if (Config.DisableMessageWindowSound.Value)
        {
            ApplyPatch(typeof(DisableMessageWindowSoundPatch));
        }

        if (!Config.WindowBGColor.Value.IsNullOrWhiteSpace())
        {
            ApplyPatch(typeof(WindowColorPatch));
        }

        if (Config.ClassicMode.Value)
        {
            ApplyPatch(typeof(ClassicModePatch));
        }

        if (Config.ExitApplication.Value)
        {
            ApplyPatch(typeof(ExitApplicationPatch));
        }
        
        if (Config.EditSave.Value)
        {
            ApplyPatch(typeof(EditSavePatch));
        }
        
        if (Config.PlayerDamageMultiplier.Value != 1f || Config.MonsterDamageMultiplier.Value != 1f)
        {
            ApplyPatch(typeof(DamageMultiplierPatch));
        }

        Log.LogInfo("Patches applied!");
    }

    private void ApplyPatch(Type type)
    {
        Log.LogInfo($"Patching {type.Name}...");

        Harmony.CreateAndPatchAll(type);
    }
}
