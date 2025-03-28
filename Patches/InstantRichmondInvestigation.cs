extern alias GSD1;
extern alias GSD2;

using HarmonyLib;
using System;

namespace Suikoden_Fix.Patches;

public class InstantRichmondInvestigationPatch
{
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.t_phase), nameof(GSD2.EventOverlayClass.t_phase.TanteiReserchSecret))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.t_phase), nameof(GSD2.EventOverlayClass.t_phase.TanteiReserchHowto))]
    [HarmonyPostfix]
    static void GSD2_ChangeTimer(int __result)
    {
        const int InvestigationEventTimeIndex = 11;

        if (__result != 1)
        {
            return;
        }

        var eventTime = GSD2.OldSrcBase.game_work?.game_data?.event_time;
        if (eventTime == null || eventTime.Count <= InvestigationEventTimeIndex || eventTime[InvestigationEventTimeIndex].Count <= 1)
        {
            return;
        }

        eventTime[InvestigationEventTimeIndex][0] = 0;
        eventTime[InvestigationEventTimeIndex][1] = 0;
        //Plugin.Log.LogWarning("Instant investigation!");
    }
}
