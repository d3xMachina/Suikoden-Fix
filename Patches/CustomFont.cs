using HarmonyLib;
using System;
using System.IO;
using TMPro;
using UnityEngine;

namespace Suikoden_Fix.Patches;

public class CustomFontPatch
{
    private static TMP_FontAsset _customFont = null;
    private static bool _attempt = false;

    [HarmonyPatch(typeof(FontManager), nameof(FontManager.CreateFont))]
    [HarmonyPrefix]
    static bool CreateCustomFont(ref TMP_FontAsset __result, TextMasterData.Lang lang)
    {
        try
        {
            if (_customFont == null)
            {
                if (_attempt)
                {
                    // Don't retry loading the font if it already failed
                    return true;
                }
                else
                {
                    _attempt = true;

                    var customFontName = Plugin.Config.CustomFont.Value;
                    var customFontPath = Path.GetFullPath(customFontName);

                    if (!File.Exists(customFontPath))
                    {
                        //Plugin.Log.LogWarning($"Custom font {customFontName} not found in {customFontPath}.");
                        customFontPath = "";

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

                    if (customFontPath == "")
                    {
                        Plugin.Log.LogWarning($"Custom font {customFontName} not found.");
                        return true;
                    }

                    Plugin.Log.LogInfo($"Loading custom font from {customFontPath}.");
                    var font = new Font(customFontPath);
                    _customFont = TMP_FontAsset.CreateFontAsset(font);

                    if (_customFont == null)
                    {
                        Plugin.Log.LogWarning($"Failed to load the custom font.");
                        return true;
                    }
                }
            }

            FontManager.currentLang = lang;
            __result = _customFont;
        }
        catch (Exception ex)
        {
            Plugin.Log.LogWarning($"Failed to load the custom font: {ex.Message}.");
            return true;
        }

        return false;
    }
}
