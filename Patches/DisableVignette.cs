using HarmonyLib;

namespace Suikoden_Fix.Patches;

public class DisableVignettePatch
{
    static bool DisableVignette(ScreenScript sc)
    {
        if (sc.effectColor.r == 0f && sc.effectColor.r == 0f && sc.effectColor.r == 0f && sc.effectSky == 0f &&
            ((Plugin.Config.DisableVignette.Value && sc.maskTex == null) ||
             (Plugin.Config.DisableMaskedVignette.Value && sc.maskTex != null)))
        {
            sc.wipe = 0f;
            return true;
        }

        return false;
    }

    [HarmonyPatch(typeof(CameraManager), nameof(CameraManager.EnableVignetteFilter))]
    [HarmonyPostfix]
    static void DisableCameraVignette(ref bool isEnabled)
    {
        var camera = CameraManager.Instance?.ActiveCamera;
        if (camera != null)
        {
            var sc = camera.GetComponent<ScreenScript>();
            if (sc != null)
            {
                DisableVignette(sc);
            }
        }
    }

    [HarmonyPatch(typeof(ScreenScript), nameof(ScreenScript.OnEnable))]
    [HarmonyPrefix]
    static void DisableScreenScriptVignettePre(ScreenScript __instance)
    {
        DisableVignette(__instance);
    }

    [HarmonyPatch(typeof(ScreenScript), nameof(ScreenScript.SetParameter))]
    [HarmonyPatch(typeof(ScreenScript), nameof(ScreenScript.EnableVignetteFilter))]
    [HarmonyPostfix]
    static void DisableScreenScriptVignettePost(ScreenScript __instance)
    {
        DisableVignette(__instance);
    }
}