# BuildLauncher app review backlog

Current source baseline is `master` at `68300f5a3b1d5bea5ef2e2ef1fe1c05a9393e16e`,
reviewed 2026-11-12. Recheck source and tests before implementing a finding.
The local `codex/backup-before-committer-rewrite` branch contains earlier
implementation and test work. Commit IDs below are for inspecting that history
with `git show`. The backup work has yet to land on this checkout.

## Data preservation and launch

| Current behavior | Work preserved locally |
| --- | --- |
| `App.FixDatabase` deletes WAL/SHM sidecars before EF migration. This can discard SQLite recovery state. | `4f97b47` serializes migration, clears abandoned EF migration-lock rows and lets SQLite recover its journals. |
| `InstallerBase` unpacks port/tool updates into the live installation. Partial extraction or post-install failure can leave it unusable. | `87f4098` stages installation and restores the original directory on a failed replacement. |
| `ArchivedAddonExtractor` can replace a same-name folder before checking later manifests and remove the ZIP after a failed manifest load. | `87f4098` validates manifests before extraction, stages files, and retains the ZIP until success. |
| `AppUpdateInstaller` downloads and extracts in place and replaces the executable on restart. It lacks staged rollback for the complete payload. | `87f4098` adds staging, digest checks when available and payload rollback. Later published-update tests and recovery fixes are in `2a2ef63`. |
| `PortStarter` looks up Windows executable overrides on all platforms and can lose a caller-supplied custom path. `PortsHelper` can throw when the Windows key is missing. | `dd6965f` selects the current OS and preserves explicit custom executable choices. |
| `PortStarter` can record time after a failed start and can skip cleanup on earlier failures. Campaign/map callers receive no launch result. | `dd6965f` propagates launch outcomes, disposes process handles and pairs successful setup with cleanup. |
| A VoidSW file-based ZIP map can fail to resolve when passed as an absolute archive path. | `93f640e` adds the package folder to the search path and passes the ZIP basename. |
| `ConfigProvider` can return an empty string as an enabled option after the last option is disabled. | `644fd0c` filters legacy empty entries and adds exact-set regression tests. |

The backup branch also contains window persistence work (`39eb7b0`),
startup cancellation and cross-platform CI tests (`e0494a3`), broader headless
UI coverage (`202ac62`), published release checks (`41237a1`) and Linux
filesystem handling (`05c53b9`). Evaluate each change against current code
and exercise its failure cases before treating the finding as resolved.

## Other current findings

- **Offline metadata on Linux.** All three `ClientProperties.PathToLocal*Json`
  getters embed Windows separators and assume a fixed development output depth.
  Verify development and published layouts from the executable's directory.
- **Credentials on Linux.** `ConfigProvider` uses Windows DPAPI for nonempty
  GitHub tokens and S3 secrets. Test Linux settings and databases moved from
  Windows. Preserve encryption and handle unavailable credentials explicitly.
- **Legacy saves.** `App.RenameSaveFolder` attempts a move when the old directory
  exists, even when the destination already exists. Preserve both directories
  on collision and verify repeated startup.
- **Install feedback.** The port and tool view models ignore install results.
  Surface actionable errors and distinguish cancellation. Check the working
  directory used when launching tools with relative resources.
- **Addon install accounting.** `InstalledAddonsProvider.AddAddonAsync` can
  return normally after a null or empty parse, allowing
  `DownloadableAddonsProvider` to report success and increment installs.
  Validate the local result before counting it. Test malformed or missing
  manifests in hash-valid archives and counter-service failure after success.
- **Download catalog state.** `CreateCacheAsync` retains old entries when a
  refresh succeeds with an empty catalog. Derived update flags can remain set
  after an addon disappears. Test empty refreshes and installation/version
  transitions. Compute status from the current installed snapshot.
- **Installed addon concurrency.** Reads using `Wait(1)` can temporarily hide
  content during a refresh. Add/delete mutations bypass the rebuild semaphore.
  Keep the last complete snapshot available and verify concurrent changes.
- **Download lifecycle.** Check initial request cancellation, bounded retries,
  HTTP range and resource identity on resume, failure cleanup and progress
  without a content length. SHA-256 verification can be shared between installers
  and addon downloads while preserving their optional/required digest rules.
- **Downloads view.** Initialization has a fixed delay and a byte batch counter.
  Check readiness, large batches, command eligibility, and overlapping refresh,
  download and cancellation.
- **Startup lifecycle.** Track initialization tasks and shutdown cancellation.
  Check service disposal during ordinary shutdown.

## Verification scope

The solution built on Windows with SDK 10.0.403 on 2026-11-12.
`Tests.Unit` passed 692/692 and `Tests.Unit.Sequential` passed 406/406.
The build reported 328 warnings and zero errors. Those results cover the
current checkout. The larger suites described in the backup history cover a
different source revision.

External and S3 suites, Linux execution, published self-update restarts and
interactive game launches were outside this documentation verification.
