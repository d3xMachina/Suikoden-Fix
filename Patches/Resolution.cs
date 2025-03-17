using HarmonyLib;
using Share.UI.Panel;
using ShareUI.Menu;
using UnityEngine;
using UnityEngine.UI;

namespace Suikoden_Fix.Patches;

public class ResolutionPatch
{
    private static float _defaultAspectRatio = 16f / 9f;
    private static float _aspectRatio = _defaultAspectRatio;
    private static int _prevTitleStep = (int)GSDTitleSelect.State.NONE;

    static void OnSetResolution(ref int width, ref int height)
    {
        var cWidth = Plugin.Config.Width.Value;
        var cHeight = Plugin.Config.Height.Value;

        if (cWidth > 0 && cHeight > 0)
        {
            width = cWidth;
            height = cHeight;

            Plugin.Log.LogInfo($"Resolution: {width}x{height}");
        }

        _aspectRatio = width / (float)height;
    }

    static void OnSetFullscreenMode(ref FullScreenMode fullscreenMode)
    {
        var cFullScreen = Plugin.Config.Fullscreen.Value;
        if (cFullScreen != -1)
        {
            fullscreenMode = cFullScreen > 0 ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        }
    }

    static void OnSetFullscreenMode(ref bool fullscreen)
    {
        var cFullscreen = Plugin.Config.Fullscreen.Value;
        if (cFullscreen != -1)
        {
            fullscreen = cFullscreen > 0;
        }
    }

    [HarmonyPatch(typeof(Screen), nameof(Screen.SetResolution), [typeof(int), typeof(int), typeof(FullScreenMode)])]
    [HarmonyPrefix]
    static void SetResolution(ref int width, ref int height, ref FullScreenMode fullscreenMode)
    {
        OnSetResolution(ref width, ref height);
        OnSetFullscreenMode(ref fullscreenMode);
    }

    [HarmonyPatch(typeof(Screen), nameof(Screen.SetResolution), [typeof(int), typeof(int), typeof(bool)])]
    [HarmonyPrefix]
    static void SetResolution2(ref int width, ref int height, ref bool fullscreen)
    {
        OnSetResolution(ref width, ref height);
        OnSetFullscreenMode(ref fullscreen); 
    }

    // Prevent the config UI from reverting the display mode
    static void AddResolutionToConfigUI()
    {
        if (Plugin.Config.Width.Value <= 0 || Plugin.Config.Height.Value <= 0)
        {
            return;
        }

        var screenSize = UIConfigWindow.c_ScreenSize;
        if (screenSize == null || screenSize.Count <= 0 || screenSize[0].Count < 2)
        {
            Plugin.Log.LogWarning("No resolution found!");
            return;
        }

        screenSize[0][0] = Plugin.Config.Width.Value;
        screenSize[0][1] = Plugin.Config.Height.Value;
    }

    // The resolution has to be set after this initialization or the scaling fix won't work
    [HarmonyPatch(typeof(Initialize), nameof(Initialize.Start))]
    [HarmonyPostfix]
    static void SetInitialDisplay()
    {
        // Apply the resolution with the hook
        Screen.SetResolution(Screen.width, Screen.height, Screen.fullScreenMode);   
        AddResolutionToConfigUI();
    }

    [HarmonyPatch(typeof(CanvasScaler), nameof(CanvasScaler.OnEnable))]
    [HarmonyPostfix]
    static void FixUIScaling(CanvasScaler __instance)
    {
        if (_aspectRatio != _defaultAspectRatio)
        {
            __instance.m_ScreenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        }
    }

    static void ScaleObject(GameObject obj, string path = "")
    {
        if (_aspectRatio == _defaultAspectRatio || obj == null)
        {
            return;
        }

        float scaleX;
        float scaleY;

        if (_aspectRatio > _defaultAspectRatio)
        {
            scaleX = _aspectRatio / _defaultAspectRatio;
            scaleY = 1f;
        }
        else
        {
            scaleX = 1f;
            scaleY = _defaultAspectRatio / _aspectRatio;
        }

        var transform = obj.transform;
        if (path != "")
        {
            transform = transform?.Find(path);
        }

        transform?.SetLocalScale(scaleX, scaleY, 1f);
    }

    [HarmonyPatch(typeof(TitlePanel), nameof(TitlePanel.Awake))]
    [HarmonyPostfix]
    static void ExpandGameTitleBackground(TitlePanel __instance)
    {
        var background = __instance.backGroundObject;

        ScaleObject(background, "gsd1/gs1");
        ScaleObject(background, "gsd2/gs2");
    }

    [HarmonyPatch(typeof(UILicense), nameof(UILicense.Awake))]
    [HarmonyPostfix]
    static void ExpandGameLicenseBackground(UILicense __instance)
    {
        ScaleObject(__instance.gameObject, "Img_bg");
    }

    [HarmonyPatch(typeof(UITips), nameof(UITips.OpenTips))]
    [HarmonyPostfix]
    static void ExpandTipsBackground(UITips __instance)
    {
        if (_aspectRatio < _defaultAspectRatio)
        {
            return;
        }

        ScaleObject(__instance.gameObject, "Img_Bg/Img_Bg");
    }

    [HarmonyPatch(typeof(UIMainMenuTips), nameof(UIMainMenuTips.OpenTips))]
    [HarmonyPostfix]
    static void ExpandTipsBackground(UIMainMenuTips __instance)
    {
        if (_aspectRatio < _defaultAspectRatio)
        {
            return;
        }

        ScaleObject(__instance.gameObject, "Img_Bg/Img_Bg");
    }

    [HarmonyPatch(typeof(GSDTitleSelect), nameof(GSDTitleSelect.Main))]
    [HarmonyPrefix]
    static void ExpandMainTitleBackground(GSDTitleSelect __instance)
    {
        var step = __instance.step;
        if (step == _prevTitleStep)
        {
            return;
        }

        if (step == (int)GSDTitleSelect.State.WaitSpriteLoad)
        {
            ScaleObject(__instance.BGObject);
            ScaleObject(__instance.gameObject, "UI_Canvas_Root/UI_Com_Footer(Clone)/Img_Bg/Img_Bg");
        }

        _prevTitleStep = step;
    }
}
