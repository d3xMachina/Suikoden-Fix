using BepInEx.Configuration;

namespace Suikoden_Fix;

public sealed class ModConfiguration
{
    private ConfigFile _config;
    public ConfigEntry<int> FPS;
    public ConfigEntry<int> Vsync;
    public ConfigEntry<int> Width;
    public ConfigEntry<int> Height;
    public ConfigEntry<int> Fullscreen;
    public ConfigEntry<bool> NoFrameSkip;
    public ConfigEntry<bool> SkipSplashscreens;
    public ConfigEntry<bool> SkipMovies;
    public ConfigEntry<float> LoadingTransitionFactor;
    public ConfigEntry<float> TitleMenuTransitionFactor;
    public ConfigEntry<float> ZoneTransitionFactor;
    public ConfigEntry<float> ZoneTransitionFactor2;
    public ConfigEntry<bool> DisableVignette;
    public ConfigEntry<bool> DisableMaskedVignette;
    public ConfigEntry<bool> DisableDepthOfField;
    public ConfigEntry<float> BloomMultiplier;
    public ConfigEntry<bool> SmoothSprites;
    public ConfigEntry<bool> DisableDiagonalMovements;
    public ConfigEntry<bool> ToggleDash;
    public ConfigEntry<bool> SaveAnywhere;
    public ConfigEntry<int> SpeedHackFactor;
    public ConfigEntry<bool> SpeedHackAffectsGameTimer;
    public ConfigEntry<bool> SpeedHackAffectsSpecialMenus;
    public ConfigEntry<bool> SpeedHackAffectsMessageBoxes;
    public ConfigEntry<bool> NoSpeedHackInBattle;
    public ConfigEntry<int> SpedUpMusic;
    public ConfigEntry<int> SpedUpSoundEffect;
    public ConfigEntry<bool> DisableMessageWindowSound;
    public ConfigEntry<bool> DisableStartledSound;
    public ConfigEntry<string> WindowBGColor;
    public ConfigEntry<bool> DisableAutoSaveNotification;
    public ConfigEntry<bool> RememberBattleSpeed;
    public ConfigEntry<bool> ClassicMode;
    public ConfigEntry<bool> ExitApplication;
    public ConfigEntry<bool> ResetGame;
    public ConfigEntry<bool> PauseGame;
    public ConfigEntry<bool> EditSave;
    public ConfigEntry<int> BackupSave;
    public ConfigEntry<float> PlayerDamageMultiplier;
    public ConfigEntry<float> MonsterDamageMultiplier;
    public ConfigEntry<float> MonsterHealthMultiplier;
    public ConfigEntry<float> ExperienceMultiplier;
    public ConfigEntry<float> LootMultiplier;
    public ConfigEntry<float> EncounterRateMultiplier;
    public ConfigEntry<bool> RecoverAfterBattle;
    public ConfigEntry<float> MoneyMultiplier;
    public ConfigEntry<bool> InstantMessage;
    public ConfigEntry<bool> UncapMoney;
    public ConfigEntry<bool> InstantRichmondInvestigation;
    public ConfigEntry<bool> InstantShop;
    public ConfigEntry<bool> StallionBoons;
    public ConfigEntry<bool> RareFindsAlwaysInStock;
    public ConfigEntry<bool> EasyMinigames;
    public ConfigEntry<bool> BetterLeona;
    public ConfigEntry<bool> DisableBinaryPatches;
    public ConfigEntry<int> EditText;
    public ConfigEntry<int> AllItemsInHQ;
    public ConfigEntry<bool> MoreTeleportLocations;

    public ModConfiguration(ConfigFile config)
    {
        _config = config;
    }

    public void Init()
    {
        FPS = _config.Bind(
             "Display",
             "FPS",
             -1,
             "Set the FPS limit. Set to 0 to uncap, a positive value, or -1 to use the default behavior. The game loop doesn't update more than 60 times a seconds and higher FPS can make the game slower and/or stutter (barely noticeable at high frame rate). If you use an external FPS limiter you can set this to 0."
        );

        Vsync = _config.Bind(
             "Display",
             "Vsync",
             -1,
             "Set to 0 to disable VSync, 1 to enable VSync or -1 to use the default behavior. When on, the framerate will match your monitor refresh rate. Higher refresh rate than 60 Hz can make the game slower and/or stutter (barely noticeable at high refresh rate). It is recommended to disable the Vsync in game or here to have the game running at 60 FPS and to enable it in the GPU driver."
        );

        Width = _config.Bind(
             "Display",
             "Width",
             -1,
             "Set to your desired width resolution."
        );

        Height = _config.Bind(
             "Display",
             "Height",
             -1,
             "Set to your desired height resolution."
        );

        Fullscreen = _config.Bind(
             "Display",
             "Fullscreen",
             -1,
             "Set to 0 for windowed mode or 1 for fullscreen mode. It also correct the aspect ratio for movies from the original Suikoden."
        );

        NoFrameSkip = _config.Bind(
             "Display",
             "NoFrameSkip",
             false,
             "Only use this option if your game is running at 60 FPS, otherwise the game will be slower or faster."
        );

        SkipSplashscreens = _config.Bind(
             "Skip",
             "SkipSplashscreens",
             true,
             "Skip the intro splashscreens."
        );

        SkipMovies = _config.Bind(
             "Skip",
             "SkipMovies",
             false,
             "Skip the intro movie."
        );

        LoadingTransitionFactor = _config.Bind(
             "Skip",
             "LoadingTransitionFactor",
             -1f,
             "Change the speed of fade in/out on the loading screen. Set to 0 for instant transition, a positive value to speed up or -1 for the default behavior."
        );

        TitleMenuTransitionFactor = _config.Bind(
             "Skip",
             "TitleMenuTransitionFactor",
             -1f,
             "Change the speed of fade in/out in the main menu. Set to 0 for instant transition, a positive value to speed up or -1 for the default behavior."
        );

        ZoneTransitionFactor = _config.Bind(
             "Skip",
             "ZoneTransitionFactor",
             -1f,
             "(Suikoden 1 only) Change the speed of fade in/out when changing zone. Set to 0 for instant transition, a positive value to speed up or -1 for the default behavior."
        );

        ZoneTransitionFactor2 = _config.Bind(
             "Skip",
             "ZoneTransitionFactor2",
             -1f,
             "(Suikoden 2 only, might cause some events to not trigger!!!) Change the speed of fade in/out when changing zone. Set to 0 for instant transition, a positive value to speed up or -1 for the default behavior."
        );

        DisableVignette = _config.Bind(
             "Visual",
             "DisableVignette",
             false,
             "Disable the vignette effect (darkened corners) that is displayed in some scenes."
        );

        DisableMaskedVignette = _config.Bind(
             "Visual",
             "DisableMaskedVignette",
             false,
             "Disable the vignette effect (darkened corners) for vignettes with a custom appearance that is displayed in some scenes (in the night time intro scene of Suikoden 2 for example)."
        );

        DisableDepthOfField = _config.Bind(
             "Visual",
             "DisableDepthOfField",
             false,
             "Disable the depth of field effect visible in battles."
        );

        BloomMultiplier = _config.Bind(
             "Visual",
             "BloomMultiplier",
             1f,
             "Change the intensity of the bloom effects. Set to 0 to disable the bloom effects entirely."
        );

        SmoothSprites = _config.Bind(
             "Visual",
             "SmoothSprites",
             false,
             "Smooth out the sprites using a bilinear filter. It might make the sprites a bit blurry."
        );

        DisableDiagonalMovements = _config.Bind(
             "Movement",
             "DisableDiagonalMovements",
             false,
             "Restrict the movements to 4 directions instead of 8, except on the worldmap."
        );

        ToggleDash = _config.Bind(
             "Movement",
             "ToggleDash",
             false,
             "Make the dash command a toggle instead of having to hold it."
        );

        SaveAnywhere = _config.Bind(
             "Misc",
             "SaveAnywhere",
             false,
             "Allows you to save anywhere to the last save slot using the select button or the F1 key. Be careful to not softlock yourself!"
        );

        SpeedHackFactor = _config.Bind(
             "Skip",
             "SpeedHackFactor",
             1,
             "Increase the game speed by X when the R2 button or the T key is pressed. Minimum value of 1."
        );

        SpeedHackAffectsGameTimer = _config.Bind(
             "Skip",
             "SpeedHackAffectsGameTimer",
             true,
             "The speedhack will change the game timer (used for events) accordingly. It will not take effect in battle. Only relevant when using SpeedHackFactor."
        );

        SpeedHackAffectsSpecialMenus = _config.Bind(
             "Skip",
             "SpeedHackAffectsSpecialMenus",
             false,
             "The speedhack will take effect in shop menus and some special menus like tablet, investigation, teleport..."
        );

        SpeedHackAffectsMessageBoxes = _config.Bind(
             "Skip",
             "SpeedHackAffectsMessageBoxes",
             false,
             "The speedhack will take effect in dialogues when a message box appears"
        );

        NoSpeedHackInBattle = _config.Bind(
             "Skip",
             "NoSpeedHackInBattle",
             false,
             "Disable the speedhack in battle. Only relevant when using SpeedHackFactor."
        );

        SpedUpMusic = _config.Bind(
             "Audio",
             "SpedUpMusic",
             -1,
             "Change the pitch or speed of the music when you change the game speed. Set 0 to have the sounds remain the same, 1 to have the sounds played at a higher pitch, 2 to have the sounds played faster at specific speeds, 3 to have the sounds played faster matching the game speed, or -1 for the default behavior."
        );

        SpedUpSoundEffect = _config.Bind(
             "Audio",
             "SpedUpSoundEffect",
             -1,
             "Change the pitch or speed of sound effects when you change the game speed. Set 0 to have the sounds remain the same, 1 to have the sounds played at a higher pitch, or -1 for the default behavior."
        );

        DisableMessageWindowSound = _config.Bind(
             "Audio",
             "DisableMessageWindowSound",
             false,
             "Don't play a sound effect when a message window appears."
        );

        DisableStartledSound = _config.Bind(
             "Audio",
             "DisableStartledSound",
             false,
             "(Suikoden 2 only) Don't play a sound effect when a character is startled (with the icon above the head)."
        );

        WindowBGColor = _config.Bind(
             "Visual",
             "WindowBGColor",
             "",
             "Change the background color of most windows instead of the default black. Use the hex format (#000C7A for example)."
        );

        DisableAutoSaveNotification = _config.Bind(
             "Visual",
             "DisableAutoSaveNotification",
             false,
             "Don't show the autosave notification window."
        );

        RememberBattleSpeed = _config.Bind(
             "Misc",
             "RememberBattleSpeed",
             false,
             "Remember the battle speed between battles. It will also disable the speedhack in battle like NoSpeedHackInBattle."
        );

        ClassicMode = _config.Bind(
             "Advanced",
             "ClassicMode",
             false,
             "Active the classic mode (PS1 visuals). This is a proof of concept, it is unfinished and many assets are missings."
        );

        ExitApplication = _config.Bind(
             "Misc",
             "ExitApplication",
             true,
             "Allow you to exit the application by pressing the start button or the escape key on the title selection screen."
        );

        ResetGame = _config.Bind(
             "Misc",
             "ResetGame",
             true,
             "Allow you to go back to the title screen of the game by pressing start + left shoulder + right shoulder on the gamepad or Escape + R on the keyboard."
        );

        PauseGame = _config.Bind(
             "Misc",
             "PauseGame",
             true,
             "Pause the game by pressing the start button or the tab key. It pauses everything including the game timers."
        );

        EditSave = _config.Bind(
             "Advanced",
             "EditSave",
             false,
             "Allow you to edit your saves. Make sure to backup your saves for safety. After saving, go to your game folder and you will have the save files in the json format (file names start with \"_decrypted\". Modify the content of the file then load your save again. You can save again to have the changes persist and disable this option."
        );

        EditText = _config.Bind(
             "Advanced",
             "EditText",
             0,
             "Allow you to edit the game texts. Set to 1 to enable, 2 to enable and log the game texts, and 0 to disable. The game texts are loaded from the GameTexts.json file in the game folder (you need to create one with UTF8 encoding). The logs of the game texts are saved in the GameTextsLog.txt file in the game folder, they are also logged in the console too but with the wrong encoding. The valid languages are : Japanese, English, French, Italian, German, Spanish, ChineseZhHant and ChineseZhHans. Example of a valid GameTexts.json :" +
@"
{
    ""English"": {
        ""add_message"": {
            ""1118"": ""LOAD"",
            ""1398"": ""Backspace∠Confirm""
        },
        ""message"" : {
            ""3278"": ""Master""
        }
    }
}"
        );

        BackupSave = _config.Bind(
             "Advanced",
             "BackupSave",
             0,
             "Keep a backup of the last N saves for each game in the game folder where N is the number you set for this option. File names start with \"_backup\"."
        );

        DisableBinaryPatches = _config.Bind(
             "Advanced",
             "DisableBinaryPatches",
             false,
             "Disable the binary patches. Use this if the game was updated but not Suikoden Fix since it could crash or have unexpected behavior otherwise. It will disable some options partially or completly depending on which one."
        );

        PlayerDamageMultiplier = _config.Bind(
             "Cheat",
             "PlayerDamageMultiplier",
             1f,
             "Multiply the damage of your party members in battle. (Damage capped at 9999 before bonus)"
        );

        MonsterDamageMultiplier = _config.Bind(
             "Cheat",
             "MonsterDamageMultiplier",
             1f,
             "Multiply the damage of enemies in battle. (Damage capped at 9999 before bonus)"
        );
        
        MonsterHealthMultiplier = _config.Bind(
             "Cheat",
             "MonsterHealthMultiplier",
             1f,
             "Multiply the HP of enemies in battle. (HP capped at 32767)"
        );

        ExperienceMultiplier = _config.Bind(
             "Cheat",
             "ExperienceMultiplier",
             1f,
             "Multiply the experience gained after a battle."
        );

        MoneyMultiplier = _config.Bind(
             "Cheat",
             "MoneyMultiplier",
             1f,
             "Multiply the money gained after a battle."
        );

        LootMultiplier = _config.Bind(
             "Cheat",
             "LootMultiplier",
             1f,
             "Multiply the chance to loot an item after a battle. A value of 255 will guarantee the monster will drop its items (if it has items it can drop)."
        );

        EncounterRateMultiplier = _config.Bind(
             "Cheat",
             "EncounterRateMultiplier",
             1f,
             "Multiply the chances to get into a battle when walking. A value of 0 will disable encounters and 512 will guarantee you the highest probability. Encounters are checked every few steps mostly."
        );

        RecoverAfterBattle = _config.Bind(
             "Cheat",
             "RecoverAfterBattle",
             false,
             "Your party members recover their HP and MP after a battle."
        );

        InstantMessage = _config.Bind(
             "Skip",
             "InstantMessage",
             false,
             "Display messages instantly."
        );

        InstantRichmondInvestigation = _config.Bind(
             "Skip",
             "InstantRichmondInvestigation",
             false,
             "(Suikoden 2 only) No need to wait for the Richmond investigations."
        );

        InstantShop = _config.Bind(
             "Skip",
             "InstantShop",
             false,
             "No need to wait when doing operations in shops like leveling up weapons or appraising items."
        );

        UncapMoney = _config.Bind(
             "Misc",
             "UncapMoney",
             false,
             "Increase the money cap from 999,999 to 999,999,999."
        );

        StallionBoons = _config.Bind(
             "Misc",
             "StallionBoons",
             false,
             "Unlock the x4 battle speed and run on the worldmap without Stallion."
        );

        RareFindsAlwaysInStock = _config.Bind(
             "Cheat",
             "RareFindsAlwaysInStock",
             false,
             "(Suikoden 2 only) Rare finds will always be in stock in the shops. Unique items can only be bought once."
        );

        EasyMinigames = _config.Bind(
             "Cheat",
             "EasyMinigames",
             false,
             "You will win minigames even if you lose. In the fish minigame, catching will be instant and give a random fish with equal probability."
        );

        BetterLeona = _config.Bind(
             "Cheat",
             "BetterLeona",
             false,
             "(Suikoden 2 only) McDohl will be available in Leona's if you recruited him. Both Kasumi and Valeria will be available if you recruited one of them."
        );

        MoreTeleportLocations = _config.Bind(
             "Cheat",
             "MoreTeleportLocations",
             false,
             "(Suikoden 2 only) Add Gregminster and the secret room in Radat Town to the teleport locations. The locations are added at the end of the list, only if you visited them before."
        );

        AllItemsInHQ = _config.Bind(
             "Cheat",
             "AllItemsInHQ",
             0,
             "Shops in the headquarters (armor, item and rune shops) sell all items in their category. Set to 1 to enable, 2 to also have key items and 0 to disable."
        );
    }
}
