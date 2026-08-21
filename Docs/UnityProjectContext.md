# Unity project context

## Baseline

- Editor: Unity `6000.3.21f1`, pinned by `ProjectSettings/ProjectVersion.txt`
- Packages: URP 17.3.0, Input System 1.20.0, Localization 1.5.8, uGUI/TMP 2.0.0, Test Framework 1.6.0
- Android: `com.joyshu93.curioclerknightshift`, API 29–36, ARM64, IL2CPP, AAB, portrait
- Scenes: `Bootstrap` loads `Main`; `GameApp` changes the UI between Menu, Tutorial, Shift, Result, Casebook, and Settings.

## Runtime flow

`ContentCatalog` creates the authoritative 24 artifacts, 10 rule templates, five difficulty bands, and six cosmetics. `ShiftGenerator` produces a deterministic queue from a seed. `RuleEngine` evaluates rules from top to bottom and requires the final catch-all. `ShiftSession` owns Hold, hearts, combo, score, coins, and the one rewarded action per shift. `ProgressionService` applies completed results to `PlayerSaveData` once.

`JsonFileSaveStore` writes a temporary JSON file and replaces the primary while preserving a backup. A corrupt primary falls back to the backup; two corrupt files fall back to a sanitized default.

## Generated assets

`CurioClerk.Editor.ProjectBuilder.BuildAll` is idempotent and creates or updates:

- `Assets/Resources/Content`: 24 Artifact, 10 Rule, 5 Difficulty, 6 Cosmetic assets
- `Assets/Localization`: English/Korean locales and UI string tables
- `Assets/Rendering`: URP asset with 2D Renderer
- `Assets/Scenes`: Bootstrap and Main
- Android player settings, build scenes, icon, and Noto Sans KR dynamic TMP font

`ContentValidator` is an `IPreprocessBuildWithReport` gate. Duplicate IDs, incomplete bilingual artifact copy, invalid trait counts, broken fallback rules, wrong asset counts, or wrong scene order fail the build.

## Validation

The human developer runs all automated Unity tests in the current no-MCP workflow and supplies the result for review:

```powershell
.\scripts\test-unity.ps1
```

The human developer builds the Android bundle and supplies the output for review:

```powershell
.\scripts\build-android.ps1
```

The Android builder selects Unity-provided SDK/NDK/OpenJDK paths, enables public IL2CPP symbols, switches to Android, validates content, and writes `Builds/Android/CurioClerk.aab`.

## Extension points

- `IAdService`: rewarded availability and completion only
- `IAnalyticsService`: consent-controlled event logging
- `IPrivacyService`: consent refresh and privacy-options entry point
- `ICrashReporter`: consent-controlled diagnostics
- `ISaveStore`: versioned durable local persistence
- `IClock`: testable local calendar time
- `IShiftSeedProvider`: standard and daily seed source

Keep platform SDK calls behind these boundaries. The Core assembly must stay free of Unity references.

Unity Localization brings Addressables as a package dependency and manages its own locale/table groups. Gameplay content does not use Addressables, remote catalogs, or content updates; generated `addressables_content_state.bin` files are ignored.
