# Manual Save Mod

A mod for "Escape from Duckov" that adds a manual save button to the pause menu.

## Features

- Adds a "Save Game" button at the top of the pause menu (ESC)
- Triggers the same autosave functionality used by the game
- Shows visual feedback when saving ("Saving..." → "Saved!")
- Multi-language support (English, Chinese, Japanese, Korean, Russian, Spanish, French, German, Portuguese)
- Works anywhere in the game (not limited to base level)

## Installation

1. Copy `ManualSaveMod.dll`, `0Harmony.dll`, and `Lang.ini` to your game's mod folder
2. Typical mod folder location: `Escape from Duckov/Duckov_Data/StreamingAssets/Mods/ManualSaveMod/`
3. Launch the game and press ESC to see the new "Save Game" button

## Usage

1. Press ESC to open the pause menu
2. Click the "Save Game" button at the top of the menu
3. Wait for the "Saved!" confirmation (about 1-2 seconds)
4. Your game is now saved!

## Customization

You can customize the button text by editing `Lang.ini`:

- The file uses INI format with sections for each text key
- Each section contains translations for different languages
- Supported keys: `save_game`, `saving`, `saved`, `error`
- Language codes follow Unity's SystemLanguage enum (e.g., `ChineseSimplified`, `Japanese`, `English`)

Example:
```ini
[save_game]
English=Save Game
ChineseSimplified=保存游戏
```

## How It Works

When you click the "Save Game" button, the mod:
1. Calls `SavesSystem.CollectSaveData()` - notifies all game systems to prepare their save data
2. Calls `SavesSystem.SaveFile()` - writes the save file to disk and creates a backup
3. Shows visual feedback on the button

This is the exact same save flow used by the game's automatic save system.

## Technical Details

- Uses Harmony patching to inject the button into the pause menu
- Patches `PauseMenu.Open()` to add the button dynamically
- Button is cloned from existing pause menu buttons to match the UI style
- Uses async/await pattern for smooth save operations without blocking the UI

## Compatibility

- Built for .NET Standard 2.1
- Requires Lib.Harmony 2.2.2 (included)
- Should be compatible with other mods

## Building from Source

```bash
dotnet build ManualSaveMod/ManualSaveMod.csproj -c Release
```

The compiled mod will be in `ManualSaveMod/ReleaseExample/ManualSaveMod/`

## License

See project root LICENSE file.