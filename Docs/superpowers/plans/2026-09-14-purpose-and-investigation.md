# Purpose and investigation implementation plan

> Execute inline in the existing isolated worktree using the repository's test-first workflow. The user has authorized routine game-design decisions without repeated approval questions.

**Goal:** Keep the reason for investigating, the evidence and the intended tool action understandable throughout both existing cases.

**Architecture:** Extend WorkbenchCatalog and GameApp presentation. Preserve domain progress, case/stage identities and save version 4. Selection and case notes are transient presentation state; notes derive from the current stage and observed evidence.

**Tech stack:** Unity 6000.3.21f1, C#, uGUI/TMP, English/Korean, Android portrait API29/36 ARM64 IL2CPP.

**Design:** September14 feedback reports unclear motivation, tiny + targets, ambiguous selection and weak enjoyment. Display a persistent case purpose and current question. Connect each object to the previous discovery. Named targets always inspect/focus; tool selection preserves that observation. An explicit Use here button executes the focused combination; drag remains a deliberate alternative. Show the selected tool with a badge, border and functional description. Reopen the case request and known clues in a notebook without future spoilers. Shorten the duplicate Ice04 heat/lid/tweezers sequence. These changes target understanding, experimentation and pacing; no automated result establishes human enjoyment.

## Constraints

- Preserve original22 dirty files, other worktrees, saves and prior APKs/companions.
- Use `.worktrees/immersive-cases-20260911`, branch `codex/immersive-cases-20260911`.
- Do not control, update or restart the personal AVD during QA. Use the dedicated QA profile only.
- No uninstall, clear, save deletion, account/ad dependency, release signing, AAB, merge or release.
- Record bilingual copy provenance before authoring; use BuildAll for generated assets.

## 1. Reproduce the experience failures

Files: `Assets/Tests/PlayMode/WorkbenchPlayModeTests.cs`, `Assets/Tests/EditMode/WorkbenchCatalogTests.cs`.

- [x] Add UI tests: persistent purpose, named large target controls, retained observation, explicit selected badge, target tap cannot execute a selected tool, Use here applies, notebook contains only known evidence, restart clears selection.
- [x] Add a catalog regression for two-action Ice04, stable stage ID and reachable key.
- [x] Run focused Unity RED tests with the reviewed temporary Editor save guard and unique evidence paths.

## 2. Implement connected investigation

Files: `WorkbenchCatalog.cs`, new `WorkbenchCaseContext.cs`, `GameApp.Workbench.cs`, `GameApp.WorkbenchHints.cs`, new `GameApp.WorkbenchContext.cs`.

- [x] Add immutable bilingual `WorkbenchCaseContext.Find(string stageId)` containing purpose and previous-discovery recap. Revise current objectives/introductions to ask physical questions instead of reciting the solution.
- [x] Render context in the introduction, workbench and notebook. Preserve current session and tool selection when closing the notebook; no save mutation.
- [x] Make `TapWorkbenchTarget` inspect/focus and `UseSelectedWorkbenchTool` apply only a valid selected tool and visible target. Keep domain prerequisites and direct drag behavior.
- [x] Add numbered contrast markers with large named target buttons. Keep observation visible while showing tool function separately; add selection badge and an explicit Use here control.
- [x] Re-inspecting a changed target returns its latest action result. Wrong experiments on the initial objects explain the material obstruction instead of only rejecting a combination.
- [x] Compress Ice04 into spring-lid warming (`lift-back`) and key retrieval (`take-key`). Preserve stage persistence and artwork IDs.
- [x] Migrate button-path regressions to explicit Use and validate both languages at 1080x1920/2400 layouts, including hints, drag, notebook, save/resume and replay.

## 3. Verify and hand off one candidate

- [x] Run `scripts/test-unity.ps1`, BuildAll and repository development APK build via a wrapper that archives/restores canonical outputs and logs.
- [x] Verify package settings and APK hash. Back up dedicated QA save, update that profile, compare save bytes.
- [x] Exercise the first introduction through existing-save replay, readable targets, tool selection, deliberate action, wrong experiment, hint, menu detour/restart and both case transitions with actual UI inputs. Inspect EN/KO screens; distinguish source-aware QA from natural human play.
- [x] Resolve important defects, record remaining limitations in QualityReview and candidate QA, remove temporary validation guards/noise, and review the diff.
- [x] Commit/push this cycle and update the existing improvement PR with source/APK/validation evidence. No personal environment update during QA; a subsequent save-preserving personal handoff is separate.

Completed September16. [QA and exact limits](../../QA/PlayerIntent-20260914.md) record the final candidate, source identity, native replay and separate personal handoff. PR #6 was updated and remains open/unmerged. Human enjoyment and existing object-state art inconsistencies remain open in QualityReview.
