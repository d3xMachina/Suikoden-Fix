//#define DEBUG_PATCH

extern alias GSD1;
extern alias GSD2;

using HarmonyLib;

namespace Suikoden_Fix.Patches;

public class InputMovementsPatch
{
    public struct PadData
    {
        public bool ok;
        public uint data;
        public bool cancelPressed;
        public GRInputManager.SingleKey cancelKey;
    }

    private static bool _dashToggle = false;
    private static bool _lastDash = false;
    private static uint _lastPadData = 0;
    private static uint _lastPadDataSanitized = 0;
    private static bool _ignoreNextDashInput = false;

    static uint HandleDash(uint padData)
    {
        const uint DashBit = 0x40;

        // Change the bit corresponding to the dash button
        var dash = (DashBit & padData) != 0;

        if (dash != _lastDash)
        {
            if (ModComponent.Instance.IsMenuOpened ||
                ModComponent.Instance.IsMessageBoxOpened ||
                ModComponent.Instance.IsInSpecialMenu)
            {
                _ignoreNextDashInput = true;
            }
            else if (dash)
            {
                if (_ignoreNextDashInput)
                {
                    _ignoreNextDashInput = false;
                }
                else
                {
                    _dashToggle = !_dashToggle;
                }
            }
            
            _lastDash = dash;
        }

        if (_dashToggle)
        {
            padData |= DashBit;
        }
        else
        {
            padData &= ~DashBit;
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

        if ((padData & PadMask) == _lastPadData)
        {
            padData &= ~PadMask;
            padData |= _lastPadDataSanitized;
        }
        else
        {
            _lastPadData = padData & PadMask;

            // 2 directions pressed, in case of irresolvable conflict we ignore the input, other priorize the most recent input
            if ((_lastPadData & PadXMask) != 0 && (_lastPadData & PadYMask) != 0)
            {
                // Irresolvable conflict as the previous input doesn't overlap
                if ((_lastPadDataSanitized & PadXMask) != (_lastPadData & PadXMask) && (_lastPadDataSanitized & PadYMask) != (_lastPadData & PadYMask))
                {
                    padData &= ~PadMask;
                }
                // Up-Right and Up-Left
                else if ((_lastPadData & UpBit) != 0 && (_lastPadData & RightBit) != 0 ||
                        (_lastPadData & UpBit) != 0 && (_lastPadData & LeftBit) != 0)
                {
                    if ((_lastPadDataSanitized & UpBit) != 0)
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
                    if ((_lastPadDataSanitized & DownBit) != 0)
                    {
                        padData &= ~PadYMask;
                    }
                    else
                    {
                        padData &= ~PadXMask;
                    }
                }
            }

            _lastPadDataSanitized = padData & PadMask;
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
            cancelKey = null,
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
                syswork.PadData = HandleDash(syswork.PadData);
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
        if (!__state.ok)
        {
            return;
        }

        // Restore the PadData value
        var syswork = GSD1::Pad.sys_work;
        if (syswork != null)
        {
            syswork.PadData = __state.data;
        }
    }

    [HarmonyPatch(typeof(GSD2::EVENTCON), nameof(GSD2::EVENTCON.NewEventPlayerMove))]
    [HarmonyPrefix]
    static void GSSD2_NewEventPlayerMove(out PadData __state)
    {
        var padData = new PadData
        {
            data = 0,
            cancelPressed = false,
            cancelKey = null,
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
                syswork.pad_dat = HandleDash(syswork.pad_dat);

                var keys = GRInputManager.Instance?.keys;
                if (keys != null && keys.TryGetValue(GRInputManager.Type.Cancel, out var key))
                {
                    // Backup the cancel key
                    padData.cancelKey = key;
                    padData.cancelPressed = key.press;

                    key.press = _dashToggle;
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
        if (!__state.ok)
        {
            return;
        }

        // Restore the PadData value
        var syswork = GSD2::EVENTCON.sys_work;
        if (syswork != null)
        {
            syswork.pad_dat = __state.data;
        }

        // Restore the cancel key
        if (__state.cancelKey != null)
        {
            __state.cancelKey.press = __state.cancelPressed;
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
