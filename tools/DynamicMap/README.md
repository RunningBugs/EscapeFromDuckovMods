# Dynamic Map Toolkit

Static site generator for Duckov raid maps with points of interest extracted straight from the AssetRipper dump.

## Overview
- `extract_map_data.py` scans a Duckov AssetRipper export for minimap sprites (`MiniMapSettings`) and static `SimplePointOfInterest` markers, converts them to JSON, and copies the required textures.
- `site/` contains a no-build HTML/CSS/JS viewer that loads the generated JSON at runtime. The output is GitHub Pages–friendly; just push `site/` to a `gh-pages` branch (or serve from `main` via the `docs/` convention).

## Prerequisites
- Python 3.8+ (standard library only).
- AssetRipper export directory (e.g. `~/Downloads/AssetRipper_linux_x64/Duckov/new/ExportedProject`).

## Usage
```bash
python3 tools/DynamicMap/extract_map_data.py \
  ~/Downloads/AssetRipper_linux_x64/Duckov/new/ExportedProject \
  --out tools/DynamicMap/site
```

What happens:
1. **Data extraction** – `site/data/maps.json` is created with:
   - map metadata (scene IDs, world→pixel scale, sprite info);
   - static POIs with world coordinates, localization (English if available), and icon metadata.
2. **Asset copy** – all referenced minimap PNGs (and POI icon sprites when available) are mirrored under `site/assets/`.

> Tip: rerun the script whenever the game is updated; the script wipes previously generated files before recreating them.

Optional flags:
- `--rotation-cw` (default `45`) controls the clockwise rotation applied to translate world coordinates into minimap space. Adjust if a future patch changes the in-game minimap orientation.

## Publishing
- Commit the generated `site/` assets or copy them to a dedicated branch.
- Enable GitHub Pages (e.g. branch `main`, folder `/site`) and the viewer will be live at `https://<user>.github.io/<repo>/site/`.

## Limitations / Future Ideas
- World→map projection currently ignores parent rotation in the scene hierarchy (most minimap POIs sit under identity transforms; adjust script if you find counterexamples).
- Only `SimplePointOfInterest` markers are exported. Add more parsers (e.g. quest beacons) by extending `collect_scene_markers`.
