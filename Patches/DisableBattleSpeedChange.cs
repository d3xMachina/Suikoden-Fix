extern alias GSD1;
extern alias GSD2;

using HarmonyLib;

namespace Suikoden_Fix.Patches;

public class DisableBattleSpeedChangePatch
{
    [HarmonyPatch(typeof(GSD1.BattleBase), nameof(GSD1.BattleBase.BattleSpeedChange))]
    [HarmonyPrefix]
    static bool GSD1_BattleSpeedChange()
    {
        return false;
    }

    [HarmonyPatch(typeof(GSD2.BattleManager), nameof(GSD2.BattleManager.BattleSpeedChange))]
    [HarmonyPrefix]
    static bool GSD2_BattleSpeedChange()
    {
        return false;
    }
}
