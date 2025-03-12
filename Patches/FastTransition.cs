extern alias GSD1;
extern alias GSD2;

using HarmonyLib;
using UnityEngine;

namespace Suikoden_Fix.Patches;

// Don't change the order
public enum TransitionState
{
    None,
    Zone,
    TitleMenu,
    Loading
}

public class FastTransitionPatch
{
    public static TransitionState Transition = TransitionState.None;

    static float ApplyTransitionFactor(float time)
    {
        if (Transition != TransitionState.None)
        {
            var factor = 1f;

            switch (Transition)
            {
                case TransitionState.Zone:
                    factor = Plugin.Config.ZoneTransitionFactor.Value;
                    break;
                case TransitionState.TitleMenu:
                    factor = Plugin.Config.TitleMenuTransitionFactor.Value;
                    break;
                case TransitionState.Loading:
                    factor = Plugin.Config.LoadingTransitionFactor.Value;
                    break;
                default:
                    break;
            }

            time = factor > 0f ? time / factor : 0f;
        }

        return time;
    }

    static int ApplyTransitionFactor(int timer)
    {
        return (int)Mathf.Round(ApplyTransitionFactor((float)timer));
    }

    static ushort ApplyTransitionFactor(ushort timer)
    {
        return (ushort)ApplyTransitionFactor((int)timer);
    }

    [HarmonyPatch(typeof(UIManager), nameof(UIManager.FadeIn), [typeof(float), typeof(Il2CppSystem.Action)])]
    [HarmonyPrefix]
    static void FadeIn1(ref float time, Il2CppSystem.Action onEnd)
    {
        if (Transition != TransitionState.None)
        {
            time = ApplyTransitionFactor(time);
            //Plugin.Log.LogInfo($"FadeIn1: {time}s");
        }
    }

    [HarmonyPatch(typeof(UIManager), nameof(UIManager.FadeIn), [typeof(float)])]
    [HarmonyPrefix]
    static void FadeIn2(ref float time)
    {
        if (Transition != TransitionState.None)
        {
            time = ApplyTransitionFactor(time);
            //Plugin.Log.LogInfo($"FadeIn2: {time}s");
        }
    }

    [HarmonyPatch(typeof(UIManager), nameof(UIManager.FadeInHalf))]
    [HarmonyPrefix]
    static void FadeInHalf(ref float time)
    {
        if (Transition != TransitionState.None)
        {
            time = ApplyTransitionFactor(time);
            //Plugin.Log.LogInfo($"FadeInHalf: {time}s");
        }
    }

    [HarmonyPatch(typeof(UIManager), nameof(UIManager.FadeOut))]
    [HarmonyPrefix]
    static void FadeOut(ref float time, Il2CppSystem.Action onEnd)
    {
        if (Transition != TransitionState.None)
        {
            time = ApplyTransitionFactor(time);
            //Plugin.Log.LogInfo($"FadeOut: {time}s");
        }
    }

    [HarmonyPatch(typeof(UIManager), nameof(UIManager.FadeWhiteOut))]
    [HarmonyPrefix]
    static void FadeWhiteOut(ref float time, Il2CppSystem.Action onEnd)
    {
        if (Transition != TransitionState.None)
        {
            time = ApplyTransitionFactor(time);
            //Plugin.Log.LogInfo($"FadeWhiteOut: {time}s");
        }
    }

    [HarmonyPatch(typeof(GSD1.G1_BACK), nameof(GSD1.G1_BACK.FilterBlackIn))]
    [HarmonyPrefix]
    static void FilterBlackIn(int timer)
    {
        if (Transition != TransitionState.None)
        {
            timer = ApplyTransitionFactor(timer);
            //Plugin.Log.LogInfo($"FilterBlackIn: timer={timer}");
        }
    }

    [HarmonyPatch(typeof(GSD1.G1_BACK), nameof(GSD1.G1_BACK.FilterBlackIn))]
    [HarmonyPrefix]
    static void FilterBlackOut(int timer)
    {
        if (Transition != TransitionState.None)
        {
            timer = ApplyTransitionFactor(timer);
            //Plugin.Log.LogInfo($"FilterBlackOut: timer={timer}");
        }
    }

    [HarmonyPatch(typeof(GSD2.G2_BACK), nameof(GSD2.G2_BACK.FilterBlackIn))]
    [HarmonyPrefix]
    static void FilterBlackIn(int pri, ref ushort timer)
    {
        if (Transition != TransitionState.None)
        {
            timer = ApplyTransitionFactor(timer);
            //Plugin.Log.LogInfo($"FilterBlackIn: pri={pri} timer={timer}");
        }
    }

    [HarmonyPatch(typeof(GSD2.G2_BACK), nameof(GSD2.G2_BACK.FilterBlackInHalf))]
    [HarmonyPrefix]
    static void FilterBlackInHalf(int pri, ref ushort timer)
    {
        if (Transition != TransitionState.None)
        {
            timer = ApplyTransitionFactor(timer);
            //Plugin.Log.LogInfo($"FilterBlackInHalf: pri={pri} timer={timer}");
        }
    }

    [HarmonyPatch(typeof(GSD2.G2_BACK), nameof(GSD2.G2_BACK.FilterBlackOut))]
    [HarmonyPrefix]
    static void FilterBlackOut(int pri, ref ushort timer, bool isReserve)
    {
        if (Transition != TransitionState.None)
        {
            timer = ApplyTransitionFactor(timer);
            //Plugin.Log.LogInfo($"FilterBlackOut: pri={pri} timer={timer}");
        }
    }

    [HarmonyPatch(typeof(GSD2.G2_BACK), nameof(GSD2.G2_BACK.FilterBlackOutHalf))]
    [HarmonyPrefix]
    static void FilterBlackOutHalf(int pri, ref ushort timer, bool isReserve)
    {
        if (Transition != TransitionState.None)
        {
            timer = ApplyTransitionFactor(timer);
            //Plugin.Log.LogInfo($"FilterBlackOutHalf: pri={pri} timer={timer}");
        }
    }

    /*
    [HarmonyPatch(typeof(GSD2.G2_BACK), nameof(GSD2.G2_BACK.FilterSet))]
    [HarmonyPrefix]
    static void FilterSet(byte rgb, ref byte timer, Color color)
    {
        Plugin.Log.LogInfo($"FilterSet: rgb={rgb} timer={timer} color={color}");
    }

    [HarmonyPatch(typeof(GSD2.G2_BACK), nameof(GSD2.G2_BACK.FilterSetRGB))]
    [HarmonyPrefix]
    static void FilterSetRGB(byte rgb, ref ushort timer)
    {
        Plugin.Log.LogInfo($"FilterSetRGB: rgb={rgb} timer={timer}");
    }

    [HarmonyPatch(typeof(GSD2.G2_BACK), nameof(GSD2.G2_BACK.BackPolySetRGB))]
    [HarmonyPrefix]
    static void BackPolySetRGB(byte rgb, ref int timer)
    {
        Plugin.Log.LogInfo($"BackPolySetRGB: rgb={rgb} timer={timer}");
    }
    */
}

public class FastZoneTransitionPatch
{
    [HarmonyPatch(typeof(GSD1::Fmain_c), nameof(GSD1::Fmain_c.fieldInit))]
    [HarmonyPatch(typeof(GSD1::Village_c), nameof(GSD1::Village_c.village01))]
    [HarmonyPatch(typeof(GSD1::Village_c), nameof(GSD1::Village_c.open_map_main))]
    [HarmonyPatch(typeof(GSD2::MACHICON), nameof(GSD2::MACHICON.MachiMain))]
    [HarmonyPatch(typeof(GSD2::BattleManager), nameof(GSD2::BattleManager.exit_sub99))]
    [HarmonyPatch(typeof(GSD2::BattleManager), nameof(GSD2::BattleManager.nige_window))]
    [HarmonyPatch(typeof(GSD2::BattleManager), nameof(GSD2::BattleManager.okane_window))]
    [HarmonyPrefix]
    static void Zone()
    {
        FastTransitionPatch.Transition = TransitionState.Zone;
        //Plugin.Log.LogInfo("Zone");
    }

    [HarmonyPatch(typeof(GSD1::Fmain_c), nameof(GSD1::Fmain_c.fieldInit))]
    [HarmonyPatch(typeof(GSD1::Village_c), nameof(GSD1::Village_c.open_map_main))]
    [HarmonyPatch(typeof(GSD1::Village_c), nameof(GSD1::Village_c.village01))]
    [HarmonyPatch(typeof(GSD2::MACHICON), nameof(GSD2::MACHICON.MachiMain))]
    [HarmonyPatch(typeof(GSD2::BattleManager), nameof(GSD2::BattleManager.exit_sub99))]
    [HarmonyPatch(typeof(GSD2::BattleManager), nameof(GSD2::BattleManager.nige_window))]
    [HarmonyPatch(typeof(GSD2::BattleManager), nameof(GSD2::BattleManager.okane_window))]
    [HarmonyPostfix]
    static void ZonePost()
    {
        FastTransitionPatch.Transition = TransitionState.None;
        //Plugin.Log.LogInfo("Zone Post");
    }
}

public class FastLoadingTransitionPatch
{
    [HarmonyPatch(typeof(GSD1::UILoadingAnim._PlayGameStart_d__11), nameof(GSD1::UILoadingAnim._PlayGameStart_d__11.MoveNext))]
    [HarmonyPatch(typeof(GSD1::UILoadingAnim._PlayTitle_d__10), nameof(GSD1::UILoadingAnim._PlayTitle_d__10.MoveNext))]
    [HarmonyPatch(typeof(GSD2::UILoadingAnim._Play_d__12), nameof(GSD2::UILoadingAnim._Play_d__12.MoveNext))]
    [HarmonyPrefix]
    static void Loading()
    {
        FastTransitionPatch.Transition = TransitionState.Loading;
        //Plugin.Log.LogInfo("Loading");
    }

    [HarmonyPatch(typeof(GSD1::UILoadingAnim._PlayGameStart_d__11), nameof(GSD1::UILoadingAnim._PlayGameStart_d__11.MoveNext))]
    [HarmonyPatch(typeof(GSD1::UILoadingAnim._PlayTitle_d__10), nameof(GSD1::UILoadingAnim._PlayTitle_d__10.MoveNext))]
    [HarmonyPatch(typeof(GSD2::UILoadingAnim._Play_d__12), nameof(GSD2::UILoadingAnim._Play_d__12.MoveNext))]
    [HarmonyPostfix]
    static void LoadingPost()
    {
        FastTransitionPatch.Transition = TransitionState.None;
        //Plugin.Log.LogInfo("Loading Post");
    }
}

public class FastTitleMenuTransitionPatch
{
    [HarmonyPatch(typeof(GSD1::TitleChapter), nameof(GSD1::TitleChapter.TitleMain))]
    [HarmonyPatch(typeof(GSD2::TitleChapter), nameof(GSD2::TitleChapter.TitleMain))]
    [HarmonyPrefix]
    static void TitleMenu()
    {
        FastTransitionPatch.Transition = TransitionState.TitleMenu;
        //Plugin.Log.LogInfo("TitleMenu");
    }

    [HarmonyPatch(typeof(GSD1::TitleChapter), nameof(GSD1::TitleChapter.TitleMain))]
    [HarmonyPatch(typeof(GSD2::TitleChapter), nameof(GSD2::TitleChapter.TitleMain))]
    [HarmonyPostfix]
    static void TitleMenuPost()
    {
        FastTransitionPatch.Transition = TransitionState.None;
        //Plugin.Log.LogInfo("TitleMenu Post");
    }
}