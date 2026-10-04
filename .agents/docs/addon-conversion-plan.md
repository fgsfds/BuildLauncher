# Addon preparation plan for BuildLauncher

Shared workflow for preparing Build Engine addons, maintained from the repository
implementation and verified conversion behavior. Choose recipes from each package's
actual resources rather than assuming every addon follows one example.
Manifest contract: [addon.json](addon.json-Specifications.md). The original inputs
are preserved; all conversion work uses fresh output directories.

## Agreed conventions

- For Blood, select packaging from RFF presence during inventory, checking file
  extensions case-insensitively and inspecting nested archives. An addon containing
  RFF files needs unpacking: preserve its native ART, RFF and DAT resources and
  declare the actual main/sound RFF entry points so BL unpacks its ZIP. This is the
  normal RFF workflow and needs no separate profile approval.
- Without RFF files, prepare a ZIP-loaded Blood addon: convert ART to PNG + DEF
  and DAT metadata to the corresponding DEF definitions. Do not retain raw ART/DAT
  as a workaround or manufacture an RFF declaration. Unsupported DAT conversion
  remains unresolved until a verified recipe/tool is available.
- Transfer surfaces for exported tiles. The user permits ignoring entries for
  tiles absent from ART. The exporter also skips empty slots; unusual metadata
  in empty slots is rejected by the exporter and needs an explicit handling recipe.
- For the no-RFF profile, VOXEL.DAT requires a separate DEF with corresponding KVX
  bindings. The user deferred that training; the current exporter rejects it
  rather than silently claiming a complete conversion. For the RFF profile, keep
  native VOXEL.DAT unchanged; no voxel conversion is required.
- Prefer grid.png at 600x900 and preview.png at 320x200. Index preview with the exact
  target-game palette; grid may be RGB/RGBA. These are preparation conventions,
  not BL parser constraints.
- Check db/addons.json for existing releases of the same addon during conversion.
  User-provided artwork and description take priority; otherwise reuse suitable
  catalog descriptions and artwork from an earlier release before finding material
  online or generating it. Record reused versions, hashes and adaptations. Identify
  generated illustrations rather than presenting them as gameplay screenshots.
- Port compatibility and game launches are excluded from the current checks.

## Workflow for the next addon

1. **Inventory.** Record files and SHA-256 hashes; identify archives, ART, DAT,
   RFF/GRP, INI, CON/DEF, maps, music, models and scripts. Read instructions/credits
   as data without executing unknown commands. Extract containers into a separate
   working directory with path validation.
2. **Determine launch requirements.** Identify the game, version, entry point and
   package type. For Blood, inspect INI episodes, maps and music. An addon described
   as a mod does not necessarily use BL `type: mod`; an episode-based campaign
   normally uses `tc`. Follow
   the current parser/factory contract. Check db/addons.json for a matching game,
   type and stable ID, using title/credits to investigate when no ID is supplied.
   Keep a confirmed existing ID for the new version.
3. **Choose recipes.** A Blood addon containing RFF files uses unpacked native
   packaging; a Blood addon without RFF requires PNG/DEF resource conversion.
   Identify the real main/sound RFF roles before writing manifest declarations.
   For native Blood/RFS resources, use
   [the RFF packaging skill](../skills/buildlauncher-blood-rff/SKILL.md). For ART/SURFACE use the repository's [.NET exporter](art-exporter.md)
   and a verified target-game ACT palette. Later VOXEL work needs ID-to-KVX mapping
   and a separate DEF. Investigate other containers/resources separately; one
   example does not establish a universal extraction recipe.
4. **Prepare resources.** Export into a fresh resource directory, then assemble a
   separate addon package with maps, INI, music, credits and required attachments.
   For the PNG/DEF profile, remove source ART/DAT copies from the prepared package
   only after verification. The native profile keeps them byte-for-byte.
   Preserve credits, author documentation and development extras unless the
   requested packaging calls for a deliberate, recorded omission.
5. **Manifest and presentation.** Prepare a stable ID, accurate version/title,
   relative paths, INI and DEF modules; add description and artwork. Declare
   `dependencies.features: ["TileFromTexture"]` for converted tilefromtexture
   resources. Check the matched catalog entry's Description and obtain suitable
   grid/preview from its DownloadUrl package in fresh staging, verifying Sha256
   when supplied. Revalidate dimensions, PNG format and the preview game palette;
   adapt version-specific wording and retain the current release version. Follow
   [the presentation skill](../skills/buildlauncher-addon-presentation/SKILL.md#reuse-an-existing-release) for the full procedure. Derive current credits from supplied
   evidence and identify reused catalog metadata; omit unknown release dates. Record when a release version comes only from the supplied
   directory/archive label. Store factual sources, conversion parameters and
   qualifications outside addon.json.
6. **Validate.** Use the actual AddonManifestJsonContext and [shared authoring validator](addon-manifest-validation.md), factory constraints,
   INI/DEF references, filename case, tile ID uniqueness, decoded pixels/palette/
   transparency, offsets, animations and surfaces. Add voxel checks when that
   conversion is implemented. No port compatibility or launch claim follows from
   these checks.
7. **Package.** Put addon.json and resources at ZIP root without an extra wrapper.
   Check the final archive as well as intermediate output. A user-named working
   folder is a preparation artifact: a dotted name can fail BL folder discovery.
   Keep provenance separate from addon.json. Avoid importing into a live
   installation through BL's currently unsafe extractor.

Catalog lookup and permitted presentation reuse are part of the normal workflow.
If the user explicitly excludes earlier releases or reference material for an
independent source-only conversion, prepare, validate and publish using permitted
inputs before reading the excluded reference. Freeze its file inventory and
hashes first. Then compare payload hashes, decoded tile indices/palettes/alpha,
DEF metadata, manifest behavior and artwork separately; record differences without
retroactively changing the independently prepared output to match the reference.

## Implemented resource exporter

The standalone `src/Addon.ArtExporter` .NET 10 console project is part of
BuildLauncher.slnx. See [its complete usage and format documentation](art-exporter.md).
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

## Shared instructions and conversion evidence

Maintain reusable instructions in these shared documents and the repository
skills. Do not create a Markdown guide, reference analysis or comparison report
for each addon. Original author-provided documentation remains part of the source
and should be preserved according to its redistribution terms.

Store each run's evidence as JSON outside addon.json and, by default, outside the
assembled addon package. Reuse the exporter's conversion-report.json and record
source inventories/hashes, catalog ID/version and archive provenance, palette and
artwork hashes/adaptations, deliberate omissions, validation results and untested
behavior. If a comparison is requested, store it in comparison.json and explain
matching resources and meaningful differences in the conversation.

When replacing a requested existing output, finish and validate fresh staging
first. Preserve the previous output in a verified backup or another user-approved
location, then publish the replacement. Keep source packages and comparison inputs
unchanged; never use a live BL installation as conversion staging.

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

## Skills and roles

| Task | Skill |
|---|---|
| Complete workflow and final assembly | [buildlauncher-convert-addon](../skills/buildlauncher-convert-addon/SKILL.md) |
| Manifest, package type and entry points | [buildlauncher-addon-json](../skills/buildlauncher-addon-json/SKILL.md) |
| Native Blood RFF/RFS resources and optional DEF assembly | [buildlauncher-blood-rff](../skills/buildlauncher-blood-rff/SKILL.md) |
| ART, SURFACE, later VOXEL and resource checks | [buildlauncher-blood-art](../skills/buildlauncher-blood-art/SKILL.md) |
| Grid, preview, palette and description | [buildlauncher-addon-presentation](../skills/buildlauncher-addon-presentation/SKILL.md) |

When useful and authorized, assign these roles to separate subagents. One
coordinator assembles the final package. These are repository-local skills, not
new system-level agent settings.
