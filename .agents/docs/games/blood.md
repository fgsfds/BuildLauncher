# Blood addon preparation

Read the [common preparation workflow](../addon-conversion-plan.md),
[manifest contract](../addon.json-Specifications.md) and
[common artwork rules](../addon-artwork-style.md) first. This profile applies to
`game.name: Blood`; its recipes are not defaults for other games.

## Resource boundaries

RFF and the manifest's rff_main/rff_sound fields are Blood-only in this workflow.
SURFACE.DAT, VOXEL.DAT and RFS compilation recipes below are Blood-specific; do
not apply their meanings to another game's DAT files. A native GRP or CON recipe
from Duke is not a Blood packaging substitute. Use INI episode definitions,
Blood resource archives and applicable DEF definitions.

## Packaging profiles

- For Blood, select packaging from RFF presence during inventory, checking file
  extensions case-insensitively and inspecting nested archives. An addon containing
  RFF files needs unpacking: preserve its native ART, RFF and DAT resources and
  declare the actual main/sound RFF entry points so BL unpacks its ZIP. This is the
  normal RFF workflow and needs no separate profile approval.
- Without RFF files, prepare a ZIP-loaded Blood addon: convert ART to PNG + DEF
  and DAT metadata to the corresponding DEF definitions. Do not retain raw ART/DAT
  as a workaround or manufacture an RFF declaration. Unsupported DAT conversion
  remains unresolved until a verified recipe/tool is available.
- Maps/INI-only addons need no resource export. Do not add an empty DEF, an RFF
  declaration or engine-feature dependency when no source resource requires it.
- Transfer surfaces for exported tiles. The user permits ignoring entries for
  tiles absent from ART. The exporter also skips empty slots; unusual metadata
  in empty slots is rejected by the exporter and needs an explicit handling recipe.
- For the no-RFF profile, VOXEL.DAT requires a separate DEF with corresponding KVX
  bindings. The user deferred that training; the current exporter rejects it
  rather than silently claiming a complete conversion. For the RFF profile, keep
  native VOXEL.DAT unchanged; no voxel conversion is required.

## Episode and edition metadata

Blood campaigns use their current addon-specific INI. Rename reserved blood.ini
and update the manifest entry point, preserving bytes except for verified edits.
Validate exact map filename case, episode sections, transitions and secret return
paths. Song and Track are optional; when present, establish addon versus base-game
music sources rather than guessing a required stock song list.

For explicitly requested BHSE identities, append ` BHSE` to the title and `-bhse`
to the existing ID, without duplicate suffixes. Preserve the edition's actual
version and use its polished/improved release date; record the original catalog
identity outside addon.json. Ordinary updates retain their original release date.

Always skip optional HD sounds and regular/general-purpose voxel packs during
resource-generating campaign installer preparation. Retain minimum campaign
voxels, original sounds and justified custom additions. Alpha-shotgun and
SmoothBlood options remain independent; detailed verified assembly is in the
[RFF skill](../../skills/buildlauncher-blood-rff/SKILL.md).
## Installers that generate game resources

For an addon whose installer compiles resources from an original game, use the
[Blood RFF installer audit](../../skills/buildlauncher-blood-rff/SKILL.md#resource-generating-installers).
Read the original script and nested optional resources before running anything;
a pre-extracted addon folder may omit the branch logic. Research is read-only.

Record checked prerequisites, unchecked copies, compiler identity/options,
main/sound base selection, generated wrappers and branch side effects in JSON.
Compilation must use fresh staging and verified user-supplied game inputs; keep
original game archives unchanged. An update compiler can be timestamp-sensitive,
so verify effective resource IDs/payloads and generated SFX records, not just
successful process completion or the resulting archive name.

Audit an optional addon as a complete behavior change. A weapon option may swap
QAVs, textures/voxels and an item table; an HD-sound option may choose a different
archive base before compilation. Current BL DEF/INI options do not implement
arbitrary file swaps, rebuilds, dependencies or exclusive selection. Preserve
required companions and defaults, or establish a deliberate separate variant.
Keep semantic voxel-preset deltas distinct from case-only changes. Record removed
bindings, absent references and source/prepared payload differences without
assuming the prepared package reproduces every installer branch.

## Implemented resource exporter

The standalone `src/Addon.ArtExporter` .NET 10 console project is part of
BuildLauncher.slnx. See [its complete usage and format documentation](../art-exporter.md).
It reads ART v1, including BUILDART-prefixed files, and optional SURFACE.DAT, then
writes indexed tiles/*.png, a root DEF and conversion-report.json.

```powershell
dotnet run --project src/Addon.ArtExporter -- `
  --input ./source-addon --output ./converted-resources `
  --palette ./game.act --def resources.def
```

Paths are placeholders. Supply the palette explicitly; no game resources are
bundled. Output must be absent and its parent must exist. Validation precedes
staging and publication. `--ifcrc` is available and disabled by default; DEF blocks
close independently of this option. The exporter handles signed offsets, Blood
extra/view bits and forward, oscillating and backward animation metadata.

Implementation map, all within this repository:

| Source | Responsibility |
|---|---|
| `src/Addon.ArtExporter/ArtReader.cs` | Bounded ART snapshots, header/array/pixel validation and decoding |
| `src/Addon.ArtExporter/IndexedPngWriter.cs` | Column-major transposition, palette rotation, transparency and PNG chunks |
| `src/Addon.ArtExporter/ArtResourceExporter.cs` | Surfaces, DEF/CRC, input hashes and safe publication |
| `src/Addon.ArtExporter/Program.cs` | CLI arguments, help and process status |
| `src/Tests.Unit/ArtResourceExporterTests.cs` | Public behavior, independent PNG decoding and failure preservation |

The optional Python/Pillow profile verifier independently checks simple
ART/SURFACE exports. Its grammar excludes optional ifcrc, extra bits and backward
animation; .NET tests cover those exporter cases. See the shared exporter guide
for its command, checks and limits.

## Later VOXEL stage

VOXEL.DAT is a little-endian signed int16 array indexed by tile ID; -1 denotes no
binding. It contains no filenames. Obtain a second example and establish voxel ID
to KVX mapping, including any required game/RFF resources, before implementing
separate voxels.def output. DEF supports [voxel-to-tile bindings](https://wiki.eduke32.com/wiki/Voxel_%28DEF%29).
Establish scale, rotation and absent-tile handling from evidence rather than guessing.
The user explicitly permitted skipping absent tiles for SURFACE; determine the
VOXEL policy separately.

Future checks must include odd table byte lengths, -1/nonempty bindings, missing
KVX and failure preservation. Existing ART tests already exercise palette,
transparency, signed offsets, animation directions, view bits, both header forms,
short surfaces and invalid/overlapping/truncated resources.


## Artwork

### Grid

Favor dark horror imagery, black/deep red backgrounds and high-contrast red or pale titles. Use gothic, distressed or game-like lettering that fits the addon. An addon scene with a title, a sparse logo card or an author illustration all occur. Death Wish, No Hope In Sight, Hostile Takeover and Bloodstream demonstrate different valid layouts; the Blood handprint is an optional game motif, not a compulsory badge.

### Preview

Campaign previews are commonly landscape title cards/logos on dark backgrounds, sometimes over a scene. Recompose the title and emblem for 320x200 rather than shrinking a portrait cover. Mods use a focused creature/weapon/resource showcase, as in Cultists With Correct Guns and Voxel Pack.

Use the [common artwork rules](../addon-artwork-style.md) for dimensions, logo
placement, pixel handling, preservation and exact preview palette verification.
