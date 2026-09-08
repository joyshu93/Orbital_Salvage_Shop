# Android QA and GMA integration completion plan

> Execute inline in the existing checkout. Independent code review is required before merge.

**Goal:** Complete the restored Android QA/GMA work without losing local changes or merged Remembering Rain content.

**Architecture:** Keep the existing replaceable ads/privacy boundaries and pinned UPM packages. Limit implementation to reproducible Android tooling, QA build configuration and restoration, with tests before fixes.

**Tech stack:** Unity 6000.3.21f1, GMA Unity 11.3.0, EDM4U 1.2.188, PowerShell, NUnit.

**Spec:** User's 2026-09-08 integration request and repository `AGENTS.md`.

## Constraints

- Work in `codex/android-qa-gma-integration` in the current checkout; preserve the story worktree, backup branch and stash.
- Android portrait, API 29/36, ARM64 IL2CPP; this task produces a development APK with debug signing and Google sample IDs only.
- Offline play remains complete; rewarded ads are opt-in; no remote telemetry or account requirement.
- Preserve English/Korean content. No release AAB, live IDs, production signing or deployment.
- Keep original dirty serialization files locally and exclude unrelated differences from the integration commit.

## Execution

- [x] Verify refs, ancestry and dirty paths; copy every initial dirty/untracked file with SHA-256 into ignored `Logs/AndroidQaIntegration-20260908/baseline`.
- [x] Create the requested branch without a new worktree.
- [x] Review each restored file and existing ads/privacy boundaries; record inclusion decisions in the validation report.
- [x] Add failing EditMode/PowerShell coverage for confirmed defects; make the smallest fixes and verify red/green evidence.
- [x] Audit SDK-generated Gradle/linker files and update provenance/notices with exact package licenses and dependencies.
- [x] Execute `ProjectBuilder.BuildAll`, `scripts/test-unity.ps1`, development/offline/release static contracts, Android preflight and telemetry exclusion tests.
- [x] Execute `scripts/build-android-dev.ps1`; inspect APK ABI, manifest, sample ID and debug signing; preserve validation logs.
- [x] Finish independent review and fix Critical/Important findings, including the final process-wait regression.

Publication follows this pre-commit validation record: stage only integration files, commit, push and create the PR; inspect checks/reviews before merge; then verify local recovery and preserved safety references. The PR and final handoff record those Git operations. See `Docs/QA/AndroidQaGmaIntegration-20260908.md` for measured results and remaining device checks.
