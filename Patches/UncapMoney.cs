extern alias GSD1;
extern alias GSD2;

using DG.Tweening;
using HarmonyLib;
using Share.UI;
using System;
using System.Linq;
using UnityEngine;

namespace Suikoden_Fix.Patches;

public class UncapMoneyPatch
{
    private const int DefaultMaxMoney = 999999;
    private const int MaxMoney = 999999999;
    private static int _windowSetNumberMoney = -1;
    private static bool _setZkin = false;
    private static int _zkin = 0;

    private struct PickPocket
    {
        public bool ok;
        public int currentMoney;
        public uint stolenMoney;
    }

    private struct CashAdjust
    {
        public bool ok;
        public byte cnt2;
        public bool moneyBufferEmpty;
    }

    static int LimitMoney(long money)
    {
        return (int)Math.Clamp(money, 0, MaxMoney);
    }

    static int GetMoneyLength(int money, bool hasSeparator)
    {
        var length = $"{money}".Length;
        var extraSeparatorLength = hasSeparator ? (length - 1) / 3 : 0;

        return length + extraSeparatorLength;
    }

    static int ChangeMoneyLength(int n, bool hasSeparator)
    {
        var defaultLength = GetMoneyLength(DefaultMaxMoney, hasSeparator);
        var length = GetMoneyLength(MaxMoney, hasSeparator);

        if (n == defaultLength)
        {
            n = length;
        }

        return n;
    }

    // To find related functions, look for scalar value 999999

    [HarmonyPatch(typeof(GSD1.TurugaiFunc_c.ALCHIN), nameof(GSD1.TurugaiFunc_c.ALCHIN.zkin), MethodType.Setter)]
    [HarmonyPrefix]
    static bool GSD1_DiceMinigameSetZkin(GSD1.TurugaiFunc_c.ALCHIN __instance, int value)
    {
        if (_setZkin)
        {
            value = _zkin;
        }

        __instance._zkin = LimitMoney(value);

        return false;
    }

    [HarmonyPatch(typeof(GSD1.TurugaiFunc_c), nameof(GSD1.TurugaiFunc_c.chinchirorin_func))]
    [HarmonyPrefix]
    static void GSD1_DiceMinigameFixOverflow(GSD1.TurugaiFunc_c __instance)
    {
        _setZkin = false;

        var btc = __instance.btc;
        if (btc == null || btc.bstep != GSD1.TurugaiFunc_c.NSTEP.STEP7)
        {
            return;
        }

        var tren = __instance.tren?.par;
        if (tren == null || (tren.PadTrig & 0x20) == 0)
        {
            return;
        }

        var alc = __instance.alc;
        if (alc == null)
        {
            return;
        }

        // Fix the overflow after a bet of x3 on 999 999 999
        var moneyBet = LimitMoney(alc.bairi * alc.kkin);
        _zkin = alc.zkin;

        if (alc.win_flg == 0) // win
        {
            _zkin += moneyBet;
        }
        else
        {
            _zkin -= moneyBet;
        }

        _setZkin = true;
    }

    [HarmonyPatch(typeof(GSD1.TurugaiFunc_c), nameof(GSD1.TurugaiFunc_c.chinchirofunc81))]
    [HarmonyPostfix]
    static void GSD1_DiceMinigameFixOverflow2(GSD1.TurugaiFunc_c __instance)
    {
        var alc = __instance.alc;
        if (alc == null)
        {
            return;
        }

        // Fix the overflow after a bet of x3 on 999 999 999
        if (alc.kkinbk < 0)
        {
            alc.kkinbk = MaxMoney;
        }
    }

    [HarmonyPatch(typeof(GSD1.GAME_WORK), nameof(GSD1.GAME_WORK.SetPocchi))]
    [HarmonyPrefix]
    static bool GSD1_SetPocchi(GSD1.GAME_WORK __instance, int pocchi)
    {
        var partyData = __instance.party_data;
        if (partyData == null)
        {
            return true;
        }

        partyData.mochi_kin = LimitMoney(pocchi);
        return false;
    }

    [HarmonyPatch(typeof(GSD1.GAME_WORK), nameof(GSD1.GAME_WORK.GetPocchi))]
    [HarmonyPrefix]
    static bool GSD1_GetPocchi(GSD1.GAME_WORK __instance, bool isLimit, ref int __result)
    {
        var partyData = __instance.party_data;
        if (partyData == null)
        {
            return true;
        }

        __result = isLimit ? LimitMoney(partyData.mochi_kin) : partyData.mochi_kin;
        return false;
    }

    [HarmonyPatch(typeof(GSD1.GAME_WORK), nameof(GSD1.GAME_WORK.CalcPocchi))]
    [HarmonyPrefix]
    static bool GSD1_GetPocchi(GSD1.GAME_WORK __instance, int addsub_pocchi)
    {
        var partyData = __instance.party_data;
        if (partyData == null)
        {
            return true;
        }

        partyData.mochi_kin = LimitMoney(partyData.mochi_kin + (long)addsub_pocchi);
        return false;
    }

    [HarmonyPatch(typeof(GSD1.GAME_WORK), nameof(GSD1.GAME_WORK.LimitPocchi))]
    [HarmonyPrefix]
    static bool GSD1_LimitPocchi(ref int __result)
    {
        __result = MaxMoney;
        return false;
    }

    [HarmonyPatch(typeof(GSD2.G2_SYS), nameof(GSD2.G2_SYS.G2_party_gold))]
    [HarmonyPrefix]
    static bool GSD2_GetPocchi(int mode, int val, ref int __result)
    {
        var partyData = GSD2.OldSrcBase.game_work?.party_data;
        if (partyData == null)
        {
            return true;
        }

        if (mode == 1)
        {
            partyData.gold = (uint)LimitMoney(partyData.gold + (long)val);
        }
        else if (mode == 2)
        {
            partyData.gold = (uint)Math.Min(val, MaxMoney);
        }

        __result = (int)partyData.gold;
        return false;
    }

    [HarmonyPatch(typeof(GSD2.EventOverlayClass.OverlayPickPocket), nameof(GSD2.EventOverlayClass.OverlayPickPocket.Entry))]
    [HarmonyPrefix]
    static void GSD2_OverlayPickPocket(GSD2.EventOverlayClass.OverlayPickPocket __instance, out PickPocket __state)
    {
        __state = new()
        {
            ok = false,
            currentMoney = 0,
            stolenMoney = 0
        };

        var para = __instance.para;
        if (para == null || para.Count < 1 || para[0] != 1)
        {
            return;
        }

        var gameData = GSD2.OldSrcBase.game_work?.game_data;
        if (gameData == null)
        {
            return;
        }

        __state.currentMoney = GSD2.G2_SYS.G2_party_gold(0, 0);
        __state.stolenMoney = gameData.suri_potch;
        __state.ok = true;
    }

    [HarmonyPatch(typeof(GSD2.EventOverlayClass.OverlayPickPocket), nameof(GSD2.EventOverlayClass.OverlayPickPocket.Entry))]
    [HarmonyPostfix]
    static void GSD2_OverlayPickPocketPost(PickPocket __state)
    {
        if (!__state.ok)
        {
            return;
        }

        // Recalculate added money without the default money cap
        var money = (int)Math.Min(__state.currentMoney + __state.stolenMoney, MaxMoney);
        GSD2.G2_SYS.G2_party_gold(2, money);
    }

    [HarmonyPatch(typeof(GSD2.EventOverlayClass.ch_main), nameof(GSD2.EventOverlayClass.ch_main.chin_end_sub))]
    [HarmonyPrefix]
    static void GSD2_ChinEndSub(GSD2.EventOverlayClass.chin_h.CHIN_DAM_WORK dw, out int __state)
    {
        __state = dw != null ? dw.p_money : 0;
    }

    [HarmonyPatch(typeof(GSD2.EventOverlayClass.ch_main), nameof(GSD2.EventOverlayClass.ch_main.chin_end_sub))]
    [HarmonyPostfix]
    static void GSD2_ChinEndSubPost(GSD2.EventOverlayClass.chin_h.CHIN_DAM_WORK dw, int __state, int __result)
    {
        if (dw == null || __result != 1)
        {
            return;
        }

        dw.p_money = Math.Min(__state, MaxMoney); // probably unnecessary
        GSD2.G2_SYS.G2_party_gold(2, dw.p_money);
    }

    [HarmonyPatch(typeof(GSD2.EventOverlayClass.ch_main), nameof(GSD2.EventOverlayClass.ch_main.cash_adjust))]
    [HarmonyPrefix]
    static void GSD2_CashAdjust(GSD2.EventOverlayClass.chin_h.CHIN_DAM_WORK dw, out CashAdjust __state)
    {
        __state = new()
        {
            ok = false
        };

        if (dw == null || dw.cnt2 != 2)
        {
            return;
        }

        dw.p_money += 100;
        dw.money_buf2 -= 100;

        if (dw.ver > 1)
        {
            dw.rest -= 100;
        }

        if (dw.p_money > MaxMoney)
        {
            dw.p_money += dw.money_buf2;
            dw.rest -= dw.money_buf2;
            dw.money_buf2 = 0;
        }

        if (dw.money_buf2 == 0)
        {
            GSD2.OldSrcBase.SD_call(0x50A);
            __state.moneyBufferEmpty = true;
        }

        __state.ok = true;
        __state.cnt2 = dw.cnt2;
        dw.cnt2 = 0xFF; // skip the code handling cnt2 == 2 since we handled it
    }

    [HarmonyPatch(typeof(GSD2.EventOverlayClass.ch_main), nameof(GSD2.EventOverlayClass.ch_main.cash_adjust))]
    [HarmonyPostfix]
    static void GSD2_CashAdjustPost(GSD2.EventOverlayClass.chin_h.CHIN_DAM_WORK dw, CashAdjust __state, ref int __result)
    {
        if (dw == null || !__state.ok)
        {
            return;
        }

        dw.cnt2 = __state.cnt2; // restore cnt2

        if (__state.moneyBufferEmpty && __result == 0)
        {
            __result = 1;
        }
    }

    [HarmonyPatch(typeof(GSD2.EventOverlayClass.ch_main), nameof(GSD2.EventOverlayClass.ch_main.set_rest_msg))]
    [HarmonyPrefix]
    static void GSD2_SetRestMsg(GSD2.WIN win, int money)
    {
        _windowSetNumberMoney = LimitMoney(money);
    }

    [HarmonyPatch(typeof(GSD2.EventOverlayClass.ch_main), nameof(GSD2.EventOverlayClass.ch_main.set_money_msg))]
    [HarmonyPrefix]
    static void GSD2_SetMoneyMsg(GSD2.WIN win, int money)
    {
        _windowSetNumberMoney = Math.Min(money, MaxMoney);
    }

    [HarmonyPatch(typeof(GSD2.EventOverlayClass.ch_main), nameof(GSD2.EventOverlayClass.ch_main.set_rest_msg))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.ch_main), nameof(GSD2.EventOverlayClass.ch_main.set_money_msg))]
    [HarmonyPostfix]
    static void GSD2_ResetWindowSetNumberMoney()
    {
        _windowSetNumberMoney = -1;
    }

    [HarmonyPatch(typeof(GSD1.Q_window_c), nameof(GSD1.Q_window_c.QAddNumberWindow))]
    [HarmonyPrefix]
    static void GSD1_FixNumberLength(GSD1.WIN win, int num, ref int n, bool isGroupCheck, bool hasSeparator)
    {
        n = ChangeMoneyLength(n, hasSeparator);
    }

    [HarmonyPatch(typeof(GSD2.window), nameof(GSD2.window.WindowSetNumber))]
    [HarmonyPrefix]
    static void GSD2_FixNumberLength(GSD2.WIN win, ref int num, ref byte n, bool hasSeparator)
    {
        n = (byte)ChangeMoneyLength(n, hasSeparator);

        if (_windowSetNumberMoney >= 0)
        {
            num = _windowSetNumberMoney;
        }
    }

    // Fix the money from the party menu being partially visible in the top right corner
    [HarmonyPatch(typeof(UIHeader), nameof(UIHeader.Initialize))]
    [HarmonyPostfix]
    static void FixMenuMoney(UIHeader __instance)
    {
        var moneyBg = __instance.moneyBG;
        if (moneyBg == null)
        {
            return;
        }

        var menuInAnim = moneyBg.GetComponents<DOTweenAnimation>().FirstOrDefault(anim => anim.id == "MenuOut");
        if (menuInAnim == null)
        {
            Plugin.Log.LogWarning("Could not find animation MenuOut");
            return;
        }

        var currencyLength = $" Potch".Length;
        var defaultLength = GetMoneyLength(DefaultMaxMoney, true) + currencyLength;
        var length = GetMoneyLength(MaxMoney, true) + currencyLength;
        var charactersDiff = length - defaultLength;

        if (charactersDiff != 0)
        {
            var perCharacterOffset = menuInAnim.endValueV3.x / defaultLength;
            var offset = perCharacterOffset * charactersDiff;
            menuInAnim.endValueV3 += new Vector3(offset, 0f, 0f);
            menuInAnim.CreateTween();
        }
    }
}
