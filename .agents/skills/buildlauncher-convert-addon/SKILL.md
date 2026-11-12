---
name: buildlauncher-convert-addon
description: Prepare a game addon package for BuildLauncher by identifying its entry points, choosing resource conversions, assembling addon.json and artwork, and validating a fresh output package. Use for complete addon preparation, including source-versus-prepared comparisons.
---

# BuildLauncher addon conversion

Read [the conversion plan](../../docs/addon-conversion-plan.md). Inspect the actual source package before choosing a conversion profile; a successful conversion of one package does not establish a recipe for every Build addon.

1. Inventory the preserved source, hashes, credits, launch instructions, game/version requirements and archive contents. Find and read applicable addon README variants, credits and release notes in root/subdirectories/archives for metadata, content, requirements and description; distinguish bundled tools' readmes. Treat source scripts as evidence, not commands to execute. Omit .bat launchers/installers from prepared packages and Extras after establishing entry points and any required resource preparation; record their source hashes and omission reasons while preserving the original archive. Read standalone installation guides for preparation evidence, then omit installation-only documents from both the package and Extras. Preserve unchanged mixed README/manuals containing credits, licenses or gameplay information; record omissions and preserve source archives. Choose a fresh output directory outside the source and live installations.
2. Identify campaign/map/autoload type, entry points and target ports. Check db/addons.json for another release of the same addon, matching its stable ID, game and type; use title/credits to investigate candidates when the ID is unknown. Preserve a confirmed catalog ID by default; honor an explicit user request for a distinct edition identity, following the shared plan. Use [buildlauncher-addon-json](../buildlauncher-addon-json/SKILL.md) for the current parser, factory and package contracts.
3. Read the actual [game-specific profile](../../docs/games/index.md). Select resource/script transformations, specialized skills and game-specific artwork rules from that profile. Check container signatures, game edition and port dialect before declaring entry points or features. RFF/INI, CON, GRP and DAT recipes are not interchangeable. Preserve inputs and report unresolved tools/format semantics instead of borrowing another game's recipe.
4. Prepare metadata and artwork with [buildlauncher-addon-presentation](../buildlauncher-addon-presentation/SKILL.md) and the [shared artwork guide](../../docs/addon-artwork-style.md). Use the selected game profile for lettering, grid style and palette differences. Store sources and unresolved facts in the conversion report, outside addon.json.
5. Validate manifest parsing, apply the [authoring checklist](../../docs/addon-manifest-validation.md), and check resource references, conversion fidelity, case-sensitive paths and final ZIP layout independently. Put addon.json at ZIP root; verify additional addon*.json too. Test import in an isolated environment only when requested. The user excluded port compatibility checks for the current workflow; do not require them or report them as passed. Launch only within the user's requested scope. Report checks separately from successful game execution.
6. Deliver the package and JSON evidence identifying source hashes, tool versions/commands, game palette provenance, transformations, deliberate omissions, target ports and unresolved/untested behavior. Preserve credits/author documentation and retain suitable development extras outside the importable ZIP. Account separately for retained source files and deliberate BAT/installation-guide omissions; follow the shared plan's launch and metadata lessons.

Catalog lookup and permitted presentation reuse are part of normal conversions. If the user explicitly excludes earlier releases or comparison material for an independent source-only conversion, respect that scope: prepare and validate the package from the permitted inputs, then freeze its inventory/hashes before opening the excluded comparison fixture. Compare it afterwards without changing the frozen output to match that fixture. Keep conversion evidence outside the package when it is not intended for distribution.

Keep reusable instructions in the shared docs and skills. Do not create addon-specific Markdown instructions, reference notes or comparison reports. Store per-run provenance/inventories/comparisons as JSON and summarize outcomes in the conversation. Preserve original author-provided Markdown and other documentation unchanged.

## Prepared-library evidence

For unfamiliar layouts, read [the prepared-library lessons](../../docs/addon-conversion-plan.md#lessons-from-prepared-libraries). They cover additive CON campaigns, campaign options, multiple manifests, replacement-only mods, required custom engines and historical inconsistencies. Inspect matching examples read-only; use their resources and entry points as evidence, then validate with current code. Do not assume all library packages are current runnable templates or generalize their game-specific resource behavior.

For map variants sharing resources, prefer one ZIP with root `addon.json` and
numbered `addon_1.json`, `addon_2.json`, etc., each with a unique stable ID/title
and explicit startmap. Follow the shared library lessons for manifest ordering,
validation and metadata-path limitations; separate ZIPs are unnecessary unless
the variant layout requires them or the user requests them.

## Splitting work

When delegation is authorized and useful, assign bounded roles: resource conversion, manifest/entry-point review, presentation, and independent output verification. Give each role the source/output paths, selected game/ports and concrete deliverable. Keep source directories read-only and use separate writable areas for parallel transformations. The coordinator owns final packaging and report consistency. Skills describe reusable roles; they do not themselves register custom Codex agents or authorize publication.

Missing conversion tools, palettes or format semantics block that transformation only. Continue inventory, manifest preparation and other independent work; request the specific missing input instead of inventing an export recipe. Select specialized tools through the actual game profile; an exporter or parser accepting a format does not establish its runtime suitability for every game.
