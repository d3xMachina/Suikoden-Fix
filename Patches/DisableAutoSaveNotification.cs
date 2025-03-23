extern alias GSD1;
extern alias GSD2;

using HarmonyLib;

namespace Suikoden_Fix.Patches;

public class DisableAutoSaveNotificationPatch
{
    [HarmonyPatch(typeof(GSD1.Village_c), nameof(GSD1.Village_c.check_auto_save))]
    [HarmonyPostfix]
    static void GSD1_DisableNotification(ref bool __result)
    {
        if (__result)
        {
            GSD1.UISaveLoad1.Save(0);
            Plugin.Log.LogInfo("AutoSave!");
        }

        __result = false;
    }

    [HarmonyPatch(typeof(GSD2.MACHICON), nameof(GSD2.MACHICON.AutoSaveCheck))]
    [HarmonyPostfix]
    static void GSD2_DisableNotification(ref bool __result)
    {
        if (__result)
        {
            // the save is already done in AutoSaveCheck
            Plugin.Log.LogInfo("AutoSave!");
        }

        __result = false;
    }
}
