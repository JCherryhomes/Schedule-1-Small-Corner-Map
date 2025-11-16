# Small Corner Map

Adds a compact, always-visible corner minimap for **Schedule I** with:

- Player position + facing indicator
- Contract delivery location markers (auto add/remove when tracked)
- Property points of interest markers
- Adjustable scale (preferences file)
- In-map time display
- Supports both Mono and IL2CPP builds

## Source / Repository
GitHub: https://github.com/JCherryhomes/Schedule-1-Small-Corner-Map

## Installation (Manual)
1. Install MelonLoader 0.7.0 for Schedule I.
2. Download the release zip (from Thunderstore or GitHub).
3. Extract the contents so that the DLLs end up in `Mods/` inside your game directory.
4. Launch the game; the minimap initializes upon entering the `Main` scene.

## Thunderstore Package Layout
```
SmallCornerMap/
  manifest.json
  icon.png              (provide a 256x256 PNG)
  README.md
  LICENSE.txt
  Mods/
    Small_Corner_Map.Mono.dll      (Mono build)
    Small_Corner_Map.Il2cpp.dll    (IL2CPP build)
```
Both DLLs are shipped so users on either backend can use the mod.

## Building
Use the Visual Studio / Rider configurations:
- `Mono` (netstandard2.1) -> outputs `Small_Corner_Map.Mono.dll`
- `IL2CPP` (net6.0) -> outputs `Small_Corner_Map.Il2cpp.dll`

After building, run the provided `Pack.ps1` script to create the Thunderstore zip.

### Development Setup
The project references DLLs from your Schedule I game installation. To configure your local paths:

1. Copy `Local.props.example` to `Local.props` in the project root
2. Edit `Local.props` and set the full paths to your game installations:
   
   **Example (both in same Steam library):**
   ```xml
   <MonoGamePath>D:\SteamLibrary\steamapps\common\Schedule I</MonoGamePath>
   <IL2CPPGamePath>D:\SteamLibrary\steamapps\common\Schedule I - IL2CPP</IL2CPPGamePath>
   ```
   
   **Example (different Steam libraries):**
   ```xml
   <MonoGamePath>C:\Program Files (x86)\Steam\steamapps\common\Schedule I</MonoGamePath>
   <IL2CPPGamePath>E:\Games\Steam\steamapps\common\Schedule I - IL2CPP</IL2CPPGamePath>
   ```

3. The project will automatically use these paths for all game DLL references and PostBuild deployment
4. `Local.props` is git-ignored, so each developer can have their own configuration

**Note:** You must have both the Mono and IL2CPP versions of Schedule I installed to build both configurations.

## Preferences
A preferences file is created automatically (MapPreferences) allowing scale adjustment and future options.

## Versioning
Current version: 2.1.0. AssemblyVersion/FileVersion kept in sync with `Constants.ModVersion`.

## Dependencies
- MelonLoader 0.7.0
(If required later, add S1API dependency by its Thunderstore identifier once published.)

## Changelog
See `CHANGELOG.md` (create and maintain for future releases).

## License
MIT. See `LICENSE.txt`.

## Contributing
Issues and PRs welcome. Please describe reproduction steps for bugs and include logs if possible.

## Notes
- Keep icon simple, high contrast.
- If Thunderstore migrates to `thunderstore.toml`, mirror data from `manifest.json` there.

## Visuals
- Circular minimap now has a thin dark feathered border for better contrast.
- Edge smoothing implemented via higher resolution + feathered alpha (no external assets).