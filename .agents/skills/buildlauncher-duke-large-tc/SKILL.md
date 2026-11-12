---
name: buildlauncher-duke-large-tc
description: Compare and prepare large CON/GRP-family total conversions (Duke3D, Redneck, NAM and WWII GI) with nested CON graphs, a main DEF, native ART and palette tables, models/voxels/media, dynamic content and selection-marker GRPs. Use alongside the Duke CON skill; not for Blood RFF conversion or automatic removal of real GRP payloads.
---

# Large CON/GRP-family total conversions

Use [the coordinator](../buildlauncher-convert-addon/SKILL.md),
[the Duke CON skill](../buildlauncher-duke-con/SKILL.md) and
[manifest/presentation rules](../buildlauncher-addon-json/SKILL.md).
This profile supplements ordinary CON campaign preparation with large resource
and container-role audits. Comparisons are read-only unless preparation is requested.

Read the [shared family profile](../../docs/games/con-grp-family.md#large-total-conversions)
and [selected game profile](../../docs/games/index.md) for workflow and artwork.

## Inventory and comparison

- Verify the catalog archive hash and preserve the supplied source archive.
  Hash every archive entry, recording unsafe paths and case collisions before
  extracting. Normalize only known wrapper/entry-point renames for comparison;
  report identical, changed, new and older-only files with inventories in JSON.
- Strip only an outer distribution wrapper when assembling runtime content.
  Preserve resource subdirectories, include roots and duplicate basenames in
  different directories. Do not flatten maps, sounds, models or nested scripts.
- Reuse stable identity and suitable artwork; use new-source scripts/resources.
  Do not overlay an older package or restore removed assets blindly: obsolete
  sounds, sprites and voxels may have been removed deliberately.

## Entry points and container roles

- Declare actual main CON and main DEF files explicitly. An existing prefixed
  CON needs no unnecessary rename. Rename game-reserved root DEF names (duke3d.def for Duke) using the
  established package name and make def_main match its exact physical case.
- Inspect GRP directories and GRPINFO contents. A tiny GRP containing only dummy
  text can be a startup-selection marker rather than resource data; record that
  role. Explicit BL entry points may replace its startup-selection role, but
  establish script references before omitting marker/helper files. This does
  not justify removing real GRP payloads or inventing a manifest grp property.
- Trace the main CON and DEF include graphs separately. Distinguish archived
  legacy/auxiliary scripts from the active graph; dynamic references and script
  execution reachability need additional analysis beyond include traversal.

## Resources and unresolved references

- Preserve native ART, palette/lookup/translucency tables, referenced raw blend
  data, models, voxels, map-specific ART/DEF and videos in their established
  layout. Blood-specific DAT-to-DEF rules do not apply to this Duke profile.
  Validate format/data boundaries where supported; do not claim unsupported
  formats were converted or tested.
- Audit active tilefromtexture/texture/model/voxel/palette directives and MAP
  versions. Add TileFromTexture for used tilefromtexture resources and TROR for
  native v9/TROR maps; do not blindly copy an older incomplete feature list.
  Verify binary MAP bounds before reading sector fields. In v9 maps, inspect
  ceilingstat/floorstat bit 1024 for actual TROR extensions; record affected
  maps and counts. A version header alone is format evidence, and simulated
  room-over-room effects do not establish TROR. See the official
  [sector flags](https://wiki.eduke32.com/wiki/Ceilingstat).
  Distinguish mandatory graphics features from optional renderer effects.
- Audit static literal references with exact paths/case, then classify absent
  paths as base-game, optional/dormant, alternate-extension candidates or
  unresolved source gaps. Record script, line and declaration in JSON/CSV.
  A reachable include file does not mean every declaration is executed. Do not
  fabricate resources, delete definitions or borrow unrelated files to make
  an audit pass; request specific guidance when required content is unresolved.
  When the user explicitly accepts existing source gaps, finish preparation
  with those declarations unchanged. Record the accepted exception and every
  absent path outside addon.json; manifest validation does not resolve them.
- Normalize confirmed own-resource references to exact shipped case, retaining
  comments/unrelated code. Preserve original archives and record each change.
- A supplied PALETTE.DAT may differ from the common Duke RGB preset. Establish
  its encoding and intended preview palette; preserve the complete table file
  and its extra data. Reuse an older indexed preview only after palette checks.

## Documentation and handoff

Read current credits and setup material as evidence. Keep credits, licenses and
mixed gameplay documentation; omit installation-only guides/images, bundled
ports/editors and proven OS thumbnail/desktop caches. Do not discard functional
DAT/RAW/editor-looking resources solely by filename or extension. Follow the
shared original-release/improved-edition date policy.

Deliver comparison/provenance JSON and unresolved items. For requested preparation,
validate the manifest, own-resource references and final archive independently.
Keep game launches, renderer behavior, port/OS compatibility and live import
untested unless requested. A comparison alone does not produce a prepared release.
