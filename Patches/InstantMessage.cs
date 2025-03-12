extern alias GSD1;
extern alias GSD2;

using HarmonyLib;
using Share.UI.Window;

namespace Suikoden_Fix.Patches;

public class InstantMessagePatch
{
    static bool skipMessage = true;

    [HarmonyPatch(typeof(UIMessageWindow), nameof(UIMessageWindow.Opened))]
    [HarmonyPrefix]
    static void Opened()
    {
        skipMessage = true;
    }

    [HarmonyPatch(typeof(UIMessageWindow), nameof(UIMessageWindow.Update))]
    [HarmonyPrefix]
    static void SkipMessage(UIMessageWindow __instance, ref bool __state)
    {
        __state = __instance.isTextAllDisp;

        if (skipMessage)
        {
            __instance.isTextAllDisp = true; // same as doing an input in this context
            skipMessage = false;
        }
    }

    [HarmonyPatch(typeof(UIMessageWindow), nameof(UIMessageWindow.Update))]
    [HarmonyPostfix]
    static void SkipMessagePost(UIMessageWindow __instance, bool __state)
    {
        __instance.isTextAllDisp = __state;
    }
}
