using HarmonyLib;

namespace Suikoden_Fix.Patches;

public class SpeedHackPatch
{
    [HarmonyPatch(typeof(SystemObject), nameof(SystemObject.IsFrameSkipEnable), MethodType.Getter)]
    [HarmonyPrefix]
    static bool IsFrameSkipEnable(ref bool __result)
    {
        __result = ModComponent.Instance.FrameSkip;

        return false;
    }
}
