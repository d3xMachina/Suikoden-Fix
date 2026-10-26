extern alias GSD1;
extern alias GSD2;

using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace Suikoden_Fix.Patches;

public class WindowColorPatch
{
    private static Color? _windowBGColor;

    private static void Initialize()
    {
        var colorStr = Plugin.Config.WindowBGColor.Value;
        if (!colorStr.IsNullOrWhiteSpace())
        {
            if (ColorUtility.TryParseHtmlString(colorStr, out var color))
            {
                _windowBGColor = color;
            }
            else
            {
                Plugin.Log.LogWarning($"Invalid color for WindowBGColor: {colorStr}");
            }
        }
    }

    [HarmonyPatch(typeof(GSD1.WindowManager), nameof(GSD1.WindowManager.GetBGColor))]
    [HarmonyPatch(typeof(GSD2.WindowManager), nameof(GSD2.WindowManager.GetBGColor))]
    [HarmonyPostfix]
    private static void GetBGColor(ref Color __result)
    {
        if (_windowBGColor != null)
        {
            var color = _windowBGColor.Value;

            // Keep the alpha channel unchanged
            __result.r = color.r;
            __result.g = color.g;
            __result.b = color.b;
        }
    }
}
