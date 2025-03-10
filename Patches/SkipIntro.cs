extern alias GSD1;
extern alias GSD2;

using HarmonyLib;
using UnityEngine;

namespace Suikoden_Fix.Patches;

public class SkipIntroPatch
{
    [HarmonyPatch(typeof(GSDTitleSelect), nameof(GSDTitleSelect.Main))]
    [HarmonyPrefix]
    static void SkipSplashscreens(GSDTitleSelect __instance)
    {
        if (Plugin.Config.SkipSplashscreens.Value)
        {
            if (__instance.step >= (int)GSDTitleSelect.State.InitSystemLogo && __instance.step <= (int)GSDTitleSelect.State.ShowSystemLogo)
            {
                Plugin.Log.LogInfo("Skip splashscreen.");

                __instance.step = (int)GSDTitleSelect.State.WaitSpriteLoad;
            }
        }
    }

    [HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlayMovie))]
    [HarmonyPrefix]
    static bool SkipMovie(string path, Transform parent, Color bgColor)
    {
        if (Plugin.Config.SkipMovies.Value &&
            (path == "GS1_OP_HD_ENG" || path == "GS1_OP_HD_JPN" ||
            path == "GS2_OP_HD_ENG" || path == "GS2_OP_HD_JPN" ||
            path == "GS1_OP_CL" || path == "GS2_OP_CL"))
        {
            Plugin.Log.LogInfo("Skip movie.");
            return false;
        }

        return true;
    }
}
