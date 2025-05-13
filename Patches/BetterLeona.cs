extern alias GSD1;
extern alias GSD2;

using HarmonyLib;
using System;

namespace Suikoden_Fix.Patches;

public class BetterLeonaPatch
{
    const int IdValeria = 12;
    const int IdKasumi = 73;
    const int IdMcDohl = 82;

    private static bool _fakeRecruited = false;
    private static bool _fakeRecruitedLeona = false;

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
        var ist = pcon?.ist;
        if (ist == null)
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
                _fakeRecruitedLeona = true;

                bool addCharacter = GSD2.G2_SYS.G2_cha_flag(4, member.charaNo) == 1 && // Check if character recruited, not dead or on leave, not in party and something else (bit 0x20 of charaFlags).
                                    GSD2.G2_SYS.G2_cha_flag(9, member.charaNo) != 1; // Check if character in party. Shouldn't be necessary, looks like mode 4 checks for this already ?
                
                _fakeRecruitedLeona = false;

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

    // Shops
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.hmonsyo), nameof(GSD2.EventOverlayClass.hmonsyo.hshopmonsyoinit))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.h_kaji), nameof(GSD2.EventOverlayClass.h_kaji.hshopkajiinit))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.h_dougu), nameof(GSD2.EventOverlayClass.h_dougu.hshopdouguinit))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.h_souko), nameof(GSD2.EventOverlayClass.h_souko.SoukoInit))]
    [HarmonyPatch(typeof(GSD2.koueki), nameof(GSD2.koueki.ShopKouekiInit))]

    // Richmond investigation
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.t_main), nameof(GSD2.EventOverlayClass.t_main.TanteiInit))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.t_main), nameof(GSD2.EventOverlayClass.t_main.CheckHintChar))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.t_phase), nameof(GSD2.EventOverlayClass.t_phase.TanteiPhaseInit))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.t_phase), nameof(GSD2.EventOverlayClass.t_phase.CheckDamyFaceChar))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.t_phase), nameof(GSD2.EventOverlayClass.t_phase.InitLvWindow))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.t_phase), nameof(GSD2.EventOverlayClass.t_phase.TanteiSecretInit))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.t_phase), nameof(GSD2.EventOverlayClass.t_phase.CheckSecretOpenLv4))]
    
    // Shops, warehouse, item transfers...
    [HarmonyPatch(typeof(GSD2.G2_SYS), nameof(GSD2.G2_SYS.G2_eqp_chk))]
    [HarmonyPatch(typeof(GSD2.G2_SYS), nameof(GSD2.G2_SYS.G2_item_num2))]

    // Other
    [HarmonyPatch(typeof(GSD2.fcommand), nameof(GSD2.fcommand.FCGetCharaPower))]
    [HarmonyPrefix]
    static void GSD2_FakeRecruited()
    {
        _fakeRecruited = true;
    }

    [HarmonyPatch(typeof(GSD2.EventOverlayClass.hmonsyo), nameof(GSD2.EventOverlayClass.hmonsyo.hshopmonsyoinit))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.h_kaji), nameof(GSD2.EventOverlayClass.h_kaji.hshopkajiinit))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.h_dougu), nameof(GSD2.EventOverlayClass.h_dougu.hshopdouguinit))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.h_souko), nameof(GSD2.EventOverlayClass.h_souko.SoukoInit))]
    [HarmonyPatch(typeof(GSD2.koueki), nameof(GSD2.koueki.ShopKouekiInit))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.t_main), nameof(GSD2.EventOverlayClass.t_main.TanteiInit))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.t_main), nameof(GSD2.EventOverlayClass.t_main.CheckHintChar))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.t_phase), nameof(GSD2.EventOverlayClass.t_phase.TanteiPhaseInit))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.t_phase), nameof(GSD2.EventOverlayClass.t_phase.CheckDamyFaceChar))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.t_phase), nameof(GSD2.EventOverlayClass.t_phase.InitLvWindow))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.t_phase), nameof(GSD2.EventOverlayClass.t_phase.TanteiSecretInit))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.t_phase), nameof(GSD2.EventOverlayClass.t_phase.CheckSecretOpenLv4))]
    [HarmonyPatch(typeof(GSD2.G2_SYS), nameof(GSD2.G2_SYS.G2_eqp_chk))]
    [HarmonyPatch(typeof(GSD2.G2_SYS), nameof(GSD2.G2_SYS.G2_item_num2))]
    [HarmonyPatch(typeof(GSD2.fcommand), nameof(GSD2.fcommand.FCGetCharaPower))]
    [HarmonyPostfix]
    static void GSD2_FakeRecruitedPost()
    {
        _fakeRecruited = false;
    }

    [HarmonyPatch(typeof(GSD2.G2_SYS), nameof(GSD2.G2_SYS.G2_cha_flag))]
    [HarmonyPrefix]
    static void GSD2_CharacterFlag(int chano, out int __state)
    {
        const int FlagRecruited = 4;
        const int FlagAutoRecruit = 70;

        __state = -1;

        if (!_fakeRecruitedLeona && (!_fakeRecruited || chano == IdMcDohl))
        {
            return;
        }

        var gameWork = GSD2.GAME_WORK.Instance;
        if (gameWork == null)
        {
            return;
        }

        var charaFlags = gameWork.chara_flag;
        if (charaFlags == null || IdMcDohl >= charaFlags.Count)
        {
            return;
        }

        if ((chano == IdMcDohl && gameWork.eventFlgCHK(0x41, 0x10)) || // McDohl
            (chano == IdValeria && (charaFlags[IdKasumi] & FlagRecruited) != 0) || // Valeria
            (chano == IdKasumi && (charaFlags[IdValeria] & FlagRecruited) != 0)) // Kasumi
        {
            __state = charaFlags[chano];
            charaFlags[chano] = FlagAutoRecruit;
        }
    }

    [HarmonyPatch(typeof(GSD2.G2_SYS), nameof(GSD2.G2_SYS.G2_cha_flag))]
    [HarmonyPostfix]
    static void GSD2_CharacterFlagPost(int chano, int __state)
    {
        if (__state == -1)
        {
            return;
        }

        var charaFlags = GSD2.GAME_WORK.Instance.chara_flag;
        charaFlags[chano] = (byte)__state;
    }

    [HarmonyPatch(typeof(GSD2.string_h), nameof(GSD2.string_h.SID_MAIN))]
    [HarmonyPrefix]
    static void GSD2_SIDMain(ref int x)
    {
        // Dialogue when you add Tir to the party
        if (x == GSD2.string_h.SID_MAIN_HAL_MSG_EV_MSG81) // 1497
        {
            x = GSD2.string_h.SID_MAIN_HAL_MSG_EV_MSG50; // 1633, Badeaux "Yeah. Let's go"
        }
    }
}
