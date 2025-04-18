extern alias GSD1;
extern alias GSD2;

using HarmonyLib;

namespace Suikoden_Fix.Patches;

public class RareFindsAlwaysInStockPatch
{
    private static int _fakePlayTime = -1;
    private static byte[] _rareFindsQuantity = null;
    private static bool _updateHdat = false;

    static bool RestoreRareFinds(GSD2.DOUGUCON dcon)
    {
        if (dcon.step != 'c')
        {
            return false;
        }

        var zai = dcon.gdat?.zai;
        if (zai == null || _rareFindsQuantity == null || _rareFindsQuantity.Length > zai.Count)
        {
            return false;
        }

        // Restore items quantity so it's not saved except for out of stock items
        for (int i = 0; i < _rareFindsQuantity.Length; ++i)
        {
            if (zai[i] == 0xFF) // 0xFF = out of stock
            {
                continue;
            }

            //Plugin.Log.LogWarning($"Restore[{i}]: before={zai[i]} after={_rareFindsQuantity[i]}");
            zai[i] = _rareFindsQuantity[i];
        }

        return true;
    }

    [HarmonyPatch(typeof(GSD2.dougu), nameof(GSD2.dougu.SDHoridashiCheck))]
    [HarmonyPatch(typeof(GSD2.monsyo), nameof(GSD2.monsyo.SMHoridashiCheck))]
    [HarmonyPrefix]
    static void GSD2_CheckRareFinds(GSD2.DOUGUCON dcon)
    {
        if (!_updateHdat)
        {
            _rareFindsQuantity = null;
        }

        var gdat = dcon?.gdat;
        if (gdat == null)
        {
            return;
        }

        if (_updateHdat)
        {
            _fakePlayTime = gdat.stim; // Skip the code that handle the rare finds
            return;
        }

        var ddat = dcon.ddat;
        if (ddat == null)
        {
            return;
        }

        var hori = ddat.hori;
        if (hori == null)
        {
            return;
        }

        var zai = gdat.zai;
        if (zai == null || ddat.hkaz > zai.Count || ddat.hkaz > hori.Count)
        {
            return;
        }

        _fakePlayTime = gdat.stim; // Skip the code that handle the rare finds

        _rareFindsQuantity = new byte[ddat.hkaz];

        for (int i = 0; i < ddat.hkaz; ++i)
        {
            _rareFindsQuantity[i] = zai[i]; // Backup items quantity

            if (zai[i] == 0xFF) // 0xFF = out of stock
            {
                continue;
            }

            if (hori[i].haskaz == -1) // unique item
            {
                zai[i] = 1;
            }
            else
            {
                zai[i] = 9; // maximum quantity
            }

            //Plugin.Log.LogWarning($"Zai[{i}]: before={_rareFindsQuantity[i]} after={zai[i]}");
        }
    }

    [HarmonyPatch(typeof(GSD2.dougu), nameof(GSD2.dougu.SDHoridashiCheck))]
    [HarmonyPatch(typeof(GSD2.monsyo), nameof(GSD2.monsyo.SMHoridashiCheck))]
    [HarmonyPostfix]
    static void GSD2_CheckRareFindsPost()
    {
        _fakePlayTime = -1;
    }

    [HarmonyPatch(typeof(GSD2.dougu), nameof(GSD2.dougu.ShopDouguMain))]
    [HarmonyPrefix]
    static void GSD2_RestoreRareFindsDougu(GSD2.dougu __instance)
    {
        var dcon = __instance.dcon;
        if (dcon == null)
        {
            return;
        }

        var restore = RestoreRareFinds(dcon);
        if (!restore)
        {
            return;
        }

        // Reconstruct hdat from zai
        _updateHdat = true;
        __instance.SDHoridashiCheck(dcon);
        _updateHdat = false;
    }

    [HarmonyPatch(typeof(GSD2.monsyo), nameof(GSD2.monsyo.ShopMonsyoMain))]
    [HarmonyPrefix]
    static void GSD2_RestoreRareFindsMonsyo(GSD2.monsyo __instance)
    {
        var dcon = __instance.dcon;
        if (dcon == null)
        {
            return;
        }

        var restore = RestoreRareFinds(dcon);
        if (!restore)
        {
            return;
        }

        // Reconstruct hdat from zai
        _updateHdat = true;
        __instance.SMHoridashiCheck(dcon);
        _updateHdat = false;
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
