# Three-Seal Dockets Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace Curio Clerk's static 12-item sorting shift with four solvable Three-Seal Dockets that use Repair, Storage, and Vault exactly four times each and make one-slot Hold necessary.

**Architecture:** Keep rule resolution, docket state, scoring, Hold, and deterministic plan generation in the Unity-free `CurioClerk.Core` assembly. Runtime content supplies two short bilingual rule packs, three prevalidated destination templates, and artifact resolution copy; `GameApp` consumes a complete `ShiftPlan` and delegates three-seal rendering to a small presentation component. Existing save, art, feedback, daily seed, Casebook, and cosmetics systems remain in place.

**Tech Stack:** Unity `6000.3.21f1`, C#, NUnit/Unity Test Framework, uGUI, TextMesh Pro, Unity Localization, deterministic `System.Random`, Android portrait/ARM64/IL2CPP.

**Spec:** `Docs/superpowers/specs/2026-08-27-three-seal-dockets-design.md`

## Global Constraints

- Keep Unity `6000.3.21f1` and package ID `com.joyshu93.curioclerknightshift`.
- Keep Android portrait, API 29 minimum, API 36 target, ARM64, IL2CPP, and AAB release output.
- Keep exactly 12 artifacts and four three-seal dockets in a normal 60~90 second shift; do not add a forced timer.
- Every normal plan uses Repair, Storage, and Vault exactly four times and is solvable with one Hold slot.
- The game remains fully playable offline without an account, server, consent, or available ad.
- Add every player-facing string in English and Korean in the same change.
- Add no new art, audio file, currency, quest, achievement, IAP, Firebase feature, or ad placement.
- Keep `Assets/Scripts/Core` free of `UnityEngine` references.
- Do not hand-edit generated `Assets/Resources/Content` or `Assets/Scenes`; the human developer runs `ProjectBuilder.BuildAll` after source catalog/localization changes.
- Do not launch Unity Editor/Hub, call Unity batch mode, or use Unity MCP. The human developer runs `scripts/test-unity.ps1`, `ProjectBuilder.BuildAll`, and device checks and supplies output.
- Before adding AI-assisted resolution/UI copy, update `Docs/AIAssetProvenance.md` and `Docs/ThirdPartyNotices.md` in the same task.
- Preserve the user's existing dirty worktree. At execution time, use `superpowers:using-git-worktrees` to create an isolated `codex/three-seal-dockets` worktree from commit `86438bb`; never stash, reset, or commit the user's current advertising/build changes.
- Do not modify `Assets/Scripts/Editor/ProjectBuilder.cs`, `Assets/Scripts/Runtime/CurioClerk.Runtime.asmdef`, Google Mobile Ads files, Android resolver files, or build scripts for this feature.

## File Structure

| Area | Files | Responsibility |
| --- | --- | --- |
| Detailed rule result | `Assets/Scripts/Core/Rules/RuleResolution.cs`, `RuleEngine.cs` | Return both the first matching rule ID and its destination while keeping `Resolve()` compatible. |
| Docket state | `Assets/Scripts/Core/Shifts/DocketState.cs` | Own three destination stamps and pristine state for one docket. |
| Shift outcomes | `Assets/Scripts/Core/Shifts/SortDisposition.cs`, `SortOutcome.cs`, `ShiftSession.cs` | Distinguish Correct/Wrong/Blocked, own four-docket progress, scoring, Hold, and queue previews. |
| Plan generation | `ShiftPlan.cs`, `ShiftRulePack.cs`, `ShiftSequenceTemplate.cs`, `DocketSequenceAnalyzer.cs`, `ShiftPlanGenerator.cs` | Generate deterministic 4/4/4 plans from validated rule packs and solvable destination templates. |
| Runtime content | `ArtifactContent.cs`, `ArtifactDefinition.cs`, `ContentCatalog.cs` | Supply rule packs, sequence templates, and bilingual artifact resolutions. |
| Validation | `ContentValidator.cs`, EditMode contract tests | Reject incomplete bilingual copy, undersupplied rule packs, invalid patterns, and unsolvable plans. |
| Presentation | `DocketProgressView.cs`, `GameApp.cs`, `ShiftFeedbackAnimator.cs`, `Localizer.cs` | Render docket stamps, disabled desks, Hold suggestion, reasoned feedback, tutorial, results, and Casebook resolution. |
| Provenance | `Docs/AIAssetProvenance.md`, `Docs/ThirdPartyNotices.md` | Record AI-assisted bilingual gameplay copy and confirm no third-party prose. |
| Generated output | `Assets/Resources/Content/Artifacts/*.asset`, `Assets/Localization/*.asset` | Human-generated output after source changes; review separately before commit. |

---

## Phase A — Deterministic Core

### Task 1: Add detailed rule resolution and isolated docket state

**Files:**
- Create: `Assets/Scripts/Core/Rules/RuleResolution.cs`
- Create: `Assets/Scripts/Core/Rules/RuleResolution.cs.meta` with GUID `b1d4eae2f1b64bcdbbf4fd1286e5c201`
- Create: `Assets/Scripts/Core/Shifts/DocketState.cs`
- Create: `Assets/Scripts/Core/Shifts/DocketState.cs.meta` with GUID `a8f50b0ab3b84ca5ae2bc61cf8c43a22`
- Modify: `Assets/Scripts/Core/Rules/RuleEngine.cs:7-45`
- Modify: `Assets/Tests/EditMode/CurioClerk.EditModeTests.asmdef`
- Modify: `Assets/Tests/EditMode/RuleEngineContractTests.cs`
- Create: `Assets/Tests/EditMode/DocketStateContractTests.cs`
- Create: `Assets/Tests/EditMode/DocketStateContractTests.cs.meta` with GUID `c8b8ed3a2a7c433fb513ba831f21e6af`

**Interfaces:**
- Consumes: existing `Artifact`, `SortingRule`, and `Destination`.
- Produces: `RuleResolution(string ruleId, Destination destination)`, `RuleEngine.ResolveDetailed(...)`, and `DocketState.IsStamped/TryStamp/MarkMistake` for Task 2.

- [ ] **Step 1: Add a direct Core test reference and write failing contracts**

Add `"CurioClerk.Core"` to the EditMode test asmdef references, then add this rule test:

```csharp
[Test]
public void ResolveDetailed_ReturnsTheFirstMatchingRuleIdAndDestination()
{
    var artifact = new Artifact("tooth", ArtifactTraits.Cursed | ArtifactTraits.Fragile);
    var rules = new[]
    {
        new SortingRule("cursed-vault", ArtifactTraits.Cursed, ArtifactTraits.None, Destination.Vault, false),
        new SortingRule("fragile-repair", ArtifactTraits.Fragile, ArtifactTraits.None, Destination.Repair, false),
        new SortingRule("fallback-storage", ArtifactTraits.None, ArtifactTraits.None, Destination.Storage, true)
    };

    var result = new RuleEngine().ResolveDetailed(artifact, rules);

    Assert.That(result.RuleId, Is.EqualTo("cursed-vault"));
    Assert.That(result.Destination, Is.EqualTo(Destination.Vault));
    Assert.That(new RuleEngine().Resolve(artifact, rules), Is.EqualTo(Destination.Vault));
}
```

Create `DocketStateContractTests.cs`:

```csharp
using CurioClerk.Core.Rules;
using CurioClerk.Core.Shifts;
using NUnit.Framework;

namespace CurioClerk.Tests.EditMode
{
    public sealed class DocketStateContractTests
    {
        [Test]
        public void ThreeUniqueStampsCompleteAPristineDocket()
        {
            var docket = new DocketState();

            Assert.That(docket.TryStamp(Destination.Vault), Is.True);
            Assert.That(docket.TryStamp(Destination.Vault), Is.False);
            Assert.That(docket.TryStamp(Destination.Repair), Is.True);
            Assert.That(docket.TryStamp(Destination.Storage), Is.True);

            Assert.That(docket.StampCount, Is.EqualTo(3));
            Assert.That(docket.IsComplete, Is.True);
            Assert.That(docket.IsPristine, Is.True);
        }

        [Test]
        public void MarkMistakeOnlyClearsPristineState()
        {
            var docket = new DocketState();
            docket.TryStamp(Destination.Storage);

            docket.MarkMistake();

            Assert.That(docket.IsPristine, Is.False);
            Assert.That(docket.IsStamped(Destination.Storage), Is.True);
            Assert.That(docket.StampCount, Is.EqualTo(1));
        }
    }
}
```

- [ ] **Step 2: Ask the developer to run EditMode tests and verify red**

Human-run command:

```powershell
.\scripts\test-unity.ps1
```

Expected: compilation/test failure because `RuleResolution`, `ResolveDetailed`, and `DocketState` do not exist.

- [ ] **Step 3: Implement the minimal detailed result and docket state**

Create `RuleResolution.cs`:

```csharp
using System;

namespace CurioClerk.Core.Rules
{
    public sealed class RuleResolution
    {
        public RuleResolution(string ruleId, Destination destination)
        {
            if (string.IsNullOrWhiteSpace(ruleId))
            {
                throw new ArgumentException("Rule id is required.", nameof(ruleId));
            }

            RuleId = ruleId;
            Destination = destination;
        }

        public string RuleId { get; }
        public Destination Destination { get; }
    }
}
```

Change `RuleEngine.Resolve` to delegate to this new method:

```csharp
public Destination Resolve(Artifact artifact, IReadOnlyList<SortingRule> rules)
    => ResolveDetailed(artifact, rules).Destination;

public RuleResolution ResolveDetailed(Artifact artifact, IReadOnlyList<SortingRule> rules)
{
    if (artifact == null)
    {
        throw new ArgumentNullException(nameof(artifact));
    }

    Validate(rules);
    for (var index = 0; index < rules.Count; index++)
    {
        if (rules[index].Matches(artifact))
        {
            return new RuleResolution(rules[index].Id, rules[index].Destination);
        }
    }

    throw new InvalidOperationException("The validated fallback rule did not match.");
}
```

Create `DocketState.cs`:

```csharp
using CurioClerk.Core.Rules;

namespace CurioClerk.Core.Shifts
{
    public sealed class DocketState
    {
        private int _stampMask;

        public bool IsPristine { get; private set; } = true;
        public int StampCount { get; private set; }
        public bool IsComplete => StampCount == 3;

        public bool IsStamped(Destination destination)
            => (_stampMask & (1 << (int)destination)) != 0;

        public bool TryStamp(Destination destination)
        {
            var bit = 1 << (int)destination;
            if ((_stampMask & bit) != 0)
            {
                return false;
            }

            _stampMask |= bit;
            StampCount++;
            return true;
        }

        public void MarkMistake() => IsPristine = false;
    }
}
```

Add minimal `.meta` files using the GUIDs listed under **Files**.

- [ ] **Step 4: Ask the developer to run EditMode tests and verify green**

Human-run command: `.\scripts\test-unity.ps1`

Expected: all existing RuleEngine tests and the new detailed/docket contracts pass; PlayMode remains unchanged.

- [ ] **Step 5: Commit the isolated Core primitives**

```powershell
git add Assets/Scripts/Core/Rules/RuleEngine.cs Assets/Scripts/Core/Rules/RuleResolution.cs Assets/Scripts/Core/Rules/RuleResolution.cs.meta Assets/Scripts/Core/Shifts/DocketState.cs Assets/Scripts/Core/Shifts/DocketState.cs.meta Assets/Tests/EditMode/CurioClerk.EditModeTests.asmdef Assets/Tests/EditMode/RuleEngineContractTests.cs Assets/Tests/EditMode/DocketStateContractTests.cs Assets/Tests/EditMode/DocketStateContractTests.cs.meta
git commit -m "feat: add three-seal docket primitives"
```

### Task 2: Make ShiftSession own four-docket sorting, scoring, mistakes, and previews

**Files:**
- Create: `Assets/Scripts/Core/Shifts/SortDisposition.cs`
- Create: `Assets/Scripts/Core/Shifts/SortDisposition.cs.meta` with GUID `d0c90c788b694c84bb7113c5852ac9c6`
- Modify: `Assets/Scripts/Core/Shifts/SortOutcome.cs`
- Modify: `Assets/Scripts/Core/Shifts/ShiftSession.cs`
- Modify temporarily: `Assets/Scripts/Runtime/Presentation/GameApp.cs:195,492`
- Replace contracts in: `Assets/Tests/EditMode/ShiftSessionContractTests.cs`

**Interfaces:**
- Consumes: `RuleResolution` and `DocketState` from Task 1.
- Produces: `ShiftSession.CurrentDocket`, `CompletedDockets`, `RequiredDockets`, `PristineDocketStreak`, `CompletedDocketPristine`, `CurrentResolution`, `CanSort`, `ShouldSuggestHold`, `PeekNextArtifact`, and detailed `SortOutcome` for presentation.

- [ ] **Step 1: Replace obsolete combo contracts with failing docket-session tests**

Use direct Core types and a 12-item pattern helper. Construct new docket sessions with the normal public constructor. The first two focused tests must be:

```csharp
[Test]
public void DuplicateCorrectDestination_IsBlockedWithoutAdvancingOrChargingAHeart()
{
    var session = CreateReferenceSession();
    session.Sort(Destination.Vault);
    var before = session.CurrentArtifact.Id;

    var outcome = session.Sort(Destination.Vault);

    Assert.That(outcome.Disposition, Is.EqualTo(SortDisposition.Blocked));
    Assert.That(session.CurrentArtifact.Id, Is.EqualTo(before));
    Assert.That(session.Hearts, Is.EqualTo(3));
    Assert.That(session.CurrentDocket.StampCount, Is.EqualTo(1));
    Assert.That(session.ShouldSuggestHold, Is.True);
}

[Test]
public void WrongSort_LosesAHeartButKeepsTheArtifactAndDocket()
{
    var session = CreateReferenceSession();
    var before = session.CurrentArtifact.Id;

    var outcome = session.Sort(Destination.Storage);

    Assert.That(outcome.Disposition, Is.EqualTo(SortDisposition.Wrong));
    Assert.That(session.CurrentArtifact.Id, Is.EqualTo(before));
    Assert.That(session.Hearts, Is.EqualTo(2));
    Assert.That(session.Mistakes, Is.EqualTo(1));
    Assert.That(session.CurrentDocket.IsPristine, Is.False);
    Assert.That(session.CurrentDocket.StampCount, Is.Zero);
}
```

Add a full-flow test that executes the spec's reference action order and asserts:

```csharp
Assert.That(session.State, Is.EqualTo(ShiftState.Completed));
Assert.That(session.CompletedDockets, Is.EqualTo(4));
Assert.That(session.CorrectSorts, Is.EqualTo(12));
Assert.That(session.Score, Is.EqualTo(2800));
Assert.That(session.Coins, Is.EqualTo(100));
Assert.That(session.CompletedDocketPristine, Is.EqualTo(new[] { true, true, true, true }));
```

Keep and adapt the existing Hold test to assert `PeekNextArtifact(0)` and `PeekNextArtifact(1)` instead of relying on a presentation-side index. Keep revive/double tests, but complete a valid three-item docket before calling `TryDoubleCoins`.

- [ ] **Step 2: Ask the developer to run tests and verify red**

Human-run command: `.\scripts\test-unity.ps1`

Expected: EditMode failures for missing `SortDisposition` and new ShiftSession properties; existing PlayMode tests may also fail after the test source compiles because runtime behavior is still old.

- [ ] **Step 3: Define the detailed sort outcome**

Create the enum:

```csharp
namespace CurioClerk.Core.Shifts
{
    public enum SortDisposition
    {
        Correct = 0,
        Wrong = 1,
        Blocked = 2
    }
}
```

Replace `SortOutcome` with this compatible shape:

```csharp
public sealed class SortOutcome
{
    public SortOutcome(
        SortDisposition disposition,
        Destination selectedDestination,
        Destination expectedDestination,
        string matchedRuleId,
        bool didCompleteDocket,
        bool didCompleteShift,
        int scoreDelta,
        int coinDelta)
    {
        Disposition = disposition;
        SelectedDestination = selectedDestination;
        ExpectedDestination = expectedDestination;
        MatchedRuleId = matchedRuleId;
        DidCompleteDocket = didCompleteDocket;
        DidCompleteShift = didCompleteShift;
        ScoreDelta = scoreDelta;
        CoinDelta = coinDelta;
    }

    public SortDisposition Disposition { get; }
    public bool WasCorrect => Disposition == SortDisposition.Correct;
    public Destination SelectedDestination { get; }
    public Destination ExpectedDestination { get; }
    public string MatchedRuleId { get; }
    public bool DidCompleteDocket { get; }
    public bool DidCompleteShift { get; }
    public int ScoreDelta { get; }
    public int CoinDelta { get; }
}
```

- [ ] **Step 4: Rewrite ShiftSession around dockets without changing ad service boundaries**

The normal constructor must reject queues whose count is not a positive multiple of three, set `RequiredDockets = queue.Count / 3`, and create a fresh `DocketState`. Add a temporary compatibility factory so the current random shift and four-item tutorial stay green until Tasks 6 and 7:

```csharp
private readonly bool _legacyMode;
private int _legacyCombo;

public ShiftSession(IReadOnlyList<Artifact> queue, IReadOnlyList<SortingRule> rules)
    : this(queue, rules, false)
{
}

public static ShiftSession CreateLegacySession(
    IReadOnlyList<Artifact> queue,
    IReadOnlyList<SortingRule> rules)
    => new ShiftSession(queue, rules, true);

private ShiftSession(
    IReadOnlyList<Artifact> queue,
    IReadOnlyList<SortingRule> rules,
    bool legacyMode)
{
    if (queue == null || queue.Count == 0 || (!legacyMode && queue.Count % 3 != 0))
    {
        throw new ArgumentException(
            "A docket shift requires a positive artifact count divisible by three.", nameof(queue));
    }

    _queue = queue;
    _rules = rules ?? throw new ArgumentNullException(nameof(rules));
    _legacyMode = legacyMode;
    RequiredDockets = legacyMode ? 0 : queue.Count / 3;
    CurrentDocket = new DocketState();
    CurrentArtifact = queue[0] ??
        throw new ArgumentException("Artifact queues cannot contain null entries.", nameof(queue));
    _nextIndex = 1;
    Hearts = 3;
    State = ShiftState.Active;
}
```

Route the public method explicitly:

```csharp
public SortOutcome Sort(Destination destination)
    => _legacyMode ? SortLegacy(destination) : SortDocket(destination);
```

Move the new docket branch below into `SortDocket`. Preserve the pre-feature behavior in this exact temporary bridge:

```csharp
private SortOutcome SortLegacy(Destination destination)
{
    EnsureActive();
    var resolution = _ruleEngine.ResolveDetailed(CurrentArtifact, _rules);
    var wasCorrect = destination == resolution.Destination;
    var scoreDelta = 0;
    var coinDelta = 0;

    if (wasCorrect)
    {
        _legacyCombo++;
        CorrectSorts++;
        scoreDelta = 100 + (_legacyCombo - 1) * 20;
        coinDelta = 5 + Math.Min(_legacyCombo - 1, 5);
        Score += scoreDelta;
        Coins += coinDelta;
    }
    else
    {
        Hearts--;
        Mistakes++;
        _legacyCombo = 0;
    }

    Advance();
    _canHold = true;
    if (Hearts <= 0)
    {
        State = ShiftState.Failed;
    }
    else if (CurrentArtifact == null)
    {
        State = ShiftState.Completed;
    }

    return Outcome(
        wasCorrect ? SortDisposition.Correct : SortDisposition.Wrong,
        destination,
        resolution,
        false,
        State == ShiftState.Completed,
        scoreDelta,
        coinDelta);
}
```

No new code outside GameApp may call the factory. In this task, change both existing `new ShiftSession(...)` calls in GameApp to `ShiftSession.CreateLegacySession(...)`, and replace the removed Combo HUD expression with `$"♥ {_session.Hearts}     {_localizer.Get(\"coins\")} {_session.Coins}"`. Task 6 switches normal shifts to the docket constructor, and Task 7 switches the tutorial then deletes the legacy factory, flag, `_legacyCombo`, and `SortLegacy` method.

Add these public docket members:

```csharp
public DocketState CurrentDocket { get; private set; }
public int CompletedDockets { get; private set; }
public int RequiredDockets { get; }
public int PristineDocketStreak { get; private set; }
public IReadOnlyList<bool> CompletedDocketPristine => _completedDocketPristine;
public RuleResolution CurrentResolution => CurrentArtifact == null
    ? null
    : _ruleEngine.ResolveDetailed(CurrentArtifact, _rules);
public bool ShouldSuggestHold => CurrentResolution != null &&
                                 CurrentDocket.IsStamped(CurrentResolution.Destination);

public bool CanSort(Destination destination)
    => State == ShiftState.Active && !CurrentDocket.IsStamped(destination);

public Artifact PeekNextArtifact(int offset)
{
    if (offset < 0)
    {
        throw new ArgumentOutOfRangeException(nameof(offset));
    }

    var index = _nextIndex + offset;
    if (index < _queue.Count)
    {
        return _queue[index];
    }

    return index == _queue.Count && HeldArtifact != null
        ? HeldArtifact
        : null;
}
```

Declare the history as `private readonly List<bool> _completedDocketPristine = new List<bool>();`; the concrete list is required for the final all-pristine `TrueForAll` check while callers receive it through `IReadOnlyList<bool>`.

`Sort` must evaluate branches in this order:

```csharp
var resolution = _ruleEngine.ResolveDetailed(CurrentArtifact, _rules);
if (CurrentDocket.IsStamped(destination))
{
    return Outcome(SortDisposition.Blocked, destination, resolution, false, false, 0, 0);
}

if (destination != resolution.Destination)
{
    Hearts--;
    Mistakes++;
    CurrentDocket.MarkMistake();
    PristineDocketStreak = 0;
    if (Hearts <= 0)
    {
        State = ShiftState.Failed;
    }

    return Outcome(SortDisposition.Wrong, destination, resolution, false, false, 0, 0);
}

var scoreDelta = 100;
var coinDelta = 5;
CorrectSorts++;
CurrentDocket.TryStamp(destination);
var completedDocket = CurrentDocket.IsComplete;
if (completedDocket)
{
    scoreDelta += 300;
    coinDelta += 5;
    if (CurrentDocket.IsPristine)
    {
        scoreDelta += 100;
        PristineDocketStreak++;
    }
    else
    {
        PristineDocketStreak = 0;
    }

    _completedDocketPristine.Add(CurrentDocket.IsPristine);
    CompletedDockets++;
}

Advance();
_canHold = true;
var completedShift = CompletedDockets == RequiredDockets;
if (completedShift)
{
    if (_completedDocketPristine.TrueForAll(value => value))
    {
        coinDelta += 20;
    }

    State = ShiftState.Completed;
}
else if (completedDocket)
{
    CurrentDocket = new DocketState();
}

Score += scoreDelta;
Coins += coinDelta;
return Outcome(SortDisposition.Correct, destination, resolution, completedDocket, completedShift,
    scoreDelta, coinDelta);
```

Add the single outcome-construction helper used by both paths:

```csharp
private static SortOutcome Outcome(
    SortDisposition disposition,
    Destination selectedDestination,
    RuleResolution resolution,
    bool didCompleteDocket,
    bool didCompleteShift,
    int scoreDelta,
    int coinDelta)
{
    return new SortOutcome(
        disposition,
        selectedDestination,
        resolution.Destination,
        resolution.RuleId,
        didCompleteDocket,
        didCompleteShift,
        scoreDelta,
        coinDelta);
}
```

Delete the `Combo` property and old increasing score/coin formula. Wrong and Blocked outcomes do not call `Advance()` and do not reset `_canHold`. Keep `TryRevive`, `TryDoubleCoins`, `CreateResult`, and the current Held-at-queue-end behavior.

- [ ] **Step 5: Ask the developer to run EditMode tests and diagnose all failures**

Human-run command: `.\scripts\test-unity.ps1`

Expected: new ShiftSession tests and the complete existing PlayMode suite pass because GameApp is temporarily routed through `CreateLegacySession`. A PlayMode regression here means the compatibility bridge is incomplete and must be fixed before commit.

- [ ] **Step 6: Commit the completed Core session behavior**

```powershell
git add Assets/Scripts/Core/Shifts/SortDisposition.cs Assets/Scripts/Core/Shifts/SortDisposition.cs.meta Assets/Scripts/Core/Shifts/SortOutcome.cs Assets/Scripts/Core/Shifts/ShiftSession.cs Assets/Scripts/Runtime/Presentation/GameApp.cs Assets/Tests/EditMode/ShiftSessionContractTests.cs
git commit -m "feat: run shifts as three-seal dockets"
```

### Task 3: Add solvability analysis and deterministic ShiftPlan generation

**Files:**
- Create: `Assets/Scripts/Core/Shifts/ShiftPlan.cs` + `.meta` GUID `199f68a2ce16452c8ccb1ca2a23aac21`
- Create: `Assets/Scripts/Core/Shifts/ShiftRulePack.cs` + `.meta` GUID `2a0a2c5295a64d1e9a2ca6ed54f88873`
- Create: `Assets/Scripts/Core/Shifts/ShiftSequenceTemplate.cs` + `.meta` GUID `3b1b3d63a6b74e40bc3542e54f622945`
- Create: `Assets/Scripts/Core/Shifts/DocketSequenceAnalyzer.cs` + `.meta` GUID `4c2c4e74b7c84f51ad4653f660733a57`
- Create: `Assets/Scripts/Core/Shifts/ShiftPlanGenerator.cs` + `.meta` GUID `5d3d5f85c8d94a62be57640771844b69`
- Retain until Task 6: `Assets/Scripts/Core/Shifts/ShiftGenerator.cs` and `.meta`
- Replace: `Assets/Tests/EditMode/ShiftGenerationContractTests.cs`

**Interfaces:**
- Consumes: Core artifacts, rules, and destinations.
- Produces: `DocketSequenceAnalyzer.MinimumHolds`, `ShiftPlanGenerator.Generate`, and immutable plan/content types for Task 4 and GameApp.

- [ ] **Step 1: Write failing analyzer and generator contracts**

Add these destination-pattern assertions:

```csharp
[TestCase("RRSVRSVRSVSV", 1)]
[TestCase("VVRSRSVSSRVR", 2)]
[TestCase("RRSVSSVRRVSV", 3)]
public void MinimumHolds_ReturnsTheValidatedTemplateCost(string pattern, int expected)
{
    var destinations = pattern.Select(ParseDestination)
        .ToArray();
    Assert.That(new DocketSequenceAnalyzer().MinimumHolds(destinations), Is.EqualTo(expected));
}
```

Add a generator test that builds 24 unique artifacts covering all three buckets, two rule packs, and the three templates, then calls `Generate` twice with seed 7727 and band 2. Assert identical artifact IDs and plan IDs, 12 distinct artifacts, 4/4/4 resolved destinations, and `MinimumHolds >= plan.SequenceTemplate.MinimumRequiredHolds`.

Add rejection tests for a rule pack with fewer than four Storage candidates, a template not containing four of each destination, and a template whose declared minimum exceeds the analyzer result.

- [ ] **Step 2: Ask the developer to run tests and verify red**

Human-run command: `.\scripts\test-unity.ps1`

Expected: compilation failure for the five missing plan-generation types.

- [ ] **Step 3: Implement immutable plan input types**

Use these exact public constructors and properties:

```csharp
public sealed class ShiftRulePack
{
    public ShiftRulePack(string id, int minimumBand, int maximumBand, IReadOnlyList<SortingRule> rules);
    public string Id { get; }
    public int MinimumBand { get; }
    public int MaximumBand { get; }
    public IReadOnlyList<SortingRule> Rules { get; }
    public bool SupportsBand(int band);
}

public sealed class ShiftSequenceTemplate
{
    public ShiftSequenceTemplate(string id, int minimumBand, int maximumBand,
        int minimumRequiredHolds, IReadOnlyList<Destination> destinations);
    public string Id { get; }
    public int MinimumBand { get; }
    public int MaximumBand { get; }
    public int MinimumRequiredHolds { get; }
    public IReadOnlyList<Destination> Destinations { get; }
    public bool SupportsBand(int band);
}

public sealed class ShiftPlan
{
    public ShiftPlan(string rulePackId, string sequenceTemplateId,
        IReadOnlyList<Artifact> queue, IReadOnlyList<SortingRule> rules);
    public string RulePackId { get; }
    public string SequenceTemplateId { get; }
    public IReadOnlyList<Artifact> Queue { get; }
    public IReadOnlyList<SortingRule> Rules { get; }
}
```

Constructors must reject blank IDs, inverted band ranges, null lists, templates other than 12 entries, and `minimumRequiredHolds < 0`. Copy incoming lists into arrays so plans cannot change after construction.

- [ ] **Step 4: Implement the one-slot sequence analyzer**

`DocketSequenceAnalyzer.MinimumHolds(IReadOnlyList<Destination>)` performs breadth-first search over this state tuple:

```text
next queue index
current destination
nullable held destination
current docket stamp bitmask
completed docket count
whether Hold is currently allowed
hold action count
```

Each successful sort is a zero-choice transition and every completed solution performs exactly `destinations.Count` sorts, so breadth-first action order minimizes Hold actions. A Hold transition either consumes the next queued destination into current while storing current, or swaps current and held; it then disables Hold until a successful sort. Return `-1` when no solution reaches `destinations.Count / 3` completed dockets with no current item. Reject null, empty, or non-multiple-of-three input.

Represent visited state with an immutable key containing every field except hold count; never prune two states whose `canHold` values differ.

- [ ] **Step 5: Implement deterministic plan generation**

Expose this method:

```csharp
public ShiftPlan Generate(
    int seed,
    int band,
    IReadOnlyList<Artifact> source,
    IReadOnlyList<ShiftRulePack> rulePacks,
    IReadOnlyList<ShiftSequenceTemplate> templates)
```

Implementation order:

1. Filter packs/templates with `SupportsBand(band)` and reject an empty result.
2. Choose one of each with `new Random(seed ^ 0x51F15EED)` and `new Random(seed ^ 0x2D0C7E7)` so pack and template choice remain independent.
3. Resolve every source artifact with the selected pack and build three buckets.
4. Require at least four distinct IDs in every bucket.
5. Deterministically Fisher-Yates shuffle each bucket with seeds `seed ^ 0x13579`, `seed ^ 0x24680`, and `seed ^ 0x369CF`; take four from each.
6. Walk the template destinations and dequeue from the matching selected bucket.
7. Reject duplicate artifact IDs, non-4/4/4 templates, analyzer result `-1`, or analyzer result below `MinimumRequiredHolds`.
8. Return a `ShiftPlan` using the selected pack's rules.

The generator must not retry another pack/template after validation failure; invalid authored content is a build/test error.

- [ ] **Step 6: Ask the developer to run EditMode tests and verify green**

Human-run command: `.\scripts\test-unity.ps1`

Expected: all plan-generation tests pass with minimum Hold values 1, 2, and 3. Record any mismatch as an analyzer/template defect; do not change expected values to fit the implementation.

- [ ] **Step 7: Confirm the legacy generator remains isolated and commit**

Run `rg -n "ShiftGenerator" Assets/Scripts Assets/Tests`. Expected: the legacy class and GameApp are the only references because this task replaces its old contract tests. Do not delete the class while GameApp still consumes it; Task 6 removes both references atomically.

```powershell
git add Assets/Scripts/Core/Shifts Assets/Tests/EditMode/ShiftGenerationContractTests.cs
git commit -m "feat: generate solvable balanced shift plans"
```

## Phase B — Authored Content and Validation

### Task 4: Publish two short rule packs and three validated shift templates

**Files:**
- Modify: `Assets/Scripts/Runtime/Content/ContentCatalog.cs`
- Modify: `Assets/Scripts/Editor/ContentValidator.cs:41-87`
- Modify: `Assets/Tests/EditMode/ContentCatalogContractTests.cs`

**Interfaces:**
- Consumes: `ShiftRulePack`, `ShiftSequenceTemplate`, and `ShiftPlanGenerator` from Task 3.
- Produces: `ContentCatalog.CreateRulePacks()` and `CreateShiftTemplates()` used by GameApp.

- [ ] **Step 1: Replace randomized-band tests with failing authored-pack tests**

Keep the existing 24-artifact and cosmetic tests. Replace `RuleAndDifficultyCatalog_HasTenTemplatesAndFiveValidBands` with assertions that:

- `ContentVersion == 2`.
- `CreateRuleTemplates()` still contains 10 unique definitions so existing generated rule assets remain stable.
- `CreateRulePacks()` returns exactly `pack-cursed-fragile` and `pack-temporal-wet`.
- Every pack contains exactly two conditional rules plus a final Storage fallback.
- Resolving the 24 catalog artifacts gives at least four candidates per destination for every pack.
- `CreateShiftTemplates()` returns the exact three templates below and the analyzer returns their declared minimum Hold counts.

- [ ] **Step 2: Ask the developer to run EditMode tests and verify red**

Human-run command: `.\scripts\test-unity.ps1`

Expected: missing methods and unchanged `ContentVersion` failures.

- [ ] **Step 3: Add exact rule packs and sequence templates**

Set `ContentVersion = 2` and add:

```csharp
public static IReadOnlyList<ShiftRulePack> CreateRulePacks()
{
    return new[]
    {
        new ShiftRulePack("pack-cursed-fragile", 1, 3, new[]
        {
            R("cursed-vault", ArtifactTraits.Cursed, ArtifactTraits.None, Destination.Vault),
            R("fragile-repair", ArtifactTraits.Fragile, ArtifactTraits.None, Destination.Repair),
            new SortingRule("fallback-storage", ArtifactTraits.None, ArtifactTraits.None,
                Destination.Storage, true)
        }),
        new ShiftRulePack("pack-temporal-wet", 2, 3, new[]
        {
            R("temporal-vault", ArtifactTraits.Temporal, ArtifactTraits.None, Destination.Vault),
            R("wet-repair", ArtifactTraits.Wet, ArtifactTraits.None, Destination.Repair),
            new SortingRule("fallback-storage", ArtifactTraits.None, ArtifactTraits.None,
                Destination.Storage, true)
        })
    };
}

public static IReadOnlyList<ShiftSequenceTemplate> CreateShiftTemplates()
{
    return new[]
    {
        T("docket-band-1", 1, 1, 1,
            Destination.Repair, Destination.Repair, Destination.Storage, Destination.Vault,
            Destination.Repair, Destination.Storage, Destination.Vault, Destination.Repair,
            Destination.Storage, Destination.Vault, Destination.Storage, Destination.Vault),
        T("docket-band-2", 2, 2, 2,
            Destination.Vault, Destination.Vault, Destination.Repair, Destination.Storage,
            Destination.Repair, Destination.Storage, Destination.Vault, Destination.Storage,
            Destination.Storage, Destination.Repair, Destination.Vault, Destination.Repair),
        T("docket-band-3", 3, 3, 3,
            Destination.Repair, Destination.Repair, Destination.Storage, Destination.Vault,
            Destination.Storage, Destination.Storage, Destination.Vault, Destination.Repair,
            Destination.Repair, Destination.Vault, Destination.Storage, Destination.Vault)
    };
}

private static ShiftSequenceTemplate T(string id, int minimumBand, int maximumBand,
    int minimumHolds, params Destination[] destinations)
    => new ShiftSequenceTemplate(id, minimumBand, maximumBand, minimumHolds, destinations);
```

Keep `CreateRulesForBand` temporarily because the legacy GameApp path still calls it. Task 6 deletes the method after switching GameApp to `CreateRulePacks()` and `CreateShiftTemplates()` in the same green commit.

- [ ] **Step 4: Extend ContentValidator without changing ProjectBuilder**

For every rule pack, resolve all artifacts and add an error unless each destination count is at least four. For every sequence template, assert 12 entries, exactly four occurrences of each destination, `MinimumHolds >= MinimumRequiredHolds`, and not `-1`. Update the success log to include `2 rule packs, 3 docket templates` while retaining existing generated-asset counts.

- [ ] **Step 5: Ask the developer to run EditMode tests and verify green**

Human-run command: `.\scripts\test-unity.ps1`

Expected: catalog and validator contracts pass; GameApp remains green through the retained legacy generation APIs until Task 6 replaces them.

- [ ] **Step 6: Commit authored gameplay structures**

```powershell
git add Assets/Scripts/Runtime/Content/ContentCatalog.cs Assets/Scripts/Editor/ContentValidator.cs Assets/Tests/EditMode/ContentCatalogContractTests.cs
git commit -m "feat: author three-seal rule packs"
```

### Task 5: Add bilingual artifact resolutions with provenance before content changes

**Files:**
- Modify first: `Docs/AIAssetProvenance.md`
- Modify first: `Docs/ThirdPartyNotices.md`
- Modify: `Assets/Scripts/Runtime/Content/ArtifactContent.cs`
- Modify: `Assets/Scripts/Runtime/Content/ArtifactDefinition.cs`
- Modify: `Assets/Scripts/Runtime/Content/ContentCatalog.cs`
- Modify: `Assets/Tests/EditMode/ContentCatalogContractTests.cs`
- Modify: `Assets/Scripts/Editor/ContentValidator.cs`

**Interfaces:**
- Consumes: existing artifact IDs, names, descriptions, traits, and generated-asset `Configure` flow.
- Produces: `ResolutionEnglish`, `ResolutionKorean`, and generated serialized resolution fields for GameApp and Casebook.

- [ ] **Step 1: Record provenance and third-party status before adding generated copy**

Add provenance entry `TEXT-GAMEPLAY-003 — Three-Seal Docket rules and curio resolutions` dated `2026-08-27 KST`. Record `Localizer.cs`, `ContentCatalog.cs`, generated localization tables, and generated Artifact assets; tool `OpenAI Codex`; reference input limited to existing project-authored names/descriptions and the approved Three-Seal design; no third-party prose; prototype copy requiring developer bilingual review.

Add a ThirdPartyNotices row named `Three-Seal bilingual gameplay copy` pointing to `TEXT-GAMEPLAY-003`, stating no third-party prose or attribution was identified and human bilingual review remains required. Update the register review date to `2026-08-27 (KST)`.

- [ ] **Step 2: Write the failing bilingual-resolution contract**

Extend the artifact catalog test:

```csharp
Assert.That(String(artifact, "ResolutionEnglish"), Is.Not.Empty, String(artifact, "Id"));
Assert.That(String(artifact, "ResolutionKorean"), Is.Not.Empty, String(artifact, "Id"));
```

Add a reflection contract that `ArtifactDefinition` exposes both properties and copies them in `Configure`.

- [ ] **Step 3: Ask the developer to run EditMode tests and verify red**

Human-run command: `.\scripts\test-unity.ps1`

Expected: missing resolution property failures.

- [ ] **Step 4: Extend artifact content and generated definitions**

Append `resolutionEnglish` and `resolutionKorean` to `ArtifactContent`'s constructor and expose read-only properties. Add matching `[TextArea, SerializeField]` fields and public getters to `ArtifactDefinition`; assign them in `Configure(ArtifactContent)`.

Update helper `A(...)` in `ContentCatalog` to accept both resolution strings. Use this exact copy table:

| ID | English resolution | Korean resolution |
| --- | --- | --- |
| clockwork-moth | Its wings settle into the rhythm of the desk lamp. | 날개가 책상 램프의 박자에 맞춰 잔잔해진다. |
| rain-jar | The rain softens to a quiet window drizzle. | 병 속의 비가 창가의 잔잔한 이슬비로 누그러진다. |
| whispering-key | Its whisper fades to the sound of one safe lock. | 속삭임이 안전한 자물쇠 하나의 소리로 잦아든다. |
| sleeping-teacup | It sighs once and dreams of warm tea. | 한 번 한숨을 쉬고 따뜻한 차를 꿈꾼다. |
| borrowed-shadow | It leans toward home, patient until sunrise. | 해 뜰 때까지 얌전히 기다리며 집 쪽으로 몸을 기울인다. |
| moon-umbrella | Its moonlit patches glow without trembling. | 달빛 기운 자리가 떨림 없이 은은히 빛난다. |
| silent-bell | For tonight, it is silent for the right reason. | 오늘 밤만큼은 올바른 이유로 고요하다. |
| mossy-watch | The moss pauses and lets the present catch up. | 이끼가 멈춰 서서 현재가 따라오게 해 준다. |
| paper-fish | It folds its fins and rests like a sealed letter. | 지느러미를 접고 봉해진 편지처럼 쉰다. |
| backward-candle | The wax holds still at this evening. | 촛농이 바로 오늘 저녁에 멈춰 선다. |
| porcelain-tooth | Its smile becomes small enough to trust. | 미소가 믿어도 될 만큼 작아진다. |
| thimble-storm | The tiny thunder lowers its voice. | 작은 천둥이 목소리를 낮춘다. |
| humming-scarf | The forbidden chorus becomes a sleepy hum. | 금지된 후렴이 졸린 콧노래로 누그러진다. |
| sundial-egg | A steady noon-bright heartbeat answers from within. | 안쪽에서 정오처럼 밝고 고른 박동이 답한다. |
| mirror-seed | Its reflection curls safely around the seed. | 반사된 빛이 씨앗 둘레를 안전하게 감싼다. |
| rusty-comet | It stops apologizing and shines at its own pace. | 사과를 멈추고 자기 속도로 빛난다. |
| ink-snowglobe | The black forecast settles into gentle flakes. | 검은 예보가 부드러운 눈송이로 가라앉는다. |
| patient-compass | Its needle rests, certain that home can wait. | 집은 기다려 준다는 듯 바늘이 편안히 멈춘다. |
| yesterday-ticket | The punched date finally agrees to stay in the past. | 찍힌 날짜가 마침내 과거에 머물기로 한다. |
| tea-crown | A warm ring dries where breakfast once ruled. | 아침 식사가 다스리던 자리에 따뜻한 고리가 마른다. |
| lantern-snail | Its little porch light glows for the night clerk. | 작은 현관등이 야간 직원을 위해 빛난다. |
| tide-locket | The tide inside quiets beneath a patient moon. | 안쪽의 밀물이 느긋한 달 아래 잔잔해진다. |
| murmur-box | One kind word leaves it peacefully quiet. | 다정한 말 한마디에 상자가 편안히 조용해진다. |
| unmelting-ice | It cools the desk without borrowing tomorrow. | 내일의 추위를 빌리지 않고 책상을 서늘하게 한다. |

- [ ] **Step 5: Extend validation and ask the developer to run tests**

`ContentValidator` must include both resolution fields in its bilingual-text check. Human-run command: `.\scripts\test-unity.ps1`

Expected: EditMode content tests pass; no generated Artifact asset is manually edited yet.

- [ ] **Step 6: Commit source copy and provenance together**

```powershell
git add Docs/AIAssetProvenance.md Docs/ThirdPartyNotices.md Assets/Scripts/Runtime/Content/ArtifactContent.cs Assets/Scripts/Runtime/Content/ArtifactDefinition.cs Assets/Scripts/Runtime/Content/ContentCatalog.cs Assets/Tests/EditMode/ContentCatalogContractTests.cs Assets/Scripts/Editor/ContentValidator.cs
git commit -m "feat: add bilingual curio resolutions"
```

## Phase C — Playable Presentation

### Task 6: Integrate ShiftPlan and render live docket progress in normal shifts

**Files:**
- Create: `Assets/Scripts/Runtime/Presentation/DocketProgressView.cs`
- Create: `Assets/Scripts/Runtime/Presentation/DocketProgressView.cs.meta` with GUID `6e4e6096d9ea4b73af68751882955c7b`
- Delete: `Assets/Scripts/Core/Shifts/ShiftGenerator.cs` and `.meta`
- Modify: `Assets/Scripts/Runtime/Content/ContentCatalog.cs` (delete `CreateRulesForBand`)
- Modify: `Assets/Scripts/Runtime/Presentation/GameApp.cs:53-100,172-263,401-471,629-707`
- Modify: `Assets/Scripts/Runtime/Presentation/ShiftFeedbackAnimator.cs`
- Modify: `Assets/Scripts/Runtime/Localization/Localizer.cs`
- Modify: `Assets/Tests/PlayMode/CurioClerk.PlayModeTests.asmdef`
- Modify: `Assets/Tests/PlayMode/GameAppPlayModeTests.cs`

**Interfaces:**
- Consumes: `ShiftPlanGenerator`, `ShiftSession` docket/query APIs, catalog rule packs/templates.
- Produces: named docket UI objects, disabled stamped destinations, Hold suggestion, robust previews, and solvable normal/daily shifts.

- [ ] **Step 1: Add the direct Core PlayMode reference and failing UI contracts**

Add `"CurioClerk.Core"` to the PlayMode asmdef. Extend `App_StartsAtMenuAndBuildsAPlayableShiftLayout` to assert `DocketProgress`, `DocketCounter`, `DocketStampRepair`, `DocketStampStorage`, and `DocketStampVault` exist.

Add this behavior test using Band 1's `R,R,S,V,...` template:

```csharp
[UnityTest]
public IEnumerator DuplicateDesk_DisablesThatDeskAndSuggestsHold()
{
    var app = CreateApp(new DeferredAdService(), new ControllablePrivacyService());
    yield return null;
    app.StartNewShift(4242);

    var first = ExpectedDestination(app);
    ChooseDestination(app, Convert.ToInt32(first));
    yield return null;

    Assert.That(first.ToString(), Is.EqualTo("Repair"));
    Assert.That(GameObject.Find("RepairButton").GetComponent<Button>().interactable, Is.False);
    Assert.That(HasEnabledOutline("HoldButton"), Is.True);
    Assert.That(ObjectText("DocketCounter"), Does.Contain("1 / 4"));
}
```

Replace `CompleteShift` and `CompleteActiveShift` with a safety-bounded solver:

```csharp
private static void CompleteActiveShift(GameApp app)
{
    for (var safety = 0; safety < 64 && SessionState(app) == "Active"; safety++)
    {
        var expected = ExpectedDestination(app);
        var button = GameObject.Find(DestinationButtonName(expected)).GetComponent<Button>();
        if (button.interactable)
        {
            typeof(GameApp).GetMethod("ChooseDestination").Invoke(app, new[] { expected });
        }
        else
        {
            app.HoldCurrent();
        }
    }

    Assert.That(SessionState(app), Is.EqualTo("Completed"),
        "The generated plan must complete within the safety bound using one Hold slot.");
}
```

Update the existing wrong-sort UI test to assert the current artifact ID remains unchanged. Update completion discovery expectation from 11 to 12 because the wrong item must later be corrected.

- [ ] **Step 2: Ask the developer to run PlayMode tests and verify red**

Human-run command: `.\scripts\test-unity.ps1`

Expected: missing docket UI and stale GameApp generator/preview behavior failures.

- [ ] **Step 3: Add a focused DocketProgressView**

Expose this API:

```csharp
public sealed class DocketProgressView : MonoBehaviour
{
    public void Configure(TMP_Text counter, IReadOnlyList<Image> stamps,
        Color openColor, Color stampedColor);

    public void Refresh(DocketState docket, int completedDockets, int requiredDockets);
}
```

`Refresh` writes localized-neutral numeric counter text as `$"{completedDockets + 1} / {requiredDockets}"` while active, `$"{requiredDockets} / {requiredDockets}"` when complete, and sets each stamp color based on `docket.IsStamped((Destination)index)`. GameApp owns localized labels and creates the stamp texts/icons; the view owns only state rendering.

- [ ] **Step 4: Replace independent GameApp generation with ShiftPlan**

Replace `_shiftGenerator` with `ShiftPlanGenerator`, add `_activePlan`, and keep `_plannedQueue`/`_activeRules` assigned from the plan for current tests and rule rendering. Clamp normal and daily gameplay bands to 1~3:

```csharp
var supportedBand = Mathf.Clamp(band, 1, 3);
var artifacts = _artifactContent.Select(item => item.ToArtifact()).ToArray();
_activePlan = _shiftPlanGenerator.Generate(seed, supportedBand, artifacts,
    ContentCatalog.CreateRulePacks(), ContentCatalog.CreateShiftTemplates());
_plannedQueue = _activePlan.Queue;
_activeRules = _activePlan.Rules;
_session = new ShiftSession(_plannedQueue, _activeRules);
```

Remove `_sortedCount`. `RefreshShiftView` must obtain previews with `_session.PeekNextArtifact(index)` so wrong sorts, empty Hold consumption, swaps, and queue-end Held return remain correct. After `rg -n "ShiftGenerator|CreateRulesForBand" Assets/Scripts Assets/Tests` confirms the only remaining executable references are the legacy class/method and the GameApp call sites being replaced, delete `ShiftGenerator.cs` plus its `.meta` and delete `ContentCatalog.CreateRulesForBand`. Keep the new analyzer/generator contracts in `ShiftGenerationContractTests.cs`.

- [ ] **Step 5: Add the minimal bilingual live-shift strings**

Add these keys to both Localizer dictionaries in the same change that first renders them:

```csharp
// English
["shift_hud"] = "HEARTS {0}   DOCKET {1}/{2}   CLEAN {3}   COINS {4}",
["docket"] = "DOCKET",
["blocked"] = "That desk is already stamped. Use HOLD.",

// Korean
["shift_hud"] = "하트 {0}   장부 {1}/{2}   말끔 {3}   코인 {4}",
["docket"] = "장부",
["blocked"] = "이미 찍힌 도장입니다. 보류를 사용하세요.",
```

- [ ] **Step 6: Build the docket strip and enforce desk availability**

In `BuildShiftScreen`, add a panel named `DocketProgress` at anchors `(0.05, 0.605)` to `(0.95, 0.67)`. Create `DocketCounter` on the left and three named stamp chips on the right. Reuse Repair/Storage/Vault colors and destination icons; do not add textures. Move previews to `0.535~0.60` and reduce the artifact card top to `0.525` to avoid overlap.

In `RefreshShiftView`:

```csharp
_docketProgress.Refresh(_session.CurrentDocket, _session.CompletedDockets, _session.RequiredDockets);
for (var index = 0; index < _destinationButtons.Length; index++)
{
    _destinationButtons[index].interactable = _session.CanSort((Destination)index);
}
_holdHighlight.enabled = _session.ShouldSuggestHold;
_hudText.text = _localizer.Get("shift_hud", _session.Hearts,
    _session.CompletedDockets + 1, _session.RequiredDockets,
    _session.PristineDocketStreak, _session.Coins);
```

Tutorial-specific interactable/highlight overrides remain in `RefreshTutorialGuidance` and are updated in Task 7.

- [ ] **Step 7: Route SortOutcome without advancing on Wrong or Blocked**

`ChooseDestination` must add the artifact to `_seenThisShift` only for `Correct`, show blocked feedback without a Wrong cue, keep the same card for Wrong/Blocked, and call `ShowResults` only when `outcome.DidCompleteShift` or session state is Failed. A completed docket uses a stronger scale pulse from a new `ShiftFeedbackAnimator.PlayDocketComplete()` method; final completion uses the existing `ShiftComplete` cue and result transition.

- [ ] **Step 8: Ask the developer to run both suites and verify normal shifts**

Human-run command: `.\scripts\test-unity.ps1`

Expected: both suites pass. New docket UI and normal/daily completion tests use the plan solver; the unchanged four-item tutorial remains on the temporary legacy bridge until Task 7. No test may sort blindly through a disabled duplicate desk.

- [ ] **Step 9: Commit normal-shift presentation**

```powershell
git rm Assets/Scripts/Core/Shifts/ShiftGenerator.cs Assets/Scripts/Core/Shifts/ShiftGenerator.cs.meta
git add Assets/Scripts/Runtime/Content/ContentCatalog.cs Assets/Scripts/Runtime/Presentation/DocketProgressView.cs Assets/Scripts/Runtime/Presentation/DocketProgressView.cs.meta Assets/Scripts/Runtime/Presentation/GameApp.cs Assets/Scripts/Runtime/Presentation/ShiftFeedbackAnimator.cs Assets/Scripts/Runtime/Localization/Localizer.cs Assets/Tests/PlayMode/CurioClerk.PlayModeTests.asmdef Assets/Tests/PlayMode/GameAppPlayModeTests.cs
git commit -m "feat: present three-seal shift progress"
```

### Task 7: Replace the tutorial with the approved six-item two-docket lesson

**Files:**
- Modify: `Assets/Scripts/Core/Shifts/ShiftSession.cs` (remove temporary legacy bridge)
- Modify: `Assets/Scripts/Runtime/Presentation/GameApp.cs:27-37,478-627`
- Modify: `Assets/Scripts/Runtime/Localization/Localizer.cs`
- Modify: `Assets/Tests/PlayMode/GameAppPlayModeTests.cs:207-309`

**Interfaces:**
- Consumes: Rule Pack A, `ShiftSession` Blocked/Hold behavior, docket UI.
- Produces: a deterministic six-item tutorial with one forced Hold and Held queue-end return.

- [ ] **Step 1: Rewrite tutorial tests for the exact six-item flow**

Assert the queue IDs are exactly:

```text
whispering-key
borrowed-shadow
sleeping-teacup
clockwork-moth
rain-jar
moon-umbrella
```

Test this sequence:

1. Sort Whispering Key to Vault.
2. Assert Vault is disabled and Hold is the only highlighted action for Borrowed Shadow.
3. Hold Borrowed Shadow.
4. Sort Sleeping Teacup to Repair and Clockwork Moth to Storage; assert docket 1 closes.
5. Sort Jar of Tuesday Rain to Storage and Moon-Mended Umbrella to Repair.
6. Assert Borrowed Shadow returns automatically; sort it to Vault and complete training.

Keep the tutorial wrong-sort contract: no heart loss, no artifact advance, and a localized rule hint.

- [ ] **Step 2: Ask the developer to run PlayMode tests and verify red**

Human-run command: `.\scripts\test-unity.ps1`

Expected: old four-item tutorial assertions fail.

- [ ] **Step 3: Replace TutorialStage and tutorial queue**

Use these stages:

```csharp
None,
FirstVault,
HoldDuplicateVault,
FirstRepair,
FirstStorage,
SecondStorage,
SecondRepair,
FinalHeldVault,
Complete
```

Use Rule Pack A's exact rules and the six IDs above. The tutorial's `ShiftSession` naturally requires two dockets because `queue.Count / 3 == 2`. Keep tutorial errors outside `ShiftSession.Sort` so training hearts remain unchanged.

Construct the tutorial session with the normal constructor:

```csharp
var tutorialRules = ContentCatalog.CreateRulePacks()
    .Single(pack => pack.Id == "pack-cursed-fragile")
    .Rules;
_session = new ShiftSession(_plannedQueue, tutorialRules);
```

After the tutorial no longer calls `CreateLegacySession`, run `rg -n "CreateLegacySession|SortLegacy|_legacyMode|_legacyCombo" Assets/Scripts Assets/Tests`. Delete the factory, private flag/counter, legacy constructor branch, and `SortLegacy`; simplify the public constructor to the docket validation/body from Task 2. The search must return no matches before committing.

- [ ] **Step 4: Replace tutorial localization in both languages**

Use concise numbered guidance:

```csharp
// English
["tutorial_body"] = "Each docket needs one REPAIR, one STORAGE, and one VAULT. The first matching rule wins.",
["tutorial_step_first_vault"] = "1 / 7 · CURSED goes to VAULT. File the key.",
["tutorial_step_hold"] = "2 / 7 · VAULT is already stamped. HOLD the shadow.",
["tutorial_step_first_repair"] = "3 / 7 · Fill the missing REPAIR stamp.",
["tutorial_step_first_storage"] = "4 / 7 · STORAGE closes the first docket.",
["tutorial_step_second_storage"] = "5 / 7 · Begin the next docket with STORAGE.",
["tutorial_step_second_repair"] = "6 / 7 · Add REPAIR. The held shadow will return.",
["tutorial_step_final_vault"] = "7 / 7 · File the returned shadow to VAULT.",
["tutorial_blocked"] = "That desk is already stamped. Use HOLD to reach a missing desk.",

// Korean
["tutorial_body"] = "장부마다 수리실, 보관실, 봉인고 도장이 하나씩 필요합니다. 먼저 맞는 규칙이 우선입니다.",
["tutorial_step_first_vault"] = "1 / 7 · 저주받음은 봉인고입니다. 열쇠를 분류하세요.",
["tutorial_step_hold"] = "2 / 7 · 봉인고 도장은 이미 찍혔습니다. 그림자를 보류하세요.",
["tutorial_step_first_repair"] = "3 / 7 · 비어 있는 수리실 도장을 채우세요.",
["tutorial_step_first_storage"] = "4 / 7 · 보관실 도장으로 첫 장부를 닫으세요.",
["tutorial_step_second_storage"] = "5 / 7 · 다음 장부를 보관실부터 시작하세요.",
["tutorial_step_second_repair"] = "6 / 7 · 수리실을 채우면 보류한 그림자가 돌아옵니다.",
["tutorial_step_final_vault"] = "7 / 7 · 돌아온 그림자를 봉인고에 분류하세요.",
["tutorial_blocked"] = "이미 찍힌 도장입니다. 보류해서 비어 있는 목적지를 찾으세요.",
```

Remove obsolete four-step tutorial keys only after `rg` confirms no caller remains.

- [ ] **Step 5: Ask the developer to run both suites and verify green**

Human-run command: `.\scripts\test-unity.ps1`

Expected: tutorial completion persists `tutorialCompleted` but does not add coins, completed shifts, or discoveries; all seven guidance states work in English and Korean.

- [ ] **Step 6: Commit tutorial replacement**

```powershell
git add Assets/Scripts/Core/Shifts/ShiftSession.cs Assets/Scripts/Runtime/Presentation/GameApp.cs Assets/Scripts/Runtime/Localization/Localizer.cs Assets/Tests/PlayMode/GameAppPlayModeTests.cs
git commit -m "feat: teach three-seal hold strategy"
```

### Task 8: Add reasoned feedback, resolution copy, results, Casebook copy, and paused-ad presentation

**Files:**
- Modify: `Assets/Scripts/Runtime/Presentation/GameApp.cs:292-317,629-765,1200-1262`
- Modify: `Assets/Scripts/Runtime/Localization/Localizer.cs`
- Modify: `Assets/Tests/PlayMode/GameAppPlayModeTests.cs`

**Interfaces:**
- Consumes: `SortOutcome.MatchedRuleId`, artifact resolution fields, completed docket history.
- Produces: rule-reason feedback, destination-colored response, four-docket result summary, Casebook resolution, and no visible rewarded offer during core-fun validation.

- [ ] **Step 1: Write failing feedback/result/Casebook contracts**

Add or update PlayMode tests to require:

- A correct Cursed item displays `CURSED took priority -> VAULT` in English and the Korean equivalent after locale switch.
- A fallback item displays `No special rule -> STORAGE`.
- `SortFeedback` contains the current artifact's localized resolution text.
- `Blocked` displays a Hold instruction, plays no Wrong cue, and changes no heart.
- `ResultDocket0` through `ResultDocket3` exist and show `PRISTINE` or `INKED` / `말끔함` or `번짐`.
- `ResultResolution` contains the resolution of the final correctly sorted artifact.
- A known Casebook card includes `CasebookResolution_<artifact-id>`; a locked card exposes neither description nor resolution.
- `RewardedAdButton` is absent on completed and failed result screens while private reward-state tests may continue invoking `RequestReward` directly.

- [ ] **Step 2: Ask the developer to run PlayMode tests and verify red**

Human-run command: `.\scripts\test-unity.ps1`

Expected: generic banner, missing resolution/result objects, and visible rewarded-button failures.

- [ ] **Step 3: Add exact bilingual UI strings**

Add these keys:

```csharp
// English
["docket_pristine"] = "PRISTINE",
["docket_inked"] = "INKED",
["rule_reason"] = "{0} -> {1}",
["rule_priority_reason"] = "{0} took priority -> {1}",
["fallback_reason"] = "No special rule -> {0}",
["result_dockets"] = "NIGHT LEDGER",
["result_waiting"] = "The remaining curios wait beneath a blanket for the next night clerk.",
["resolution_label"] = "AFTER FILING",

// Korean
["docket_pristine"] = "말끔함",
["docket_inked"] = "번짐",
["rule_reason"] = "{0} -> {1}",
["rule_priority_reason"] = "{0} 우선 -> {1}",
["fallback_reason"] = "특수 규칙 없음 -> {0}",
["result_dockets"] = "야간 장부",
["result_waiting"] = "남은 유물은 담요 아래에서 다음 야간반을 기다립니다.",
["resolution_label"] = "처리 후",
```

Use ASCII `->` rather than arrow glyphs because the release font tests already avoid unsupported symbols.

- [ ] **Step 4: Render reason and resolution from Core outcome data**

Add `Resolution(ArtifactContent)` beside `Name`/`Description`. Before sorting, capture the current `ArtifactContent`; after `Sort`, format the reason from `MatchedRuleId` and destination. A conditional rule whose item also matches a lower conditional rule uses `rule_priority_reason`; a final fallback uses `fallback_reason`; otherwise use `rule_reason`. Do not re-run rule selection to decide the matched rule ID.

Find the already-selected rule by exact `MatchedRuleId`, derive its label from `RequiredAll` or `RequiredAny` with the existing `TraitsText`, and determine priority only by inspecting later non-fallback matches:

```csharp
private string RuleReason(Artifact artifact, SortOutcome outcome)
{
    var matchedIndex = -1;
    for (var index = 0; index < _activeRules.Count; index++)
    {
        if (_activeRules[index].Id == outcome.MatchedRuleId)
        {
            matchedIndex = index;
            break;
        }
    }

    if (matchedIndex < 0)
    {
        throw new InvalidOperationException("Sort outcome references an unknown active rule.");
    }

    var matched = _activeRules[matchedIndex];
    var destination = DestinationName(outcome.ExpectedDestination);
    if (matched.IsFallback)
    {
        return _localizer.Get("fallback_reason", destination);
    }

    var matchedTraits = matched.RequiredAll != ArtifactTraits.None
        ? matched.RequiredAll
        : matched.RequiredAny;
    var hasLowerMatch = false;
    for (var index = matchedIndex + 1; index < _activeRules.Count; index++)
    {
        if (!_activeRules[index].IsFallback && _activeRules[index].Matches(artifact))
        {
            hasLowerMatch = true;
            break;
        }
    }

    return _localizer.Get(
        hasLowerMatch ? "rule_priority_reason" : "rule_reason",
        TraitsText(matchedTraits),
        destination);
}
```

For Correct, color the feedback panel by selected destination, append the localized artifact resolution on a second line, and play Correct unless `DidCompleteShift`. For Wrong, show expected destination and matched rule reason, keep the artifact on desk, and play Wrong. For Blocked, use Wine, show `blocked`, pulse Hold, and play no cue.

- [ ] **Step 5: Rebuild results and Casebook without changing save schema**

Store the last correctly sorted artifact ID in GameApp. `ShowResults` creates four compact rows from `CompletedDocketPristine`; missing rows on failure show the localized inked/waiting state. Create `ResultResolution` from the last correct artifact and show score/coins underneath the ledger.

Remove visible rewarded-button creation from `ShowResults`. Keep `RequestReward`, `TryRevive`, `TryDoubleCoins`, and service tests intact but reachable only through tests until monetization resumes.

For known Casebook artifacts, display description and resolution in separate named texts, reducing description height as needed. Locked cards continue to show only `casebook_locked` and no resolution object text.

- [ ] **Step 6: Repair affected feedback and reward PlayMode tests**

Update terminal feedback tests to use `CompleteActiveShift` until one correct sort remains, because blind 11-sort loops are invalid with closed desks. Update failed-revive tests to invoke private `RequestReward(false)` instead of clicking a hidden button. Keep duplicate callback, dismissed, failed, unavailable, and one-reward-only assertions unchanged.

- [ ] **Step 7: Ask the developer to run both suites and verify green**

Human-run command: `.\scripts\test-unity.ps1`

Expected: all EditMode and PlayMode tests pass; exact totals may exceed the previous 79/31 baseline. Record the new passed counts and zero failures from developer-supplied output.

- [ ] **Step 8: Commit the complete player-facing loop**

```powershell
git add Assets/Scripts/Runtime/Presentation/GameApp.cs Assets/Scripts/Runtime/Localization/Localizer.cs Assets/Tests/PlayMode/GameAppPlayModeTests.cs
git commit -m "feat: reward three-seal shift decisions"
```

## Phase D — Generated Assets and Human Validation

### Task 9: Regenerate content/localization and perform the human validation gate

**Files:**
- Human-generated and reviewed: `Assets/Resources/Content/Artifacts/*.asset`
- Human-generated and reviewed: `Assets/Localization/UI Shared Data.asset`
- Human-generated and reviewed: `Assets/Localization/UI_en.asset`
- Human-generated and reviewed: `Assets/Localization/UI_ko.asset`
- Possibly human-generated `.meta` files only when Unity reports them as new
- Update after evidence: `Docs/UnityProjectContext.md`

**Interfaces:**
- Consumes: all source and test changes from Tasks 1~8.
- Produces: generated bilingual assets, human test evidence, and a handoff-ready gameplay branch.

- [ ] **Step 1: Run static repository checks before Unity generation**

Agent-run read-only checks:

```powershell
git diff --check
rg -n "CreateRulesForBand|ShiftGenerator|COMBO" Assets/Scripts Assets/Tests
rg -n "ResolutionEnglish|ResolutionKorean" Assets/Scripts/Runtime/Content Assets/Tests/EditMode
git status --short
```

Expected: no whitespace errors; no active `CreateRulesForBand`, `ShiftGenerator`, or player HUD `COMBO` references; resolution fields appear in catalog, definition, validator, and tests. Any remaining legacy identifier must be explained and removed if executable.

- [ ] **Step 2: Ask the developer to run the complete Unity test script**

Human-run command:

```powershell
.\scripts\test-unity.ps1
```

Expected: zero EditMode and PlayMode failures. Diagnose supplied output before continuing.

- [ ] **Step 3: Ask the developer to run ProjectBuilder.BuildAll**

Human action in Unity `6000.3.21f1`:

```text
Tools > Curio Clerk > Generate Project Assets
```

Expected: 24 Artifact assets receive both resolution fields, localization tables receive every new EN/KO key, content validation reports 2 rule packs and 3 docket templates, and no scene is hand-edited.

- [ ] **Step 4: Review generated diffs before staging**

Agent-run checks after the developer reports BuildAll completion:

```powershell
git status --short
git diff -- Assets/Resources/Content/Artifacts Assets/Localization
```

Expected: generated diffs contain resolution serialization and new localization entries only. Reject changes to fonts, Android settings, ads, scenes, art import settings, or unrelated content. Because execution occurs in an isolated worktree, these diffs must not include the user's pre-existing dirty-main changes.

- [ ] **Step 5: Ask the developer to rerun tests after generation**

Human-run command: `.\scripts\test-unity.ps1`

Expected: zero failures with generated assets present.

- [ ] **Step 6: Perform the bilingual manual play checklist**

The human developer checks both English and Korean:

1. Complete the six-item tutorial, including forced Hold and queue-end Held return.
2. Complete three normal shifts at bands 1, 2, and 3.
3. Confirm every shift contains 12 unique artifacts and ends with 4 Repair, 4 Storage, 4 Vault.
4. Confirm at least one Hold is necessary and no valid plan deadlocks.
5. Make one wrong sort; confirm the item remains, the rule reason appears, and only one heart is lost.
6. Attempt a stamped destination by drag; confirm no heart loss and Hold feedback.
7. Confirm all four docket completion effects, final resolution, results ledger, and Casebook resolution.
8. Confirm no rewarded-ad button appears and offline/menu/settings/Casebook/daily flows still work.
9. Confirm no text clips or overlaps on the portrait emulator/device.

- [ ] **Step 7: Update project context with verified totals and behavior**

Update `Docs/UnityProjectContext.md` only with evidence actually supplied by the developer: new EditMode/PlayMode pass totals, Three-Seal runtime flow, 2 rule packs, 3 templates, 24 resolutions, and the fact that agents still do not operate Unity.

- [ ] **Step 8: Commit generated output and verified context separately**

```powershell
git add Assets/Resources/Content/Artifacts Assets/Localization Docs/UnityProjectContext.md
git commit -m "build: regenerate three-seal content"
```

Before committing, `git diff --cached --name-only` must list only the expected generated assets and project context. Do not include ProjectSettings, ads, fonts, art `.meta`, Android resolver output, or build scripts.

- [ ] **Step 9: Final branch verification**

Agent-run:

```powershell
git diff --check HEAD~1 HEAD
git log --oneline --decorate -10
git status --short --branch
```

Human evidence required before claiming the feature passes:

- final `scripts/test-unity.ps1` output with zero failures;
- `ProjectBuilder.BuildAll` success message;
- English and Korean tutorial plus one 12-item shift each;
- manual confirmation that every destination is used four times and Hold is necessary.

Do not run `scripts/build-android.ps1` for this gameplay-plan completion. Run Android QA/release build only after the fun prototype passes playtest and the developer requests a device/release handoff.

---

## Expected Commit Sequence

1. `feat: add three-seal docket primitives`
2. `feat: run shifts as three-seal dockets`
3. `feat: generate solvable balanced shift plans`
4. `feat: author three-seal rule packs`
5. `feat: add bilingual curio resolutions`
6. `feat: present three-seal shift progress`
7. `feat: teach three-seal hold strategy`
8. `feat: reward three-seal shift decisions`
9. `build: regenerate three-seal content`

Every commit stages exact paths only. Before each commit, inspect `git diff --cached --name-status`; never absorb unrelated user work.
