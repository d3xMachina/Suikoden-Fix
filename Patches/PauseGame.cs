extern alias GSD1;
extern alias GSD2;

using HarmonyLib;

namespace Suikoden_Fix.Patches;

public class PauseGamePatch
{
    private static double _customUnscaledTime = 0;
    private static bool _disableSkipButton = false;
    private static bool _disableChapterUpdate = false;

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

    // Gallery events Suikoden 2
    [HarmonyPatch(typeof(GSD2.MACHICON), nameof(GSD2.MACHICON.MachiMain))]

    // Gallery endings
    [HarmonyPatch(typeof(GSD1.End_vil_c), nameof(GSD1.End_vil_c.ending_main))]
    [HarmonyPatch(typeof(GSD2.ending_c), nameof(GSD2.ending_c.EndingMain))]

    // Gallery credits
    [HarmonyPatch(typeof(GSD1.Stf_vil_c), nameof(GSD1.Stf_vil_c.staff_credits_exit))]
    [HarmonyPatch(typeof(GSD1.Stf_vil_c), nameof(GSD1.Stf_vil_c.staff_roll))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.Overlay_staff), nameof(GSD2.EventOverlayClass.Overlay_staff.StaffMain00))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.Overlay_staff), nameof(GSD2.EventOverlayClass.Overlay_staff.StaffMain01))]

    // Gallery movies and movies in gallery events
    [HarmonyPatch(typeof(GSDTitleSelect), nameof(GSDTitleSelect.Main))]

    [HarmonyPrefix]
    static void DisableSkipButton()
    {
        if (!ModComponent.Instance.IsInDanceMinigame)
        {
            _disableSkipButton = true;
        }
    }

    // Gallery events Suikoden 1
    [HarmonyPatch(typeof(GSD1.ChapterManager), nameof(GSD1.ChapterManager.Update))]
    [HarmonyPrefix]
    static void GSD1_DisableSkipButtonInGalleryEvents()
    {
        if (Omake.IsMemoryGalleryMode)
        {
            _disableSkipButton = true;
        }
    }

    [HarmonyPatch(typeof(GSD1.ChapterManager), nameof(GSD1.ChapterManager.Update))]
    [HarmonyPatch(typeof(GSD2.MACHICON), nameof(GSD2.MACHICON.MachiMain))]
    [HarmonyPatch(typeof(GSD1.End_vil_c), nameof(GSD1.End_vil_c.ending_main))]
    [HarmonyPatch(typeof(GSD2.ending_c), nameof(GSD2.ending_c.EndingMain))]
    [HarmonyPatch(typeof(GSD1.Stf_vil_c), nameof(GSD1.Stf_vil_c.staff_credits_exit))]
    [HarmonyPatch(typeof(GSD1.Stf_vil_c), nameof(GSD1.Stf_vil_c.staff_roll))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.Overlay_staff), nameof(GSD2.EventOverlayClass.Overlay_staff.StaffMain00))]
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.Overlay_staff), nameof(GSD2.EventOverlayClass.Overlay_staff.StaffMain01))]
    [HarmonyPostfix]
    static void RestoreSkipButton()
    {
        _disableSkipButton = false;
    }

    [HarmonyPatch(typeof(GSDTitleSelect), nameof(GSDTitleSelect.Main))]
    [HarmonyPostfix]
    static void RestoreSkipButtonInTitle(GSDTitleSelect __instance)
    {
        _disableSkipButton = false;
        ModComponent.Instance.IsInMovieGallery = __instance.step == (int)GSDTitleSelect.State.GalleryMovie;
    }

    // In game movies
    [HarmonyPatch(typeof(GSD2.EventOverlayClass.Overlay_EventMovieInit), nameof(GSD2.EventOverlayClass.Overlay_EventMovieInit.OverlayEventMovieEnd))]
    [HarmonyPatch(typeof(GSD2.Magic.Shi4), nameof(GSD2.Magic.Shi4.magic_func05))] // ??
    [HarmonyPatch(typeof(GSD2.Magic.Swo4), nameof(GSD2.Magic.Swo4.magic_func03))] // ??
    [HarmonyPrefix]
    static void SkipInGameMovie()
    {
        if (ModComponent.Instance.SkipScene)
        {
            SoundManager.IsCri()?.Stop();
        }
    }

    [HarmonyPatch(typeof(UITitleMovieInsert), nameof(UITitleMovieInsert.IsOpentedInsert))]
    [HarmonyPrefix]
    static bool IsOpentedInsert(ref bool __result)
    {
        if (_disableSkipButton)
        {
            __result = true;
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(UITitleMovieInsert), nameof(UITitleMovieInsert.UpdateInsertText))]
    [HarmonyPrefix]
    static bool UpdateInsertText()
    {
        return !_disableSkipButton;
    }

    [HarmonyPatch(typeof(GRInputManager), nameof(GRInputManager.IsOn))]
    [HarmonyPrefix]
    static bool GRInputManagerIsOn(GRInputManager.Type t, ref bool __result)
    {
        if (_disableSkipButton && t == GRInputManager.Type.MovieSkip)
        {
            __result = ModComponent.Instance.SkipScene;
            return false;
        }

        return true;
    }

    // Fix a game bug when exiting the gallery event in Suikoden 1.
    // The game will attempt to read a null pointer sometimes when exiting a gallery event.
    // This is because the ressources are unloaded and there is a fade time before switching to the new chapter.
    // So the chapter will keep being updated for a little bit with unloaded ressources...
    // To fix this, we prevent the chapter update during this fade time.
    [HarmonyPatch(typeof(Framework.Chapter), nameof(Framework.Chapter.Update))]
    [HarmonyPrefix]
    static bool FixCrashOnGalleryEventExit()
    {
        return !_disableChapterUpdate;
    }

    [HarmonyPatch(typeof(GSD1.ChapterManager), nameof(GSD1.ChapterManager.ExitMemory))]
    [HarmonyPrefix]
    static void GSD1_ExitMemory()
    {
        _disableChapterUpdate = true;
    }

    [HarmonyPatch(typeof(Framework.Chapter), nameof(Framework.Chapter.Request), [typeof(Il2CppSystem.Type)])]
    [HarmonyPrefix]
    static void ChapterChanged()
    {
        _disableChapterUpdate = false;
    }
}
