extern alias GSD1;
extern alias GSD2;

using HarmonyLib;

namespace Suikoden_Fix.Patches;

public class ResetGamePatch
{
    private static bool _isInGSD2ExitChapter = false;

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

    [HarmonyPatch(typeof(GSD2.ExitChapter), nameof(GSD2.ExitChapter.Awake))]
    [HarmonyPrefix]
    static void GSD2_ExitChapter()
    {
        _isInGSD2ExitChapter = true;
    }

    [HarmonyPatch(typeof(GSD2.ExitChapter), nameof(GSD2.ExitChapter.Awake))]
    [HarmonyPostfix]
    static void GSD2_ExitChapterPost()
    {
        _isInGSD2ExitChapter = false;
    }

    [HarmonyPatch(typeof(UnityEngine.SceneManagement.SceneManager), nameof(UnityEngine.SceneManagement.SceneManager.LoadScene), [typeof(string)])]
    [HarmonyPrefix]
    static void LoadScene(ref string sceneName)
    {
        if (ModComponent.Instance.ResetOnExit && _isInGSD2ExitChapter)
        {
            sceneName = "GSD2";
        }
    }
}
