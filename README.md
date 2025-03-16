# Suikoden Fix

A BepInEx plugin containing a bunch of tweaks for Suikoden I & II HD Remaster.

## Features

All features are optional.

- Ultrawide support.
- Exit the game or reset the game to its title screen.
- Disable vignette.
- Skip splashscreen and intro movies.
- Fast transitions on zone changes, loading and title menu.
- Display messages instantly.
- Dash as a toggle.
- Disable diagonal movements.
- Remember the battle speed between battles.
- Save anywhere in the last save slot. (be careful to not softlock yourself)
- Damage, experience, encounter rate, loot rate and money multipliers.
- Speed up the game. (with setting to affect the game timers)
- Edit your saves.
- Disable the speed up of the music when the battle speed change.
- Disable the sound when a message window appears, footstep sounds and startled sound effects.
- Change the background color of some windows.
- Adjust framerate and vsync.

## Installation

- Download BepInEx 6.0.0-pre.2 IL2CPP build from [here](https://github.com/BepInEx/BepInEx/releases/download/v6.0.0-pre.2/BepInEx-Unity.IL2CPP-win-x64-6.0.0-pre.2.zip) and extract the content to the game directory (where "Suikoden I and II HD Remaster.exe" is located). Replace the files if asked.
- Download the mod [here](https://github.com/d3xMachina/Suikoden-Fix/releases/latest) and extract the content to "(GAME_PATH)\BepInEx\plugins\". Replace the files if asked.
- On Steam Deck, add to the Steam launch options : export WINEDLLOVERRIDES="winhttp=n,b"; %command%
- Run the game once to generate the config file, change the config in "(GAME_PATH)\BepInEx\config\d3xMachina.suikoden_fix.cfg" and restart the game.
- To remove the console : open "(GAME_PATH)\BepInEx\config\BepInEx.cfg" and change "Enabled = true" to "Enabled = false" in the "[Logging.Console]" section.

## License

Suikoden Fix is available on Github under the MIT license.
