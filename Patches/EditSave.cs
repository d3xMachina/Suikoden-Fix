using HarmonyLib;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Suikoden_Fix.Patches;

public class EditSavePatch
{
    static string FormatJson(string json, bool prettify)
    {
        try
        {
            var jsonObject = JsonNode.Parse(json);

            var options = new JsonSerializerOptions
            {
                WriteIndented = prettify
            };

            return jsonObject.ToJsonString(options);
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Failed to format json: {ex.Message}.");
        }

        return json;
    }

    static string GetDecryptedSaveName(string path)
    {
        var fileName = Path.GetFileName(path);

        if (fileName.StartsWith("_sharetmpsave"))
        {
            return "";
        }

        var gameName = Directory.GetParent(path).Name;
        return $"_decrypted_{gameName}_{fileName}.json";
    }

    [HarmonyPatch(typeof(SystemSave), nameof(SystemSave.Save))]
    [HarmonyPostfix]
    static void Save(string path, string json)
    {
        var fileName = "";

        try
        {
            fileName = GetDecryptedSaveName(path);
            
            if (fileName != "")
            {
                json = FormatJson(json, true);
                File.WriteAllText(fileName, json, System.Text.Encoding.UTF8);

                Plugin.Log.LogInfo($"Saved decrypted save \"{fileName}\".");
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Cannot save decrypted save \"{fileName}\": {ex.Message}.");
        }
    }

    [HarmonyPatch(typeof(SystemSave), nameof(SystemSave.Load))]
    [HarmonyPrefix]
    static bool Load(string path, Il2CppSystem.Action<string> cb)
    {
        bool saveLoaded = false;
        var fileName = "";

        try
        {
            fileName = GetDecryptedSaveName(path);

            if (fileName != "")
            {
                if (File.Exists(fileName))
                {
                    var json = File.ReadAllText(fileName, System.Text.Encoding.UTF8);
                    json = FormatJson(json, false);
                    var saveData = SystemSave.HEADER + Encrypter.Encrypt(json, SystemSave.ENCRYPT_PASSWORD);

                    var display = new SystemSave.__c__DisplayClass16_0
                    {
                        cb = cb
                    };

                    var end = new Action<string>(display._Load_b__0);
                    end?.Invoke(saveData);

                    saveLoaded = true;

                    Plugin.Log.LogInfo($"Loaded decrypted save \"{fileName}\".");
                }
                else
                {
                    Plugin.Log.LogInfo($"No decrypted save found named \"{fileName}\".");
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Cannot load decrypted save named \"{fileName}\": {ex.Message}. Fallback to steam save.");
        }

        return !saveLoaded;
    }
}
