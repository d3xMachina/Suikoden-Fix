extern alias GSD1;
extern alias GSD2;

using HarmonyLib;
using Suikoden_Fix;

namespace Suikoden_Fix.Patches;

public class ToggleDashPatch
{
    public struct PadData
    {
        public uint data;
        public bool ok;
    }

    [HarmonyPatch(typeof(GSD1::Pad), nameof(GSD1::Pad.PadUpdate))]
    [HarmonyPostfix]
    static void GSSD1_PadUpdate(bool isInput, bool isUpdate)
    {
        if (!isInput || !isUpdate)
        {
            return;
        }

        var syswork = GSD1::Pad.sys_work;
        if (syswork == null)
        {
            return;
        }

        //Plugin.Log.LogWarning($"PadData={syswork.PadData} PadTrig={syswork.PadTrig} PadNTrig={syswork.PadNTrig} PadRTrig={syswork.PadRTrig}");        
    }

    [HarmonyPatch(typeof(GSD1::Village_c), nameof(GSD1::Village_c.NewPlayerMove))]
    [HarmonyPrefix]
    static void GSSD1_NewPlayerMovePre(int step_level, out PadData __state)
    {
        const uint DashBit = 0x40;

        var padData = new PadData
        {
            data = 0,
            ok = false
        };

        var syswork = GSD1::Pad.sys_work;
        if (syswork != null)
        {
            // Backup the PadData value
            padData.data = syswork.PadData;
            padData.ok = true;

            // Change the bit corresponding to the dash button
            var dash = (DashBit & padData.data) != 0;
            ref var lastDash = ref ModComponent.Instance.LastDash;
            ref var InvertDash = ref ModComponent.Instance.InvertDash;

            if (dash != lastDash)
            {
                if (dash)
                {
                    InvertDash = !InvertDash;
                }
            
                lastDash = dash;
            }

            if (InvertDash)
            {
                if (dash)
                {
                    syswork.PadData &= ~DashBit; 
                }
                else
                {
                    syswork.PadData |= DashBit;
                }
            }
        }

        __state = padData;
    }

    [HarmonyPatch(typeof(GSD1::Village_c), nameof(GSD1::Village_c.NewPlayerMove))]
    [HarmonyPostfix]
    static void GSSD1_NewPlayerMovePost(int step_level, PadData __state)
    {
        // Restore the PadData value
        if (__state.ok)
        {
            var syswork = GSD1::Pad.sys_work;
            if (syswork != null)
            {
                syswork.PadData = __state.data;
            }
        }
    }

    [HarmonyPatch(typeof(GSD2::G2_PAD), nameof(GSD2::G2_PAD.Update))]
    [HarmonyPostfix]
    static void GSSD2_PadUpdate(bool isUpdate)
    {
        if (!isUpdate)
        {
            return;
        }

        var syswork = GSD2::EVENTCON.sys_work;
        if (syswork == null)
        {
            return;
        }

        //Plugin.Log.LogWarning($"PadData={syswork.pad_dat} PadTrig={syswork.pad_trg} PadNTrig={syswork.pad_trg2} PadRTrig={syswork.pad_trg3}");        
    }

    [HarmonyPatch(typeof(GSD2::EVENTCON), nameof(GSD2::EVENTCON.NewEventPlayerMove))]
    [HarmonyPrefix]
    static void GSSD2_NewEventPlayerMove(out PadData __state)
    {
        const uint DashBit = 0x40;

        var padData = new PadData
        {
            data = 0,
            ok = false
        };

        var syswork = GSD2::EVENTCON.sys_work;
        if (syswork != null)
        {
            // Backup the PadData value
            padData.data = syswork.pad_dat;
            padData.ok = true;

            // Change the bit corresponding to the dash button
            var dash = (DashBit & padData.data) != 0;
            ref var lastDash = ref ModComponent.Instance.LastDash;
            ref var InvertDash = ref ModComponent.Instance.InvertDash;

            if (dash != lastDash)
            {
                if (dash)
                {
                    InvertDash = !InvertDash;
                }
            
                lastDash = dash;
            }

            // Does not trigger the dash...
            if (InvertDash)
            {
                if (dash)
                {
                    syswork.pad_dat &= ~DashBit;
                }
                else
                {
                    syswork.pad_dat |= DashBit;
                }

                // should not be used normally
                syswork.g_shu_dash = !dash;
            }
        }

        __state = padData;
    }

    [HarmonyPatch(typeof(GSD2::EVENTCON), nameof(GSD2::EVENTCON.NewEventPlayerMove))]
    [HarmonyPostfix]
    static void GSSD2_NewEventPlayerMovePost(PadData __state)
    {
        // Restore the PadData value
        if (__state.ok)
        {
            var syswork = GSD2::EVENTCON.sys_work;
            if (syswork != null)
            {
                syswork.pad_dat = __state.data;
            }
        }
    }
}
