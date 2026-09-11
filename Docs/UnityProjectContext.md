# Unity project context

## Baseline

Historical baseline checked: 2026-09-08, main `d15e39a` plus emulator QA fixes for stale ad callbacks, incident rules-panel sizing, localized fallback destinations, case-specific endings, and completed-case board spacing. Runtime architecture below was updated for the hands-on investigations change on 2026-09-11; this is not a new validation result. Repository `AGENTS.md` is authoritative for automation permission.

- Editor: Unity `6000.3.21f1`, pinned by `ProjectSettings/ProjectVersion.txt`
- Packages: URP 17.3.0, Input System 1.20.0, Localization 1.5.8, uGUI/TMP 2.0.0, Test Framework 1.6.0
- Ads: Google Mobile Ads Unity 11.3.0 and EDM4U 1.2.188. UMP controls request permission; normal gameplay remains available during consent failure or offline operation.
- Android: `com.joyshu93.curioclerknightshift`, API 29–36, ARM64, IL2CPP, AAB, portrait
- Scenes: `Bootstrap` loads `Main`; `GameApp` changes the UI between Menu, Narrative, Workbench, Tutorial, Shift, Results, Collection/Casebook, and Settings.

## Runtime flow

The incident board resolves saved progress across Unmelting Ice and Remembering Rain. The story route is `StartIncident` → `ShowIncidentIntro` → the workbench introduction → `BeginIncidentStage` → `BeginWorkbench`. Each of the ten existing stage IDs opens one object problem with inspectable parts and usable tools. Story advancement no longer requires twelve classified objects or destination stamps. The old standalone tutorial is not the first-launch story entry.

`Assets/Scripts/Runtime/Content/Workbench/WorkbenchCatalog.cs` is the source of truth for the ten workbench scenes: objective, dialogue, observations, tool labels, target positions and visibility, action prerequisites, results, and next clues. `FirstIncidentCatalog` and `SecondIncidentCatalog` retain stable case/stage IDs, chronology, and legacy sorting/narrative definitions; their ordered sorting rules no longer drive story play. Pure C# `Core/Workbench` defines immutable puzzles/steps and `WorkbenchSession`, which records observations and completed actions. Wrong experiments leave progress intact; authored dependencies prevent premature actions, and the final action reports completion once.

`GameApp.Workbench.cs` presents the object, visible targets, findings and tool tray using uGUI/TMP. `WorkbenchToolDrag` supports drag/drop alongside tap-tool-then-target input and cancels a drag when its page is disabled. `WorkbenchToolIcon` draws tool silhouettes; `WorkbenchAtmosphere` animates decorative reactions without delaying domain completion. `Localizer` selects English/Korean; workbench content uses `LocalizedCopy`, including retained messages, so returning after a language change displays the selected language.

`PlayerSaveData` remains version 4. The workbench uses the existing `IncidentRunner` and `ProgressionService.ApplyIncidentStage` boundary, records completion using the existing `IncidentQuality.Stable` value, and adds no new sorting grade or save schema. Completed stages and discovered artifacts persist; an unfinished workbench remains in memory through a menu detour, while a process restart returns to the completed-stage checkpoint. Replay does not award or overwrite saved progress. `JsonFileSaveStore` writes a temporary JSON file and replaces the primary while preserving a backup. A corrupt primary falls back to the backup; two corrupt files fall back to a sanitized default.

The separate Free Shift practice mode preserves twelve-item, three-stamp sorting. `ContentCatalog` supplies the 24 artifacts, 10 rule templates, five difficulty bands, and six cosmetics; `ShiftPlanGenerator` produces deterministic plans from a seed. `RuleEngine` evaluates rules from top to bottom and requires the final catch-all. `ShiftSession` owns Hold, hearts, combo, score, coins, and the one rewarded action per shift. `ProgressionService` applies completed sorting results once. Sorting result screens intentionally hide monetization offers; private reward methods and their service boundaries remain covered by automated tests.

## Generated assets

`CurioClerk.Editor.ProjectBuilder.BuildAll` is idempotent and creates or updates:

- `Assets/Resources/Content`: 24 Artifact, 10 Rule, 5 Difficulty, 6 Cosmetic assets
- `Assets/Localization`: English/Korean locales and UI string tables
- `Assets/Rendering`: URP asset with 2D Renderer
- `Assets/Scenes`: Bootstrap and Main
- Android player settings, build scenes, icon, and Noto Sans KR dynamic TMP font

`BuildAll` also configures the imported workbench atlas as a Sprite texture with a 2048 maximum size, NPOT scaling disabled, and a full-rectangle sprite mesh. Run `BuildAll` after content, localization, scene, font, icon, player-setting or art changes; change catalogs/builders instead of hand-editing generated assets.

`Assets/Resources/Art/Workbench/workbench-states.png` is one original six-panel state atlas derived from existing project-owned curio art. `WorkbenchArtwork` loads it at runtime, chooses the state from stable stage/action IDs, and scales authored crop rectangles from the 1536 × 1024 source coordinates to the actual imported texture size. The base and letter use adjusted row boundaries to retain the base ornament without including it in the letter. The PNG is retained unchanged: its dark-plum background is opaque with slight color variation, rather than true alpha or an exact flat color. The workbench backdrop is matched to approximately RGB(33,20,30); the atlas should not be treated as a transparent cutout on arbitrary background colors. Prompts and asset limitations are recorded in `Docs/AIAssetProvenance.md`.

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
