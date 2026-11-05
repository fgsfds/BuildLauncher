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
detects installed games, downloads source ports / tools / mods / campaigns / maps from
GitHub, and launches them with configured command-line arguments.

- UI: **Avalonia** desktop app (Fluent theme), MVVM via **CommunityToolkit.Mvvm**.
- Runtime: **.NET 10**, nullable enabled, implicit usings, file-scoped namespaces.
- Persistence: **SQLite** via EF Core.
- Networking: `HttpClient` against the **GitHub API**, with a fully offline mode.
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

Always run the affected unit test project (s) plus a build after a change. There is no
separate lint command: analyzers run on build.

For native smoke tests of published Windows and Linux executables and the release gate,
see [published release checks](.agents/docs/published-build-tests.md).

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
  Addon.ArtExporter Standalone .NET CLI for ART/SURFACE to indexed PNG + DEF.
  S3                S3 file uploader and utilities.
  Avalonia.Desktop  App entry point, DI composition root, views, view models.
  Benchmarks        BenchmarkDotNet harness.
  Tests.*           Test projects (see above).
db/                 addon/data/manifest JSON databases shipped with the app.
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
  paths, cached addon-folder detection.
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

Addon metadata is loaded from `db/addons.json` (remote/database) and `db/data.json`,
and validated by `MetadataProvider`. Offline mode reads the local `db/` files.
`ClientProperties.PathToLocal*Json` locates them in dev and in a published layout.

### Persistence

- `Database.Client.DatabaseContext` is the only `DbContext`; use
  `IDbContextFactory<DatabaseContext>` (registered by `WithDatabase`) — do not inject a
  scoped context into singletons.
- Migrations live in `src/Database.Client/Migrations` and are applied at startup through
  `DatabaseInitializer.InitializeAsync()`. Add new migrations with EF Core tooling against
  `Database.Client`; never edit an already-applied migration.
- The DB file is `BuildLauncher.db` in the working folder. Startup uses `DatabaseInitializer`
  to serialize migrations and recover abandoned EF migration locks. Never delete
  `*.db-wal`/`*.db-shm` files; let SQLite recover/checkpoint its own journal files.

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
  `Helpers/HeadlessView` hosts production XAML with Fluent/DataGrid templates and
  keyboard activation. Call `HeadlessView.Prepare()` before constructing controls;
  it shares process-wide icon registration while restoring per-test app resources.
  Headless control coverage includes settings, custom ports, campaign/map search and
  selection, download filtering/multi-selection/refresh, and port/tool release states.
  Use fake services and controlled task completions; these tests do not launch games,
  download packages, or exercise native file pickers. Run them with the existing
  sequential test command above on Windows and Linux.
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
The standalone .NET resource exporter lives in `src/Addon.ArtExporter`; read
`.agents/docs/art-exporter.md` for its CLI and supported formats. An independent read-only ART/SURFACE audit helper is included in the resource
skill; its supported profile is documented in the shared exporter guide.
VOXEL.DAT export into a separate DEF
is a later training step. The user excluded port compatibility checks for this
conversion workflow; report them as untested instead of requiring them.

- Keep the manifest contract separate from `db/addons.json` (download catalog),
  `db/data.json` (application/game data), and `db/manifests.json` (manifest collection).
- Validate generated manifests with `AddonManifestJsonContext` and
  `Core.All.Helpers.AddonManifestValidator`, shared with `DevViewModel.GetAddonJson`.
  See `.agents/docs/addon-manifest-validation.md` for the read-only CLI. Then check
  factory constraints, references and package layout; keep excluded port checks
  untested. A serializer-only test fixture is not necessarily a runnable template.
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
- Newly generated grid/preview art should match the library's old-school 8-bit
  Build texture/sprite look rather than realistic painted covers. Inspect actual
  references and supply them to imagegen; compose portrait and landscape separately.
  Honor explicit requests to ignore an addon's existing artwork. Preserve game
  identity, verify the exact palette, and inspect final pixels/title readability.
- Only TCs/campaigns and standalone games require grid and preview images.
  Maps and mods require only preview; do not generate a grid unless explicitly
  requested or treat a missing grid as incomplete preparation.
- When creating grid/preview images, follow `.agents/docs/addon-artwork-style.md`
  and the presentation skill. Match the same game/type references and preserve
  established addon branding. Compose grid and preview separately; previews may
  be title cards or resource showcases. Record missing-reference fallbacks.
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

## Addon tooling verification (2026-10-04)

After adding the exporter and shared authoring validator, the solution build passed
with zero errors; `Tests.Unit` passed 692/692 and `Tests.Unit.Sequential` 406/406.
The exporter and shared validator have been exercised against real addon resources
as well as isolated regression fixtures. Resource, manifest and packaging checks
remain separate from port compatibility, game launches and live BL import. The
older app-review baseline below remains a historical review.

## App review backlog (2026-10-01)

This originated as a source review; completed fixes are marked with their dates. P1 means a high-impact
correctness/data-preservation issue; P2 means normal-priority correctness or usability.
Re-check the named code before implementing a finding and remove/update resolved items.
Keep fixes focused; this backlog does not authorize a wholesale rewrite.

### P1: preserve user data and working installations

1. **Resolved (2026-11-05): startup preserves SQLite recovery files.**
   DatabaseInitializer serializes application startup migrations with an exclusive
   database-specific file handle that the OS releases after a crash. While holding it,
   startup removes abandoned EF migration-lock rows and lets SQLite recover its own
   WAL/SHM data. The lock filename is retained; its presence alone does not hold the lock.
   All application startup migrations must use this initializer. External migration
   tools must not run concurrently. Startup logs migration failures rather than deleting
   recovery files or resetting the database.
   Regression tests reproduce EF startup waiting indefinitely on an abandoned
   __EFMigrationsLock row, then verify WAL-only committed data, pending migrations,
   active startup exclusion, normal open connections, repeated startup, unrelated
   recovery files and corruption preservation. The original historical failing database
   was unavailable; the reproduced EF lock failure is not proof of its exact condition.

2. **Resolved (2026-11-05): failed port/tool updates preserve live installations.**
   Installation now downloads to an isolated directory and prepares a sibling staging
   directory, including existing user files and staged tool backup/post-install work.
   Replacement rolls back a failed move and retains recovery files if rollback fails.
   Cancellation returns Cancelled; cleanup errors do not hide the operation result.
   Regression coverage includes lookup/HTTP/hash/archive/post-install failures,
   cancellation, DOSBlood backups, successful updates and directory replacement rollback.

3. **Resolved (2026-11-05): addon extraction validates manifests before replacement.**
   Root manifests are all parsed and their required runtime fields checked before native
   extraction. Any manifest can require native extraction. Files are extracted with
   their directory layout into fresh staging and committed with rollback; the source ZIP
   is removed only after success. Tests cover malformed later manifests, interrupted
   extraction and preservation of same-name user content. These tests do not establish
   archive traversal or symbolic-link protection.

4. **Resolved (2026-11-05): self-update preparation and replacement are recoverable.**
   Preparation checks the download outcome, verifies a supplied release digest, extracts
   into fresh staging and requires the expected executable before publishing its marker.
   Installation replaces every payload file and restores changed files on failure.
   Tests cover bad hashes, missing executables, stale staging, cancellation, HTTP/archive
   errors, companion files and a replacement collision. Actual published Windows/Linux
   self-restarts and recovery from process termination during commit remain untested.

### P2: cross-platform correctness and operation results

5. **Linux offline JSON paths contain Windows separators.**
   `src/Core.Client/Helpers/ClientProperties.cs`, the three `PathToLocal*Json` getters,
   combine paths such as `@"db\addons.json"`. Backslashes are literal filename characters
   on Linux. Build paths from separate segments and locate repository/dev files without
   assuming a fixed depth under `artifacts/bin`. Check all three JSON files in a Linux
   published layout and the actual development output layout. `Program.Main` sets the
   current directory to the application base directory, so use that when reproducing.

6. **Credential settings throw on Linux.**
   `src/Core.Client/Config/ConfigProvider.cs`, `Protect` and `Unprotect`, throw outside
   Windows, but `GitHubToken` and `S3SecretKey` use them unconditionally for nonempty
   values. Provide a platform-specific secret-store implementation or explicitly
   disable unsupported settings with usable feedback. Do not fall back silently to
   plaintext. Check Linux save/load and a database copied from Windows with encrypted
   credentials; unavailable credentials must not crash startup or settings bindings.

7. **Resolved (2026-11-05): executable overrides follow the runtime platform.**
   Explicit custom executable choices take precedence over per-platform addon overrides;
   absent per-port entries fall back without clearing a custom choice. Regression tests
   cover Windows and Linux selection, missing entries and explicit custom executables.

8. **Resolved (2026-11-05): launch results and cleanup are explicit.**
   PortStarter returns an outcome consumed by campaign/map UI notifications, disposes
   process handles, measures process wait time and records playtime only after a
   successful wait. Successful setup is paired with cleanup in finally, including
   argument-generation and persistence failures. Tests cover null handles, start/wait
   errors, setup/argument/persistence/cleanup failures and UI error notifications.

9. **Port/tool installation failures receive no UI feedback.**
   `src/Avalonia.Desktop/ViewModels/PortViewModel.cs` and `ToolViewModel.cs` assign
   `isInstalled` but never inspect it. The installer converts most exceptions to an
   error result, so the view-model exception handlers never run for those failures.
   Handle success, cancellation, hash errors and ordinary errors explicitly through
   `IUserNotifier`; avoid exposing raw stack traces to users. Check each result kind.

10. **Invalid addons can be reported as installed and counted remotely.**
    `InstalledAddonsProvider.AddAddonAsync` returns normally for null/empty parsed
    addons. `DownloadableAddonsProvider.DownloadAddonAsync` then increments installs
    and returns success. Return an explicit add/validation result and count only a
    successful installation; a counter-service failure should not invalidate the local
    install. Check a hash-valid ZIP with no supported manifest and a malformed manifest.

### P2: cache transitions, concurrency and state

11. **An empty catalog refresh retains old entries; update flags can remain stale.**
    `src/Addons/Providers/DownloadableAddonsProvider.cs`, `CreateCacheAsync`, returns
    before replacing `_cache` when the API returns zero addons. `GetDownloadableAddons`
    mutates cached DTO flags but does not reset `IsUpdateAvailable` on every branch,
    including after the installed addon disappears. Publish an empty snapshot on an
    empty successful refresh and derive both flags afresh for every item. Check populated
    to empty refresh, update available to uninstalled, and versioned to unversioned installs.

12. **Installed cache locking is incomplete and contention hides content.**
    `src/Addons/Providers/InstalledAddonsProvider.cs` locks cache rebuilds, but add/delete
    operations bypass the semaphore. Read helpers use `Wait(1)` and return empty maps/mods
    or only official campaigns when the lock is busy. Background refresh can therefore
    temporarily hide installed content, while unsynchronized writes can race with snapshot
    enumeration or lose additions. Use a consistent synchronization policy and keep the
    last complete snapshot available during refresh, raising events after releasing locks.
    Exercise refresh concurrent with add/delete and enumeration; do not add nested
    semaphore waits that deadlock.

13. **Resolved (2026-11-05): disabling the last option returns an empty set.**
    ConfigProvider ignores empty entries when reading and editing serialized option
    sets. Tests assert an actually empty result after disabling the last option,
    repeated enable/disable cycles across provider instances, duplicate enables and
    normalization of legacy empty separators. The former test only checked absence
    of the removed option and did not detect the empty-string entry.

14. **Legacy save-folder migration can prevent startup.**
    `src/Avalonia.Desktop/App.axaml.cs`, `RenameSaveFolder`, calls `Directory.Move`
    whenever the old folder exists without checking whether the new folder already
    exists. Both folders cause an exception before the main lifetime starts. Make the
    migration idempotent, preserve both sets of saves on conflicts, and report issues
    without blocking startup. Check old-only, new-only, both-present and repeat runs.

### Simplification and usability work

- Consolidate duplicated SHA-256 verification in `InstallerBase` and
  `DownloadableAddonsProvider` into one cancellation-aware streaming helper. Keep the
  distinction between optional hashes for some releases and mandatory addon hashes.
- Reuse the existing `IProcessRunner`, `IFolderOpener` and `IUserNotifier` consistently
  in port/tool view models instead of adding more direct `Process.Start` calls or static
  notification calls. `ToolViewModel.Launch` creates an unused `ProcessStartInfo` and
  does not set the tool's working directory; check relative resource/config loading.
- Share the small install-result/progress/notification routines in port/tool view models
  while retaining distinct release and capability rules. Remove unused locals, redundant
  branches and duplicate XML summaries as neighboring code is touched; avoid building
  a generic framework for two view models.
- Make catalog getters pure: compute installation state from a snapshot and index
  installed addons by ID once rather than repeatedly scanning all installed entries.
  Avoid fetching/sorting the whole catalog repeatedly for list and `HasUpdates` bindings.
- In `DownloadsViewModel`, replace the unconditional two-second initialization delay
  with explicit readiness, replace the `byte` batch counter with `int`, and make command
  availability reflect selection and operation state. The download predicate currently
  always returns true, and refresh/download commands share one busy boolean. Check
  overlapping refresh/download, cancel after disposal, and batches exceeding 255 items.
- Surface recovery/retry and concise failure messages for download, hash, extraction
  and launch problems. Show indeterminate progress without `Content-Length`, and update
  progress during resumed transfers instead of leaving the bar stationary.
- Simplify `FilesDownloader` to a bounded retry loop with one stream owner and one
  cleanup path. Its current cancellation scope excludes the initial HTTP request;
  resume requests use total length as the inclusive range end (one byte too far), do
  not validate `Content-Range`/resource identity, and leave temp files on non-cancellation
  failures. Use an open-ended range or `length - 1`, verify the resumed offset and file
  identity, and check pre-cancellation, rejected/mismatched ranges, changed resources,
  zero-length responses and exhausted retries with a fresh HttpClient per factory call.
- Track initialization tasks instead of discarding them in `App.Run` and
  `ViewModelsFactory`; make completion, errors and shutdown cancellation explicit.
  Dispose the root service provider on normal shutdown as well as before rebuilding it.
- If SDK reproducibility is desired, add an explicit SDK/roll-forward policy to
  `global.json`; the current runner selection alone does not pin an SDK.

### App regression coverage (2026-11-05)

The expanded local suites passed on Windows: **736/736** in Tests.Unit and
**459/459** in Tests.Unit.Sequential. The solution build passed with zero errors;
existing analyzer warnings remain. This update added 32 test cases and strengthened
existing assertions relative to the previous 713/450 local test baseline.

| Area | Added or strengthened behavior checks |
| --- | --- |
| Downloads and integrity | HTTP failures preserve an existing destination; empty successful downloads replace it; SHA-256 accepts API prefixes/casing, rejects changed payloads and respects cancellation without retaining file handles. |
| Archives and installation | Nested files with equal basenames remain distinct; retries after post-install failure retain nested saves; disposal of uncommitted staging preserves the live directory. |
| Self-update | Failed preparation preserves an earlier prepared update and marker; rollback removes newly introduced files as well as restoring replaced files. |
| Configuration and games | Exact option sets survive repeated toggles and provider recreation; legacy empty entries are ignored; window-setting updates preserve unrelated preferences; game-path changes refresh detection and notify observers. |
| Launch and UI | Cleanup and playtime wait for process completion; simultaneous launch/cleanup failures retain both diagnostics; canceled closes do not save settings; missing monitor data preserves placement. |
| Addon loading | Deserializable manifests with invalid required identifiers, titles or game fields preserve both the source archive and existing user content. |
| SQLite startup | Restart snapshots cover WAL with and without retained SHM, pending migrations, pre-cancellation and queued startup resuming after lock release. |

Tests that change the current culture or use Avalonia remain in Tests.Unit.Sequential.
Decimal-comma formatting is simulated with a cloned invariant culture because the
application enables invariant globalization; the tests do not change runtime settings.
File tests use unique temporary directories and fake HTTP/process dependencies.

These tests do not establish comprehensive code coverage, external-service or S3
behavior, Linux runtime behavior, native multi-monitor transitions, real game launches,
or published self-update restarts. Run Tests.External and Tests.Database separately
when their services and credentials are available. Historical validation below remains
for comparison, not as the current test count.

### Validation baseline and gaps

Reviewed on Windows with SDK 10.0.401. `dotnet build --no-restore --verbosity quiet
-m:1` succeeded with **296 warnings, 0 errors**. Both local test projects passed:
`Tests.Unit`: **606/606**; `Tests.Unit.Sequential`: **403/403**. External/S3 tests,
Linux execution, published self-update and interactive game launches were not run.
These passing suites do not establish that the failure cases above are safe.

Prioritize behavioral regression tests for P1 recovery and platform/transition cases.
`Tests.Unit/PortInstallerTests.cs`, `Uninstall_DeletesInstallFolder`, now calls
`Uninstall` against an isolated installation and asserts deletion. The downloader resume fake accepts a range
without checking its end, so it misses the range bug. Linux path tests must use an
actual Linux layout rather than Windows fixtures containing backslashes.

Reduce compiler nullability and ownership warnings first, then address analyzer noise
incrementally. Preserve analyzer configuration and avoid treating the current warning
count as a permanent allowance or suppressing warnings merely to make the build quiet.
