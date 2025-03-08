﻿using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Suikoden_Fix.Patches;
using HarmonyLib;
using System;

namespace Suikoden_Fix;

[BepInPlugin("d3xMachina.suikoden_fix", "Suikoden Fix", "1.2.1")]
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

        if (Config.SkipSplashscreens.Value || Config.SkipMovies.Value)
        {
            ApplyPatch(typeof(SkipIntroPatch));
        }

        if (Config.DisableVignette.Value)
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

        bool fastTransition = false;

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

        if (Config.SpeedHackFactor.Value > 1)
        {
            ApplyPatch(typeof(SpeedHackPatch));
        }

        if (Config.NoHighPitchMusic.Value)
        {
            ApplyPatch(typeof(NoHighPitchMusicPatch));
        }

        ApplyPatch(typeof(DisableMessageBoxSoundPatch));

        Log.LogInfo("Patches applied!");
    }

    private void ApplyPatch(Type type)
    {
        Log.LogInfo($"Patching {type.Name}...");

        Harmony.CreateAndPatchAll(type);
    }
}
