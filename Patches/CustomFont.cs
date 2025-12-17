using BepInEx;
using HarmonyLib;
using System;
using System.IO;
using TMPro;
using UnityEngine;

namespace Suikoden_Fix.Patches;

public class CustomFontPatch
{
    private class CustomFont
    {
        public TMP_FontAsset asset = null;
        public bool loadAttempt = false;
    }

    private static readonly CustomFont _customUIFont = new();
    private static readonly CustomFont _customMessageFont = new();
    private static bool _isInMessage = false;

    static TMP_FontAsset LoadFontAsset(string customFontName)
    {
        if (customFontName.IsNullOrWhiteSpace())
        {
            return null;
        }

        try
        {
            var customFontPath = Path.GetFullPath(customFontName);

            if (!File.Exists(customFontPath))
            {
                //Plugin.Log.LogWarning($"Custom font {customFontName} not found in {customFontPath}.");
                customFontPath = "";

                if (!customFontName.Contains(Path.DirectorySeparatorChar) &&
                    !customFontName.Contains(Path.AltDirectorySeparatorChar))
                {
                    foreach (var fontPath in Font.GetPathsToOSFonts())
                    {
                        //Plugin.Log.LogWarning($"Font path: {fontPath}");
                        var fontName = Path.GetFileName(fontPath);
                        if (fontName.Equals(customFontName, StringComparison.OrdinalIgnoreCase))
                        {
                            customFontPath = fontPath;
                            break;
                        }
                    }
                }
            }

            if (customFontPath == "")
            {
                Plugin.Log.LogWarning($"Custom font {customFontName} not found.");
                return null;
            }

            Plugin.Log.LogInfo($"Loading custom font from {customFontPath}.");

            var font = new Font(customFontPath);
            var fontAsset = TMP_FontAsset.CreateFontAsset(font);

            if (fontAsset == null)
            {
                Plugin.Log.LogWarning($"Failed to load the custom font.");
            }

            return fontAsset;
        }
        catch (Exception ex)
        {
            Plugin.Log.LogWarning($"Failed to load the custom font: {ex.Message}.");
            return null;
        }
    }

    [HarmonyPatch(typeof(UIFontBehabiour), nameof(UIFontBehabiour.UpdateTextFont))]
    [HarmonyPrefix]
    static void InMessagePre(UIFontBehabiour __instance)
    {
        var gameObject = __instance.gameObject;
        if (gameObject == null)
        {
            return;
        }

        if (gameObject.GetComponent<Share.UI.Window.UIMessageWindow>() != null || // dialogues
            gameObject.GetComponent<UISystemEDChara>() != null) // ending recap
        {
            _isInMessage = true;
        }

        //Plugin.Log.LogWarning($"UIFontBehabiour: name={gameObject.name}");
    }

    [HarmonyPatch(typeof(UIFontBehabiour), nameof(UIFontBehabiour.UpdateTextFont))]
    [HarmonyPostfix]
    static void InMessagePost()
    {
        _isInMessage = false;
    }

    [HarmonyPatch(typeof(SystemObject), nameof(SystemObject.GetLanguageFont), [typeof(TextMasterData.Lang)])]
    [HarmonyPrefix]
    static bool CreateCustomFont(out TMP_FontAsset __result, TextMasterData.Lang lang)
    {
        //Plugin.Log.LogWarning($"CreateCustomFont: isInMessage={_isInMessage}");
        var customFont = _isInMessage ? _customMessageFont : _customUIFont;

        // Try loading the font only once
        if (customFont.asset == null && !customFont.loadAttempt)
        {
            var customFontName = _isInMessage ? Plugin.Config.CustomFontMessage.Value : Plugin.Config.CustomFontUI.Value;
            customFont.asset = LoadFontAsset(customFontName);
            customFont.loadAttempt = true;

            if (customFont.asset == null)
            {
                customFont.asset = FontManager.CreateFont(lang);
            }
            else
            {
                FontManager.currentLang = lang;
            }

            if (customFont.asset != null)
            {
                SystemObject._loadedFontAssetEx.Add(customFont.asset); // Keep the asset loaded
                UIFontBehabiour.UpdateTextFontAll();
            }
        }

        __result = customFont.asset;

        return false;
    }
}
