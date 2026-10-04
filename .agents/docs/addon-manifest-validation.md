# Addon manifest authoring validation

Parse every generated manifest with
`AddonManifestJsonContext.Default.AddonManifestJsonModel`, then call
`Core.All.Helpers.AddonManifestValidator.Validate(manifest, rootFileNames)`.
`DevViewModel.GetAddonJson` uses this shared helper before serialization. Its
`Result` returns the first authoring error without modifying the model or files.

From the repository root, use the read-only console command for a prepared package:

```powershell
dotnet run --project src/Addon.ArtExporter -- validate-addon `
  --input ./prepared-addon

# Validate another manifest at the same package root.
dotnet run --project src/Addon.ArtExporter -- validate-addon `
  --input ./prepared-addon --manifest addon-extra.json
```

Paths are placeholders. The default filename is addon.json. Manifest filenames
must be root JSON filenames. Run the command separately for every addon*.json.
Exit 0 means parsing and authoring checks passed, 1 means JSON/data/filesystem
validation failed, and 2 means invalid command arguments. Nothing is saved,
renamed, installed or launched.

## Shared rules

- User-addon type must be TC, Map or Mod; the game enum must be defined.
- ID, title and release version must be nonblank. The editor also checks its raw
  ID input before applying the game prefix.
- Map packages need a startmap file with a nonblank name or an episode/level slot.
  The editor retains form-specific checks for missing episode/level selections;
  the shared model helper does not add new numeric slot constraints.
- Normal packages cannot use the following case-insensitive root filenames:

```text
duke3d.def rr.def rrra.def nam.def napalm.def ww2gi.def
blood.def sw.def exhumed.def
game.con defs.con user.con nam.con napalm.con ww2gi.con
blood.ini cryptic.ini
```

- Normal Blood packages cannot contain root ART, DAT or RFS files. Convert ART/DAT
  resources to DEF; RFS addons require another supported preparation recipe.
- Blood root music names blood00.ogg through blood09.ogg are reserved. Start custom
  tracks at blood10.ogg.

The package-file restrictions are bypassed for Standalone games, an actual nonblank
custom executable mapping, or a nonblank main/sound RFF declaration, matching the
editor's intended exceptions. Empty executable dictionaries, whitespace-only
paths and whitespace-only RFF declarations do not bypass validation. Required
metadata and map checks still apply. These exceptions do not establish that a
custom executable exists or that its package runs.

The Blood preparation policy selects unpacked native packaging when the addon
contains RFF files; native ART/RFF/DAT may remain. Identify and declare its real
main/sound RFF entry points: the helper examines declarations, and BL extraction
is triggered by these fields rather than scanning for RFF filenames. Without
RFF, prepare DEF-backed ART/DAT resources and report unsupported conversion.
This workflow rule does not change the validator's other documented exceptions.

Checks use root filenames only, matching the original editor's directory scan.
The helper has no filesystem access; callers supply names from the actual final
package. It does not recursively reject nested resources or interpret containers.

## Parsing, authoring and runtime are distinct

The [parser specification](addon.json-Specifications.md) permits some values that
the editor's authoring policy rejects. For example, JSON accepts an empty version
and the Official enum; this helper requires a release version and a user-addon
type. Do not silently invent a release number for an unversioned source: record
the limitation and establish the version policy with the user.

These rules do not replace factory checks, required resource/file-reference checks,
case-sensitive path checks, final ZIP layout validation or game execution. For
example, a Mod with def_main may pass this helper but is rejected by AddonFactory.
No port compatibility checks are performed by this command. It also does not
infer feature requirements: declare TileFromTexture when the generated DEF uses
tilefromtexture.

Run these checks against the final prepared root files. Check referenced INI/DEF
files, maps, music and exact filename case separately; those are packaging checks,
not extra behavior of the shared helper. Keep the results in JSON conversion
evidence, following [the shared preparation plan](addon-conversion-plan.md#shared-instructions-and-conversion-evidence).

## Implementation and tests

- `src/Core.All/Helpers/AddonManifestValidator.cs`: shared, filesystem-free rules.
- `src/Avalonia.Desktop/ViewModels/DevViewModel.cs`: form construction and shared validation.
- `src/Addon.ArtExporter/Program.cs`: parsing and read-only validate-addon command.
- `src/Tests.Unit/AddonManifestValidatorTests.cs`: rule/exception and diagnostic cases.
- `src/Tests.Unit.Sequential/DevViewModelTests.cs`: editor command integration.

Run the affected unit projects and a solution build according to AGENTS.md.
