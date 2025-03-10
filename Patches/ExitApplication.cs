using HarmonyLib;

namespace Suikoden_Fix.Patches;

public class ExitApplicationPatch
{
    [HarmonyPatch(typeof(GSDTitleSelect), nameof(GSDTitleSelect.Main))]
    [HarmonyPostfix]
    static void GetTitleStep(GSDTitleSelect __instance)
    {
        ModComponent.Instance.TitleSelectStep = (GSDTitleSelect.State)__instance.step;
    }
}
