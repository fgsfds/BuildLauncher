# Addon grid and preview style

Use these shared rules when creating new addon artwork. Preserve supplied artwork
and suitable images from an existing release before creating replacements. The
visual target is the prepared-addon library's style for the same game and addon
type, together with the addon's own identity. This guide is self-contained; a
specific external reference directory is not required.

## Required artwork by addon type

- TCs/campaigns and standalone games require grid.png and preview.png.
- Maps and mods require only preview.png. Do not create a grid by default or
  treat its absence as incomplete preparation. Create one only if explicitly requested.

This shared type rule applies to every game profile. Existing grid files do not
make grids mandatory for maps/mods. These are preparation conventions, not new
manifest parser requirements. Use grid.png at 600x900 when required and
preview.png at 320x200 for all types. Grids may be RGB or RGBA. Previews
must be opaque indexed PNGs using the verified target palette in its original
order. See [palette acquisition](../skills/buildlauncher-addon-presentation/SKILL.md#prepare-the-package-images).

## Default for newly created previews

Preserve suitable existing author previews unless replacement is requested.

**Map addons:** When a preview is missing or unusable, always render a top-down
2D layout from the actual playable map, even if grid or sprite artwork is
available. For a package with several maps, use the manifest's declared start map
or another verified entry map if the manifest does not specify a file. Draw walls
and sector connections, preserve proportions, and mark the player start when it
is valid and in frame. Label partial views as the main area. Record the source
map path, hash and rendered scope. If the map cannot be identified or decoded,
report the blocker instead of substituting unrelated artwork.

**Other addon types:** Choose the source in this order:

1. Use an available addon grid. Crop to fill the entire 320x200 landscape frame,
   preserving aspect ratio, without letterboxing, stretching or solid padding.
   When the grid contains the addon's logo/name, frame it in the center of the
   preview and keep it prominent. Inspect the full title and logo bounds; avoid a fixed
   top-third crop that clips lettering. If the complete logo cannot fit or be
   centered within a valid crop, place the original logo over a crop of the same
   grid, preserving all lettering, colors and proportions. Verify any mask at
   native size so it does not erase glyphs. Preserve the original grid unchanged.
2. If no usable grid is available, use a spritesheet. An existing sheet or an
   atlas assembled from the addon's actual shipped sprite/tile images is valid;
   keep whole frames and intentional pixel edges. Do not invent sprite art or
   substitute a map diagram, sound waveform or voxel rendering for a spritesheet.
3. If neither source is usable, use the complete addon title
   centered on one solid background color, with balanced line breaks, readable
   contrast and comfortable margins. Do not add unverified labels or claims.

A map schematic is a preview fallback only; do not use it as grid artwork.
Do not use waveforms or voxel renderings as preview fallbacks.
When replacing previews made during a repair run, keep unrelated original artwork
and source grids. Record source hashes, crop rectangles, visible logo/title bounds
when identified, sprite/tile IDs and the reason for any fallback in JSON evidence.
Deterministic cropping, sprite-sheet assembly, map schematics, text rendering
and exact palette conversion are appropriate technical operations. Use imagegen
for illustrations and artistic edits.

## Default for newly generated artwork

This section describes illustrated grids and explicitly requested illustrations.
For previews, use the [shared source order](#default-for-newly-created-previews).

The user's preference is the prepared library's old-school 8-bit Build Engine
look. For new illustrations, favor coarse indexed-game textures, hard square
pixels, jagged sprite edges, simple sector geometry, flat lighting and short color
ramps. Use plain readable titles and restrained sprite/emblem compositions where
appropriate. Do not default to realistic painted box art, cinematic characters,
photographic materials, smooth bloom or modern rendering. Preserve suitable
existing author artwork unless the user asks for new images or another style.

Inspect actual library references and pass the relevant images to imagegen;
describing their style from memory is insufficient. Select examples through the
actual game's [artwork profile](games/index.md).
Use a reference for its visual technique, not another addon's title or setting.
When the user asks to ignore the selected addon's old grid/preview, use other
addons as style references and generate the assets required for that addon type anew.

For games without suitable same-game examples, keep the game's subject and verified
palette. Start with author artwork or an actual addon scene and its branding.
Borrow only the pixel technique or layout of a related Build example and record
the fallback without calling it an established style for the target game.
Historical painted covers can be reused when suitable; new illustrations follow
the style guidance above.

A prompt saying only "8-bit" can still produce realistic painted artwork. Specify
logical low resolution, visible pixels, coarse game textures, simple shading and
pixel title edges explicitly. If a draft is too realistic, regenerate it against
references; palette quantization or pixelating a photograph does not by itself
match the requested game-art style. Label generated game-like scenes as illustrations,
not gameplay screenshots.

When a grid is required, compose portrait and landscape separately. Optional technical processing for a
coarse grid is a 200x300 logical bitmap enlarged 3x with nearest neighbor to 600x900;
this is a style choice, not a mandatory resolution for every source image. Use
nearest neighbor when it preserves intentional pixel edges, avoid stretching and
check lettering after resizing. A verified fixed game palette can also be used for
the grid.

## Reviewed examples

On 2026-10-05, the supplied prepared-addon library was inventoried and all 233
images were visually reviewed through contact sheets: 186 grids and 47 previews.
Every grid was 600x900 and every preview was 320x200. Most previews were indexed;
one Blood RGB preview and one Duke3D RGBA preview are historical exceptions,
not templates for new files. PNG output and the exact verified game palette
remain the current preparation rules. Many older grids use JPEG; match their
appearance while producing grid.png.

| Game / reference family | Grids | Previews | Strength of evidence |
|---|---:|---:|---|
| Blood | 35 | 32 | Many campaign covers/title cards and several mod showcases |
| Duke Nukem 3D / Duke3D | 138 | 12 | Strong common campaign layout; author covers and map/mod variants |
| Shadow Warrior / Wang | 9 | 1 | Several covers, one preview; limited preview evidence |
| Exhumed / PowerSlave / Slave | 1 | 0 | One cover; no observed preview style |
| Standalone | 3 | 2 | Distinct project identities; no universal standalone theme |
| Ion Fury / Fury | 0 | 0 | No grid/preview examples in the reviewed library |
| NAM | 0 | 0 | No grid/preview examples in the reviewed library |
| Redneck Rampage / Rides Again / Redneck | 0 | 0 | No grid/preview examples in the reviewed library |

Counts include multiple releases of some addons. They measure available images,
not unique designs or a requirement to produce both assets for every addon.

## Match the game and addon

Before creating artwork, identify the actual game and package type from the source
and manifest. Read the applicable source README, credits and release notes for the
addon's setting, title, content and author identity. Choose representative artwork
for the same game and, when available, the same type: campaign, map or resource mod.
Inspect at least two references when available. An existing release of the same
addon is the strongest visual reference; record its identity and any adaptations.

Use the selected game profile as a default for new work. Keep an established custom
logo, official cover or total-conversion identity when it fits the addon; the
library itself contains those exceptions. Match layout, typography, contrast,
palette mood and retro texture, rather than copying another addon's title,
characters or map scene into an unrelated package.

Game-specific typography, subjects, palette identity and reference families live
in [the game profiles](games/index.md), each under its Artwork section. Read only
the actual game's profile; a style observed for one game is not another game's
branding. The profile index labels games with limited/absent reviewed examples.

For a requested mod grid, adapt the mod’s actual feature showcase to portrait
using the appropriate game profile.

## Compose required artwork

- For illustrated grids, preserve the subject, title and aspect ratio. If a crop
  would lose essential content, compose a new layout or pad the grid. Record the
  crop and resampling method. Grid-derived previews follow the crop-to-fill rule.
- Give the grid one clear title/subject hierarchy and a readable silhouette at
  thumbnail size. Preserve meaningful scene detail and the character of game
  textures. Prefer the selected references' typography and contrast to a generic
  modern app-card layout. No exact font file was identified by the audit; use a
  verified author logo or match the observed letterforms without claiming an
  unverified font identity.
- When positioning an addon logo on a grid, center it within the upper third
  of the image: horizontal center at half the width and vertical center at one
  sixth of the height. For 600x900, this is (300,150). Use the logo's visible
  bounds rather than the full canvas or transparent padding. Preserve its size
  and background detail when only placement is requested.
- For title-only image edits, preserve the existing background resolution,
  textures, colors, framing, and scene. Pixelate the title independently; restore
  any background pixels changed outside the intended edit area.
- Preserve an existing release's visual identity. Add a version badge only when
  the addon already uses one or the user requests it; do not invent dates, credits,
  feature claims or branding from the cover.
- Use imagegen for illustrated raster art and artistic edits, following the
  presentation skill. Technical dimension/format/palette processing is separate.
  Validate the actual final indexed preview after quantization, because colors,
  thin text and small outlines can change.

## Verify and record

Inspect every required final image at native size and thumbnail size, alongside the chosen
references. Check title spelling, contrast, crop, aspect ratio, pixel-art edges,
readability and whether all supplied images clearly identify the same addon. Verify PNG
format, exact dimensions, preview indexed mode and exact palette separately.

In the run's JSON evidence, record game/type, selected profile, reference addon
IDs/titles and available image hashes, supplied/reused/generated status, crops and
technical processing. Distinguish observed style from an inferred fallback and
record limited/missing references. Do not add addon-specific Markdown style notes
or put visual provenance into addon.json. Do not bundle the reference collection
or change its original files as part of this workflow.
