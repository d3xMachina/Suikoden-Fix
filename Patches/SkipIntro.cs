extern alias GSD1;
extern alias GSD2;

using HarmonyLib;

namespace Suikoden_Fix.Patches;

public class SkipIntroPatch
{
    [HarmonyPatch(typeof(GSDTitleSelect), nameof(GSDTitleSelect.Main))]
    [HarmonyPrefix]
    static void SkipSplashscreens(GSDTitleSelect __instance)
    {
        if (!Plugin.Config.SkipSplashscreens.Value)
        {
            return;
        }

        if (__instance.step >= (int)GSDTitleSelect.State.InitSystemLogo && __instance.step <= (int)GSDTitleSelect.State.ShowSystemLogo)
        {
            Plugin.Log.LogInfo("Skip splashscreen.");

            __instance.step = (int)GSDTitleSelect.State.WaitSpriteLoad;
        }
    }

    [HarmonyPatch(typeof(GSD1::TitleChapter), nameof(GSD1::TitleChapter.TitleMain))]
    [HarmonyPrefix]
    static void GSD1_SkipIntroMovie(GSD1::TitleChapter __instance, int step_level)
    {
        if (!Plugin.Config.SkipMovies.Value)
        {
            return;
        }

        var step = GSD1.ChapterManager.GetGameStepSeq(step_level);
        switch (step)
        {
            case (int)GSD1.TitleChapter.State.BeforeMovie:
                Plugin.Log.LogInfo("Skip intro.");
                GSD1.ChapterManager.SetGameStepSeq(step_level, (int)GSD1.TitleChapter.State.MovieWait);
                break;
            case (int)GSD1.TitleChapter.State.MenuSelect:
                __instance.timer = UnityEngine.Time.time; // Prevent movie start after 20 secs
                break;
            default:
                break;
        }
}

    [HarmonyPatch(typeof(GSD2::TitleChapter), nameof(GSD2::TitleChapter.TitleMain))]
    [HarmonyPrefix]
    static void GSD2_SkipIntroMovie(GSD2::TitleChapter __instance)
    {
        if (!Plugin.Config.SkipMovies.Value)
        {
            return;
        }

        switch (__instance.step)
        {
            case (int)GSD2.TitleChapter.State.BeforeMovie:
                Plugin.Log.LogInfo("Skip intro.");
                __instance.step = (int)GSD2.TitleChapter.State.MovieWait;
                break;
            case (int)GSD2.TitleChapter.State.MenuSelect:
                __instance.timer = UnityEngine.Time.time; // Prevent movie start after 20 secs
                break;
            default:
                break;
        }
    }
}
