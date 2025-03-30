extern alias GSD1;
extern alias GSD2;

using HarmonyLib;

namespace Suikoden_Fix.Patches;

public class InstantBlacksmithPatch
{
    [HarmonyPatch(typeof(GSD1.h_kajiya_c), nameof(GSD1.h_kajiya_c.kajiya_level02))]
    [HarmonyPrefix]
    static void GSD1_SetWaitTimer()
    {
        var work = GSD1.h_kajiya_c.kajiya_work;
        if (work == null || work.step != 1)
        {
            return;
        }

        work.timer = 1; // set 1 instead of 0 since it's decremented first
    }

    [HarmonyPatch(typeof(GSD2.kaji), nameof(GSD2.kaji.SKMaidoKitaCom))]
    [HarmonyPrefix]
    static void GSD2_SetWaitTimer(GSD2.DOUGUCON dcon)
    {
        if (dcon == null || dcon.mstep != 1 && dcon.mstep != 2) // step 1 is "tink tink" and step 2 is the following wait
        {
            return;
        }

        dcon.wtim = 1; // set 1 instead of 0 since it's decremented first and it avoids calling the "tink tink" sound
    }
}
