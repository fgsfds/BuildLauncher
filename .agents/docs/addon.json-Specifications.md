# BuildLauncher addon.json specification

This page documents the manifest accepted by BuildLauncher, including the differences between JSON parsing and actual addon loading/launching. It extends the [original addon descriptor](https://lerppu.net/db64/#Addon-Descriptor-Specification-v1-0).

**Documentation revision: 1.0j. Parser and runtime contract reviewed 2026-10-04 against [BuildLauncher commit 3ecec2a](https://github.com/fgsfds/BuildLauncher/tree/3ecec2a5fb6374d76427f1999fd9a2881c2b7c9e).** This is a documentation revision, not a new JSON field or a change to the application. There is no manifest schema-version token. The top-level `version` is the addon's release version.

## JSON rules

- One manifest is one JSON object. A package may contain several separate manifests; do not put an array in `addon.json`.
- Property names are case-sensitive: use `id`, `game`, `con_main`, etc. `Title` is not an alias for `title`.
- Enum values are case-insensitive: `tc` and `TC`, or `duke3d` and `Duke3D`, are equivalent. This does not make filesystem paths or all strings case-insensitive.
- Unknown properties are rejected by the main manifest context and the nested models it handles. The custom `startmap` converter uses separate contexts that currently ignore extra map-object properties; do not rely on that exception. Do not add `schema_version`, `$schema`, `ports`, or conversion notes to the manifest. Keep conversion notes in a separate file.
- Trailing commas are accepted by the current serializer. Comments are not. Generate standard JSON without either for portability.
- The five required properties are `id`, `type`, `game`, `title`, and `version`. Required strings and the game object must not be null. Optional properties may be omitted; use omission when no value is needed.
- A syntactically valid manifest does not prove that its files exist, its scripts work, or the selected source port supports the addon.

The source of truth for names and types is [AddonManifestJsonModel and its serializer context](https://github.com/fgsfds/BuildLauncher/blob/3ecec2a5fb6374d76427f1999fd9a2881c2b7c9e/src/Core.All/Serializable/Addon/AddonManifestJsonModel.cs).

## Authoring validation

The current checkout additionally shares authoring rules between
`DevViewModel.GetAddonJson` and the resource CLI through
[AddonManifestValidator](../../src/Core.All/Helpers/AddonManifestValidator.cs).
These rules are stricter than deserialization: require nonblank ID, title and
release version, a user-addon type, and a start map for map packages; ordinary
packages must avoid reserved root filenames and unsupported Blood ART/DAT/RFS.
Standalone/custom-executable/RFF exceptions affect package-file restrictions only.
They do not establish runtime support.

Run from the repository root for every generated manifest:

```powershell
dotnet run --project src/Addon.ArtExporter -- validate-addon --input ./prepared-addon
```

See [the complete authoring rules](addon-manifest-validation.md) for exceptions,
additional-manifest validation and exit codes. These additions describe the current
checkout rather than the older parser/runtime review commit above. Required-file
checks, factory constraints and game execution remain separate.

## Required fields

### `id` - addon identity

**Required, string.** Stable identity used by dependencies, incompatibilities, settings, and the catalog. `id` plus `version` identifies a release; changing the display title does not change the ID. ID comparisons in `AddonId` are case-insensitive.

For new packages, prefer a game prefix and a short stable name, such as `duke3d-example-campaign`. ASCII letters, numbers, `-`, `_`, and `+` are a naming convention, not an enforced character whitelist in the current parser. Keep the same ID for later releases of the same addon. Avoid empty IDs and collisions with official addon IDs.

### `type` - addon type

**Required, string enum.** Use one of these values for user packages:

| Value | Intended use | Packaging/launch implication |
| --- | --- | --- |
| `tc` | A selected campaign or total conversion | May replace main scripts/resources; loaded as the selected campaign. |
| `map` | A selected user map, optionally with resources | Supply `startmap.file`; package is loaded and the map path is passed to the port. |
| `mod` | An optional autoload modification | Multiple mods may be enabled; use additional scripts/resources rather than main replacements. Package as ZIP. |

`Official` also exists in the enum and parses, but normal drag-and-drop import routes only `TC`, `Map`, and `Mod`. Official campaigns are supplied by the application; do not use `official` for a converted user addon.

`con_main` does **not** automatically change the type. A `mod` with non-null `def_main` is rejected by `AddonFactory`, even if JSON parsing succeeds. For autoload mods, main CON, RTS, Blood INI/RFF files are not copied into the resulting mod object, and `executables` and `options` are discarded. Use `con_modules` and `def_modules` for supported additional scripts.

### `game` - supported game

**Required, object.** The object has these properties:

| Property | Type | Required | Current behavior |
| --- | --- | --- | --- |
| `name` | string enum | Yes | One game identifier from the list below. |
| `version` | string | No | Game-edition constraint; distinct from the top-level addon version. |
| `crc` | string | No | Stored CRC metadata. Integers and arrays are rejected; current compatibility checks do not enforce this CRC. |

Canonical `name` values from [GameEnum](https://github.com/fgsfds/BuildLauncher/blob/3ecec2a5fb6374d76427f1999fd9a2881c2b7c9e/src/Core.All/Enums/GameEnum.cs):

```text
Duke3D, Duke64, Blood, Wang, Fury, Slave, NAM, WW2GI,
Redneck, RidesAgain, TekWar, Witchaven, Witchaven2, Standalone, DukeZeroHour
```

Legacy names `ShadowWarrior` and `Exhumed` are accepted and mapped to `Wang` and `Slave`. Use canonical names in new manifests. `all` and `paladins` are not defined game names. A bare value such as `"game": "Duke3D"` is not accepted by the manifest context; always use an object.

Enum recognition does not imply complete campaign support. The current campaign factory handles `Duke3D`, `Fury`, `Redneck`, `NAM`, `WW2GI`, `Wang`, `Slave`, `Blood`, and `Standalone`. Other enum members exist for application/game support but do not have general user-campaign construction in that factory. Do not infer addon support from the enum alone.

For Duke3D, established edition strings are `Duke3D_13D`, `Duke3D_Atomic`, and `Duke3D_WT`. The parser accepts other strings, but the port compatibility check uses `SupportedGamesVersions`, and the autoload validator compares a mod's game version to the selected campaign's version. Omit the constraint unless it is needed and supported by the relevant port. Previously listed `blood_10`, `blood_111`, `blood_121`, `fury_10`, `fury_20`, and `fury_as` are not a defined game-version enum in this implementation.

If supplying CRC metadata, use a string such as `"0x982AFE4A"`. There is no current CRC dependency-chain search or CRC-based exclusion of games. Do not rely on `crc` to select the correct edition or validate downloaded content.

### `title` - display title

**Required, string.** The title shown by BuildLauncher. There is no fallback to the folder or package name when the property is missing from a manifest: parsing fails. Supply a nonempty title.

### `version` - addon release version

**Required, string.** The property must be present, unlike the optional `version` in a dependency entry. The parser accepts an empty string; use a meaningful source release version when one exists, and document an unversioned source separately instead of inventing its release number.

Prefer numeric components separated by dots, with an optional suffix after `-`, for example `1.0`, `2.0.0`, or `3.14-RC2`. The current [VersionComparer](https://github.com/fgsfds/BuildLauncher/blob/3ecec2a5fb6374d76427f1999fd9a2881c2b7c9e/src/Core.All/Helpers/VersionComparer.cs) compares numeric segments, treats omitted trailing numeric segments as zero (`1.0` equals `1.0.0`), and compares suffix text ordinally. A release without a suffix sorts after one with the same numeric version and a suffix. It also accepts nonnumeric segments; this is not strict SemVer validation. Do not assume all version/suffix comparisons ignore case.

## Optional metadata

| Property | JSON type | Meaning |
| --- | --- | --- |
| `author` | string | Author and contributor credits. |
| `description` | string | Description displayed by the Markdown viewer in BuildLauncher. Use `\n` for line breaks; Markdown and links may be included. Build palette codes such as `^12` are not a documented BL formatting mechanism. |
| `release_date` | string | ISO date `YYYY-MM-DD`, deserialized as `DateOnly`, for example `2026-10-04`. No time component. |

There are no manifest fields for thumbnail paths. Optional `grid.*` and `preview.*` image files are discovered at the package/folder root; use an image format the bitmap loader can decode, such as PNG.

## Resource and script fields

All these fields are optional. File paths describe the contents of the addon package/folder. Use relative paths, `/` separators, and the actual spelling and case of the shipped files. For converted packages, avoid absolute paths and `..` escapes. These are packaging requirements for portable output, not claims that the parser validates containment or checks file existence.

| Property | JSON type | Purpose |
| --- | --- | --- |
| `con_main` | string | Main CON replacement for Duke-style campaigns (`Duke3D`, `Fury`, `Redneck`, `NAM`, `WW2GI`). |
| `con_modules` | array of strings | Additional CON modules, in declared order, where the port supports them. |
| `def_main` | string | Main DEF replacement. Invalid for an autoload `mod`. |
| `def_modules` | array of strings | Additional DEF modules, in declared order. Use this for autoload DEF scripts. |
| `rts` | string | RTS resource for a Duke-style campaign. Actual command-line handling depends on the port. |
| `ini` | string | Blood episode/campaign INI replacement. |
| `rff_main` | string | Blood main RFF replacement. Causes a ZIP package to be unpacked. |
| `rff_sound` | string | Blood sound RFF replacement. Causes a ZIP package to be unpacked. |

These are file declarations, not conversion instructions. BuildLauncher does not unpack a nested GRP or RFF and translate its contents merely because a path appears here. Preserve a game/port-supported archive or extract/convert the needed resources during addon preparation, after inspecting the original launch instructions and testing the target port. There is no manifest `grp` field or general per-file conversion recipe.

Ports may skip unsupported CON/DEF switches or reject an operation. Declaring a script does not add support for its syntax to a port. Feature requirements belong in `dependencies.features`.

## `dependencies` - required addons and engine features

**Optional, object.** May contain `addons`, `features`, or both:

| Property | Type | Meaning |
| --- | --- | --- |
| `addons` | array of objects | Each entry requires string `id`; optional string `version` expresses the dependency version. |
| `features` | array of string enums | Engine capabilities required from the selected port. |

A dependency version can start with `>=`, `<=`, `==`, `>`, or `<`; no prefix means equality. Omission means no version constraint. These are interpreted by `VersionComparer`, not by the JSON parser. Keep comparison operators in dependency versions, not the addon's own version.

Dependencies do not automatically download other packages. Autoload compatibility checks use the selected campaign and installed mods; some official dependencies have dedicated game/port launch handling. Do not assume a general campaign dependency resolver will mount an arbitrary dependent TC.

### Official addon IDs

IDs are case-insensitive. The application supplies these expansions when installed:

| ID | Expansion |
| --- | --- |
| `dukevaca` | Duke Caribbean: Life's a Beach |
| `dukenw` | Duke: Nuclear Winter |
| `dukedc` | Duke it out in D.C. |
| `bloodcp` | Blood: Cryptic Passage |
| `wanton` | Shadow Warrior: Wanton Destruction |
| `twindragon` | Shadow Warrior: Twin Dragon |
| `route66` | Redneck Rampage: Route 66 |
| `platoon` | WWII GI: Platoon Leader |

### Supported feature names

The complete manifest feature enum is:

```text
EDuke32_CON, Hightile, Models, Sloped_Sprites, TROR,
Wall_Rotate_Cstat, Dynamic_Lighting, Modern_Types, SndInfo, TileFromTexture
```

Names are case-insensitive. There is no comma in the `Dynamic_Lighting` value. Required features filter usable ports and autoload mods; different ports advertise different capabilities. JSON recognition alone does not prove that a port fully implements a feature needed by a particular addon.

## `incompatibles` - incompatible addons

**Optional, object.** Use an `addons` array with the same `{ "id": "...", "version": "..." }` entries as dependencies. Omit `version` to describe incompatibility with all versions.

The model also accepts a `features` array here because it shares the dependency model, but the factory does not use incompatible features. Do not emit it.

The autoload validator considers incompatibilities between a selected campaign and mods. A campaign can list `{ "id": "*" }` to exclude every autoload mod. This is a specific wildcard check, not general glob matching. Incompatibilities should not be documented as a universal resolver for every possible pair of addon types.

## `startmap` - map to start

**Optional at JSON level, object.** For a runnable `map` package, supply a file form, for example `"startmap": { "file": "maps/example.map" }`.

The model/converter also accepts the slot form `"startmap": { "volume": 0, "level": 3 }`, with both integers required. However, the current `AppendMapArgs` implementation only handles `MapFileJsonModel` and rejects slot maps. Do not emit the slot form for converted map packages until launch support is implemented. No supported slot numbering convention can be promised from the current launcher.

`startmap` on a `tc` does not currently cause the generic campaign launch path to auto-start that map. Use a campaign's scripts/INI for its normal episode structure. Do not describe this field as a launcher reboot or an in-game skill-selection workflow; BL constructs external process arguments.

## `executables` - bundled executables by OS and port

**Optional, object.** Preferred structure: OS name maps to an object whose keys are port names and values are executable paths relative to the addon folder. A package can declare one or several supported ports per OS; both OS keys are not mandatory at parsing time.

OS keys: `Windows`, `Linux`.

Port keys from `PortEnum`:

```text
EDuke32, VoidSW, Raze, NBlood, Rednukem, PCExhumed,
NotBlood, BuildGDX, Fury, Stub, DosBox, ZeroRecomp
```

An executable is run with the argument builder of the declared port. Use only keys whose command-line behavior matches the bundled executable. Recognition of a key does not guarantee that this game/addon can launch with that port.

The legacy form `"executables": { "Windows": "eduke32.exe", "Linux": "eduke32" }` still parses. Its values are mapped by case-insensitive filename prefix: `nblood` -> `NBlood`, `notblood` -> `NotBlood`, `eduke` -> `EDuke32`, `raze` -> `Raze`; everything else -> `Stub`. Prefix detection uses the complete value, so a path beginning with `bin/` does not identify its basename's port. Generate the explicit nested format instead.

A non-null `executables` property causes ZIP extraction. The factory resolves executable paths from the extracted addon folder. Only campaigns retain these mappings; autoload mods discard them.

**Current runtime limitation:** `PortStarter` selects the `Windows` dictionary even on Linux. The UI compatibility helper also accesses the Windows dictionary and may throw when it is absent. A Windows dictionary lacking the selected port can erase a previously supplied custom executable override. Linux entries are representable metadata, but bundled-executable launching on Linux is not currently implemented correctly. Do not work around this by declaring a Linux binary under `Windows`; fix the launcher before claiming cross-platform launch support.

## `options` - user-selectable extra files

**Optional, array of objects.** Each object has:

| Property | Type | Meaning |
| --- | --- | --- |
| `name` | string | Option name shown in the UI and used as its settings key. Defaults to empty when omitted; converted packages should supply a nonempty unique name. |
| `parameters` | object | File path -> parameter type. Optional in the model; supply it for an option that does something. |

Supported parameter types are `DEF` and `INI`, case-insensitively. `DEF` appends the port's additional-DEF switch. `INI` appends `-ini` and is supported only for Blood. An INI option on another game is rejected during argument construction. Options are retained for campaigns/maps and discarded for autoload mods.

Option names must be unique; the factory inserts them into a dictionary. Likewise, each dependency/incompatibility list must have unique IDs, ignoring case. Duplicate names/IDs can parse successfully and then fail during addon creation.

## Package layout and preparation

For normal ZIP import, put **`addon.json` at the ZIP root**, alongside the resources, without a wrapper directory. Additional manifests can be named `addon1.json`, `addon2.json`, etc.; each is a complete object. Keep a primary `addon.json`: drag-and-drop and main-manifest helpers specifically look for that exact root entry, although the extractor scans entries beginning with `addon` and ending in `.json`.

```text
example.zip
  addon.json
  grid.png          (optional)
  preview.png       (optional)
  scripts/main.con
  scripts/main.def
  maps/example.map
```

Ordinary ZIP resources remain packed and are passed to the source port. ZIPs are unpacked when the first inspected manifest has non-null `rff_main`, `rff_sound`, or `executables`; a root `addons.grpinfo` also triggers unpacking via a separate GRPINFO route. Do not use a multi-manifest package to depend on extraction-trigger fields appearing only in a later manifest. The current extractor does not validate every manifest before deciding whether to unpack.

For a pre-unpacked TC, use one immediate subfolder of the game's configured campaigns folder and put its `addon*.json` files at that subfolder's root. Campaign scans support these folders. Map scans discover `.map` and `.zip` files at the maps-folder root; autoload scans discover `.zip` files at the mods-folder root. Folder-based autoload mods are not supported. Avoid unpacked folder names containing a dot: `AddonFilePathWrapper.IsFolder` currently infers folder status from the absence of a path extension.

When converting an original addon:

1. Inspect its README, original batch/launch commands, scripts, archives, and target game/edition. Determine its actual entry points and required port features.
2. Work on a copy in a fresh output directory. Preserve the original archive and existing installations. Extract nested containers or convert resources only when required by the selected port and verified against its input formats.
3. Choose `tc`, `map`, or `mod` from the launch behavior. Write a root manifest with real IDs, credits, versions, paths, and dependencies; omit unsupported or unverified claims.
4. Check declared files plus files referenced by CON/DEF includes and Blood INI episode sections. Preserve exact casing for Linux. Keep required base-game resources external rather than silently adding them to the package.
5. Deserialize **every** manifest with `AddonManifestJsonContext.Default.AddonManifestJsonModel` and run the shared authoring validator against its actual root files. Then check factory constraints, references and output layout. A generic JSON linter alone does not validate BL's contract.
6. Test import and game execution only within the requested scope, in an isolated installation. The current conversion workflow excludes port compatibility checks. Record the tested game, port, OS and untested combinations separately.

Current extraction has destructive failure paths: an existing same-name output folder can be deleted, and the source ZIP can be removed after unpacking even if later manifest loading fails. This describes current behavior, not a recommended preparation strategy. Keep source copies and validate before importing.

## Complete examples

These examples demonstrate manifest structure. Their resource names are illustrative: include the named files and check the target port before using them as packages.

### Duke-style campaign with an optional DEF module

```json
{
  "id": "duke3d-example-campaign",
  "type": "tc",
  "game": { "name": "Duke3D" },
  "title": "Example Campaign",
  "version": "1.0",
  "author": "Example Author",
  "release_date": "2026-10-04",
  "description": "A campaign.\n\nSee the included README for credits.",
  "con_main": "scripts/main.con",
  "def_main": "scripts/main.def",
  "dependencies": { "features": ["EDuke32_CON"] },
  "options": [
    { "name": "Extra music", "parameters": { "scripts/music.def": "DEF" } }
  ]
}
```

### Blood campaign retaining RFF containers

```json
{
  "id": "blood-example-campaign",
  "type": "tc",
  "game": { "name": "Blood" },
  "title": "Example Blood Campaign",
  "version": "1.0",
  "ini": "EXAMPLE.INI",
  "rff_main": "EXAMPLE.RFF",
  "rff_sound": "EXAMPLESND.RFF",
  "dependencies": { "features": ["Modern_Types"] },
  "options": [
    { "name": "Alternate episodes", "parameters": { "ALTERNATE.INI": "INI" } }
  ]
}
```

Use `Modern_Types` only when the campaign actually requires modern Blood map types. This example triggers ZIP extraction; retain a source copy.

### User map requiring an official expansion

```json
{
  "id": "duke3d-example-map",
  "type": "map",
  "game": { "name": "Duke3D" },
  "title": "Example Caribbean Map",
  "version": "1.0",
  "startmap": { "file": "maps/example.map" },
  "dependencies": { "addons": [{ "id": "dukevaca" }] }
}
```

### Autoload mod with additional scripts and incompatibilities

```json
{
  "id": "duke3d-example-mod",
  "type": "mod",
  "game": { "name": "Duke3D" },
  "title": "Example Autoload Mod",
  "version": "1.0",
  "con_modules": ["scripts/extra.con"],
  "def_modules": ["scripts/extra.def"],
  "dependencies": {
    "addons": [{ "id": "duke3d-example-campaign", "version": ">=1.0" }],
    "features": ["EDuke32_CON"]
  },
  "incompatibles": { "addons": [{ "id": "duke3d-conflicting-mod" }] }
}
```

Package this mod as ZIP. Do not add `def_main`, `options`, or executable mappings.

### Campaign bundling explicitly mapped executables

```json
{
  "id": "duke3d-example-bundled",
  "type": "tc",
  "game": { "name": "Duke3D" },
  "title": "Example Bundled Campaign",
  "version": "1.0",
  "con_main": "scripts/main.con",
  "executables": {
    "Windows": { "EDuke32": "bin/eduke32.exe" },
    "Linux": { "EDuke32": "bin/eduke32" }
  }
}
```

This demonstrates the nested format and causes extraction. The Linux entry is currently metadata only because of the runtime limitation described above; resource/script lookup must also be checked from the bundled executable's working directory.

## Implementation references and verification

The reviewed source commit is linked above so these findings can be reproduced even after the application changes:

- [Manifest models](https://github.com/fgsfds/BuildLauncher/tree/3ecec2a5fb6374d76427f1999fd9a2881c2b7c9e/src/Core.All/Serializable/Addon) and [custom converters](https://github.com/fgsfds/BuildLauncher/blob/3ecec2a5fb6374d76427f1999fd9a2881c2b7c9e/src/Core.All/Serializable/JsonConverters.cs): JSON types and legacy mappings.
- [AddonFactory](https://github.com/fgsfds/BuildLauncher/blob/3ecec2a5fb6374d76427f1999fd9a2881c2b7c9e/src/Addons/Addons/AddonFactory.cs): game/type construction, mod restrictions, options, and executable paths.
- [ManifestHelper](https://github.com/fgsfds/BuildLauncher/blob/3ecec2a5fb6374d76427f1999fd9a2881c2b7c9e/src/Core.Client/Helpers/ManifestHelper.cs), [ArchivedAddonExtractor](https://github.com/fgsfds/BuildLauncher/blob/3ecec2a5fb6374d76427f1999fd9a2881c2b7c9e/src/Addons/Helpers/ArchivedAddonExtractor.cs), and [InstalledAddonsProvider](https://github.com/fgsfds/BuildLauncher/blob/3ecec2a5fb6374d76427f1999fd9a2881c2b7c9e/src/Addons/Providers/InstalledAddonsProvider.cs): import and package discovery.
- [AutoloadModsValidator](https://github.com/fgsfds/BuildLauncher/blob/3ecec2a5fb6374d76427f1999fd9a2881c2b7c9e/src/Addons/Helpers/AutoloadModsValidator.cs), [PortsHelper](https://github.com/fgsfds/BuildLauncher/blob/3ecec2a5fb6374d76427f1999fd9a2881c2b7c9e/src/Avalonia.Desktop/Helpers/PortsHelper.cs), [argument builders](https://github.com/fgsfds/BuildLauncher/tree/3ecec2a5fb6374d76427f1999fd9a2881c2b7c9e/src/Ports/Builders), and [PortStarter](https://github.com/fgsfds/BuildLauncher/blob/3ecec2a5fb6374d76427f1999fd9a2881c2b7c9e/src/Ports/Ports/PortStarter.cs): compatibility and launch behavior.
- [SerializerTests](https://github.com/fgsfds/BuildLauncher/blob/3ecec2a5fb6374d76427f1999fd9a2881c2b7c9e/src/Tests.Unit/SerializerTests.cs), [AddonFactoryTests](https://github.com/fgsfds/BuildLauncher/blob/3ecec2a5fb6374d76427f1999fd9a2881c2b7c9e/src/Tests.Unit.Sequential/AddonFactoryTests.cs), and [db/manifests.json](https://github.com/fgsfds/BuildLauncher/blob/3ecec2a5fb6374d76427f1999fd9a2881c2b7c9e/db/manifests.json): contract tests and maintained manifest examples. Serializer-only fixtures may contain combinations that the factory cannot use; they are not all conversion templates.

All five complete JSON examples were checked with the current manifest serializer on Windows/.NET 10. Additional parsing checks covered missing required properties, property/enum casing, game aliases, CRC types, map formats, and executable formats. This review does not establish successful interactive launches or Linux behavior.

## Changelog

**1.0j (2026-10-04)** - Added the shared developer/CLI authoring validation and its distinction from the unchanged parser contract. Moved maintained repository documentation to `.agents/docs/`.

**1.0i (2026-10-04)** - Reconciled documentation with the current code: required fields and case rules; game/feature/port enums; string-only, unenforced CRC metadata; `release_date`; `options`; nested executables and legacy mapping; package layout; separation of parser support and runtime limitations; complete examples.

**1.0a** - Added `type`, `ports`, `RFF`, `SND`, `INI` tokens

**1.0b** - Added `features` token, extended `game` with different versions, removed `ports`

**1.0c** - Made all tokens lowercase

**1.0d** - Additional games and features, proposed changes

**1.0e** - Updated to the specs currently used by the BuildLauncher

**1.0f** - Replaced `-` with `_` in game versions enums

**1.0g** - Added `executables` token

**1.0h** - Renamed shadowwarrior -> wang, exhumed -> slave, added SndInfo feature
