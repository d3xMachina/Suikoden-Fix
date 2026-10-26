extern alias GSD1;
extern alias GSD2;

using BepInEx.Configuration;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System;
using System.Globalization;

namespace Suikoden_Fix.Patches;

file class StatsMultiplierPatch
{
    public static bool ParseStatsFromConfig(ConfigEntry<string> entry, float[] statMultiplier)
    {
        string[] parts = entry.Value.Split(',');

        if (parts.Length != statMultiplier.Length)
        {
            Plugin.Log.LogError($"Invalid value configured for {entry.Definition.Key}. It should have {statMultiplier.Length} numbers separated by a comma.");
            return false;
        }

        for (int i = 0; i < parts.Length; ++i)
        {
            if (!float.TryParse(parts[i], NumberStyles.Float, CultureInfo.InvariantCulture, out statMultiplier[i]))
            {
                Plugin.Log.LogError($"Invalid value configured for {entry.Definition.Key}. Invalid float at index {i}: '{parts[i]}'.");
                return false;
            }
        }

        return true;
    }
}

public class StatsMultiplierGSD1Patch
{
    private enum Stat : int
    {
        // Respect "noryoku" order
        Strength,
        Dexterity,
        Protection,
        Speed,
        Magic,
        Luck,
        Max
    }

    private static readonly float[] _statMultiplier = new float[(int)Stat.Max];
    private static bool _initialized = false;

    private static ushort MultiplyStat(Stat type, ushort stat)
    {
        var multiplier = _statMultiplier[(int)type];
        return (ushort)Math.Clamp(Math.Round(stat * (double)multiplier), 0, ushort.MaxValue);
    }

    private static void Initialize()
    {
        _initialized = StatsMultiplierPatch.ParseStatsFromConfig(Plugin.Config.MonsterStatMultiplier, _statMultiplier);
    }

    [HarmonyPatch(typeof(GSD1.BattleBase), nameof(GSD1.BattleBase.battle))]
    [HarmonyPostfix]
    private static void GSD1_ChangeMonsterStats()
    {
        var monsterDatas = GSD1.BattleBase.battle_work?.monster_data_table;
        if (!_initialized || monsterDatas == null)
        {
            return;
        }

        foreach (var monsterData in monsterDatas)
        {
            var stats = monsterData?.noryoku;
            if (stats == null || stats.Length > (int)Stat.Max)
            {
                continue;
            }

            // BATTLE_WORK.monster_data_table is a shallow copy of VIL_BATTLE.monster_data_table
            // Creating a new array avoid changing both
            var newStats = new Il2CppStructArray<ushort>(stats.Length);
            monsterData.noryoku = newStats;

            for (int i = 0; i < stats.Length; ++i)
            {
                newStats[i] = MultiplyStat((Stat)i, stats[i]);
            }

            //var monsterName = GSD1.G1_string_id_h.G1_SID_MSG(monsterData.name);
            //Plugin.Log.LogWarning($"name={monsterName} name_id={monsterData.name} para=[{newStats.Join()}]");
        }
    }
}

public class StatsMultiplierGSD2Patch
{
    private enum Stat : int
    {
        // Respect "para" order
        Strength,
        Magic,
        Protection,
        MagicDefense,
        Dexterity,
        Speed,
        Luck,
        // Extra
        Attack,
        Max,
    }

    private static readonly float[] _statMultiplier = new float[(int)Stat.Max];
    private static bool _initialized = false;

    private static ushort MultiplyStat(Stat type, ushort stat)
    {
        var multiplier = _statMultiplier[(int)type];
        return (ushort)Math.Clamp(Math.Round(stat * (double)multiplier), 0, ushort.MaxValue);
    }

    private static void Initialize()
    {
        _initialized = StatsMultiplierPatch.ParseStatsFromConfig(Plugin.Config.MonsterStatMultiplier2, _statMultiplier);
    }

    [HarmonyPatch(typeof(GSD2.BattleManager), nameof(GSD2.BattleManager.BattleMainInit))]
    [HarmonyPostfix]
    private static void GSD2_ChangeMonsterStats(GSD2.BattleManager __instance)
    {
        var battleWork = __instance.battle_work;
        if (!_initialized || battleWork == null)
        {
            return;
        }

        var monsterDatas = battleWork.monster_data;
        if (monsterDatas != null)
        {
            foreach (var monsterData in monsterDatas)
            {
                var stats = monsterData?.para;
                if (stats == null || stats.Length > (int)Stat.Luck + 1)
                {
                    continue;
                }

                // BATTLE_WORK.monster_data is a shallow copy
                // Creating a new array avoid changing both
                var newStats = new Il2CppStructArray<ushort>(stats.Length);
                monsterData.para = newStats;

                for (int i = 0; i < stats.Length; ++i)
                {
                    newStats[i] = MultiplyStat((Stat)i, stats[i]);
                }

                //var monsterName = GSD2.string_h.SID_MAIN(monsterData.name_id);
                //Plugin.Log.LogWarning($"name={monsterName} name_id={monsterData.name_id} para=[{newStats.Join()}]");

                var attackData = monsterData.attack_data;
                if (attackData != null)
                {
                    var newAttackData = new Il2CppReferenceArray<GSD2.MONSTER_ATTACK_DATA>(attackData.Length);
                    monsterData.attack_data = newAttackData;

                    for (int i = 0; i < attackData.Length; ++i)
                    {
                        if (attackData[i] == null)
                        {
                            newAttackData[i] = null;
                            continue;
                        }

                        newAttackData[i] = attackData[i].MemberwiseClone().Cast<GSD2.MONSTER_ATTACK_DATA>(); // All members are value type
                        newAttackData[i].attack = MultiplyStat(Stat.Attack, attackData[i].attack);

                        //Plugin.Log.LogWarning($"Attack data [{i}]={newAttackData[i].attack}");
                    }
                }
            }
        }

        var pmData = battleWork.pm_data;
        if (pmData != null)
        {
            foreach (var data in pmData)
            {
                var stats = data?.base_para;
                if (stats == null || data.IsPlayer || stats.Length > (int)Stat.Luck + 1)
                {
                    continue;
                }

                var newStats = new Il2CppStructArray<int>(stats.Length);
                data.base_para = newStats;

                for (int i = 0; i < stats.Length; ++i)
                {
                    newStats[i] = MultiplyStat((Stat)i, (ushort)stats[i]);
                }

                //Plugin.Log.LogWarning($"PM_DATA: name={data.name} base_para=[{newStats.Join()}] para_bonus=[{data.para_bonus.Join()}] calc_para=[{data.calc_para.Join()}]");
            }
        }
    }
}
