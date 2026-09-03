# Multi-Incident Board and Remembering Rain Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 첫 사건을 해결한 플레이어에게 재사용 가능한 사건 보드와 실제 두 번째 사건 `기억하는 비`의 12개 물건 첫 교대를 제공하고, 공개된 교대를 마쳐도 사건을 거짓으로 해결하지 않으며 나중에 추가된 단계부터 안전하게 재개한다.

**Architecture:** 순수 C#의 `IncidentProgressResolver`가 순서가 있는 사건 정의와 기존 저장 기록으로 `Locked`, `Available`, `AwaitingContent`, `Resolved`를 계산한다. Runtime Content는 두 사건과 이중언어 대사·규칙·큐를 제공하고, Presentation은 `IncidentBoardState`를 재사용 가능한 uGUI/TMP 컴포넌트에 바인딩한다. Unity 전용 색·연출 값은 `ProjectBuilder.BuildAll`이 생성하는 ScriptableObject profile에 두며, 진행 판정은 애니메이션과 분리한다.

**Tech Stack:** Unity 6000.3.21f1, C# 9, deterministic Core asmdef without UnityEngine, uGUI, TextMesh Pro, ScriptableObject, CanvasGroup/RectTransform coroutine animation, Unity Test Framework EditMode/PlayMode, JsonUtility save, generated Resources content.

**Spec:** `Docs/superpowers/specs/2026-09-03-multi-incident-remembering-rain-design.md`

## Global Constraints

- Unity version is pinned to `6000.3.21f1`.
- Android is portrait, API 29 minimum, API 36 target, ARM64, IL2CPP, AAB.
- The game remains fully playable offline without an account, telemetry consent, or an available ad.
- 한 교대는 물건 12개이며 강제 시간 제한이 없고 수리실·보관실·봉인고를 정확히 네 번씩 사용한다.
- 에너지, 확률형 보상, 서버, 계정, 강제 광고와 새 외부 패키지를 추가하지 않는다.
- 모든 플레이어용 문구는 영어와 한국어를 같은 변경에서 제공한다.
- `Assets/Scripts/Core`에는 `UnityEngine` 참조를 추가하지 않는다.
- 생성된 `Assets/Resources/Content`와 `Assets/Scenes`를 손으로 수정하지 않는다. source catalog와 `ProjectBuilder.BuildAll`만 수정한다.
- 에이전트는 Unity Editor/Hub, Unity batch/CLI와 Unity MCP를 실행하지 않는다. Unity 실행은 인간 개발자가 수행한다.
- 현재 수정된 물건 `.png.meta`, TMP Settings, ProjectSettings와 미추적 Google Mobile Ads 파일은 사용자/Unity 소유다. 되돌리거나 이 계획의 커밋에 포함하지 않는다.
- 새 아트와 음원은 만들지 않는다. 달 우산 sprite, Rain cue, 기존 초상·배경·절차형 피드백을 재사용한다.
- `Docs/AIAssetProvenance.md`와 `Docs/ThirdPartyNotices.md`의 `DESIGN-NARRATIVE-002`, `TEXT-NARRATIVE-005` 기록을 유지한다.
- 테스트를 먼저 작성한다. 사용자 테스트 부담을 줄이기 위해 전체 Unity suite는 Task 1의 red gate와 Task 6의 green gate, 총 두 번만 요청한다.

---

## Planned File Map

| Responsibility | Create | Modify |
| --- | --- | --- |
| Incident lifecycle model | `Assets/Scripts/Core/Incidents/IncidentProgressModel.cs`; `IncidentProgressResolver.cs` | `IncidentRunner.cs`; `ProgressionService.cs` |
| Authored second incident | `Assets/Scripts/Runtime/Content/Incidents/SecondIncidentCatalog.cs` | `IncidentDefinition.cs`; `FirstIncidentCatalog.cs`; `ContentCatalog.cs`; `Localizer.cs` |
| Unity presentation profile | `Assets/Scripts/Runtime/Content/Incidents/IncidentPresentationStyleContent.cs`; `Assets/Scripts/Runtime/Content/IncidentPresentationProfile.cs` | `ProjectBuilder.cs`; `ContentValidator.cs`; `VisualAssetLibrary.cs` |
| Incident board presentation | `Assets/Scripts/Runtime/Presentation/IncidentBoardState.cs`; `IncidentCardView.cs`; `IncidentBoardView.cs`; `IncidentBoardTransitionView.cs` | `GameApp.cs`; `IncidentReactionView.cs` |
| EditMode tests | `Assets/Tests/EditMode/IncidentProgressResolverTests.cs` | `IncidentProgressionContractTests.cs`; `ProgressionContractTests.cs`; `IncidentContentContractTests.cs`; `ContentCatalogContractTests.cs`; `EditorAutomationContractTests.cs` |
| PlayMode tests | none | `GameAppPlayModeTests.cs`; `IncidentPresentationViewPlayModeTests.cs` |
| Generated after human BuildAll | `Assets/Resources/Content/IncidentPresentation/unmelting-ice.asset`; `remembering-rain.asset` and Unity `.meta` files | none by hand |
| Documentation | this plan | `Docs/AIAssetProvenance.md`; `Docs/ThirdPartyNotices.md`; implementation status in the spec after validation |

## Fixed Interfaces

Core progression contracts:

```csharp
namespace CurioClerk.Core.Incidents
{
    public enum IncidentLifecycle
    {
        Locked = 0,
        Available = 1,
        AwaitingContent = 2,
        Resolved = 3
    }

    public sealed class IncidentProgressDefinition
    {
        public IncidentProgressDefinition(
            string id,
            IReadOnlyList<string> stageIds,
            bool completesWhenAllStagesCompleted);

        public string Id { get; }
        public IReadOnlyList<string> StageIds { get; }
        public bool CompletesWhenAllStagesCompleted { get; }
    }

    public sealed class IncidentProgressEntry
    {
        public IncidentProgressDefinition Definition { get; }
        public IncidentLifecycle Lifecycle { get; }
        public int NextStageIndex { get; }
    }

    public sealed class IncidentProgressSnapshot
    {
        public IReadOnlyList<IncidentProgressEntry> Incidents { get; }
        public IncidentProgressEntry Current { get; }
        public IncidentProgressEntry Find(string incidentId);
    }

    public sealed class IncidentProgressResolver
    {
        public IncidentProgressSnapshot Resolve(
            PlayerSaveData save,
            IReadOnlyList<IncidentProgressDefinition> definitions);
    }
}
```

`IncidentRunner` gains an explicit conclusion flag and separates content exhaustion from story resolution:

```csharp
public IncidentRunner(
    string incidentId,
    IReadOnlyList<string> stageIds,
    int startingStageIndex,
    bool completesWhenAllStagesCompleted);

public bool IsContentExhausted { get; }
public IncidentStageCompletion CompleteCurrentStage(IncidentQuality quality);
```

The existing three-argument constructor remains as a compatibility overload and delegates with `completesWhenAllStagesCompleted: true`. `IncidentStageCompletion.IncidentCompleted` is true only when the exhausted final authored stage is also an actual conclusion.

Runtime incident definition contract:

```csharp
public IncidentDefinition(
    string id,
    LocalizedCopy title,
    string leadArtifactId,
    IncidentVisualCue boardVisualCue,
    bool completesWhenAllStagesCompleted,
    LocalizedCopy awaitingContentClue,
    IReadOnlyList<IncidentStageDefinition> stages);

public string LeadArtifactId { get; }
public IncidentVisualCue BoardVisualCue { get; }
public bool CompletesWhenAllStagesCompleted { get; }
public LocalizedCopy AwaitingContentClue { get; }
public IncidentProgressDefinition CreateProgressDefinition();
```

`awaitingContentClue` may be null only for a conclusive incident. `FirstIncidentCatalog` uses lead `unmelting-ice`, cue `Frost`, conclusion `true`, and no waiting clue. `SecondIncidentCatalog` uses lead `moon-umbrella`, cue `Rain`, conclusion `false`, and the approved first clue.

Presentation state contract:

```csharp
public enum IncidentCardAction { None = 0, Start = 1, Replay = 2 }

public sealed class IncidentCardState
{
    public string IncidentId { get; }
    public string Title { get; }
    public string LeadArtifactId { get; }
    public string Status { get; }
    public string Clue { get; }
    public string ActionLabel { get; }
    public IncidentCardAction Action { get; }
    public IncidentLifecycle Lifecycle { get; }
}

public sealed class IncidentBoardState
{
    public IncidentCardState Current { get; }
    public IReadOnlyList<IncidentCardState> Resolved { get; }
}

public sealed class IncidentBoardPresenter
{
    public IncidentBoardState Build(
        IReadOnlyList<IncidentDefinition> incidents,
        IncidentProgressSnapshot progress,
        Localizer localizer);
}
```

---

### Task 1: Write the complete red acceptance contract

**Files:**

- Create: `Assets/Tests/EditMode/IncidentProgressResolverTests.cs`
- Modify: `Assets/Tests/EditMode/IncidentProgressionContractTests.cs:49-113`
- Modify: `Assets/Tests/EditMode/ProgressionContractTests.cs:108-188`
- Modify: `Assets/Tests/EditMode/IncidentContentContractTests.cs:1-330`
- Modify: `Assets/Tests/EditMode/ContentCatalogContractTests.cs:150-185`
- Modify: `Assets/Tests/EditMode/EditorAutomationContractTests.cs`
- Modify: `Assets/Tests/PlayMode/GameAppPlayModeTests.cs:47-167,789-1030,2434-2455`
- Modify: `Assets/Tests/PlayMode/IncidentPresentationViewPlayModeTests.cs`

**Interfaces:**

- Consumes: the exact contracts in `Fixed Interfaces` and the object names below.
- Produces: one test-first acceptance boundary for Tasks 2–5.

- [ ] **Step 1: Add the resolver lifecycle tests.**

Create tests named:

- `Resolve_FirstIncompleteIncidentIsAvailableAndSecondIsLocked`
- `Resolve_FirstResolvedUnlocksSecondAtStageZero`
- `Resolve_OpenEndedIncidentWithAllAuthoredStagesCompleteAwaitsContent`
- `Resolve_AppendedStageMakesWaitingSaveAvailableWithoutReset`
- `Resolve_UnknownLegacyIncidentFallsBackWithoutLosingOtherSaveData`

Use this exact waiting-to-resume case:

```csharp
[Test]
public void Resolve_AppendedStageMakesWaitingSaveAvailableWithoutReset()
{
    var save = SaveWithStage("rain-01-voices");
    var resolver = new IncidentProgressResolver();

    var waiting = resolver.Resolve(save, Definitions(rainStages: new[] { "rain-01-voices" }));
    Assert.That(waiting.Current.Lifecycle, Is.EqualTo(IncidentLifecycle.AwaitingContent));

    var resumed = resolver.Resolve(
        save,
        Definitions(rainStages: new[] { "rain-01-voices", "rain-02-window" }));
    Assert.That(resumed.Current.Lifecycle, Is.EqualTo(IncidentLifecycle.Available));
    Assert.That(resumed.Current.NextStageIndex, Is.EqualTo(1));
}
```

- [ ] **Step 2: Extend runner and save tests.**

Add an open-ended runner assertion:

```csharp
[Test]
public void Runner_ExhaustsAvailableContentWithoutResolvingAnOpenIncident()
{
    var runner = new IncidentRunner(
        "remembering-rain",
        new[] { "rain-01-voices" },
        0,
        completesWhenAllStagesCompleted: false);

    var completion = runner.CompleteCurrentStage(IncidentQuality.Precise);

    Assert.That(completion.IncidentCompleted, Is.False);
    Assert.That(runner.IsContentExhausted, Is.True);
}
```

Change `ApplyIncidentStage_KeepsTheBestQualityAndAdvancesOnce` to also prove stage index 6 is not clamped to 5. Preserve the existing best-quality and duplicate-completion assertions.

- [ ] **Step 3: Update incident content expectations before changing production content.**

Replace all `CreateIncidents().Single()` calls with `Single(incident => incident.Id == "unmelting-ice")`. Add `SecondIncident_HasApprovedRulesQueueCopyAndTwoRequiredHolds`, asserting:

```csharp
Assert.That(stage.Queue.Select(item => item.ArtifactId), Is.EqualTo(new[]
{
    "moon-umbrella", "paper-fish", "sleeping-teacup", "clockwork-moth",
    "backward-candle", "patient-compass", "rain-jar", "porcelain-tooth",
    "humming-scarf", "yesterday-ticket", "murmur-box", "ink-snowglobe"
}));
Assert.That(Pattern(destinations), Is.EqualTo("VVRSRSVRSRSV"));
Assert.That(analyzer.MinimumHolds(destinations), Is.EqualTo(2));
Assert.That(incident.CompletesWhenAllStagesCompleted, Is.False);
Assert.That(incident.AwaitingContentClue.Korean,
    Is.EqualTo("빗속의 목소리는 선임 관리인을 알고 있다."));
```

- [ ] **Step 4: Add catalog, localization, builder and validator assertions.**

Assert two incidents, six total stages, two presentation styles, dynamic `{0}/{1}` stage labels, every new English/Korean key, deterministic profile paths, and validator summary text `2 incidents, 6 incident stages`.

- [ ] **Step 5: Add PlayMode menu and completion tests.**

Add tests named:

- `Menu_FirstResolved_ShowsRememberingRainAsCurrentAndIceAsReplayRecord`
- `Menu_RememberingRainWaiting_ShowsClueWithoutFakeActionButton`
- `RememberingRain_FirstShiftUsesApprovedKoreanOpeningAndQueue`
- `RememberingRain_CompletionPersistsStageButDoesNotResolveIncident`
- `ResolvedIceReplay_DoesNotMoveRememberingRainProgress`
- `IncidentBoardTransition_DisableAppliesStaticFinalState`
- `IncidentReaction_RainCueUsesCoolAtmosphereAndCompletesOnce`

Use these stable hierarchy names in assertions: `CurrentIncidentCard`, `IncidentState`, `IncidentTitle`, `IncidentClue`, `IncidentArtwork`, `IncidentButton`, `IncidentWaitingState`, `ResolvedIncidentCard_unmelting-ice`, `ReplayIncident_unmelting-ice`, `CollectionButton`, `FreeShiftButton`.

- [ ] **Step 6: Ask the human developer for the single red run.**

The Unity Editor must be closed first.

```powershell
Set-Location -LiteralPath 'C:\Users\D-\Documents\Codex_Project\Orbital_Salvage_Shop\.worktrees\three-seal-dockets'
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\scripts\test-unity.ps1
```

Expected result: compile or assertion failure naming missing lifecycle, second incident, board state/profile, or menu contracts. Record the first relevant failure; do not diagnose unrelated `.png.meta` whitespace as a test failure.

- [ ] **Step 7: Commit the red contract only.**

```powershell
git add -- Assets/Tests/EditMode/IncidentProgressResolverTests.cs Assets/Tests/EditMode/IncidentProgressionContractTests.cs Assets/Tests/EditMode/ProgressionContractTests.cs Assets/Tests/EditMode/IncidentContentContractTests.cs Assets/Tests/EditMode/ContentCatalogContractTests.cs Assets/Tests/EditMode/EditorAutomationContractTests.cs Assets/Tests/PlayMode/GameAppPlayModeTests.cs Assets/Tests/PlayMode/IncidentPresentationViewPlayModeTests.cs
git commit -m "test: define remembering rain acceptance contract"
```

### Task 2: Implement generic incident lifecycle and save restoration

**Files:**

- Create: `Assets/Scripts/Core/Incidents/IncidentProgressModel.cs`
- Create: `Assets/Scripts/Core/Incidents/IncidentProgressResolver.cs`
- Modify: `Assets/Scripts/Core/Incidents/IncidentRunner.cs:7-67`
- Modify: `Assets/Scripts/Core/Progression/ProgressionService.cs`
- Modify only if a test proves necessary: `Assets/Scripts/Core/Progression/PlayerSaveData.cs`

**Interfaces:**

- Consumes: existing `PlayerSaveData.incidentStageRecords`, `completedIncidentIds`, `activeIncidentId`, `activeIncidentStage`.
- Produces: the Core types and runner overload fixed above.

- [ ] **Step 1: Implement immutable progression model types.**

Validate blank IDs, empty stage lists, blank/duplicate stage IDs and null entries in constructors. Copy collections before exposing them as read-only. `IncidentProgressSnapshot.Find` returns null for a missing ID.

- [ ] **Step 2: Implement deterministic resolution.**

Use this precedence for each ordered definition:

```csharp
var recordedPrefix = CountContiguousRecordedStages(definition.StageIds, recordedStageIds);
var legacyIndex = string.Equals(save.activeIncidentId, definition.Id, StringComparison.Ordinal)
    ? Math.Min(save.activeIncidentStage, definition.StageIds.Count)
    : 0;
var nextStageIndex = Math.Max(recordedPrefix, legacyIndex);
var explicitlyResolved = completedIncidentIds.Contains(definition.Id);
var resolved = explicitlyResolved ||
               (definition.CompletesWhenAllStagesCompleted &&
                nextStageIndex == definition.StageIds.Count);
```

The first definition is unlocked. Each later definition is unlocked only if the immediately preceding definition is resolved. An unlocked non-resolved definition is `Available` when `nextStageIndex < StageIds.Count`, otherwise `AwaitingContent`. `Current` is the first unlocked non-resolved entry. Unknown saved IDs are ignored; the resolver must not delete or overwrite coins, locale, settings, discoveries or quality records.

- [ ] **Step 3: Separate runner exhaustion from resolution.**

Store `_completesWhenAllStagesCompleted`, add `IsContentExhausted`, and build completion as:

```csharp
var exhausted = completedIndex + 1 == _stageIds.Count;
var completion = new IncidentStageCompletion(
    IncidentId,
    _stageIds[completedIndex],
    completedIndex,
    quality,
    completedIndex + 1,
    exhausted && _completesWhenAllStagesCompleted);
```

Keep `IsComplete` as a compatibility alias to `IsContentExhausted` during this milestone so unrelated callers continue compiling; new code uses the precise name.

- [ ] **Step 4: Remove the hard-coded five-stage clamp.**

In `ProgressionService.ApplyIncidentStage`, replace `Math.Min(5, ...)` with `Math.Max(0, completion.NextStageIndex)`. Remove `DefaultIncidentId` and the old single-incident `RestoreIncident` method after all callers move to `IncidentProgressResolver`. Do not change unrelated shift, cosmetic or daily progression.

- [ ] **Step 5: Confirm Core remains Unity-free.**

```powershell
rg -n "using UnityEngine|UnityEngine\." Assets/Scripts/Core/Incidents Assets/Scripts/Core/Progression
```

Expected result: no matches in the new or changed Core incident files.

- [ ] **Step 6: Commit the lifecycle implementation.**

```powershell
git add -- Assets/Scripts/Core/Incidents/IncidentProgressModel.cs Assets/Scripts/Core/Incidents/IncidentProgressResolver.cs Assets/Scripts/Core/Incidents/IncidentRunner.cs Assets/Scripts/Core/Progression/ProgressionService.cs Assets/Scripts/Core/Progression/PlayerSaveData.cs
git commit -m "feat: resolve ordered incident progression"
```

### Task 3: Author Remembering Rain and bilingual status copy

**Files:**

- Create: `Assets/Scripts/Runtime/Content/Incidents/SecondIncidentCatalog.cs`
- Modify: `Assets/Scripts/Runtime/Content/Incidents/IncidentDefinition.cs:153-197`
- Modify: `Assets/Scripts/Runtime/Content/Incidents/FirstIncidentCatalog.cs`
- Modify: `Assets/Scripts/Runtime/Content/ContentCatalog.cs:150-155`
- Modify: `Assets/Scripts/Runtime/Localization/Localizer.cs:100-120,227-247`

**Interfaces:**

- Consumes: `IncidentProgressDefinition`, existing artifact catalog, `SortingRule`, `NarrativeBeat`, `ArtifactReaction`.
- Produces: two ordered definitions from `ContentCatalog.CreateIncidents()` and complete EN/KR menu copy.

- [ ] **Step 1: Extend `IncidentDefinition`.**

Implement the fixed constructor and properties. `CreateProgressDefinition()` maps `Stages.Select(stage => stage.Id)` and the conclusion flag. Require a valid `LeadArtifactId`; require `AwaitingContentClue` for non-conclusive definitions.

- [ ] **Step 2: Update the first incident without changing its five stages.**

Pass:

```csharp
leadArtifactId: "unmelting-ice",
boardVisualCue: IncidentVisualCue.Frost,
completesWhenAllStagesCompleted: true,
awaitingContentClue: null
```

Do not alter existing queues, dialogue, quality reactions or Hold counts.

- [ ] **Step 3: Create `SecondIncidentCatalog`.**

Use incident ID `remembering-rain`, stage ID `rain-01-voices`, resonance Hold artifact `paper-fish`, minimum required Holds `2`, and the exact queue from Task 1. Rules are ordered exactly:

```csharp
new SortingRule("incident-wet-vault", ArtifactTraits.Wet, ArtifactTraits.None, Destination.Vault, false),
new SortingRule("incident-fragile-repair", ArtifactTraits.Fragile, ArtifactTraits.None, Destination.Repair, false),
new SortingRule("incident-alive-storage", ArtifactTraits.Alive, ArtifactTraits.None, Destination.Storage, false),
new SortingRule("incident-fallback-storage", ArtifactTraits.None, ArtifactTraits.None, Destination.Storage, true)
```

Copy the exact bilingual intro, outro, clue and Stable/Precise/Resonant reactions from the approved spec. Use the approved moods and `Rain`, `InkSeal` cues. Do not add stage-only traits.

- [ ] **Step 4: Return both incidents in story order.**

```csharp
public static IReadOnlyList<IncidentDefinition> CreateIncidents()
    => new[] { FirstIncidentCatalog.Create(), SecondIncidentCatalog.Create() };
```

- [ ] **Step 5: Replace fixed `/5` menu copy with dynamic totals and add board keys.**

Use these exact pairs:

| Key | English | Korean |
| --- | --- | --- |
| `incident_continue` | `Continue Investigation · {0}/{1}` | `조사 계속 · {0}/{1}` |
| `incident_stage` | `Investigation {0}/{1}` | `조사 {0}/{1}` |
| `incident_first_investigation` | `Begin First Investigation` | `첫 조사 시작` |
| `incident_in_progress` | `Investigation in progress` | `조사 진행 중` |
| `incident_first_clue` | `FIRST CLUE` | `첫 단서` |
| `incident_waiting` | `Next shift in preparation` | `다음 교대 준비 중` |
| `incident_resolved` | `CASE RESOLVED` | `사건 해결` |
| `incident_replay_case` | `Replay Case` | `사건 다시보기` |
| `incident_return_board` | `Return to Incident Board` | `사건 보드로 돌아가기` |

- [ ] **Step 6: Commit content and localization.**

```powershell
git add -- Assets/Scripts/Runtime/Content/Incidents/IncidentDefinition.cs Assets/Scripts/Runtime/Content/Incidents/FirstIncidentCatalog.cs Assets/Scripts/Runtime/Content/Incidents/SecondIncidentCatalog.cs Assets/Scripts/Runtime/Content/ContentCatalog.cs Assets/Scripts/Runtime/Localization/Localizer.cs
git commit -m "feat: author remembering rain first shift"
```

### Task 4: Add generated Unity presentation profiles and validation

**Files:**

- Create: `Assets/Scripts/Runtime/Content/Incidents/IncidentPresentationStyleContent.cs`
- Create: `Assets/Scripts/Runtime/Content/IncidentPresentationProfile.cs`
- Modify: `Assets/Scripts/Runtime/Content/ContentCatalog.cs`
- Modify: `Assets/Scripts/Editor/ProjectBuilder.cs:64-78,710-738`
- Modify: `Assets/Scripts/Editor/ContentValidator.cs:39-180,260-300`
- Modify: `Assets/Scripts/Runtime/Presentation/VisualAssetLibrary.cs:1-110`

**Interfaces:**

- Consumes: incident IDs and source hex values.
- Produces: `VisualAssetLibrary.IncidentProfile(string incidentId)` with a safe null fallback.

- [ ] **Step 1: Define source and generated profile contracts.**

```csharp
public sealed class IncidentPresentationStyleContent
{
    public string IncidentId { get; }
    public string AccentHex { get; }
    public string SurfaceHex { get; }
    public float TransitionStrength { get; }
}

[CreateAssetMenu(menuName = "Curio Clerk/Incident Presentation Profile", fileName = "IncidentPresentation")]
public sealed class IncidentPresentationProfile : ScriptableObject
{
    public string IncidentId => incidentId;
    public Color AccentColor => accentColor;
    public Color SurfaceColor => surfaceColor;
    public float TransitionStrength => transitionStrength;
    public void Configure(IncidentPresentationStyleContent content);
}
```

`Configure` validates both hex strings with `ColorUtility.TryParseHtmlString`, clamps strength to `0.5f..1.5f`, and throws for blank IDs or invalid colors.

- [ ] **Step 2: Add exactly two source styles.**

```csharp
new IncidentPresentationStyleContent("unmelting-ice", "D6A85F", "6E334F", 0.85f),
new IncidentPresentationStyleContent("remembering-rain", "8094B8", "343B57", 1.00f)
```

- [ ] **Step 3: Generate deterministic assets.**

In `CreateContentAssets`, create `Assets/Resources/Content/IncidentPresentation`, then `LoadOrCreate<IncidentPresentationProfile>` at `.../{incidentId}.asset`, call `Configure`, and mark dirty. Do not delete unrelated generated assets.

- [ ] **Step 4: Validate source and generated profiles.**

`ContentValidator` must require two incident definitions, six total stages, globally unique incident/stage IDs, one valid style per incident, and two generated profile assets whose `IncidentId` matches the filename. Remove the rule that every incident must have exactly five stages. Keep per-stage bilingual, lead artifact, 12-item, 4/4/4 and Hold solvability checks.

Update the successful summary to:

```text
Curio Clerk validation passed: 24 artifacts, 10 rules, 2 rule packs, 3 docket templates, 2 incidents, 6 incident stages, 5 difficulties, 6 cosmetics, 2 scenes.
```

- [ ] **Step 5: Load profiles without making them required at runtime.**

Cache `Resources.Load<IncidentPresentationProfile>("Content/IncidentPresentation/" + incidentId)`. Return null for blank or missing IDs; views use their built-in wine/amber fallback.

- [ ] **Step 6: Commit source, builder, validator and loader changes.**

Do not stage generated `.asset` files until the human runs BuildAll in Task 6.

```powershell
git add -- Assets/Scripts/Runtime/Content/Incidents/IncidentPresentationStyleContent.cs Assets/Scripts/Runtime/Content/IncidentPresentationProfile.cs Assets/Scripts/Runtime/Content/ContentCatalog.cs Assets/Scripts/Editor/ProjectBuilder.cs Assets/Scripts/Editor/ContentValidator.cs Assets/Scripts/Runtime/Presentation/VisualAssetLibrary.cs
git commit -m "feat: generate reusable incident presentation profiles"
```

### Task 5: Integrate the incident board, replay and open-ended completion

**Files:**

- Create: `Assets/Scripts/Runtime/Presentation/IncidentBoardState.cs`
- Create: `Assets/Scripts/Runtime/Presentation/IncidentCardView.cs`
- Create: `Assets/Scripts/Runtime/Presentation/IncidentBoardView.cs`
- Create: `Assets/Scripts/Runtime/Presentation/IncidentBoardTransitionView.cs`
- Modify: `Assets/Scripts/Runtime/Presentation/GameApp.cs:80-120,155-175,200-425,1150-1190,2100-2180`
- Modify: `Assets/Scripts/Runtime/Presentation/IncidentReactionView.cs:160-190`

**Interfaces:**

- Consumes: `IncidentProgressResolver`, `IncidentBoardPresenter`, two runtime definitions, `IncidentPresentationProfile`, existing page-building helpers.
- Produces: menu A, playable second incident, safe replay, waiting state and decorative transition.

- [ ] **Step 1: Implement `IncidentBoardPresenter`.**

Map `Available` to a Start action, `AwaitingContent` to no action plus the approved clue, and `Resolved` to Replay. Use `NextStageIndex + 1` and the definition's actual stage count for dynamic labels. Return resolved cards in catalog order. Do not expose locked incidents as clickable cards.

- [ ] **Step 2: Implement `IncidentCardView`.**

Configure references once, then bind state:

```csharp
public void Configure(
    Image surface,
    Image artwork,
    TMP_Text status,
    TMP_Text title,
    TMP_Text clue,
    Button actionButton,
    TMP_Text actionLabel);

public void Bind(
    IncidentCardState state,
    IncidentPresentationProfile profile,
    Sprite artwork,
    UnityAction action);
```

Missing profile uses the current Wine/Amber palette. Missing artwork hides the Image without hiding text or action. `Action.None` hides the button and shows `IncidentWaitingState` as text; it never leaves a disabled button that looks actionable.

- [ ] **Step 3: Implement board and transition views.**

`IncidentBoardView` binds one current card plus zero or more resolved cards. `IncidentBoardTransitionView` animates only when `GameApp` passes a transient reveal flag: resolved card settles upward, current card rises and fades in, and a cool rain veil expands then clears in 1.2–1.8 seconds scaled by profile strength. `OnDisable` and `OnDestroy` stop the coroutine and apply the exact final RectTransform/CanvasGroup state. Progress and saving never wait for this animation.

- [ ] **Step 4: Replace the single-incident assumption in `GameApp`.**

Store the ordered definitions, resolver snapshot and selected definition. Replace `.Single()` and `RestoreIncidentProgress()` with `RefreshIncidentProgress()`:

```csharp
var definitions = _incidents.Select(value => value.CreateProgressDefinition()).ToArray();
_incidentProgress = _incidentProgressResolver.Resolve(_save, definitions);
var current = _incidentProgress.Current;
_activeIncident = current == null
    ? null
    : _incidents.Single(value => value.Id == current.Definition.Id);
_incidentRunner = current == null
    ? null
    : new IncidentRunner(
        _activeIncident.Id,
        _activeIncident.Stages.Select(stage => stage.Id).ToArray(),
        current.NextStageIndex,
        _activeIncident.CompletesWhenAllStagesCompleted);
```

ShowMenu creates the hero current card, compact resolved cards, `CollectionButton` → `ShowCollection`, `FreeShiftButton` and Settings. Preserve portrait safe-area bounds and one-thumb bottom actions.

- [ ] **Step 5: Make replay target-specific and non-destructive.**

Replace parameterless replay with `ReplayIncident(string incidentId)`. It creates an in-memory runner at stage 0 and sets `_isIncidentReplay`; it never changes `activeIncidentId`, stage records or completed IDs. Returning from replay refreshes the true current incident.

- [ ] **Step 6: Handle second-incident content exhaustion.**

At stage completion, use `completion.IncidentCompleted` for incident-complete sound/visuals and `runner.IsContentExhausted` only for navigation. For `remembering-rain` stage 1:

- persist `rain-01-voices` quality and `activeIncidentStage = 1`;
- do not add `remembering-rain` to `completedIncidentIds`;
- show the approved outro and quality reaction;
- label the final result action `Return to Incident Board`;
- return to the menu's first clue and waiting state.

First incident final completion sets an in-memory pending reveal for `remembering-rain`; relaunch skips the animation and shows the same correct static state.

- [ ] **Step 7: Extend key-reaction Rain cue.**

Add a Rain branch to `IncidentReactionView.AnimateCue` using the existing atmosphere overlay:

```csharp
if (cue == IncidentVisualCue.Rain)
{
    _warmthOverlay.enabled = true;
    _warmthOverlay.color = new Color(0.31f, 0.48f, 0.63f, presence * 0.28f);
    return;
}
```

Do not add texture or audio files. Keep callback-once and disable/enable recovery behavior.

- [ ] **Step 8: Commit presentation integration.**

```powershell
git add -- Assets/Scripts/Runtime/Presentation/IncidentBoardState.cs Assets/Scripts/Runtime/Presentation/IncidentCardView.cs Assets/Scripts/Runtime/Presentation/IncidentBoardView.cs Assets/Scripts/Runtime/Presentation/IncidentBoardTransitionView.cs Assets/Scripts/Runtime/Presentation/GameApp.cs Assets/Scripts/Runtime/Presentation/IncidentReactionView.cs
git commit -m "feat: present ordered incident board"
```

### Task 6: Generate, verify once, review and close the milestone

**Files:**

- Generated by human Unity action: `Assets/Resources/Content/IncidentPresentation/**`
- Modify after successful verification: `Docs/superpowers/specs/2026-09-03-multi-incident-remembering-rain-design.md`
- Review only: every file in the Planned File Map and the final Git diff

**Interfaces:**

- Consumes: completed Tasks 1–5.
- Produces: generated profiles, one green automated evidence run, one focused human UX verdict and a final milestone commit.

- [ ] **Step 1: Ask the human developer to generate assets.**

Open the correct worktree in Unity 6000.3.21f1 and choose:

```text
Tools > Curio Clerk > Generate Project Assets
```

Expected Console summary contains `2 incidents, 6 incident stages`. If compilation fails, diagnose the first C# error before retrying. After generation, save and close Unity so batch tests can acquire the project lock.

- [ ] **Step 2: Inspect generated scope before staging.**

```powershell
git status --short
git diff -- Assets/Resources/Content/IncidentPresentation Assets/Scripts Docs
```

Expected new generated content is only the two incident presentation `.asset` files and their `.meta` files. Do not stage pre-existing artifact `.png.meta`, TMP Settings, ProjectSettings or Google Mobile Ads files.

- [ ] **Step 3: Ask the human developer for the single green run.**

```powershell
Set-Location -LiteralPath 'C:\Users\D-\Documents\Codex_Project\Orbital_Salvage_Shop\.worktrees\three-seal-dockets'
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\scripts\test-unity.ps1
```

Expected: both `EditMode passed: N/N` and `PlayMode passed: N/N`. A Unity exit code without those two lines is not a pass; inspect the referenced log before retrying.

- [ ] **Step 4: Perform one focused human visual/play check.**

Reopen Unity and verify in Korean:

1. first incident resolved state shows `기억하는 비` as the large current card and `녹지 않는 얼음` as a compact replay record;
2. the first two items force the understandable first Hold, and the late Repair collision forces the second Hold;
3. Rain cue and large dialogue are visible without hiding rule text or bottom actions;
4. after all 12 items, the menu shows `빗속의 목소리는 선임 관리인을 알고 있다.` and `다음 교대 준비 중`;
5. 자유 교대 and first-incident replay still work, and replay does not reset the second incident.

Record only concrete failures: clipped text, unclear action, wrong state, missing cue, save regression or broken replay. Subjective requests for more spectacle become a separate polish decision rather than expanding this milestone silently.

- [ ] **Step 5: Update implementation status only after evidence exists.**

Change the spec status to `구현 및 자동 검증 완료, 인간 플레이 확인 완료` only if Step 3 and Step 4 both pass. Otherwise record the exact open issue without a completion claim.

- [ ] **Step 6: Run repository checks that do not require Unity.**

```powershell
git diff --check -- Assets/Scripts Assets/Tests Docs Assets/Resources/Content/IncidentPresentation
rg -n "CreateIncidents\(\)\.Single\(\)|Math\.Min\(5|Expected 1 incident|must contain five stages" Assets/Scripts Assets/Tests
```

Expected: scoped whitespace check exits 0 and the hard-coded single-incident/five-stage search returns no matches.

- [ ] **Step 7: Review scope line by line.**

Confirm:

- two incidents, six stages, no new art/audio/package;
- first incident queues and copy unchanged;
- second incident pattern `VVRSRSVRSRSV`, 4/4/4 and Hold count 2;
- `remembering-rain` is waiting, not resolved, after stage 1;
- adding `rain-02-*` would resume at index 1;
- missing profile/animation cannot block play or saving;
- only intended files are staged.

- [ ] **Step 8: Commit generated assets and final status.**

```powershell
git add -- Assets/Resources/Content/IncidentPresentation Docs/superpowers/specs/2026-09-03-multi-incident-remembering-rain-design.md
git commit -m "chore: finalize remembering rain vertical slice"
```

Do not push unless the developer explicitly asks.

---

## Plan Self-Review

- Spec coverage: lifecycle, menu A, save compatibility, second incident content, Hold route, bilingual copy, Unity profile/components, fallback behavior, tests and manual gate all map to Tasks 1–6.
- Scope control: no later Remembering Rain stages, third incident, new art/audio, monetization, server or broad Casebook redesign is included.
- Type consistency: `IncidentLifecycle`, `IncidentProgressDefinition`, `IncidentProgressEntry`, `IncidentProgressSnapshot`, `IncidentBoardState` and `IncidentCardState` use the same names across tests, content and presentation tasks.
- Persistence consistency: `IsContentExhausted` controls navigation; `IncidentCompleted` alone controls permanent resolution.
- Test cadence: all acceptance tests precede production changes; only one red and one green full Unity run are requested.
- Dirty-worktree safety: every commit command lists exact files and excludes existing Unity/ads changes.
