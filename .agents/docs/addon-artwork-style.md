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
preview.png at 320x200 for all types.

## Default for newly generated artwork

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
palette while borrowing only the pixel technique/layout of a related Build example;
record this fallback. Standalone projects keep their own identity and actual game
palette. Existing painted-cover examples in game profiles describe historical variety, not
the default direction for newly generated art under this preference.

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
the grid; the preview still requires indexed 320x200 output with unchanged palette
order. Check style, legibility and palette/dimensions independently at final size.

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

Campaign grids are usually covers; maps and mods need only previews. For a requested mod grid, adapt its feature showcase to portrait
using the appropriate game profile rather than inventing campaign content.

## Compose required artwork

- Produce preview.png at 320x200 for every type; produce grid.png at 600x900
  for TCs/campaigns and standalone games. A required grid may be RGB/RGBA;
  the preview must be indexed with the verified target-game palette. Follow
  [the presentation skill](../skills/buildlauncher-addon-presentation/SKILL.md)
  for source priorities, palette provenance and deterministic technical conversion.
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
- For preview title cards and screenshots with an added addon logo, center the
  logo horizontally and vertically and make it a prominent, readable foreground
  element. Avoid small corner titles. Keep the established letterforms and palette,
  preserve their aspect ratio and leave scene context visible around the title.
  A two-line logo may occupy roughly half the width and two-thirds of the height;
  adapt this starting point to the actual logo rather than stretching it. Preserve
  the background's original detail when moving or resizing the logo.
- Compose the preview independently for landscape. Keep title words, key emblems
  and the main subject in frame. A campaign title card is valid; a preview is not
  required to be a gameplay screenshot. When it is a screenshot, use actual addon
  content and identify generated artwork as an illustration.
- Preserve an existing release's visual identity. Add a version badge only when
  the addon already uses one or the user requests it; do not invent dates, credits,
  feature claims or branding from the cover.
- Use imagegen for newly generated raster art and artistic edits, following the
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
