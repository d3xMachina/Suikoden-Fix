extern alias GSD1;
extern alias GSD2;

using HarmonyLib;
using Share.UI.Window;

namespace Suikoden_Fix.Patches;

public class InstantMessagePatch
{
    private static bool _skipMessage = true;
    private static int _messagePage = 0;

    [HarmonyPatch(typeof(ShareSaveData), nameof(ShareSaveData.LoadFile), [typeof(string), typeof(Il2CppSystem.Action<bool>)])]
    [HarmonyPostfix]
    static void GSD1_ForceFastestSpeed()
    {
        var config = ShareSaveData.system_config;
        if (config != null)
        {
            config.message_speed = 0;
        }
    }

    // For GSD2, the Update code is skipped in GSD1

    [HarmonyPatch(typeof(UIMessageWindow), nameof(UIMessageWindow.Opened))]
    [HarmonyPostfix]
    static void Opened(UIMessageWindow __instance)
    {
        _messagePage = __instance.nowPage;

        if (!__instance.isMessageInputWait)
        {
            _skipMessage = true;
        }
    }

    [HarmonyPatch(typeof(UIMessageWindow), nameof(UIMessageWindow.Update))]
    [HarmonyPrefix]
    static void SkipMessage(UIMessageWindow __instance, ref bool __state)
    {
        __state = __instance.isTextAllDisp;

        if (!__instance.isMessageInputWait)
        {
            if (_messagePage != __instance.nowPage)
            {
                _skipMessage = true;
                _messagePage = __instance.nowPage;
            }
        
            if (_skipMessage)
            {
                __instance.isTextAllDisp = true; // same as doing an input in this context
                _skipMessage = false;
            }
        }
    }

    [HarmonyPatch(typeof(UIMessageWindow), nameof(UIMessageWindow.Update))]
    [HarmonyPostfix]
    static void SkipMessagePost(UIMessageWindow __instance, bool __state)
    {
        __instance.isTextAllDisp = __state;
    }

    // For GSD1

    [HarmonyPatch(typeof(GSD1.Event_c), nameof(GSD1.Event_c.chkBottanWait))]
    [HarmonyPrefix]
    static void GSD1_CheckButton(ref uint __state)
    {
        var sysWork = GSD1.Event_c.sys_work;
        if (sysWork == null)
        {
            return;
        }

        __state = sysWork.PadTrig;

        if (GSD1.Event_c.eventMsgWaitF != 1)
        {
            sysWork.PadTrig |= 0x20; // simulate a Confirm input
        }
    }

    [HarmonyPatch(typeof(GSD1.Event_c), nameof(GSD1.Event_c.chkBottanWait))]
    [HarmonyPostfix]
    static void GSD1_CheckButtonPost(uint __state)
    {
        var sysWork = GSD1.Event_c.sys_work;
        if (sysWork == null)
        {
            return;
        }

        sysWork.PadTrig = __state; // restore input state
    }

    [HarmonyPatch(typeof(GSD1.Ws_serif_c), nameof(GSD1.Ws_serif_c.WriteSerifuWindow))]
    [HarmonyPatch(typeof(GSD1.Go_ws_main), nameof(GSD1.Go_ws_main.WriteSerifuWindow))]
    [HarmonyPrefix]
    static void GSD1_CheckButton2(ref uint __state)
    {
        var sysWork = GSD1.Event_c.sys_work;
        if (sysWork == null)
        {
            return;
        }

        __state = sysWork.PadTrig;
        sysWork.PadTrig |= 0x20; // simulate a Confirm input
    }

    [HarmonyPatch(typeof(GSD1.Ws_serif_c), nameof(GSD1.Ws_serif_c.WriteSerifuWindow))]
    [HarmonyPatch(typeof(GSD1.Go_ws_main), nameof(GSD1.Go_ws_main.WriteSerifuWindow))]
    [HarmonyPostfix]
    static void GSD1_CheckButton2Post(uint __state)
    {
        var sysWork = GSD1.Event_c.sys_work;
        if (sysWork == null)
        {
            return;
        }

        sysWork.PadTrig = __state; // restore input state
    }

    [HarmonyPatch(typeof(GSD1.W_serifu_c), nameof(GSD1.W_serifu_c.war_WriteSerifuWindow))]
    [HarmonyPrefix]
    static void GSD1_CheckButtonWar(ref uint __state)
    {
        var sysWork = GSD1.Event_c.sys_work;
        if (sysWork == null)
        {
            return;
        }

        __state = sysWork.PadTrig;
        sysWork.PadData |= 0x20; // simulate a Confirm input
    }

    [HarmonyPatch(typeof(GSD1.W_serifu_c), nameof(GSD1.W_serifu_c.war_WriteSerifuWindow))]
    [HarmonyPostfix]
    static void GSD1_CheckButtonWarPost(uint __state)
    {
        var sysWork = GSD1.Event_c.sys_work;
        if (sysWork == null)
        {
            return;
        }

        sysWork.PadData = __state; // restore input state
    }

    [HarmonyPatch(typeof(GSD1.D_azukar_c), nameof(GSD1.D_azukar_c.azukari_DispKaiwaBuff))]
    [HarmonyPatch(typeof(GSD1.h_omise_c), nameof(GSD1.h_omise_c.DispKaiwaBuff))]
    [HarmonyPatch(typeof(GSD1.G1_i_main_c), nameof(GSD1.G1_i_main_c.i_DispKaiwaBuff))]
    [HarmonyPrefix]
    static void GSD1_KaiwaBuffer(GSD1.KAIWA_BUFFER kaiwa_buff)
    {
        if (kaiwa_buff == null)
        {
            return;
        }

        kaiwa_buff.timer = 1; // Decremented first
    }

    [HarmonyPatch(typeof(GSD1.G1_i_main_c), nameof(GSD1.G1_i_main_c.i_disp_window))]
    [HarmonyPrefix]
    static void GSD1_Battle1v1(GSD1.G1_i_main_c __instance)
    {
        var kaiwaBuffer = __instance.ikki_work?.kaiwa;
        if (kaiwaBuffer == null)
        {
            return;
        }

        kaiwaBuffer.timer = 1; // Decremented first
    }

    static void GSD2_CheckMessageMinigame(int winNo, byte[] msgStep, GSD2.WIN[] msgWin)
    {
        if (msgStep == null || winNo >= msgStep.Length)
        {
            return;
        }

        var step = msgStep[winNo];
        if (step != 1)
        {
            return;
        }

        if (msgWin == null || winNo >= msgWin.Length)
        {
            return;
        }

        var window = msgWin[winNo];
        if (window == null || GSD2.OldSrcBase.w_getFlg(window, 0x200) || GSD2.OldSrcBase.w_getFlg(window, 0x100))
        {
            return;
        }

        GSD2.window.WindowMessageDispAll(window);
    }

    [HarmonyPatch(typeof(GSD2.EventOverlayClass.ch_main), nameof(GSD2.EventOverlayClass.ch_main.msg_check))]
    [HarmonyPrefix]
    static void GSD2_CheckMessageDiceMinigame(GSD2.EventOverlayClass.chin_h.CHIN_DAM_WORK dw, int win_no)
    {
        GSD2_CheckMessageMinigame(win_no, dw?.msg_step, dw?.msgwin);
    }

    [HarmonyPatch(typeof(GSD2.EventOverlayClass.fish_dsp), nameof(GSD2.EventOverlayClass.fish_dsp.BFI_CheckMessage))]
    [HarmonyPrefix]
    static void GSD2_CheckMessageFishMinigame(GSD2.EventOverlayClass.Overlay_fishing.BASE_FISHING bfi, int window_no)
    {
        GSD2_CheckMessageMinigame(window_no, bfi?.msg_stp, bfi?.bfiwin);
    }

    [HarmonyPatch(typeof(GSD2.EventOverlayClass.m_main), nameof(GSD2.EventOverlayClass.m_main.MoguraMain))]
    [HarmonyPrefix]
    static void GSD2_CheckMessageMoleMinigame(GSD2.EventOverlayClass.mogura_h.MOGURA_DAM_WORK dw)
    {
        var window = dw?.msg_win;
        if (window == null || GSD2.OldSrcBase.w_getFlg(window, 0x100))
        {
            return;
        }

        GSD2.window.WindowMessageDispAll(window);
    }

    [HarmonyPatch(typeof(GSD2.EventOverlayClass.r_action), nameof(GSD2.EventOverlayClass.r_action.DispEventWork))]
    [HarmonyPrefix]
    static void GSD2_CheckMessageCookingMinigame(GSD2.EventOverlayClass.r_action_h.EVENT_WORK ew)
    {
        if (ew == null || (ew.flg & 1) == 0 || ew.pc_type == null || ew.pc_num > ew.pc_type.Count)
        {
            return;
        }

        for (int i = 0; i < ew.pc_num; ++i)
        {
            var window = ew.pc_type[i]?.msg_win;
            if (window == null || !GSD2.OldSrcBase.w_getFlg(window, 1))
            {
                continue;
            }

            GSD2.window.WindowMessageDispAll(window);
        }
    }
}
