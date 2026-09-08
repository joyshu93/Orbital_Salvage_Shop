# Curio-First UX Overhaul Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Rebuild the Three-Seal shift presentation so a new portrait-mobile player understands the docket goal and Hold timing while the existing curio art becomes readable, alive, and emotionally responsive.

**Architecture:** Keep deterministic rules, queueing, scoring, and save data in the existing Core types. Recompose the procedural uGUI screen in `GameApp`, make `DocketProgressView` authoritative for explicit stamp state and docket completion, extend `ShiftFeedbackAnimator` for short curio transitions, and add one focused result-row animator. Generate the second TMP font deterministically through `ProjectBuilder`.

**Tech Stack:** Unity 6000.3.21f1, C#, uGUI, TextMesh Pro, NUnit EditMode and PlayMode tests, Unity Localization assets generated from `Localizer`.

**Spec:** `Docs/superpowers/specs/2026-08-27-curio-first-ux-overhaul-design.md`

## Global Constraints

- Unity is pinned to `6000.3.21f1`; agents do not launch Unity Editor, Hub, batch mode, or Unity MCP.
- Android remains portrait, API 29 minimum, API 36 target, ARM64, IL2CPP, and fully playable offline.
- Every player-facing string is authored in both English and Korean in the same task.
- No timer, energy system, account, server, telemetry expansion, banner/interstitial/app-open ad, or visible rewarded-ad control.
- Preserve the existing Three-Seal domain logic, 12-artifact shift, four dockets, and deterministic Hold-solvable plans.
- Update `Docs/AIAssetProvenance.md` and `Docs/ThirdPartyNotices.md` before adding the Gowun Batang font file.
- Generated content, localization, scenes, and TMP assets come from `ProjectBuilder.BuildAll`; do not hand-edit generated output.
- Preserve unrelated working-tree changes. In particular, do not stage the existing artifact `.png.meta` rewrites.
- The current generated artifact-resolution and localization diffs remain unstaged until the final regeneration task.
- Use subagents only for bounded independent review or documentation work. Tasks that touch `GameApp.cs` run serially.

---

## File Structure

### New files

- `Assets/Fonts/GowunBatang/GowunBatang-Bold.ttf`: official display-font source.
- `Assets/Fonts/GowunBatang/OFL.txt`: font license copied from the same official source directory.
- `Assets/Scripts/Runtime/Presentation/ResultLedgerAnimator.cs`: staggered result-row reveal only.
- `Assets/Tests/PlayMode/ShiftPresentationViewPlayModeTests.cs`: focused component tests for docket and animation views.

Unity creates the matching `.meta` files when the human opens/imports the project. Stage them only after confirming their paths and importer types.

### Modified source files

- `Assets/Scripts/Editor/ProjectBuilder.cs`: deterministic generation of Noto Sans KR and Gowun Batang TMP assets.
- `Assets/Scripts/Runtime/Localization/Localizer.cs`: bilingual B1/T1 state copy.
- `Assets/Scripts/Runtime/Presentation/GameApp.cs`: B1 layout, typography roles, current-state copy, asynchronous filing transitions, and T1 tutorial.
- `Assets/Scripts/Runtime/Presentation/DocketProgressView.cs`: explicit empty/complete labels, destination-specific stamped colors, and docket-complete pulse.
- `Assets/Scripts/Runtime/Presentation/ShiftFeedbackAnimator.cs`: M1 idle, entrance, correct, wrong, and Hold transitions.
- `Assets/Tests/EditMode/ReleaseAssetContractTests.cs`: committed body/display TMP asset contract.
- `Assets/Tests/PlayMode/GameAppPlayModeTests.cs`: screen hierarchy, typography, Hold reason, tutorial, results, and regression coverage.
- `Docs/AIAssetProvenance.md`: human-approved font sourcing record.
- `Docs/ThirdPartyNotices.md`: Gowun Batang and OFL notice.
- `Docs/UnityProjectContext.md`: final test and manual-validation evidence only after the human supplies it.

### Generated files reviewed only after `ProjectBuilder.BuildAll`

- `Assets/Resources/Fonts/GowunBatang-Bold-Dynamic.asset`
- `Assets/Localization/UI Shared Data.asset`
- `Assets/Localization/UI_en.asset`
- `Assets/Localization/UI_ko.asset`
- `Assets/Resources/Content/Artifacts/*.asset`

---

### Task 1: Add the Approved Display Font and Typography Roles

**Files:**
- Modify first: `Docs/AIAssetProvenance.md`
- Modify first: `Docs/ThirdPartyNotices.md`
- Create after docs: `Assets/Fonts/GowunBatang/GowunBatang-Bold.ttf`
- Create after docs: `Assets/Fonts/GowunBatang/OFL.txt`
- Modify: `Assets/Scripts/Editor/ProjectBuilder.cs:492-543`
- Modify: `Assets/Scripts/Runtime/Presentation/GameApp.cs:36-43,1257-1279`
- Modify: `Assets/Tests/EditMode/ReleaseAssetContractTests.cs`
- Modify: `Assets/Tests/PlayMode/GameAppPlayModeTests.cs:34-75`
- Generate by human: `Assets/Resources/Fonts/GowunBatang-Bold-Dynamic.asset`

**Interfaces:**
- Consumes: `ProjectBuilder.ConfigureFontAssets()`, `GameApp.CreateText(...)`, the committed Noto Sans KR TMP asset.
- Produces: `GameApp.TextRole { Interface, Display }`, `CreateText(..., TextRole role = TextRole.Interface)`, and Resources font `Fonts/GowunBatang-Bold-Dynamic`.

- [ ] **Step 1: Record provenance before adding the binary**

Add a human-approved entry to `Docs/AIAssetProvenance.md` naming the Curio-First UX decision, and add this notice to `Docs/ThirdPartyNotices.md`:

```markdown
### Gowun Batang Bold

- Source: https://github.com/google/fonts/tree/main/ofl/gowunbatang
- File: `Assets/Fonts/GowunBatang/GowunBatang-Bold.ttf`
- License: SIL Open Font License 1.1
- Use: Curio names, titles, and short resolution copy.
```

- [ ] **Step 2: Add the exact official font and license files**

Download only these official raw files:

```powershell
Invoke-WebRequest -Uri 'https://raw.githubusercontent.com/google/fonts/main/ofl/gowunbatang/GowunBatang-Bold.ttf' -OutFile 'Assets/Fonts/GowunBatang/GowunBatang-Bold.ttf'
Invoke-WebRequest -Uri 'https://raw.githubusercontent.com/google/fonts/main/ofl/gowunbatang/OFL.txt' -OutFile 'Assets/Fonts/GowunBatang/OFL.txt'
```

Verify `GowunBatang-Bold.ttf` is non-empty and `OFL.txt` starts with `SIL OPEN FONT LICENSE Version 1.1` before continuing.

- [ ] **Step 3: Write failing font-contract tests**

Extend `ReleaseAssetContractTests`:

```csharp
[Test]
public void ReleaseAssets_ContainBodyAndDisplayTmpFonts()
{
    var body = Resources.Load<TMP_FontAsset>("Fonts/NotoSansKR-Dynamic");
    var display = Resources.Load<TMP_FontAsset>("Fonts/GowunBatang-Bold-Dynamic");

    Assert.That(body, Is.Not.Null);
    Assert.That(display, Is.Not.Null);
    Assert.That(body.name, Does.StartWith("NotoSansKR"));
    Assert.That(display.name, Does.StartWith("GowunBatang-Bold"));
}
```

Update the first PlayMode test so menu `Title` must use Gowun Batang while `StartShiftButton` must continue to use Noto Sans KR.

- [ ] **Step 4: Ask the human to run the red test**

Run from the worktree with Unity Editor closed:

```powershell
.\scripts\test-unity.ps1
```

Expected: EditMode fails `ReleaseAssets_ContainBodyAndDisplayTmpFonts` because `GowunBatang-Bold-Dynamic` has not been generated.

- [ ] **Step 5: Generalize deterministic TMP asset creation**

Replace the single-font body of `ConfigureFontAssets()` with two calls:

```csharp
EnsureTextMeshProResources();
ClearDefaultTmpSpriteAsset();
EnsureDynamicFontAsset(
    "Assets/Fonts/NotoSansKR/NotoSansKR-Variable.ttf",
    "Assets/Resources/Fonts/NotoSansKR-Dynamic.asset",
    "NotoSansKR-Dynamic");
EnsureDynamicFontAsset(
    "Assets/Fonts/GowunBatang/GowunBatang-Bold.ttf",
    "Assets/Resources/Fonts/GowunBatang-Bold-Dynamic.asset",
    "GowunBatang-Bold-Dynamic");
```

`EnsureDynamicFontAsset(string sourcePath, string assetPath, string assetName)` must import the source synchronously, create the TMP asset only when absent, set `AtlasPopulationMode.Dynamic`, add its atlas and material as sub-assets, and mark the asset dirty. Preserve the existing TMP default-sprite clearing behavior in `ClearDefaultTmpSpriteAsset()`.

- [ ] **Step 6: Add font roles to `GameApp`**

Add:

```csharp
private enum TextRole { Interface, Display }
private static TMP_FontAsset s_InterfaceFont;
private static TMP_FontAsset s_DisplayFont;
```

Extend `CreateText` with `TextRole role = TextRole.Interface`, load both Resources fonts once, and assign the display font only for `TextRole.Display`. Apply Display to menu title, artifact name, result title, result resolution, Casebook artifact names, and tutorial-complete title. Keep rules, traits, HUD, instructions, and every button label on Interface.

- [ ] **Step 7: Ask the human to generate font assets**

In the correct worktree project, run `Tools > Curio Clerk > Generate Project Assets`. Expected validator output still includes `24 artifacts, 10 rules, 2 rule packs, 3 docket templates, 5 difficulties, 6 cosmetics, 2 scenes`.

- [ ] **Step 8: Ask the human to run the green test**

Run `.\scripts\test-unity.ps1` with the Editor closed. Expected: the new font contract and title/button font-role assertions pass; all prior tests remain green.

- [ ] **Step 9: Commit only the font task**

Stage the two docs, official source and license plus their `.meta` files, `ProjectBuilder.cs`, `GameApp.cs`, two test files, and generated Gowun TMP asset plus `.meta`:

```powershell
git commit -m "feat: add curio display typography"
```

Do not stage localization, artifact assets, scenes, ProjectSettings, or artifact image `.meta` files.

---

### Task 2: Make Docket Stamps Explicit

**Files:**
- Modify: `Assets/Scripts/Runtime/Localization/Localizer.cs`
- Modify: `Assets/Scripts/Runtime/Presentation/DocketProgressView.cs`
- Modify: `Assets/Scripts/Runtime/Presentation/GameApp.cs:1297-1352`
- Create: `Assets/Tests/PlayMode/ShiftPresentationViewPlayModeTests.cs`

**Interfaces:**
- Consumes: `DocketState.IsStamped(Destination)` and `Localizer.Get(...)`.
- Produces: `DocketProgressView.Configure(TMP_Text, IReadOnlyList<Image>, IReadOnlyList<TMP_Text>, Color, IReadOnlyList<Color>)` and `Refresh(DocketState, int, int, string, string)`.
- Produces: bilingual `docket_empty` and `docket_complete` localization values supplied by `GameApp`.

- [ ] **Step 1: Write failing component tests**

Create `ShiftPresentationViewPlayModeTests` with a helper that constructs one counter, three Image surfaces, and three TMP labels. Add:

```csharp
[UnityTest]
public IEnumerator DocketProgress_LabelsEmptyAndCompletedStamps()
{
    var view = CreateDocketView(out var labels, out var surfaces);
    var docket = new DocketState();
    docket.TryStamp(Destination.Repair);

    view.Refresh(docket, 0, 4, "EMPTY", "COMPLETE");
    yield return null;

    Assert.That(labels[0].text, Is.EqualTo("COMPLETE"));
    Assert.That(labels[1].text, Is.EqualTo("EMPTY"));
    Assert.That(labels[2].text, Is.EqualTo("EMPTY"));
    Assert.That(surfaces[0].color, Is.Not.EqualTo(surfaces[1].color));
}
```

Add a second test confirming counter `4 / 4` remains valid after all required dockets are complete.

Add integration assertions that the three unstamped labels read `EMPTY` in English and `빈칸` in Korean.

- [ ] **Step 2: Ask the human to run the red test**

Run `.\scripts\test-unity.ps1`. Expected: PlayMode compilation or tests fail because the new Configure/Refresh signatures and status labels do not exist.

- [ ] **Step 3: Add exact localized stamp labels**

Add these keys to both `Localizer` dictionaries:

```csharp
["docket_empty"] = "EMPTY",
["docket_complete"] = "COMPLETE",
```

```csharp
["docket_empty"] = "빈칸",
["docket_complete"] = "완료",
```

- [ ] **Step 4: Implement explicit stamp state**

`DocketProgressView` stores three labels and three stamped colors. `Refresh` assigns:

```csharp
var stamped = docket != null && docket.IsStamped((Destination)index);
_stamps[index].color = stamped ? _stampedColors[index] : _openColor;
_labels[index].text = stamped ? completedLabel : openLabel;
```

Validate all lists contain exactly three non-null entries.

- [ ] **Step 5: Build labeled stamps in `GameApp`**

Change `CreateDocketStamp` to return its surface and expose an `out TMP_Text statusLabel`. Use one icon plus a bold status label. Supply destination colors `[DustyRose, Sage, Amber]`. Call `Refresh` with localized `docket_empty` and `docket_complete`.

- [ ] **Step 6: Ask the human to run the green test**

Run `.\scripts\test-unity.ps1`. Expected: the new docket component tests and all prior tests pass.

- [ ] **Step 7: Commit**

Stage only `Localizer`, the view, `GameApp`, focused PlayMode test, and generated `.meta` for the new test file:

```powershell
git commit -m "feat: make docket stamp state explicit"
```

---

### Task 3: Recompose the Shift Screen Around the Curio

**Files:**
- Modify: `Assets/Scripts/Runtime/Presentation/GameApp.cs:432-513,1297-1411`
- Modify: `Assets/Tests/PlayMode/GameAppPlayModeTests.cs:34-115`

**Interfaces:**
- Consumes: Task 1 `TextRole`, Task 2 labeled `DocketProgressView`, existing artwork and drag handler.
- Produces: stable B1 object names and anchor bands used by later tests: `ShiftHud`, `DocketProgress`, `RulesPanel`, `CurrentArtifactCard`, `SortFeedbackPanel`, `HoldButton`, and destination buttons.

- [ ] **Step 1: Write failing B1 hierarchy and readability tests**

Add helpers returning a named object's `RectTransform` and TMP component. Test that:

```csharp
Assert.That(Rect("DocketProgress").anchorMin.y, Is.GreaterThanOrEqualTo(0.84f));
Assert.That(Rect("RulesPanel").anchorMin.y, Is.GreaterThan(Rect("CurrentArtifactCard").anchorMax.y));
Assert.That(Text("RuleList").fontSize, Is.GreaterThanOrEqualTo(24f));
Assert.That(Text("ArtifactName").fontSize, Is.GreaterThanOrEqualTo(40f));
Assert.That(Text("ArtifactTraits").fontSize, Is.GreaterThanOrEqualTo(23f));
Assert.That(Text("RepairButton").fontSize, Is.GreaterThanOrEqualTo(28f));
Assert.That(Rect("CurrentArtifactCard").rect.height,
    Is.GreaterThan(Rect("RulesPanel").rect.height));
```

Also assert `ArtifactName` uses Gowun Batang and `RuleList` uses Noto Sans KR.

- [ ] **Step 2: Ask the human to run the red test**

Run `.\scripts\test-unity.ps1`. Expected: PlayMode fails the new anchor and minimum-size assertions against the current small B1-incompatible screen.

- [ ] **Step 3: Implement B1 anchor bands**

Use these portrait bands as the baseline; adjust only within 0.01 to avoid overlap:

```text
HUD                 y 0.945 - 0.985
Docket strip        y 0.850 - 0.935
Rules panel         y 0.700 - 0.840
Next/Held previews  y 0.625 - 0.690
Curio card          y 0.305 - 0.615
Current feedback    y 0.235 - 0.295
Hold                y 0.175 - 0.225
Destinations        y 0.035 - 0.145
```

Remove the oversized empty portion of the rules panel. Use font sizes: HUD 28, rules header 26, rules 24, artifact name 42 Display, description 25 Interface, traits 24 bold Interface, preview 20, feedback 24, Hold 28, and destination buttons 30. Keep the illustration as the dominant area in `CurrentArtifactCard`.

- [ ] **Step 4: Preserve one-hand input**

Keep the three destination buttons in the bottom reach zone and keep `ArtifactDragHandler` on the curio card. Ensure preview cards do not intercept raycasts over the card or buttons.

- [ ] **Step 5: Ask the human to run the green test**

Run `.\scripts\test-unity.ps1`. Expected: B1 hierarchy, font-role, drag-input, and all regression tests pass.

- [ ] **Step 6: Commit**

```powershell
git add Assets/Scripts/Runtime/Presentation/GameApp.cs Assets/Tests/PlayMode/GameAppPlayModeTests.cs
git commit -m "feat: focus shifts on the current curio"
```

---

### Task 4: Explain the Current Decision and Hold Timing

**Files:**
- Modify: `Assets/Scripts/Runtime/Localization/Localizer.cs`
- Modify: `Assets/Scripts/Runtime/Presentation/GameApp.cs:220-286,697-871`
- Modify: `Assets/Tests/PlayMode/GameAppPlayModeTests.cs:76-120,570-660`

**Interfaces:**
- Consumes: `ShiftSession.CurrentResolution`, `ShiftSession.ShouldSuggestHold`, `DocketProgressView`, `RuleReason(...)`.
- Produces: `RefreshDecisionMessage()`, `SetHoldPresentation(bool required)`, and bilingual keys `decision_prompt`, `hold_required`, `hold_for_next`.

- [ ] **Step 1: Add bilingual failing expectations**

Update `DuplicateDesk_DisablesThatDeskAndSuggestsHold` to assert English contains both `REPAIR` and `already full`, and then rebuild the same authored state in Korean and assert:

```csharp
Assert.That(ObjectText("SortFeedback"), Does.Contain("수리실").And.Contain("이미 찼어요"));
Assert.That(ObjectText("HoldButton"), Does.Contain("다음 장부"));
```

Add a test that a fresh artifact shows only `Compare its traits with tonight's rules.` and does not contain the preceding artifact's resolution.

- [ ] **Step 2: Ask the human to run the red test**

Run `.\scripts\test-unity.ps1`. Expected: PlayMode fails because the current Hold copy is only `That desk is already stamped. Use HOLD.` and the Hold label is generic.

- [ ] **Step 3: Add exact localized copy**

Add these keys to both dictionaries:

```csharp
["decision_prompt"] = "Compare its traits with tonight's rules.",
["hold_required"] = "The correct desk is {0}, but it is already full. Hold this curio.",
["hold_for_next"] = "HOLD FOR NEXT DOCKET",
```

```csharp
["decision_prompt"] = "물건의 특성과 오늘 밤의 규칙을 비교하세요.",
["hold_required"] = "정답은 {0}이지만 이미 찼어요. 이 물건을 보류하세요.",
["hold_for_next"] = "다음 장부까지 보류",
```

- [ ] **Step 4: Drive one authoritative decision message**

`RefreshDecisionMessage()` uses the current session only:

```csharp
if (_session.ShouldSuggestHold)
{
    _statusText.text = _localizer.Get(
        "hold_required",
        DestinationName(_session.CurrentResolution.Destination));
    SetHoldPresentation(true);
}
else
{
    _statusText.text = _localizer.Get("decision_prompt");
    SetHoldPresentation(false);
}
```

`SetHoldPresentation` changes the label and high-contrast outline together. Blocked drag attempts call the same method instead of creating a second message path. Do not label a stamped-destination attempt as Wrong and do not lose a heart.

- [ ] **Step 5: Ask the human to run the green test**

Run `.\scripts\test-unity.ps1`. Expected: bilingual decision/Hold assertions and all previous tests pass.

- [ ] **Step 6: Commit**

```powershell
git add Assets/Scripts/Runtime/Localization/Localizer.cs Assets/Scripts/Runtime/Presentation/GameApp.cs Assets/Tests/PlayMode/GameAppPlayModeTests.cs
git commit -m "feat: explain why curios need hold"
```

Do not stage generated localization tables yet.

---

### Task 5: Add M1 Cozy Reactive Transitions

**Files:**
- Modify: `Assets/Scripts/Runtime/Presentation/DocketProgressView.cs`
- Modify: `Assets/Scripts/Runtime/Presentation/ShiftFeedbackAnimator.cs`
- Modify: `Assets/Scripts/Runtime/Presentation/GameApp.cs:220-286,482-513,697-871`
- Modify: `Assets/Tests/PlayMode/ShiftPresentationViewPlayModeTests.cs`
- Modify: `Assets/Tests/PlayMode/GameAppPlayModeTests.cs`

**Interfaces:**
- Consumes: Task 2 `DocketProgressView`, Task 4 current-decision presentation, and existing `PlayerFeedbackCue` events.
- Produces from `ShiftFeedbackAnimator`: `Configure(RectTransform card, RectTransform artwork, RectTransform feedback, RectTransform heldPreview)`, `PlayCorrect(Action)`, `PlayWrong()`, `PlayHold(Action)`, and `SetIdleEnabled(bool)`.
- Produces from `DocketProgressView`: `PlayComplete(Action)`.

- [ ] **Step 1: Write failing animator tests**

In `ShiftPresentationViewPlayModeTests`, configure real RectTransforms and add tests that:

```csharp
var completed = false;
animator.PlayCorrect(() => completed = true);
yield return new WaitForSecondsRealtime(0.7f);
Assert.That(completed, Is.True);
Assert.That(artwork.anchoredPosition, Is.EqualTo(restPosition));
```

Add equivalent Hold callback/reset coverage and a Wrong test that confirms the artifact card, not only the feedback strip, moves during the animation and returns to rest. Disable the GameObject and assert every Transform resets.

Add a docket-view test that calls `PlayComplete`, waits `0.8f`, confirms its callback fired, and confirms the three stamp surfaces returned to their rest scale.

- [ ] **Step 2: Write the stale-feedback integration test**

Sort one authored artifact correctly. Before the correct transition finishes, assert the old artifact artwork and its resolution are still visible. After `0.9f`, assert the next artwork is visible and `SortFeedback` equals the neutral decision prompt, not the previous resolution.

- [ ] **Step 3: Ask the human to run the red test**

Run `.\scripts\test-unity.ps1`. Expected: PlayMode fails because callbacks, artwork motion, Hold motion, and delayed next-artifact presentation do not exist.

- [ ] **Step 4: Implement animator state safely**

Use unscaled-time coroutines and one routine token per visual target. Durations:

```csharp
private const float EntranceDuration = 0.20f;
private const float CorrectDuration = 0.42f;
private const float WrongDuration = 0.28f;
private const float HoldDuration = 0.34f;
```

Idle moves only the artwork RectTransform by 2-4 px and a very small rotation. Correct lifts the artwork and pulses the selected feedback. Wrong shakes the card once. Hold moves artwork toward the held preview before invoking its callback. `OnDisable` stops all coroutines and restores cached positions, scales, rotations, and alpha.

`DocketProgressView.PlayComplete` brightens and pulses its three stamp surfaces together for `0.70f`, then invokes its callback. Its `OnDisable` stops the completion routine and restores cached stamp colors and scales.

- [ ] **Step 5: Gate input during outgoing transitions**

Add `_inputLocked`. Correct flow becomes:

```csharp
_inputLocked = true;
ShowSortFeedback(artifact, content, outcome, false);
_feedbackAnimator.PlayCorrect(() => CompleteCorrectTransition(outcome));
```

`CompleteCorrectTransition` optionally calls `_docketProgress.PlayComplete(...)`, then either calls `ShowResults()` or `RefreshShiftView()`, clears old feedback through `RefreshDecisionMessage()`, and unlocks input. Hold uses the same callback pattern. Wrong and Blocked keep the current artifact and unlock immediately after their short response.

- [ ] **Step 6: Update tests that perform consecutive tutorial actions**

Replace immediate click chains with a helper:

```csharp
private static IEnumerator WaitForFilingTransition()
{
    yield return new WaitForSecondsRealtime(0.95f);
}
```

Yield it after every correct sort or Hold whose next artifact is asserted. Do not add arbitrary sleeps to production code.

- [ ] **Step 7: Ask the human to run the green test**

Run `.\scripts\test-unity.ps1`. Expected: animator component tests, stale-feedback test, and all updated integration tests pass.

- [ ] **Step 8: Commit**

```powershell
git add Assets/Scripts/Runtime/Presentation/DocketProgressView.cs Assets/Scripts/Runtime/Presentation/ShiftFeedbackAnimator.cs Assets/Scripts/Runtime/Presentation/GameApp.cs Assets/Tests/PlayMode/ShiftPresentationViewPlayModeTests.cs Assets/Tests/PlayMode/GameAppPlayModeTests.cs
git commit -m "feat: animate cozy curio filing feedback"
```

---

### Task 6: Replace the Text-Heavy Tutorial with T1 Guided Play

**Files:**
- Modify: `Assets/Scripts/Runtime/Localization/Localizer.cs`
- Modify: `Assets/Scripts/Runtime/Presentation/GameApp.cs:165-174,520-695,1413-1440`
- Modify: `Assets/Tests/PlayMode/GameAppPlayModeTests.cs:240-430`

**Interfaces:**
- Consumes: existing six-artifact tutorial queue, Task 2 stamp labels, Task 4 Hold reason, Task 5 transition callbacks.
- Produces: `tutorial_goal`, `tutorial_first_docket_complete`, `tutorial_finish`, `TraitsText(ArtifactTraits, ArtifactTraits emphasized)` and guided T1 stage presentation.

- [ ] **Step 1: Rewrite failing tutorial expectations**

Keep the fixed queue and progression assertions. Replace `1 / 7`-only expectations with exact learning goals:

```csharp
Assert.That(ObjectText("TutorialCoach"),
    Does.Contain("one REPAIR, one STORAGE, and one VAULT"));
```

After the first three successful sorts and their transitions, assert a visible `TutorialDocketCompleteCard` contains `used each desk once`. At the duplicate destination, assert the coach contains both `correct desk` and `already full`. On completion, assert the final body contains `Fill all three stamps` and `Hold`.

Add Korean equivalents in the same tests by rebuilding the guided shift after `SetLocale(app, "ko")`.

- [ ] **Step 2: Ask the human to run the red test**

Run `.\scripts\test-unity.ps1`. Expected: PlayMode fails because the current tutorial still leads with numbered instructions and has no first-docket completion card.

- [ ] **Step 3: Add concise bilingual tutorial copy**

Add:

```csharp
["tutorial_goal"] = "Each docket needs one REPAIR, one STORAGE, and one VAULT stamp.",
["tutorial_first_docket_complete"] = "One docket complete! You used each desk once.",
["tutorial_finish"] = "Fill all three stamps. If a curio's desk is already full, Hold it.",
```

```csharp
["tutorial_goal"] = "장부 하나에는 수리실·보관실·봉인고 도장이 하나씩 필요해요.",
["tutorial_first_docket_complete"] = "장부 하나 완성! 세 장소를 한 번씩 사용했습니다.",
["tutorial_finish"] = "세 도장을 채우고, 자리가 찬 물건은 보류하세요.",
```

- [ ] **Step 4: Make the tutorial teach through the B1 screen**

Keep `ShowTutorial()` as a one-sentence clock-in screen, not a multi-paragraph rules page. During the first docket:

- show `tutorial_goal` until the first action;
- emphasize the whole matching rule line and the matching trait token with the same Amber rich-text style;
- leave player input responsible for the destination choice;
- after the third stamp, show `TutorialDocketCompleteCard` for the docket-complete transition;
- at duplicate Vault, rely on the shared Hold-required state instead of a separate contradictory message;
- finish with `tutorial_finish`.

Tutorial wrong sorts keep hearts at 3, keep the artifact current, and re-emphasize the matching trait/rule.

- [ ] **Step 5: Ask the human to run the green test**

Run `.\scripts\test-unity.ps1`. Expected: all fixed-order, no-heart-loss, Hold-return, completion-save, and bilingual T1 assertions pass.

- [ ] **Step 6: Commit**

```powershell
git add Assets/Scripts/Runtime/Localization/Localizer.cs Assets/Scripts/Runtime/Presentation/GameApp.cs Assets/Tests/PlayMode/GameAppPlayModeTests.cs
git commit -m "feat: teach the first docket through play"
```

---

### Task 7: Give Docket Results a Short Emotional Payoff

**Files:**
- Create: `Assets/Scripts/Runtime/Presentation/ResultLedgerAnimator.cs`
- Modify: `Assets/Scripts/Runtime/Presentation/GameApp.cs:874-928`
- Modify: `Assets/Tests/PlayMode/ShiftPresentationViewPlayModeTests.cs`
- Modify: `Assets/Tests/PlayMode/GameAppPlayModeTests.cs:665-715,925-955`

**Interfaces:**
- Consumes: four `CompletedDocketPristine` values, Task 1 display font, existing final artifact resolution.
- Produces: `ResultLedgerAnimator.Configure(IReadOnlyList<CanvasGroup> rows)` and `Play()`.

- [ ] **Step 1: Write failing result-animation and typography tests**

Component test:

```csharp
animator.Configure(rows);
animator.Play();
Assert.That(rows[0].alpha, Is.Zero);
yield return new WaitForSecondsRealtime(0.8f);
Assert.That(rows.All(row => Mathf.Approximately(row.alpha, 1f)), Is.True);
```

Integration tests assert `ResultTitle` and `ResultResolution` use Gowun Batang, every `ResultDocketN` has a CanvasGroup, the four rows still report Pristine/Inked correctly, and `RewardedAdButton` remains absent.

- [ ] **Step 2: Ask the human to run the red test**

Run `.\scripts\test-unity.ps1`. Expected: PlayMode compilation/tests fail because `ResultLedgerAnimator` and row CanvasGroups do not exist.

- [ ] **Step 3: Implement the focused row animator**

Validate exactly four non-null CanvasGroups. `Play()` resets every alpha and scale, then reveals each row 0.10 seconds apart over 0.18 seconds using unscaled time. `OnDisable` stops the routine and restores all rows to alpha 1 and scale 1.

- [ ] **Step 4: Wire the result screen**

Capture each docket row TMP RectTransform, add a CanvasGroup, configure `ResultLedgerAnimator`, and call `Play()`. Use Display type for result title/resolution and Interface type for scores and ledger status. Preserve result durability, daily best, and hidden monetization behavior.

- [ ] **Step 5: Ask the human to run the green test**

Run `.\scripts\test-unity.ps1`. Expected: result animation, font-role, ledger content, save durability, and reward-boundary regressions pass.

- [ ] **Step 6: Commit**

```powershell
git add Assets/Scripts/Runtime/Presentation/ResultLedgerAnimator.cs Assets/Scripts/Runtime/Presentation/GameApp.cs Assets/Tests/PlayMode/ShiftPresentationViewPlayModeTests.cs Assets/Tests/PlayMode/GameAppPlayModeTests.cs
git commit -m "feat: animate the completed night ledger"
```

Include the new script `.meta` generated by the human import; do not stage unrelated generated files.

---

### Task 8: Regenerate, Review, and Validate the Approved Slice

**Files:**
- Generated by human: `Assets/Resources/Fonts/GowunBatang-Bold-Dynamic.asset`
- Generated by human: `Assets/Localization/UI Shared Data.asset`
- Generated by human: `Assets/Localization/UI_en.asset`
- Generated by human: `Assets/Localization/UI_ko.asset`
- Generated by human: `Assets/Resources/Content/Artifacts/*.asset`
- Modify after evidence: `Docs/UnityProjectContext.md`

**Interfaces:**
- Consumes: all prior tasks and the human-run Unity validation surface.
- Produces: reviewed generated assets, exact automated evidence, manual acceptance evidence, and final branch-ready status.

- [ ] **Step 1: Perform static pre-generation checks**

Run read-only checks:

```powershell
git status --short
git diff --check -- Assets/Scripts Assets/Tests Docs
rg -n "RewardedAdButton|tutorial_goal|hold_required|GowunBatang" Assets/Scripts Assets/Tests
```

Expected: no diff-check errors in authored files, no visible `RewardedAdButton` creation, and all new localization/font identifiers have test coverage.

- [ ] **Step 2: Ask the human to run the full pre-generation suite**

With Unity Editor closed:

```powershell
.\scripts\test-unity.ps1
```

Expected: all EditMode and PlayMode tests pass with failed 0 and skipped 0. Record the exact counts from both XML files.

- [ ] **Step 3: Ask the human to regenerate assets**

Open the correct worktree in Unity 6000.3.21f1 and run `Tools > Curio Clerk > Generate Project Assets`. Expected validation text:

```text
Curio Clerk validation passed: 24 artifacts, 10 rules, 2 rule packs, 3 docket templates, 5 difficulties, 6 cosmetics, 2 scenes.
```

- [ ] **Step 4: Review generated diffs before staging**

Accept only:

- Gowun TMP generated asset and its `.meta`;
- localization tables containing the new B1/T1 keys;
- 24 artifact assets containing the already-authored bilingual resolutions.

Reject or leave unstaged unrelated scenes, ProjectSettings, Noto font rewrites, advertising/plugin files, and artifact image `.png.meta` rewrites.

- [ ] **Step 5: Ask the human to rerun the full post-generation suite**

Close Unity Editor and run `.\scripts\test-unity.ps1` again. Expected: all tests pass, failed 0, skipped 0, and XML timestamps are newer than the generation run.

- [ ] **Step 6: Run the minimum manual acceptance gate**

In a 9:16 portrait Game view:

1. Complete the Korean tutorial once.
2. After its first docket, confirm the player can explain “one stamp per destination.”
3. Complete one Korean normal shift and verify the first Hold-required state is understood within five seconds.
4. Intentionally make one wrong sort and verify the artifact stays while the card shakes and the reason is readable.
5. Confirm Hold moves the artifact to the shelf, correct filing stamps the docket, and four result rows reveal in sequence.
6. Switch to English and inspect the same B1 shift screen for clipping and overlap.
7. Record approximate duration for the 12-artifact shift; target 60-90 seconds without a forced timer.

Criteria 2 and 3 are blocking. If either fails, do not call the slice ready and do not update the context document as passed.

- [ ] **Step 7: Update the project context only with supplied evidence**

In `Docs/UnityProjectContext.md`, replace stale test counts and presentation notes with the exact XML counts, BuildAll validator text, locales manually checked, Game view aspect, shift duration, and any explicit limitation. Do not claim Android device or APK validation.

- [ ] **Step 8: Commit generated output and evidence separately**

Stage exact approved paths, inspect `git diff --cached --name-status`, and commit:

```powershell
git commit -m "build: regenerate curio-first presentation"
```

- [ ] **Step 9: Final branch audit**

Run:

```powershell
git status --short
git log --oneline -12
git diff --check HEAD~1..HEAD
```

Report the branch as Ready only if authored tests, generated assets, and the minimum manual gate passed. Otherwise report Ready with limitations or Failed using the exact missing evidence.
