# Unity project context

## Baseline

Last checked: 2026-09-08, main `d15e39a` plus emulator QA fixes for stale ad callbacks, incident rules-panel sizing, localized fallback destinations, case-specific endings, and completed-case board spacing. Repository `AGENTS.md` is authoritative for automation permission.

- Editor: Unity `6000.3.21f1`, pinned by `ProjectSettings/ProjectVersion.txt`
- Packages: URP 17.3.0, Input System 1.20.0, Localization 1.5.8, uGUI/TMP 2.0.0, Test Framework 1.6.0
- Ads: Google Mobile Ads Unity 11.3.0 and EDM4U 1.2.188. UMP controls request permission; normal gameplay remains available during consent failure or offline operation.
- Android: `com.joyshu93.curioclerknightshift`, API 29–36, ARM64, IL2CPP, AAB, portrait
- Scenes: `Bootstrap` loads `Main`; `GameApp` changes the UI between Menu, Tutorial, Shift, Result, Casebook, and Settings.

## Runtime flow

`ContentCatalog` creates the authoritative 24 artifacts, 10 rule templates, five difficulty bands, and six cosmetics. `ShiftGenerator` produces a deterministic queue from a seed. `RuleEngine` evaluates rules from top to bottom and requires the final catch-all. `ShiftSession` owns Hold, hearts, combo, score, coins, and the one rewarded action per shift. `ProgressionService` applies completed results to `PlayerSaveData` once.

`JsonFileSaveStore` writes a temporary JSON file and replaces the primary while preserving a backup. A corrupt primary falls back to the backup; two corrupt files fall back to a sanitized default.

The incident board resolves saved progress across Unmelting Ice and Remembering Rain. `SecondIncidentCatalog` authors the five Rain shifts, their ordered rules, protective Hold conditions, and bilingual narrative. Core-fun result screens intentionally hide monetization offers; private reward methods and their service boundaries remain covered by automated tests.

## Generated assets

`CurioClerk.Editor.ProjectBuilder.BuildAll` is idempotent and creates or updates:

- `Assets/Resources/Content`: 24 Artifact, 10 Rule, 5 Difficulty, 6 Cosmetic assets
- `Assets/Localization`: English/Korean locales and UI string tables
- `Assets/Rendering`: URP asset with 2D Renderer
- `Assets/Scenes`: Bootstrap and Main
- Android player settings, build scenes, icon, and Noto Sans KR dynamic TMP font

`ContentValidator` is an `IPreprocessBuildWithReport` gate. Duplicate IDs, incomplete bilingual artifact copy, invalid trait counts, broken fallback rules, wrong asset counts, or wrong scene order fail the build.

## Validation

The human developer authorizes agents to run the repository scripts directly. Confirm the exact project, preserve unrelated dirty files, and retain logs before running:

```powershell
.\scripts\test-unity.ps1
```

For a QA APK using Google sample IDs and debug signing:

```powershell
.\scripts\build-android-dev.ps1
```

Only when the requested scope includes a release Android build, the release bundle route is:

```powershell
.\scripts\build-android.ps1
```

The Android builder selects Unity-provided SDK/NDK/OpenJDK paths, enables public IL2CPP symbols, switches to Android, validates content, and writes `Builds/Android/CurioClerk.aab`.

## Extension points

- `IAdService`: rewarded availability and completion only
- `IPrivacyService`: consent refresh and privacy-options entry point
- `ISaveStore`: versioned durable local persistence
- `IClock`: testable local calendar time
- `IShiftSeedProvider`: standard and daily seed source

Keep platform SDK calls behind these boundaries. The Core assembly must stay free of Unity references.

No Unity MCP is available in the current QA session; command-line Unity validation and ADB runtime checks are supported. No community MCP is used. The v1 no-remote-telemetry gate excludes Firebase/remote game analytics and crash reporting; old save consent fields are compatibility data, not evidence of active telemetry.

Unity Localization brings Addressables as a package dependency and manages its own locale/table groups. Gameplay content does not use Addressables, remote catalogs, or content updates; generated `addressables_content_state.bin` files are ignored.
