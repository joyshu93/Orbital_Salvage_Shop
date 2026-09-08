# Android QA / Google Mobile Ads integration validation

Date: 2026-09-08 KST. Scope: development APK and integration cleanup, not a release deployment.

## Starting state and preservation

The checkout was verified as local `main` at `73536b803ecd3d6171d8f01f6d85ccc0939f10c2`, containing fetched `origin/main` at `613684c` and two additional commits. The remote contained merged Remembering Rain PRs #1 and #2. There were no conflicts or staged files. The expanded dirty inventory contained 31 tracked modifications and 15 untracked files.

Work moved to `codex/android-qa-gma-integration` in the same checkout. No new worktree was created. All 46 original files were copied with SHA-256 records to ignored `Logs/AndroidQaIntegration-20260908/baseline` before validation.

Recovery points remain preserved:

- `codex/local-main-backup-20260908`: `7b338518c72e47a63b49a2c486d036afcc75d89c`.
- `stash@{0}` (`codex/local-main-pre-sync-20260908`): `408229c528fb26ce8b31b65ff52f52a01c2a2303`.
- `.worktrees/three-seal-dockets`: existing story checkout at `bce515a76d36ea2a4ff920fe403b5926d1c8b971`, including its local changes; not modified by this task.

The pre-existing local plan commit adds `Docs/superpowers/plans/2026-08-27-three-seal-dockets.md` relative to `origin/main`. It remains in branch ancestry. The integration does not rewrite or discard it.

## File decisions

| Files | Decision and reason |
| --- | --- |
| `Assets/Scripts/Editor/ProjectBuilder.cs` | Keep QA APK entry point, scoped toolchain configuration, Google settings creation and sample-ID configuration. Fix pre-generation state capture, constructor side effects, stale asset references and initial rewarded-ID loss during plugin asset creation. |
| `Assets/Tests/EditMode/EditorAutomationContractTests.cs`, `AndroidBuildStateContractTests.cs` and its `.meta` | Keep toolchain and entry-point coverage; add duplicate-reference, generation failure/success, existing/missing service asset, repeated asset reload and raw Android preference restoration regressions. |
| `Assets/Scripts/Runtime/CurioClerk.Runtime.asmdef` | Remove the restored duplicate `GoogleMobileAds.Core.dll` entry. The correct three references already exist on main, so the final file has no semantic diff. |
| `Assets/GoogleMobileAds.meta`, `Assets/GoogleMobileAds/Resources.meta`, `link.xml`, `link.xml.meta` | Keep plugin-generated IL2CPP linker support and folder metadata. They are not a second SDK installation. Local Google settings assets remain ignored. |
| `Assets/Plugins/Android/{mainTemplate.gradle,settingsTemplate.gradle,gradleTemplate.properties}` and their metadata | Keep Unity/EDM4U build templates and pinned dependency blocks required for Android GMA/UMP and AndroidX. |
| `ProjectSettings/AndroidResolverDependencies.xml`, `GvhProjectSettings.xml` | Keep the resolved dependency inventory and Jetifier setting; inspected values match the pinned package dependency XML and contain no machine paths or identifiers requiring secrecy. |
| `scripts/build-android-dev.ps1`, `.cmd`, development contract and process-runner tests | Keep one-command preflight, tests, development APK creation and artifact checks. A controlled child-process fixture demonstrated why the wrapper must wait for the Unity process rather than its lingering descendants. |
| `scripts/build-android.ps1` | Remove the restored override-synthesis helper, which contradicted bundled-first selection and accepted incomplete overrides. Retain existing release gates; use the same corrected process wait as the QA wrapper. |
| `scripts/check-android-toolchain.ps1`, `test-android-toolchain-preflight.ps1` | Align complete bundled/external selection, all-or-none overrides and exact required file markers with the C# resolver. Cover partial bundled installations and missing command-line/build tools. |
| `scripts/test-release-build-contracts.ps1`, `test-no-remote-telemetry-gate.ps1` | Keep contract coverage and fix Windows PowerShell 5.1 fixture assembly/stderr handling. Failure-code, diagnostic, stale-output and sanitization assertions remain active. |
| Provenance, notices, service setup, package licenses, integration plan and this report | Record actual resolved SDKs, licenses, supported workflow, validation and remaining release work. |
| 20 existing `Assets/Resources/Art/Artifacts/*.png.meta` modifications | Exclude from the integration commit. Differences are trailing whitespace only; preserve the original local bytes. |
| `Assets/Resources/Fonts/NotoSansKR-Dynamic.asset` | Exclude the pre-existing generated Atlas 1 from the integration commit; preserve the original local file. No font source or art change was requested. |
| `ProjectSettings/ProjectSettings.asset` | Exclude the pre-existing empty-keystore serialization difference (`{inproject}: `); preserve the original local file. |

After the QA build, all 22 excluded local files matched their initial SHA-256 values. Generated content, scenes and localization had no diff from the merged baseline. Remembering Rain's catalog and bilingual localization remained unchanged.

## Verified behavior

The normal Android QA route uses Google sample app and rewarded IDs and Unity debug signing. Release identifiers remain environment-supplied and are rejected when missing, malformed or equal to the configured samples. No live release environment values were present during this run.

Gameplay still uses replaceable ads/privacy boundaries, presents the menu independently of asynchronous consent and allows normal progression with no available ad. Only opt-in rewarded display is called by the game. The dedicated offline QA route excludes Google runtime service implementations. No Firebase or remote game telemetry integration was introduced.

QA state now snapshots before `BuildAll`, restores bundle/signing/service configuration after success or failure, and reloads service assets before cleanup. Android tool restoration preserves raw embedded/custom preferences, remembered missing paths and original preference-key presence; it attempts remaining cleanup if one restoration fails.

## Validation results

| Validation | Result / evidence |
| --- | --- |
| Pinned toolchain preflight | Pass: Unity 6000.3.21f1, Android SDK/API 36, CMake 3.22.1, NDK and OpenJDK. |
| `ProjectBuilder.BuildAll` | Asset generation and content validation passed inside the successful QA player build. Generated story/content/localization changes: none. |
| `scripts/test-unity.ps1` | EditMode **161/161** and PlayMode **116/116**, zero failures/skips. Repeated successfully by the one-command QA wrapper. |
| Remembering Rain acceptance | Included in the full PlayMode run: Korean five-shift route and final resolution/progression checks. |
| Development / offline QA / release static contracts | All passed. Release tests use controlled fixture archives; no release AAB was built. |
| Android preflight controlled fixtures | Passed, including incomplete bundled SDK with a complete external toolchain, partial override rejection and missing required markers. |
| No-remote-telemetry mutation suite and Release-mode gate | Both passed; negative fixtures remained rejected. |
| Development wrapper process-wait fixture | Old `Start-Process -Wait` failed the elapsed-time contract; `.WaitForExit()` passed while preserving test-before-build and APK verification. |
| `scripts/build-android-dev.ps1` | Built the QA APK; Unity and wrapper both exited 0. The observed delayed wrapper return was subsequently corrected and verified by the controlled process fixture. |
| APK inspection | Passed with Android Build Tools 36.0.0 `aapt2`, ZIP inspection, player identifier scan and `apksigner verify --verbose --print-certs`. |
| Post-build restoration | All seven captured Android tool preference entries identical; ignored service assets, their metadata and PlayerSettings byte-equivalent to pre-build copies. |
| Independent code review | No Critical or Important findings after corrections. |

Local evidence is under ignored `Logs/AndroidQaIntegration-20260908`, including red/green test logs, static-contract output, APK manifest/badging/signature and before/after restoration records. Full current logs remain in `Logs/EditMode.log`, `Logs/PlayMode.log` and `Logs/AndroidDevelopmentBuild.log`.

The first standalone `BuildAll` attempt completed asset validation but exited with native shutdown error `-1073741819`, following an Editor TLS certificate error. That failure log was retained. The subsequent full test runs and QA build completed successfully, including normal Unity exit. No TLS validation or compiler check was disabled.

## QA artifact

- Path: `Builds/Android/CurioClerk-qa.apk`.
- Size: **100,731,162 bytes**.
- SHA-256: `16778BF075739FEAF67F2BE477EDFC83C453A6FF0925BFA3B1F01E2D53C04F5A`.
- Package/version: `com.joyshu93.curioclerknightshift`, `1.0.0` (`10000`).
- Minimum/target API: **29 / 36**; portrait; debuggable.
- Native ABI/backend: **arm64-v8a only / IL2CPP**.
- Signing certificate: **Android Debug**, verified.
- Manifest app ID: `ca-app-pub-3940256099942544~3347511713`.
- Player rewarded ID: `ca-app-pub-3940256099942544/5224354917`; no non-sample ad IDs found in the inspected player metadata/native/assets entries.

These IDs match Google's [Unity test-ad guidance](https://developers.google.com/admob/unity/test-ads) and [setup documentation](https://developers.google.com/admob/unity/quick-start). The previous QA APK and symbols were copied to local evidence before replacement. The existing release AAB was untouched. No artifacts, credentials or local service settings are committed.

## Remaining checks

- Real-device offline launch/resume, save continuity, UMP regional consent, sample rewarded display/dismissal/no-fill and reward callback timing still need device validation. Automated Editor tests and APK inspection do not establish those outcomes.
- The first standalone Editor shutdown failure was not diagnosed as a project regression; retain its log if it recurs.
- Explicit tests for initially absent Android preference keys and restoring custom-tool mode remain a Minor coverage gap. Raw preference restoration is implemented and the actual build's before/after entries matched.
- Native/transitive SDK notices, Data Safety declarations, production identifiers/signing, store assets and human release review remain outside this development QA handoff. See `Docs/ThirdPartyNotices.md` and release documentation.
