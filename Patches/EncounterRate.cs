extern alias GSD1;
extern alias GSD2;

using HarmonyLib;
using System;

namespace Suikoden_Fix.Patches;

public class EncounterRatePatch
{
    private static bool _isInBattleCheck = false;
    private static bool _isInUserMove = false;

    static int GSD1_MultiplyEncounterRate(int rand, int max)
    {
        var multiplier = (double)Plugin.Config.EncounterRateMultiplier.Value;
        int newRand;

        if (multiplier <= 0)
        {
            newRand = max; // no encounter
        }
        else
        {
            // divide because the check is rand < battleProbability
            newRand = (int)Math.Clamp(Math.Round(rand / multiplier), 0, max);
        }

        return newRand;
    }

    static int GSD2_MultiplyEncounterRate(int rand, int max)
    {
        var multiplier = (double)Plugin.Config.EncounterRateMultiplier.Value;
        int newRand;

        if (multiplier <= 0)
        {
            newRand = max + 1; // no encounter
        }
        else
        {
            // divide because the check is rand < battleProbability + 1
            newRand = (int)Math.Clamp(Math.Round(rand / multiplier), 0, max);
        }

        return newRand;
    }

    [HarmonyPatch(typeof(GSD1.Common), nameof(GSD1.Common.rand))]
    [HarmonyPostfix]
    static void GSD1_GetRand(ref int __result)
    {
        if (_isInBattleCheck && !_isInUserMove)
        {
            var byteRand = __result & 0xFF; // only the 8 LSB are used in fieldMain and f_wanderingMonsterChk
            byteRand = GSD1_MultiplyEncounterRate(byteRand, byte.MaxValue);
            __result = (__result & ~0xFF) | byteRand; // we replace the 8 LSB, we could just return the 8 LSB too
            //Plugin.Log.LogWarning($"Rand: {__result}");
        }
    }

    [HarmonyPatch(typeof(GSD1.Common), nameof(GSD1.Common.vrand))]
    [HarmonyPostfix]
    static void GSD1_GetVRand(ref int __result)
    {
        if (_isInBattleCheck)
        {
            __result = GSD1_MultiplyEncounterRate(__result, byte.MaxValue);
            //Plugin.Log.LogWarning($"VRand: {__result}");
        }
    }

    [HarmonyPatch(typeof(GSD2.OldSrcBase), nameof(GSD2.OldSrcBase.get_rand), [typeof(int)])]
    [HarmonyPostfix]
    static void GSD2_GetRand(ref int __result, int n)
    {
        if (_isInBattleCheck)
        {
            __result = GSD2_MultiplyEncounterRate(__result, n);
            //Plugin.Log.LogWarning($"Rand: {__result}");
        }
    }

    [HarmonyPatch(typeof(GSD2.EVENTCON), nameof(GSD2.EVENTCON.EventBatlleCheck))]
    // Check for references to oujya_monsho_chk() for GSD1
    [HarmonyPatch(typeof(GSD1.Village_c), nameof(GSD1.Village_c.v_wanderingMonsterChk))]
    [HarmonyPatch(typeof(GSD1.Fmain_c), nameof(GSD1.Fmain_c.f_wanderingMonsterChk))]
    [HarmonyPatch(typeof(GSD1.Fmain_c), nameof(GSD1.Fmain_c.fieldMain))]
    [HarmonyPrefix]
    static void BattleCheck()
    {
        _isInBattleCheck = true;
    }

    [HarmonyPatch(typeof(GSD2.EVENTCON), nameof(GSD2.EVENTCON.EventBatlleCheck))]
    [HarmonyPatch(typeof(GSD1.Village_c), nameof(GSD1.Village_c.v_wanderingMonsterChk))]
    [HarmonyPatch(typeof(GSD1.Fmain_c), nameof(GSD1.Fmain_c.f_wanderingMonsterChk))]
    [HarmonyPatch(typeof(GSD1.Fmain_c), nameof(GSD1.Fmain_c.fieldMain))]
    [HarmonyPostfix]
    static void EventBatlleCheckPost()
    {
        _isInBattleCheck = false;
    }

    // This function uses rand() and is run in fieldMain, so we make sure to not modify rand for it
    [HarmonyPatch(typeof(GSD1.Fmain_c), nameof(GSD1.Fmain_c.userMove))]
    [HarmonyPrefix]
    static void GSD1_UserMove()
    {
        _isInUserMove = true;
    }

    [HarmonyPatch(typeof(GSD1.Fmain_c), nameof(GSD1.Fmain_c.userMove))]
    [HarmonyPostfix]
    static void GSD1_UserMovePost()
    {
        _isInUserMove = false;
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
