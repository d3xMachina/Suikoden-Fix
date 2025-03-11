extern alias GSD1;
extern alias GSD2;

using HarmonyLib;
using System;

namespace Suikoden_Fix.Patches;

public class ExperienceMultiplierPatch
{
    [HarmonyPatch(typeof(GSD1.BattleBase), nameof(GSD1.BattleBase.calc_exp))]
    [HarmonyPostfix]
    static void GSD1_CalcExp()
    {
        var expExit = GSD1.BattleBase.exit_work?.exp;
        if (expExit != null)
        {
            for (int i = 0; i < expExit.Length; ++i)
            {
                expExit[i] = (int)MathF.Round(expExit[i] * Plugin.Config.ExperienceMultiplier.Value);
            }
        }
    }

    [HarmonyPatch(typeof(GSD2.BattleManager), nameof(GSD2.BattleManager.calc_exp))]
    [HarmonyPostfix]
    static void GSD2_CalcExp(GSD2.BATTLE_WORK battle_work)
    {
        var expExit = battle_work.exit_work?.exp;
        if (expExit != null)
        {
            for (int i = 0; i < expExit.Length; ++i)
            {
                expExit[i] = (int)MathF.Round(expExit[i] * Plugin.Config.ExperienceMultiplier.Value);
            }
        }
    }
}
