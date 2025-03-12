extern alias GSD1;
extern alias GSD2;

using HarmonyLib;
using System;

namespace Suikoden_Fix.Patches;

public class ExperienceMultiplierPatch
{
    static int MultiplyExperience(int experience)
    {
        return Math.Clamp((int)Math.Round(experience * (double)Plugin.Config.ExperienceMultiplier.Value), 0, int.MaxValue);
    }

    [HarmonyPatch(typeof(GSD1.BattleBase), nameof(GSD1.BattleBase.calc_exp))]
    [HarmonyPostfix]
    static void GSD1_CalcExperience()
    {
        var expTable = GSD1.BattleBase.exit_work?.exp;
        if (expTable != null)
        {
            for (int i = 0; i < expTable.Length; ++i)
            {
                expTable[i] = MultiplyExperience(expTable[i]);
            }
        }
    }

    [HarmonyPatch(typeof(GSD2.BattleManager), nameof(GSD2.BattleManager.calc_exp))]
    [HarmonyPostfix]
    static void GSD2_CalcExperience(GSD2.BATTLE_WORK battle_work)
    {
        var expTable = battle_work.exit_work?.exp;
        if (expTable != null)
        {
            for (int i = 0; i < expTable.Length; ++i)
            {
                expTable[i] = MultiplyExperience(expTable[i]);
            }
        }
    }
}
