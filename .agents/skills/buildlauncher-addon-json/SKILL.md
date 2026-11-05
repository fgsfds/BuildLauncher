---
name: buildlauncher-addon-json
description: Prepare, review, or update BuildLauncher addon.json manifests and addon package layouts using the repository's actual parser, factory, and port behavior. Use for addon conversion and manifest specification work, not general JSON editing.
---

# BuildLauncher addon manifests

Use this skill in the BuildLauncher repository when preparing an addon for BL, reviewing a manifest/package, or reconciling its specification with application code.

## Sources and contract

Read [the maintained specification](../../docs/addon.json-Specifications.md) for fields, examples, packaging, and known runtime limitations. Resolve repository paths from the checkout root. Check the relevant implementation before relying on a documented behavior if code has changed since the specification's reviewed commit:

- `src/Core.All/Serializable/Addon/AddonManifestJsonModel.cs` and adjacent models: serialized names, required fields, and serializer options.
- `src/Core.All/Serializable/JsonConverters.cs` and `src/Core.All/Enums/`: enum values and legacy game/executable mappings.
- `src/Addons/Addons/AddonFactory.cs`: usable addon types, campaign game coverage, executable resolution, and option construction.
- `src/Core.Client/Helpers/ManifestHelper.cs`, `src/Addons/Helpers/ArchivedAddonExtractor.cs`, and `src/Addons/Providers/InstalledAddonsProvider.cs`: main manifest location, extraction triggers, and folder discovery.
- `src/Addons/Helpers/AutoloadModsValidator.cs`, `src/Avalonia.Desktop/Helpers/PortsHelper.cs`, and `src/Ports/Builders/`: compatibility and command-line behavior.
- `src/Ports/Ports/PortStarter.cs`: executable selection and working directory.

`db/manifests.json` is the maintained manifest collection. `db/addons.json` is the downloadable catalog and `db/data.json` contains application/game data; they have different contracts. Do not copy catalog fields into an addon manifest. Serializer test fixtures prove parsing, not that their field combinations can become runnable addons.

For complete resource conversion, use [buildlauncher-convert-addon](../buildlauncher-convert-addon/SKILL.md); this skill owns manifest and package contracts. ART/SURFACE export details and presentation are in the dedicated resource and presentation skills.

## Preparing a conversion

- Find source-addon README variants case-insensitively in root/subdirectories/archives, plus credits and release notes; use applicable documentation for title, authors, release information, content, requirements and description. Reconcile it with catalog metadata, noting outdated headers or conflicts. Inspect launch scripts, active entry-point/includes, episode/map declarations and nested containers according to the selected game profile. Derive the game, entry points, required features, and package type from these sources. A user calling something a mod does not necessarily mean BL's autoload `mod` type.
- Work from a preserved source copy into a fresh staging/output directory. Validate archive entries before extraction: reject paths escaping that directory, rooted paths, and links that could escape it; check case collisions for Linux output. Do not use BL's live installer as a safe converter: its current extraction/cleanup can replace existing content and delete the source ZIP on failure.
- Check db/addons.json for an existing release of the same addon and preserve its confirmed stable ID by default, applying the explicit edition exception below. Catalog Description can supply manifest description, and earlier package artwork can be reused via the presentation skill. Catalog DownloadUrl, FileSize, Sha256 and other catalog-only fields do not belong in addon.json.
- Keep IDs stable across releases unless the user explicitly requests a distinct edition identity. Apply any edition-specific naming convention through the selected game profile. Use the source edition's actual version even if it is lower than the original catalog version; record the original identity separately. Preserve actual credits, release versions, and requirements. If a source fact is unavailable, omit optional metadata or record the uncertainty outside the manifest; do not invent a source release number. The parser permits an empty version, but the developer authoring validator requires a nonblank release version. For an unversioned source, record this limitation and obtain a version policy rather than inventing a release number.
- For ordinary updates, retain the original addon's `release_date`; improved editions of older addons such as BHSE use the improved edition's release date. Prefer README or previously verified edition-specific evidence; record provenance and do not substitute latest update dates or catalog UpdateDate.
- Emit the exact serialized property names, canonical enum strings, and relative `/` paths matching file case. Unknown workflow metadata belongs in separate JSON conversion evidence, not addon-specific Markdown instructions or addon.json fields. Use a root `addon.json` and no ZIP wrapper directory; validate every additional `addon*.json`.
- Declare feature requirements from the prepared resources: a generated tilefromtexture DEF requires `dependencies.features: ["TileFromTexture"]`. The exporter does not infer or write this manifest field.
- Read the selected [game-specific profile](../../docs/games/index.md) before choosing resource/script fields. RFF entry points are Blood-only, CON main/modules apply only to supported game/port dialects, and native GRP support depends on the actual game/port. A parsed field or authoring exception does not establish a valid resource recipe. Keep per-game INI, reserved-script renames, native ART/DAT and container conversion instructions in that profile.
- Choose file-based `startmap` for map packages. Keep autoload mods in ZIPs and use module fields rather than `def_main`. Check extraction triggers before selecting the output layout. Choose container retention, mounting and conversion through the game profile; no universal resource conversion follows from the JSON model.
- Use explicit OS -> port -> path executable mappings only when needed. Check the current Windows-only executable selection before promising Linux support. Check script/resource lookup from the executable's working directory.

## Variants and optional content

Use [the shared library lessons](../../docs/addon-conversion-plan.md#lessons-from-prepared-libraries) when a source offers soundtrack/graphics choices, dependent expansion patches or several mods sharing assets. Preserve verified module order and independent IDs; audit every manifest and option path. Options do not enforce exclusive selection and are discarded for autoload mods. Historical executable shorthand and game aliases are accepted legacy data, not preferred output templates; use canonical names and explicit current mappings.

## Validation and delivery

Read [the shared authoring validation](../../docs/addon-manifest-validation.md). Use the repository CLI: `dotnet run --project src/Addon.ArtExporter -- validate-addon --input PACKAGE_DIRECTORY [--manifest addon-extra.json]`. It parses with the real context and runs AddonManifestValidator, the rules shared with DevViewModel.GetAddonJson. Run it for every generated manifest against its actual prepared root filenames; do not rely on serializer acceptance alone.

Deserialize each final manifest using `AddonManifestJsonContext.Default.AddonManifestJsonModel` with the repository's .NET runtime. Also verify required resource paths, referenced includes/INI maps, uniqueness of option names and dependency IDs, addon-factory constraints, and the actual final ZIP/folder layout. The current workflow excludes port compatibility checks; record them as untested. Do not add a `$schema` property to manifests: the current parser rejects it.

Run the build and affected unit project(s) for repository changes, following `AGENTS.md`. Use `Tests.Unit` for parser/pure checks and `Tests.Unit.Sequential` for factory, archive, and UI tests requiring the existing headless/static-state setup. Keep import validation in a disposable isolated installation. Launch games only when requested; otherwise report parsing, package checks, and argument verification separately from a successful game launch.

Deliver the prepared package/manifest and JSON conversion evidence with input provenance, transformations, target game/port, validated checks, unresolved requirements, and untested OS/port combinations. A schema or JSON linter is supplementary and does not prove runtime compatibility.

For documentation work, keep the local specification and the wiki text aligned when wiki publication is requested. Link implementation claims to a reviewed commit, retain historical changelog entries, and distinguish parser acceptance, runtime behavior, and recommended packaging. Updating documentation does not authorize unrelated launcher fixes, catalog/S3 uploads, or publishing converted addons.
