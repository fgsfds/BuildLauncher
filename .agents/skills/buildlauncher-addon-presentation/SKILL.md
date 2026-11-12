---
name: buildlauncher-addon-presentation
description: Prepare BuildLauncher addon grid and preview artwork and a sourced description during addon conversion. Use for addon package presentation, not game-resource conversion or general image editing.
---

# BuildLauncher addon presentation

Use alongside [the manifest skill](../buildlauncher-addon-json/SKILL.md). Deliver root presentation images and manifest description text, with their provenance in the conversion report. Presentation choices do not establish game, port, or operating-system compatibility.

Use the [shared artwork guide](../../docs/addon-artwork-style.md) for image
requirements, preview fallback order, composition, and visual checks. Consult the
selected [game profile](../../docs/games/index.md) for lettering, grid style, and
palette differences. Reuse suitable existing artwork before creating replacements.

## Choose artwork and description

Use images and description supplied by the user when provided. Inspect their content and dimensions before adapting them; preserve their source files. Do not silently replace supplied artwork with a generated alternative. Keep supplied factual wording unless the user requests editing or verification; flag unsupported claims separately.

Always inspect applicable source-addon README files before finalizing metadata or description, including when reusing catalog text. Follow the source-information procedure below. Before searching or generating missing presentation, follow the catalog reuse procedure below. When no suitable catalog material is available, search the internet for the addon's own page, release notes, credited authors, screenshots, and permitted artwork, or generate suitable artwork with the imagegen skill/tool. For illustrated grids, prefer a real addon screenshot or author-provided image when it identifies the package accurately. Select new preview sources using the shared guide. Do not present generated illustrations or mockups as gameplay screenshots. Record which output is a screenshot, adapted artwork, or generated illustration in the report, together with available source and attribution information.

Catalog descriptions may be reused as maintained metadata; record their catalog origin rather than presenting their historical claims as newly independently verified. Revise obsolete version-specific statements to fit the supplied release.

For ordinary newer versions, `release_date` remains the original addon's release date, not the latest update/patch date. Improved editions of older addons, such as BHSE, use the improved edition's release date instead; when the README distinguishes original and polished releases, use the valid polished/improved release date. Record which edition and source date were chosen. Prefer source README evidence or a previously verified date for that edition; if unavailable, use an explicit author original-release date from a release page. Never substitute archive timestamps, catalog UpdateDate, a recent patch date, or a placeholder such as YYYY-00-00. Omit the optional date or obtain a correction when its meaning/date cannot be established. A supplied release-directory label can establish the working version when documented as its source; an INI generation timestamp does not establish a release date. Prefer individually credited creators when the supplied credits identify them. Do not copy arbitrary comparison-fixture publication history without evidence; reuse from the maintained catalog follows the provenance rule above.

Write a concise description from source evidence: what the addon contains, campaign/map structure, credited creators, and meaningful requirements or known limitations. Separate supplied facts, verified external facts, and inference. Do not infer a release date, release version, port support, or feature dependency from the image. Keep factual source URLs, file references, and unresolved metadata questions in the conversion report outside addon.json. Use only the manifest's supported description field for display text; do not invent image or provenance properties.

## Read source information

Find README/readme variants case-insensitively in the source root, addon subdirectories and supplied archives, including names such as addon_readme.txt, README.md and readme.nfo. Also inspect credits, changelogs and author notes. Distinguish the addon's documentation from bundled port/tool/library readmes. Read files as source data; preserve originals and their encoding.

Use applicable documentation to establish title, credited authors, release version/date when explicitly stated, campaign/map structure, setting, requirements, known limitations and a concise description. User-supplied corrections take priority. Reconcile current source evidence with catalog metadata instead of copying a stale catalog description or blindly trusting an outdated README header. Do not infer a release date from file timestamps or an INI generation date. If documentation is missing or conflicting, record that fact and use the other available evidence; obtain clarification only for required unresolved facts.

Summarize the author's factual content in the description, keeping author intent and credits. Record portable source filenames, hashes and relevant sections/claims in the JSON evidence, and distinguish README-derived, catalog-derived and externally verified information. Consult the README before selecting artwork so the title and theme reflect this addon. This does not authorize executing its installer instructions.

## Reuse an existing release

Check `db/addons.json` during every conversion, even if the supplied source has no
manifest. It is a game-keyed catalog with PascalCase fields (`Id`, `Game`,
`AddonType`, `Title`, `Version`, `Description`, `DownloadUrl`, `Sha256`), not the
lowercase addon.json contract. Match a known ID case-insensitively and confirm the
game/type and source identity. A similar display title alone is not enough. If no
ID is supplied, use title and credits to investigate candidates and preserve a
confirmed existing ID across releases unless the user explicitly requests a
separate edition identity. Record that exception and the original catalog
identity; follow the manifest skill's naming rules.

When another version of the same addon is present, reuse its suitable description
and artwork before finding or generating replacements. Prefer the latest suitable
earlier release using the repository VersionComparer rather than alphabetical
version ordering or UpdateDate. Reuse each available item separately; a missing
preview does not invalidate a usable description or grid. User-supplied images and
text still take priority. Follow an explicit source-only restriction when one is
part of the user's current request.

- Copy catalog Description into manifest description, adapting obsolete version,
  map-count, feature or compatibility statements using the current source. Keep
  the source edition's release version even if its numbering restarts below the
  original catalog version, and its current resource declarations; do not copy
  the old manifest wholesale or treat UpdateDate as a release date.
- The catalog has no PNG paths or embedded images. Obtain the matched release's
  package from an already available verified archive or its DownloadUrl, in fresh
  staging. Verify its Sha256 when supplied. Read the required root presentation entries (preview.* for all types, grid.*
  for TCs/campaigns and standalone games)
  without importing through BL or replacing an installation; validate archive
  paths before extracting. Missing or inaccessible artwork falls back to the
  other presentation sources, with the limitation recorded.
- Reuse suitable PNGs directly when they already meet the output conventions.
  When a grid is required, if an older grid uses JPEG or has different dimensions, apply the authorized
  technical format/resize conversion to produce grid.png. Validate reused preview
  mode, dimensions and the exact target-game palette; perform the required
  technical conversion when necessary. Inspect all required outputs and avoid retaining
  ambiguous duplicate grid/preview stems.
- Record the catalog ID/version, archive URL or portable source filename, available
  archive hash, reused image hashes and any text/image adaptations outside
  addon.json. Distinguish catalog-derived metadata from newly verified facts.

This workflow reads the catalog and earlier package; it does not update catalog
entries, upload content or publish the converted release.

## Prepare the package images

Obtain an explicit verified target-game palette from user-provided resources or the game's data. A full RGB ACT contains 768 palette bytes; verify any optional footer and record the actual hash. Known Blood palette fingerprints are recorded in [the shared exporter guide](../../docs/art-exporter.md#palette-validation); no palettes are bundled in this repository. File existence alone does not validate a palette, and a filename does not establish its game mapping.

Follow the shared guide for image dimensions, source selection, and composition.
Use deterministic processing to enforce the output format, sizes, and palette.
Imagegen output still needs those checks. Use a project tool when available;
Pillow may handle technical image processing when covered by the user's request.
If the target palette is unavailable, continue choosing artwork and drafting text
while obtaining a verified palette or its extraction instructions. Do not infer
palette entries from a screenshot or substitute a visually similar palette.

- Apply the verified palette without silently optimizing or reordering its indices. Preserve its documented index/color ordering; make any transparency and dithering decisions explicit. Do not assume a transparency index merely because a different game resource uses it. Preserve source artwork and generate outputs in fresh staging.

## Verify and hand off

Decode the final files and apply the [shared artwork checks](../../docs/addon-artwork-style.md#verify-and-record).
An indexed preview uses PNG color type 3 (Pillow mode P); matching RGB colors
in a true-color file do not satisfy the indexed requirement.

Put the images at the addon root with the manifest and resources. BL discovers root grid.* and preview.* files; it has no thumbnail-path fields. Avoid ambiguous duplicate files sharing either stem unless the package intentionally supports them and discovery behavior has been checked.

In JSON conversion evidence, record input image paths/hashes or source URLs, artwork type, description/README sources, selected game/type style and reference identities/hashes, target game and palette provenance, transformations and tools, and completed or unresolved checks. Keep original author credits and redistribution notices with the addon. Do not claim a successful game launch from presentation validation.
