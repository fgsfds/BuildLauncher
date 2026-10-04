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
- Migrations live in `src/Database.Client/Migrations` and are applied at startup via
  `dbContext.Database.Migrate()`. Add new migrations with EF Core tooling against
  `Database.Client`; never edit an already-applied migration.
- The DB file is `BuildLauncher.db` in the working folder. Startup currently deletes
  `*.db-wal`/`*.db-shm` files; this is a data-loss bug, not a maintenance pattern to copy.
  Let SQLite recover/checkpoint its own journal files.

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
`.agents/docs/addon-conversion-plan.md`. Resource work is routed to
`.agents/skills/buildlauncher-blood-art/SKILL.md` for PNG/DEF export and
`.agents/skills/buildlauncher-blood-rff/SKILL.md` for Blood addons containing RFF
files, unpacked native packaging and optional DEF assembly; artwork and descriptions to
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
  same addon. Preserve a confirmed existing ID and reuse suitable descriptions
  and grid/preview artwork from an older release before searching or generating
  replacements. User-supplied materials take priority; record the reused version
  and validate artwork dimensions, format and game palette. See the presentation
  skill for catalog fields, archive retrieval and version-specific checks.
- Blood addons containing RFF files need unpacking and may retain native ART,
  RFF and DAT. Detect RFF case-insensitively, including inside source archives;
  identify and declare the actual main/sound RFF files so BL unpacks the ZIP.
  This is the normal profile for those addons and needs no separate approval.
  Without RFF, convert ART/DAT to DEF-backed resources; unsupported DAT conversion
  is unresolved, not a reason to retain raw files or invent an RFF declaration.
  Use existing RFFs directly. Any required RFS compilation needs verified inputs
  and commands; no compiler is implemented in the ART exporter. Keeping native
  VOXEL.DAT in the RFF profile does not implement VOXEL-to-DEF export.
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

This is a source review, not a record of completed fixes. P1 means a high-impact
correctness/data-preservation issue; P2 means normal-priority correctness or usability.
Re-check the named code before implementing a finding and remove/update resolved items.
Keep fixes focused; this backlog does not authorize a wholesale rewrite.

### P1: preserve user data and working installations

1. **SQLite recovery files are deleted before opening the database.**
   `src/Avalonia.Desktop/App.axaml.cs`, `FixDatabase`, deletes every `.db-wal` and
   `.db-shm` in the working directory before migration. A WAL can contain committed
   changes that have not reached the main DB, especially after a crash. It can also
   belong to another running process or another database. Remove blanket deletion and
   let SQLite recover. Check a restart with uncheckpointed committed data and a second
   open connection. Reference: [SQLite WAL lifecycle](https://www.sqlite.org/wal.html).

2. **A failed update can uninstall an existing port/tool.**
   `src/Core.Client/InstallerBase.cs`, `InstallAsync`, calls `Uninstall()` for any
   exception, including release lookup, HTTP failure and cancellation before extraction.
   `PortInstaller.Uninstall` recursively deletes the live install folder; `Backup()` is
   empty for ports. `ToolInstaller.Uninstall` for DOSBlood deletes `BLOOD.EXE` before
   restoring its backup, even when failure occurred before any backup was made.
   Extract into a unique staging directory, verify/post-process there,
   and replace the live install only after success with rollback available. Cleanup
   failures must not hide the original error. Return `Cancelled` for cancellation.
   Check network failure, corrupt archive, hash mismatch and cancellation against an
   existing installation, including configs and user files.

3. **Addon extraction replaces live content before validation and deletes its source
   even when manifest loading fails.** `src/Addons/Helpers/ArchivedAddonExtractor.cs`,
   `Unpack`, recursively deletes an existing folder named after the ZIP before extracting.
   `TryExtractIfNeededAsync` deletes the archive in `finally` whenever `unpackedTo` is
   set, including later deserialization failures. Stage and validate all manifests
   before replacing content or removing the archive; preserve the previous folder on
   failure. Check a malformed second manifest, interrupted extraction, and a same-name
   folder containing user edits. Do not claim archive traversal protection without
   checking the actual SharpCompress behavior and adding adversarial archive tests.

4. **Self-update has no integrity check or rollback.**
   `src/Core.Client/AppUpdateInstaller.cs` ignores the download result, never checks
   the release hash, reuses the update directory, and renames the old executable before
   ensuring replacement will succeed. A failed move leaves the normal executable absent;
   stale staging contents can survive earlier attempts. Validate download and hash,
   extract into fresh staging, verify the expected executable, then replace with rollback.
   Create the update marker only after preparation succeeds. Check bad hashes, missing
   executable, stale staging, cancellation and a failed replacement move. Verify both
   published Windows and Linux layouts; do not assume copying just the executable is
   sufficient for every publish configuration.

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

7. **Addon executable selection always uses Windows and can erase a custom override.**
   `src/Ports/Ports/PortStarter.cs`, `StartAsync`, selects `OSEnum.Windows` regardless
   of runtime OS. The `TryGetValue(..., out pathToExe)` also resets a supplied custom
   executable when the addon has a Windows dictionary but no entry for that port.
   Select `CommonProperties.OSEnum`, use a separate lookup variable, and define override
   precedence explicitly. Check Linux manifests, absent per-port entries, and custom ports.

8. **Launch errors are swallowed and cleanup/playtime are unreliable.**
   `PortStarter.StartPortAsync` logs failures and returns normally; `StartAsync` then
   records time even if no process started. The stopwatch includes setup time, the
   disposable process handle is never disposed, and `AfterEnd` is skipped if argument
   construction or playtime persistence throws after `BeforeStart` changed files.
   Return a launch outcome, dispose the handle, measure the running process, and pair
   successful setup with cleanup in `finally`. Check a null process, start/wait failures,
   argument failure after setup, and persistence failure. The UI should report the
   executable and a useful error instead of silently appearing to launch successfully.

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

13. **Disabling the last option leaves an empty-string option.**
    `src/Core.Client/Config/ConfigProvider.cs`, `GetEnabledOptions`, splits an empty
    stored string into a set containing `""`. Remove empty entries or delete the empty
    row; check `Assert.Empty` after disabling the last option and enable/disable cycles.
    The existing last-option test checks only that the former option is absent.

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

### Validation baseline and gaps

Reviewed on Windows with SDK 10.0.401. `dotnet build --no-restore --verbosity quiet
-m:1` succeeded with **296 warnings, 0 errors**. Both local test projects passed:
`Tests.Unit`: **606/606**; `Tests.Unit.Sequential`: **403/403**. External/S3 tests,
Linux execution, published self-update and interactive game launches were not run.
These passing suites do not establish that the failure cases above are safe.

Prioritize behavioral regression tests for P1 recovery and platform/transition cases.
`Tests.Unit/PortInstallerTests.cs`, `Uninstall_DeletesInstallFolder`, currently never
calls `Uninstall` or asserts deletion; replace its misleading constructor-only checks
with isolated file-operation assertions. The downloader resume fake accepts a range
without checking its end, so it misses the range bug. Linux path tests must use an
actual Linux layout rather than Windows fixtures containing backslashes.

Reduce compiler nullability and ownership warnings first, then address analyzer noise
incrementally. Preserve analyzer configuration and avoid treating the current warning
count as a permanent allowance or suppressing warnings merely to make the build quiet.
