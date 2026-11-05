---
name: buildlauncher-duke-con
description: Prepare Duke Nukem 3D, Redneck Rampage, NAM and WWII GI campaigns with CON entry points, includes, episode definitions, native ART, DEF textures and map/audio resources. Use for shared CON/GRP-family campaign packaging and comparisons; not Blood INI/RFF conversion or proof of port compatibility.
---

# CON/GRP family campaign preparation

Use with [the coordinator](../buildlauncher-convert-addon/SKILL.md),
[manifest skill](../buildlauncher-addon-json/SKILL.md) and
[presentation skill](../buildlauncher-addon-presentation/SKILL.md).
Read [the shared family rules](../../docs/games/con-grp-family.md) and the
[selected game profile](../../docs/games/index.md) before preparing the package. It records CON/maps/audio and native ART/DEF
packaging profiles. GRP/custom-engine sources still need their own resource
inventory and established recipe. For large nested-resource TCs with selection
markers, palette overrides and dynamic content, use
[buildlauncher-duke-large-tc](../buildlauncher-duke-large-tc/SKILL.md).

1. Read current README/credits and bundled manual for source version, game edition,
   launch setup and renderer requirements. Render PDF setup pages when text
   extraction is empty or misses image/vector text. A reused archive filename
   does not establish its current version. Compare with a hash-verified older
   catalog package; preserve its stable identity and suitable artwork. Ordinary
   updates retain the original release date; improved editions use the improved
   edition's release date, following the presentation skill.
2. For a selected episode campaign use `TC` and the actual supported game name; choose main
   or additive CON fields from the source script role.
   A verified additive campaign may use `con_modules` while retaining the base
   main CON; do not turn a module into a main replacement solely because it is a
   campaign. Preserve module order and check the port additional-CON switch.
   If only replacement DEFS/USER scripts are supplied, consult the shared
   profile for obtaining the missing main CON from the verified required game
   edition. Do not assume appending those replacements as modules reproduces
   the original include order and compiled actor constants.
   Keep an established addon-specific CON prefix. Rename reserved root CON files
   and update actual include tokens transitively in the new source scripts.
   Preserve unrelated code, comments, encoding and line endings. Record exact edits.
3. Audit `definevolumename`, `definelevelname`, `music` and `definesound` in active
   CON text, including continuation lines. Check shipped map names/case and
   native map structure, including supported v9/TROR maps. Declare `TROR` when
   current maps require it, even if an older manifest omitted the feature.
   For addons supplying only their custom campaign, hide unrelated stock episodes
   and name the replaced episode after the addon, following the shared episode-menu
   rules using commands supported by the selected game/port. Preserve intentionally supplied multiple episodes and custom/secret map
   slots. Place menu overrides after active declarations/includes; `undefinevolume`
   requires `EDuke32_CON`. Record compiler checks separately from rendered-menu
   and gameplay checks.
   Resolve bundled resources separately from external
   base-game resources. Fix a bundled filename's case only after checking all
   relevant references. Normalize active CON/DEF file-reference tokens to the
   exact shipped case, per user preference; keep comments/unrelated code intact.
   Do not invent copies of absent stock files.
4. Derive features from current scripts/resources and the verified older layout.
   Preserve a justified `EDuke32_CON` declaration for EDuke32-specific code.
   Never apply the Blood ART/DAT policy merely because this is another Build game.
   For an established family native ART profile, retain ART bytes and verify
   headers, tile ranges/dimensions and payload lengths, recognizing BUILDART
   prefixes. Rename game-reserved DEF entry points (duke3d.def for Duke) and declare their replacement as the
   justified main/module DEF, checking texture paths separately. Do not declare
   TileFromTexture for a definetexture-only/native ART package. Without resources,
   no resource export, empty DEF or container declaration is needed.
5. Preserve original author documentation, map notes and required attachments;
   record layout decisions for optional extras. Read and omit standalone
   installation-only guides from package and Extras; keep mixed README/manuals
   unchanged when they also contain credits, licenses or gameplay information.
   Omit BAT launchers, bundled
   redundant source ports and base-game data after establishing their role.
   Preserve a verified required custom engine and its runtime resources using
   supported executable mappings; do not discard it as a generic bundled port.
   Reuse valid
   images, checking the actual game palette.
6. For reported modern-port CON failures, follow the shared legacy-script
   adaptation rules. Inspect exact lines, correct malformed tokens conservatively,
   and record the tested compiler revision. Keep inherited nonfatal warnings
   separate from errors; do not substitute another edition's actor code or rewrite
   original control flow just to obtain a warning-free log.
7. Validate with the repository `validate-addon` CLI, references, native map data,
   source-file accounting and final root ZIP inventory/hashes. Verify differences
   from the older release after accounting for deliberate CON renames/edits.
   Keep JSON provenance outside addon.json. Game launches, renderer behavior,
   port compatibility and live import remain untested unless requested.
