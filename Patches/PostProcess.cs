extern alias GSD1;
extern alias GSD2;

using HarmonyLib;

namespace Suikoden_Fix.Patches;

public class PostProcessPatch
{
    [HarmonyPatch(typeof(UnityEngine.Rendering.PostProcessing.PostProcessEffectSettings), nameof(UnityEngine.Rendering.PostProcessing.PostProcessEffectSettings.OnEnable))]
    [HarmonyPrefix]
    static void SetPostProcess(UnityEngine.Rendering.PostProcessing.PostProcessEffectSettings __instance)
    {
        if (Plugin.Config.BloomMultiplier.Value == 1f ||
            ModComponent.Instance.ActiveGame == ModComponent.Game.None)
        {
            return;
        }

        if (__instance.name != "Bloom") // in-game bloom on the field is called "Bloom"
        {
            return;
        }

        var bloom = __instance.TryCast<UnityEngine.Rendering.PostProcessing.Bloom>();
        if (bloom == null)
        {
            return;
        }

        var intensityParameter = bloom.intensity;
        if (intensityParameter == null)
        {
            return;
        }

        var intensity = intensityParameter.GetValue<float>();
        intensityParameter.value = intensity * Plugin.Config.BloomMultiplier.Value;
        __instance.name += "Changed"; // make sure we multiply it only once
    }

    [HarmonyPatch(typeof(GSD1.PostProcessEffectManager), nameof(GSD1.PostProcessEffectManager.Start))]
    [HarmonyPostfix]
    static void GSD1_SetBattlePostProcess(GSD1.PostProcessEffectManager __instance)
    {
        if (Plugin.Config.BloomMultiplier.Value != 1f)
        {
            var intensityParameter = __instance.m_bloom?.intensity;
            if (intensityParameter != null)
            {
                var intensity = intensityParameter.GetValue<float>();
                intensityParameter.value = intensity * Plugin.Config.BloomMultiplier.Value;
            }
        }

        if (Plugin.Config.DisableDepthOfField.Value)
        {
            GSD1.PostProcessEffectManager.enableDof = false;
        }
    }

    [HarmonyPatch(typeof(GSD2.PostProcessEffectManager), nameof(GSD2.PostProcessEffectManager.Start))]
    [HarmonyPostfix]
    static void GSD2_SetBattlePostProcess(GSD2.PostProcessEffectManager __instance)
    {
        if (Plugin.Config.BloomMultiplier.Value != 1f)
        {
            var intensityParameter = __instance.m_bloom?.intensity;
            if (intensityParameter != null)
            {
                var intensity = intensityParameter.GetValue<float>();
                intensityParameter.value = intensity * Plugin.Config.BloomMultiplier.Value;
            }
        }
        
        if (Plugin.Config.DisableDepthOfField.Value)
        {
            GSD2.PostProcessEffectManager.isDepthOfFieldActive = false;
        }
    }
}
