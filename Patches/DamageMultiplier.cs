extern alias GSD1;
extern alias GSD2;

using HarmonyLib;
using System;

namespace Suikoden_Fix.Patches;

public class DamageMultiplierPatch
{
    private const int MaxDamage = 9999;

    static int MultiplyDamage(int damage, bool isPlayer)
    {
        var multiplier = isPlayer ? Plugin.Config.PlayerDamageMultiplier.Value : Plugin.Config.MonsterDamageMultiplier.Value;
        return (int)Math.Clamp(Math.Round(damage * (double)multiplier), 0, MaxDamage);
    }

    static bool GSD1_TryIsPlayer(int charaNo, out bool isPlayer)
    {
        var battleWork = GSD1.BattleBase.battle_work;

        if (battleWork != null && charaNo < battleWork.pm_data.Count)
        {
            var pmData = battleWork.pm_data[charaNo];
            isPlayer = pmData.i_name == 0; // not sure if it always work
            return true;
        }

        isPlayer = true;
        return false;
    }

    static int GSD1_MultiplyDamage(int damage, int charaNo)
    {
        if (GSD1_TryIsPlayer(charaNo, out var isPlayer))
        {
            return MultiplyDamage(damage, isPlayer);
        }

        return damage;
    }

    [HarmonyPatch(typeof(GSD1.BattleBase), nameof(GSD1.BattleBase.calc_attack_damage))]
    [HarmonyPatch(typeof(GSD1.BattleBase), nameof(GSD1.BattleBase.calc_magic_damage))]
    [HarmonyPostfix]
    static void GSD1_CalcDamage(int attack, int guard, ref int __result)
    {
        __result = GSD1_MultiplyDamage(__result, attack);
    }

    [HarmonyPatch(typeof(GSD1.BattleBase), nameof(GSD1.BattleBase.DamageHantei))]
    [HarmonyPostfix]
    static void GSD1_CalcDamageHantei(int magic_level, int zokusei, int kogeki_mokuhyo, int player_no, int damage, ref int __result)
    {
        __result = GSD1_MultiplyDamage(__result, player_no);
    }

    [HarmonyPatch(typeof(GSD1.BattleBase), nameof(GSD1.BattleBase.DamageHantei2))]
    [HarmonyPostfix]
    static void GSD1_CalcDamageHantei2(int zokusei1, int zokusei2, int kogeki_mokuhyo, int player_no, int damage, ref int __result)
    {
        __result = GSD1_MultiplyDamage(__result, player_no);
    }

    [HarmonyPatch(typeof(GSD2.BattlePlayerCharacter), nameof(GSD2.BattlePlayerCharacter.CalcPlayerDamage))]
    [HarmonyPatch(typeof(GSD2.BattlePlayerCharacter), nameof(GSD2.BattlePlayerCharacter.PlayerCalcMagicDamage))]
    [HarmonyPatch(typeof(GSD2.BattlePlayerCharacter), nameof(GSD2.BattlePlayerCharacter.PlayerCalcBouhatuDamage))]
    [HarmonyPatch(typeof(GSD2.BattlePlayerCharacter), nameof(GSD2.BattlePlayerCharacter.PlayerClacCombMagicDamage))]
    // Already multiplied by CalcPlayerDamage
    // [HarmonyPatch(typeof(GSD2.BattlePlayerCharacter), nameof(GSD2.BattlePlayerCharacter.PlayerCalcSpcDamage))]
    [HarmonyPostfix]
    static void GSD2_CalcPlayerDamage(ref int __result)
    {
        __result = MultiplyDamage(__result, true);
    }

    [HarmonyPatch(typeof(GSD2.BattleMonsterCharacter), nameof(GSD2.BattleMonsterCharacter.CalcMonsterDamage))]
    [HarmonyPatch(typeof(GSD2.BattleMonsterCharacter), nameof(GSD2.BattleMonsterCharacter.CalcMonsterHangekiDamage))]
    [HarmonyPatch(typeof(GSD2.boss_las), nameof(GSD2.boss_las.CalcMonsterDamageLast))]
    [HarmonyPostfix]
    static void GSD2_CalcMonsterDamage(ref int __result)
    {
        __result = MultiplyDamage(__result, false);
    }
}
