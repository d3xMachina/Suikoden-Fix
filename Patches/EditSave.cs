using HarmonyLib;

namespace Suikoden_Fix.Patches;

public class EditSavePatch
{
    [HarmonyPatch(typeof(SystemSave), nameof(SystemSave.Save))]
    [HarmonyPostfix]
    static void Save(string path, string json, SystemSave.DataSize dataSize, Il2CppSystem.Action<bool> cb, bool is_consume)
    {
        var fileName = "";

        try
        {
            fileName = System.IO.Path.GetFileName(path);
            
            if (!fileName.StartsWith("_sharetmpsave"))
            {
                fileName = "_decrypted_" + fileName + ".json";
                System.IO.File.WriteAllText(fileName, json, System.Text.Encoding.UTF8);

                Plugin.Log.LogInfo($"Saved decrypted save \"{fileName}\".");
            }
        }
        catch (System.Exception ex)
        {
            Plugin.Log.LogError($"Cannot save decrypted save \"{fileName}\": {ex.Message}.");
        }
    }

    [HarmonyPatch(typeof(SteamService), nameof(SteamService.Load))]
    [HarmonyPrefix]
    static bool Load(string path, Il2CppSystem.Action<string> end, bool isHikitugi)
    {
        bool saveLoaded = false;
        var fileName = "";

        try
        {
            fileName = System.IO.Path.GetFileName(path);

            if (!fileName.StartsWith("_sharetmpsave"))
            {
                fileName = "_decrypted_" + fileName + ".json";

                if (System.IO.File.Exists(fileName) )
                {
                    var saveData = System.IO.File.ReadAllText(fileName, System.Text.Encoding.UTF8);
                    saveData = SystemSave.HEADER + Encrypter.Encrypt(saveData, SystemSave.ENCRYPT_PASSWORD);
                    end.Invoke(saveData);
                    saveLoaded = true;

                    Plugin.Log.LogInfo($"Loaded decrypted save \"{fileName}\".");
                }
                else
                {
                    Plugin.Log.LogInfo($"No decrypted save found named \"{fileName}\".");
                }
            }
        }
        catch (System.Exception ex)
        {
            Plugin.Log.LogError($"Cannot load decrypted save named \"{fileName}\": {ex.Message}. Fallback to steam save.");
        }

        return !saveLoaded;
    }
}
