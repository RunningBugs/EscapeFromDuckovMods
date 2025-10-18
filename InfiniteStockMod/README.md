InfiniteStockMod — Infinite NPC Shop Stocks for Escape From Duckov

Overview
- Keeps NPC shop stock counts effectively infinite while trading, without BepInEx.
- Uses the built‑in Duckov modding API (`Duckov.Modding.ModBehaviour`) and reflection to set shop stock values to a large number whenever shop UIs are active.

Build
- Ensure the `dlls` symlink at repo root points to `Duckov_Data/Managed`.
- Run: `dotnet build InfiniteStockMod/InfiniteStockMod.csproj -c Release`
- Output dll: `InfiniteStockMod/bin/Release/netstandard2.1/InfiniteStockMod.dll`

Publish
- Create a folder named `InfiniteStockMod` with these files:
  - `InfiniteStockMod.dll` (build output)
  - `info.ini` (see `ReleaseExample/InfiniteStockMod/info.ini`)
  - `preview.png` (square image, e.g., 256x256)
- Place the folder under your game install at `Duckov_Data/Mods/`, or upload via Steam Workshop per the official guide.

Notes
- Namespace must match `info.ini` `name` value: `name=InfiniteStockMod` → class `InfiniteStockMod.ModBehaviour`.
- The mod uses reflection to find shop UI/logic types (`StockShopView`, `BlackMarketView`, etc.). If a future update renames these, open an issue or adjust `ModBehaviour` to include the new names.
- No BepInEx required; the mod is fully compatible with the Duckov mod loader and Steam Workshop.

