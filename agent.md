# agent.md

Instructions for AI coding agents working in the **BuildLauncher** repository.

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

.NET SDK is pinned to the `10.0.x` band. Central package management is on (`Directory.Packages.props`); never hard-code
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
  S3                S3 file uploader and utilities.
  Avalonia.Desktop  App entry point, DI composition root, views, view models.
  Benchmarks        BenchmarkDotNet harness.
  Tests.*           Test projects (see above).
db/                 addon/data/manifest JSON databases shipped with the app.
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
- Concrete ports and tools are registered as keyed-by-base-type singletons:
  `AddSingleton<BasePort, EDuke32>()`, `AddSingleton<BaseTool, Mapster32>()`.
  Consumers receive `IEnumerable<BasePort>` / `IEnumerable<BaseTool>`.
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
  (title + optional version, case-insensitive, semantically versioned).
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
models subscribe to. Addons are keyed by `AddonId` in per-type caches guarded by a
`SemaphoreSlim`; preserve that locking when touching caches.

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
- The DB file is `BuildLauncher.db` in the working folder. Startup deletes stale
  `*.db-wal`/`*.db-shm` files; account for that.

### Error handling

Operations that can fail return `Result` / `Result<T>` (`Core.All`) with a `ResultEnum`
and message rather than throwing across layer boundaries. Use exceptions for programmer
errors and unexpected states, and log user-facing failures through `ILogger`.

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
- Do not add network/disk-dependent tests to `Tests.Unit`; isolate with fakes (`ConfigProviderFake`,
  `FakeHttpMessageHandler`, offline API, in-memory stubs).

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
