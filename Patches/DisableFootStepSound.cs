using HarmonyLib;
using static FootSoundPlayer;

namespace Suidoken_Fix.Patches;

public class DisableFootStepSoundPatch
{
    [HarmonyPatch(typeof(FootSoundPlayer), nameof(FootSoundPlayer.PlayFootStepSE))]
    [HarmonyPrefix]
    static bool PlayFootStepSE(FootStepType t)
    {
        return false;
    }
}
