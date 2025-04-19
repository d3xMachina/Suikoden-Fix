extern alias GSD1;
extern alias GSD2;

using HarmonyLib;

namespace Suikoden_Fix.Patches;

public class EasyMinigamesPatch
{
    private static bool _cardsForceCleared = false;
    private static bool _cardsIgnoreCheckTime = false;

    private struct CardsData
    {
        public int Step;
        public int CurrentTime;
        public short RecordTime;
    }

    // This function determines the dices combinations
    [HarmonyPatch(typeof(GSD1.TurugaiFunc_c), nameof(GSD1.TurugaiFunc_c.chinchirofunc50))]
    [HarmonyPostfix]
    static void GSD1_DiceMinigame(GSD1.TurugaiFunc_c __instance)
    {
        if (__instance.alc?.dise == null)
        {
            return;
        }

        /* 0 = Nothing
         * 1-6 = Roll n
         * 8 = Triple 2/3/4/5/6
         * 16 = Triple 1
         * 32 = 4-5-6
         * 64 = 1-2-3
         * 128 = Dice out
         */
        __instance.alc.dise.st_flg = 16;
        __instance.alc.msgno = 0x10;
    }

    [HarmonyPatch(typeof(GSD1.Ka_main_c), nameof(GSD1.Ka_main_c.ka_coin_loop))]
    [HarmonyPrefix]
    static void GSD1_CardsMinigame(GSD1.Ka_main_c __instance, out CardsData __state)
    {
        __state = new CardsData
        {
            Step = __instance.ka_main_coin_step,
            CurrentTime = -1,
            RecordTime = -1
        };

        _cardsForceCleared = false;
        _cardsIgnoreCheckTime = false;

        if (__instance.ka_main_coin_step == 0x10) // Lost because of timer
        {
            // Remove cards from the field
            if (__instance.TojiruCard(__instance.wait_time) != 0)
            {
                __instance.ka_main_coin_step = 0xF;
                _cardsForceCleared = true;
            }

            ++__instance.wait_time;
        }

        // Make a new record time temporarely to unlock Georges
        if (__instance.ka_main_coin_step == 0xF) // Check clear
        {
            if (__instance.CheckClear() != 0)
            {
                if (!_cardsForceCleared)
                {
                    __instance.TimeCheck();
                }

                __state.CurrentTime = __instance.time_cnt;
                __instance.time_cnt = 0;
                _cardsIgnoreCheckTime = true; // TimeCheck() update the time count
            }
        }
        else if (__instance.ka_main_coin_step == 0x14) // Process record time
        {
            var recordTimes = GSD1.OldSrcBase.game_work?.window_data;
            if (recordTimes != null &&
                __instance.cursor_ichi < recordTimes.Count &&
                __instance.time_cnt >= recordTimes[__instance.cursor_ichi])
            {
                __state.RecordTime = recordTimes[__instance.cursor_ichi]; // Backup the record time
                __state.CurrentTime = __instance.time_cnt;
                __instance.time_cnt = 0;
            }
        }
    }

    [HarmonyPatch(typeof(GSD1.Ka_main_c), nameof(GSD1.Ka_main_c.ka_coin_loop))]
    [HarmonyPostfix]
    static void GSD1_CardsMinigamePost(GSD1.Ka_main_c __instance, CardsData __state)
    {
        // Keep time count at 0 after processing the so it is displayed correctly
        if (__state.CurrentTime != -1 &&
            (__state.Step != 0x14 || __state.Step == __instance.ka_main_coin_step))
        {
            __instance.time_cnt = __state.CurrentTime;
        }
        
        if (__state.RecordTime != -1)
        {
            var recordTimes = GSD1.OldSrcBase.game_work?.window_data;
            recordTimes[__instance.cursor_ichi] = __state.RecordTime; // Restore the record time
        }
    }

    [HarmonyPatch(typeof(GSD1.Ka_main_c), nameof(GSD1.Ka_main_c.CheckClear))]
    [HarmonyPrefix]
    static bool GSD1_CardsMinigameCheckClear(ref int __result)
    {
        if (_cardsForceCleared)
        {
            __result = 1;
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(GSD1.Ka_main_c), nameof(GSD1.Ka_main_c.TimeCheck))]
    [HarmonyPrefix]
    static bool GSD1_CardsMinigameCheckTime()
    {
        return !_cardsIgnoreCheckTime;
    }

    [HarmonyPatch(typeof(GSD1.Co_main_c), nameof(GSD1.Co_main_c.coin_loop))]
    [HarmonyPrefix]
    static void GSD1_CupsMinigame(GSD1.Co_main_c __instance)
    {
        if (__instance.co_main_coin_step == 0xC)
        {
            __instance.yubi_ichi = __instance.coin_ichi;
        }
    }

    [HarmonyPatch(typeof(GSD2.EventOverlayClass.k_phase), nameof(GSD2.EventOverlayClass.k_phase.KikoriPhaseGoal))]
    [HarmonyPostfix]
    static void GSD2_RopeMinigame(GSD2.EventOverlayClass.kikori_h.KIKORI_WORK kikori)
    {
        if (kikori == null || kikori.kikori_step != 'b')
        {
            return;
        }

        kikori.juni_no = 0; // current player
    }

    [HarmonyPatch(typeof(GSD2.EventOverlayClass.Overlay_rbattle), nameof(GSD2.EventOverlayClass.Overlay_rbattle.RbattleMain))]
    [HarmonyPostfix]
    static void GSD2_CookingMinigame(GSD2.EventOverlayClass.Overlay_rbattle __instance)
    {
        var work = __instance.rb_work;
        if (work == null)
        {
            return;
        }

        work.win_flg = 1;
    }

    [HarmonyPatch(typeof(GSD2.EventOverlayClass.ch_main), nameof(GSD2.EventOverlayClass.ch_main.chin_main_sub))]
    [HarmonyPrefix]
    static void GSD2_DiceMinigame(GSD2.EventOverlayClass.chin_h.CHIN_DAM_WORK dw)
    {
        if (dw == null)
        {
            return;
        }

        if (dw.player == 0)
        {
            dw.dice_val = (1 << 8) + (1 << 4) + 1; // triple 1, each dice result is stored on 1 nibble
        }
    }

    [HarmonyPatch(typeof(GSD2.EventOverlayClass.Overlay_h_dance), nameof(GSD2.EventOverlayClass.Overlay_h_dance.DanceMain))]
    [HarmonyPrefix]
    static void GSD2_DanceMinigame(GSD2.EventOverlayClass.Overlay_h_dance __instance)
    {
        var work = __instance.dance_work;
        if (work == null)
        {
            return;
        }

        work.dance_flg = 0;
        work.timing_flg = 0;
    }

    [HarmonyPatch(typeof(GSD2.EventOverlayClass.fish_cal), nameof(GSD2.EventOverlayClass.fish_cal.fish_eat_check))]
    [HarmonyPrefix]
    static bool GSD2_FishMinigameCheckEat(ref int __result)
    {
        var eatData = GSD2.EventOverlayClass.fish_cal.eat_check_dat;
        if (eatData == null || eatData.Length <= 0)
        {
            return true;
        }

        // The fish eat the bait instantly with same probability for every kind of fishes
        var rand = UnityEngine.Random.RandomRangeInt(0, eatData.Length);
        if (eatData[rand] == null)
        {
            Plugin.Log.LogWarning("Eat check data is null!");
            __result = -1;
        }
        else
        {
            __result = (int)eatData[rand].kind;
        }

        return false;
    }

    [HarmonyPatch(typeof(GSD2.EventOverlayClass.Overlay_fishing), nameof(GSD2.EventOverlayClass.Overlay_fishing.OverlayBaseFishingMain))]
    [HarmonyPrefix]
    static void GSD2_FishMinigameMain(GSD2.EventOverlayClass.Overlay_fishing __instance)
    {
        var bfi = __instance.bfi;
        if (bfi == null)
        {
            return;
        }

        bfi.fight_count = 301; // No need to wait to have a fish biting the bait
        bfi.distance = 0; // Instant catch
        
        // Never fail catching minigame
        //bfi.under_count = 0;
        //bfi.over_count = 0;
    }

    [HarmonyPatch(typeof(GSD2.EventOverlayClass.m_main), nameof(GSD2.EventOverlayClass.m_main.MoguraMain))]
    [HarmonyPrefix]
    static void GSD2_MoleMinigame(GSD2.EventOverlayClass.mogura_h.MOGURA_DAM_WORK dw, out int __state)
    {
        __state = -1;

        if (dw == null)
        {
            return;
        }

        if (dw.sub_step == 10)
        {
            dw.mogura_win_flg = 1;

            // the score needs to be better than the highest score to get the reward item in ultimate difficulty
            if (dw.select_mogura_no >= 3 && dw.now_score <= dw.max_score)
            {
                __state = dw.max_score;
                dw.now_score = dw.max_score + 1;
            }
        }
    }

    [HarmonyPatch(typeof(GSD2.EventOverlayClass.m_main), nameof(GSD2.EventOverlayClass.m_main.MoguraMain))]
    [HarmonyPostfix]
    static void GSD2_MoleMinigamePost(GSD2.EventOverlayClass.mogura_h.MOGURA_DAM_WORK dw, int __state)
    {
        if (dw == null || __state == -1)
        {
            return;
        }

        // Restore the previous score so the cheated score is not saved
        dw.max_score = __state;
    }
}
