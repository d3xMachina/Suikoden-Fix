extern alias GSD1;
extern alias GSD2;

using HarmonyLib;

namespace Suikoden_Fix.Patches;

public class RareFindsAlwaysInStockPatch
{
    private static int _fakePlayTime = -1;

    [HarmonyPatch(typeof(GSD2.dougu), nameof(GSD2.dougu.SDHoridashiCheck))]
    [HarmonyPrefix]
    static void GSD2_CheckRareFinds(GSD2.DOUGUCON dcon, ref byte[] __state)
    {
        var gdat = dcon?.gdat;
        if (gdat == null || gdat.zai == null)
        {
            return;
        }

        _fakePlayTime = gdat.stim; // Skip the code that handle the rare finds

        __state = new byte[gdat.zai.Count];

        for (int i = 0; i < gdat.zai.Count; ++i)
        {
            __state[i] = gdat.zai[i]; // Backup items quantity
            gdat.zai[i] = 9; // maximum quantity
        }
    }

    [HarmonyPatch(typeof(GSD2.dougu), nameof(GSD2.dougu.SDHoridashiCheck))]
    [HarmonyPostfix]
    static void GSD2_CheckRareFindsPost(GSD2.DOUGUCON dcon, byte[] __state)
    {
        var gdat = dcon?.gdat;
        if (gdat == null || gdat.zai == null)
        {
            return;
        }

        _fakePlayTime = -1;

        if (__state != null)
        {
            // Restore items quantity so it's not saved
            for (int i = 0; i < gdat.zai.Count; ++i)
            {
                gdat.zai[i] = __state[i];
            }
        }
    }

    [HarmonyPatch(typeof(GSD2.MACHICON), nameof(GSD2.MACHICON.MachiPlayMin))]
    [HarmonyPrefix]
    static bool GSD2_MachiPlayMin(ref int __result)
    {
        if (_fakePlayTime != -1)
        {
            __result = _fakePlayTime;
            return false;
        }

        return true;
    }
}
