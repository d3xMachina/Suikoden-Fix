extern alias GSD1;
extern alias GSD2;

using HarmonyLib;

namespace Suikoden_Fix.Patches;

public class ClassicModePatch
{
    [HarmonyPatch(typeof(GSD1.GlobalWork), nameof(GSD1.GlobalWork.ChangeHDMode))]
    [HarmonyPatch(typeof(GSD2.GAME_WORK), nameof(GSD2.GAME_WORK.ChangeHDMode))]
    [HarmonyPrefix]
    static void ChangeHDMode(ref bool isHD)
    {
        isHD = false;
    }

    [HarmonyPatch(typeof(ShareSaveData), nameof(ShareSaveData.SetHDMode))]
    [HarmonyPrefix]
    static void AlwaysSaveHDMode(SystemObject.Mode mode, ref bool flag)
    {
        flag = true;
    }
}
