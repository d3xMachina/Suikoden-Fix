extern alias GSD1;
extern alias GSD2;

using HarmonyLib;
using System;

namespace Suikoden_Fix.Patches;

public class BetterLeonaPatch
{
    private struct MemberCheck
    {
        public byte charaNo;
        public bool check;

        public MemberCheck(byte charaNo)
        {
            this.charaNo = charaNo;
            this.check = true;
        }
    }

    [HarmonyPatch(typeof(GSD2.EventOverlayClass.Overlay_partychg), nameof(GSD2.EventOverlayClass.Overlay_partychg.PCpartyAllTblSet))]
    [HarmonyPostfix]
    static void GSD2_SetPartyTable(GSD2.EventOverlayClass.Overlay_partychg.PCCON pcon)
    {
        const int FlagRecruited = 4;
        const int IdValeria = 12;
        const int IdKasumi = 73;
        const int IdMcDohl = 82;

        var ist = pcon?.ist;
        if (ist == null)
        {
            return;
        }

        var gameWork = GSD2.GAME_WORK.Instance;
        if (gameWork == null)
        {
            return;
        }

        // Must be sorted by character ID in ascending order
        var members = new MemberCheck[]
        {
            new(IdValeria),
            new(IdKasumi),
            new(IdMcDohl),
        };

        // McDohl ID is 82, that's why I added + 1
        int maxMembers = Math.Max(ist.Count, GSD2.EventOverlayClass.Overlay_partychg.PAMAX_MEM + 1);

        var charaFlags = gameWork.chara_flag;
        if (charaFlags == null || charaFlags.Count < maxMembers)
        {
            return;
        }

        var newIst = new GSD2.SP_PARTY[maxMembers];
        int indexNewIst = 0;

        void TryAddCharacters(byte currentCharaNo = byte.MaxValue)
        {
            for (int indexMember = 0; indexMember < members.Length; ++indexMember)
            {
                var member = members[indexMember];
                if (!member.check || currentCharaNo < member.charaNo)
                {
                    continue;
                }

                members[indexMember].check = false;

                // Character already added
                if (currentCharaNo == member.charaNo)
                {
                    continue;
                }

                //Plugin.Log.LogWarning($"MOD Try Add {member.charaNo} currentCharaNo={currentCharaNo}");

                // Try to add the character
                var charaFlag = charaFlags[member.charaNo]; // Backup the character flag

                if ((member.charaNo == IdMcDohl && gameWork.eventFlgCHK(0x41, 0x10)) || // McDohl
                    (member.charaNo == IdValeria && (charaFlags[IdKasumi] & FlagRecruited) != 0) || // Valeria
                    (member.charaNo == IdKasumi && (charaFlags[IdValeria] & FlagRecruited) != 0)) // Kasumi
                {
                    charaFlags[member.charaNo] |= FlagRecruited; // Recruited
                }
        
                bool addCharacter = GSD2.G2_SYS.G2_cha_flag(4, member.charaNo) == 1 && // Check if character recruited, not dead or on leave, not in party and something else (bit 0x20 of charaFlags).
                                    GSD2.G2_SYS.G2_cha_flag(9, member.charaNo) != 1; // Check if character in party. Shouldn't be necessary, looks like mode 4 checks for this already ?

                charaFlags[member.charaNo] = charaFlag; // Restore the character flag

                // Conditions not met
                if (!addCharacter)
                {
                    continue;
                }

                //Plugin.Log.LogWarning($"MOD Add {member.charaNo}");

                // Add the character
                newIst[indexNewIst] = new()
                {
                    cno = member.charaNo,
                    ist = 0,
                    nou = 0
                };

                ++indexNewIst;
            }
        }

        // ist is sorted by character ID
        for (int indexIst = 0; indexIst < pcon.inin; ++indexIst)
        {
            var spParty = ist[indexIst];
            if (spParty == null)
            {
                //Plugin.Log.LogWarning("SP_PARTY NULL");
                continue;
            }

            // Try to insert the characters
            TryAddCharacters(spParty.cno);

            //Plugin.Log.LogWarning($"Add {spParty.cno}");
            newIst[indexNewIst] = spParty;
            ++indexNewIst;
        }

        // Try to add the remaining characters
        TryAddCharacters();

        if (indexNewIst > 0)
        {
            // Replace the characters array
            pcon.ist = newIst;
            pcon.inin = indexNewIst;

            // Recalculate the pagination
            pcon.lpage = (byte)((indexNewIst - 1) / GSD2.EventOverlayClass.Overlay_partychg.PWPARH);
            pcon.liby = (byte)((indexNewIst - 1) % GSD2.EventOverlayClass.Overlay_partychg.PWPARH);
        }
    }
}
