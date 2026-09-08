# Remembering Rain Full-Incident Implementation Plan

> **For Codex:** REQUIRED SUB-SKILL: Use `superpowers:test-driven-development` for every behavior change, `unity-workbench:unity-feature-implementation` before Unity implementation work, and `superpowers:verification-before-completion` before claiming the milestone complete.

**Goal:** Complete all five shifts of `The Remembering Rain / 기억하는 비`, add reusable narrative beats between the first three dockets of an incident shift, resolve the incident cleanly, and reveal a read-only third-incident teaser without adding a new gameplay system.

**Architecture:** Keep rules, queues, progress, and story copy as immutable deterministic domain/content data. Extend the existing `NarrativeSequenceView` so the same visual-novel presentation can show a senior clerk or a named supernatural voice. `GameApp` owns one optional in-shift overlay and routes it through the existing owned-transition mechanism, so presentation cannot strand gameplay. A zero-stage, non-conclusive incident is a generic upcoming-content preview; adding its first stage later promotes it to playable content without save migration.

**Tech stack:** Unity 6000.3.21f1, C# domain/content code, uGUI, TextMesh Pro, Unity Test Framework, generated Resources and scenes via `ProjectBuilder.BuildAll`.

**Approved specification:** `Docs/superpowers/specs/2026-09-04-curio-clerk-story-master-design.md`, especially Incident 2 and Incident 3.

**Global constraints:** Preserve offline play, portrait one-hand controls, 12 artifacts per shift, four 3-item dockets, all three destinations, no forced timer, no new input mode, no new art/audio/package, no hand-edits to generated Resources or scenes, and bilingual English/Korean player copy. The human developer authorizes Codex to run the repository's Unity test/build scripts, control Unity Editor/Hub, run `ProjectBuilder.BuildAll`, and exercise the game for this milestone. Confirm the exact worktree before every Unity action, do not use a community Unity MCP, record validation evidence, and preserve unrelated dirty files.

## Fixed product boundary

This milestone delivers exactly:

- four additional Remembering Rain stages, making five in total;
- three short narrative interludes after dockets 1, 2, and 3 of each Rain stage;
- named dialogue for `Senior Clerk / 선임 관리인` and `Voice in the Rain / 빗속의 목소리`;
- completion of Remembering Rain after stage 5;
- a current but non-playable `One Minute Ahead / 1분 앞선 저녁` teaser;
- save-compatible resume, replay, and successor reveal behavior;
- automated contracts and a short Codex-operated acceptance playtest.

It does not deliver branching dialogue, a codex, a timer, new destinations, new traits, new art, voice acting, a third playable incident, or the rest of the campaign.

## Authored shift matrix

Destination letters below are `R = Repair`, `S = Storage`, and `V = Vault`. Every stage has four of each.

| Stage | Rules, top to bottom | Queue | Expected | Min Holds | Protected Hold |
| --- | --- | --- | --- | ---: | --- |
| `rain-01-voices` | Wet→V, Fragile→R, Alive→S, fallback→S | moon-umbrella, paper-fish, sleeping-teacup, clockwork-moth, backward-candle, patient-compass, rain-jar, porcelain-tooth, humming-scarf, yesterday-ticket, murmur-box, ink-snowglobe | `VVRSRSVRSRSV` | 2 | paper-fish |
| `rain-02-names-under-water` | Temporal→V, Wet→R, Alive→S, Fragile→R, fallback→S | moon-umbrella, mossy-watch, rain-jar, clockwork-moth, paper-fish, humming-scarf, yesterday-ticket, porcelain-tooth, whispering-key, ink-snowglobe, patient-compass, unmelting-ice | `RVVSRSVRSRSV` | 2 | rain-jar |
| `rain-03-unsent-letter` | Alive→S, Fragile→R, Cursed→V, fallback→V | rain-jar, humming-scarf, paper-fish, moon-umbrella, whispering-key, porcelain-tooth, clockwork-moth, mirror-seed, borrowed-shadow, yesterday-ticket, silent-bell, patient-compass | `VSSRVRSRVRVS` | 2 | paper-fish |
| `rain-04-dry-order` | Cursed→V, Wet→S, Fragile→R, Alive→S, fallback→R | sleeping-teacup, rain-jar, moon-umbrella, whispering-key, backward-candle, clockwork-moth, silent-bell, rusty-comet, ink-snowglobe, lantern-snail, tide-locket, murmur-box | `RSSVRSVRVRSV` | 2 | moon-umbrella |
| `rain-05-testimony` | Temporal→V, Wet→R, Alive→S, Cursed→V, Fragile→R, fallback→S | moon-umbrella, paper-fish, clockwork-moth, rain-jar, humming-scarf, patient-compass, whispering-key, thimble-storm, ink-snowglobe, borrowed-shadow, murmur-box, unmelting-ice | `RRSVSSVRRVSV` | 3 | paper-fish |

The stage 1 pattern is an existing contract and must be verified from the current implementation before editing. The four new patterns have been independently counted as 4/4/4. Stage 5 intentionally contains three adjacent duplicate destinations; the others contain one.

## Fixed narrative matrix

Each stage keeps its full-screen intro and outro. The following one-beat interludes appear after docket completion, before the next artifact is shown. A slash separates English and Korean; the speaker is explicit.

### Stage 1 — Voices

Keep the existing intro, outro, and quality reactions. Add:

1. **Voice in the Rain / 빗속의 목소리:** “Not that name. The one before it.” / “그 이름 말고. 그 전의 이름.”
2. **Senior Clerk / 선임 관리인:** “The rain is reciting intake labels we erased years ago.” / “비가 오래전에 지운 접수표의 이름들을 읊고 있어요.”
3. **Voice in the Rain / 빗속의 목소리:** “You kept the key. Did you keep the promise?” / “열쇠는 간직했구나. 약속도 간직했니?”

### Stage 2 — Names Under Water

Intro:

1. **Senior Clerk:** “The rain washed every current name from the ledger. Older names are surfacing underneath.” / “비가 장부의 지금 이름을 모두 씻어 냈어요. 그 아래에서 오래된 이름들이 떠오릅니다.”
2. **Senior Clerk:** “Memories that point to another time go to the Vault first. Mend what is wet, let the living rest, then repair what is merely fragile.” / “다른 시간을 가리키는 기억은 먼저 봉인고로 보내세요. 젖은 것은 수리하고, 살아 있는 것은 쉬게 한 뒤, 단지 깨지기 쉬운 것을 수리합니다.”

Interludes:

1. **Voice in the Rain:** “Tuesday. You always took the Tuesday watch.” / “화요일. 넌 늘 화요일 당번이었지.”
2. **Senior Clerk:** “That was before this coat, before anyone called me senior.” / “이 외투를 입기 전, 아무도 저를 선임이라 부르기 전의 일이에요.”
3. **Voice in the Rain:** “Do not let the jar forget which Tuesday.” / “그 병이 어느 화요일인지 잊게 두지 마.”

Outro:

1. **Senior Clerk:** “The date inside the jar matches my oldest surviving duty sheet.” / “병 속 날짜가 남아 있는 제 가장 오래된 근무표와 일치해요.”
2. **Voice in the Rain:** “I remember the shift you chose not to.” / “네가 기억하지 않기로 한 근무를 나는 기억해.”

Quality reactions:

- Stable: “The old names remain blurred, but they no longer wash away.” / “오래된 이름은 흐릿하지만 더는 씻겨 나가지 않는다.”
- Precise: “Tuesday settles into one legible line of the ledger.” / “화요일이 장부의 읽을 수 있는 한 줄로 가라앉는다.”
- Resonant: “The held jar preserves a name the senior clerk cannot bring themself to read aloud.” / “보호한 빗물병이 선임 관리인이 차마 소리 내어 읽지 못하는 이름 하나를 지킨다.”

### Stage 3 — Unsent Letter

Intro:

1. **Senior Clerk:** “The paper fish unfolded during the day. It was folded from a letter that was never sent.” / “낮 동안 종이 물고기가 펼쳐졌어요. 보내지 못한 편지로 접혀 있었습니다.”
2. **Senior Clerk:** “Let living words rest before you mend their paper. Vault anything cursed. We need the sentence intact.” / “살아 있는 문장은 종이를 고치기 전에 쉬게 하세요. 저주받은 것은 봉인하고요. 문장을 온전히 남겨야 합니다.”

Interludes:

1. **Voice in the Rain:** “To the place that keeps what has nowhere else to go...” / “달리 갈 곳 없는 것을 지키는 곳에게...”
2. **Senior Clerk:** “There is no person’s name after ‘To.’ Keep the folds in order.” / “‘받는 이’ 뒤에 사람 이름이 없어요. 접힌 순서를 지켜 주세요.”
3. **Voice in the Rain:** “Night Repository. If this reaches you, you have begun to forget.” / “야간 보관소. 이 편지가 닿았다면, 너는 잊기 시작한 거야.”

Outro:

1. **Senior Clerk:** “The letter was addressed to the repository itself, before I became senior.” / “이 편지는 제가 선임이 되기 전부터 보관소 자체에 보내진 것이었어요.”
2. **Senior Clerk:** “It was not unsent. The recipient never knew how to answer.” / “보내지 못한 게 아니었어요. 받는 곳이 답하는 법을 몰랐던 겁니다.”

Quality reactions:

- Stable: “The letter closes without losing another word.” / “편지가 단어를 더 잃지 않고 접힌다.”
- Precise: “Every crease returns to its original sentence.” / “모든 접힌 자국이 원래 문장으로 돌아간다.”
- Resonant: “The protected paper fish swims once around the words ‘Night Repository.’” / “보호한 종이 물고기가 ‘야간 보관소’라는 글자 둘레를 한 바퀴 헤엄친다.”

### Stage 4 — Dry Order

Intro:

1. **Senior Clerk:** “A perfectly dry order fell from the wet umbrella. It demands a retrieval review of this repository.” / “젖은 우산에서 완전히 마른 명령서가 떨어졌어요. 이 보관소의 회수 심사를 요구하고 있습니다.”
2. **Senior Clerk:** “Contain cursed evidence first. Store what is rain-touched, mend what is fragile, and let the living rest. Do not spend the umbrella’s last voice too early.” / “저주받은 증거를 먼저 봉인하세요. 비에 닿은 것은 보관하고, 깨지기 쉬운 것은 수리하고, 살아 있는 것은 쉬게 하세요. 우산의 마지막 목소리를 너무 일찍 쓰면 안 됩니다.”

Interludes:

1. **Senior Clerk:** “The form is official. The sender line is empty.” / “양식은 공식 문서가 맞아요. 발신란만 비어 있습니다.”
2. **Voice in the Rain:** “It was not delivered from outside.” / “그건 밖에서 배달된 게 아니야.”
3. **Senior Clerk:** “The seal is dry because it was stamped inside the umbrella.” / “인장이 마른 건 우산 안쪽에서 찍혔기 때문이에요.”

Outro:

1. **Senior Clerk:** “This order was waiting here. No courier, no outside office, no sender.” / “이 명령서는 여기서 기다리고 있었어요. 배달원도, 외부 기관도, 발신자도 없이.”
2. **Voice in the Rain:** “Then ask who taught the house to sign.” / “그렇다면 누가 이 집에 서명하는 법을 가르쳤는지 물어.”

Quality reactions:

- Stable: “The dry order remains sealed and readable.” / “마른 명령서가 봉인된 채 읽을 수 있게 남는다.”
- Precise: “The blank sender line shines more clearly than the printed order.” / “빈 발신란이 인쇄된 명령보다 더 선명하게 빛난다.”
- Resonant: “The protected umbrella places one warm drop beside the empty signature.” / “보호한 우산이 빈 서명 옆에 따뜻한 빗방울 하나를 놓는다.”

### Stage 5 — Testimony

Intro:

1. **Senior Clerk:** “The letter fragments and the rain are replaying one final night. This time, we listen in order.” / “편지 조각과 비가 마지막 밤 하나를 되풀이하고 있어요. 이번에는 순서대로 듣겠습니다.”
2. **Senior Clerk:** “Time first, then water, then the living voice. Protect the paper fish until its sentence has somewhere safe to land.” / “시간을 먼저, 그다음 물을, 그다음 살아 있는 목소리를 다루세요. 종이 물고기의 문장이 안전하게 닿을 곳이 생길 때까지 보호합니다.”

Interludes:

1. **Voice in the Rain:** “If the rooms forget their names, the ledger will mistake the whole house for lost property.” / “방들이 자기 이름을 잊으면, 장부는 이 집 전체를 분실물로 오인할 거야.”
2. **Voice in the Rain:** “Leave the order where the next careful hand can find it.” / “다음의 조심스러운 손이 찾을 수 있는 곳에 명령서를 남겨.”
3. **Senior Clerk:** “Those coordinates are not a street. They are this desk.” / “이 좌표는 거리가 아니에요. 바로 이 작업대입니다.”

Outro:

1. **Voice in the Rain:** “I was the clerk before your senior. I left the truth for whoever came next.” / “나는 네 선임보다 먼저 일한 관리인이야. 다음에 올 사람을 위해 진실을 남겼어.”
2. **Senior Clerk:** “Then the order was meant to be found here—not obeyed in silence.” / “그렇다면 이 명령서는 여기서 발견되기 위한 것이었어요. 아무 말 없이 따르기 위한 게 아니라.”
3. **Senior Clerk:** “Tomorrow, we trace the time printed beneath the missing signature.” / “내일은 사라진 서명 아래에 찍힌 시간을 추적하겠습니다.”

Quality reactions:

- Stable: “The testimony survives, incomplete but no longer scattered.” / “증언이 불완전하지만 더는 흩어지지 않은 채 남는다.”
- Precise: “The final shift plays in a clear, unbroken order.” / “마지막 근무가 끊김 없이 선명한 순서로 재생된다.”
- Resonant: “The paper fish rests on the desk coordinates as though the letter has finally arrived.” / “종이 물고기가 작업대 좌표 위에 내려앉는다. 편지가 마침내 도착한 듯하다.”

Third-incident preview:

- ID: `one-minute-ahead`
- Title: `One Minute Ahead / 1분 앞선 저녁`
- Lead artifact: `backward-candle`
- Board cue: `AmberWarmth`
- Zero stages; `completesWhenAllStagesCompleted = false`
- Awaiting clue: `The moss grows toward 2:17. / 이끼가 2시 17분을 향해 자라고 있다.`
- Presentation style: accent `D6A85F`, deep surface `4A2D36`, cue intensity `0.92`.

## Task 1: Extend immutable incident content for named docket beats

**Files:**

- Modify: `Assets/Scripts/Runtime/Content/Incidents/IncidentDefinition.cs`
- Modify: `Assets/Scripts/Runtime/Presentation/NarrativeSequenceView.cs`
- Modify: `Assets/Tests/EditMode/IncidentContentContractTests.cs`
- Modify: `Assets/Tests/PlayMode/IncidentPresentationViewPlayModeTests.cs`

**Step 1: Write failing EditMode contracts**

Add tests proving:

- `NarrativeBeat` preserves an optional bilingual speaker and the old constructor leaves it null;
- `IncidentDocketBeat` rejects docket numbers outside 1–3 and null narrative;
- `IncidentStageDefinition` defensively copies docket beats;
- duplicate docket numbers are rejected;
- `FindDocketBeat(1..3)` returns the authored beat and other values return null.

Use this public shape:

```csharp
public sealed class IncidentDocketBeat
{
    public IncidentDocketBeat(int completedDocketNumber, NarrativeBeat narrative);
    public int CompletedDocketNumber { get; }
    public NarrativeBeat Narrative { get; }
}

public sealed class NarrativeBeat
{
    public NarrativeBeat(LocalizedCopy copy, SeniorClerkMood mood, IncidentVisualCue visualCue);
    public NarrativeBeat(LocalizedCopy speaker, LocalizedCopy copy,
        SeniorClerkMood mood, IncidentVisualCue visualCue);
    public LocalizedCopy Speaker { get; }
}
```

Append `IReadOnlyList<IncidentDocketBeat> docketBeats = null` to the stage constructor to preserve all existing call sites. Normalize null to an empty read-only list.

**Step 2: Write a failing PlayMode view test**

Play one beat with speaker `Voice in the Rain / 빗속의 목소리`, assert the localized speaker is shown, click Continue, and assert completion fires once. Retain a regression test that a null speaker still displays the localized `senior_clerk` label.

**Step 3: Implement the smallest model and view change**

In `NarrativeSequenceView.RefreshBeat()` select:

```csharp
_speaker.text = beat.Speaker == null
    ? new Localizer(_locale).Get("senior_clerk")
    : beat.Speaker.ForLocale(_locale);
```

Do not add a second dialogue component or a speaker enum; bilingual data already models the required extension.

**Step 4: Unity checkpoint**

With Unity Editor closed, run:

```powershell
Set-Location -LiteralPath 'C:\Users\D-\Documents\Codex_Project\Orbital_Salvage_Shop\.worktrees\three-seal-dockets'
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\scripts\test-unity.ps1
```

Expected baseline increase: at least 5 new EditMode tests and 2 new PlayMode assertions, with no failures. Diagnose logs before proceeding if either process exits nonzero.

**Step 5: Commit**

```powershell
git add Assets/Scripts/Runtime/Content/Incidents/IncidentDefinition.cs Assets/Scripts/Runtime/Presentation/NarrativeSequenceView.cs Assets/Tests/EditMode/IncidentContentContractTests.cs Assets/Tests/PlayMode/IncidentPresentationViewPlayModeTests.cs
git commit -m "feat: model incident docket narrative beats"
```

## Task 2: Support a generic zero-stage upcoming incident

**Files:**

- Modify: `Assets/Scripts/Core/Incidents/IncidentProgressModel.cs`
- Modify: `Assets/Scripts/Runtime/Content/Incidents/IncidentDefinition.cs`
- Modify: `Assets/Scripts/Runtime/Presentation/GameApp.cs`
- Modify: `Assets/Tests/EditMode/IncidentProgressResolverTests.cs`
- Modify: `Assets/Tests/EditMode/IncidentContentContractTests.cs`
- Modify: `Assets/Tests/PlayMode/GameAppPlayModeTests.cs`

**Step 1: Write failing domain tests**

Prove the following lifecycle without depending on menu UI:

- a locked zero-stage open incident remains `Locked` until its predecessor is resolved;
- once unlocked it becomes `AwaitingContent` with `NextStageIndex == 0`;
- a zero-stage conclusive incident is rejected;
- when a later build appends a first stage with the same incident ID, the same save resolves it as `Available` without migration.

Change constructor validation to permit no stages only when `completesWhenAllStagesCompleted` is false. Keep the bilingual awaiting-clue requirement.

```csharp
if (completesWhenAllStagesCompleted && stageIds.Count == 0)
{
    throw new ArgumentException(
        "A conclusive incident requires at least one stage.", nameof(stageIds));
}
```

Mirror that invariant in `IncidentDefinition`.

**Step 2: Write failing GameApp tests**

Create three incidents where the third has no stages. After resolving the second, assert:

- the third card is current and shows its awaiting clue;
- no start/continue button is present or interactable;
- no `IncidentRunner` is constructed and no exception occurs;
- reopening the menu produces the same state.

**Step 3: Guard runner creation by lifecycle**

Update `RefreshIncidentProgress()` so a runner exists only for playable content:

```csharp
_incidentRunner = current == null || current.Lifecycle != IncidentLifecycle.Available
    ? null
    : new IncidentRunner(
        _activeIncident.Id,
        _activeIncident.Stages.Select(stage => stage.Id).ToArray(),
        current.NextStageIndex,
        _activeIncident.CompletesWhenAllStagesCompleted);
```

Do not add a teaser-specific ID check. This is a reusable content-delivery boundary.

**Step 4: Commit after passing supplied Unity results**

```powershell
git add Assets/Scripts/Core/Incidents/IncidentProgressModel.cs Assets/Scripts/Runtime/Content/Incidents/IncidentDefinition.cs Assets/Scripts/Runtime/Presentation/GameApp.cs Assets/Tests/EditMode/IncidentProgressResolverTests.cs Assets/Tests/EditMode/IncidentContentContractTests.cs Assets/Tests/PlayMode/GameAppPlayModeTests.cs
git commit -m "feat: support upcoming incident previews"
```

## Task 3: Author the complete Remembering Rain incident and successor teaser

**Files:**

- Modify: `Assets/Scripts/Runtime/Content/Incidents/SecondIncidentCatalog.cs`
- Create: `Assets/Scripts/Runtime/Content/Incidents/ThirdIncidentCatalog.cs`
- Create: `Assets/Scripts/Runtime/Content/Incidents/ThirdIncidentCatalog.cs.meta`
- Modify: `Assets/Scripts/Runtime/Content/ContentCatalog.cs`
- Modify: `Assets/Scripts/Editor/ContentValidator.cs`
- Modify: `Assets/Tests/EditMode/ContentCatalogContractTests.cs`
- Modify: `Assets/Tests/EditMode/IncidentContentContractTests.cs`
- Modify: `Docs/AIAssetProvenance.md`
- Modify: `Docs/ThirdPartyNotices.md`

**Step 1: Register runtime narrative provenance before adding the copy**

Add `TEXT-NARRATIVE-006` covering the five-stage Remembering Rain script and docket interludes. Update the existing ThirdPartyNotices Remembering Rain row to reference it. State that all English/Korean prose is prototype copy requiring in-context review.

**Step 2: Replace single-stage assumptions with failing catalog contracts**

Assert:

- ordered incident IDs are `unmelting-ice`, `remembering-rain`, `one-minute-ahead`;
- total stage count is 10;
- Rain stage IDs match the matrix exactly;
- every Rain stage has 12 unique artifacts, exact rule order, exact destination pattern, 4/4/4 balance, expected minimum Holds, expected lead, expected protected artifact, three docket beats numbered 1/2/3, and bilingual nonblank copy/speaker;
- `one-minute-ahead` has zero stages and the fixed title, lead, cue, clue, and open status;
- all exposed collections remain immutable.

Do not test prose by duplicating every sentence. Test key plot anchors per stage: `Tuesday/화요일`, `Night Repository/야간 보관소`, empty sender, `desk/작업대`, and previous clerk identity.

**Step 3: Author content from the fixed matrices**

Refactor `SecondIncidentCatalog` only enough to avoid unreadable duplication. Preferred helpers:

```csharp
private static NarrativeBeat Senior(string en, string ko,
    SeniorClerkMood mood, IncidentVisualCue cue)
    => new NarrativeBeat(Copy(en, ko), mood, cue);

private static NarrativeBeat RainVoice(string en, string ko,
    SeniorClerkMood mood = SeniorClerkMood.Alert)
    => new NarrativeBeat(
        Copy("Voice in the Rain", "빗속의 목소리"),
        Copy(en, ko), mood, IncidentVisualCue.Rain);

private static IncidentDocketBeat Docket(int number, NarrativeBeat beat)
    => new IncidentDocketBeat(number, beat);
```

Use the exact shift and narrative matrices in this plan. Set Remembering Rain to `completesWhenAllStagesCompleted: true`; its awaiting clue may be null because it is now conclusive.

Add `ThirdIncidentCatalog.CreatePreview()` with the fixed preview data. In `ContentCatalog` return all three incidents and all three presentation styles.

**Step 4: Update deterministic validation**

Change expected counts to `3 incidents` and `10 incident stages`. Validate each stage has either zero docket beats or exactly three ordered beats, each beat has bilingual speaker and body, and completed docket numbers are 1, 2, 3 with no duplicates. Validate zero-stage incidents are open and have a bilingual awaiting clue.

**Step 5: BuildAll and tests**

Open only this worktree in Unity and run `Tools > Curio Clerk > Generate Project Assets`. Expected Console summary:

```text
Curio Clerk validation passed: 24 artifacts, 10 rules, 2 rule packs, 3 docket templates, 3 incidents, 10 incident stages, 5 difficulties, 6 cosmetics, 2 scenes.
```

Then close Unity and run `scripts/test-unity.ps1`. Do not treat Unity exit code 1 or 2 as a test assertion failure until `Logs/EditMode.log`, `Logs/PlayMode.log`, and XML results have been inspected; retry only after confirming no Unity process owns the project.

**Step 6: Commit source and generated profile**

Include only generated files causally changed by BuildAll, expected to include `Assets/Resources/Content/IncidentPresentation/one-minute-ahead.asset` and its `.meta`. Do not stage unrelated importer changes.

```powershell
git add Assets/Scripts/Runtime/Content/Incidents/SecondIncidentCatalog.cs Assets/Scripts/Runtime/Content/Incidents/ThirdIncidentCatalog.cs Assets/Scripts/Runtime/Content/Incidents/ThirdIncidentCatalog.cs.meta Assets/Scripts/Runtime/Content/ContentCatalog.cs Assets/Scripts/Editor/ContentValidator.cs Assets/Tests/EditMode/ContentCatalogContractTests.cs Assets/Tests/EditMode/IncidentContentContractTests.cs Assets/Resources/Content/IncidentPresentation/one-minute-ahead.asset Assets/Resources/Content/IncidentPresentation/one-minute-ahead.asset.meta Docs/AIAssetProvenance.md Docs/ThirdPartyNotices.md
git commit -m "feat: complete remembering rain incident content"
```

## Task 4: Integrate the reusable in-shift docket interlude

**Files:**

- Modify: `Assets/Scripts/Runtime/Presentation/GameApp.cs`
- Modify: `Assets/Tests/PlayMode/GameAppPlayModeTests.cs`

**Step 1: Write failing PlayMode integration tests**

Cover:

1. After the first docket of Rain, the normal sigil completion finishes, then `IncidentDocketInterlude` becomes active while destination and Hold input remain locked.
2. The overlay shows the stage’s first localized speaker/body and uses the Rain cue.
3. Continue hides the overlay exactly once, advances to artifact 5, refreshes previews, and unlocks input.
4. Dockets 2 and 3 show their corresponding beats; docket 4 goes directly to incident results.
5. Tutorial and free shifts never create/show an interlude.
6. Disabling or destroying the overlay while it owns a transition flushes the callback once and cannot strand `_inputLocked`.

Prefer existing reflection/UI lookup helpers in `GameAppPlayModeTests`; do not create a second test harness.

**Step 2: Build one hidden overlay with existing helpers**

Add fields:

```csharp
private GameObject _incidentDocketInterlude;
private NarrativeSequenceView _incidentDocketInterludeView;
```

Inside `BuildShiftScreen`, only for incident shifts, create a full-screen child after the ordinary shift UI so it renders above the desk:

- dim Plum surface with an incident cue image;
- senior portrait/art region using the existing portrait resolver;
- large display-font speaker and one-to-two-line body;
- one Amber Continue button;
- `CanvasGroup` or active state that fully blocks desk raycasts while visible;
- initial state inactive.

Name the root exactly `IncidentDocketInterlude` and the button `IncidentDocketInterludeContinueButton` for stable tests. Keep touch targets at least as large as the existing Continue button.

**Step 3: Route completion through a single continuation**

Split the current tail into:

```csharp
private void FinishCorrectTransition(SortOutcome outcome)
{
    if (outcome.DidCompleteDocket)
    {
        ResetDocketMistakePresentation();
        HideDocketWarmth();
    }

    if (outcome.DidCompleteShift)
    {
        FinishCompletedShift();
        return;
    }

    if (TryPlayIncidentDocketInterlude())
    {
        return;
    }

    ResumeShiftAfterCorrectTransition();
}
```

`TryPlayIncidentDocketInterlude()` returns false unless all are true:

- `_isIncidentShift`;
- `_incidentStage != null`;
- `_session.CompletedDockets` is 1, 2, or 3;
- a matching `IncidentDocketBeat` exists;
- the overlay and view are active/configured.

When true, activate the overlay and call:

```csharp
_incidentDocketInterludeView.Play(
    new[] { beat.Narrative },
    _localizer.Locale,
    VisualAssetLibrary.SeniorClerk,
    OwnTransition(CompleteIncidentDocketInterlude));
```

`CompleteIncidentDocketInterlude()` hides the overlay and calls `ResumeShiftAfterCorrectTransition()`. Add the interlude view to `FlushPendingTransitions()` before the fallback `CompleteOwnedTransition(version)` call. If it is inactive or missing, complete directly. Never save progress or mutate the shift from the view callback.

**Step 4: Korean test checkpoint**

After PlayMode tests pass, ask for one Korean stage-1 playtest:

- complete docket 1 and confirm the rain voice interrupts before artifact 5;
- confirm no desk button reacts beneath the overlay;
- Continue once and confirm artifact 5 appears;
- repeat through docket 3;
- finish docket 4 and confirm results appear without a fourth interlude.

**Step 5: Commit**

```powershell
git add Assets/Scripts/Runtime/Presentation/GameApp.cs Assets/Tests/PlayMode/GameAppPlayModeTests.cs
git commit -m "feat: show narrative beats between incident dockets"
```

## Task 5: Close five-stage progression, replay, and successor reveal

**Files:**

- Modify: `Assets/Tests/PlayMode/GameAppPlayModeTests.cs`
- Modify if a failing test requires it: `Assets/Scripts/Runtime/Presentation/GameApp.cs`
- Modify if a failing test requires it: `Assets/Scripts/Core/Incidents/IncidentProgressResolver.cs`

**Step 1: Replace obsolete single-stage expectations**

The old expectation that Rain becomes `AwaitingContent` after `rain-01-voices` is no longer valid. Replace it with:

- stage 1 result records `rain-01-voices` and menu CTA continues at stage 2;
- stage 4 result continues at stage 5;
- stage 5 records completion exactly once, marks Rain `Resolved`, sets `one-minute-ahead` as current `AwaitingContent`, and requests the existing successor reveal transition;
- Rain’s resolved card exposes replay while the teaser exposes no play action;
- replay does not duplicate completion rewards or incident records;
- a save containing stages 1–3 resumes at stage 4;
- an old save containing only stage 1 resumes at stage 2;
- language switching preserves stage and card state.

**Step 2: Implement only if tests expose a real gap**

Prefer current `IncidentRunner`, `IncidentProgressResolver`, completion recording, and board presenter. Do not add a Rain-specific progress branch. If a generic defect is found, fix it at the smallest shared boundary and add a domain regression test.

**Step 3: Commit**

```powershell
git add Assets/Tests/PlayMode/GameAppPlayModeTests.cs Assets/Scripts/Runtime/Presentation/GameApp.cs Assets/Scripts/Core/Incidents/IncidentProgressResolver.cs
git commit -m "test: cover full remembering rain progression"
```

Before committing, use `git diff --cached --name-only` and unstage any implementation file that did not actually change.

## Task 6: Final validation and handoff

**Files:**

- Modify: `Docs/AIAssetProvenance.md` only if runtime wording changed from this plan
- Modify: `Docs/ThirdPartyNotices.md` only if a new external element was introduced
- No other planned source changes

**Step 1: Static review**

Run read-only checks:

```powershell
git status --short
git diff --check
rg -n "TODO|TBD|PLACEHOLDER|coming soon" Assets/Scripts/Runtime/Content/Incidents Assets/Scripts/Runtime/Presentation/GameApp.cs Docs/superpowers/plans/2026-09-04-remembering-rain-full-incident.md
```

The literal UI concept is an awaiting clue, not the English phrase `coming soon`; no placeholder copy may ship. Confirm every new player-facing line has English and Korean.

**Step 2: Full Unity verification**

With Unity closed:

```powershell
.\scripts\test-unity.ps1
```

Then open only the worktree, run `Tools > Curio Clerk > Validate Project`, and confirm zero Console errors. Do not build Android for this narrative milestone unless the developer requests a device build.

**Step 3: Codex-operated acceptance playtest**

Play in Korean from Rain stage 1 through stage 5 using a development save or test route. Use rendered screenshots and explicit UI-state assertions for objective layout/input checks, and record the narrative evaluation separately:

- each interlude is noticed without explanation;
- each interlude is readable in at most one tap and does not feel like a modal chore;
- Hold is used deliberately at least once per shift and protects the named story object;
- the player can state after each stage what new fact was learned;
- after stage 5 the player understands that the voice is the previous clerk, the order was left for the next clerk, and the coordinates indicate the current desk;
- the `1분 앞선 저녁` title and clue create a clear inspection hook;
- no line truncates, no button is hidden by the portrait layout, and no input passes through the overlay.

Acceptance threshold: all seven checks pass, no blocker occurs, and at least four of five stage hooks are recalled without prompting. If comprehension fails, revise copy/pacing before adding more campaign incidents.

**Step 4: Final commit if validation documentation changed**

```powershell
git add Docs/AIAssetProvenance.md Docs/ThirdPartyNotices.md
git commit -m "docs: close remembering rain validation"
```

Skip this commit if those files did not change.

## Plan self-review

- Story coverage: all five Remembering Rain shifts, the previous-clerk reveal, first recall order, and current-desk address are represented.
- Gameplay coverage: every shift has 12 unique artifacts, 4/4/4 destinations, a protected Hold target, and no timer.
- Presentation coverage: only the first three completed dockets pause for story; the fourth enters results.
- Architecture coverage: immutable content owns authored beats, the existing narrative view renders them, and GameApp owns transition safety.
- Save coverage: old one-stage saves continue at stage 2; the zero-stage preview upgrades without migration.
- Scope coverage: no new art, audio, package, destination, trait, or generated scene edit.
- Localization coverage: every fixed player-facing line is paired in English and Korean.
- Provenance coverage: the plan is registered before generated text; runtime copy receives a separate entry before implementation.
- Verification coverage: TDD is required per behavior, authorized Unity execution is recorded, and the acceptance route measures comprehension rather than only button function.
- Placeholder scan: no implementation placeholder is authorized; the teaser is a deliberate `AwaitingContent` lifecycle with an authored clue.
