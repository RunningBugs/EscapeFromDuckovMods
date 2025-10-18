MyDuckovMod template aligned with the official Escape From Duckov modding guide.

Build steps
- Set `DuckovPath` in `MyDuckovMod.csproj` to your local install folder that contains `Duckov.exe` (e.g., `/Data/SteamLibrary/steamapps/common/Escape from Duckov`).
- Build the project with `dotnet build MyDuckovMod/MyDuckovMod.csproj -c Release`.
- Publish your mod folder as:
  - `MyDuckovMod/` (folder)
    - `MyDuckovMod.dll` (from `bin/Release/netstandard2.1/`)
    - `info.ini` (see `ReleaseExample/MyDuckovMod/info.ini`)
    - `preview.png` (add a square image, e.g., 256x256)
- Place the folder under `Duckov_Data/Mods/` or upload via Steam Workshop per the official docs.

Notes
- The project references the game’s assemblies from your install via `DuckovPath`; nothing is copied into this repo.
- Namespace must match `info.ini` `name`, and `ModBehaviour` class must be present under that namespace (e.g., `MyDuckovMod.ModBehaviour`).

