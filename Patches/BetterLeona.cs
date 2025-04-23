extern alias GSD1;
extern alias GSD2;

using HarmonyLib;

namespace Suikoden_Fix.Patches;

public class BetterLeonaPatch
{
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.Overlay_partychg), nameof(GSD2.EventOverlayClass.Overlay_partychg.PCpartyAllTblSet))]
    [HarmonyPrefix]
    static bool GSD2_SetPartyTable(GSD2.EventOverlayClass.Overlay_partychg.PCCON pcon)
    {
        const int FlagRecruited = 4;
        const int IdValeria = 12;
        const int IdKasumi = 73;
        const int IdMcDohl = 82;

        var ist = pcon?.ist;
        if (ist == null || ist.Count < GSD2.EventOverlayClass.Overlay_partychg.PAMAX_MEM)
        {
            return true;
        }

        var gameWork = GSD2.GAME_WORK.Instance;
        if (gameWork == null)
        {
            return true;
        }

        var charaFlags = gameWork.chara_flag;
        if (charaFlags == null || charaFlags.Count < GSD2.EventOverlayClass.Overlay_partychg.PAMAX_MEM + 1)
        {
            return true;
        }

        // Handles loading pst array with the party members
        GSD2.EventOverlayClass.Overlay_partychg.PCpartyTblSet(pcon);

        int indexIst = 0;

        // Add all available characters
        // McDohl ID is 82, that's why I added + 1 and since index 0 is never used, no need to resize the ist array
        for (int charaNo = 1; charaNo < GSD2.EventOverlayClass.Overlay_partychg.PAMAX_MEM + 1; ++charaNo)
        {
            //Plugin.Log.LogWarning($"chano={charaNo} flags={charaFlags[charaNo]}");

            var charaFlag = charaFlags[charaNo]; // Backup the character flag

            if ((charaNo == IdMcDohl && gameWork.eventFlgCHK(0x41, 0x10)) || // McDohl
                (charaNo == IdValeria && (charaFlags[IdKasumi] & FlagRecruited) != 0) || // Valeria
                (charaNo == IdKasumi && (charaFlags[IdValeria] & FlagRecruited) != 0)) // Kasumi
            {
                charaFlags[charaNo] |= FlagRecruited; // Recruited
            }
            
            bool addCharacter = GSD2.G2_SYS.G2_cha_flag(4, charaNo) == 1 && // Check if character recruited, not dead or on leave, not in party and something else (bit 0x20 of charaFlags).
                                GSD2.G2_SYS.G2_cha_flag(9, charaNo) != 1; // Check if character in party. Shouldn't be necessary, looks like mode 4 checks for this already ?

            charaFlags[charaNo] = charaFlag; // Restore the character flag

            if (!addCharacter)
            {
                continue;
            }

            // Add the character
            GSD2.SP_PARTY spParty = new()
            {
                cno = (byte)charaNo,
                ist = 0,
                nou = 0
            };

            ist[indexIst] = spParty;
            ++indexIst;
        }

        pcon.inin = indexIst;
        if (indexIst == 0)
        {
            pcon.lpage = 0;
            pcon.liby = 0;
        }
        else
        {
            pcon.lpage = (byte)((indexIst - 1) / GSD2.EventOverlayClass.Overlay_partychg.PWPARH);
            pcon.liby = (byte)((indexIst - 1) % GSD2.EventOverlayClass.Overlay_partychg.PWPARH);
        }

        if (indexIst == 0 && pcon.pnin != GSD2.EventOverlayClass.Overlay_partychg.PWPARH)
        {
            --pcon.lpby;
        }

        return false;
    }
}
