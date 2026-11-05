# Published release checks

The reusable `published-build-tests.yml` workflow publishes the Windows and Linux
Release profiles and runs the actual self-contained executables. Linux uses Xvfb;
Windows uses the runner's native desktop. Trimming remains disabled.

Each test copies the published output to a fresh directory, adds the repository's
`db/` files as offline fixtures, and launches twice. The normal composition root,
SQLite migrations, native window and XAML resources run on both launches. The checks
verify consent, catalog deserialization, Settings/Ports/Tools/Standalone/About page
binding and layout, rendering to PNG, theme persistence, active-tab restoration and
saved window placement. Failures, missing completion markers, crash logs, nonzero
exit codes and a 120-second per-launch timeout fail the job. Logs and page images
are uploaded even on failure.

The `--smoke-test` and `--smoke-test-restart` switches are opt-in and require both
`--offline` and a `.published-smoke-test` marker next to the executable. Use the
script, which creates a fresh directory and never reuses an existing installation:

```powershell
dotnet publish src/Avalonia.Desktop/Avalonia.Desktop.csproj -p:PublishProfile=Windows -c Release -o artifacts/published-windows
./scripts/Test-PublishedBuild.ps1 -PublishDirectory artifacts/published-windows -ResultsDirectory artifacts/smoke-results
```

For Linux use the `Linux` profile and run the same script under
`xvfb-run --auto-servernum --server-args="-screen 0 1600x1000x24" pwsh -File`.
PowerShell 7, Xvfb and Avalonia's native Linux dependencies must be installed.

`Build and test` includes both published checks. `Create release` also calls the
reusable workflow, including for manual releases, using the triggering commit SHA.
Signing waits for both operating systems to pass, then downloads the tested Windows
artifact instead of rebuilding it. The existing release remains Windows-only.

These are deterministic offline smoke tests, not exhaustive UI automation. They do
not test real downloads, game launches, self-update, native file pickers, signing,
or all desktop/window-manager configurations. Existing unit and headless UI tests
remain necessary. The JSON fixtures are test inputs, not additions to release archives.
