# Three-Seal Dockets core-loop redesign

Date: 2026-08-27
Product: **Curio Clerk: Night Shift / 기묘한 분실물 야간반**
Decision status: approved direction, pending written-spec review

## Purpose

현재 게임의 정적 분류를 60~90초짜리 순서 퍼즐로 바꾸되, 따뜻한 오컬트 분실물 보관소의 정체성, 12개 물건, 세 목적지, 한 손 세로 조작, 무제한 사고 시간, 오프라인 플레이를 유지한다.

이 설계의 성공 조건은 다음과 같다.

- 모든 정상 교대에서 Repair, Storage, Vault가 정확히 네 번씩 사용된다.
- Hold가 단순 연기가 아니라 적어도 한 번은 정답 순서를 만들기 위해 필요하다.
- 플레이어는 짧은 우선순위 규칙과 현재 장부 상태만으로 모든 결정을 설명할 수 있다.
- 생성된 교대는 한 칸 Hold로 반드시 완주할 수 있고, 운으로 막히지 않는다.
- 기존 유물 일러스트와 설명이 분류 직후의 반응에 연결된다.
- 최초 검증 범위에는 새 통화, 새 메타게임, 새 광고, 새 서버, 새 계정, 새 아트가 없다.

## Product constraints

- Unity `6000.3.21f1`을 유지한다.
- Android portrait, API 29 minimum, API 36 target, ARM64, IL2CPP를 유지한다.
- 한 교대는 물건 12개이며 플레이어에게 강제 시간 제한을 두지 않는다.
- 게임은 계정, 서버, 네트워크, 광고 가용성 없이 완전히 플레이 가능해야 한다.
- 영어와 한국어의 모든 새 플레이어 문구를 동시에 제공한다.
- Unity Editor, Hub, Unity batch 명령, 비공식 Unity MCP는 사용하지 않는다.
- 생성된 콘텐츠와 씬은 직접 수정하지 않고 `ProjectBuilder.BuildAll`의 원본 데이터와 빌더를 수정한다.
- 새 플레이어 문구나 생성 자산을 추가하기 전에 `Docs/AIAssetProvenance.md`와 `Docs/ThirdPartyNotices.md`를 갱신한다.

## Core decision

한 교대를 네 개의 **Three-Seal Docket / 삼중 봉인 장부**로 나눈다. 각 장부는 다음 세 도장을 정확히 한 번씩 받아야 닫힌다.

- Repair / 수리실
- Storage / 보관실
- Vault / 봉인고

물건의 정답 목적지는 기존 `RuleEngine`이 위에서 아래로 규칙을 평가하여 결정한다. 현재 장부에 정답 목적지 도장이 이미 있으면 그 물건은 틀린 것이 아니지만 지금은 처리할 수 없다. 플레이어는 Hold를 사용하여 다음 물건을 처리하고 빠진 목적지를 채운다.

세 개의 서로 다른 목적지가 모두 찍히면 장부 하나가 완성되고 빈 장부가 열린다. 네 번째 장부가 완성되면 교대가 끝난다.

## Player-visible rules

최초 검증 빌드의 규칙 묶음은 조건 두 개와 fallback 한 개만 사용한다.

예시 규칙 묶음 A:

1. `CURSED / 저주받음 → VAULT / 봉인고`
2. `FRAGILE / 깨지기 쉬움 → REPAIR / 수리실`
3. `OTHERWISE / 그 외 → STORAGE / 보관실`

예시 규칙 묶음 B:

1. `TEMPORAL / 시간성 → VAULT / 봉인고`
2. `WET / 젖어 있음 → REPAIR / 수리실`
3. `OTHERWISE / 그 외 → STORAGE / 보관실`

여러 조건이 맞으면 위 규칙이 우선한다. 규칙 길이를 늘리는 것으로 난이도를 올리지 않는다. 검증 단계의 난이도는 목적지 순서와 Hold 교환 횟수로 조절한다.

각 규칙 묶음은 현재 24개 유물을 분류했을 때 세 목적지마다 최소 네 개 후보를 제공해야 한다. 이 조건을 만족하지 않는 규칙 묶음은 콘텐츠 검증에서 거부한다.

## Docket state

현재 장부는 세 목적지의 사용 여부와 장부 내 실수 여부만 가진다. 교대 세션은 별도로 완료 장부 수와 표시용 pristine 연속 수를 가진다.

- `StampedDestinations`: 현재 장부에서 성공적으로 사용한 목적지 집합
- `IsPristine`: 현재 장부가 열린 뒤 진짜 오답이 없었는지 여부
- `CompletedDockets`: 세션에서 닫은 장부 수, 0~4
- `PristineDocketStreak`: 마지막으로 연속 완성한 pristine 장부 수. 오답 시 0으로 돌아가며 경제 보상에는 직접 곱하지 않는다.

정답 목적지가 아직 찍히지 않았다면 정답 분류는 다음 상태 변화를 만든다.

1. 목적지 도장을 추가한다.
2. 물건을 큐에서 제거한다.
3. 정답 수와 기본 점수·코인을 올린다.
4. 세 목적지가 모두 찍혔다면 장부 완성 보너스를 지급하고 새 빈 장부를 연다.
5. 네 번째 장부였다면 교대를 완료한다.

정답 목적지가 이미 찍혀 있다면 결과는 `Blocked`이다.

- 하트, 점수, 콤보, 큐, Hold, 장부는 변하지 않는다.
- 해당 목적지 버튼은 이미 비활성으로 보이므로 일반 탭으로는 이 상태가 발생하지 않아야 한다.
- 드래그나 경계 입력으로 시도된 경우 물건을 원위치시키고 Hold를 부드럽게 강조한다.

## Hold behavior

기존 한 칸 Hold 모델을 유지한다.

- Hold가 비어 있으면 현재 물건을 Hold에 놓고 다음 큐 물건을 가져온다.
- Hold에 물건이 있으면 현재 물건과 Held 물건을 교환한다.
- 성공적인 분류가 한 번 발생하기 전에는 Hold를 연속으로 다시 사용할 수 없다.
- Hold에는 점수·코인·하트 페널티가 없다.
- 큐가 끝났을 때 Held 물건이 남아 있으면 그 물건이 자동으로 현재 물건이 된다.

Hold는 자유로운 되감기가 아니라 한 번의 인접 순서 조정이다. 다음 두 물건 미리보기와 Held 표시를 계속 제공하여 판단 근거를 숨기지 않는다.

## Wrong-sort behavior

진짜 오답은 정답 목적지와 다른, 아직 열린 목적지를 선택한 경우다.

- 하트를 하나 잃는다.
- 현재 장부의 `IsPristine`이 false가 된다.
- 표시용 `PristineDocketStreak`이 0이 된다.
- 현재 물건, 큐, Held 물건, 목적지 도장은 변하지 않는다.
- 실제 정답과 그 근거가 된 첫 규칙을 강조한다.
- 짧은 피드백 입력 잠금 후 플레이어가 같은 물건을 다시 판단한다.

오답 물건을 자동으로 다음으로 넘기지 않는다. 그래야 한 번의 실수가 4/4/4 목적지 균형이나 장부 완주 가능성을 파괴하지 않고, 플레이어가 방금 배운 규칙을 즉시 적용할 수 있다.

하트가 0이 되면 기존과 같이 교대가 실패한다. 실패 결과는 완성한 장부와 미완성 장부를 함께 보여주며 동일한 seed로 재도전할 수 있다. 광고 부활과 코인 두 배는 핵심 재미 검증 화면에서 숨기되, 기존 서비스 코드는 삭제하지 않는다.

## Shift-plan generation

큐와 규칙을 독립적으로 무작위 생성하지 않는다. 새 `ShiftPlan`은 규칙과 12개 큐를 한 단위로 생성한다.

`ShiftPlanGenerator`는 Runtime content assembly를 참조하지 않는다. Runtime에서 제공한 규칙 묶음, 유물 목록, 목적지 템플릿을 입력받아 순수 Core 로직으로 계획을 만든다. 결정적 생성 절차는 다음과 같다.

1. seed와 진행 밴드에 맞는 검증된 규칙 묶음을 선택한다.
2. 24개 유물을 `RuleEngine`으로 분류하여 Repair, Storage, Vault 후보 버킷을 만든다.
3. 각 버킷에 서로 다른 유물이 네 개 이상 있는지 검증한다.
4. 각 버킷에서 서로 다른 유물 네 개를 seed 기반으로 선택한다.
5. 밴드에 맞는 검증된 12칸 목적지 순서 템플릿을 선택한다.
6. 선택한 유물을 해당 목적지 칸에 결정적으로 배치한다.
7. 완성 계획이 4/4/4이며 한 칸 Hold로 풀리고, 지정된 Hold 필요 횟수를 만족하는지 검증한다.

초기 구현은 범용 퍼즐 생성 알고리즘 대신 소수의 검증된 목적지 순서 템플릿을 사용한다. 예시 템플릿:

`V, V, R, S, R, S, V, S, S, R, V, R`

이 템플릿은 첫 장부에서 Hold를 요구하고 이후 Held 물건 교환을 다시 사용하게 한다. 목적지 템플릿과 실제 유물 선택은 분리되므로 새 유물은 기존 특성과 결과 문구만 갖추면 기존 템플릿에 자동으로 참여할 수 있다.

초기 밴드 목표:

- Tutorial: 수동 6개 큐, 장부 2개, 강제 Hold 1회와 큐 종료 Held 복귀
- Band 1: Hold 필수 국면 1회 이상
- Band 2~3: Hold 필수 또는 유리한 교환 2회 이상
- 더 높은 밴드는 최초 플레이테스트 통과 후 설계한다.

실행 시 검증에 실패한 계획을 조용히 보정하거나 재추첨하지 않는다. 콘텐츠·테스트 단계에서 모든 템플릿을 검증하고, 잘못된 계획은 명시적 오류로 거부한다.

## Reference 12-item shift

규칙 묶음 A와 다음 큐를 첫 검증 교대의 기준 사례로 사용한다.

| Queue | Artifact | Traits | Destination |
| ---: | --- | --- | --- |
| 1 | Whispering Key | Cursed, Metallic | Vault |
| 2 | Borrowed Shadow | Temporal, Cursed | Vault |
| 3 | Sleeping Teacup | Alive, Fragile | Repair |
| 4 | Clockwork Moth | Alive, Metallic | Storage |
| 5 | Moon-Mended Umbrella | Wet, Fragile | Repair |
| 6 | Jar of Tuesday Rain | Wet, Temporal | Storage |
| 7 | Porcelain Tooth | Cursed, Fragile | Vault; Cursed priority |
| 8 | Rusty Comet | Metallic, Temporal | Storage |
| 9 | Patient Compass | Metallic, Alive | Storage |
| 10 | Backward Candle | Temporal, Fragile | Repair |
| 11 | Tea-Stained Crown | Cursed, Metallic, Wet | Vault |
| 12 | Sundial Egg | Alive, Temporal, Fragile | Repair |

의도된 처리 흐름:

1. Docket 1: Key→V, Shadow는 Hold, Teacup→R, Moth→S.
2. Docket 2: Hold 교환으로 Shadow→V, Rain→S, Tooth가 중복 V이므로 Held Umbrella와 교환, Umbrella→R.
3. Docket 3: Comet→S, Compass가 중복 S이므로 Held Tooth와 교환, Tooth→V, Candle→R.
4. Docket 4: Crown→V, Egg→R, 큐 종료 후 Held Compass→S.

이 사례는 12개 고유 유물, 4/4/4 목적지, 우선순위 충돌, 빈 Hold, Hold 교환, 큐 종료 Held 복귀를 모두 검증한다.

## Scoring and rewards

숫자는 플레이테스트용 초기값이며 경제 확장의 근거가 아니다.

- 정답 분류: 100점, 5코인
- 장부 완성: 300점, 5코인
- 실수 없이 장부 완성: 추가 100점
- 네 장부 모두 실수 없이 완성: 추가 20코인
- Hold와 `Blocked`: 점수 변화 없음

최대 기본 결과는 2,800점과 100코인이다. 기존 장식 가격을 바꾸지 않고 먼저 획득 속도만 관찰한다. 점수는 일일 seed 비교와 자기 기록을 위한 값이며 강제 순위표나 서버 기능을 만들지 않는다.

기존 물건별 증가형 콤보는 화면의 주 피드백에서 제거한다. 대신 현재 장부 도장 0/3~3/3과 연속 pristine 장부 0/4~4/4를 보여준다.

## Feedback design

정답 직후 일반적인 `CORRECT` 문구보다 판정 이유를 먼저 보여준다.

- `CURSED took priority → VAULT / 저주받음 우선 → 봉인고`
- `FRAGILE → REPAIR / 깨지기 쉬움 → 수리실`
- `No special rule → STORAGE / 특수 규칙 없음 → 보관실`

목적지별 반응은 기존 유물 스프라이트와 UI 오버레이로 만든다.

- Repair: 금빛 수선선과 밝은 촉각 피드백
- Storage: 종이 이름표와 따뜻한 선반 음영
- Vault: 황동 봉인 문양과 낮고 짧은 촉각 피드백

각 유물에는 영어·한국어의 짧은 `resolution` 문장 하나를 추가한다. 문장은 특정 목적지에 종속되지 않고 유물이 안전하게 처리된 직후의 반응을 묘사한다. 매 정답 뒤 0.4~0.7초 동안 비차단형으로 표시하여 기존 설명과 그림이 결과에 연결되게 한다.

장부 완성 연출은 네 단계로 점층한다.

1. 첫 장부: 세 도장이 밀랍 원으로 합쳐지고 짧은 종소리
2. 둘째 장부: 책상 램프가 한 단계 밝아짐
3. 셋째 장부: 배경에 작은 오컬트 문양이 나타남
4. 넷째 장부: 완전한 봉인, 교대 완료 피드백, 교대를 끝낸 마지막 유물의 결과 문장

최초 구현은 새 그림과 새 음원을 요구하지 않는다. 기존 아이콘, 색, 패널, 스프라이트, 사운드·햅틱 경계를 재사용한다.

## Presentation

기존 portrait 화면과 하단 세 목적지 버튼을 유지한다.

- 규칙 목록 아래 또는 현재 카드 위에 Repair, Storage, Vault 세 도장의 현재 장부 상태를 한 줄로 표시한다.
- 현재 장부에서 사용된 목적지 버튼은 비활성화하고 도장 완료 표시를 겹친다.
- 정답 목적지가 닫혀 있는 물건이 현재일 때 Hold 버튼을 은은하게 강조한다. 정답 자체를 대신 알려주는 화살표나 자동 분류는 사용하지 않는다.
- 다음 두 물건과 Held 물건의 이름·그림을 계속 표시한다.
- HUD는 하트, 현재 장부 번호, 연속 pristine 장부를 우선 표시하고 코인은 보조 표시한다.
- 결과 화면은 숫자보다 네 장부의 완성·오염 상태와 대표 유물 resolution을 먼저 보여준다.

`GameApp`의 화면 생성 방식을 유지하되, 세 도장 표현과 상태 갱신은 작은 전용 presentation component로 분리한다. unrelated UI refactor는 하지 않는다.

## Tutorial

튜토리얼은 규칙 묶음 A와 수동 6개 큐로 장부 두 개를 완성한다.

목적지 패턴은 `V, V, R, S, S, R`이며 한 번의 Hold로 풀린다.

유물 순서는 Whispering Key, Borrowed Shadow, Sleeping Teacup, Clockwork Moth, Jar of Tuesday Rain, Moon-Mended Umbrella로 고정한다.

1. 첫 Vault 물건을 분류한다.
2. 다음 Vault 물건은 이미 찍힌 목적지이므로 Hold한다.
3. Repair와 Storage를 처리하여 첫 장부를 닫는다.
4. Storage와 Repair를 처리한다.
5. 큐 종료 후 Held Vault가 돌아오면 둘째 장부를 닫는다.

교육 중 진짜 오답은 하트를 깎지 않고 규칙을 다시 강조한다. 튜토리얼 문구와 하이라이트는 영어·한국어를 함께 제공한다.

## Architecture

### Core

`Assets/Scripts/Core`에는 Unity 참조 없이 다음 책임을 둔다.

- `ShiftPlan`: 규칙과 12개 유물 큐를 함께 보유하는 불변 계획
- `ShiftPlanGenerator`: 규칙 후보 분류, 4/4/4 선택, 템플릿 배치, seed 결정성
- `DocketState`: 세 목적지 도장, pristine 여부, 완료 판정
- `ShiftSession`: 현재 물건, Held 물건, 하트, 점수, 코인, 장부 진행과 입력 결과
- `RuleEngine`: 기존 첫 일치 규칙 판정 유지

`ShiftSession`의 분류 결과는 UI가 원인을 추측하지 않도록 최소한 다음 정보를 제공한다. 분류 자체의 결과와 장부·교대 완료 여부는 서로 겹칠 수 있으므로 분리한다.

- disposition: Correct, Wrong, Blocked
- did complete docket: true/false
- did complete shift: true/false
- selected destination
- expected destination
- matched rule id
- completed docket count
- score and reward deltas

### Runtime content

`Assets/Scripts/Runtime/Content`는 다음을 제공한다.

- 승인된 짧은 규칙 묶음
- 목적지 순서 템플릿과 밴드 연결
- 각 유물의 기존 bilingual name/description과 새 bilingual resolution

`Assets/Resources/Content`는 계속 `ProjectBuilder.BuildAll`이 생성한다.

### Presentation

`Assets/Scripts/Runtime/Presentation`는 다음만 담당한다.

- 현재 장부 도장 표시
- 목적지 버튼 활성·비활성 상태
- 판정 이유와 유물 resolution 표시
- 목적지별 정답 피드백
- 장부 완성 1~4단계 연출
- 튜토리얼 단계 안내

Core 상태를 presentation에서 다시 계산하지 않는다.

## Persistence and compatibility

최초 검증 구현은 저장 스키마를 올리지 않는다.

- 기존 `discoveredArtifactIds`를 계속 사용한다.
- 유물이 한 번이라도 올바르게 분류되면 기존과 같이 발견 처리한다.
- 발견된 유물의 Casebook은 같은 catalog의 bilingual resolution을 표시할 수 있으므로 별도 resolved-id 목록이 필요 없다.
- 기존 코인, 완료 교대 수, 언어, 사운드, 햅틱, 장식, 일일 기록을 보존한다.

기존 저장에서 이어서 시작한 플레이어도 새 규칙의 Band 1부터 시작한다. 완료 교대 수로 곧바로 미설계 고난도 밴드에 들어가지 않도록 검증 버전에서는 band를 지원 범위로 clamp한다.

## Testing strategy

AGENTS.md의 TDD 순서를 따른다. 각 행동 변경은 실패하는 EditMode 또는 PlayMode 테스트를 먼저 추가한다.

### EditMode contracts

- 세 목적지를 한 번씩 정답 처리하면 장부 하나가 완성된다.
- 현재 장부에서 중복 목적지는 `Blocked`이며 모든 상태가 그대로다.
- 오답은 하트와 pristine 상태만 변경하고 현재 물건과 큐를 유지한다.
- Hold 빈 슬롯, Hold 교환, 분류 전 연속 Hold 금지, 큐 종료 Held 복귀가 동작한다.
- 네 장부, 정답 12개에서만 교대가 완료된다.
- 세 번의 진짜 오답에서만 교대가 실패한다.
- 우선순위 충돌은 첫 규칙 목적지를 사용하고 matched rule id를 반환한다.
- 같은 seed는 같은 규칙과 같은 유물 큐를 만든다.
- 생성 계획은 항상 12개 고유 유물과 목적지 4/4/4를 가진다.
- 모든 목적지 템플릿은 한 칸 Hold로 완주 가능하고 목표 Hold 횟수를 만족한다.
- 모든 승인 규칙 묶음은 현재 catalog에서 목적지별 후보 네 개 이상을 가진다.
- 점수와 코인 최대값, pristine 보너스, 오답 비보상이 정확하다.

### PlayMode contracts

- 현재 장부 도장과 장부 번호가 분류 후 갱신된다.
- 이미 사용한 목적지 버튼은 비활성이고 Hold가 강조된다.
- 오답 뒤 같은 물건이 남고 올바른 규칙 줄이 강조된다.
- Hold 교환 후 현재·Held 그림과 다음 두 미리보기가 정확하다.
- 장부 완성과 교대 완료가 서로 다른 피드백을 발생시킨다.
- 영문·한국어 튜토리얼, 판정 이유, 결과 문구가 모두 존재한다.
- 기존 언어 변경, 저장, Casebook, 설정 이동이 유지된다.

Unity 테스트와 `ProjectBuilder.BuildAll`은 인간 개발자가 실행하고 결과를 제공한다. AI는 Unity Editor나 batch automation을 실행하지 않는다.

## Playtest gates

최초 검증은 8~12명의 새 플레이어를 목표로 하되, 확보 가능한 인원이 적으면 원시 인원과 결과를 그대로 기록하고 비율을 과장하지 않는다.

- 80% 이상이 튜토리얼 뒤 규칙 우선순위를 설명한다.
- 80% 이상이 첫 강제 Hold 상황에서 8초 안에 Hold를 사용한다.
- 정상 교대의 80% 이상이 강제 타이머 없이 60~90초에 끝난다.
- 첫 정식 교대 완료율은 60~85%, 무실수율은 15~40%를 목표로 한다.
- 70% 이상이 Hold를 순서를 풀기 위한 선택으로 설명한다.
- 10분 뒤 60% 이상이 유물 하나와 그 처리 반응을 기억한다.
- 50% 이상이 안내 없이 다음 교대를 자발적으로 시작한다.
- `운이 나빠 막혔다` 또는 `규칙이 함정이었다`는 응답이 15% 미만이다.

현재 버전과 새 버전을 모두 플레이할 수 있다면 가장 중요한 비교값은 설문 점수가 아니라 세 번째 교대 뒤의 자발적 추가 교대 선택률이다.

## Minimum implementation scope

첫 구현은 다음 범위로 제한한다.

1. Core의 장부 상태와 새 분류 결과
2. 결정적 `ShiftPlan` 생성과 검증된 목적지 템플릿
3. 규칙 묶음 두 개와 수동 6개 튜토리얼
4. 12개짜리 기준 교대 및 일반 seed 교대
5. 현재 장부의 세 도장 UI와 목적지 비활성 상태
6. Hold 필수·교환·큐 종료 복귀
7. 목적지별 기존 자산 기반 피드백과 장부 완성 연출
8. 24개 유물의 짧은 bilingual resolution
9. 결과 화면의 네 장부 상태와 대표 resolution
10. 위 동작을 고정하는 EditMode·PlayMode 테스트

## Explicitly out of scope

- 새 유물, 새 일러스트, 새 배경, 새 음악
- 범용 퍼즐 solver가 임의의 모든 목적지 순서를 고치는 기능
- 네 번째 이상 조건 규칙과 장문 예외 규칙
- 실시간 제한, 에너지, 강제 대기, 확률형 보상
- 서버, 계정, 클라우드 저장, 온라인 순위표
- 광고 SDK 변경, Firebase, 스토어 제출, 수익화 재설계
- IAP, 새 통화, 퀘스트, 업적, 배틀패스
- `GameApp` 전체 재작성이나 관련 없는 아키텍처 정리

## Implementation sequence after spec approval

1. Core 계약 테스트로 Docket과 분류 결과를 정의한다.
2. 최소 Core 상태 구현으로 테스트를 통과시킨다.
3. 생성 계획과 템플릿의 결정성·4/4/4·solvability 테스트를 작성한다.
4. 규칙 묶음과 resolution 콘텐츠를 추가하고 provenance/notices를 먼저 갱신한다.
5. PlayMode 테스트를 통해 장부 UI, Hold, 오답 유지, 결과 화면을 정의한다.
6. presentation과 builder/localization 원본을 최소 변경한다.
7. 정적 검증을 수행한 뒤 인간 개발자에게 Unity 테스트와 `ProjectBuilder.BuildAll` 실행을 요청한다.
8. 인간이 제공한 결과를 진단하고, 플레이테스트 빌드로 넘어가기 전에 회귀를 닫는다.

상세 파일별 작업 순서와 각 테스트의 red-green 단계는 이 명세 승인 후 별도 implementation plan에서 정의한다.
