# 다중 사건 보드와 `기억하는 비` 첫 교대 설계

작성일: 2026-09-03

상태: 구현 및 자동 검증 완료, 인간 플레이 확인 완료

제품: **Curio Clerk: Night Shift / 기묘한 분실물 야간반**

## 결정 요약

첫 사건 `녹지 않는 얼음`을 해결한 플레이어에게 실제 두 번째 사건 `기억하는 비`를 연다. 메인 메뉴는 현재 사건을 큰 주인공 카드로 보여주고, 해결한 사건은 작은 기록으로 남긴다. 두 번째 사건은 우선 물건 12개짜리 첫 교대 하나만 제공하며, 교대 종료 후 사건을 거짓으로 해결 처리하지 않고 `조사 진행 중 · 다음 교대 준비 중` 상태로 유지한다.

이 변경은 한 사건과 다섯 단계에 고정된 현재 진행 코드를 범용 사건 순서와 콘텐츠 경계로 바꾼다. 기존 저장, 첫 사건 다시보기, 자유 교대, 물건 24개, 삼중 봉인 장부, 한 칸 Hold, 로컬라이징, 아트와 피드백 시스템을 재사용한다.

## 목표

- 첫 사건 뒤에 명확한 장기 목표와 다음 미스터리를 제공한다.
- 메뉴를 단순한 모드 선택 화면이 아니라 사건 진행이 보이는 보관소 업무판으로 만든다.
- 두 번째 사건의 첫 교대만으로 새로운 이야기, 새로운 규칙 우선순위, 두 번의 필수 Hold 판단을 제공한다.
- 이후 사건과 교대를 추가할 때 `GameApp`이나 저장 형식을 사건별로 다시 고치지 않게 한다.
- Unity의 데이터·컴포넌트·애니메이션 기능을 표현 계층에 활용하면서 결정론적 판정은 순수 C#에 유지한다.
- 짧은 제작 범위 안에서 대화, 판단, 반응을 이전보다 크고 분명하게 만든다.

## 유지 조건

- Unity `6000.3.21f1`, Android 세로 화면, 한 손 조작을 유지한다.
- 한 교대는 물건 12개이며 강제 시간 제한이 없다.
- 수리실·보관실·봉인고를 정확히 네 번씩 사용한다.
- 계정, 서버, 에너지, 확률형 보상, 강제 광고를 추가하지 않는다.
- 게임은 완전히 오프라인으로 플레이할 수 있다.
- 모든 플레이어용 문구는 영어와 한국어를 동시에 제공한다.
- 생성되는 Resources 자산과 Scene은 직접 편집하지 않고 source catalog와 `ProjectBuilder.BuildAll`에서 관리한다.
- 새 물건 그림, 새 캐릭터, 새 음원이나 거대한 범용 프레임워크를 이번 범위에 추가하지 않는다.

## 검토한 기술 접근

### 1. 기존 단일 사건 코드를 조건문으로 확장

`GameApp`과 `ProgressionService`에 두 사건 ID를 직접 추가하고 첫 사건 완료 여부에 따라 분기한다.

- 장점: 구현량이 가장 작다.
- 단점: 세 번째 사건부터 다시 수정해야 하며, 현재의 기본 사건 ID와 최대 다섯 단계 하드코딩을 더 악화시킨다.
- 결정: 사용하지 않는다.

### 2. 사건 순서와 화면 상태를 분리하는 혼합형 구조

결정론적 사건·저장 판정은 일반 C#으로 두고, Unity 표현은 재사용 가능한 MonoBehaviour와 ScriptableObject 프로필로 분리한다.

- 장점: 현재 구조와 테스트 자산을 재사용하면서 사건 추가 비용과 저장 위험을 낮춘다.
- 단점: 단일 사건 가정을 제거하는 작은 구조 변경이 필요하다.
- 결정: **채택한다.**

### 3. 모든 사건 콘텐츠를 ScriptableObject로 전면 이전

사건, 단계, 대사, 규칙, 큐와 표현을 모두 Unity 자산으로 옮긴다.

- 장점: Inspector 저작 경험은 가장 Unity답다.
- 단점: 현재 code catalog와 검증 테스트를 크게 다시 만들고, 직렬화·마이그레이션·생성 자산 중복 위험이 크다.
- 결정: 이번 수직 검증판에는 과도하다. 반복 저작이 실제 병목으로 확인될 때 별도로 평가한다.

## 플레이어 흐름

### 첫 사건 미완료

- 중앙의 큰 현재 사건 카드는 `녹지 않는 얼음`이다.
- 현재 단계와 `사건 조사` 또는 `계속 조사` 행동을 표시한다.
- 두 번째 사건은 아직 선택하거나 미리 해결 상태로 볼 수 없다.
- 자유 교대와 설정은 계속 사용할 수 있다.

### 첫 사건을 방금 해결

1. `녹지 않는 얼음` 카드에 금빛 `사건 해결` 인장이 찍힌다.
2. 카드는 작아져 해결 기록 영역으로 이동한다.
3. 화면 조명이 잠깐 낮아지고 잔잔한 빗방울 파동이 번진다.
4. `기억하는 비` 카드가 중앙으로 올라온다.
5. 달빛으로 기운 우산과 사건 제목이 크게 나타난다.
6. `첫 조사 시작` 행동이 활성화된다.

사건 전환 연출은 저장이 성공한 뒤 표시하며, 애니메이션이 중단돼도 다음 실행에서 올바른 사건 보드가 나타난다.

### 두 번째 사건 첫 교대 완료

- `기억하는 비`는 해결 기록으로 이동하지 않는다.
- 카드에 첫 단서가 채워진다.
- 주 행동 영역은 비활성 버튼이 아니라 명시적 상태 문구 `다음 교대 준비 중`을 표시한다.
- 첫 사건 해결 기록을 눌러 다시보기할 수 있다.
- `사건 기록`, `자유 교대`, `설정`은 계속 사용할 수 있다.

## 사건 수명 주기

화면과 저장 판정은 다음 네 상태를 구분한다.

- `Locked`: 앞 사건이 아직 해결되지 않았다.
- `Available`: 플레이 가능한 미완료 교대가 있다.
- `AwaitingContent`: 현재 포함된 모든 교대를 완료했지만 사건이 아직 결말에 도달하지 않았다.
- `Resolved`: 결말이 있는 사건의 마지막 교대까지 완료했다.

`IncidentDefinition`에는 현재 포함된 마지막 단계가 사건 결말인지 나타내는 명시적 속성을 추가한다. 이름은 구현 계획에서 기존 명명 규칙에 맞추되 의미는 `CompletesWhenAllStagesCompleted`여야 한다.

- `녹지 않는 얼음`: `true`
- 첫 교대만 들어간 `기억하는 비`: `false`
- 향후 두 번째 교대를 추가하면 기존 저장은 자동으로 `AwaitingContent`에서 `Available`로 돌아온다.
- 진짜 마지막 교대가 추가되는 시점에만 이 속성을 `true`로 바꾼다.

단순히 현재 빌드에 포함된 단계 수를 모두 처리했다는 이유로 `completedIncidentIds`에 사건을 추가하면 안 된다.

## 런타임 책임 분리

### `IncidentDefinition`

- 사건 ID와 영어·한국어 제목
- 순서가 있는 `IncidentStageDefinition` 목록
- 현재 마지막 단계의 결말 여부
- 사건 카드가 사용할 논리적 lead artifact와 visual cue

Core에 Unity 타입을 유입하지 않는다. 색, Sprite, AnimationCurve 같은 Unity 표현 값은 이 정의에 넣지 않는다.

### `IncidentCatalog`

- 출시 순서가 있는 전체 사건 목록
- 사건 ID의 전역 유일성
- 모든 stage ID의 사건 간 전역 유일성
- 이전 사건 해결을 기본 해금 조건으로 사용

현재 `ContentCatalog.CreateIncidents()`의 결과를 권위 있는 순서로 사용할 수 있지만, 호출부는 더 이상 `.Single()`이나 특정 사건 ID를 가정하지 않는다.

### `IncidentProgressResolver`

저장 데이터와 사건 목록으로 다음 값을 계산하는 순수 C# 책임이다.

- 각 사건의 수명 주기 상태
- 현재 사건
- 다음 플레이 가능한 단계
- 해결 사건 목록
- 카드에 표시할 완료 단계 수와 첫 단서 상태

`GameApp`은 사건을 선택하거나 저장 값을 보정하는 규칙을 직접 소유하지 않는다.

### `IncidentBoardState`

화면이 필요한 읽기 전용 상태다.

- 현재 hero card의 사건 ID, 제목, lead artifact, 상태와 CTA
- 해결 기록 카드 목록
- 첫 단서와 `AwaitingContent` 안내
- 사건 기록, 자유 교대, 설정 행동의 가용 여부

표현 계층은 이 상태를 표시할 뿐 진행 규칙을 다시 계산하지 않는다.

### Unity 표현 컴포넌트

- `IncidentBoardView`: 현재 카드, 해결 기록과 보조 행동을 배치하고 상태를 바인딩한다.
- `IncidentCardView`: `Locked`, `Available`, `AwaitingContent`, `Resolved`를 같은 컴포넌트로 표현한다.
- `IncidentBoardTransitionView`: `CanvasGroup`, `RectTransform`, 코루틴과 `AnimationCurve`를 사용해 해결 인장, 축소 이동, 새 사건 등장 순서를 재생한다.
- `IncidentPresentationProfile`: 사건별 강조색, lead artifact ID, visual cue와 연출 강도를 보유하는 ScriptableObject다.

`IncidentPresentationProfile` 자산은 `ProjectBuilder.BuildAll`이 source catalog에서 결정론적으로 생성한다. 생성된 Resources 자산을 Inspector에서 직접 수정하지 않는다. 프로필이 없을 때는 기존 와인·황동 색과 lead artifact sprite를 사용하는 정적 fallback을 제공한다.

새 Animator Controller나 범용 타임라인 시스템을 만들지 않는다. 현재 프로젝트의 `ShiftFeedbackAnimator`, `NarrativeSequenceView`, `IncidentReactionView`, `CanvasGroup` 기반 코루틴 문법과 수명 주기 관리 방식을 재사용한다.

## 메인 메뉴 A안

세로 화면의 시각 우선순위는 다음과 같다.

1. 게임 제목과 야간반 표식
2. 화면 중심의 현재 사건 hero card
3. 큰 사건 행동 또는 명시적인 준비 중 상태
4. 작은 해결 사건 기록
5. 사건 기록과 자유 교대
6. 설정

현재 사건 카드에는 다음만 표시한다.

- 상태: `첫 조사`, `조사 진행 중`, `다음 교대 준비 중`
- 사건 제목
- lead artifact 그림
- 현재 단서 한 줄
- 가능한 경우에만 활성 CTA

해결 기록 카드는 제목, `사건 해결` 인장과 다시보기 행동만 갖는다. 해결한 사건과 현재 사건을 같은 크기의 버튼으로 나열하지 않는다.

`다음 교대 준비 중`은 눌리지 않는 버튼으로 만들지 않는다. 읽기 전용 상태 패널로 표시하고, 바로 아래에 사용 가능한 `자유 교대` 행동을 둔다.

## 사건 2: The Remembering Rain / 기억하는 비

### 핵심 미스터리

봉인된 우산 안에서 실내 비가 내린다. 빗방울은 물건 주인의 기억과 목소리를 반복하며, 그중 하나가 선임 관리인의 이름을 부른다. 첫 교대는 현상을 설명하지 않고 다음 사실만 남긴다.

> The voice inside the rain knows the senior clerk.
> 빗속의 목소리는 선임 관리인을 알고 있다.

### 첫 교대 대사

도입 1, `Concerned + Rain`:

> Since midnight, it has been raining inside the sealed umbrella. Do not open it.
> 자정부터 봉인된 우산 안에서 비가 내리고 있어요. 절대 열지 마세요.

도입 2, `Alert + Rain`:

> Every drop repeats someone's memory. One of them is saying my name.
> 빗방울마다 누군가의 기억을 되풀이합니다. 그중 하나가 제 이름을 부르고 있어요.

도입 3, `Neutral + InkSeal`:

> Seal what the rain has touched. Mend the fragile. Let the living rest. Read the ledger from the top.
> 비에 젖은 것은 봉인하고, 깨지기 쉬운 것은 수리하세요. 살아 있는 것은 쉬게 하세요. 규칙은 위에서부터 적용합니다.

종료 1, `Concerned + Rain`:

> The rain falls silent. One drop remains on the inside of the seal.
> 비가 멎습니다. 봉인 안쪽에 빗방울 하나만 남았습니다.

종료 2, `Alert + Rain`:

> It whispers, “You promised to come back.” The senior clerk does not answer.
> 빗방울이 속삭입니다. “돌아오겠다고 약속했잖아.” 선임 관리인은 대답하지 않습니다.

품질별 짧은 반응 초안:

- Stable / 안정: `The last drop shivers, but does not fall.` / `마지막 빗방울이 떨리지만 떨어지지 않는다.`
- Precise / 정교: `The rain gathers into one clear memory.` / `비가 하나의 선명한 기억으로 모인다.`
- Resonant / 공명: `The umbrella closes by itself, as if it recognizes your hands.` / `우산이 스스로 접힌다. 당신의 손길을 알아본 듯하다.`

메뉴 문구:

- `Investigation in progress / 조사 진행 중`
- `First clue / 첫 단서`
- `Next shift in preparation / 다음 교대 준비 중`
- `Case resolved / 사건 해결`
- `Replay case / 사건 다시보기`

문구는 구현 전 `TEXT-NARRATIVE-005` 출처 기록을 유지하고, Unity에서 한국어 줄바꿈과 영어 축약 여부를 검토한다.

## 첫 교대 규칙

위에서 아래로 첫 번째로 일치한 규칙을 적용한다.

1. `WET / 젖어 있음 → VAULT / 봉인고`
2. `FRAGILE / 깨지기 쉬움 → REPAIR / 수리실`
3. `ALIVE / 살아 있음 → STORAGE / 보관실`
4. `OTHERWISE / 그 외 → STORAGE / 보관실`

이 우선순위 때문에 `달빛으로 기운 우산`과 `종이 물고기`는 깨지기 쉬워도 비에 젖은 흔적을 먼저 봉인해야 한다. 규칙 수를 늘리지 않고, 기존 특성의 충돌이 의미 있는 판단을 만든다.

## 물건 12개와 결과

추가 특성 없이 기존 catalog 특성을 그대로 사용한다.

| 순서 | ID | 물건 | 관련 특성 | 목적지 |
| ---: | --- | --- | --- | --- |
| 1 | `moon-umbrella` | Moon-Mended Umbrella / 달빛으로 기운 우산 | Wet, Fragile | Vault / 봉인고 |
| 2 | `paper-fish` | Paper Fish / 종이 물고기 | Alive, Wet, Fragile | Vault / 봉인고 |
| 3 | `sleeping-teacup` | Sleeping Teacup / 잠든 찻잔 | Alive, Fragile | Repair / 수리실 |
| 4 | `clockwork-moth` | Clockwork Moth / 태엽 나방 | Alive, Metallic | Storage / 보관실 |
| 5 | `backward-candle` | Backward Candle / 거꾸로 타는 양초 | Temporal, Fragile | Repair / 수리실 |
| 6 | `patient-compass` | Patient Compass / 참을성 많은 나침반 | Metallic, Alive | Storage / 보관실 |
| 7 | `rain-jar` | Jar of Tuesday Rain / 화요일 빗물병 | Wet, Temporal | Vault / 봉인고 |
| 8 | `porcelain-tooth` | Porcelain Tooth / 도자기 이빨 | Fragile, Cursed | Repair / 수리실 |
| 9 | `humming-scarf` | Humming Scarf / 콧노래 목도리 | Alive, Cursed | Storage / 보관실 |
| 10 | `yesterday-ticket` | Yesterday Ticket / 어제행 승차권 | Temporal, Fragile | Repair / 수리실 |
| 11 | `murmur-box` | Murmur Box / 웅얼거림 상자 | Cursed, Alive, Metallic | Storage / 보관실 |
| 12 | `ink-snowglobe` | Ink Snow Globe / 잉크 스노글로브 | Wet, Cursed, Fragile | Vault / 봉인고 |

목적지 패턴은 `V V R S R S V R S R S V`이며 수리실·보관실·봉인고가 각각 네 번이다. `DocketSequenceAnalyzer` 기준 최소 Hold 횟수는 정확히 2회여야 한다.

### 의도된 Hold 경로

1. 우산을 봉인고로 보낸다. 같은 장부에 봉인고 인장이 생긴다.
2. 종이 물고기도 봉인고가 정답이므로 지금은 처리할 수 없다. 첫 번째 Hold로 보호한다.
3. 잠든 찻잔을 수리실, 태엽 나방을 보관실로 보내 첫 장부를 닫는다.
4. 양초→수리실, 나침반→보관실, 빗물병→봉인고로 두 번째 장부를 닫는다.
5. 도자기 이빨→수리실, 목도리→보관실까지 세 번째 장부에 찍는다.
6. 어제행 승차권도 수리실이므로 현재 장부의 수리실 인장과 충돌한다. 두 번째 Hold로 종이 물고기와 교환한다.
7. 종이 물고기→봉인고로 세 번째 장부를 닫는다.
8. 웅얼거림 상자→보관실, 잉크 스노글로브→봉인고를 처리한다.
9. 큐가 끝나면 Held의 어제행 승차권이 돌아오고 수리실로 보내 네 번째 장부를 닫는다.

첫 Hold는 비에 젖은 종이 물고기를 안전하게 보호하는 이야기 행동이고, 두 번째 Hold는 오래 보관한 물건을 적절한 순간에 꺼내 장부를 완성하는 숙련 판단이다.

## 저장 호환성과 마이그레이션

기존 필드는 삭제하지 않는다.

- `activeIncidentId`
- `activeIncidentStage`
- `incidentStageRecords`
- `completedIncidentIds`

새 resolver는 stage ID 기록과 현재 catalog를 우선 사용해 진행을 계산한다. 사건마다 다음 규칙을 적용한다.

1. 알려진 stage ID의 기록을 해당 사건에 연결한다.
2. 완료된 stage의 연속 prefix 다음을 현재 단계로 선택한다.
3. `completedIncidentIds`에 있는 결말형 사건은 해결 상태로 인정한다.
4. 구버전 저장에서 stage 기록이 불완전하면 같은 `activeIncidentId`의 `activeIncidentStage`를 안전한 범위의 fallback으로만 사용한다.
5. 알 수 없는 사건 ID나 범위를 벗어난 단계는 catalog의 가장 가까운 유효 진행 경계로 복구한다.
6. 다른 저장 데이터, 최고 품질 기록, 발견 물건, 코인과 설정은 삭제하지 않는다.

기존의 `DefaultIncidentId = "unmelting-ice"`와 `Math.Min(5, ...)` 같은 사건별 하드코딩은 제거한다. 단계 수는 해당 `IncidentDefinition`에서 얻는다.

두 번째 사건 첫 교대를 마쳤을 때 stage record는 저장하지만 `completedIncidentIds`에는 추가하지 않는다. 이후 새 stage ID가 catalog 뒤에 추가되면 resolver가 첫 미완료 단계를 자동 선택한다. 저장 버전 전체를 초기화하거나 완료 기록을 위조하지 않는다.

다시보기는 별도 임시 runner로 stage 0부터 실행하며 정규 진행 위치와 해결 상태를 되돌리지 않는다.

## 시각·상호작용 피드백

새 그림 없이 다음 기존 자원을 재사용한다.

- `moon-umbrella` sprite
- 작업대 배경과 와인·황동·종이 색 체계
- `IncidentVisualCue.Rain`
- `NarrativeSequenceView`의 큰 초상·대사 전환
- `IncidentReactionView`와 `ShiftFeedbackAnimator`의 화면 강조
- 기존 절차형 피드백 음향과 선택적 햅틱 경계

사건 카드 전환은 1.2~1.8초 안에 끝나며 입력을 오래 막지 않는다. 일반 화면 흔들림, 네온 섬광, 색종이, 게임쇼식 보상은 사용하지 않는다. 빗방울 파동, 어두워지는 촛불, 황동 인장과 달빛 반사처럼 세계 안의 재료로 화려함을 만든다.

애니메이션과 소리 callback은 진행 판정의 조건이 아니다. 컴포넌트가 없거나 비활성화돼도 정적 카드와 텍스트, CTA는 정상 동작해야 한다.

## 오류 처리와 콘텐츠 검증

`ContentValidator`는 다음을 거부한다.

- 중복 incident ID
- 사건 사이에서 중복되는 stage ID
- 영어 또는 한국어 필수 문구 누락
- lead artifact 누락
- 12개가 아니거나 중복 물건이 있는 큐
- 세 목적지가 4/4/4가 아닌 교대
- `DocketSequenceAnalyzer` 결과가 2가 아닌 첫 교대
- 한 칸 Hold로 풀 수 없는 교대
- 결말형 사건인데 완료 가능한 마지막 stage가 없는 정의

발전 단계의 잘못된 콘텐츠는 Editor 검증에서 실패시킨다. 이미 배포된 player에서 표현 profile만 빠진 경우에는 기본 카드 표현으로 계속 진행한다. 필수 사건 정의가 손상된 경우 저장을 덮어쓰지 않고 자유 교대로 돌아갈 수 있는 읽기 가능한 오류 상태를 제공한다.

## 테스트 전략

행동 변경은 저장소 원칙대로 실패하는 테스트를 먼저 작성한다. 구현 완료 후 한 번의 전체 Unity 테스트와 한 번의 인간 시각 확인으로 묶는다.

### EditMode

- 첫 사건 미완료·완료·두 번째 사건 대기 상태를 resolver가 정확히 계산한다.
- 첫 사건 완료 기존 저장이 두 번째 사건 stage 0을 연다.
- 두 번째 사건 첫 stage 완료가 사건 해결로 기록되지 않는다.
- 대기 저장에 stage 2 정의를 추가하면 stage 1부터 재개한다.
- 알 수 없는 사건과 범위 밖 legacy stage가 데이터 손실 없이 복구된다.
- 사건·stage ID 전역 유일성과 양언어 문구를 검증한다.
- 첫 교대가 12개 고유 물건, 4/4/4 목적지와 최소 Hold 2회를 만족한다.

### PlayMode

- 첫 사건 완료 메뉴가 해결 기록과 현재 `기억하는 비` 카드를 동시에 보여준다.
- 현재 카드 CTA가 첫 교대를 시작한다.
- 첫 교대 완료 후 `다음 교대 준비 중`이 비상호작용 상태로 보인다.
- 자유 교대, 설정과 첫 사건 다시보기가 계속 작동한다.
- 카드 전환 컴포넌트가 비활성화돼도 즉시 정적 최종 상태가 보인다.
- 한국어와 영어에서 제목, 단서와 상태 문구가 잘리지 않는 기준 레이아웃을 사용한다.

### 인간 확인 한 번

구현과 자동 테스트 완료 뒤 개발자가 다음만 확인한다.

1. `Tools > Curio Clerk > Generate Project Assets`
2. 메인 메뉴에서 첫 사건 해결 기록과 `기억하는 비` hero card 확인
3. 한국어로 첫 교대를 진행하며 두 번의 Hold 필요성과 Rain 연출 확인
4. 완료 후 첫 단서와 `다음 교대 준비 중`, 자유 교대와 다시보기 확인

문구나 작은 애니메이션 조정마다 전체 테스트를 반복 요청하지 않는다. Core·저장·콘텐츠 계약이 바뀌는 묶음이 끝났을 때 검증한다.

## 이번 구현 범위

포함:

- 범용 다중 사건 선택과 상태 계산
- 기존 저장 호환
- 메인 메뉴 A안
- `기억하는 비` 첫 교대 12개
- 영어·한국어 대사와 첫 단서
- 기존 Unity 표현 시스템을 재사용한 사건 전환과 Rain cue
- 검증과 자동 테스트

제외:

- `기억하는 비`의 두 번째 이후 교대
- 세 번째 사건
- 새 물건, 새 특성, 새 목적지나 새 입력
- 대형 사건 선택 지도나 분기형 비주얼노벨
- 새 일러스트, 초상, 음원 또는 외부 패키지
- 광고, Firebase, 스토어 출시와 수익화 작업
- 통화·장식품·Casebook의 대규모 재설계

## 완료 기준

- 기존 첫 사건 저장과 다시보기가 유지된다.
- 첫 사건을 해결하면 두 번째 사건이 명확히 현재 목표가 된다.
- 두 번째 사건 첫 교대에서 세 목적지를 각각 네 번 사용하고 Hold를 정확히 두 번 이상 판단해야 한다.
- 교대의 Wet 우선순위와 마지막 목소리로 새로운 미스터리가 이해된다.
- 첫 교대 후 사건이 해결됐다고 거짓 표시하지 않는다.
- 나중에 stage를 추가하면 기존 저장이 새 콘텐츠부터 이어진다.
- 메인 메뉴가 현재 사건, 해결 기록, 준비 중 상태를 한눈에 구분한다.
- 연출을 꺼도 게임과 저장이 정상 진행한다.
- 영어·한국어 콘텐츠 검증과 관련 EditMode·PlayMode 테스트가 통과한다.
