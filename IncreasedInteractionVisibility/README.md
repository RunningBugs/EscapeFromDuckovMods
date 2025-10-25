# IncreasedInteractionVisibility Mod

A mod for Escape from Duckov that increases the visibility distance of interaction markers/dots, making it easier to see interactable objects from further away without changing the actual interaction range.

## Features

- **Increased Marker Visibility**: Interaction markers/dots are visible from up to 50m away (increased from default ~10-15m)
- **LOD Adjustments**: Modifies Level of Detail (LOD) settings to keep markers visible at greater distances
- **Camera Far Clip Adjustment**: Extends camera rendering distance to support increased visibility
- **No Interaction Range Changes**: The actual interaction distance remains unchanged - you still need to be close to interact
- **Automatic Detection**: Continuously scans for and updates all interaction markers in the scene

## Installation

1. Copy the contents of `ReleaseExample/IncreasedInteractionVisibility/` to your game's mod folder:
   - `IncreasedInteractionVisibility.dll`
   - `info.ini`

2. The mod folder location is typically:
   ```
   <Game Installation>/Duckov_Data/StreamingAssets/Mods/IncreasedInteractionVisibility/
   ```

3. Launch the game and the mod will be loaded automatically

## How It Works

The mod scans for `InteractMarker` components in the scene and applies the following modifications:

1. **Renderer Culling**: Disables occlusion culling on marker renderers to prevent premature hiding
2. **LOD Groups**: Increases LOD transition thresholds by 2x, making markers stay visible longer
3. **Canvas Distance**: For world-space canvases, increases the plane distance limit
4. **Camera Settings**: Extends the main camera's far clip plane for better long-distance rendering

The mod runs periodic updates (every 0.5 seconds) to catch newly spawned markers.

## Technical Details

- **Target Framework**: .NET Standard 2.1
- **Dependencies**: Unity Engine, TeamSoda.Duckov assemblies
- **Method**: Runtime component scanning and modification
- **Performance**: Minimal impact, updates run every 0.5 seconds
- **Update Interval**: 0.5 seconds (configurable in code)

## Configuration

To adjust the visibility distance or update frequency:

1. Edit `ModBehaviour.cs`
2. Modify these constants:
   ```csharp
   private const float INCREASED_CULLING_DISTANCE = 50f;  // Marker visibility distance
   private const float UPDATE_INTERVAL = 0.5f;  // How often to check for new markers
   ```
3. Rebuild the mod using `dotnet build -c Release`

## Building from Source

```bash
cd game-source/IncreasedInteractionVisibility
dotnet build -c Release
```

The compiled DLL will be placed in `ReleaseExample/IncreasedInteractionVisibility/`

## Compatibility

- Game Version: Escape from Duckov (current version)
- Other Mods: Should be compatible with most other mods
- Performance Impact: Minimal - only scans markers every 0.5 seconds

## Troubleshooting

**Markers still not visible from far away:**
- Some markers may use different rendering systems
- Try increasing `INCREASED_CULLING_DISTANCE` to a higher value (e.g., 100f)
- Check the game console/logs for "IncreasedInteractionVisibility" messages

**Performance issues:**
- Increase `UPDATE_INTERVAL` to reduce scanning frequency (e.g., 1.0f for once per second)
- The mod only processes each marker once, so performance impact should be minimal

## License

See the main repository LICENSE file.

## Author

lisanhu

## Version

1.0.0