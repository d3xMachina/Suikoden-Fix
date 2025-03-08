//#define DEBUG_PATCH

extern alias GSD1;
extern alias GSD2;

using HarmonyLib;

namespace Suikoden_Fix.Patches;

public class InputMovementsPatch
{
    public struct PadData
    {
        public uint data;
        public bool ok;
    }

    static uint HandleDash(uint padData, out bool dash)
    {
        const uint DashBit = 0x40;

        // Change the bit corresponding to the dash button
        dash = (DashBit & padData) != 0;
        ref var lastDash = ref ModComponent.Instance.LastDash;
        ref var InvertDash = ref ModComponent.Instance.InvertDash;

        if (dash != lastDash)
        {
            if (!dash)
            {
                InvertDash = !InvertDash;
            }
            
            lastDash = dash;
        }

        if (InvertDash)
        {
            if (dash)
            {
                padData &= ~DashBit; 
            }
            else
            {
                padData |= DashBit;
            }
        }

        return padData;
    }

    static uint HandleDiagonals(uint padData)
    {
        const uint UpBit = 0x1000;
        const uint RightBit = 0x2000;
        const uint DownBit = 0x4000;
        const uint LeftBit = 0x8000;
        const uint PadMask = 0xF000;
        const uint PadXMask = 0xA000;
        const uint PadYMask = 0x5000;

        ref var LastPadData = ref ModComponent.Instance.LastPadData;
        ref var LastPadDataSanitized = ref ModComponent.Instance.LastPadDataSanitized;

        if ((padData & PadMask) == LastPadData)
        {
            padData &= ~PadMask;
            padData |= LastPadDataSanitized;
        }
        else
        {
            LastPadData = padData & PadMask;

            // 2 directions pressed, in case of irresolvable conflict we ignore the input, other priorize the most recent input
            if ((LastPadData & PadXMask) != 0 && (LastPadData & PadYMask) != 0)
            {
                // Irresolvable conflict as the previous input doesn't overlap
                if ((LastPadDataSanitized & PadXMask) != (LastPadData & PadXMask) && (LastPadDataSanitized & PadYMask) != (LastPadData & PadYMask))
                {
                    padData &= ~PadMask;
                }
                // Up-Right and Up-Left
                else if ((LastPadData & UpBit) != 0 && (LastPadData & RightBit) != 0 ||
                        (LastPadData & UpBit) != 0 && (LastPadData & LeftBit) != 0)
                {
                    if ((LastPadDataSanitized & UpBit) != 0)
                    {
                        padData &= ~PadYMask;
                    }
                    else
                    {
                        padData &= ~PadXMask;
                    }
                }
                // Down-Right and Down-Left
                else
                {
                    if ((LastPadDataSanitized & DownBit) != 0)
                    {
                        padData &= ~PadYMask;
                    }
                    else
                    {
                        padData &= ~PadXMask;
                    }
                }
            }

            LastPadDataSanitized = padData & PadMask;
        }

        return padData;
    }

    [HarmonyPatch(typeof(GSD1::Village_c), nameof(GSD1::Village_c.NewPlayerMove))]
    [HarmonyPrefix]
    static void GSSD1_NewPlayerMovePre(int step_level, out PadData __state)
    {
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

            if (Plugin.Config.ToggleDash.Value)
            {
                syswork.PadData = HandleDash(syswork.PadData, out _);
            }

            if (Plugin.Config.DisableDiagonalMovements.Value)
            {
                syswork.PadData = HandleDiagonals(syswork.PadData);
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

    [HarmonyPatch(typeof(GSD2::EVENTCON), nameof(GSD2::EVENTCON.NewEventPlayerMove))]
    [HarmonyPrefix]
    static void GSSD2_NewEventPlayerMove(out PadData __state)
    {
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

            if (Plugin.Config.ToggleDash.Value)
            {
                syswork.pad_dat = HandleDash(syswork.pad_dat, out var dash);

                if (ModComponent.Instance.InvertDash)
                {
                    syswork.g_shu_dash = !dash;
                }
            }

            if (Plugin.Config.DisableDiagonalMovements.Value)
            {
                syswork.pad_dat = HandleDiagonals(syswork.pad_dat);
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

#if DEBUG_PATCH
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

        Plugin.Log.LogWarning($"PadData={syswork.PadData} PadTrig={syswork.PadTrig} PadNTrig={syswork.PadNTrig} PadRTrig={syswork.PadRTrig}");        
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

        Plugin.Log.LogWarning($"PadData={syswork.pad_dat} PadTrig={syswork.pad_trg} PadNTrig={syswork.pad_trg2} PadRTrig={syswork.pad_trg3}");        
    }
#endif
}
