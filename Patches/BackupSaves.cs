using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Suikoden_Fix.Patches;

public class BackupSavesPatch
{
    static string GetBackupPrefix(string path)
    {
        var gameName = Directory.GetParent(path).Name;
        return $"_backup_{gameName}_";
    }

    static void RemoveOldestBackups(string path)
    {
        var backupPrefix = GetBackupPrefix(path);

        Queue<string> existingBackups = new(
            Directory.GetFiles(".", $"{backupPrefix}*")
                .Select(Path.GetFileName)
                .OrderBy(name => name)
        );

        while (existingBackups.Count > Plugin.Config.BackupSave.Value)
        {
            var oldestBackup = existingBackups.Dequeue();
            File.Delete(oldestBackup);
            Plugin.Log.LogInfo($"Backup {oldestBackup} deleted.");
        }
    }

    static string GetBackupSaveName(string path)
    {
        var fileName = Path.GetFileName(path);

        if (fileName.StartsWith("_sharetmpsave"))
        {
            return "";
        }

        var backupPrefix = GetBackupPrefix(path);
        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");

        return $"{backupPrefix}{timestamp}_{fileName}";
    }

    [HarmonyPatch(typeof(SteamService), nameof(SteamService.Save))]
    [HarmonyPostfix]
    static void Save(string path, string json)
    {
        var fileName = "";

        try
        {
            fileName = GetBackupSaveName(path);

            if (fileName != "")
            {
                File.WriteAllText(fileName, json, System.Text.Encoding.UTF8);
                RemoveOldestBackups(path);

                Plugin.Log.LogInfo($"Backup \"{fileName}\" saved.");
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Cannot save backup \"{fileName}\": {ex.Message}.");
        }
    }
}
