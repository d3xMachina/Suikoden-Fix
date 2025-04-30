extern alias GSD1;
extern alias GSD2;

using HarmonyLib;

namespace Suikoden_Fix.Patches;

public class PauseGamePatch
{
    private static double _customUnscaledTime = 0;
    
    [HarmonyPatch(typeof(GSD1.ChapterManager), nameof(GSD1.ChapterManager.Update))]
    [HarmonyPatch(typeof(GSD2.GRChapterManager), nameof(GSD2.GRChapterManager.Update))]
    [HarmonyPrefix]
    static bool ChapterManagerUpdate()
    {
        if (ModComponent.Instance.GamePaused)
        {
            SystemObject._isUpdateFrame = false;
            return false;
        }

        return true;
    }

    // Needed if you have the FrameRate patch
    [HarmonyPatch(typeof(SystemObject), nameof(SystemObject.IsUpdateFrame), MethodType.Getter)]
    [HarmonyPostfix]
    static void IsUpdateFrame(ref bool __result)
    {
        if (ModComponent.Instance.GamePaused)
        {
            __result = false;
        }
    }

    [HarmonyPatch(typeof(SystemObject), nameof(SystemObject.UpdateShaderCustomUnscaledTime))]
    [HarmonyPrefix]
    static bool UpdateShaderCustomUnscaledTime()
    {
        if (!ModComponent.Instance.GamePaused)
        {
            // Make the animation of shaders match when unpausing the game by using a delta time instead of a time since the game was started
            _customUnscaledTime += UnityEngine.Time.unscaledDeltaTime;
            UnityEngine.Shader.SetGlobalFloat("_CustomUnscaledTime", (float)(_customUnscaledTime % 3600.0));
        }

        return false;
    }

    // Disable because of draw (it checks frameskipping)
    [HarmonyPatch(typeof(UITips), nameof(UITips.Update))]
    // Disable because of input, the alternative would be to disable partially GRInputManager updates
    [HarmonyPatch(typeof(Share.UI.Window.UIMessageWindow), nameof(Share.UI.Window.UIMessageWindow.Update))]
    [HarmonyPatch(typeof(Share.UI.UISystemDialog), nameof(Share.UI.UISystemDialog.Update))]
    [HarmonyPatch(typeof(UILicense), nameof(UILicense.Update))]
    [HarmonyPatch(typeof(UIManual), nameof(UIManual.Update))]
    [HarmonyPrefix]
    static bool UpdateUI()
    {
        return !ModComponent.Instance.GamePaused;
    }
}
