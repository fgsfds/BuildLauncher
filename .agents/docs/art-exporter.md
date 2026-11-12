# ART resource export and verification

The current `master` checkout includes the read-only
[ART/SURFACE profile verifier](#validation). The standalone .NET exporter and
its tests are in local commit `4a79ece7920beea555dcb1c78601adbdf6420bd4`
(available via `codex/backup-before-committer-rewrite`). The exporter commands,
format limits and output contract below apply to a checkout containing
`src/Addon.ArtExporter`. BuildLauncher itself does not perform this conversion.

For Blood addons containing RFF files, use unpacked native packaging and retain
ART/RFF/DAT; the ART exporter is unnecessary for that profile. Without RFF,
ART/DAT require DEF-backed conversion. This tool implements ART/SURFACE only;
VOXEL and other unsupported DAT formats require a separate verified recipe. See
[the common workflow](addon-conversion-plan.md) and [game profiles](games/index.md) for recipe selection; Blood-specific DAT semantics must not be applied to other games.

## Run the exporter from the backup checkout

Install the .NET 10 SDK, then run from the root of the checkout containing the exporter:

```powershell
dotnet run --project src/Addon.ArtExporter -- --help

dotnet run --project src/Addon.ArtExporter -- `
  --input ./source-addon `
  --output ./converted-resources `
  --palette ./blood.act `
  --def resources.def
```

These paths are placeholders for your input directory, new output directory and
palette file. The output must not exist, and its parent directory must exist.
The tool does not modify the source or overwrite existing output. Input and output
must be separate directories, with neither containing the other. Linked source
files and linked directory ancestors are rejected to keep that separation explicit.

For a reusable command-line binary:

```powershell
dotnet publish src/Addon.ArtExporter -c Release -o ./artifacts/art-exporter-cli
```

This produces a framework-dependent application requiring the .NET 10 runtime.
Use the published `BuildLauncher.ArtExporter` executable, or run its DLL with dotnet.

| Option | Behavior |
|---|---|
| `--input DIRECTORY` | Required. Reads all root files with a case-insensitive `.art` extension; no recursive container extraction. |
| `--output NEW_DIRECTORY` | Required. Publishes a complete export to a previously absent directory. |
| `--palette GAME.act` | Required. RGB ACT palette with 256 colors. No game palette is bundled. |
| `--surface FILE` | Optional explicit SURFACE.DAT path. Otherwise discovers root SURFACE.DAT case-insensitively. |
| `--def NAME.def` | Root DEF filename; defaults to `resources.def`. Paths and unsafe portable filename characters are rejected. |
| `--ifcrc` | Adds `ifcrc 0xXXXXXXXX`, computed from the original column-major ART pixel bytes. Disabled by default. |
| `--help` | Prints usage and exits successfully. |

Exit codes: 0 for success/help, 2 for usage or invalid path arguments, 1 for invalid
resource data or filesystem failures. Errors identify the resource and tile where
possible. Duplicate options and unknown flags are rejected.

For generated addon.json, run `validate-addon --input PACKAGE_DIRECTORY` with
the same executable, or see [manifest authoring validation](addon-manifest-validation.md)
for the checkout, dotnet command and rules. This command has its own --input/--manifest options;
it performs no export or file writes.

## Input formats and conversion

ART v1 consists of four little-endian int32 header values: version, legacy total
count, first tile ID and last tile ID. The actual slot count is `last - first + 1`;
the legacy count is not used. The header is followed by signed int16 widths,
signed int16 heights, uint32 picanm metadata and each tile's column-major bytes.
The optional eight-byte ASCII `BUILDART` prefix is also supported.

Tile IDs come from the header rather than filenames. The implementation supports
IDs 0 through 65535, rejects overlapping ranges, negative dimensions, truncated
or trailing data, and limits each input file to 256 MiB. Empty slots produce no
PNG. Empty slots with nonzero metadata and the unsupported high metadata bit are
rejected instead of discarded silently.

Use the exact target-game palette. Supported ACT files contain 768 RGB bytes,
optionally followed by a four-byte big-endian footer: color count 0/256 and
transparent index 255/65535. Palette DAT, RIFF PAL and reduced-color ACT files are
not accepted as ACT; convert those to a verified full RGB palette first.

### Palette validation

The known Blood RGB ACT reference has SHA-256
`1dd1cd9dbb85a905ad7438a9de18d10ab406044f1291eaae2241315dbd87a386` for its 768 RGB
bytes. Its corresponding rotated tile-PNG palette has SHA-256
`9615ac0d70b96a3611392f46b6748411bda545a05b5129b75c33bc2f84176a14`.
These are game-palette fingerprints, not an addon-specific palette recipe. Supply
the correct verified palette for the selected game; no palette is bundled.
The supplied Duke3D/Redneck RGB ACT presentation reference has SHA-256
`3b11874a049a27940e6e50e62dd8578da71b4d0a9132038eff8fefa4386fe713` for 768 bytes.
Its game mapping was confirmed in the supplied editor's Duke 3D / RR / NAM / WWII GI
palette menu and matched a verified older Duke3D package's indexed preview.
This records presentation-palette evidence, not a tested Duke ART export recipe.

Presentation previews use the original palette ordering, independently of the
rotation used for exported tiles. Check the actual palette bytes and alpha rather
than assuming a screenshot or PNG filename establishes the palette.

Output PNGs preserve colors and transparency without quantization. Their indices
are `(sourceIndex + 1) % 256`, and the palette moves its final RGB entry to index 0.
Source index 255 becomes transparent PNG index 0. PNGs use 8-bit indexed color,
PLTE, tRNS, zlib-compressed scanlines and chunk CRCs; they require no native imaging
library. See the [PNG specification](https://www.w3.org/TR/png-3/) for the format.

Metadata becomes:

- Signed x/y offsets from picanm bytes at bit positions 8 and 16.
- Animation frame count from bits 0-5, type from bits 6-7 and speed from bits 24-27.
  Oscillating and forward ranges end at `tile + frames`; backward ranges end at
  `tile - frames`. Endpoint bounds are checked.
- Blood view bits 28-30 as DEF `extra`, when nonzero.
- Blood SURFACE.DAT byte at the exported tile ID as `surface`, when nonzero.

An existing SURFACE.DAT must cover every exported tile ID. Exported surface values
must be 0-14. Missing DAT means zero surfaces. Entries for absent or empty tiles
are intentionally omitted and counted in the report, per the user's conversion
policy. This is not an equivalent replacement for an entire global surface table.

VOXEL.DAT conversion will produce a separate DEF in a later stage. For now its
presence in the source causes a clear unsupported-input error, so a partial resource
conversion cannot be mistaken for complete voxel preparation. No `--voxel` flag is
implemented yet. The exporter performs no port compatibility or game-launch checks.

## Output and packaging

```text
converted-resources/
  resources.def
  conversion-report.json
  tiles/
    4608.png
    ...
```

DEF entries are sorted by tile ID, use relative `tiles/<id>.png` paths and CRLF
line endings, and close texture blocks independently of the optional CRC flag.
The report contains exporter version, counts, index mapping, options and input
filenames/sizes/SHA-256 hashes. It does not embed machine-specific absolute paths.

All inputs and definitions are validated first. Files are written into a unique
sibling staging directory, then moved to the final destination after success.
On failure the final output remains absent, existing content is preserved, and
cleanup targets only the owned staging directory. A cleanup failure is reported
without replacing the original error.

This is a resource exporter, not a complete addon packager. Copy the checked DEF
and tiles into a separate prepared package with maps, INI, music, credits and
[addon.json](addon.json-Specifications.md). Reference the DEF in `def_modules`
and declare `dependencies.features: ["TileFromTexture"]` for its tilefromtexture
resources. The exporter does not create this manifest or select artwork.
Keep conversion provenance outside addon.json and decide separately whether the
report belongs in the distributed archive. For the complete process, read
[the addon preparation plan](addon-conversion-plan.md).

## Validation

In the exporter checkout, `src/Tests.Unit/ArtResourceExporterTests.cs` covers public export
behavior, including independent PNG decoding and CRC checks, pixel orientation,
palette/transparency, metadata, both header forms and preservation on failures.

```powershell
dotnet build
dotnet test --project src/Tests.Unit/Tests.Unit.csproj --no-build
```

For each conversion, compare decoded tile dimensions, indices, palettes,
transparency and rendered RGBA; PNG compression bytes can differ without changing
resources. Verify DEF tile IDs, signed offsets, surfaces and animations against
the source. Keep per-run measurements in JSON rather than addon-specific Markdown.
The repository's optional Python/Pillow [profile verifier](../skills/buildlauncher-blood-art/scripts/audit_art_profile.py)
can independently check a simple ART/SURFACE export:

```text
python .agents/skills/buildlauncher-blood-art/scripts/audit_art_profile.py SOURCE OUTPUT --def resources.def --palette GAME.act
```

That read-only verifier supports a simple line-based tilefromtexture/animtilerange
grammar. It checks ART/DEF tile ID sets, nonsquare pixel orientation, dimensions,
rotated indices and palettes, transparency, signed offsets, surfaces and
forward/oscillating animations; it rejects duplicate/escaping references,
unreferenced PNGs and inconsistent resources. A present SURFACE table must cover
exported IDs; missing DAT expects zero surfaces.

Its grammar excludes optional ifcrc, Blood extra bits, backward animations and
VOXEL.DAT. The exporter checkout's .NET tests cover the first three cases. Voxel
conversion is still deferred. It does not validate addon manifests,
ZIP import or game execution. Run its isolated checks from the repository root:

```powershell
python .agents/skills/buildlauncher-blood-art/scripts/test_audit_art_profile.py
```

Use JSON inventories and comparison evidence following
[the shared preparation plan](addon-conversion-plan.md#shared-instructions-and-conversion-evidence).
