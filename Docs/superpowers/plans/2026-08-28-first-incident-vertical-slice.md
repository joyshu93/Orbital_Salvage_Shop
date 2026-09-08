# Curio Clerk First Incident Vertical Slice Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 승인된 서사형 직업 퍼즐 설계를 실제로 평가할 수 있도록, `녹지 않는 얼음` 사건의 5개 교대·대화·판단·반응·저장·결과를 하나의 10~15분짜리 Android 세로 수직 검증판으로 완성한다.

**Architecture:** 기존 `RuleEngine`, `ShiftSession`, 삼중 봉인 장부와 한 칸 Hold를 유일한 판정 권위로 유지한다. 새 Core 계층은 사건 단계·품질·진행만 계산하고, Runtime Content는 5개 저작 교대와 영어/한국어 장면을 제공하며, Presentation은 판정을 다시 계산하지 않고 대화·서리·물건 반응·결과를 표현한다. 기존 무작위 교대는 `Free Shift`로 보존하고 광고·도감·장식 코드는 삭제하지 않되 첫 사건의 주동기에서는 숨긴다.

**Tech Stack:** Unity 6000.3.21f1, C# 9, deterministic Core asmdef without UnityEngine, uGUI, TextMesh Pro, Unity Test Framework EditMode/PlayMode, `JsonUtility` save, generated localization tables through `ProjectBuilder.BuildAll`.

**Spec:** `Docs/superpowers/specs/2026-08-28-narrative-occupational-puzzle-design.md`

## Global Constraints

- Unity Editor/Hub, Unity batch/CLI, community Unity MCP를 에이전트가 실행하지 않는다. Unity 실행·테스트·`ProjectBuilder.BuildAll`은 인간 개발자가 수행한다.
- 작업 디렉터리는 `C:\Users\D-\Documents\Codex_Project\Orbital_Salvage_Shop\.worktrees\three-seal-dockets`이고 브랜치는 `codex/three-seal-dockets`다.
- 현재 수정된 20개 물건 `.png.meta`와 `Assets/TextMesh Pro/Resources/TMP Settings.asset`은 사용자/Unity 소유 변경이다. 이 계획의 커밋에 포함하거나 되돌리지 않는다. 모든 커밋은 명시적 파일만 `git add`한다.
- 플레이어용 새 문구는 영어와 한국어를 같은 변경에서 추가한다.
- 새 그림·효과·생성 문구·합성음 코드를 추가하기 전에 `Docs/AIAssetProvenance.md`와 `Docs/ThirdPartyNotices.md`를 먼저 갱신한다.
- 첫 사건이 플레이 경험 기준을 통과하기 전에는 나머지 11개 사건, 분기 대화, 경제, 광고, Firebase, 스토어 준비를 구현하지 않는다.
- 사건 교대도 물건 12개, 목적지별 4개, 강제 시간 제한 없음, 한 손 세로 조작을 유지한다.
- 기존 실패 규칙은 유지하되 사건 모드에서 하트가 0이면 광고 부활 대신 같은 단계를 즉시 재시도한다. 영구 손실은 없다.
- 테스트는 먼저 작성하되, 사용자의 테스트 피로를 줄이기 위해 Unity 전체 실행 요청은 Task 3, Task 8, Task 11의 세 milestone으로 묶는다. 작은 문구·애니메이션 수정마다 전체 suite를 요구하지 않는다.

## Planned File Map

| Layer | Add | Modify |
| --- | --- | --- |
| Core | `Assets/Scripts/Core/Incidents/IncidentQuality.cs`, `IncidentStageRun.cs`, `IncidentRunner.cs` | `Artifacts/ArtifactTraits.cs`, `Progression/PlayerSaveData.cs`, `Progression/ProgressionService.cs` |
| Runtime Content | `Assets/Scripts/Runtime/Content/Incidents/IncidentDefinition.cs`, `FirstIncidentCatalog.cs` | `ContentCatalog.cs` |
| Runtime Presentation | `Assets/Scripts/Runtime/Presentation/NarrativeSequenceView.cs`, `IncidentReactionView.cs` | `AppScreen.cs`, `GameApp.cs`, `DocketProgressView.cs`, `VisualAssetLibrary.cs` |
| Infrastructure | none | `Assets/Scripts/Runtime/Infrastructure/Feedback/IPlayerFeedbackService.cs`, `UnityPlayerFeedbackService.cs` |
| Editor | none | `Assets/Scripts/Editor/ContentValidator.cs`, `ProjectBuilder.cs` for deterministic narrative-art import |
| Tests | `Assets/Tests/EditMode/IncidentProgressionContractTests.cs`, `IncidentContentContractTests.cs`; `Assets/Tests/PlayMode/IncidentPresentationViewPlayModeTests.cs` | `SaveStoreContractTests.cs`, `ContentCatalogContractTests.cs`, `GameAppPlayModeTests.cs`, `ShiftPresentationViewPlayModeTests.cs` |
| Content/art/docs | four senior-clerk portraits, one frost overlay, `Docs/NarrativeSlicePrompts.md` | `Localizer.cs`, provenance/notices docs |

## Fixed Data Contracts

Core types must use these signatures so save, content, and presentation do not invent parallel state:

```csharp
namespace CurioClerk.Core.Incidents
{
    public enum IncidentQuality { Stable = 0, Precise = 1, Resonant = 2 }

    public sealed class IncidentStageRun
    {
        public IncidentStageRun(string stageId, string resonanceHoldArtifactId);
        public bool ResonanceConditionMet { get; }
        public void RecordHold(string artifactId);
        public IncidentQuality Evaluate(ShiftResult result);
    }

    public sealed class IncidentRunner
    {
        public IncidentRunner(string incidentId, IReadOnlyList<string> stageIds, int startingStageIndex);
        public string IncidentId { get; }
        public int CurrentStageIndex { get; }
        public string CurrentStageId { get; }
        public bool IsComplete { get; }
        public IncidentStageCompletion CompleteCurrentStage(IncidentQuality quality);
    }
}
```

`IncidentStageRun.Evaluate` is intentionally small: a completed shift with any mistake is `Stable`; no mistakes is `Precise`; no mistakes plus the authored Hold condition is `Resonant`. It throws for an active or failed shift so a failed run can never advance story state.

Save schema version 4 adds only JSON-friendly fields:

```csharp
[Serializable]
public sealed class IncidentStageRecord
{
    public string stageId = string.Empty;
    public int bestQuality;
}

public string activeIncidentId = "unmelting-ice";
public int activeIncidentStage;
public List<IncidentStageRecord> incidentStageRecords = new List<IncidentStageRecord>();
public List<string> completedIncidentIds = new List<string>();
```

No dictionary, polymorphic JSON, mid-animation save, or mid-shift resume is added. Relaunch resumes at the current stage's opening scene.

Runtime content uses immutable C# data, not a second rule engine:

```csharp
public sealed class IncidentStageDefinition
{
    public string Id { get; }
    public IReadOnlyList<NarrativeBeat> IntroBeats { get; }
    public IReadOnlyList<NarrativeBeat> OutroBeats { get; }
    public ArtifactReaction Reactions { get; }
    public string LeadArtifactId { get; }
    public string ResonanceHoldArtifactId { get; }
    public IReadOnlyList<IncidentArtifactEntry> Queue { get; }
    public IReadOnlyList<SortingRule> Rules { get; }
    public int MinimumRequiredHolds { get; }
    public ShiftPlan CreateShiftPlan(IReadOnlyDictionary<string, ArtifactContent> artifacts);
}
```

`IncidentArtifactEntry` contains `ArtifactId` and `AddedTraits`. `CreateShiftPlan` clones catalog artifacts with `baseTraits | AddedTraits`; base catalog assets are not mutated. `NarrativeBeat` and `ArtifactReaction` carry paired English/Korean strings and presentation cues.

The presentation enums live beside those content types and use these fixed values:

```csharp
public enum SeniorClerkMood { Neutral = 0, Concerned = 1, Alert = 2, Relieved = 3 }
public enum IncidentVisualCue { None = 0, Frost = 1, InkSeal = 2, AmberWarmth = 3, Rain = 4 }

public sealed class LocalizedCopy
{
    public string English { get; }
    public string Korean { get; }
    public string ForLocale(string locale);
}

public sealed class ArtifactReaction
{
    public LocalizedCopy Stable { get; }
    public LocalizedCopy Precise { get; }
    public LocalizedCopy Resonant { get; }
    public LocalizedCopy ForQuality(IncidentQuality quality);
}
```

## Authored Shift Matrix

Destination patterns use `R` = Repair, `S` = Storage, `V` = Vault. The queue order below is authoritative for the first slice and must be asserted by tests.

| Stage | Ordered rules | Queue IDs in order | Pattern | Min Hold | Resonance Hold |
| --- | --- | --- | --- | --- | --- |
| `ice-01-crack` | Fragile → R; Temporal → V; fallback → S | `unmelting-ice`, `moon-umbrella`, `clockwork-moth`, `mossy-watch`, `sleeping-teacup`, `patient-compass`, `rain-jar`, `porcelain-tooth`, `thimble-storm`, `rusty-comet`, `tide-locket`, `borrowed-shadow` | `RRSVRSVRSVSV` | 1 | none |
| `ice-02-spread` | Frosted → S; Cursed → V; Fragile → R; fallback → S | `whispering-key`, `silent-bell`, `sleeping-teacup`, `unmelting-ice*`, `backward-candle`, `moon-umbrella*`, `humming-scarf`, `clockwork-moth*`, `patient-compass*`, `lantern-snail`, `murmur-box`, `yesterday-ticket` | `VVRSRSVSSRVR` | 2 | none |
| `ice-03-tomorrow` | Temporal → V; Frosted → S; Fragile → R; fallback → S | `moon-umbrella`, `sleeping-teacup`, `clockwork-moth*`, `unmelting-ice*`, `patient-compass*`, `thimble-storm*`, `mossy-watch`, `porcelain-tooth`, `lantern-snail`, `rain-jar`, `tide-locket*`, `rusty-comet` | `RRSVSSVRRVSV` | 3 | none |
| `ice-04-frozen-seal` | Temporal → V; Frosted → S; Fragile → R; fallback → S | `unmelting-ice*`, `mossy-watch`, `moon-umbrella`, `clockwork-moth*`, `sleeping-teacup`, `patient-compass*`, `rain-jar`, `thimble-storm*`, `tide-locket*`, `porcelain-tooth`, `rusty-comet`, `lantern-snail` | `VVRSRSVSSRVR` | 2 | `mossy-watch` |
| `ice-05-thaw` | Temporal → V; Frosted → S; Fragile → R; fallback → S | `paper-fish`, `moon-umbrella`, `clockwork-moth*`, `unmelting-ice*`, `patient-compass*`, `thimble-storm*`, `mossy-watch`, `mirror-seed`, `ink-snowglobe`, `rain-jar`, `tide-locket*`, `rusty-comet` | `RRSVSSVRRVSV` | 3 | `moon-umbrella` |

`*` means `ArtifactTraits.Frosted` is added only for that stage. Stage 3 and later deliberately put Temporal above Frosted, so the frosted `unmelting-ice` resolves to Vault; this is the priority-learning proof. In Stage 4 the player files `unmelting-ice` to Vault, immediately sees `mossy-watch` also requiring Vault, and must Hold it while the next Repair item opens a valid order. This is the concrete mandatory-Hold proof.

---

### Task 1: Add incident quality, stage-run tracking, and deterministic advancement

**Files:**

- Create: `Assets/Scripts/Core/Incidents/IncidentQuality.cs`
- Create: `Assets/Scripts/Core/Incidents/IncidentStageRun.cs`
- Create: `Assets/Scripts/Core/Incidents/IncidentRunner.cs`
- Create: `Assets/Tests/EditMode/IncidentProgressionContractTests.cs`

- [ ] Write `IncidentProgressionContractTests` first with these cases: mistakes produce `Stable`; zero mistakes produce `Precise`; zero mistakes after holding the configured artifact produce `Resonant`; holding another artifact does not qualify; failed results cannot be evaluated; completing stages advances exactly once and the fifth completion marks the incident complete.

```csharp
[Test]
public void StageRun_ResonatesOnlyAfterTheAuthoredHold()
{
    var run = new IncidentStageRun("ice-04-frozen-seal", "mossy-watch");
    run.RecordHold("unmelting-ice");
    Assert.That(run.Evaluate(CompletedResult(mistakes: 0)), Is.EqualTo(IncidentQuality.Precise));

    run.RecordHold("mossy-watch");
    Assert.That(run.Evaluate(CompletedResult(mistakes: 0)), Is.EqualTo(IncidentQuality.Resonant));
}
```

- [ ] Implement the three Core types with constructor validation, copied read-only stage IDs, no `UnityEngine` reference, and no scoring or localization responsibility.
- [ ] Ensure `CompleteCurrentStage` returns an immutable `IncidentStageCompletion` containing `IncidentId`, `StageId`, zero-based completed index, `Quality`, `NextStageIndex`, and `IncidentCompleted`.
- [ ] Review the new Core files for `using UnityEngine`; expected result is no matches.
- [ ] Commit only the Core and test files.

```powershell
git add Assets/Scripts/Core/Incidents Assets/Tests/EditMode/IncidentProgressionContractTests.cs
git commit -m "feat: model incident stage outcomes"
```

### Task 2: Persist incident progress without damaging existing saves

**Files:**

- Modify: `Assets/Scripts/Core/Progression/PlayerSaveData.cs`
- Modify: `Assets/Scripts/Core/Progression/ProgressionService.cs`
- Modify: `Assets/Tests/EditMode/ProgressionContractTests.cs`
- Modify: `Assets/Tests/EditMode/SaveStoreContractTests.cs`

- [ ] Add failing tests for v3 → v4 migration, stage-record round-trip, invalid negative stage recovery, best-quality monotonic update, duplicate completed incident suppression, and preservation of coins/locale/feedback preferences during recovery.

```csharp
[Test]
public void ApplyIncidentStage_KeepsTheBestQualityAndAdvancesOnce()
{
    var save = new PlayerSaveData();
    var service = new ProgressionService();
    service.ApplyIncidentStage(save, Completion("ice-01-crack", IncidentQuality.Precise, 1, false));
    service.ApplyIncidentStage(save, Completion("ice-01-crack", IncidentQuality.Stable, 1, false));

    Assert.That(save.activeIncidentStage, Is.EqualTo(1));
    Assert.That(save.incidentStageRecords.Single().bestQuality, Is.EqualTo((int)IncidentQuality.Precise));
}
```

- [ ] Set `PlayerSaveData.CurrentVersion` to 4, add `IncidentStageRecord` and the four fields fixed above, and sanitize null lists, blank record IDs, duplicate records, invalid quality integers, negative stage values, and duplicate completion IDs. Do not reset unrelated progression.
- [ ] Add `ProgressionService.RestoreIncident(save, incidentId, stageIds)` and `ApplyIncidentStage(save, completion)`. Unknown/blank incident IDs restore to `unmelting-ice` stage 0; an index beyond the five known stages clamps to the completed boundary 5.
- [ ] Keep `ApplyShift`, daily, cosmetics, and existing save behavior unchanged.
- [ ] Commit only the four modified files.

```powershell
git add Assets/Scripts/Core/Progression/PlayerSaveData.cs Assets/Scripts/Core/Progression/ProgressionService.cs Assets/Tests/EditMode/ProgressionContractTests.cs Assets/Tests/EditMode/SaveStoreContractTests.cs
git commit -m "feat: persist first incident progress"
```

### Task 3: Author and validate the five deterministic incident shifts

**Files:**

- Modify first: `Docs/AIAssetProvenance.md`
- Modify first: `Docs/ThirdPartyNotices.md`
- Modify: `Assets/Scripts/Core/Artifacts/ArtifactTraits.cs`
- Create: `Assets/Scripts/Runtime/Content/Incidents/IncidentDefinition.cs`
- Create: `Assets/Scripts/Runtime/Content/Incidents/FirstIncidentCatalog.cs`
- Modify: `Assets/Scripts/Runtime/Content/ContentCatalog.cs`
- Modify: `Assets/Scripts/Editor/ContentValidator.cs`
- Create: `Assets/Tests/EditMode/IncidentContentContractTests.cs`
- Modify: `Assets/Tests/EditMode/ContentCatalogContractTests.cs`

- [ ] Before adding player-facing generated prose, add provenance entry `TEXT-NARRATIVE-004` covering the five-stage English/Korean prototype script and a matching no-third-party-attribution row.
- [ ] Add failing content tests that assert exactly one incident, five ordered stage IDs, twelve unique queue items per stage, the exact queue/pattern matrix above, four items per destination, declared minimum Hold counts, complete bilingual intro/outro/reaction copy, valid lead IDs, and valid resonance IDs.
- [ ] Add `ArtifactTraits.Frosted = 1 << 6`; do not add Frosted to any base artifact in `CreateArtifacts()`.
- [ ] Implement the immutable incident content types and `FirstIncidentCatalog.Create()`. `IncidentStageDefinition.CreateShiftPlan` must throw on missing artifact IDs, duplicate queue IDs, empty rules, or a queue count other than twelve.
- [ ] Use these short scene beats. Implementation must copy both languages as authored and may change punctuation only for line wrapping.

| Stage | Opening beat EN / KO | Closing hook EN / KO |
| --- | --- | --- |
| 1 | “First night? Start with the one that refuses to become water.” / “첫날이죠? 물이 되기를 거부하는 것부터 맡아 봅시다.” | “The crack is sealed. The leaf inside moved anyway.” / “금은 봉합됐어요. 그런데 안쪽의 낙엽은 움직였습니다.” |
| 2 | “The frost has chosen company. Treat every white-rimmed curio as one condition.” / “서리가 동료를 골랐군요. 흰 테가 생긴 물건은 모두 같은 상태로 보세요.” | “The leaf is gone. No water escaped.” / “낙엽이 사라졌어요. 물은 한 방울도 새지 않았는데요.” |
| 3 | “This watch carries the same leaf—and tomorrow’s date. Time takes priority over frost.” / “이 시계에도 같은 낙엽이 있어요. 날짜는 내일이고요. 시간 이상이 서리보다 우선입니다.” | “Tomorrow is pointing back at this desk.” / “내일이 이 책상을 가리키고 있습니다.” |
| 4 | “The Vault seal is frozen. Protect the next Vault curio in Hold and open Repair first.” / “봉인고 인장이 얼었습니다. 다음 봉인 물건은 보류에서 보호하고 수리실 순서를 먼저 여세요.” | “The held curios are trembling in the same rhythm.” / “보류했던 물건들이 같은 박자로 떨고 있어요.” |
| 5 | “No new rule tonight. Read the priority, protect the order, and let the ice answer.” / “오늘 새 규칙은 없습니다. 우선순위를 읽고, 순서를 보호하고, 얼음이 답하게 하세요.” | “The ice melts without water. Rain begins inside the sealed umbrella parcel.” / “얼음은 물 없이 녹았습니다. 봉인된 우산 소포 안에서 빗소리가 납니다.” |

- [ ] Give every stage three bilingual reactions. Their semantic contract is fixed: `Stable` acknowledges recovery, `Precise` acknowledges calm care, `Resonant` makes the artifact or office answer the player. Keep each reaction to at most two short sentences and at most 90 Korean characters / 150 English characters.
- [ ] Extend `ContentValidator.ValidateCatalog` to reject missing bilingual text, unbalanced destinations, unsolvable/under-declared Hold plans, invalid lead/resonance IDs, and missing quality reactions. Update its success log to include `1 incident, 5 incident stages`.
- [ ] Human milestone A — ask the developer to close any Unity instance using this worktree and run once:

```powershell
Set-Location -LiteralPath 'C:\Users\D-\Documents\Codex_Project\Orbital_Salvage_Shop\.worktrees\three-seal-dockets'
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\scripts\test-unity.ps1
```

Expected green baseline after implementation: all existing tests plus the new incident/save/content EditMode tests pass. If Unity exits 1 or 2 before producing XML, diagnose the relevant `Logs/EditMode.log`/`Logs/PlayMode.log`; do not repeatedly rerun unchanged code.
- [ ] Commit only the source, tests, and the two documentation files.

```powershell
git add Docs/AIAssetProvenance.md Docs/ThirdPartyNotices.md Assets/Scripts/Core/Artifacts/ArtifactTraits.cs Assets/Scripts/Runtime/Content/Incidents Assets/Scripts/Runtime/Content/ContentCatalog.cs Assets/Scripts/Editor/ContentValidator.cs Assets/Tests/EditMode/IncidentContentContractTests.cs Assets/Tests/EditMode/ContentCatalogContractTests.cs
git commit -m "feat: author the unmelting ice incident"
```

### Task 4: Add fixed bilingual incident interface copy

**Files:**

- Modify: `Assets/Scripts/Runtime/Localization/Localizer.cs`
- Modify: `Assets/Tests/EditMode/ContentCatalogContractTests.cs`

- [ ] Add a failing test that every key below exists in both `Localizer.Entries("en")` and `Entries("ko")`, contains no raw placeholder key, and keeps matching format placeholders.
- [ ] Add these keys with concise copy: `incident_begin`, `incident_continue`, `incident_stage`, `incident_complete`, `incident_next_teaser`, `free_shift`, `senior_clerk`, `narrative_continue`, `retry_stage`, `next_stage`, `quality_stable`, `quality_precise`, `quality_resonant`, `quality_stable_body`, `quality_precise_body`, `quality_resonant_body`, `trait_frosted`, `calm_streak`, `incident_hold_protect`, `incident_failed_body`.
- [ ] Use Korean labels `사건 시작`, `사건 계속 · {0}/5`, `사건 {0}/5`, `첫 사건 해결`, `다음 사건 · 실내에서 비를 맞은 우산`, `자유 교대`, `선임 관리인`, `계속`, `같은 교대 다시 하기`, `다음 교대`, `안정`, `정교`, `공명`, `서리 묻음`, `손길이 안정되었습니다`, `보호 보류` as the terminology source of truth.
- [ ] Do not add long lore paragraphs to `Localizer`; story beats remain in incident content.
- [ ] Commit the localization and contract test together.

```powershell
git add Assets/Scripts/Runtime/Localization/Localizer.cs Assets/Tests/EditMode/ContentCatalogContractTests.cs
git commit -m "feat: localize incident interface copy"
```

### Task 5: Create and approve the minimal narrative art set

**Files:**

- Modify before generation: `Docs/AIAssetProvenance.md`
- Modify before generation: `Docs/ThirdPartyNotices.md`
- Create: `Docs/NarrativeSlicePrompts.md`
- Create after approval: `Assets/Resources/Art/Characters/senior-clerk-neutral.png`
- Create after approval: `Assets/Resources/Art/Characters/senior-clerk-concerned.png`
- Create after approval: `Assets/Resources/Art/Characters/senior-clerk-alert.png`
- Create after approval: `Assets/Resources/Art/Characters/senior-clerk-relieved.png`
- Create after approval: `Assets/Resources/Art/Effects/frost-overlay.png`
- Modify: `Assets/Scripts/Runtime/Presentation/VisualAssetLibrary.cs`
- Modify: `Assets/Scripts/Editor/ProjectBuilder.cs`
- Modify: `Assets/Scripts/Editor/ContentValidator.cs`
- Modify: `Assets/Tests/EditMode/EditorAutomationContractTests.cs`
- Modify: `Assets/Tests/PlayMode/GameAppPlayModeTests.cs`

- [ ] Before calling any image tool, add provisional provenance IDs `ART-NARRATIVE-001` and `ART-EFFECT-001` and a no-third-party row. Record that release approval is pending.
- [ ] Invoke the `imagegen` skill and retain the exact normalized prompts in `Docs/NarrativeSlicePrompts.md`. The portrait prompt must request one original, non-famous adult senior night clerk, warm occult archive clothing, waist-up portrait, deep plum/parchment/brass palette, large mobile-readable silhouette, no text/logo/signature, no named artist/studio/franchise style. Generate neutral first, then edit that project-owned output for concerned, alert, and relieved expressions so identity and clothing stay consistent.
- [ ] Generate four 768 × 1024 transparent portrait PNGs and one 1024 × 1536 transparent frost-edge overlay with empty center, pale blue-white crystalline veins, subtle amber reflection, no text/symbol/logo, designed for a 9:16 mobile artifact card.
- [ ] Show the five PNG previews to the developer and pause for visual approval. Reject outputs with tiny facial features, photorealistic mismatch, horror gore, modern clothing, illegible silhouette, opaque checkerboard, text, logo, watermark, or inconsistent identity.
- [ ] After approval, place only selected outputs at the exact paths above; record SHA-256, dimensions, reference inputs, selection/rejection notes, and prototype status in provenance.
- [ ] Add `ProjectBuilder.ConfigureNarrativeArtAssets()` and call it from `BuildAll`. It creates the `Characters`/`Effects` resource folders, imports only the five exact paths, sets Sprite/Single, alpha transparency on, mipmaps off, and a mobile-appropriate max size. Add an editor automation contract for the call and import settings; extend `ContentValidator` to reject missing sprites.
- [ ] Extend `VisualAssetLibrary` with `SeniorClerk(SeniorClerkMood mood)` and `FrostOverlay`. Missing resources must return null so the UI retains a readable text fallback.
- [ ] Add a PlayMode resource contract that loads all four portraits and the overlay after Unity import and verifies non-null sprites and expected minimum dimensions. Check alpha corners with a non-Unity image inspection before committing instead of making shipped textures CPU-readable.
- [ ] Commit only the approved art, prompt/provenance records, library change, test, and their Unity-generated `.meta` files. Never include the pre-existing dirty artifact `.meta` files.

```powershell
git add Docs/AIAssetProvenance.md Docs/ThirdPartyNotices.md Docs/NarrativeSlicePrompts.md Assets/Resources/Art/Characters Assets/Resources/Art/Effects Assets/Scripts/Runtime/Presentation/VisualAssetLibrary.cs Assets/Scripts/Editor/ProjectBuilder.cs Assets/Scripts/Editor/ContentValidator.cs Assets/Tests/EditMode/EditorAutomationContractTests.cs Assets/Tests/PlayMode/GameAppPlayModeTests.cs
git commit -m "feat: add first incident narrative art"
```

### Task 6: Build a large, readable visual-novel dialogue view

**Files:**

- Create: `Assets/Scripts/Runtime/Presentation/NarrativeSequenceView.cs`
- Create: `Assets/Tests/PlayMode/IncidentPresentationViewPlayModeTests.cs`

- [ ] Write failing PlayMode tests for: first beat display; English/Korean selection; tap/button advancing exactly one beat; portrait expression change; callback exactly once; missing portrait leaving readable speaker/body text; disable/enable not duplicating completion.

```csharp
[UnityTest]
public IEnumerator NarrativeView_AdvancesLargeBilingualBeatsAndCompletesOnce()
{
    var view = CreateNarrativeView(out var speaker, out var body, out var portrait, out var button);
    var completionCount = 0;
    view.Play(TwoBeatSequence(), "ko", MoodSprite, () => completionCount++);
    Assert.That(body.text, Is.EqualTo("첫 문장"));

    button.onClick.Invoke();
    Assert.That(body.text, Is.EqualTo("둘째 문장"));
    button.onClick.Invoke();
    button.onClick.Invoke();
    Assert.That(completionCount, Is.EqualTo(1));
}
```

- [ ] Implement `Configure(TMP_Text speaker, TMP_Text body, Image portrait, Image cueSurface, Button continueButton)` and `Play(IReadOnlyList<NarrativeBeat> beats, string locale, Func<SeniorClerkMood, Sprite> portraitResolver, Action completed)`.
- [ ] Keep layout ownership in `GameApp`; the view owns only sequence state and expression/cue refresh. It must not know `ShiftSession`, destination rules, save data, or incident advancement.
- [ ] Require body text at least 36 px, speaker at least 28 px, no more than two visible sentences, and a button/tap target at least 96 logical pixels high in the later GameApp layout test.
- [ ] Commit the view and its isolated tests.

```powershell
git add Assets/Scripts/Runtime/Presentation/NarrativeSequenceView.cs Assets/Tests/PlayMode/IncidentPresentationViewPlayModeTests.cs
git commit -m "feat: add incident narrative sequence view"
```

### Task 7: Add four-tier artifact and office reactions

**Files:**

- Modify before audio changes: `Docs/AIAssetProvenance.md`
- Modify before audio changes: `Docs/ThirdPartyNotices.md`
- Create: `Assets/Scripts/Runtime/Presentation/IncidentReactionView.cs`
- Modify: `Assets/Scripts/Runtime/Presentation/DocketProgressView.cs`
- Modify: `Assets/Scripts/Runtime/Infrastructure/Feedback/IPlayerFeedbackService.cs`
- Modify: `Assets/Scripts/Runtime/Infrastructure/Feedback/UnityPlayerFeedbackService.cs`
- Modify: `Assets/Tests/PlayMode/IncidentPresentationViewPlayModeTests.cs`
- Modify: `Assets/Tests/PlayMode/ShiftPresentationViewPlayModeTests.cs`

- [ ] Add provenance entry `AUDIO-SYNTH-002` before code for new procedural key-reaction and incident-complete tones; state that no samples, voices, third-party audio, or named composer reference is used.
- [ ] Add failing tests that distinguish all four tiers: ordinary correct uses current filing motion; docket completion reveals a connected central sigil; key artifact reaction owns the card for 1.0–1.8 seconds and shows its authored line; incident completion warms the full screen and invokes `IncidentComplete` feedback once.
- [ ] Implement `IncidentReactionView.Configure(...)`, `SetFrosted(bool)`, `PlayKeyReaction(string text, IncidentVisualCue cue, Action completed)`, `PlayMistake(string text)`, and `PlayIncidentComplete(Action completed)`. Restore transforms, alpha, colors, and callbacks on disable/re-enable using the same safety pattern as `ShiftFeedbackAnimator`.
- [ ] Extend `DocketProgressView` with an optional completion sigil. Existing callers without the sigil must keep working; the current three-stamp pulse remains the fallback.
- [ ] Add `PlayerFeedbackCue.KeyReaction` and `IncidentComplete`; synthesize short warm chime/low-glass tones. Do not add music or binary audio in this slice.
- [ ] Avoid neon, confetti, arcade stars, red explosions, large score popups, and camera shake. Use frost whitening, amber warmth, ink/seal lines, artifact scale/tilt, and restrained haptics only.
- [ ] Commit the reaction components, feedback boundary, tests, and provenance docs.

```powershell
git add Docs/AIAssetProvenance.md Docs/ThirdPartyNotices.md Assets/Scripts/Runtime/Presentation/IncidentReactionView.cs Assets/Scripts/Runtime/Presentation/DocketProgressView.cs Assets/Scripts/Runtime/Infrastructure/Feedback Assets/Tests/PlayMode/IncidentPresentationViewPlayModeTests.cs Assets/Tests/PlayMode/ShiftPresentationViewPlayModeTests.cs
git commit -m "feat: add layered incident reactions"
```

### Task 8: Make the incident—not Free Shift—the primary menu and opening flow

**Files:**

- Modify: `Assets/Scripts/Runtime/Presentation/AppScreen.cs`
- Modify: `Assets/Scripts/Runtime/Presentation/GameApp.cs`
- Modify: `Assets/Tests/PlayMode/GameAppPlayModeTests.cs`

- [ ] Add failing GameApp tests asserting the main menu shows `IncidentButton`, `FreeShiftButton`, and `SettingsButton`; hides `DailyShiftButton`, `CollectionButton`, coin totals, and rewarded offers; and labels new/continued/completed incident state correctly in English and Korean.
- [ ] Add `AppScreen.Narrative` and `AppScreen.IncidentResults`. Keep existing enum values stable and append new values.
- [ ] In `Awake`, load `ContentCatalog.CreateIncidents().Single()`, restore its runner through `ProgressionService`, and do not start a shift before player input.
- [ ] Replace the main menu's motivation hierarchy: large incident title/state and one primary `IncidentButton`; quiet secondary `FreeShiftButton`; Settings. Keep `StartNewShift(seed)` as the Free Shift entry and leave Daily/Collection/Cosmetics code reachable only through existing public methods/tests, not the primary menu.
- [ ] Add `StartIncident()`, `ShowIncidentIntro()`, and `BeginIncidentStage()`. Starting or continuing plays the current stage intro through `NarrativeSequenceView`, then builds its authored `ShiftPlan` and a new `IncidentStageRun`.
- [ ] Keep the old separate tutorial code in place for now but stop routing first launch through `ShowTutorial()`. The first two incident shifts teach role, three desks, rules, and Hold in context. Do not delete tutorial code in the same change.
- [ ] Build the narrative page with portrait occupying roughly 45% of the screen, body 36–42 px, high-contrast paper dialogue panel, no gameplay HUD/buttons, and a full-width lower-third Continue target.
- [ ] Human milestone B — ask the developer to run `ProjectBuilder.BuildAll` once because localization/art/source content changed, then run `scripts/test-unity.ps1` once. Expected: all suites pass and generated localization contains the new keys.
- [ ] Ask for one Editor/Game-view manual check only: Korean menu → first incident opening → first artifact. Confirm role is clear, text is readable, portrait is consistent, and the shift begins without the old tutorial wall. Do not request all five shifts yet.
- [ ] Commit only the three source/test files plus generated localization assets changed by the human `BuildAll`; review every generated diff before staging and exclude unrelated TMP/artifact meta changes.

```powershell
git add Assets/Scripts/Runtime/Presentation/AppScreen.cs Assets/Scripts/Runtime/Presentation/GameApp.cs Assets/Tests/PlayMode/GameAppPlayModeTests.cs Assets/Localization
git commit -m "feat: lead with the first incident"
```

### Task 9: Integrate incident state, highlighted judgment, and mandatory Hold into the shift

**Files:**

- Modify: `Assets/Scripts/Runtime/Presentation/GameApp.cs`
- Modify: `Assets/Tests/PlayMode/GameAppPlayModeTests.cs`

- [ ] Add failing tests for an incident stage using exactly twelve authored items, all three destinations, Frosted trait localization, current matched rule highlight before input, larger artifact card, and a distinct `보호 보류` label.
- [ ] Add the concrete Stage 4 test below. It must use production content rather than a hand-built queue.

```csharp
[UnityTest]
public IEnumerator FrozenSeal_RequiresHoldingTheWatchAfterTheIceUsesVault()
{
    var app = CreateAppAtIncidentStage(stageIndex: 3, locale: "ko");
    yield return BeginIncidentShift(app);
    Assert.That(CurrentArtifactId(app), Is.EqualTo("unmelting-ice"));

    ChooseDestination(app, Destination.Vault);
    yield return WaitForFilingTransition(app);
    Assert.That(CurrentArtifactId(app), Is.EqualTo("mossy-watch"));
    Assert.That(ObjectText("SortFeedback"), Does.Contain("보류").And.Contain("보호"));
    Assert.That(Button("VaultButton").interactable, Is.False);
    Assert.That(Button("HoldButton").interactable, Is.True);
}
```

- [ ] For incident shifts only, use the authored plan and stage rule list. Free Shift must continue to use `ShiftPlanGenerator` unchanged.
- [ ] Increase the current artifact's visual priority without changing one-hand controls: artifact card at least 38% of vertical height, artwork at least 45% of card width, rule panel less prominent, destination buttons at least 110 logical pixels high, Hold directly above the destination row.
- [ ] Before every incident decision, highlight only the rule returned by `RuleEngine.ResolveDetailed(CurrentArtifact, _activeRules)`. Do not show an answer arrow or highlight the correct destination.
- [ ] When the current artifact has `Frosted`, call `IncidentReactionView.SetFrosted(true)` and show localized `FROSTED/서리 묻음`. Base content traits remain unchanged.
- [ ] In `HoldCurrent`, record the pre-Hold artifact ID in `IncidentStageRun` only after `_session.Hold()` succeeds. Stage 4 must therefore record `mossy-watch`, not the following artifact.
- [ ] Change the Hold explanation in incident mode from abstract duplicate-stamp copy to the authored protection sentence, while preserving the normal Free Shift copy.
- [ ] Track a presentation-only consecutive-correct counter from `SortOutcome`; at three, show `calm_streak`, reset on wrong, and do not turn it into a score multiplier.
- [ ] Run no additional full Unity suite here unless the editor reports a compile error; Task 11 is the next planned full checkpoint.
- [ ] Commit GameApp and its tests.

```powershell
git add Assets/Scripts/Runtime/Presentation/GameApp.cs Assets/Tests/PlayMode/GameAppPlayModeTests.cs
git commit -m "feat: integrate incident judgment and protective hold"
```

### Task 10: Connect correct, wrong, docket, and key-item reactions to authoritative outcomes

**Files:**

- Modify: `Assets/Scripts/Runtime/Presentation/GameApp.cs`
- Modify: `Assets/Tests/PlayMode/GameAppPlayModeTests.cs`

- [ ] Add failing tests for: wrong sort retains the artifact and shows the decisive rule; the next correct sort visually repairs the mistake state; ordinary correct does not use the key reaction; `unmelting-ice` and `moon-umbrella` use authored reactions; a completed docket plays the connected seal tier before advancing.
- [ ] In `ChooseDestination`, consume `SortOutcome` exactly once. Never re-sort or manually mutate `DocketState` to decide narrative quality.
- [ ] On wrong incident sort, keep the current artifact, show one decisive bilingual correction, call `IncidentReactionView.PlayMistake`, reset calm streak, and retain existing heart/mistake accounting. Avoid a generic pink `WRONG` banner as the dominant element.
- [ ] On ordinary correct, retain the existing destination filing motion plus procedural correct cue. On a lead/key artifact, first show its authored 1.0–1.8 second reaction, then complete the existing filing transition without an extra confirmation tap.
- [ ] On docket completion, display the connected seal/sigil and warm the desk. On the next correct after a mistake, visually close the sigil crack; this state is presentation only and resets at the next docket.
- [ ] Ensure transition callbacks remain exactly-once when the screen is disabled, rebuilt, paused, or destroyed. Gameplay advancement cannot depend on a particle, sprite, audio, or haptic existing.
- [ ] Commit the integration and tests.

```powershell
git add Assets/Scripts/Runtime/Presentation/GameApp.cs Assets/Tests/PlayMode/GameAppPlayModeTests.cs
git commit -m "feat: connect artifact reactions to filing outcomes"
```

### Task 11: Complete stage results, retry, progression, and the five-shift ending

**Files:**

- Modify: `Assets/Scripts/Runtime/Presentation/GameApp.cs`
- Modify: `Assets/Tests/PlayMode/GameAppPlayModeTests.cs`

- [ ] Add failing tests that a successful incident shift evaluates exactly once, records best quality, persists the next stage before leaving results, plays the stage outro, and starts the next authored stage. Add failure tests asserting no stage advance, no rewarded-ad button, no permanent loss, and an immediate same-stage retry button.
- [ ] Add a result-quality test for all three labels and reaction bodies in both languages. `Stable`, `Precise`, and `Resonant` all expose `NextStageButton`; none blocks story content.
- [ ] Implement `ShowIncidentResults()`, `RetryIncidentStage()`, and `ContinueIncident()`. Keep the existing `ShowResults()` path unchanged for Free Shift.
- [ ] Apply `ProgressionService.ApplyIncidentStage` and `Save()` before the result Continue button becomes available. Rebuilding the result screen or receiving duplicate callbacks must not advance twice.
- [ ] On failed incident shift, show the authored gentle failure body and only `RetryStageButton` plus menu return. Do not call `RequestReward`, show a revive ad, remove coins, wait, or alter completed stage records.
- [ ] After stage 5, show full-screen frost receding, warm office light, the final ice reaction, the sealed umbrella rain hook, `첫 사건 해결`, and `다음 사건 · 실내에서 비를 맞은 우산`. Since the next incident is out of scope, the teaser returns to the menu; it does not fabricate a playable sixth stage.
- [ ] Keep `Free Shift` available from the menu for replay/testing, but do not present its coins, daily file, casebook, or cosmetics as the ending reward.
- [ ] Human milestone C — if Task 8's BuildAll was completed after the final content/localization/art change, do not run it again. Run one `scripts/test-unity.ps1`. Expected test counts are the current 97 EditMode / 55 PlayMode plus every new test added by this plan; record the exact observed totals rather than predicting a fixed final number.
- [ ] Ask the developer for two complete Korean first-incident recordings in portrait: one careful run and one hesitation/mistake run. Use the checklist below; do not request release AAB yet.
- [ ] Commit GameApp, tests, and only reviewed generated assets/localization changes.

```powershell
git add Assets/Scripts/Runtime/Presentation/GameApp.cs Assets/Tests/PlayMode/GameAppPlayModeTests.cs Assets/Localization
git commit -m "feat: complete the first incident loop"
```

### Task 12: Review the vertical slice against player experience, not feature count

**Files:**

- Create: `Docs/Playtests/FirstIncidentVerticalSlice.md`
- Modify only if evidence requires it: files from Tasks 1–11

- [ ] Record device/emulator, resolution, locale, build/commit, run type, duration, stage reached, mistakes, Hold moments, and timestamped observations. Do not store tester names or personal data.
- [ ] For the developer recordings, verify: role stated within 60 seconds; first docket unassisted; Stage 4 Hold understood without a destination arrow; right/wrong immediately distinct; important Korean text readable; key artifact reaction visually owns the moment; next shift chosen without prompting; no perceived forced timer.
- [ ] Test the failure path once: spend all hearts, confirm same stage retry, confirm no ad/energy/wait, and confirm reopening the app resumes the same stage intro.
- [ ] Recruit five fresh players only after developer recordings show no obvious comprehension or readability blocker. Record aggregate counts for all eight approved experience gates from the spec.
- [ ] Ask every player: `지금 다음 교대를 할 수 없다면, 무엇이 가장 궁금한가요?` Mark the slice failed if answers center only on points/coins/unlocks rather than the ice, umbrella, senior clerk, or changing office.
- [ ] Do not start incident 2 when any hard gate fails. Fix only the smallest cause in the relevant earlier task, add a regression test where automatable, and repeat the affected recording—not the entire release pipeline.
- [ ] After all automated tests and experience gates pass, use `superpowers:requesting-code-review`, address only evidence-backed findings, then use `superpowers:finishing-a-development-branch` to choose merge/push handling.
- [ ] Commit the anonymized playtest report separately.

```powershell
git add Docs/Playtests/FirstIncidentVerticalSlice.md
git commit -m "docs: record first incident playtest evidence"
```

## Final Acceptance Checklist

- [ ] Main menu immediately answers who the player is and what case continues next.
- [ ] Five incident shifts are deterministic, balanced 4/4/4, solvable, and use 12 unique objects each.
- [ ] Stage 3 proves rule priority; Stage 4 proves a concrete mandatory protective Hold; Stage 5 combines both without new instructions.
- [ ] Dialogue, judgment, and reaction are visually distinct full-screen states with readable Korean and English.
- [ ] Correct, wrong, docket-complete, key-artifact, and incident-complete feedback are distinguishable without relying on score numbers.
- [ ] Stable/Precise/Resonant all advance; only their reactions differ.
- [ ] Failure retries the same stage immediately without ad, wait, energy, or permanent loss.
- [ ] Existing Free Shift still works and still uses deterministic Core rules.
- [ ] No new online/account/telemetry dependency, forced timer, banner/interstitial/app-open ad, loot box, or energy system exists.
- [ ] Provenance/notices are updated before every new art, generated-text, or audio addition.
- [ ] User/Unity dirty `.meta` and TMP settings files were never staged accidentally.
- [ ] Human-run Unity tests, BuildAll, Korean portrait recordings, and five-player experience evidence are recorded before incident 2 begins.
