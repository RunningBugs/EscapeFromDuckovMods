Duckov AssetRipper Data Tools

Overview
- This folder contains two Python 3 scripts that work on an AssetRipper ExportedProject to extract item data and fishing special pairs. They use only the Python standard library (no external pip deps).

Prerequisites
- Python 3.8+
- AssetRipper export of the game: a folder that contains an `Assets/` subfolder (e.g., `~/Downloads/AssetRipper_linux_x64/Duckov/ExportedProject`).

1) List all items (IDs and attributes)
- Script: tools/list_items_from_ripper.py
- What it does:
  - Scans all prefabs under `Assets/` to find items (MonoBehaviour blocks with `typeID` and `displayName`).
  - Outputs `items.csv` (tabular) and `items.json` (detailed) with many attributes:
    - IDs and names: `typeID`, `prefabName`, `displayNameKey`, localized `nameEN/zh`, `descEN/zh`
    - Item props: `maxStackCount`, `stackable`, `value`, `quality`, `displayQuality`, `weight`, `order`, `soundKey`, `iconGUID`
    - Presence flags: `inventory`, `usageUtilities`, `slots`, `itemGraphic`
    - Tags: raw GUIDs plus resolved `tagKeys`, `tagsEN`, `tagsZH` (via `.meta` GUID mapping + Localization CSVs)
    - Stats: parsed baseValue stats (e.g., `FishingDifficulty`)
- Usage:
  - python3 tools/list_items_from_ripper.py <ExportedProject> --out_csv items.csv --out_json items.json
  - Example:
    - python3 tools/list_items_from_ripper.py ~/Downloads/AssetRipper_linux_x64/Duckov/ExportedProject --out_csv items.csv --out_json items.json

2) List fish and their specialPairs
- Script: tools/fish_special_pairs.py
- What it does:
  - Finds all fish items (prefab `Fish_*` or display key `Item_Fish_*`).
  - Scans all assets once for `specialPairs` sections and aggregates identical entries.
  - Resolves localization and Only* tag flags: `SunnyOnly`, `DayOnly`, `NightOnly`, `RainOnly`, `StormOnly`.
  - Outputs `fish_special_pairs.csv` with one row per fish/pair (fishes with no special pairs are included with empty bait fields).
- Usage:
  - python3 tools/fish_special_pairs.py <ExportedProject> --out_csv fish_special_pairs.csv
  - Example:
    - python3 tools/fish_special_pairs.py ~/Downloads/AssetRipper_linux_x64/Duckov/ExportedProject --out_csv fish_special_pairs.csv

Notes / Tips
- If a name is blank, the localization key wasn’t found in `Assets/StreamingAssets/Localization/*.csv`. The raw key is still present (`displayNameKey` or `tagKeys`).
- `occurrences` in `fish_special_pairs.csv` tells how many identical pairs were found in the same asset file (multiple spawners configured identically).
- Scripts parse Unity YAML as text; they’re fast and robust for simple fields. For deeper data (e.g., complex nested lists), consider AssetRipper JSON export or UnityPy.

