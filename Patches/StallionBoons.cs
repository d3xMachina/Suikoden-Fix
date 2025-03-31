extern alias GSD1;
extern alias GSD2;

using HarmonyLib;

namespace Suikoden_Fix.Patches;

public class StallionBoonsPatch
{
    private static bool _isInFmainInit = false;
    private static bool _isInEventPlayerMove2 = false;

    [HarmonyPatch(typeof(GSD1.Fmain_c), nameof(GSD1.Fmain_c.high_speed_chk))]
    [HarmonyPrefix]
    static bool GSD1_HighSpeedCheck(GSD1.Fmain_c __instance, ref bool __result)
    {
        if (__instance.field_apx == 0)
        {
            __result = true;
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(GSD1.Fmain_c), nameof(GSD1.Fmain_c.fmain_init))]
    [HarmonyPrefix]
    static void GSD1_FmainInit()
    {
        _isInFmainInit = true;
    }

    [HarmonyPatch(typeof(GSD1.Fmain_c), nameof(GSD1.Fmain_c.fmain_init))]
    [HarmonyPostfix]
    static void GSD1_FmainInitPost()
    {
        _isInFmainInit = false;
    }

    [HarmonyPatch(typeof(GSD1.Fmain_c), nameof(GSD1.Fmain_c.HSS_chk))]
    [HarmonyPrefix]
    static bool GSD1_HSSCheck(GSD1.Fmain_c __instance, ref bool __result)
    {
        if (_isInFmainInit && __instance.field_apx == 0)
        {
            __result = true;
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(GSD2.EVENTCON), nameof(GSD2.EVENTCON.EventPlayerMove2))]
    [HarmonyPrefix]
    static void GSD2_EventPlayerMove2()
    {
        _isInEventPlayerMove2 = true;
    }
    
    [HarmonyPatch(typeof(GSD2.EVENTCON), nameof(GSD2.EVENTCON.EventPlayerMove2))]
    [HarmonyPostfix]
    static void GSD2_EventPlayerMove2Post()
    {
        _isInEventPlayerMove2 = false;
    }

    [HarmonyPatch(typeof(GSD2.G2_SYS), nameof(GSD2.G2_SYS.G2_p_pano_get))]
    [HarmonyPrefix]
    static bool GSD2_PPanoGet(int chano, ref int __result)
    {
        const int StallionId = 67;

        if (_isInEventPlayerMove2 && chano == StallionId)
        {
            __result = 0; // character in party
            return false;
        }

        return true;
    }
}
