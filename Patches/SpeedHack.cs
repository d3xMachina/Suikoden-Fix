extern alias GSD1;
extern alias GSD2;

using HarmonyLib;
using UnityEngine.InputSystem;

namespace Suikoden_Fix.Patches;

public class SpeedHackPatch
{
    private static bool _isInChapterManagerUpdate = false;
    private static int _chapterUpdateCount = 0;
    private static bool _isInUpdateTimer = false;
    private static float _lastRealtimeSinceStartup = 0f;
    private static float _faketimeSinceStartup = 0f;

    [HarmonyPatch(typeof(GRInputManager), nameof(GRInputManager.Create))]
    [HarmonyPostfix]
    static void RemoveBinding(GRInputManager __instance, GRInputManager.Mode _mode)
    {
        if (__instance.currentMap == null || __instance.currentMap.Count == 0)
        {
            return;
        }

        foreach (InputActionMap actionMap in __instance.currentMap)
        {
            if (actionMap == null)
            {
                continue;
            }

            foreach (InputAction action in actionMap.actions)
            {
                for (int i = action.bindings.Count - 1; i >= 0; i--)
                {
                    InputBinding binding = action.bindings[i];

                    if (binding.path.Contains("/rightTrigger"))
                    {
                        action.ChangeBinding(i).Erase();
                        Plugin.Log.LogInfo($"Removed binding {binding.path} in action '{action.name}' in map '{actionMap.name}'");
                    }
                    else
                    {
                        //Plugin.Log.LogInfo($"Binding {binding.path} in action '{action.name}' in map '{actionMap.name}'");
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(GSD1.ChapterManager), nameof(GSD1.ChapterManager.Update))]
    [HarmonyPrefix]
    static void GSD1_ChapterManagerUpdate()
    {
        _isInChapterManagerUpdate = true;
        _chapterUpdateCount = 0;
    }

    [HarmonyPatch(typeof(GSD1.ChapterManager), nameof(GSD1.ChapterManager.Update))]
    [HarmonyPostfix]
    static void GSD1_ChapterManagerUpdatePost()
    {
        _isInChapterManagerUpdate = false;
    }

    [HarmonyPatch(typeof(Framework.Chapter), nameof(Framework.Chapter.Update))]
    [HarmonyPrefix]
    static void ChapterUpdate()
    {
        if (_isInChapterManagerUpdate)
        {
            if (_chapterUpdateCount > 0)
            {
                // Disable inputs so only the first update processes inputs
                GSD1.Pad.PadUpdate(false, false);
            }

            ++_chapterUpdateCount;
        }
    }

    // This returns the elapsed time since the last call to this function
    [HarmonyPatch(typeof(GSD1.GameInit), nameof(GSD1.GameInit.main_initialize))]
    [HarmonyPatch(typeof(GSD1.GameInit), nameof(GSD1.GameInit.update_game_timer))]
    [HarmonyPatch(typeof(GSD2.GRChapterManager), nameof(GSD2.GRChapterManager.G2_Count_Up))]
    [HarmonyPrefix]
    static void UpdateGameTimer()
    {
        _isInUpdateTimer = true;
    }

    [HarmonyPatch(typeof(GSD1.GameInit), nameof(GSD1.GameInit.main_initialize))]
    [HarmonyPatch(typeof(GSD1.GameInit), nameof(GSD1.GameInit.update_game_timer))]
    [HarmonyPatch(typeof(GSD2.GRChapterManager), nameof(GSD2.GRChapterManager.G2_Count_Up))]
    [HarmonyPostfix]
    static void UpdateGameTimerPost()
    {
        _isInUpdateTimer = false;
    }

    [HarmonyPatch(typeof(UnityEngine.Time), nameof(UnityEngine.Time.realtimeSinceStartup), MethodType.Getter)]
    [HarmonyPostfix]
    static void GetTimeSinceStartup(ref float __result)
    {
        var elapsed = __result - _lastRealtimeSinceStartup;
        _faketimeSinceStartup += elapsed * ModComponent.Instance.GameTimerMultiplier;
        _lastRealtimeSinceStartup = __result;

        // Unity methods are only accessed from 1 thread so no need to check the thread id with the pinvoke method GetCurrentWin32ThreadId()
        if (_isInUpdateTimer)
        {
            //Plugin.Log.LogWarning($"Real Time={__result} Fake Time={_faketimeSinceStartup}");
            __result = _faketimeSinceStartup;
        }
    }
}
