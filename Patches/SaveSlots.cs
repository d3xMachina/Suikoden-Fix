using HarmonyLib;

namespace Suikoden_Fix.Patches;

public class SaveSlotsPatch
{
    private static bool _isInCoInit = false;

    private static void ExecuteCoroutineSynchronously(Il2CppSystem.Collections.IEnumerator coroutine)
    {
        if (coroutine == null)
        {
            return;
        }

        System.Collections.Stack stack = new();
        stack.Push(coroutine);

        while (stack.Count > 0)
        {
            var current = (Il2CppSystem.Collections.IEnumerator)stack.Pop();

            while (current.MoveNext())
            {
                var nested = current.Current?.TryCast<Il2CppSystem.Collections.IEnumerator>();
                if (nested == null)
                {
                    continue;
                }

                stack.Push(current);
                stack.Push(nested);
                break;
            }
        }
    }

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

    [HarmonyPatch(typeof(UISaveLoadBase._CoInit_d__76), nameof(UISaveLoadBase._CoInit_d__76.MoveNext))]
    [HarmonyPrefix]
    static void CoInitPre()
    {
        _isInCoInit = true;
    }

    [HarmonyPatch(typeof(UISaveLoadBase._CoInit_d__76), nameof(UISaveLoadBase._CoInit_d__76.MoveNext))]
    [HarmonyPostfix]
    static void CoInitPost()
    {
        _isInCoInit = false;
    }

    [HarmonyPatch(typeof(CoroutineList), nameof(CoroutineList.WaitForCoroutine))]
    [HarmonyPrefix]
    static void FasterSaveSlotsProcessing(CoroutineList __instance)
    {
        if (!_isInCoInit || __instance?.enumerators == null)
        {
            return;
        }

        foreach (var coroutine in __instance.enumerators)
        {
            ExecuteCoroutineSynchronously(coroutine);
        }

        __instance.enumerators.Clear();
    }
}
