using HarmonyLib;
using LitJson;
using System;
using System.IO;

namespace Suikoden_Fix.Patches;

public class EditSavePatch
{
    static string FormatJson(string json, bool prettify)
    {
        try
        {
            JsonData jsonData = JsonMapper.ToObject(json);
            var jsonWriter = new JsonWriter
            {
                PrettyPrint = prettify,
                IndentValue = 2
            };

            JsonMapper.ToJson(jsonData, jsonWriter);
            return jsonWriter.ToString().Trim();
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Failed to format json: {ex.Message}.");
        }

        return json;
    }

    static string GetDecryptedPath(string savePath)
    {
        var fileName = Path.GetFileName(savePath);

        if (fileName.StartsWith("_sharetmpsave"))
        {
            return "";
        }

        var gameName = Directory.GetParent(savePath).Name;

        return $"SuikodenFix/Decrypted/{gameName}/{fileName}.json";
    }

    [HarmonyPatch(typeof(SystemSave), nameof(SystemSave.Save))]
    [HarmonyPostfix]
    static void Save(string path, string json)
    {
        var decryptedPath = "";

        try
        {
            decryptedPath = GetDecryptedPath(path);

            if (decryptedPath != "")
            {
                var decryptedDirectory = Path.GetDirectoryName(decryptedPath);

                if (!Directory.Exists(decryptedDirectory))
                {
                    Directory.CreateDirectory(decryptedDirectory);
                }

                json = FormatJson(json, true);
                File.WriteAllText(decryptedPath, json, System.Text.Encoding.UTF8);

                Plugin.Log.LogInfo($"Saved decrypted save at \"{decryptedPath}\".");
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Cannot save decrypted save at \"{decryptedPath}\": {ex.Message}.");
        }
    }

    [HarmonyPatch(typeof(SystemSave), nameof(SystemSave.Load))]
    [HarmonyPrefix]
    static bool Load(string path, Il2CppSystem.Action<string> cb)
    {
        bool saveLoaded = false;
        var decryptedPath = "";

        try
        {
            decryptedPath = GetDecryptedPath(path);

            if (decryptedPath != "")
            {
                if (File.Exists(decryptedPath))
                {
                    var json = File.ReadAllText(decryptedPath, System.Text.Encoding.UTF8);
                    json = FormatJson(json, false);
                    var saveData = SystemSave.HEADER + Encrypter.Encrypt(json, SystemSave.ENCRYPT_PASSWORD);

                    var display = new SystemSave.__c__DisplayClass16_0
                    {
                        cb = cb
                    };

                    var end = new Action<string>(display._Load_b__0);
                    end?.Invoke(saveData);

                    saveLoaded = true;

                    Plugin.Log.LogInfo($"Loaded decrypted save at \"{decryptedPath}\".");
                }
                else
                {
                    Plugin.Log.LogInfo($"No decrypted save found at \"{decryptedPath}\".");
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Cannot load decrypted save at \"{decryptedPath}\": {ex.Message}. Fallback to steam save.");
        }

        return !saveLoaded;
    }
}
