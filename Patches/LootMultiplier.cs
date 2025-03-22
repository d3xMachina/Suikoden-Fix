extern alias GSD1;
extern alias GSD2;

using HarmonyLib;
using System;
using System.Collections.Generic;

namespace Suikoden_Fix.Patches;

public class LootMultiplierPatch
{
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
    [HarmonyPrefix]
    static void GSD1_ChangeMonsterData(ref List<byte[]> __state)
    {
        var monsterDatas = GSD1.BattleBase.battle_work?.monster_data_table;
        if (monsterDatas == null)
        {
            return;
        }

        __state = new();

        foreach (var monsterData in monsterDatas)
        {
            var items = monsterData?.item;
            if (items == null)
            {
                continue;
            }

            __state.Add(items);

            // First Elem: item ID, Second Elem: Loot chance (up to 100), and repeat...
            for (int i = 1; i < items.Count; i += 2)
            {
                items[i] = MultiplyLoot(items[i]);
                //Plugin.Log.LogInfo($"Item LootRate={items[i]}");
            }
        }
    }

    [HarmonyPatch(typeof(GSD1.BattleBase), nameof(GSD1.BattleBase.get_monster_okane))]
    [HarmonyPostfix]
    static void GSD1_RestoreMonsterData(List<byte[]> __state)
    {
        var monsterDatas = GSD1.BattleBase.battle_work?.monster_data_table;
        if (monsterDatas == null)
        {
            return;
        }

        int i = 0;
        foreach (var monsterData in monsterDatas)
        {
            if (monsterData?.item == null)
            {
                continue;
            }

            var backupItem = __state[i];
            for (int j = 0; j < backupItem.Length; ++j)
            {
                monsterData.item[j] = backupItem[j];
            }
            ++i;
        }
    }

    [HarmonyPatch(typeof(GSD2.BattleManager), nameof(GSD2.BattleManager.get_monster_okane))]
    [HarmonyPrefix]
    static void GSD2_ChangeMonsterData(GSD2.BattleManager __instance, ref List<byte[]> __state)
    {
        var monsterDatas = __instance.battle_work?.monster_data;
        if (monsterDatas == null)
        {
            return;
        }

        __state = new();

        foreach (var monsterData in monsterDatas)
        {
            var items = monsterData?.item_prob;
            if (items == null)
            {
                continue;
            }

            __state.Add(items);

            for (int i = 0; i < items.Count; ++i)
            {
                items[i] = MultiplyLoot(items[i]);
                //Plugin.Log.LogInfo($"Item LootRate={items[i]}");
            }
        }
    }

    [HarmonyPatch(typeof(GSD2.BattleManager), nameof(GSD2.BattleManager.get_monster_okane))]
    [HarmonyPostfix]
    static void GSD2_RestoreMonsterData(GSD2.BattleManager __instance, List<byte[]> __state)
    {
        var monsterDatas = __instance.battle_work?.monster_data;
        if (monsterDatas == null)
        {
            return;
        }

        int i = 0;
        foreach (var monsterData in monsterDatas)
        {
            if (monsterData?.item_prob == null)
            {
                continue;
            }

            var backupItem = __state[i];
            for (int j = 0; j < backupItem.Length; ++j)
            {
                monsterData.item_prob[j] = backupItem[j];
            }
            ++i;
        }
    }
}
