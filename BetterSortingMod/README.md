# BetterSortingMod

Adds advanced inventory sorting controls to Duckov. Requires running Krafs.Publicizer on the game assemblies so private inventory UI fields become accessible.

## Setup

1. Copy the game's managed assemblies into `dlls/` (see other sample mods for the expected layout).
2. Build the publicized references once:
   ```bash
   dotnet build -c Release BetterSortingMod/BetterSortingMod.csproj
   ```
   Krafs.Publicizer generates the required `*.publicized.dll` files automatically.
3. Build the mod (Debug or Release):
   ```bash
   dotnet build -c Release BetterSortingMod/BetterSortingMod.csproj
   ```
4. Copy `BetterSortingMod/bin/Release/BetterSortingMod.dll` (and optional PDB) to the game's `Mods` folder. A copy also lands in `BetterSortingMod/ReleaseExample/BetterSortingMod/`.

## Gameplay

- The inventory "整理" button is split into a metric selector and an action button.
- Choose a metric (weight, value, value per weight, stack value) to run a one-off sorted arrangement that skips locked slots.
- Use the right-hand button for vanilla sorting at any time.
