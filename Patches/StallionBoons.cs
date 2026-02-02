extern alias GSD1;
extern alias GSD2;

using HarmonyLib;

namespace Suikoden_Fix.Patches;

public class StallionBoonsPatch
{
    private static bool _isInEventPlayerMove2 = false;

    [HarmonyPatch(typeof(GSD1.Fmain_c), nameof(GSD1.Fmain_c.fmain_init))]
    [HarmonyPostfix]
    static void GSD1_FmainInit(GSD1.Fmain_c __instance)
    {
        var fieldWork = __instance.f_work;
        if (fieldWork == null)
        {
            return;
        }

        fieldWork.spd = 2;

        if (__instance.field_apx == 1) // in boat
        {
            var partyData = GSD1.OldSrcBase.game_work?.party_data;
            if (partyData != null && partyData.area_no == 10)
            {
                fieldWork.spd = 1;
            }
        }
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
