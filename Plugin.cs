using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using Suikoden_Fix.Patches;
using HarmonyLib;
using System;

namespace Suikoden_Fix;

[BepInPlugin("d3xMachina.suikoden_fix", "Suikoden Fix", "1.3.9")]
public partial class Plugin : BasePlugin
{
    public static new ManualLogSource Log;
    public static new ModConfiguration Config;

    public override void Load()
    {
        Log = base.Log;

        Log.LogInfo("Loading...");

        Config = new ModConfiguration(base.Config);
        Config.Init();
        if (ModComponent.Inject())
        {
            ApplyPatches();
        }
    }

    private void ApplyPatches()
    {
        bool fastTransition = false;
        bool disableBattleSpeedChange = false;
        bool disableSoundEffect = false;
        bool menuDetection = false;

        if (Config.FPS.Value >= 0 || Config.Vsync.Value >= 0)
        {
            ApplyPatch(typeof(FrameratePatch));
        }

        if (Config.SkipSplashscreens.Value || Config.SkipMovies.Value)
        {
            ApplyPatch(typeof(SkipIntroPatch));
        }

        if (Config.DisableVignette.Value || Config.DisableMaskedVignette.Value)
        {
            ApplyPatch(typeof(DisableVignettePatch));
        }

        if (Config.ToggleDash.Value || Config.DisableDiagonalMovements.Value)
        {
            ApplyPatch(typeof(InputMovementsPatch));

            if (Config.ToggleDash.Value)
            {
                menuDetection = true;
            }
        }

        if (Config.DisableFootStepSound.Value)
        {
            ApplyPatch(typeof(DisableFootStepSoundPatch));
        }

        if (Config.ZoneTransitionFactor.Value >= 0f)
        {
            ApplyPatch(typeof(FastZoneTransitionPatch));
            fastTransition = true;
        }

        if (Config.LoadingTransitionFactor.Value >= 0f)
        {
            ApplyPatch(typeof(FastLoadingTransitionPatch));
            fastTransition = true;
        }

        if (Config.TitleMenuTransitionFactor.Value >= 0f)
        {
            ApplyPatch(typeof(FastTitleMenuTransitionPatch));
            fastTransition = true;
        }

        if (fastTransition)
        {
            ApplyPatch(typeof(FastTransitionPatch));
        }

        if (Config.SpedUpMusic.Value >= 0 || Config.SpedUpSoundEffect.Value >= 0)
        {
            ApplyPatch(typeof(SpedUpSoundPatch));
        }

        if (Config.SpeedHackFactor.Value > 1)
        {
            ApplyPatch(typeof(SpeedHackPatch));
            disableBattleSpeedChange = true;
            menuDetection = true;
        }

        if (Config.RememberBattleSpeed.Value)
        {
            disableBattleSpeedChange = true;
            menuDetection = true;
        }

        if (disableBattleSpeedChange)
        {
            ApplyPatch(typeof(DisableBattleSpeedChangePatch));
        }

        if (menuDetection)
        {
            ApplyPatch(typeof(MenuDetectionPatch));
        }

        if (Config.DisableMessageWindowSound.Value)
        {
            ApplyPatch(typeof(DisableMessageWindowSoundPatch));
            disableSoundEffect = true;
        }

        if (Config.DisableStartledSound.Value)
        {
            ApplyPatch(typeof(DisableReactionSoundPatch));
            disableSoundEffect = true;
        }

        if (disableSoundEffect)
        {
            ApplyPatch(typeof(DisableSoundEffectPatch));
        }

        if (!Config.WindowBGColor.Value.IsNullOrWhiteSpace())
        {
            ApplyPatch(typeof(WindowColorPatch));
        }

        if (Config.ClassicMode.Value)
        {
            ApplyPatch(typeof(ClassicModePatch));
        }

        if (Config.ExitApplication.Value)
        {
            ApplyPatch(typeof(ExitApplicationPatch));
        }
        
        if (Config.EditSave.Value)
        {
            ApplyPatch(typeof(EditSavePatch));
        }
        
        if (Config.PlayerDamageMultiplier.Value != 1f || Config.MonsterDamageMultiplier.Value != 1f)
        {
            ApplyPatch(typeof(DamageMultiplierPatch));
        }
        
        if (Config.MonsterHealthMultiplier.Value != 1f)
        {
            ApplyPatch(typeof(HealthMultiplierPatch));
        }
        
        if (Config.ExperienceMultiplier.Value != 1f)
        {
            ApplyPatch(typeof(ExperienceMultiplierPatch));
        }
        
        if (Config.MoneyMultiplier.Value != 1f)
        {
            ApplyPatch(typeof(MoneyMultiplierPatch));
        }
        
        if (Config.LootMultiplier.Value != 1f)
        {
            ApplyPatch(typeof(LootMultiplierPatch));
        }
        
        if (Config.InstantMessage.Value)
        {
            ApplyPatch(typeof(InstantMessagePatch));
        }

        if (Config.EncounterRateMultiplier.Value != 1f)
        {
            ApplyPatch(typeof(EncounterRatePatch));
        }

        if ((Config.Height.Value > 0 && Config.Width.Value > 0) || Config.Fullscreen.Value >= 0)
        {
            ApplyPatch(typeof(ResolutionPatch));
        }

        if (Config.RecoverAfterBattle.Value)
        {
            ApplyPatch(typeof(RecoverAfterBattlePatch));
        }

        if (Config.DisableAutoSaveNotification.Value)
        {
            ApplyPatch(typeof(DisableAutoSaveNotificationPatch));
        }

        if (Config.UncapMoney.Value)
        {
            ApplyPatch(typeof(UncapMoneyPatch));
        }

        if (Config.InstantRichmondInvestigation.Value)
        {
            ApplyPatch(typeof(InstantRichmondInvestigationPatch));
        }

        if (Config.FixKeyboardBindings.Value)
        {
            ApplyPatch(typeof(FixKeyboardBindingsPatch));
        }

        if (Config.BackupSave.Value > 0)
        {
            ApplyPatch(typeof(BackupSavesPatch));
        }

        if (Config.InstantShop.Value)
        {
            ApplyPatch(typeof(InstantShopPatch));
        }

        if (Config.StallionBoons.Value)
        {
            ApplyPatch(typeof(StallionBoonsPatch));
        }

        if (Config.SaveAnywhere.Value)
        {
            ApplyPatch(typeof(SaveAnywherePatch));
        }

        Log.LogInfo("Patches applied!");
    }

    private void ApplyPatch(Type type)
    {
        Log.LogInfo($"Patching {type.Name}...");

        Harmony.CreateAndPatchAll(type);
    }
}
