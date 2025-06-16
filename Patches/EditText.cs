extern alias GSD1;
extern alias GSD2;

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Suikoden_Fix.Patches;

public class EditTextPatch
{
    private const int JsonBufferSize = 4;

    private static readonly Dictionary<string, Dictionary<int, string>> _texts = new();
    private static TextMasterData.Lang _language = TextMasterData.Lang.Max;

    private static void LoadTexts()
    {
        const string FilePath = "GameTexts.json";

        _texts.Clear();

        if (!File.Exists(FilePath))
        {
            Plugin.Log.LogWarning($"{FilePath} not found in the game folder. No texts loaded.");
            return;
        }

        try
        {
            Plugin.Log.LogInfo("Loading texts...");

            using var stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read, JsonBufferSize, FileOptions.SequentialScan);
            SkipBOM(stream);

            bool loaded = ParseJsonStream(stream);
            if (loaded)
            {
                Plugin.Log.LogInfo("Texts loaded.");
            }
            else
            {
                Plugin.Log.LogInfo("No texts.");
            }
        }
        catch (JsonException ex)
        {
            Plugin.Log.LogError($"Texts json parsing error: {ex.Message}.");
        }
    }

    private static void SkipBOM(Stream stream)
    {
        ReadOnlySpan<byte> utf8Bom = [0xEF, 0xBB, 0xBF];
        var buffer = new byte[utf8Bom.Length];
        var readBytes = stream.Read(buffer, 0, utf8Bom.Length);
        var readSpan = new ReadOnlySpan<byte>(buffer);

        if (!readSpan.StartsWith(utf8Bom))
        {
            // No BOM, seek back
            stream.Seek(0, SeekOrigin.Begin);
        }
    }

    // Load only the text for the current language
    private static bool ParseJsonStream(Stream stream)
    {
        var buffer = new byte[JsonBufferSize];
        var currentLanguage = TextMasterData.Lang.Max;
        string currentId = null;
        int currentIndex = 0;
        Dictionary<int, string> currentIdDict = null;
        int depth = 0;
        bool inLanguage = false;
        bool inId = false;
        bool success = false;

        // Read first chunk
        var bytesRead = stream.Read(buffer);
        if (bytesRead == 0)
        {
            return false;
        }

        var reader = new Utf8JsonReader(buffer, isFinalBlock: false, state: default);

        while (true)
        {
            while (bytesRead > 0 && !reader.Read())
            {
                bytesRead = ReadMoreFromStream(stream, ref buffer, ref reader);
            }

            if (bytesRead == 0)
            {
                break;
            }

            switch (reader.TokenType)
            {
                case JsonTokenType.StartObject:
                    ++depth;
                    break;

                case JsonTokenType.EndObject:
                    --depth;
                    if (depth == 0)
                    {
                        success = true;
                    }
                    else if (depth == 1)
                    {
                        inLanguage = false;
                    }
                    else if (depth == 2)
                    {
                        inId = false;
                        currentIdDict = null;
                    }
                    break;

                case JsonTokenType.PropertyName when depth == 1:
                    // Top-level property is language
                    if (Enum.TryParse(reader.GetString(), ignoreCase: true, out TextMasterData.Lang lang) &&
                        lang != TextMasterData.Lang.Max)
                    {
                        currentLanguage = lang;
                        inLanguage = true;
                    }
                    break;

                case JsonTokenType.PropertyName when depth == 2 && inLanguage:
                    // Property inside language is ID
                    currentId = reader.GetString();
                    inId = true;

                    if (currentLanguage == _language &&
                        !_texts.TryGetValue(currentId, out currentIdDict))
                    {
                        currentIdDict = new();
                        _texts[currentId] = currentIdDict;
                    }
                    break;

                case JsonTokenType.PropertyName when depth == 3 && inId:
                    // Property inside ID is index
                    if (int.TryParse(reader.GetString(), out int index))
                    {
                        currentIndex = index;
                    }
                    break;

                case JsonTokenType.String when depth == 3 && inId:
                    // Value is the message
                    if (currentIdDict != null)
                    {
                        currentIdDict[currentIndex] = reader.GetString();
                    }
                    break;

                default:
                    break;
            }

            if (success)
            {
                break;
            }
        }

        return success;
    }

    private static int ReadMoreFromStream(Stream stream, ref byte[] buffer, ref Utf8JsonReader reader)
    {
        int bytesRead;
        if (reader.BytesConsumed < buffer.Length)
        {
            ReadOnlySpan<byte> leftover = buffer.AsSpan((int)reader.BytesConsumed);
            if (leftover.Length == buffer.Length)
            {
                Array.Resize(ref buffer, buffer.Length * 2);
                //Plugin.Log.LogWarning($"Buffer size={buffer.Length}");
            }

            leftover.CopyTo(buffer);
            bytesRead = stream.Read(buffer.AsSpan(leftover.Length));
        }
        else
        {
            bytesRead = stream.Read(buffer);
        }

        //Plugin.Log.LogWarning($"Buffer={System.Text.Encoding.UTF8.GetString(buffer)}");
        reader = new Utf8JsonReader(buffer, isFinalBlock: bytesRead == 0, reader.CurrentState);

        return bytesRead;
    }

    [HarmonyPatch(typeof(TextMasterData), nameof(TextMasterData.GetSystemText))]
    [HarmonyPrefix]
    static bool GetSystemText(string id, int index, ref string __result)
    {
        if (_language == TextMasterData.CurrentLanguage &&
            _texts.TryGetValue(id, out var idDict) && // Check if the ID exists in the language dictionary
            idDict.TryGetValue(index, out var text)) // Check if the index exists in the ID dictionary
        {
            __result = text;
            return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(ShareSaveData), nameof(ShareSaveData.LoadDataCore))]
    [HarmonyPatch(typeof(GSDTitleSelect._LangChange_d__59), nameof(GSDTitleSelect._LangChange_d__59.MoveNext))]
    [HarmonyPostfix]
    static void CheckLanguage()
    {
        var systemConfig = ShareSaveData.system_config;
        if (systemConfig == null)
        {
            return;
        }

        var languageByte = systemConfig.langauge;
        if (!Enum.IsDefined(typeof(TextMasterData.Lang), (int)languageByte))
        {
            return;
        }

        var language = (TextMasterData.Lang)languageByte;
        if (language == TextMasterData.Lang.Max ||
            language == _language)
        {
            return;
        }

        _language = language;

        Plugin.Log.LogInfo($"Language changed: {language}.");
        LoadTexts();
    }
}

public class LogTextPatch
{
    private const string FilePath = "GameTextsLog.txt";

    public static void RemoveLogs()
    {
        try
        {
            File.Delete(FilePath);
        }
        catch (Exception ex)
        {
            Plugin.Log.LogWarning($"Cannot delete texts logs : {ex}.");
        }
    }

    static void LogText(string type, string text)
    {
        var line = $"{type} {text}";

        try
        {
            File.AppendAllText(FilePath, $"{line}\n", System.Text.Encoding.UTF8);
        }
        catch (Exception ex)
        {
            Plugin.Log.LogWarning($"Cannot append texts logs : {ex}.");
        }

        Plugin.Log.LogInfo(line);
    }

    [HarmonyPatch(typeof(TextMasterData), nameof(TextMasterData.GetSystemText))]
    [HarmonyPostfix]
    static void GetSystemTextPost(string id, int index, ref string __result)
    {
        LogText($"[{id}:{index}]", __result);
    }

    [HarmonyPatch(typeof(TextMasterData), nameof(TextMasterData.GetSystemTextEx))]
    [HarmonyPostfix]
    static void GetSystemTextEx(string id, int index, int gsd, string __result)
    {
        LogText($"[{id}:{index}:{gsd}]", __result);
    }
}