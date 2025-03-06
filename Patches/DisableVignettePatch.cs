using HarmonyLib;
using UnityEngine;

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

    [HarmonyPatch(typeof(ScreenScript), nameof(ScreenScript.SetParameter))]
    [HarmonyPrefix]
    static void SetParameter(ref float _wipe, Texture _maskTex, Vector4 _maskOffset, float _rate, Color _effectColor, float _effectSky)
    {
        if (_effectColor.r == 0f && _effectColor.g == 0f && _effectColor.b == 0f && _maskTex == null)
        {
            _wipe = 0f;
        }
    }
}