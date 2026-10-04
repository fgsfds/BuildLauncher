---
name: buildlauncher-blood-rff
description: Prepare Blood addons containing RFF files as unpacked native ART/RFF/DAT packages with main/sound RFF declarations, audit established RFS compilation recipes, and assemble optional DEF modules. Use for Blood RFF/RFS preparation rather than ART-to-PNG export.
---

# Native Blood resources and DEF options

Use for Blood addons containing RFF files, detected case-insensitively in the
source inventory and nested archives. These addons need unpacking and may retain
native ART, RFF and DAT; no separate native-profile approval is needed. Blood
addons without RFF instead require DEF-backed conversion of ART/DAT. Read [the Blood profile](../../docs/games/blood.md) and [common conversion plan](../../docs/addon-conversion-plan.md)
and [the manifest skill](../buildlauncher-addon-json/SKILL.md). Identify an existing RFS compiler and verified recipe if rebuilding is required.

## Native package contract

- Preserve existing addon ART, RFF and DAT byte-for-byte, including SURFACE.DAT
  and VOXEL.DAT. Use existing RFFs directly; establish any required rebuilding
  recipe separately. Native VOXEL.DAT
  remains native; separate VOXEL-to-DEF export is still deferred.
- Declare the actual `rff_main` and/or `rff_sound` relative root filenames. The
  current ArchivedAddonExtractor unpacks a ZIP when either field is non-null;
  the shared authoring validator skips normal Blood root-file restrictions for a
  nonblank main or sound RFF declaration. Do not invent an RFF field to bypass checks.
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

## Preparation preferences

For these Blood campaign installer conversions, always skip optional HD sounds and
the regular/general-purpose voxel pack. Choose the original game sound archive
before adding campaign sounds. Do not load or expose the optional decals/pickups/
switches/props pack includes. Keep addon-required minimum voxels and the verified
addon-specific extra/tree deltas; shared voxel assets used by them remain required.
Preserve original source archives and author credits.

## Resource-generating installers

Audit the original distribution script, not only an already extracted addon
folder: compilation choices and generated wrappers may be absent there. Hash the
source archive/compiler and read applicable root and nested README files. Record
branch labels, inputs, output files, commands and line numbers in JSON evidence.
Do not run an installer against a live game folder for research.

- Separate checked prerequisites from unchecked copy inputs. A recipe may require
  main/sound/GUI RFFs and original ART, while also copying logo videos or CD music.
  Files copied for a portable installation are not necessarily generated resources
  or resources that belong in a distributable addon. Establish each runtime role.
- Trace sound-base selection independently: one branch may delete an included
  HD SOUNDS.RFF and copy the original game archive, while another preserves the
  included HD archive. Both may then compile the same custom RAW/SFX script.
  This is not equivalent to a DEF checkbox over one already compiled archive.
- Record compiler identity and option semantics. A verified BARF 1.01 example uses
  `barfc staged-main.rff -u @main.rfs` and the equivalent sound command; embedded
  help describes update as adding or replacing newer files. Preserve relevant
  timestamps and verify actual resulting resources; do not assume unconditional
  overwrite or explain a mismatch by timestamps without evidence.
- Preserve native resource IDs, aliases and generated SFX parameters. Compare
  every RFS resource payload and generated SFX record with compiled output, and
  keep inherited resources separate. Distinguish compiler output from prepared
  edits; a matching archive filename does not establish compilation fidelity.
- Trace wrapper generation and archive insertion. A generated blood.def inside an
  autoload ZIP can be decomposed into an addon-specific base wrapper plus optional
  modules, but preserve include order and all default resources. Do not lose a
  required minimum voxel preset when making extra voxels optional.
- Model each installer option as its full set of effects: includes, QAV/SEQ/tile
  bindings, item tables, archive rebuilds and settings. A weapon branch may change
  shell artwork and ITEMS.ITM as well as QAVs; a DEF-only wrapper is then a partial
  adaptation. BL options support DEF and Blood INI arguments, not arbitrary file
  swaps or option dependencies. Use a separately prepared variant or an established
  engine resource mechanism when a file swap is required.
- Check conditional companions: smoothing may require a separate normal-shotgun
  module when the alternate shotgun is disabled. A source branch available only
  with another option must not become independently selectable without auditing
  those dependencies. Do not infer equivalence from a skin/weapon option's name.
- Keep performance presets and platform alternatives separate. Reduced-memory
  voxel definitions and author renderer/config recommendations are not proof of
  compatibility and must not be silently applied to all packages.

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

For NBlood's fixed-name custom item table, a verified packaging alternative to
swapping a loose file is a small GRP containing virtual ITEMS.ITM. Load the default
archive from the base DEF and an alternate archive from the selected weapon DEF.
Place both below a non-autoload subdirectory, and omit the loose root ITEMS.ITM
so it cannot take filesystem precedence. Verify first-pass loadgrp runs before
resource initialization, newest-group lookup and userItemsInit's name-based lookup
in the actual engine source. Record both payload hashes and check both option
states; this is engine-specific source evidence, not a tested launch recipe for
all ports. See [NBlood item loading](https://github.com/NBlood/NBlood/blob/master/source/blood/src/nnextcitem.cpp)
and [virtual filesystem lookup](https://github.com/NBlood/NBlood/blob/master/source/build/src/vfs.cpp).

Keep alpha shotgun and SmoothBlood independently selectable. Do not make the
alpha wrapper include the full smoothing wrapper. When needed for stable NBlood
option ordering, register only the normal shotgun companion before overriding
its QAV IDs with alpha resources; verify that no other weapons become smoothed.
Inspect AddExternalResource's named-resource cache and numeric-ID behavior and
check both selection orders. Keep alpha tile, shell voxel and item-table changes
inside the alpha option. Do not generalize this ordering recipe to other engines.

A missing external SEQ path may leave an existing compiled numeric-ID resource
active. Audit the RFF fallback before calling it a missing runtime resource, and
keep the distinction from a missing optional replacement. Verify similarly named
files by payload and IDs instead of silently substituting a prefix variant.

A renamed optional module must preserve semantics. Investigate removed texture
lines or weapon definitions before repeating them; do not infer that an unused
file is unnecessary. Verify option collisions and incompatible-addon declarations
against the actual overlapping resources.

Check every include, PNG, KVX, map and rffdefineid path, including appended resource
extensions. Distinguish exact matches, case-only mismatches and truly missing files.
Fix confirmed source typos only with established intent, and record corrections.
INI/DEF semantic lookup and filesystem case checks are separate from JSON parsing.
Run [`validate-addon`](../../docs/addon-manifest-validation.md) for every final manifest. Put input hashes,
compiled-resource comparisons, preset deltas, omissions and unresolved questions
in JSON evidence outside the package. Keep shared instructions here; no per-addon
Markdown is required.
