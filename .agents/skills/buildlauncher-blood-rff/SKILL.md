---
name: buildlauncher-blood-rff
description: Prepare Blood addons containing RFF files as unpacked native ART/RFF/DAT packages with main/sound RFF declarations, audit established RFS compilation recipes, and assemble optional DEF modules. Use for Blood RFF/RFS preparation rather than ART-to-PNG export.
---

# Native Blood resources and DEF options

Use for Blood addons containing RFF files, detected case-insensitively in the
source inventory and nested archives. These addons need unpacking and may retain
native ART, RFF and DAT; no separate native-profile approval is needed. Blood
addons without RFF instead require DEF-backed conversion of ART/DAT. Read [the shared conversion plan](../../docs/addon-conversion-plan.md)
and [the manifest skill](../buildlauncher-addon-json/SKILL.md). The repository ART
exporter has no RFF writer or RFS compiler; this skill records packaging and
verification steps, not an implemented compilation command.

## Native package contract

- Preserve existing addon ART, RFF and DAT byte-for-byte, including SURFACE.DAT
  and VOXEL.DAT. Use existing RFFs directly; establish any required rebuilding
  recipe separately. Native VOXEL.DAT
  remains native; separate VOXEL-to-DEF export is still deferred.
- Declare the actual `rff_main` and/or `rff_sound` relative root filenames. The
  current ArchivedAddonExtractor unpacks a ZIP when either field is non-null;
  the shared authoring validator bypasses normal Blood root-file restrictions
  only for a nonblank declaration. Do not invent an RFF field to bypass checks.
- Prepare in fresh staging. Rename a reserved blood.ini to an addon-specific INI
  and update `ini`; preserve its contents unless a verified edit is required.
  Validate map, music, video and sound references separately.
- Unpack author-provided autoload archives into staging with path/case checks.
  Preserve their resource layout when referenced by DEF. Do not ship bundled
  ports, configuration or installer tools merely because they were in the source;
  decide and record omissions after establishing their runtime role.
- Derive required features from resources and current BL code. Native ART does
  not remove requirements from additional tilefromtexture DEF or modern maps.
  Record author-required ports without claiming compatibility tests or launches.

## RFS compilation boundary

Read compiler scripts as data. Establish the compiler/version, full command line,
base main/sound archives, source resource tree and overwrite/merge order before
rebuilding. Obtain required game inputs from the user; do not substitute a
prepared comparison archive or assume an included sounds.rff is the intended base.
A source sound archive may contain different sample payloads and formats.

Preserve resource names, types, numeric IDs, aliases and generated data records.
RAW files alone cannot replace SFX definitions: preserve volume, pitch, pitch
range, format, loop start and RAW name. Main and sound scripts may assign music
into the sound archive despite the script's name. Determine destination from the
actual compiler commands. Preserve inherited base resources if the recipe merges
with a game archive rather than producing an addon-only archive.

Verify RFFs by decoded resource data and effective `(type, ID)` lookup, not archive
hash alone; compiler padding and directory ordering can differ. Do not deduplicate
repeated names blindly: the same payload can have multiple IDs. Validate signatures,
versions, directory/resource bounds and dictionary flags before trusting an audit.
The [NBlood reader implementation](https://github.com/NBlood/NBlood/blob/master/source/blood/src/resource.cpp)
and [packed structures](https://github.com/NBlood/NBlood/blob/master/source/blood/src/resource.h)
provide the format reference: 48-byte entries, version-dependent directory XOR,
and optional encryption of a resource's first 256 bytes. A read-only directory
or payload audit does not establish a working RFS compiler or game launch.

Keep compiler support files, such as a GUI resource archive, only after confirming
whether the game needs them. Missing base inputs or commands block rebuilding;
continue inventory, DEF assembly, metadata and manifest checks independently.

## Optional DEF assembly

Build a base wrapper from confirmed required includes. Expose independent optional
wrappers through manifest `options` with `parameters: {"module.def": "DEF"}`;
keep optional modules out of the base include graph.

For cumulative voxel presets, compare parsed voxel statements: keep the common
baseline once, extract additions relative to the preceding preset, and verify
both differences and conflicts by tile ID and voxel path. This can make an
additional-voxel option and a tree option independently selectable. Preserve
scale, rotation and block semantics. Do not apply line subtraction to arbitrary
DEF scripts with includes, conditional directives or order-dependent overrides.

A renamed optional module must preserve semantics. Investigate removed texture
lines or weapon definitions before repeating them; do not infer that an unused
file is unnecessary. Verify option collisions and incompatible-addon declarations
against the actual overlapping resources.

Check every include, PNG, KVX, map and rffdefineid path, including appended resource
extensions. Distinguish exact matches, case-only mismatches and truly missing files.
Fix confirmed source typos only with established intent, and record corrections.
INI/DEF semantic lookup and filesystem case checks are separate from JSON parsing.
Run the read-only validate-addon CLI for every final manifest. Put input hashes,
compiled-resource comparisons, preset deltas, omissions and unresolved questions
in JSON evidence outside the package. Keep shared instructions here; no per-addon
Markdown is required.
