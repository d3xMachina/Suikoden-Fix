extern alias GSD1;
extern alias GSD2;

using HarmonyLib;
using Share.UI.Window;

namespace Suikoden_Fix.Patches;

public class InstantMessagePatch
{
    static bool skipMessage = true;

    // For GSD2, the Update code is skipped in GSD1

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

    // For GSD1

    [HarmonyPatch(typeof(GSD1.Event_c), nameof(GSD1.Event_c.chkBottanWait))]
    [HarmonyPrefix]
    static void GSD1_CheckButton(GSD1.Event_c __instance, ref uint __state)
    {
        var sysWork = GSD1.Event_c.sys_work;
        if (sysWork == null)
        {
            return;
        }

        __state = sysWork.PadTrig;

        if (GSD1.Event_c.eventMsgWaitF != 1)
        {
            sysWork.PadTrig |= 0x20; // simulate a Confirm input
        }
    }

    [HarmonyPatch(typeof(GSD1.Event_c), nameof(GSD1.Event_c.chkBottanWait))]
    [HarmonyPostfix]
    static void GSD1_CheckButton(uint __state)
    {
        var sysWork = GSD1.Event_c.sys_work;
        if (sysWork == null)
        {
            return;
        }

        sysWork.PadTrig = __state; // restore input state
    }
}
