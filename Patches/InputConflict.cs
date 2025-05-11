extern alias GSD1;
extern alias GSD2;

using HarmonyLib;

namespace Suikoden_Fix.Patches;

public class InputConflictPatch
{
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.Overlay_h_dance), nameof(GSD2.EventOverlayClass.Overlay_h_dance.DanceMain))]
    [HarmonyPostfix]
    static void GSD2_DanceMinigame(int __result)
    {
        ModComponent.Instance.IsInDanceMinigame = __result == 0;
    }
}
