extern alias GSD1;
extern alias GSD2;

using HarmonyLib;
using System;
using System.Collections.Generic;

namespace Suikoden_Fix.Patches;

public class LootMultiplierPatch
{
    static byte GSD1_MultiplyLoot(byte chance)
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

    static byte GSD2_MultiplyLoot(byte chance, float capMultiplier)
    {
        var multiplier = (double)Plugin.Config.LootMultiplier.Value;
        var newChance = chance * multiplier * capMultiplier;

        if (multiplier > 0 && chance > 0 && newChance < 1)
        {
            // guarantee you have at least a chance to loot if LootMultiplier is positive
            newChance = 1;
        }
        else
        {
            newChance = Math.Clamp(Math.Round(newChance), 0, 255);
        }

        return (byte)newChance;
    }

    [HarmonyPatch(typeof(GSD1.BattleBase), nameof(GSD1.BattleBase.get_monster_okane))]
    [HarmonyPrefix]
    static void GSD1_ChangeMonsterData(out List<byte[]> __state)
    {
        __state = new();

        var monsterDatas = GSD1.BattleBase.battle_work?.monster_data_table;
        if (monsterDatas == null)
        {
            return;
        }

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
                items[i] = GSD1_MultiplyLoot(items[i]);
                //Plugin.Log.LogInfo($"Item LootRate={items[i]}");
            }
        }
    }

    [HarmonyPatch(typeof(GSD1.BattleBase), nameof(GSD1.BattleBase.get_monster_okane))]
    [HarmonyPostfix]
    static void GSD1_RestoreMonsterData(List<byte[]> __state)
    {
        if (__state.Count == 0)
        {
            return;
        }

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

    /*
     * Explanation of the way the loot system works :
     * - Each monster has an item probably table with 3 items. The probability for an item can be 0 and up to 255.
     * - The cumulative of all items probability should not exceed 255, which would mean an item is guaranteed to drop.
     * - The game will try to roll an item drop for each dead monster until it drops one, or none if it checked all dead monsters.
     */
    [HarmonyPatch(typeof(GSD2.BattleManager), nameof(GSD2.BattleManager.get_monster_okane))]
    [HarmonyPrefix]
    static void GSD2_ChangeMonsterData(GSD2.BATTLE_WORK battle_work, out List<byte[]> __state)
    {
        __state = new();

        var monsterDatas = battle_work?.monster_data;
        if (monsterDatas == null)
        {
            return;
        }

        foreach (var monsterData in monsterDatas)
        {
            var items = monsterData?.item_prob;
            if (items == null)
            {
                continue;
            }

            __state.Add(items);

            int cumulativeChance = 0;
            for (int i = 0; i < items.Count; ++i)
            {
                cumulativeChance += items[i];
            }

            if (cumulativeChance == 0)
            {
                continue;
            }

            // Cap the cumulative probability of items to 255
            var capMultiplier = 1f;
            var expectedCumulativeChance = cumulativeChance * Plugin.Config.LootMultiplier.Value;

            if (expectedCumulativeChance > 255f)
            {
                capMultiplier = 255f / expectedCumulativeChance;
            }

            int newCumulativeChance = 0;
            int indexMaxChance = -1;
            for (int i = 0; i < items.Count; ++i)
            {
                items[i] = GSD2_MultiplyLoot(items[i], capMultiplier);
                newCumulativeChance += items[i];

                //Plugin.Log.LogInfo($"Item lootrate: monster={monsterData.name} chance={items[i]}");

                if (indexMaxChance == -1 || items[i] > items[indexMaxChance])
                {
                    indexMaxChance = i;
                }
            }

            // Fix rounding errors to cap the cumulative probability to 255 by changing a bit the probability of the item with the most chance to loot
            if (expectedCumulativeChance >= 255f)
            {
                items[indexMaxChance] += (byte)(255 - newCumulativeChance);
                //Plugin.Log.LogWarning($"Item lootrate adjusted: index={indexMaxChance} chance={items[indexMaxChance]}");
            }
        }
    }

    [HarmonyPatch(typeof(GSD2.BattleManager), nameof(GSD2.BattleManager.get_monster_okane))]
    [HarmonyPostfix]
    static void GSD2_RestoreMonsterData(GSD2.BATTLE_WORK battle_work, List<byte[]> __state)
    {
        if (__state.Count == 0)
        {
            return;
        }

        var monsterDatas = battle_work?.monster_data;
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
