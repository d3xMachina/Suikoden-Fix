using HarmonyLib;

namespace Suikoden_Fix.Patches;

public class SpedUpSoundPatch
{
    private static bool _isInSetPitchType = false;

    private struct PitchData
    {
        public bool ok;
        public float bgmPitch;
        public float sePitch;
        public bool speedUpBattleBGM;
    };

    [HarmonyPatch(typeof(SoundManager), nameof(SoundManager.SetPitchType))]
    [HarmonyPrefix]
    static void SetPitchType(ref PitchData __state)
    {
        _isInSetPitchType = true;

        __state = new()
        {
            ok = false
        };

        // Backup the pitch values
        var soundManager = SoundManager.Instance;
        if (soundManager != null)
        {
            __state.bgmPitch = soundManager.bgmPitch;
            __state.sePitch = soundManager.sePitch;

            var config = ShareSaveData.system_config;
            if (config != null)
            {
                __state.speedUpBattleBGM = config.speedUpBattleBGM;
                __state.ok = true;

                config.speedUpBattleBGM = true;
            }
        }
    }

    [HarmonyPatch(typeof(SoundManager), nameof(SoundManager.SetPitchType))]
    [HarmonyPostfix]
    static void SetPitchTypePost(SoundManager.PitchType pitch_type, PitchData __state)
    {
        _isInSetPitchType = false;

        if (!__state.ok)
        {
            return;
        }

        // Restore the pitch values
        var soundManager = SoundManager.Instance;
        if (soundManager != null)
        {
            if (Plugin.Config.SpedUpMusic.Value == 0)
            {
                soundManager.bgmPitch = __state.bgmPitch;
            }

            if (Plugin.Config.SpedUpSoundEffect.Value == 0)
            {
                soundManager.sePitch = __state.sePitch;
            }
        }

        var config = ShareSaveData.system_config;
        if (config != null)
        {
            config.speedUpBattleBGM = __state.speedUpBattleBGM;
        }
    }

    [HarmonyPatch(typeof(SoundManager), nameof(SoundManager.UpdateBgmPitch))]
    [HarmonyPrefix]
    static bool UpdateBgmPitch()
    {
        if (_isInSetPitchType)
        {
            return Plugin.Config.SpedUpMusic.Value != 0;
        }
        
        return true;
    }

    [HarmonyPatch(typeof(SoundManager), nameof(SoundManager.UpdateSePitch))]
    [HarmonyPrefix]
    static bool UpdateSePitch()
    {
        if (_isInSetPitchType)
        {
            return Plugin.Config.SpedUpSoundEffect.Value != 0;
        }

        return true;
    }

    [HarmonyPatch(typeof(SoundManager), nameof(SoundManager.SetBgmPlayerPitch))]
    [HarmonyPrefix]
    static void SetBgmPlayerPitch(ref float pitch, out bool __state)
    {
        __state = SoundManager.timeStretchBgmEnabled;

        if (Plugin.Config.SpedUpMusic.Value > 0)
        {
            SoundManager.timeStretchBgmEnabled = Plugin.Config.SpedUpMusic.Value >= 2;

            if (Plugin.Config.SpedUpMusic.Value == 3)
            {
                pitch = ModComponent.Instance.GameSpeed;
            }
        }
    }

    [HarmonyPatch(typeof(SoundManager), nameof(SoundManager.SetBgmPlayerPitch))]
    [HarmonyPostfix]
    static void SetBgmPlayerPitchPost(bool __state)
    {
        SoundManager.timeStretchBgmEnabled = __state;
    }

    [HarmonyPatch(typeof(SoundManager), nameof(SoundManager.SetSePlayerPitch))]
    [HarmonyPrefix]
    static void SetSePlayerPitch(ref float pitch, out bool __state)
    {
        __state = SoundManager.timeStretchSeEnabled;

        if (Plugin.Config.SpedUpSoundEffect.Value > 0)
        {
            SoundManager.timeStretchSeEnabled = Plugin.Config.SpedUpSoundEffect.Value >= 2;

            if (Plugin.Config.SpedUpSoundEffect.Value == 3)
            {
                pitch = ModComponent.Instance.GameSpeed;
            }
        }
    }

    [HarmonyPatch(typeof(SoundManager), nameof(SoundManager.SetSePlayerPitch))]
    [HarmonyPostfix]
    static void SetSePlayerPitchPost(bool __state)
    {
        SoundManager.timeStretchSeEnabled = __state;
    }

    /*
    [HarmonyPatch(typeof(SoundManager), nameof(SoundManager.PlaySE), [typeof(string), typeof(int), typeof(float), typeof(bool)])]
    [HarmonyPrefix]
    static void PlaySE(string clip, int channel, float delay, bool isLoop)
    {
        Plugin.Log.LogWarning($"SE={clip}");
    }
    */
}
