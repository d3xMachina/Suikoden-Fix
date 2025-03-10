extern alias GSD1;
extern alias GSD2;

using HarmonyLib;
using UnityEngine;

namespace Suikoden_Fix.Patches;

public class WindowColorPatch
{
    [HarmonyPatch(typeof(GSD1.WindowManager), nameof(GSD1.WindowManager.GetBGColor))]
    [HarmonyPatch(typeof(GSD2.WindowManager), nameof(GSD2.WindowManager.GetBGColor))]
    [HarmonyPostfix]
    static void GetBGColor(int winSuke, ref Color __result)
    {
        if (ModComponent.Instance.WindowBGColor != null)
        {
            var color = ModComponent.Instance.WindowBGColor.Value;

            // Keep the alpha channel unchanged
            __result.r = color.r;
            __result.g = color.g;
            __result.b = color.b;
        }
    }
}
