extern alias GSD1;
extern alias GSD2;

using HarmonyLib;
using System;
using UnityEngine.InputSystem;

namespace Suikoden_Fix.Patches;

public class SpeedHackPatch
{
    private static bool _isInChapterManagerUpdate = false;
    private static int _chapterUpdateCount = 0;

    [HarmonyPatch(typeof(GRInputManager), nameof(GRInputManager.Create))]
    [HarmonyPostfix]
    static void RemoveBinding(GRInputManager __instance, GRInputManager.Mode _mode)
    {
        if (__instance.currentMap == null || __instance.currentMap.Count == 0)
        {
            return;
        }

        foreach (InputActionMap actionMap in __instance.currentMap)
        {
            if (actionMap == null)
            {
                continue;
            }

            foreach (InputAction action in actionMap.actions)
            {
                for (int i = action.bindings.Count - 1; i >= 0; i--)
                {
                    InputBinding binding = action.bindings[i];

                    if (binding.path.Contains("/rightTrigger"))
                    {
                        action.ChangeBinding(i).Erase();
                        Plugin.Log.LogInfo($"Removed binding {binding.path} in action '{action.name}' in map '{actionMap.name}'");
                    }
                    else
                    {
                        //Plugin.Log.LogInfo($"Binding {binding.path} in action '{action.name}' in map '{actionMap.name}'");
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(GSD1.ChapterManager), nameof(GSD1.ChapterManager.Update))]
    [HarmonyPrefix]
    static void GSD1_ChapterManagerUpdate()
    {
        _isInChapterManagerUpdate = true;
        _chapterUpdateCount = 0;
    }

    [HarmonyPatch(typeof(GSD1.ChapterManager), nameof(GSD1.ChapterManager.Update))]
    [HarmonyPostfix]
    static void GSD1_ChapterManagerUpdatePost()
    {
        _isInChapterManagerUpdate = false;
    }

    [HarmonyPatch(typeof(Framework.Chapter), nameof(Framework.Chapter.Update))]
    [HarmonyPrefix]
    static void ChapterUpdate()
    {
        if (_isInChapterManagerUpdate)
        {
            if (_chapterUpdateCount > 0)
            {
                // Disable inputs so only the first update processes inputs
                var sysWork = GSD1.GlobalWork.Instance?.sys_work;
                if (sysWork != null)
                {
                    sysWork.PadTrig = 0;
                    sysWork.PadNTrig = 0;
                    sysWork.PadRTrig = 0;
                }
            }

            ++_chapterUpdateCount;
        }
    }

    [HarmonyPatch(typeof(GSD1.Pad), nameof(GSD1.Pad.PadUpdate))]
    [HarmonyPrefix]
    static bool GSD1_PadUpdate()
    {
        // Update inputs only once when frame skip is disabled
        return !_isInChapterManagerUpdate || _chapterUpdateCount == 0;
    }

    [HarmonyPatch(typeof(SoundManager), nameof(SoundManager.GetDecOffset))]
    [HarmonyPrefix]
    static bool FixBgmPlayTime(int channel, ref long __result)
    {
        var bgmPlayers = SoundManager.Instance?.bgmPlayer;
        if (bgmPlayers == null || channel >= bgmPlayers.Count)
        {
            return true;
        }

        var bgmPlayer = bgmPlayers[channel];
        if (bgmPlayer == null)
        {
            return true;
        }

        var playback = bgmPlayer.playback;
        if (!playback.GetNumPlayedSamples(out var samples, out var sampleRate))
        {
            return true;
        }

        __result = (long)Math.Round((double)samples / sampleRate * 1000);
        //Plugin.Log.LogWarning($"DecOffset={__result} pitch={SoundManager.Instance.bgmPitch}");

        return false;
    }
}
