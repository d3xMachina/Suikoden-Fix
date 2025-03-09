extern alias GSD1;
extern alias GSD2;

using HarmonyLib;
using Share.UI.Window;

namespace Suikoden_Fix.Patches;

public class DisableMessageWindowSoundPatch
{
    [HarmonyPatch(typeof(GSD1.Event_c), nameof(GSD1.Event_c.SetEventMsgMWOpen))]
    [HarmonyPrefix]
    static void GSD1_SetEventMsgMWOpen()
    {
        ModComponent.Instance.IsEventMsgMWOpen = true;
    }

    [HarmonyPatch(typeof(GSD1.Event_c), nameof(GSD1.Event_c.SetEventMsgMWOpen))]
    [HarmonyPostfix]
    static void GSD1_SetEventMsgMWOpenPost()
    {
        ModComponent.Instance.IsEventMsgMWOpen = false;
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

    [HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlaySE), [typeof(string), typeof(int), typeof(float), typeof(bool)])]
    [HarmonyPrefix]
    static bool PlaySE(string clip, int channel, float delay, bool isLoop)
    {
        //Plugin.Log.LogWarning($"PlaySE: {clip}");

        if (ModComponent.Instance.IsEventMsgMWOpen && clip == "SD_WOP")
        {
            return false;
        }

        return true;
    }
}
