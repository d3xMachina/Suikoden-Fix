using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Suikoden_Fix.Patches;

public class BackupSavesPatch
{
    static void RemoveOldestBackups(string backupDirectory)
    {
        Queue<string> existingBackups = new(
            Directory.GetFiles(backupDirectory, "*")
                .OrderBy(path => Path.GetFileName(path))
        );

        while (existingBackups.Count > Plugin.Config.BackupSave.Value)
        {
            var oldestBackup = existingBackups.Dequeue();
            File.Delete(oldestBackup);
            Plugin.Log.LogInfo($"Deleted backup at {oldestBackup}.");
        }
    }

    static string GetBackupPath(string savePath)
    {
        var fileName = Path.GetFileName(savePath);

        if (fileName.StartsWith("_sharetmpsave"))
        {
            return "";
        }

        var gameName = Directory.GetParent(savePath).Name;
        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");

        return $"SuikodenFix/Backup/{gameName}/{timestamp}_{fileName}";
    }

    [HarmonyPatch(typeof(SteamService), nameof(SteamService.Save))]
    [HarmonyPostfix]
    static void Save(string path, string json)
    {
        var backupPath = "";

        try
        {
            backupPath = GetBackupPath(path);

            if (backupPath != "")
            {
                var backupDirectory = Path.GetDirectoryName(backupPath);

                if (!Directory.Exists(backupDirectory))
                {
                    Directory.CreateDirectory(backupDirectory);
                }

                File.WriteAllText(backupPath, json, System.Text.Encoding.UTF8);
                RemoveOldestBackups(backupDirectory);

                Plugin.Log.LogInfo($"Saved backup at \"{backupPath}\".");
            }
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"Cannot save backup at \"{backupPath}\": {ex.Message}.");
        }
    }
}
