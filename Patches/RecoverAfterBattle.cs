extern alias GSD1;
extern alias GSD2;

using HarmonyLib;

namespace Suikoden_Fix.Patches;

public class RecoverAfterBattlePatch
{
    [HarmonyPatch(typeof(GSD1.BattleBase), nameof(GSD1.BattleBase.battle_end))]
    [HarmonyPrefix]
    static void GSD1_RecoverPlayer()
    {
        var battleDatas = GSD1.BattleBase.battle_work?.p_battle_data;
        if (battleDatas == null)
        {
            return;
        }

        foreach (var battleData in battleDatas)
        {
            var playerData = battleData?.player_data?.player_base;
            if (playerData == null)
            {
                continue;
            }

            if (Plugin.Config.RecoverHpAfterBattle.Value)
            {
                playerData.hp = playerData.max_hp;
            }

            if (Plugin.Config.RecoverMpAfterBattle.Value)
            {
                GSD1.G_monsyo_h.calc_magic_point(playerData); // restore mp
            }
        }
    }

    [HarmonyPatch(typeof(GSD2.BattleManager), nameof(GSD2.BattleManager.BattleEnd))]
    [HarmonyPrefix]
    static void GSD2_RecoverPlayer(GSD2.BattleManager __instance)
    {
        var characterDatas = __instance.battle_work?.c_varia_dat;
        if (characterDatas == null)
        {
            return;
        }

        foreach (var characterData in characterDatas)
        {
            if (characterData == null)
            {
                continue;
            }

            if (Plugin.Config.RecoverHpAfterBattle.Value)
            {
                characterData.now_hp = characterData.max_hp;
            }

            if (Plugin.Config.RecoverMpAfterBattle.Value &&
                characterData.mp != null)
            {
                for (int i = 0; i < characterData.mp.Length; ++i)
                {
                    // current MP on 4 MSB and max MP on 4 LSB
                    var maxMp = characterData.mp[i] & 0xF;
                    characterData.mp[i] = (byte)(maxMp | (maxMp << 4));
                }
            }
        }
    }
}
