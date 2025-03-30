extern alias GSD1;
extern alias GSD2;

using HarmonyLib;

namespace Suikoden_Fix.Patches;

public class InstantShopPatch
{
    [HarmonyPatch(typeof(GSD1.h_kajiya_c), nameof(GSD1.h_kajiya_c.kajiya_level02))]
    [HarmonyPrefix]
    static void GSD1_SetBlacksmithTimer()
    {
        var work = GSD1.h_kajiya_c.kajiya_work;
        if (work == null || work.step != 1)
        {
            return;
        }

        work.timer = 1; // set 1 instead of 0 since it's decremented first
    }

    [HarmonyPatch(typeof(GSD1.h_kantei_c), nameof(GSD1.h_kantei_c.kantei_kantei03))]
    [HarmonyPrefix]
    static void GSD1_SetAppraiserTimer()
    {
        var work = GSD1.h_kantei_c.kantei_work;
        if (work == null || work.step != 2)
        {
            return;
        }

        work.timer = 1; // set 1 instead of 0 since it's decremented first
    }

    [HarmonyPatch(typeof(GSD2.kaji), nameof(GSD2.kaji.SKMaidoKitaCom))]
    [HarmonyPrefix]
    static void GSD2_SetBlacksmithTimer(GSD2.DOUGUCON dcon)
    {
        // step 1 is "tink tink" and step 2 is the following wait
        if (dcon == null || dcon.mstep != 1 && dcon.mstep != 2)
        {
            return;
        }

        dcon.wtim = 1; // set 1 instead of 0 since it's decremented first and it avoids calling the "tink tink" sound
    }

    [HarmonyPatch(typeof(GSD2.kantei), nameof(GSD2.kantei.SKAMaidoKanCom))]
    [HarmonyPrefix]
    static void GSD2_SetAppraiserTimer(GSD2.DOUGUCON dcon)
    {
        // step 1 is appraising, step 2 is the following wait but keep step 2 intact since it shows the item appraised
        if (dcon == null || dcon.mstep != 1)
        {
            return;
        }

        dcon.wtim = 1; // set 1 instead of 0 since it's decremented first
    }

    /*
    [HarmonyPatch(typeof(GSD1.P_sound_c), nameof(GSD1.P_sound_c.Qsd_call))]
    [HarmonyPrefix]
    static void GSD1_SdCall(int effect_no)
    {
        Plugin.Log.LogWarning($"Sound=0x{effect_no:X}");
    }

    [HarmonyPatch(typeof(GSD2.OldSrcBase), nameof(GSD2.OldSrcBase.SD_call), [typeof(int), typeof(string)])]
    [HarmonyPrefix]
    static void GSD2_SdCall(int call)
    {
        Plugin.Log.LogWarning($"Sound=0x{call:X}");
    }
    */
}
