using HarmonyLib;

namespace Suikoden_Fix.Patches;

public class NoHighPitchMusicPatch
{
    [HarmonyPatch(typeof(SoundManager), nameof(SoundManager.SetPitchType))]
    [HarmonyPrefix]
    static bool SetPitchType(SoundManager.PitchType pitch_type)
    {
        return false;
    }
}
