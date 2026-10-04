# AGENTS.md

Instructions for AI coding agents working in the **BuildLauncher** repository.

## Repository language

Write all new and edited repository content in English, including documentation,
skills, agent instructions, code comments, identifiers, and commit messages.
Preserve original third-party names, quotations, and localized resource text when
translation would change their meaning or behavior. Conversation with the user may
remain in their preferred language.
Keep documentation self-contained: use repository-relative references and generic
input/output examples, never machine-specific paths to external local files or folders.

## Project overview

BuildLauncher is a cross-platform (Windows + Linux) desktop frontend for Build Engine
games (Duke Nukem 3D, Blood, Shadow Warrior, Redneck Rampage, Ion Fury, etc.). It
detects installed games, downloads source ports, tools, mods, campaigns and maps, and launches them with
configured command-line arguments.

- UI: **Avalonia** desktop app (Fluent theme), MVVM via **CommunityToolkit.Mvvm**.
- Runtime: **.NET 10**, nullable enabled, implicit usings, file-scoped namespaces.
- Persistence: **SQLite** via EF Core.
- Networking uses `HttpClient` for GitHub releases, the addon catalog and remote
  assets. `--offline` uses local metadata when the JSON files can be resolved.
- Content DB: versioned JSON files under `db/` (`addons.json`, `data.json`, `manifests.json`).

## Build, run, test

.NET 10 SDK is required by the target framework. `global.json` currently selects only
the test runner; it does not pin an SDK version. Central package management is on (`Directory.Packages.props`); never hard-code
package versions in a `.csproj`.

```pwsh
dotnet restore
dotnet build                          # builds BuildLauncher.slnx
dotnet run --project src/Avalonia.Desktop
```

Tests use **xunit v3** on the Microsoft.Testing.Platform. `global.json` selects that
runner, so use the `--project` form (not a bare project path):

```pwsh
dotnet test --project ./src/Tests.Unit/Tests.Unit.csproj --no-build
dotnet test --project ./src/Tests.Unit.Sequential/Tests.Unit.Sequential.csproj --no-build
dotnet test --project ./src/Tests.External/Tests.External.csproj --no-build
```

- `Tests.Unit` runs in parallel. Keep pure/unit-style tests here.
- `Tests.Unit.Sequential` disables parallelization (`xunit.runner.json`) because it
  hosts **Avalonia headless** UI tests and tests touching shared static state. Put any
  test that must not run concurrently here.
- `Tests.External` and `Tests.Database` hit real external services.
- `Tests.Database` validates `db/*.json` against the S3 bucket and requires
  `MINIO_ACCESS_KEY` and `MINIO_SECRET_KEY` environment variables. It is not expected to
  pass locally without those secrets; CI runs it separately on changes to `db/**`.
- `Benchmarks` uses BenchmarkDotNet and is not part of the test suite.

Always run the affected unit test projects plus a build after a change. There is no
separate lint command: analyzers run on build.

## Repository layout

```
src/
  Core.All          Domain enums, JSON models, Result types, release-provider base,
                    version/OS helpers. No project references (pure shared layer).
  Database.Client   EF Core DatabaseContext + DbEntities + Migrations.
  Core.Client       Client services: config, logging, HTTP/GitHub/offline API,
                    downloaders, archive tools, caches, playtime/rating providers.
  Games             BaseGame + concrete games, games paths/provider.
  Addons            BaseAddon + campaign/map/mod types, addon factory, extractor,
                    installed & downloadable addon providers, metadata provider.
  Ports             BasePort + concrete ports, installer, cmd-argument builders,
                    PortsProvider, PortStarter, process runner.
  Tools             BaseTool + editing/utility tools, tool installer.
  Addon.ArtExporter Standalone .NET CLI for ART/SURFACE export and manifest validation.
  S3                S3 file uploader and utilities.
  Avalonia.Desktop  App entry point, DI composition root, views, view models.
  Benchmarks        BenchmarkDotNet harness.
  Tests.*           Test projects (see above).
db/                 Versioned addon catalog, game data and manifest collection.
.agents/docs/       Maintained addon specifications and conversion documentation.
.agents/skills/     Repository-local addon preparation skills and reference checks.
```

Project references (never introduce a cycle that reverses these):

```
Core.All          -> (none)
Database.Client   -> Core.All
Core.Client       -> Core.All, Database.Client
Games             -> Core.All, Core.Client
Addons            -> Core.All, Core.Client, Games
Ports             -> Core.All, Games, Addons
Tools             -> Core.All, Games
Addon.ArtExporter -> Core.All
S3                -> Core.Client
Avalonia.Desktop  -> Core.All, Games, Ports, Tools, S3
Benchmarks        -> Core.Client, Addons
```

`Ports` and `Tools` are the only projects that register concrete port/tool
implementations; the UI depends on their abstractions.

## Architecture essentials

### Composition root and DI

- `src/Avalonia.Desktop/App.axaml.cs` is the single composition root. `LoadBindings()`
  builds a `ServiceCollection` and registers services through `With*` extension methods,
  then validates the provider with `ValidateOnBuild`/`ValidateScopes`.
- Every subsystem exposes an `IServiceCollection With...()` extension:
  `WithGames`, `WithPorts`, `WithTools`, `WithAddons`, `WithS3FilesUploader`,
  `WithClient`, `WithMVVM`, `WithBitmapsCache`, `WithConfig`/`WithFakeConfig`,
  `WithDatabase`, `WithHttpClients`/`WithFakeHttpClients`,
  `WithGitHubApi`/`WithOfflineApi`, `WithFileLogging`, `WithDebugLogging`. **When you add a service, register it in the
  owning `With...` helper, not in `App`.**
- Concrete ports and tools are registered as ordinary base-type singletons:
  `AddSingleton<BasePort, EDuke32>()`, `AddSingleton<BaseTool, Mapster32>()`.
  Consumers receive `IEnumerable<BasePort>` / `IEnumerable<BaseTool>`; these registrations
  are not keyed services.
- Two runtime modes are selected in `Program.Main`: `--dev` (developer mode) and
  `--offline` (offline mode). Offline swaps in `OfflineApiInterface` and fake HTTP
  clients. Design mode swaps in `ConfigProviderFake`. Keep both branches working.
- Bitmaps are cached through keyed services (`KeyedServicesEnum.Bitmaps`) exposing
  `ICacheGetter<Bitmap>` / `ICacheAdder<Stream>`; resolve with `[FromKeyedServices]`.

### Domain model

The three core abstractions are parallel in shape:

- `Games.Games.BaseGame` — install detection via required files, skills, addon folder
  paths, cached addon-folder detection and cached base-game install status.
  `InvalidateAddonsCache()` resets both cached results.
- `Addons.Addons.BaseAddon` — an immutable, `required`-initialized description of a
  campaign (`TC`), map, mod (`AutoloadMod`), or official addon. Identity is `AddonId`
  (manifest ID + optional version, case-insensitive, semantically versioned).
- `Ports.Ports.BasePort` — executable/config paths, supported games & features, and
  command-line argument construction through `CmdParametersBuilder`. Port-specific
  behavior is added by overriding `CustomModifyArgs`, `BeforeStart`, `AfterEnd`.

New games/ports/addons follow existing subclasses; do not special-case them in view
models. Add an enum value in `Core.All.Enums` and a concrete implementation, then wire
it in the relevant `With...` helper.

### Providers and data flow

Providers (`Games.Providers`, `Addons.Providers`, `Ports.Providers`) are the stateful
coordination layer between the domain model and the UI. They own caches and raise
events (`AddonsChangedEvent`, `GameChangedEvent`, `MetadataUpdatedEvent`) that view
models subscribe to. Downloadable addons use dictionaries keyed by `AddonId`;
installed addons use per-type lists. Cache rebuilds/read snapshots use `SemaphoreSlim`,
but several installed-cache mutations currently bypass it (see review below). Do not
assume these caches are thread-safe. Publish snapshots and raise events outside locks;
marshal UI notifications onto the UI thread.

The downloadable catalog comes from `addons.json`. `MetadataProvider` uses game
and addon metadata from `data.json` and `manifests.json`. Offline mode looks for
local copies via `ClientProperties.PathToLocal*Json`. Those getters use Windows
separators and a fixed relative search depth on this checkout.

### Persistence

- `Database.Client.DatabaseContext` is the only `DbContext`; use
  `IDbContextFactory<DatabaseContext>` (registered by `WithDatabase`) — do not inject a
  scoped context into singletons.
- Migrations live in `src/Database.Client/Migrations`. `App.Run` calls
  `Database.Migrate()` synchronously on startup. Add migrations with EF Core
  tooling against `Database.Client` and preserve already-applied migrations.
- The DB file is `BuildLauncher.db` in the application working folder. This
  checkout calls `FixDatabase` before migration and deletes matching SQLite
  journal sidecars. Preserve SQLite recovery files in new work and see the
  [app review backlog](.agents/docs/app-review-backlog.md) for the saved fix.

### Error handling

Prefer `Result` / `Result<T>` (`Core.All`) with a `ResultEnum` and message for expected
operational failures. Existing behavior is inconsistent: `FilesDownloader`, for example,
throws on HTTP errors and returns cancellation only during part of the operation.
Inspect each API's actual contract; never ignore an install/download result. Treat
cancellation separately from failure, preserve existing user data, and show actionable
errors through the UI notifier as well as logging diagnostic details.

## Code conventions (mandatory)

These are enforced by `.editorconfig` and analyzers; follow them in every edit.

- **XML documentation is required on every member**, public and private: `<summary>`,
  and `<param>` / `<typeparam>` / `<returns>` / `<inheritdoc />` where applicable.
  `GenerateDocumentationFile` is on and missing docs produce warnings.
- **Discard intentionally-unused results** with `_ =`, e.g. `_ = services.WithConfig();`,
  `_ = sb.Append(...)`, `_ = cache.Remove(x);`. This silences analyzer warnings without
  changing behavior. Do not use `_ =` to swallow a result that actually matters.
- Use `var` everywhere; never spell out the type when it is apparent.
- File-scoped namespaces only, matching folder structure (`dotnet_style_namespace_match_folder`).
- Follow the existing brace/indent style: Allman braces, 4 spaces, **CRLF** line endings,
  no trailing whitespace, blank line after closing braces as the file already does.
- Naming: interfaces start with `I`; types and non-field members are PascalCase;
  private fields are `_camelCase`.
- Mark every new class `sealed` by default. Only leave a class unsealed when it is
  actually designed to be inherited (a base/abstract type with subclasses); if not,
  remove the modifier from any existing unsealed class you touch.
- Async: suffix `Async`, accept/propagate `CancellationToken` where the surrounding API
  does, and use `ConfigureAwait(false)` in library code (Core.All, Core.Client, Games,
  Addons, Ports, Tools, S3) as the existing code does. Suppress CA2007 only with a
  comment explaining why, as in `InstalledAddonsProvider`.
- Prefer modern C# the codebase already uses: collection expressions (`[...]`),
  pattern matching, `required`/`init`, records, `readonly` collections, raw strings,
  local functions. `List<T>` is acceptable (CA1002 disabled) but prefer read-only
  collection types across API boundaries.
- `AnalysisMode` is `All`. Do not blanket-disable analyzer rules; if a suppression is
  genuinely required, use a narrowly scoped `#pragma warning disable` with a reason, or
  add the specific rule to `.editorconfig` with justification.
- Do not add code comments unless they explain non-obvious intent; the codebase relies
  on XML docs instead of inline narration.

## Testing conventions

- One test class per subject, `sealed`, named `<Type>Tests`, namespace `Tests.Unit`
  (or the matching test project). Mirror production naming.
- Use xunit `[Fact]`/`[Theory]`, `[InlineData]`, and `Moq` for dependencies.
- Test observable behavior through public APIs; avoid testing private members.
- UI/view-model tests belong in `Tests.Unit.Sequential` (Avalonia headless).
- Do not add real-network or shared-disk-dependent tests to `Tests.Unit`; isolate
  dependencies with fakes (`ConfigProviderFake`, `FakeHttpMessageHandler`, offline API,
  in-memory stubs). File-operation tests may use unique temporary directories and must
  clean up only their own files.

## Guardrails

- Never hard-code package versions; edit `Directory.Packages.props`.
- Never commit secrets, tokens, or the MinIO credentials.
- Do not change `Directory.Build.props` shared properties (TFM, version, nullable,
  analyzer mode) as a shortcut for a local compile fix.
- Keep `db/*.json` valid: they are validated against the S3 bucket by
  `Tests.Database` and by a dedicated CI workflow.
- When adding a game, port, tool, addon type, or setting, update the corresponding enum
  and DI registration; do not introduce game-specific branches in shared code.
- Prefer editing existing files and following neighboring patterns over creating new
  abstractions.
- Never commit, amend, push, or open pull requests unless the user explicitly asks.
- Commit messages start with a capital letter and use `Fixed` / `Added` / `Updated`
  rather than `Fix` / `Add` / `Update`.

## Addon manifest and conversion work

For preparing/reviewing `addon.json`, converting addon package layouts, or updating
its specification, read `.agents/skills/buildlauncher-addon-json/SKILL.md` and
`.agents/docs/addon.json-Specifications.md`. The skill is repository-local; load its file
explicitly if the current client does not discover repository skills automatically.
The specification records its reviewed source commit; re-check changed code before
reusing a documented contract.

For complete addon preparation or source/prepared comparisons, use
`.agents/skills/buildlauncher-convert-addon/SKILL.md` and
`.agents/docs/addon-conversion-plan.md` for the common workflow, then read the
actual game's profile through `.agents/docs/games/index.md`. Game profiles own
resource/script recipes and artwork styles: RFF/INI and Blood DAT rules are
Blood-only; CON and GRP support depend on the actual game and port. Do not apply
Blood recipes or Shadow Warrior lettering to other games. Apply shared
CON/GRP-family rules to Duke, Redneck, Rides Again, NAM and WWII GI through
`.agents/docs/games/con-grp-family.md`, using their actual commands and resources.
Resource work is routed to
`.agents/skills/buildlauncher-blood-art/SKILL.md` for PNG/DEF export and
`.agents/skills/buildlauncher-blood-rff/SKILL.md` for Blood addons containing RFF
files, unpacked native packaging and optional DEF assembly. CON/GRP-family campaigns
use `.agents/skills/buildlauncher-duke-con/SKILL.md` for reserved CON renames,
include/episode/resource checks and native MAP/audio packaging. Large nested-resource
CON/GRP-family TCs also use `.agents/skills/buildlauncher-duke-large-tc/SKILL.md` for container
roles, include graphs, palette tables and unresolved-reference classification.
Route artwork and descriptions to
`.agents/skills/buildlauncher-addon-presentation/SKILL.md`.
The [ART guide](.agents/docs/art-exporter.md) covers the .NET exporter and
independent Python/Pillow verifier. VOXEL.DAT export remains deferred. The user excluded port compatibility
checks for this conversion workflow. Report them as untested.


- Keep the manifest contract separate from `db/addons.json` (download catalog),
  `db/data.json` (application/game data), and `db/manifests.json` (manifest collection).
- Run [`validate-addon`](.agents/docs/addon-manifest-validation.md) for each
  generated manifest. It parses with `AddonManifestJsonContext` and applies the
  authoring rules shared with the developer editor. Verify factory constraints,
  references and package layout before import.
- Preserve source archives and live installations; prepare conversions in a fresh
  output directory and validate every manifest before importing anything into BL.
- Check `db/addons.json` during every conversion for an existing release of the
  same addon. Preserve a confirmed existing ID unless the user explicitly requests
  a separate edition identity; record the original catalog ID/version. Reuse
  suitable descriptions and grid/preview artwork from an older release before searching or generating
  replacements. User-supplied materials take priority; record the reused version
  and validate artwork dimensions, format and game palette. See the presentation
  skill for catalog fields, archive retrieval and version-specific checks.
- Read the source addon's applicable README variants, credits and release notes
  in root/subdirectories/archives for metadata and description, even when a
  catalog entry exists. Distinguish addon documentation from bundled tools'
  readmes; reconcile conflicts and record provenance rather than copying stale
  catalog text or assuming an outdated README header describes the current release.
- For newer versions of the same addon, `release_date` remains the original
  release date. Improved editions of older addons, such as BHSE, use the
  improved edition's release date instead. Prefer README or previously verified
  edition-specific evidence; record provenance and keep latest update dates
  outside addon.json. Do not infer release dates from catalog UpdateDate.
- BAT launchers/installers are unnecessary in prepared BL addons. Read them as
  preparation evidence, establish declared entry points and required resources,
  then omit .bat files from the runtime package and Extras. Preserve original
  source archives and record hashes/reasons for omissions. Follow the shared
  conversion plan's launch and metadata lessons for edition versions, naming,
  current INI references and optional music fields.
- Standalone installation-only guides are unnecessary for adapted BL addons.
  Read them for preparation evidence, then omit them from the package and Extras;
  preserve source archives and record hashes/reasons. Keep mixed README/manuals
  with credits, licenses or gameplay information unchanged rather than deleting
  them or removing their installation sections.
- Follow the [shared artwork guide](.agents/docs/addon-artwork-style.md) and
  presentation skill for image requirements, preview fallbacks, composition,
  validation and provenance. Game profiles supply lettering, grid styles and
  palette differences.
- Select runtime scripts, resource containers, native packaging and conversions
  through `.agents/docs/games/index.md` and the actual game profile. RFF entry
  points are Blood-only; CON/GRP support and DAT meanings depend on game/port.
  Keep detailed Blood and Duke recipes in their profiles and specialized skills.
- Declare `TileFromTexture` when generated DEF resources use `tilefromtexture`;
  the resource exporter does not generate the addon manifest.
- For a requested source-only conversion, finish and validate the output and freeze
  its file hashes before opening a prepared comparison fixture.
- Keep maintained instructions shared in `.agents/docs/` and `.agents/skills/`.
  Do not create Markdown instructions, reference notes or reports for individual
  addons. Keep per-conversion inventories, provenance and comparisons in JSON
  outside addon.json; summarize results in the conversation. Preserve original
  author-provided documentation. Run documented CLI commands from the repository
  root.
- Record conversion provenance, unresolved requirements, and untested game/port/OS
  combinations outside `addon.json`. Do not infer conversion or launch support merely
  from the presence of a model field or enum member.
- When wiki publication is requested, keep its text aligned with the local
  specification. That request does not authorize committing/pushing application
  changes, changing catalog data, uploading to S3, or publishing addon packages.

## App review and backup work

See the [app review backlog](.agents/docs/app-review-backlog.md) for current
limitations and the local commits containing recovery work and regression tests.
Check the active checkout before relying on backup-only code or commands.
