extern alias GSD1;
extern alias GSD2;

using HarmonyLib;

namespace Suikoden_Fix.Patches;

public class ResetGamePatch
{
    [HarmonyPatch(typeof(GSD1.ExitChapter._Start_d__0), nameof(GSD1.ExitChapter._Start_d__0.MoveNext))]
    [HarmonyPrefix]
    static bool GSD1_ResetOnExitChapter(GSD1.ExitChapter._Start_d__0 __instance)
    {
        if (ModComponent.Instance.ResetOnExit && __instance.__1__state == 1)
        {
            __instance.__1__state = -1;
            UnityEngine.SceneManagement.SceneManager.LoadScene("GSD1");

            return false;
        }

        return true;
    }
}
