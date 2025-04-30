extern alias GSD1;
extern alias GSD2;

using HarmonyLib;

namespace Suikoden_Fix.Patches;

public class GameTimerPatch
{
    private static bool _isInUpdateTimer = false;
    private static float _lastRealtimeSinceStartup = 0f;
    private static float _faketimeSinceStartup = 0f;

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
