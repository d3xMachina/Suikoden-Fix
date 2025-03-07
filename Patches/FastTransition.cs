extern alias GSD1;
extern alias GSD2;

using HarmonyLib;

namespace Suikoden_Fix.Patches;

// Don't change the order
public enum TransitionState
{
    None,
    // Zone changes
    FieldInit,
    OpenMapMain,
    Village01,
    // Title menu
    TitleMain,
    // Loading screen
    UILoadingAnim
}

public class FastTransitionPatch
{
    static void ApplyTransitionFactor(ref float time)
    {
        var transition = ModComponent.Instance.transition;

        if (transition != TransitionState.None)
        {
            var factor = 1f;

            if (transition >= TransitionState.UILoadingAnim)
            {
                factor = Plugin.Config.LoadingTransitionFactor.Value;
            }
            else if (transition >= TransitionState.TitleMain)
            {
                factor = Plugin.Config.TitleMenuTransitionFactor.Value;
            }
            else if (transition >= TransitionState.FieldInit)
            {
                factor = Plugin.Config.ZoneTransitionFactor.Value;
            }

            time = factor > 0f ? time / factor : 0f;
        }
    }

    [HarmonyPatch(typeof(UIManager), nameof(UIManager.FadeIn), [typeof(float), typeof(Il2CppSystem.Action)])]
    [HarmonyPrefix]
    static void FadeIn1(ref float time, Il2CppSystem.Action onEnd)
    {
        if (ModComponent.Instance.transition != TransitionState.None)
        {
            ApplyTransitionFactor(ref time);
            Plugin.Log.LogInfo($"FADE IN 1: {time}s");
        }
    }

    [HarmonyPatch(typeof(UIManager), nameof(UIManager.FadeIn), [typeof(float)])]
    [HarmonyPrefix]
    static void FadeIn2(ref float time)
    {
        if (ModComponent.Instance.transition != TransitionState.None)
        {
            ApplyTransitionFactor(ref time);
            //Plugin.Log.LogInfo($"FADE IN 2: {time}s");
        }
    }

    [HarmonyPatch(typeof(UIManager), nameof(UIManager.FadeInHalf))]
    [HarmonyPrefix]
    static void FadeInHalf(ref float time)
    {
        if (ModComponent.Instance.transition != TransitionState.None)
        {
            ApplyTransitionFactor(ref time);
            //Plugin.Log.LogInfo($"FADE IN HALF: {time}s");
        }
    }

    [HarmonyPatch(typeof(UIManager), nameof(UIManager.FadeOut))]
    [HarmonyPrefix]
    static void FadeOut(ref float time, Il2CppSystem.Action onEnd)
    {
        if (ModComponent.Instance.transition != TransitionState.None)
        {
            ApplyTransitionFactor(ref time);
            //Plugin.Log.LogInfo($"FADE OUT: {time}s");
        }
    }

    [HarmonyPatch(typeof(UIManager), nameof(UIManager.FadeWhiteOut))]
    [HarmonyPrefix]
    static void FadeWhiteOut(ref float time, Il2CppSystem.Action onEnd)
    {
        if (ModComponent.Instance.transition != TransitionState.None)
        {
            ApplyTransitionFactor(ref time);
            //Plugin.Log.LogInfo($"FADEWHITE OUT: {time}s");
        }
    }
}

public class FastZoneTransitionPatch
{
    [HarmonyPatch(typeof(GSD1::Fmain_c), nameof(GSD1::Fmain_c.fieldInit))]
    [HarmonyPrefix]
    static void GSD1_fieldInit()
    {
        ModComponent.Instance.transition = TransitionState.FieldInit;
    }

    [HarmonyPatch(typeof(GSD1::Village_c), nameof(GSD1::Village_c.village01))]
    [HarmonyPrefix]
    static void GSD1_village01()
    {
        ModComponent.Instance.transition = TransitionState.Village01;
    }

    [HarmonyPatch(typeof(GSD1::Village_c), nameof(GSD1::Village_c.open_map_main))]
    [HarmonyPrefix]
    static void GSD1_OpenMapMain()
    {
        ModComponent.Instance.transition = TransitionState.OpenMapMain;
    }

    [HarmonyPatch(typeof(GSD1::Fmain_c), nameof(GSD1::Fmain_c.fieldInit))]
    [HarmonyPatch(typeof(GSD1::Village_c), nameof(GSD1::Village_c.open_map_main))]
    [HarmonyPatch(typeof(GSD1::Village_c), nameof(GSD1::Village_c.village01))]
    [HarmonyPostfix]
    static void ZonePost()
    {
        ModComponent.Instance.transition = TransitionState.None;
    }
}

public class FastLoadingTransitionPatch
{
    [HarmonyPatch(typeof(GSD1::UILoadingAnim._PlayGameStart_d__11), nameof(GSD1::UILoadingAnim._PlayGameStart_d__11.MoveNext))]
    [HarmonyPatch(typeof(GSD1::UILoadingAnim._PlayTitle_d__10), nameof(GSD1::UILoadingAnim._PlayTitle_d__10.MoveNext))]
    [HarmonyPrefix]
    static void GSD1_UILoadingAnim()
    {
        ModComponent.Instance.transition = TransitionState.UILoadingAnim;
    }

    [HarmonyPatch(typeof(GSD1::UILoadingAnim._PlayGameStart_d__11), nameof(GSD1::UILoadingAnim._PlayGameStart_d__11.MoveNext))]
    [HarmonyPatch(typeof(GSD1::UILoadingAnim._PlayTitle_d__10), nameof(GSD1::UILoadingAnim._PlayTitle_d__10.MoveNext))]
    [HarmonyPostfix]
    static void ZonePost()
    {
        ModComponent.Instance.transition = TransitionState.None;
    }
}

public class FastTitleMenuTransitionPatch
{
    [HarmonyPatch(typeof(GSD1::TitleChapter), nameof(GSD1::TitleChapter.TitleMain))]
    [HarmonyPrefix]
    static void GSD1_TitleMain()
    {
        ModComponent.Instance.transition = TransitionState.TitleMain;
    }

    [HarmonyPatch(typeof(GSD1::TitleChapter), nameof(GSD1::TitleChapter.TitleMain))]
    [HarmonyPostfix]
    static void ZonePost()
    {
        ModComponent.Instance.transition = TransitionState.None;
    }
}