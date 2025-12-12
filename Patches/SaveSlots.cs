using HarmonyLib;

namespace Suikoden_Fix.Patches;

public class SaveSlotsPatch
{
    [HarmonyPatch(typeof(UISaveLoadBase), nameof(UISaveLoadBase.Exec))]
    [HarmonyPrefix]
    static bool ScrollFast(ref int __result, UISaveLoadBase __instance)
    {
        const int ScrollCount = 5;

        if (!SystemObject.IsUpdateFrame ||
            __instance.isScrolling ||
            __instance.mode != 0 ||
            __instance.funcType == UISaveLoadBase.FuncType.ClearSave)
        {
            return true;
        }

        var scrollUp = GRInputManager.IsRepeat(GRInputManager.Type.Left);
        var scrollDown = GRInputManager.IsRepeat(GRInputManager.Type.Right);

        if ((scrollUp || scrollDown) && scrollUp != scrollDown)
        {
            var clipName = __instance.GetSeClipName("SD_SYS_CURSOR1");
            SoundManager.PlaySE(clipName);

            for (int i = 0; i < ScrollCount; ++i)
            {
                if (scrollUp)
                {
                    __instance.ScrollUp();
                }
                else
                {
                    __instance.ScrollDown();
                }
            }

            __result = 0;
            return false;
        }

        return true;
    }
}
