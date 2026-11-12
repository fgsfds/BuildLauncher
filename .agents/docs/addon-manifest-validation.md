# Addon manifest authoring validation

The current `master` checkout has authoring checks in
[DevViewModel.GetAddonJson](../../src/Avalonia.Desktop/ViewModels/DevViewModel.cs).
Run BuildLauncher with `--dev` to use its addon editor:

```powershell
dotnet run --project src/Avalonia.Desktop -- --dev
```

For a prepared package, deserialize each root `addon*.json` with
`JsonSerializer.Deserialize` and
`AddonManifestJsonContext.Default.AddonManifestJsonModel` in a .NET caller
referencing `src/Core.All`. Reject a null result. Apply the editor checklist
below to each manifest and its actual root files. The editor validates its form
when generating a manifest, so it cannot substitute for checking every existing
manifest in a multi-manifest archive.

## Editor checklist

- Select TC, Map or Mod and a game. Supply a nonblank title, ID and release
  version. The editor checks its raw ID field before applying the game prefix.
- For a map, select either a file with a nonblank filename or an episode and
  level. Use the file form for prepared map packages because the current launch
  builder rejects slots. Confirm that the named file resolves in the port.
- For ordinary packages, avoid these reserved root filenames. Matching in the
  editor is case-insensitive.

```text
duke3d.def rr.def rrra.def nam.def napalm.def ww2gi.def
blood.def sw.def exhumed.def
game.con defs.con user.con nam.con napalm.con ww2gi.con
blood.ini cryptic.ini
```

- Ordinary Blood packages may not contain root ART, DAT or RFS files. Follow
  [the Blood profile](games/blood.md) for preparation. Custom Blood OGG tracks
  start at `blood10.ogg` because the editor reserves `blood00.ogg` through
  `blood09.ogg` in this profile.

The root-file checks are skipped for Standalone or when a custom executable or
main/sound RFF form field is non-null. Supply only real, nonblank paths when
using those exceptions. The checks inspect top-level filenames and do not read
nested containers.

## Final package checks

Verify factory constraints, referenced files and includes, exact path casing,
and the final ZIP or directory layout separately. For example, the parser
accepts the Official enum and empty version strings, while the editor requires
a user-addon type and a nonblank release version. The factory also rejects
`def_main` on an autoload mod. Declare `TileFromTexture` when the active DEF
uses `tilefromtexture`. Record results in the package's JSON conversion evidence.

## Validator in the backup checkout

The shared `AddonManifestValidator` and read-only `validate-addon` command are
available in local commit `4a79ece7920beea555dcb1c78601adbdf6420bd4`
(available via `codex/backup-before-committer-rewrite`). Run from a checkout
containing that implementation:

```powershell
dotnet run --project src/Addon.ArtExporter -- validate-addon --input ./prepared-addon
dotnet run --project src/Addon.ArtExporter -- validate-addon --input ./prepared-addon --manifest addon-extra.json
```

The command defaults to `addon.json`. Run it once per root manifest. Exit 0
means parsing and authoring checks passed, 1 means validation failed, and 2
means invalid arguments. This version requires nonblank executable/RFF
declarations before they can bypass root-file checks. It has no install or
launch side effects. Use the current checkout's parser, editor and factory as
the authority for its own behavior.
