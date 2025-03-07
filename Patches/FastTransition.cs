extern alias GSD1;
extern alias GSD2;

using HarmonyLib;

namespace Suikoden_Fix.Patches;

public enum TransitionState
{
    None,
    FieldInit,
    OpenMapMain,
    Village01
}

public class FastTransitionPatch
{
    [HarmonyPatch(typeof(UIManager), nameof(UIManager.FadeIn), [typeof(float), typeof(Il2CppSystem.Action)])]
    [HarmonyPrefix]
    static void FadeIn1(ref float time, Il2CppSystem.Action onEnd)
    {
        if (ModComponent.Instance.transition != TransitionState.None)
        {
            time /= Plugin.Config.ZoneTransitionFactor.Value;
            //Plugin.Log.LogInfo($"FADE IN 1: {time}s");
        }
    }

    [HarmonyPatch(typeof(UIManager), nameof(UIManager.FadeIn), [typeof(float)])]
    [HarmonyPrefix]
    static void FadeIn2(ref float time)
    {
        if (ModComponent.Instance.transition != TransitionState.None)
        {
            time /= Plugin.Config.ZoneTransitionFactor.Value;
            //Plugin.Log.LogInfo($"FADE IN 2: {time}s");
        }
    }

    [HarmonyPatch(typeof(UIManager), nameof(UIManager.FadeInHalf))]
    [HarmonyPrefix]
    static void FadeInHalf(ref float time)
    {
        if (ModComponent.Instance.transition != TransitionState.None)
        {
            time /= Plugin.Config.ZoneTransitionFactor.Value;
            //Plugin.Log.LogInfo($"FADE IN HALF: {time}s");
        }
    }

    [HarmonyPatch(typeof(UIManager), nameof(UIManager.FadeOut))]
    [HarmonyPrefix]
    static void FadeOut(ref float time, Il2CppSystem.Action onEnd)
    {
        if (ModComponent.Instance.transition != TransitionState.None)
        {
            time /= Plugin.Config.ZoneTransitionFactor.Value;
            //Plugin.Log.LogInfo($"FADE OUT: {time}s");
        }
    }

    [HarmonyPatch(typeof(UIManager), nameof(UIManager.FadeWhiteOut))]
    [HarmonyPrefix]
    static void FadeWhiteOut(ref float time, Il2CppSystem.Action onEnd)
    {
        if (ModComponent.Instance.transition != TransitionState.None)
        {
            time /= Plugin.Config.ZoneTransitionFactor.Value;
            //Plugin.Log.LogInfo($"FADEWHITE OUT: {time}s");
        }
    }

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
