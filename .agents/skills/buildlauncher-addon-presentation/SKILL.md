---
name: buildlauncher-addon-presentation
description: Prepare BuildLauncher addon grid and preview artwork and a sourced description during addon conversion. Use for addon package presentation, not game-resource conversion or general image editing.
---

# BuildLauncher addon presentation

Use alongside [the manifest skill](../buildlauncher-addon-json/SKILL.md). Deliver root presentation images and manifest description text, with their provenance in the conversion report. Presentation choices do not establish game, port, or operating-system compatibility.

## Choose artwork and description

Use images and description supplied by the user when provided. Inspect their content and dimensions before adapting them; preserve their source files. Do not silently replace supplied artwork with a generated alternative. Keep supplied factual wording unless the user requests editing or verification; flag unsupported claims separately.

Before searching or generating missing presentation, follow the catalog reuse procedure below. When no suitable catalog material is available, search the internet for the addon's own page, release notes, credited authors, screenshots, and permitted artwork, or generate suitable artwork with the imagegen skill/tool. Prefer a real addon screenshot or author-provided image when it identifies the package accurately. Do not present generated illustrations or mockups as gameplay screenshots. Record which output is a screenshot, adapted artwork, or generated illustration in the report, together with available source and attribution information.

Catalog descriptions may be reused as maintained metadata; record their catalog origin rather than presenting their historical claims as newly independently verified. Revise obsolete version-specific statements to fit the supplied release.

A supplied release-directory label can establish the working version when documented as its source; an INI generation timestamp does not establish a release date. Prefer individually credited creators when the supplied credits identify them. Do not copy arbitrary comparison-fixture publication history without evidence; reuse from the maintained catalog follows the provenance rule above.

Write a concise description from source evidence: what the addon contains, campaign/map structure, credited creators, and meaningful requirements or known limitations. Separate supplied facts, verified external facts, and inference. Do not infer a release date, release version, port support, or feature dependency from the image. Keep factual source URLs, file references, and unresolved metadata questions in the conversion report outside addon.json. Use only the manifest's supported description field for display text; do not invent image or provenance properties.

## Reuse an existing release

Check `db/addons.json` during every conversion, even if the supplied source has no
manifest. It is a game-keyed catalog with PascalCase fields (`Id`, `Game`,
`AddonType`, `Title`, `Version`, `Description`, `DownloadUrl`, `Sha256`), not the
lowercase addon.json contract. Match a known ID case-insensitively and confirm the
game/type and source identity. A similar display title alone is not enough. If no
ID is supplied, use title and credits to investigate candidates and preserve a
confirmed existing ID across releases.

When another version of the same addon is present, reuse its suitable description
and artwork before finding or generating replacements. Prefer the latest suitable
earlier release using the repository VersionComparer rather than alphabetical
version ordering or UpdateDate. Reuse each available item separately; a missing
preview does not invalidate a usable description or grid. User-supplied images and
text still take priority. Follow an explicit source-only restriction when one is
part of the user's current request.

- Copy catalog Description into manifest description, adapting obsolete version,
  map-count, feature or compatibility statements using the current source. Keep
  the target release version and its current resource declarations; do not copy
  the old manifest wholesale or treat UpdateDate as a release date.
- The catalog has no PNG paths or embedded images. Obtain the matched release's
  package from an already available verified archive or its DownloadUrl, in fresh
  staging. Verify its Sha256 when supplied. Read root grid.* and preview.* entries
  without importing through BL or replacing an installation; validate archive
  paths before extracting. Missing or inaccessible artwork falls back to the
  other presentation sources, with the limitation recorded.
- Reuse suitable PNGs directly when they already meet the output conventions.
  If an older grid uses JPEG or has different dimensions, apply the authorized
  technical format/resize conversion to produce grid.png. Validate reused preview
  mode, dimensions and the exact target-game palette; perform the required
  technical conversion when necessary. Inspect both outputs and avoid retaining
  ambiguous duplicate grid/preview stems.
- Record the catalog ID/version, archive URL or portable source filename, available
  archive hash, reused image hashes and any text/image adaptations outside
  addon.json. Distinguish catalog-derived metadata from newly verified facts.

This workflow reads the catalog and earlier package; it does not update catalog
entries, upload content or publish the converted release.

## Prepare the package images

Obtain an explicit verified target-game palette from user-provided resources or the game's data. A full RGB ACT contains 768 palette bytes; verify any optional footer and record the actual hash. Known Blood palette fingerprints are recorded in [the shared exporter guide](../../docs/art-exporter.md#palette-validation); no palettes are bundled in this repository. File existence alone does not validate a palette, and a filename does not establish its game mapping.

- Prefer PNG for both outputs. Use grid.png at exactly 600 x 900 pixels, and preview.png at exactly 320 x 200 pixels. These dimensions and PNG preference are the user's conversion convention, not requirements enforced by the BL manifest parser.
- The preview must be an indexed PNG using the target game's palette. The grid has no required palette restriction.
- Identify the target game first and obtain its actual palette from a verified game resource or user-supplied palette/tool. Record the source and a hash. Do not approximate the palette from a screenshot, substitute a visually similar palette, or assume every Build game uses Blood's palette. If it is unavailable, continue choosing artwork and drafting text, and request the palette or extraction instructions before declaring preview complete.
- Choose a crop that preserves the subject and legible title. Resize without changing the intended content; do not stretch a landscape image into portrait artwork. If an appropriate crop loses essential content, use a considered composition or padding. Preserve pixel-art edges when appropriate and record the actual crop and resampling choice rather than assuming a universal method.
- Use imagegen for generation and ordinary artistic edits. It does not guarantee exact dimensions, PNG indexed mode, or an exact game palette. Exact resizing and palette quantization require deterministic processing and verification. Use an available project conversion tool; Pillow may be used for the technical image operation when the user has explicitly requested resizing, indexed-palette conversion, or the corresponding technical processing. Such authorization may already be present in the conversation; do not ask again. Otherwise obtain the needed technical instructions/tool before using Python to edit images.
- Apply the verified palette without silently optimizing or reordering its indices. Preserve its documented index/color ordering; make any transparency and dithering decisions explicit. Do not assume a transparency index merely because a different game resource uses it. Preserve source artwork and generate outputs in fresh staging.

## Verify and hand off

Decode both final images and verify their format and exact dimensions. Verify that preview.png is indexed (PNG color type 3 / Pillow mode P), that its palette entries and index ordering match the selected source palette, and that every pixel index is valid. A true-color PNG showing similar colors is insufficient. Inspect both final images visually after resizing and palette conversion for crop errors, distorted aspect ratio, illegible text, and unintended color or transparency changes.

Put the images at the addon root with the manifest and resources. BL discovers root grid.* and preview.* files; it has no thumbnail-path fields. Avoid ambiguous duplicate files sharing either stem unless the package intentionally supports them and discovery behavior has been checked.

In JSON conversion evidence, record input image paths/hashes or source URLs, artwork type, description sources, target game and palette provenance, transformations and tools, and completed or unresolved checks. Keep original author credits and redistribution notices with the addon. Do not claim a successful game launch from presentation validation.
