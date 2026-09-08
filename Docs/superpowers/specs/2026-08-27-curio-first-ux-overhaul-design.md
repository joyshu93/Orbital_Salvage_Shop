# Curio-First UX Overhaul Design

**Date:** 2026-08-27
**Status:** Approved in conversation; pending written-spec review
**Selected direction:** B1 layout + M1 motion + T1 tutorial + F1 typography

## Context

The Three-Seal Dockets implementation made the deterministic core loop deeper: every docket requires one Repair, one Storage, and one Vault stamp, and a generated shift contains moments where Hold is necessary. EditMode 96/96 and PlayMode 34/34 passed after content generation.

The first manual playtest still failed the product goal. The player completed the tutorial but did not understand what the game was asking them to do or why Hold was necessary. The portrait shift screen also exposed presentation problems:

- completed and empty docket stamps were hard to distinguish;
- Hold used only a subtle outline and did not explain the blocking state;
- feedback from the previous artifact remained visible while judging the next artifact;
- rules, previews, traits, and buttons used small, low-contrast type;
- a large rules panel contained unused space while essential copy stayed small;
- artifact illustrations were attractive but static;
- correct, wrong, Hold, and docket-complete events did not feel sufficiently different.

This overhaul keeps the Three-Seal domain rules and existing art, but redesigns the tutorial and presentation so a new player can understand the loop from the screen itself.

## Goals

1. Explain that one docket is completed by using Repair, Storage, and Vault exactly once each.
2. Explain Hold as a timing tool: the artifact's correct destination is known, but that destination has already been used in the current docket.
3. Keep the artifact illustration as the visual focus without hiding the rules needed for judgment.
4. Make Korean and English readable on an Android portrait screen without leaning closer to the display.
5. Give correct, wrong, Hold, and docket-complete events distinct emotional feedback.
6. Reuse the existing 24 illustrations, background, icons, deterministic shift logic, and procedural UI architecture.
7. Keep the implementation appropriate for one developer and a short iteration.

## Non-goals

- No new game mode, metagame, timer, energy system, account, server, telemetry, or advertising work.
- No new artifact illustration or frame-by-frame animation.
- No complex particle framework or shader system.
- No scene hand-editing; generated scenes remain owned by `ProjectBuilder.BuildAll`.
- No Android release build or store-readiness work in this iteration.
- No changes to the deterministic Three-Seal rules, shift-plan balance, scoring rules, or save format unless a test proves a presentation requirement cannot be met without one.

## Selected Layout: B1 Curio-First, Rules Always Visible

The portrait shift screen is divided into five vertically ordered regions.

1. **Compact HUD:** hearts, docket number, pristine count, and coins.
2. **Docket strip:** three large labeled stamps. A filled stamp uses a solid destination color, a check mark, and `Complete/완료`. An empty stamp uses a neutral low-opacity surface, a dashed edge, and `Empty/빈칸`.
3. **Readable rules:** all three prioritized rules remain visible at once. The panel shrinks to its content instead of reserving unused height.
4. **Large curio card:** the illustration is the dominant element. The display-font artifact name, readable description, and bold trait line sit next to or beneath it without competing with the art.
5. **Current-state feedback and actions:** one message describes only the current artifact state, followed by Hold and the three large destination buttons.

The next-two and held previews remain available but are visually subordinate. They may use artwork plus a short name; they must not use essential text smaller than the minimum body size. Click and drag input both remain supported.

When a new artifact enters, feedback from the previous artifact is cleared. The default message becomes a neutral prompt to compare its traits with the rules.

## Hold Communication

When the current artifact resolves to a destination that is already stamped in the current docket, four cues communicate the same fact:

1. the completed destination stamp remains solid and labeled `Complete/완료`;
2. that destination action is visibly disabled;
3. the current-state message says, for example, `The correct desk is Repair, but it is already full. Hold this curio.` / `정답은 수리실이지만 이미 찼어요. 이 물건을 보류하세요.`;
4. the Hold action gains a high-contrast glow and the label `Hold for the next docket/다음 장부까지 보류` where space permits.

Hold is no longer communicated by outline alone. Using Hold slides the curio toward the held shelf, updates the held preview, and advances the queue. The held curio returns through the normal artifact-entry motion later in the shift.

## Motion and Feedback: M1 Cozy Reactive

Motion uses the existing illustration sprites, RectTransform movement, scale, color, and alpha. It must remain restrained and support the warm occult tone.

- **Idle:** the artifact illustration drifts vertically by roughly 2-4 px with a slow, irregular-feeling cycle and a soft glow. Text and the whole card do not float.
- **Artifact entry:** a short settle-in motion makes each arrival distinct.
- **Correct:** the illustration lifts slightly, the selected destination receives a stamp impact, and a small warm flash appears. The reason and artifact resolution appear once.
- **Wrong:** the curio card makes one short horizontal shake, the heart count changes, and the correct rule reason appears. The artifact remains current.
- **Hold:** the illustration moves toward the held shelf and the held preview responds. This must read as placing an object aside, not discarding it.
- **Docket complete:** the three stamps brighten together, followed by a roughly 0.7-second docket card showing `Pristine/말끔함` or `Inked/번짐` and the earned coin response. It must not interrupt the 60-90-second shift rhythm.
- **Result screen:** the four docket rows enter in a short sequence and the final artifact resolution uses the display typeface. No rewarded-ad control is shown.

Existing sound and haptic feedback cues remain connected to the same events. No new audio asset is required in this iteration.

## Tutorial: T1 Guided First Docket

The tutorial uses the existing six fixed artifacts and two-docket structure, but removes dependence on a text-heavy introduction.

1. The opening shows one sentence: `Each docket needs one Repair, one Storage, and one Vault stamp.` / `장부 하나에는 수리실·보관실·봉인고 도장이 하나씩 필요해요.`
2. During the first three artifacts, the matching trait word and rule word receive the same emphasis. The player still performs each action. Every choice produces a clear stamp impact.
3. On the first docket completion, a short card states: `One docket complete. You used each desk once.` / `장부 하나 완성! 세 장소를 한 번씩 사용했습니다.`
4. The second docket creates the fixed duplicate-destination situation. The completed stamp, disabled destination, explicit reason sentence, and highlighted Hold action appear together.
5. Holding moves the artifact to the shelf. The player files the other two artifacts, then sees the held artifact return and completes the second docket.
6. Tutorial mistakes do not cost hearts. The artifact remains, while the matching trait and rule receive renewed emphasis.
7. The closing sentence is: `Fill all three stamps. If a curio's desk is already full, Hold it.` / `세 도장을 채우고, 자리가 찬 물건은 보류하세요.`

The tutorial target remains approximately 60 seconds. It retains the existing `tutorialCompleted` save flag and does not add a reset or account requirement.

## Typography: F1 Gowun Batang + Noto Sans KR

Typography uses two roles only.

- **Gowun Batang:** titles, artifact names, resolution copy, and short emotional result lines.
- **Noto Sans KR:** rules, traits, HUD, buttons, tutorial instructions, and all small functional text.

Rules and buttons use a medium or bold Noto Sans KR weight, stronger contrast, and larger minimum sizes than the current screen. Essential labels must not be reduced to make a fixed layout fit; the layout must reflow or remove nonessential detail first.

Gowun Batang is sourced from the official Google Fonts repository and includes an OFL license: <https://github.com/google/fonts/tree/main/ofl/gowunbatang>.

Before adding the font file, update `Docs/AIAssetProvenance.md` and `Docs/ThirdPartyNotices.md`. Commit the license alongside the font source. `ProjectBuilder` generates or updates the corresponding TMP asset deterministically.

## Presentation Architecture

Core domain code remains unchanged unless a failing test demonstrates a missing presentation-safe query.

- `GameApp` owns the B1 layout, localized current-state copy, and tutorial stage transitions.
- `DocketProgressView` owns filled/empty stamp labels, visuals, and docket-complete presentation.
- `ShiftFeedbackAnimator` owns artifact idle, entry, correct, wrong, Hold, and result transitions. If this creates mixed responsibilities, extract only the artifact-specific behavior into a small `ArtifactMotionView`; do not introduce a general animation framework.
- `ProjectBuilder` owns generation and assignment of both TMP font assets.
- `Localizer` owns every new player-facing English and Korean string.

The presentation components consume existing `ShiftSession` facts such as the current resolution, stamped destinations, held artifact, completed dockets, and result ledger. Presentation code must not duplicate rule resolution or queue logic.

## Generated Assets and Working Tree

The current uncommitted generated artifact-resolution and localization assets belong to the pre-overhaul presentation pass. They must not be committed as the final generation output. After the overhaul source changes are complete, the human developer runs `ProjectBuilder.BuildAll`, and only the newly reviewed generated content and localization diffs are staged.

Unity-generated artifact `.png.meta` rewrites remain out of scope unless inspection proves a deliberate importer change. Scene, ProjectSettings, font, advertising, or plugin diffs that are not required by this design are rejected.

## Automated Validation

The current baseline is EditMode 96/96 and PlayMode 34/34. New tests must be written first for behavior changes and must cover at least:

- all new localization keys in English and Korean;
- correct filled/empty stamp view state;
- explicit Hold-required message and emphasis;
- clearing previous-artifact feedback when the next artifact enters;
- fixed tutorial order, two completed dockets, and required Hold;
- no heart loss for tutorial mistakes;
- display-font assignment for titles, artifact names, and resolutions;
- body-font assignment for rules, traits, instructions, and buttons;
- dispatch of distinct correct, wrong, Hold, artifact-entry, and docket-complete presentation states;
- continued absence of a visible rewarded-ad control.

After source changes, the human developer runs:

1. `scripts/test-unity.ps1`;
2. `ProjectBuilder.BuildAll` for content, localization, font, and generated presentation assets;
3. `scripts/test-unity.ps1` again.

Agents do not launch Unity Editor, Hub, batch mode, or Unity MCP.

## Manual Acceptance Criteria

The minimum manual gate is one Korean tutorial and one normal shift, followed by a compact English layout check.

1. After completing the first tutorial docket, the player can state without external explanation that one docket requires each destination once.
2. In the first normal-shift blocked-destination situation, the player understands within five seconds why Hold is required.
3. Rules, traits, buttons, and feedback are readable at portrait phone scale without moving closer to the screen.
4. Correct, wrong, Hold, and docket completion look and feel like distinct events.
5. Previous-artifact feedback never interferes with judging the current artifact.
6. Korean and English screens have no clipping, overlap, or unsafe touch targets.
7. A 12-artifact shift remains naturally completable in roughly 60-90 seconds without a forced timer.

Criteria 1 and 2 are release-blocking for this iteration. If either fails, the UX returns to design or implementation rather than being accepted on visual polish alone.

## Completion Boundary

This iteration is complete when the approved B1/M1/T1/F1 presentation is implemented, automated validation is green, generated diffs are reviewed, and the minimum manual gate passes. Android build, device installation, advertising, Firebase, and store preparation remain explicitly deferred.
