extern alias GSD1;
extern alias GSD2;

using HarmonyLib;
using System;

namespace Suikoden_Fix.Patches;

public class EncounterRatePatch
{
    private const short Gsd2MaxProbability = 512;
    private static bool _isInEventBatlleCheck = false;

    static int MultiplyEncounterRate(int rand)
    {
        var multiplier = (double)Plugin.Config.EncounterRateMultiplier.Value;
        int newRand;

        if (multiplier <= 0)
        {
            newRand = Gsd2MaxProbability + 1; // no encounter
        }
        else
        {
            // divide because the check is rand < battleProbability + 1
            newRand = (int)Math.Clamp(Math.Round(rand / multiplier), 0, Gsd2MaxProbability);
        }

        return newRand;
    }

    [HarmonyPatch(typeof(GSD2.OldSrcBase), nameof(GSD2.OldSrcBase.get_rand), [typeof(int)])]
    [HarmonyPostfix]
    static void GSD2_GetRand(ref int __result)
    {
        if (_isInEventBatlleCheck)
        {
            __result = MultiplyEncounterRate(__result);
            //Plugin.Log.LogWarning($"Rand: {__result}");
        }
    }

    [HarmonyPatch(typeof(GSD2.EVENTCON), nameof(GSD2.EVENTCON.EventBatlleCheck))]
    [HarmonyPrefix]
    static void GSD2_EventBatlleCheck()
    {
        _isInEventBatlleCheck = true;
    }

    [HarmonyPatch(typeof(GSD2.EVENTCON), nameof(GSD2.EVENTCON.EventBatlleCheck))]
    [HarmonyPostfix]
    static void GSD2_EventBatlleCheckPost()
    {
        _isInEventBatlleCheck = false;
    }

    /* Alternative way by setting the probability of the event, but need to be run only once...
    [HarmonyPatch(typeof(GSD2.EVENTCON), nameof(GSD2.EVENTCON.EventBatlleCheck))]
    [HarmonyPrefix]
    static void GSD2_CheckBattle(GSD2.EVENTCON __instance)
    {
        var mapNo = __instance.mno;
        var mapEventDatas = __instance.mdat?.eventdata?.mapeventdat;
        if (mapEventDatas == null || mapEventDatas.Count <= mapNo)
        {
            return;
        }

        var mapEventData = mapEventDatas[mapNo];
        if (mapEventData == null)
        {
            return;
        }

        mapEventData.batlpro = (ushort)MultiplyEncounterRate(mapEventData.batlpro);
        Plugin.Log.LogWarning($"Battle Probability: {mapEventData.batlpro}");
    }
    */
}
