# Hands-on investigations implementation plan

> Agentic execution: coordinated parallel implementation in this isolated worktree; the developer delegated implementation choices and asked to proceed without further design questions.

**Goal:** Replace both story cases' repetitive classification with object inspection and tools, concrete goals, visible consequences, and natural bilingual dialogue.

**Architecture:** Plain C# workbench state feeds a dedicated GameApp partial presenter. Existing incident IDs, progression, save format, artwork and free sorting practice remain reusable.

**Tech Stack:** Unity 6000.3.21f1, C#, uGUI/TMP, existing Input System/EventSystem.

**Spec:** ../specs/2026-09-11-hands-on-investigations-design.md

## Global constraints

- Unity 6000.3.21f1; portrait Android API 29/36, ARM64 IL2CPP debug APK; official sample ads only.
- All player-facing text English and Korean; provenance/notices updated before copy changes.
- No AAB, release, merge, deletion of saves or unrelated changes. Existing dirty files, prior worktrees/APKs/logs retained.
- Core has no UnityEngine; generated content/scenes come from ProjectBuilder.BuildAll.
- Parent alone starts Unity and Android operations after confirming exact worktree/device.

## Tasks

1. **State model + EditMode tests.** Create Core/Workbench/WorkbenchSession.cs and Tests/EditMode/WorkbenchSessionTests.cs. Write tests for observations, prerequisite ordering, wrong-tool recovery, duplicate applications and completion. Start with minimal signatures/stubs, capture expected failing assertions before implementation. `new WorkbenchSession(puzzle); session.Observe("crack"); session.Apply("cloth", "crack");` must complete only its authored step; wrong targets leave completion unchanged.
2. **Ten bilingual scenes + validation tests.** Create Content/Workbench/WorkbenchContent.cs and WorkbenchCatalog.cs. Every old stage ID maps to one concrete problem, two to four tools, two to four visual targets, authored interventions, inspected observations, and successful/blocked feedback. Test all references, both languages, distinct stories and solvability before filling catalog data. Author clue-driven causal actions and readable colloquial NPC speech.
3. **Workbench UI + story integration.** First add PlayMode regression asserting story entry reaches the workbench with an objective and interactive object rather than a sorting session. Implement GameApp.Workbench.cs, WorkbenchToolDrag.cs and precise entry/result changes. Integrate existing IncidentRunner and ApplyIncidentStage once; preserve replay and old saves. Add meaningful input, modal/menu, completion, locale and aspect-ratio coverage. Update obsolete story tests to explicitly represent the new behavior; preserve sorting rule tests.
4. **Validation and delivery.** Run repository Unity tests, BuildAll and development APK script in this worktree, archiving interim logs. Review full change; resolve actionable failures. Exercise both cases, wrong experiments, drag/drop, menu/restart and replay on dedicated Android. Produce QA/change report, source/hash/build settings. Install verified APK for the user's next play while preserving existing progress.

## Progress

- Worktree based on 7287dc70; baseline previous APK/tests preserved.
- Design decisions are delegated by the developer; no pending design approval.
