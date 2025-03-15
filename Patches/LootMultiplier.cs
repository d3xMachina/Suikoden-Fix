extern alias GSD1;
extern alias GSD2;

using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System;

namespace Suikoden_Fix.Patches;

public class LootMultiplierPatch
{
    private static Il2CppReferenceArray<GSD1.MONSTER_DATA_SO> _gsd1LastMonsterData = null;
    private static Il2CppReferenceArray<GSD2.MONSTER_DATA> _gsd2LastMonsterData = null;

    static byte MultiplyLoot(byte chance)
    {
        var multiplier = (double)Plugin.Config.LootMultiplier.Value;
        var newChance = chance * multiplier;

        if (multiplier > 0 && chance > 0 && newChance < 1)
        {
            // guarantee you have at least 1% chance to loot if LootMultiplier is positive
            newChance = 1;
        }
        else
        {
            newChance = Math.Clamp(Math.Round(newChance), 0, 100);
        }

        return (byte)newChance;
    }

    [HarmonyPatch(typeof(GSD1.BattleBase), nameof(GSD1.BattleBase.get_monster_okane))]
    //[HarmonyPatch(typeof(GSD1.Fmain_c), nameof(GSD1.Fmain_c.fieldClose2))]
    //[HarmonyPatch(typeof(GSD1.Event_c), nameof(GSD1.Event_c.eventBattle))]
    [HarmonyPrefix]
    static void GSD1_ChangeMonsterData()
    {
        var monsterDatas = GSD1.BattleBase.game_work?.battle_data?.battle_ground?.monster_data;
        if (monsterDatas == null || monsterDatas == _gsd1LastMonsterData)
        {
            return;
        }

        foreach (var monsterData in monsterDatas)
        {
            if (monsterData == null)
            {
                continue;
            }

            var items = monsterData.item;
            if (items == null)
            {
                continue;
            }

            // First Elem: item ID, Second Elem: Loot chance (up to 100), and repeat...
            for (int i = 1; i < items.Count; i += 2)
            {
                items[i] = MultiplyLoot(items[i]);
                //Plugin.Log.LogInfo($"Item LootRate={items[i]}");
            }
        }

        _gsd1LastMonsterData = monsterDatas;
    }

    [HarmonyPatch(typeof(GSD2.BattleManager), nameof(GSD2.BattleManager.get_monster_okane))]
    [HarmonyPrefix]
    static void GSD2_ChangeMonsterData()
    {
        var monsterDatas = GSD2.BattleManager.sys_work?.battle_data?.monster_data;
        if (monsterDatas == null || monsterDatas == _gsd2LastMonsterData)
        {
            return;
        }

        foreach (var monsterData in monsterDatas)
        {
            if (monsterData == null)
            {
                continue;
            }

            var items = monsterData.item_prob;
            if (items == null)
            {
                continue;
            }

            for (int i = 0; i < items.Count; ++i)
            {
                items[i] = MultiplyLoot(items[i]);
                //Plugin.Log.LogInfo($"Item LootRate={items[i]}");
            }
        }

        _gsd2LastMonsterData = monsterDatas;
    }
}
