using HarmonyLib;
using UnityEngine;

namespace Suikoden_Fix.Patches;

public class FrameratePatch
{
    [HarmonyPatch(typeof(SystemObject), nameof(SystemObject.SetTargetFrameRate))]
    [HarmonyPrefix]
    static bool SetTargetFrameRate(int targetFrameRate)
    {
        if (Plugin.Config.FPS.Value >= 0)
        {
            Plugin.Log.LogInfo($"FPS set to {Plugin.Config.FPS.Value}.");
            Application.targetFrameRate = Plugin.Config.FPS.Value;
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(SystemObject), nameof(SystemObject.SetVsyncCount))]
    [HarmonyPrefix]
    static bool SetVsyncCount(int count)
    {
        if (Plugin.Config.Vsync.Value > 0)
        {
            Plugin.Log.LogInfo("VSync enabled.");
            QualitySettings.vSyncCount = 1;
            return false;
        }
        else if (Plugin.Config.Vsync.Value == 0)
        {
            Plugin.Log.LogInfo("VSync disabled.");
            QualitySettings.vSyncCount = 0;
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(SystemObject), nameof(SystemObject._FrameChange))]
    [HarmonyPostfix]
    static void FrameChange(int srcFps)
    {
        Plugin.Log.LogDebug($"FrameChange");

        SetVsyncCount(0); // parameter doesn't matter
        SetTargetFrameRate(0); // parameter doesn't matter
    }

    [HarmonyPatch(typeof(SystemObject), nameof(SystemObject.Force60FPS))]
    [HarmonyPrefix]
    static bool Force60FPS(bool isOn)
    {
        Plugin.Log.LogDebug($"Force60FPS");

        return false;
    }
}
