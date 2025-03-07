extern alias GSD1;
extern alias GSD2;

using HarmonyLib;

namespace Suikoden_Fix.Patches;

public class SkipIntroPatch
{
    [HarmonyPatch(typeof(GSDTitleSelect), nameof(GSDTitleSelect.Main))]
    [HarmonyPrefix]
    static void SkipSplashscreens(ref GSDTitleSelect __instance)
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
}
