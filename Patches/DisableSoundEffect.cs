extern alias GSD1;
extern alias GSD2;

using HarmonyLib;
using Share.UI.Window;

namespace Suikoden_Fix.Patches;

public class DisableSoundEffectPatch
{
    public static bool IsEventMsgMWOpen = false;
    public static bool IsECObjEfctCon = false;

    [HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlaySE), [typeof(string), typeof(int), typeof(float), typeof(bool)])]
    [HarmonyPrefix]
    static bool PlaySE(string clip, int channel, float delay, bool isLoop)
    {
        //Plugin.Log.LogWarning($"PlaySE: {clip}");

        if (IsEventMsgMWOpen && clip == "SD_WOP" ||
            IsECObjEfctCon && clip == "SD_HD_REACTION1")
        {
            return false;
        }

        return true;
    }
}

public class DisableMessageWindowSoundPatch
{
    [HarmonyPatch(typeof(GSD1.Event_c), nameof(GSD1.Event_c.SetEventMsgMWOpen))]
    [HarmonyPrefix]
    static void GSD1_SetEventMsgMWOpen()
    {
        DisableSoundEffectPatch.IsEventMsgMWOpen = true;
    }

    [HarmonyPatch(typeof(GSD1.Event_c), nameof(GSD1.Event_c.SetEventMsgMWOpen))]
    [HarmonyPostfix]
    static void GSD1_SetEventMsgMWOpenPost()
    {
        DisableSoundEffectPatch.IsEventMsgMWOpen = false;
    }

    [HarmonyPatch(typeof(GSD2.WindowManager), nameof(GSD2.WindowManager.PlaySEMessageWindow))]
    [HarmonyPrefix]
    static bool GSD2_PlaySEMessageWindow(WINDOW_PLAY_SE_TYPE type)
    {
        if (type == WINDOW_PLAY_SE_TYPE.OPEN)
        {
            return false;
        }

        return true;
    }
}

public class DisableReactionSoundPatch
{
    // Only relevant for GSD2

    [HarmonyPatch(typeof(GSD2.EVENTCON), nameof(GSD2.EVENTCON.ECObjEfctCon))]
    [HarmonyPrefix]
    static void GSD2_SetEventMsgMWOpen()
    {
        DisableSoundEffectPatch.IsECObjEfctCon = true;
    }

    [HarmonyPatch(typeof(GSD2.EVENTCON), nameof(GSD2.EVENTCON.ECObjEfctCon))]
    [HarmonyPostfix]
    static void GSD2_SetEventMsgMWOpenPost()
    {
        DisableSoundEffectPatch.IsECObjEfctCon = false;
    }
}