using HarmonyLib;

namespace Suikoden_Fix.Patches;

public class DisableVignettePatch
{
    [HarmonyPatch(typeof(CameraManager), nameof(CameraManager.EnableVignetteFilter))]
    [HarmonyPrefix]
    static void DisableCameraVignette(ref bool isEnabled)
    {
        isEnabled = false;
    }

    [HarmonyPatch(typeof(ScreenScript), nameof(ScreenScript.EnableVignetteFilter))]
    [HarmonyPrefix]
    static void DisableScreenScriptVignette(ref bool isEnabled)
    {
        isEnabled = false;
    }
}