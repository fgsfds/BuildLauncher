# Addon manifest authoring validation

The read-only `validate-addon` command parses a manifest with
`AddonManifestJsonContext.Default.AddonManifestJsonModel` and runs
[`AddonManifestValidator`](../../src/Core.All/Helpers/AddonManifestValidator.cs).
The [developer editor](../../src/Avalonia.Desktop/ViewModels/DevViewModel.cs)
uses the same helper when generating a manifest from the form. Start the desktop
app with `--dev` to use the editor.

From the repository root, run the command once per manifest against the final
prepared package directory.

```powershell
dotnet run --project src/Addon.ArtExporter -- validate-addon --input ./prepared-addon
dotnet run --project src/Addon.ArtExporter -- validate-addon --input ./prepared-addon --manifest addon-extra.json
```

The default manifest is `addon.json`. Use a root filename ending in `.json`
for an alternate manifest. Exit 0 means the manifest passed parsing and
authoring checks, 1 means JSON, validation or filesystem failure, and 2 means
invalid arguments. An unrecognized game name produces a diagnostic and exit
code 1. The command leaves package files unchanged.

## Authoring rules

- Supply TC, Map or Mod, a defined game, and a nonblank title, ID and release
  version. The editor also checks its raw ID field before applying the game prefix.
- For a map, supply a nonblank start-map filename or an episode/level slot.
  Use the file form for prepared map packages because the current launch
  builder rejects slots. Confirm that the named file resolves in the port.
- Ordinary packages must avoid these case-insensitive root filenames.

```text
duke3d.def rr.def rrra.def nam.def napalm.def ww2gi.def
blood.def sw.def exhumed.def
game.con defs.con user.con nam.con napalm.con ww2gi.con
blood.ini cryptic.ini
```

- Ordinary Blood packages cannot contain root ART, DAT or RFS files. Follow
  [the Blood profile](games/blood.md) for preparation. Custom Blood OGG tracks
  start at `blood10.ogg` because the validator reserves `blood00.ogg` through
  `blood09.ogg` in this profile.

The validator skips root-file restrictions for Standalone, a nonblank custom
executable mapping, or a nonblank `rff_main` or `rff_sound` declaration.
Blank values and empty executable dictionaries leave the restrictions in force.
Metadata and map checks still apply. The helper only inspects filenames supplied
by the caller, and the CLI supplies files from the package root.

## Final package checks

Verify factory constraints, referenced files and includes, exact path casing
and the final ZIP or directory layout. For example, the parser accepts the
Official enum and an empty version string. Authoring requires a user-addon type
and a nonblank release version. The factory rejects `def_main` on an autoload
mod. Declare `TileFromTexture` when the active DEF uses `tilefromtexture`.
Record authoring and package checks in the JSON conversion evidence. Verify
port compatibility and game execution when those checks are in scope.
