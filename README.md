# Vanguard Galaxy Hangar (VGHangar)

A BepInEx plugin for [Vanguard Galaxy](https://store.steampowered.com/app/3471800/) that overhauls the hangar UI. Replaces the default arrow-based ship carousel with a scrollable, filterable list — making fleets of dozens of ships actually browsable.

- **Scrollable ship list** — every ship in the hangar shown at once, sorted by name, with class, size, and average turret/module level on each row.
- **Role filter chips** — toggle Combat / Mining / Salvaging / Cargo with role-coloured icons. All roles active by default.
- **Size filter chips** — toggle hull sizes (1, 2, 3 …) independently. Drones are always shown.
- **Click-to-select** — picking a ship in the list drives the underlying carousel, so the game's existing ship-detail panel still loads as before. The vanilla Previous/Next arrow buttons are hidden because the list replaces them.

The plugin is purely additive UI — no game logic, no save data, no balance changes. Disabling the plugin (or removing the DLL) restores the vanilla carousel exactly.

## Install

1. **Install BepInEx 5.x** — grab `BepInEx_win_x64_5.4.x.zip` from the [BepInEx releases](https://github.com/BepInEx/BepInEx/releases) and unzip it into your Vanguard Galaxy install folder (next to `VanguardGalaxy.exe`).
2. **Launch the game once** so BepInEx creates its `BepInEx/plugins/` and `BepInEx/config/` subfolders, then close the game.
3. **Download the VGHangar release** zip from [Releases](https://github.com/fank/vanguard-galaxy-hangar/releases) (or Nexus Mods, once published).
4. **Unzip** into `BepInEx/plugins/`. The zip contains a single `VGHangar/` folder that drops in cleanly:
   ```
   VanguardGalaxy/BepInEx/plugins/
     VGHangar/
       VGHangar.dll
       README.md
   ```
5. **Launch the game.** Open the BepInEx console — you should see a load line ending with the number of Harmony patches applied, e.g.:
   ```
   [Info :Vanguard Galaxy Hangar] Vanguard Galaxy Hangar v0.1.0 loaded (1 patches)
   ```

## Uninstall

Delete the `BepInEx/plugins/VGHangar/` folder. The plugin holds no config and no per-save state, so nothing else needs cleanup.

## Troubleshooting

**No load line in the BepInEx console**
- Check that `BepInEx/plugins/VGHangar/VGHangar.dll` exists.
- Enable the console: `BepInEx/config/BepInEx.cfg` → `[Logging.Console]` → `Enabled = true`.

**Hangar opens but the list doesn't show**
- The plugin patches `PersonalHangar.ShowShips`. If a game update renames or removes that method, the load line will still appear but the list won't render. Check the BepInEx console for `VGHangar:` errors and open an issue with the log.

**The arrow buttons came back / list looks broken after a game update**
- The publicized stubs in `VGHangar/lib/` are pinned to a specific game version. After a game update the type signatures may drift; rebuild against the new game DLLs (see Build below) and ship a new release.

## Build

The repo commits **publicized stubs** of the three game-specific assemblies it references — `Assembly-CSharp.dll`, `UnityEngine.UI.dll`, `Unity.TextMeshPro.dll` — at `VGHangar/lib/`. These are method-signature-only stubs (every IL body replaced with `throw null;` by `assembly-publicizer --strip`), legal to redistribute, and enough to compile against. The real runtime takes over in-game.

The remaining references — BepInEx, HarmonyX, and the Unity engine modules — come from NuGet (see `VGHangar/VGHangar.csproj`).

```bash
# Build the DLL
make build      # or: dotnet build VGHangar/VGHangar.csproj -c Debug

# Build + copy into the game's BepInEx/plugins/ folder (WSL/Steam path; edit Makefile if yours differs)
make deploy
```

To regenerate the stubs after a game update, install [`assembly-publicizer`](https://github.com/CabbageCrow/AssemblyPublicizer) and run:

```bash
assembly-publicizer --strip <game>/VanguardGalaxy_Data/Managed/Assembly-CSharp.dll  -o VGHangar/lib/Assembly-CSharp.dll
assembly-publicizer --strip <game>/VanguardGalaxy_Data/Managed/UnityEngine.UI.dll   -o VGHangar/lib/UnityEngine.UI.dll
assembly-publicizer --strip <game>/VanguardGalaxy_Data/Managed/Unity.TextMeshPro.dll -o VGHangar/lib/Unity.TextMeshPro.dll
```

`--strip` is required — without it, the committed DLLs would carry the proprietary IL bodies, which can't be redistributed.

## License

MIT — see [LICENSE](LICENSE).
