# BossLiveMapMod Localization

This mod supports localization through the `Lang.ini` file.

## Installation

Place the `Lang.ini` file in the same directory as `BossLiveMapMod.dll`:
```
BepInEx/plugins/BossLiveMapMod/
├── BossLiveMapMod.dll
└── Lang.ini
```

## Supported Languages

The mod uses Unity's `SystemLanguage` enum. Common values:
- `English`
- `ChineseSimplified`
- `ChineseTraditional`
- `Japanese`
- `Korean`
- `Russian`
- `German`
- `French`
- `Spanish`
- etc.

## File Format

```ini
; Comments start with semicolon or hash
[key_name]
English=English Text
ChineseSimplified=中文文本
```

## Available Keys

| Key | Default (English) | Usage |
|-----|------------------|-------|
| `mobs` | Mobs | Checkbox to toggle mob markers |
| `nearby` | Nearby | Checkbox to filter only nearby active mobs |
| `live` | Live | Checkbox to enable real-time position updates |
| `names` | Names | Checkbox to show character names on markers |
| `alpha` | Alpha | Label for transparency slider |

## Example Translations

### Chinese Simplified
```ini
[mobs]
English=Mobs
ChineseSimplified=小怪

[nearby]
English=Nearby
ChineseSimplified=附近

[live]
English=Live
ChineseSimplified=实时

[names]
English=Names
ChineseSimplified=名称

[alpha]
English=Alpha
ChineseSimplified=透明度
```

### Russian
```ini
[mobs]
English=Mobs
Russian=Мобы

[nearby]
English=Nearby
Russian=Рядом

[live]
English=Live
Russian=В реальном времени

[names]
English=Names
Russian=Имена

[alpha]
English=Alpha
Russian=Прозрачность
```

## Adding Your Language

1. Open `Lang.ini` in a text editor
2. Add your language under each section using the correct `SystemLanguage` name
3. Save the file
4. Restart the game

The mod will automatically detect your system language and use the appropriate translations. If your language is not found, it will fallback to English.

## Notes

- If `Lang.ini` is not found, the mod will use default English labels
- You can use `\n` in values for line breaks
- Unknown language tokens will be logged as warnings but won't break the mod