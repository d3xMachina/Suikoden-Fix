# Suikoden Fix

A BepInEx plugin containing a bunch of tweaks for Suikoden I & II HD Remaster.

## Features

All features are optional.

- Ultrawide support.
- Exit the game, pause the game or reset the game to its title screen.
- Disable vignette.
- Skip splashscreen and intro movies.
- Fast transitions on zone changes, loading and title menu.
- Display messages instantly.
- Dash as a toggle.
- Disable diagonal movements.
- Remember the battle speed between battles.
- Save anywhere in the last save slot. (be careful to not softlock yourself)
- Disable autosave notifications.
- Raise the money cap to 999,999,999.
- Damage, health, experience, encounter rate, loot rate and money multipliers.
- Run on worldmap and unlock x4 battle speed without Stallion.
- MchDohl and both Kasumi and Valeria available at Leona's.
- Instant weapon upgrade, item appraisal and Richmond investigations.
- Speed up the game. (with setting to affect the game timers)
- Edit your saves.
- Disable or alter the speed up of the music and sound effects when the battle speed change.
- Disable the sound when a message window appears and startled sound effects.
- Change the background color of some windows.
- Adjust bloom and depth of fields effects.
- Adjust framerate, vsync and disable frame skipping.
- Rare finds always in stock and always win minigames.

## Installation

- Download BepInEx 6.0.0-pre.2 IL2CPP build from [here](https://github.com/BepInEx/BepInEx/releases/download/v6.0.0-pre.2/BepInEx-Unity.IL2CPP-win-x64-6.0.0-pre.2.zip) and extract the content to the game directory (where `Suikoden I and II HD Remaster.exe` is located). Replace the files if asked.
- Download the mod [here](https://github.com/d3xMachina/Suikoden-Fix/releases/latest) and extract the content to `(GAME_PATH)\BepInEx\plugins\`. Replace the files if asked.
- On Steam Deck, add to the Steam launch options : `export WINEDLLOVERRIDES="winhttp=n,b"; %command%`
- Run the game once to generate the config file, change the config in `(GAME_PATH)\BepInEx\config\d3xMachina.suikoden_fix.cfg` and restart the game.
- To remove the console : open `(GAME_PATH)\BepInEx\config\BepInEx.cfg` and replace `Enabled = true` with `Enabled = false` in the `[Logging.Console]` section.

## Save editing

- Replace `EditSave = false` with `EditSave = true` in the configuration file `(GAME_PATH)\BepInEx\config\d3xMachina.suikoden_fix.cfg`.
- Backup your saves located in `(GAME_PATH)\Save\(USER_ID)\` on Windows or `(STEAM_LIBRARY_PATH)/steamapps/compatdata/1932640/pfx` on Linux in case you make a mistake.
- After saving in game, go to your game folder and you will have the save file in the json format. The file name starts with `_decrypted` followed by either `_gsd1` for Suikoden 1 or `_gsd2` for Suikoden 2, then `_Data` and the save slot number.
- Modify the content of the file with a text editor like notepad. You can refer to [this documentation](https://github.com/asilverthorn/suikoden_ref/blob/main/Suikoden1_Remaster_Save_Editing.md) for Suikoden 1.
- Load your save again.
- You can save again to have the changes persist and disable the `EditSave` option in the configuration file.


## License

Suikoden Fix is available on Github under the MIT license.
