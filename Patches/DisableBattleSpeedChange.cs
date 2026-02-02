extern alias GSD1;
extern alias GSD2;

using HarmonyLib;

namespace Suikoden_Fix.Patches;

public class DisableBattleSpeedChangePatch
{
    [HarmonyPatch(typeof(GSD1.BattleBase), nameof(GSD1.BattleBase.BattleSpeedChange))]
    [HarmonyPatch(typeof(GSD2.BattleManager), nameof(GSD2.BattleManager.BattleSpeedChange))]
    [HarmonyPrefix]
    static bool DisableBattleSpeedChange()
    {
        return false;
    }
}
