extern alias GSD1;
extern alias GSD2;

using HarmonyLib;

namespace Suikoden_Fix.Patches;

public class MoreTeleportLocationsPatch
{
    const int UnusedIndex1 = 3;

    [HarmonyPatch(typeof(GSD2.EventOverlayClass.telepo), nameof(GSD2.EventOverlayClass.telepo.TtelmachiChk))]
    [HarmonyPostfix]
    static void GSD2_TeleportAddLocations(GSD2.EventOverlayClass.telepo __instance)
    {
        const int GregminsterIndex = 24;
        const int RadatSecretMisDataIndex = 6;
        const int VisitedFlag = 0x80;

        var tcon = __instance.tcon;
        if (tcon == null)
        {
            return;
        }

        var tdat = tcon.tdat;
        if (tdat == null)
        {
            return;
        }

        var townFlags = GSD2.OldSrcBase.game_work?.town_flag;
        if (townFlags == null)
        {
            return;
        }

        var teleportMessages = GSD2.EventOverlayClass.telepo.telepo_msg;

        // Add Gregminster location
        if (tcon.ttkaz < GSD2.EventOverlayClass.telepo.TDATMAX &&
            GregminsterIndex < townFlags.Count &&
            (townFlags[GregminsterIndex] & VisitedFlag) != 0)
        {
            // Check if Gregminster was already added, shouldn't be necessary
            var gregminsterFound = false;
            for (int i = 0; i < tcon.ttkaz; ++i)
            {
                if (tdat[i] == GregminsterIndex)
                {
                    gregminsterFound = true;
                    break;
                }
            }

            if (!gregminsterFound)
            {
                tdat[tcon.ttkaz] = GregminsterIndex;
                ++tcon.ttkaz;

                // Fix the message displayed for Gregminster
                if (teleportMessages != null &&
                    GregminsterIndex < teleportMessages.Count)
                {
                    teleportMessages[GregminsterIndex] = 455;
                }
            }
        }

        var misDatas = __instance.tmisdat;

        // Add Radat Town secret location
        if (tcon.ttkaz < GSD2.EventOverlayClass.telepo.TDATMAX &&
            misDatas != null &&
            RadatSecretMisDataIndex < misDatas.Count)
        {
            var radatSecretData = misDatas[RadatSecretMisDataIndex];
            if (radatSecretData != null)
            {
                var townFlagIndex = radatSecretData.machino;
                if (townFlagIndex < townFlags.Count &&
                    (townFlags[townFlagIndex] & VisitedFlag) != 0)
                {
                    // 100% to get the secret room in Radat Town
                    for (int i = 0; i < misDatas.Count; ++i)
                    {
                        misDatas[i] = misDatas[RadatSecretMisDataIndex];
                    }

                    // Hijack an unused teleport location
                    tdat[tcon.ttkaz] = UnusedIndex1;
                    ++tcon.ttkaz;

                    // Fix the message displayed for the secret location
                    if (teleportMessages != null &&
                        UnusedIndex1 < teleportMessages.Count)
                    {
                        teleportMessages[UnusedIndex1] = 361;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(GSD2.EventOverlayClass.telepo), nameof(GSD2.EventOverlayClass.telepo.telepoMain))]
    [HarmonyPrefix]
    static void GSD2_TeleportMain(GSD2.EventOverlayClass.telepo __instance)
    {
        var tcon = __instance.tcon;
        if (tcon == null)
        {
            return;
        }

        var step = tcon.step;
        if (step != 3)
        {
            return;
        }

        var sysWork = GSD2.OldSrcBase.sys_work;
        if (sysWork == null)
        {
            return;
        }

        // Check confirm pressed
        if (((sysWork.pad_trg | sysWork.pad_trg3) & 0x20) == 0)
        {
            return;
        }

        // Force Viki to do a random teleport
        if (tcon.dno < tcon.tdat.Count &&
            tcon.tdat[tcon.dno] == UnusedIndex1)
        {
            tcon.misflg = 1;
        }
    }
}
