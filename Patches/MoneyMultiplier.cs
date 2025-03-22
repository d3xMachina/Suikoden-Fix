extern alias GSD1;
extern alias GSD2;

using HarmonyLib;
using System;

namespace Suikoden_Fix.Patches;

public class MoneyMultiplierPatch
{
    static int MultiplyMoney(int money)
    {
        var multiplier = Plugin.Config.MoneyMultiplier.Value;
        return (int)Math.Clamp(Math.Round(money * (double)multiplier), 0, int.MaxValue);
    }

    [HarmonyPatch(typeof(GSD1.BattleBase), nameof(GSD1.BattleBase.get_monster_okane))]
    [HarmonyPostfix]
    static void GSD1_GetMonsterMoney()
    {
        var exitWork = GSD1.BattleBase.exit_work;
        if (exitWork != null)
        {
            exitWork.monster_okane = MultiplyMoney(exitWork.monster_okane);
        }
    }

    [HarmonyPatch(typeof(GSD2.BattleManager), nameof(GSD2.BattleManager.get_monster_okane))]
    [HarmonyPostfix]
    static void GSD2_GetMonsterMoney(GSD2.BATTLE_WORK battle_work)
    {
        var exitWork = battle_work.exit_work;
        if (exitWork != null)
        {
            exitWork.monster_okane = MultiplyMoney(exitWork.monster_okane);
        }
    }
}
