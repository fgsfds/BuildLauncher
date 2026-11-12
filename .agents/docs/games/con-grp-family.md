# Shared CON/GRP family preparation

Read the [common workflow](../addon-conversion-plan.md), then this guide and the
selected [game profile](index.md). Duke Nukem 3D, Redneck Rampage (including
Rides Again), NAM and WWII GI share GRP container and CON script formats.
The preparation rules below apply to all of them: script/include and resource
checks, reserved-name handling, base-edition sourcing, conservative legacy syntax
adaptation, campaign menu cleanup, native ART/MAP/audio retention, feature audits,
large-TC inventory and package validation.

Use the actual game name, resources, palette and entry points. References to
Duke 1.3D/Atomic, duke3d.def, EDuke32 switches/extensions and binary TROR describe
specific cases; apply their general preparation rule to the selected game,
while using only commands/features supported by its scripts and target runtime.
Shared formats do not require replacing another game's actor code with Duke's.
Artwork stays in each game's profile. Blood RFF/INI/DAT recipes do not apply.

## CON campaigns

Use [the CON campaign skill](../../skills/buildlauncher-duke-con/SKILL.md) for this family.
A selected campaign can define several episodes in its CON; it is still one TC
manifest, not one manifest per episode and not an autoload Mod.

- Inspect the current source and a hash-verified older package independently.
  Reuse stable identity, naming conventions and suitable artwork, but prepare
  the new scripts/maps from the new source. Compare hashes after accounting for
  deliberate CON renames/include edits. Do not replace new files with old ones
  just because the older package launches.
- Normal root `GAME.CON`, `DEFS.CON` and `USER.CON` are reserved by BL's authoring
  editor. Give them a stable addon-specific prefix, declare the main file in
  `con_main`, and update actual `include` tokens to the renamed files. Follow the
  include graph; do not blindly replace words in comments/strings or copy the
  older version's complete scripts. Preserve auxiliary headers and referenced layout.
- Some classic campaigns ship replacement DEFS/USER scripts but rely on the
  registered game's original GAME.CON. Establish the required game edition from
  the README and verify the base archive identity before preparing the missing
  main script. When it is needed, copy only that edition's main CON, give it an
  addon-specific name and change its includes to the renamed addon scripts.
  Preserve the rest of the script and record both source and output hashes;
  never copy the entire base-game archive or substitute another edition silently.
  Loading replacement constants only after the original actor code as additive
  modules is not equivalent to replacing its includes at compilation time.
- Audit absent legacy sound/music/map declarations against that same edition's
  original USER.CON and GRP. Record inherited absent declarations separately from
  unresolved addon resources; do not manufacture assets or delete declarations
  solely because a stock script mentions a file that its own game archive lacks.
- Original-era scripts can contain syntax tolerated by DOS engines but rejected
  by modern CON compilers, including stray punctuation in numeric parameters.
  A manifest/resource audit does not verify CON compilation. When adapting a
  reported failure, inspect the exact diagnostic and compare affected code with
  the matching original script. Apply the smallest supported correction and
  record before/after tokens and the engine revision outside addon.json.
  Separate fatal errors from inherited warnings: do not rewrite dangling else
  branches, action/define mismatches or intentional sound overrides merely to
  silence warnings, as that can change original gameplay. If a compiler check
  is requested, run it in an isolated directory with explicit base-game data,
  preserve its log, and distinguish compilation from gameplay testing. Keep
  excluded port/game checks untested unless the user requests that investigation.
- For EDuke32, the current BL builder supplies `con_main` with `-x` and mounts
  the prepared ZIP with `-g`; it does not run the author's manual installer.
  Keep required base-game files external and avoid inventing executable/RFF/GRP
  manifest fields. Bundled ports and BAT launchers are unnecessary here.
- Read `definevolumename`/`definelevelname` for episodes and map slots. CON music
  declarations can continue across lines; music selector 0 covers title/end
  tracks, with episode music in later selectors. Check active declarations after
  excluding comments. Do not validate Duke episodes as Blood INI sections.
- When a selected addon supplies only a custom campaign that replaces a stock
  episode, expose only its authored campaign episodes and rename the replaced
  episode to the addon title. Do not leave unrelated original campaigns in the
  episode menu. Preserve deliberately supplied multiple episodes and any
  explicitly intended access to original campaigns; establish episode roles from
  the source before changing the menu.
  Apply menu overrides after all active level/volume declarations and includes,
  so later stock declarations cannot recreate removed episodes. In EDuke32,
  [undefinevolume](https://wiki.eduke32.com/wiki/Undefinevolume) removes an
  episode's name and level entries; never apply it to a retained custom episode.
  For a single campaign replacing slot 0, remove unused slots 1 through 6 and
  finish with `definevolumename 0 EXAMPLE CAMPAIGN`. Keep custom map slot numbers,
  secret exits, level progression and music assignments unchanged. For other
  episode layouts, remove only unused slots and define the retained episode names
  in ascending slot order; do not assume the custom episode is always slot 0.
  Declare `dependencies.features: ["EDuke32_CON"]` when using `undefinevolume`,
  preserving other required features. Record exact edits and compiler evidence
  outside addon.json; compilation does not verify the rendered menu or gameplay.
- Keep native MAP and supported audio files when no transformation is required.
  Check Build map version, section counts/bounds and complete file length for a
  supported format. A maps/CON/audio source does not require PNG/DEF export.
  For an established family native ART/DEF profile, preserve ART byte-for-byte,
  checking headers, tile ranges/dimensions and pixel payload lengths; recognize
  optional BUILDART prefixes. Keep referenced texture subdirectories and rename
  game-reserved DEF names (including duke3d.def for Duke) to an addon-specific
  DEF with the correct main/module
  declaration. Do not impose Blood ART/DAT conversion rules on this family. GRP and
  custom-engine profiles still need source-specific preparation evidence.
- Separate bundled references from base-game resources: an absent stock VOC or
  title MIDI is not automatically a missing addon file. Verify stock membership
  against available game data; if unavailable, record that limitation. Check
  exact case for own files/includes, including main CON and custom MIDI/audio.
  Normalize active CON/DEF resource-reference tokens to the exact shipped
  spelling when needed, preserving comments, unrelated code and original inputs.
- Native version-9 maps use the binary TROR format; validate their sector/wall/
  sprite data with the supported layout and include `TROR` in required features.
  Do not copy an older manifest feature list without checking current maps. See
  [the EDukeWiki TROR guide](https://wiki.eduke32.com/wiki/True_Room_Over_Room_Mapping_Guide).
- Declare `EDuke32_CON` when the code genuinely requires EDuke32 extensions;
  feature filtering does not certify every advertised port can run the code.
  Treat renderer recommendations and game-edition requirements as source claims
  until tested. An Atomic-or-superior requirement does not justify silently
  claiming World Tour compatibility or forcing an unsupported edition constraint.
- Preserve author README, credits, map notes, PDF/manual and other required
  attachments. If a manual yields no extracted text, render its setup pages;
  embedded-image extraction can miss vector-drawn page content.
- Take the version from current author documentation, not an archive/mirror slug
  that may have been reused. For release_date, preserve the original release
  date for an ordinary update, or use the improved edition's release date for
  an improved edition. A latest changelog/update date is not a substitute.
  Prefer README evidence; otherwise establish an explicit author original-release
  date for the selected edition and record its provenance.
- Reuse suitable grid/preview images before generating new ones. Verify preview
  palette bytes against the selected game's documented palette source, with original ordering.
  Keep artwork validation separate from resource export and game execution.

## Large total conversions

Use [the large-TC skill](../../skills/buildlauncher-duke-large-tc/SKILL.md) when the
package combines a nested CON graph, main DEF, native ART/palette tables,
models/voxels/media and dynamic content. Compare full inventories after known
wrapper/entry-point normalization; preserve resource directories and establish
GRP/GRPINFO roles before choosing transformations. A selection-marker GRP is
not evidence of a packed resource archive. Trace active includes, classify
missing literals and retain script/line/declaration evidence without assuming
all declarations are used in playable content. Keep unresolved source gaps
visible rather than borrowing older assets or deleting definitions. If the user
explicitly accepts missing source resources, finish the package with declarations
unchanged and record that exception in its JSON evidence. Do not report the
missing-reference check as passed.

For binary Duke maps, validate structure and inspect v9 sector ceilingstat and
floorstat bit 1024 to confirm TROR extensions. Record map names/counts separately
from the version header. Simulated room-over-room effects are not proof of TROR;
see the [official sector flags](https://wiki.eduke32.com/wiki/Ceilingstat).
