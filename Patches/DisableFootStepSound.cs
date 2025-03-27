using HarmonyLib;

namespace Suikoden_Fix.Patches;

public class DisableFootStepSoundPatch
{
    [HarmonyPatch(typeof(FootSoundPlayer), nameof(FootSoundPlayer.PlayFootStepSE))]
    [HarmonyPrefix]
    static bool PlayFootStepSE(FootSoundPlayer.FootStepType t)
    {
        return false;
    }
}
