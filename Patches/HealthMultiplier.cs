extern alias GSD1;
extern alias GSD2;

using HarmonyLib;
using System;

namespace Suikoden_Fix.Patches;

public class HealthMultiplierPatch
{
    static ushort MultiplyHealth(ushort health)
    {
        var multiplier = Plugin.Config.MonsterHealthMultiplier.Value;
        return (ushort)Math.Clamp(Math.Round(health * (double)multiplier), 1, short.MaxValue);
    }

    [HarmonyPatch(typeof(GSD1.BattleBase), nameof(GSD1.BattleBase.battle))]
    [HarmonyPostfix]
    static void GSD1_ChangeMonsterHealth()
    {
        var monsterDatas = GSD1.BattleBase.battle_work?.monster_data_table;
        if (monsterDatas == null)
        {
            return;
        }

        foreach (var monsterData in monsterDatas)
        {
            if (monsterData == null)
            {
                continue;
            }

            monsterData.hp = MultiplyHealth(monsterData.hp);
            //Plugin.Log.LogWarning($"HP: id={monsterData.name} hp={monsterData.hp}");
        }
    }

    [HarmonyPatch(typeof(GSD2.BattleManager), nameof(GSD2.BattleManager.BattleMainInit))]
    [HarmonyPostfix]
    static void GSD2_ChangeMonsterHealth(GSD2.BattleManager __instance)
    {
        var battleWork = __instance.battle_work;
        if (battleWork == null)
        {
            return;
        }

        var monsterDatas = battleWork.monster_data;
        if (monsterDatas != null)
        {
            foreach (var monsterData in monsterDatas)
            {
                if (monsterData == null)
                {
                    continue;
                }

                monsterData.hp = MultiplyHealth(monsterData.hp);
                //Plugin.Log.LogWarning($"HP: id={monsterData.name} hp={monsterData.hp}");
            }
        }

        var pmData = battleWork.pm_data;
        if (pmData != null)
        {
            foreach (var data in pmData)
            {
                if (data == null || data.IsPlayer)
                {
                    continue;
                }

                data.max_hp = MultiplyHealth(data.max_hp);
                data.hp = (short)MultiplyHealth((ushort)data.hp);
                //Plugin.Log.LogWarning($"PM_DATA: name={data.name} max_hp={data.max_hp} hp={data.hp} id={data.chara_id} no={data.chara_no}");
            }
        }
    }
}
