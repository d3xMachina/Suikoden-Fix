extern alias GSD1;
extern alias GSD2;

using HarmonyLib;

namespace Suikoden_Fix.Patches;

public class SaveAnywherePatch
{
    [HarmonyPatch(typeof(GSD1.Village_c), nameof(GSD1.Village_c.g3_event))]
    [HarmonyPostfix]
    static void GSD1_EventHandler(GSD1.Village_c __instance)
    {
        ModComponent.Instance.IsInGameEvent = __instance.event_exit_flg == 0;
    }

    [HarmonyPatch(typeof(GSD2.EVENTCON), nameof(GSD2.EVENTCON.EventControll))]
    [HarmonyPostfix]
    static void GSD2_EventHandler(GSD2.EVENTCON __instance)
    {
        ModComponent.Instance.IsInGameEvent = (__instance.syust & 0x1000) != 0;
    }
}
