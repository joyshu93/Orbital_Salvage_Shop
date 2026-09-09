# Curio Clerk 내부 플레이테스트 안내 / Internal playtest guide

이 문서는 처음 보는 사람이 첫 사건과 Remembering Rain을 플레이하도록 준비한 안내입니다. 아직 사람 테스트 결과는 없습니다. 정확한 APK, 소스 커밋, SHA-256, 검증 범위는 [통합 QA 보고서](../QA/InternalPlaytestIntegration-20260909.md)를 확인하세요. 개발 빌드이며 스토어 배포본이 아닙니다.

This guide prepares a first-time player to try the first investigation and Remembering Rain. No human test results have been collected. See the [integration QA report](../QA/InternalPlaytestIntegration-20260909.md) for the exact APK, source commit, SHA-256, and validation limits. This is a development build, not a store release.

## 준비와 설치 / Preparation and installation

- Android 10/API 29 이상, ARM64 앱 실행이 가능한 세로 화면 기기를 사용합니다. PC만 있으면 담당자가 준비한 전용 Android 에뮬레이터를 사용합니다. 실제 기기 성능은 아직 검증하지 않았습니다.
- 기존 게임 저장이 없는 전용 기기나 새 Android 사용자 프로필을 권장합니다. 이전 설치가 있다면 담당자에게 알려 주세요. 앱을 제거하거나 데이터를 지우지 마세요. 업데이트는 저장을 유지하며, 기존 진행은 첫 실행 관찰과 구분해 기록합니다.
- 담당자는 QA 보고서의 APK 파일을 로컬로 전달하고 해시를 대조합니다. USB 설치 시 정확한 테스트 기기를 선택한 뒤 `adb -s <테스트_기기> install -r "<APK_경로>"`를 실행할 수 있습니다. 휴대전화 파일 앱으로 설치한다면 Android가 요구하는 해당 앱의 설치 권한만 허용합니다. 개발자 서명 변경 오류가 나면 중단하고 담당자에게 알립니다. 기존 앱을 삭제해 해결하지 않습니다.
- 앱 이름은 **Curio Clerk: Night Shift**입니다. 화면을 세로로 두고 실행합니다. 계정 로그인이나 광고 시청 없이 플레이합니다. 오프라인으로 시작해도 됩니다. 메뉴의 Settings/설정에서 English/한국어를 고릅니다.

- Use a portrait Android device running Android 10/API 29 or later that supports ARM64 apps. A prepared dedicated Android emulator is also suitable. Physical-device performance has not yet been validated.
- Prefer a dedicated device or a new Android user profile without an existing game save. Tell the facilitator if the game is already installed. Do not uninstall it or clear its data. Updates preserve saves; record prior progress separately from first-run observations.
- The facilitator provides the local APK named in the QA report and verifies its hash. For USB installation, select the exact test device and run `adb -s <test_device> install -r "<APK_path>"`. For installation from the phone's file app, allow only the installation permission Android requests for that app. Stop and report a signing conflict; do not remove the existing app to work around it.
- Launch **Curio Clerk: Night Shift** in portrait orientation. No account or ad viewing is required. Starting offline is fine. Select English or 한국어 from Settings.

## 플레이 순서 / Play session

먼저 읽을 것은 이 순서까지입니다. 관찰표와 질문은 진행자용이며, 플레이어에게 정답이나 물건별 목적지를 미리 설명하지 않습니다. 30~50분을 확보하되 제한 시간은 없습니다. 원하면 언제든 쉬거나 끝내도 됩니다.

The player only needs to read through this sequence before starting. The observations and questions below are for the facilitator; do not explain item destinations or solutions in advance. Set aside roughly 30–50 minutes, without a time limit. The player may pause or stop whenever they wish.

1. 메뉴의 **Begin First Investigation / 첫 조사 시작** 버튼을 선택합니다. 첫 사건 **The Unmelting Ice / 녹지 않는 얼음**을 시작해 화면 안내를 읽고 진행합니다.
2. 생각한 이유와 망설인 순간을 편하게 말해 주세요. 잘해야 하는 시험이 아닙니다. 막히면 원하는 만큼 살펴보고, 도움이 필요하면 요청하세요.
3. 첫 사건의 다섯 교대와 결말을 본 뒤 사건 보드로 돌아옵니다. 다음 사건 **The Remembering Rain / 기억하는 비**를 시작해 가능한 곳까지 진행합니다. 권장 범위는 두 사건의 결말까지지만 중도 종료도 유효한 기록입니다.
4. 자연스러운 플레이를 마친 뒤에만 추가 확인을 합니다. 한 번 일부러 다른 목적지를 선택하고 반응을 봅니다. 실패했다면 화면의 같은 교대 재시도 버튼을 사용합니다. 한 교대 완료 후 앱을 닫았다 다시 열어 진행을 확인합니다. 여유가 있으면 언어를 바꾸고 사건을 다시 봅니다.

1. Choose the investigation's **Begin First Investigation** button and start **The Unmelting Ice**. Read the in-game guidance and proceed.
2. Say what you are considering and where you hesitate, if comfortable. This is a test of the game, not your ability. Explore at your own pace and request help if needed.
3. After the first case's five shifts and ending, return to the incident board. Start **The Remembering Rain** and continue as far as you wish. Both endings are the recommended scope; stopping earlier is still a useful observation.
4. Only after the natural play session, try optional checks: intentionally select a different desk once and observe the response; use the same-shift retry button if a shift fails; close and reopen the app after completing a shift; if time permits, change language and replay a case.

진행은 **완료한 교대 단위**로 저장됩니다. 진행 중인 교대에서 앱 프로세스가 종료되면 그 교대의 시작부터 다시 합니다. 앱 전환으로 잠깐 나갔다 돌아오는 것과 프로세스 종료는 구분합니다. 같은 교대 재시도에는 광고나 대기가 필요 없습니다. 메뉴의 QA · Ads / Consent는 이번 사람 플레이테스트의 진행 경로에 포함하지 않습니다.

Progress is saved at **completed-shift checkpoints**. If the app process ends during a shift, that shift starts again from the beginning. Briefly switching apps is different from terminating the process. Retrying a failed shift requires no ad or wait. The menu's QA · Ads / Consent screen is outside this human gameplay session.

## 진행자 관찰 / Facilitator observations

설명을 덧붙이기 전에 행동과 플레이어의 말을 먼저 기록합니다. 정답을 알려준 시점부터 해당 과제는 “도움 없이 완료”로 세지 않습니다. 첫 자연 플레이와 의도적인 오답·재시도 검증을 구분합니다. 요청을 받으면 도움을 제공하되, 그 내용을 기록합니다. 화면 녹화는 선택 사항이며 플레이어가 동의한 경우에만 게임 화면을 기록합니다.

Record behavior and the player's own words before adding explanations. Once a solution is supplied, do not count that task as unassisted. Separate natural play from intentional error/retry checks. Provide help when requested and record what was explained. Recording is optional and should capture only the game, with the player's agreement.

| 관찰 / Observation | 기록할 사실 / Facts to record |
|---|---|
| 시작과 역할 / Start and role | 첫 행동, 자신의 역할을 설명한 말 / First action and the player's description of their role |
| 규칙과 장부 / Rules and docket | 첫 장부 완료 여부, 규칙을 보는 순서 / First docket completion and the order in which rules are consulted |
| Hold / Hold | 처음 누른 시점, 기대한 결과와 실제 결과에 대한 말 / First use, expected result, and response to the actual result |
| 입력 반응 / Input response | 반복 탭, 입력을 기다리는 순간, 오답 뒤 행동 / Repeated taps, waiting, and actions after an error |
| 읽기 / Reading | 다시 읽거나 가까이 본 문구와 화면 위치 / Text reread or examined closely, and its location |
| 흐름 / Pacing | 자발적 다음 교대 선택, 쉬거나 그만두려 한 지점 / Voluntary continuation and points where the player wanted a break or to stop |
| 사건 전환 / Case transition | 결말 뒤 다음 행동, 다음 사건을 찾은 방법 / Action after the ending and how the next case was found |
| 저장·재시도 / Saves and retries | 기대한 재개 지점, 실제 상태, 도움 필요 여부 / Expected resume point, actual state, and help needed |

## 답을 유도하지 않는 질문 / Neutral questions

진행을 방해하지 않도록 질문은 교대가 끝났거나 플레이어가 멈춘 뒤에 합니다. 질문에 예시 정답을 붙이지 않습니다.

Ask at a shift boundary or after the player stops. Do not attach examples of desired answers.

1. 이 게임에서 무엇을 하고 있다고 생각했나요? / What did you think your job was in this game?
2. 방금 물건을 어디로 보낼지 어떻게 결정했나요? / How did you decide where to send that curio?
3. Hold를 누르기 전에 무엇이 일어날 거라고 예상했나요? 실제로는 어땠나요? / What did you expect Hold to do? What happened?
4. 예상과 다르게 반응한 순간이 있었나요? 그때 무엇을 했나요? / Was there a moment when the game responded differently from what you expected? What did you do?
5. 읽거나 이해하는 데 시간이 걸린 부분이 있었나요? 어디였나요? / Did anything take time to read or understand? Where?
6. 계속하고 싶거나 멈추고 싶었던 순간은 언제였나요? / When did you want to continue, pause, or stop?
7. 가장 기억에 남은 물건이나 장면은 무엇인가요? 이유는 무엇인가요? / Which curio or scene do you remember most, and why?
8. 결말을 본 뒤 어떤 일이 이어질 것으로 생각했나요? / What did you expect to happen after the ending?
9. 다시 한다면 무엇을 다르게 해 보고 싶나요? / If you played again, what would you try differently?
10. 덧붙이고 싶은 것이 있나요? / Is there anything else you would like to add?

## 익명 기록 양식 / Anonymous record

이름, 계정, 연락처, 기기 일련번호, 광고 식별자는 적지 않습니다. 기록 ID는 P01처럼 임의로 붙이며 실제 신원과 연결하는 목록을 만들지 않습니다. 사람 응답이 들어오기 전에는 빈칸을 0명 통과로 집계하지 않습니다.

Do not record names, accounts, contact details, device serials, or advertising identifiers. Use an arbitrary ID such as P01, with no identity lookup list. Blank responses are uncollected data, not zero passing participants.

```text
기록 ID / Record ID:
빌드 SHA-256 또는 QA 보고서 버전 / Build SHA-256 or QA report revision:
언어 / Language:
기기 종류·Android 버전·화면 비율 / Device class, Android version, aspect ratio:
처음 플레이인지, 기존 진행 여부 / First play? Existing progress?
오프라인/온라인 / Offline or online:
실제 플레이 시간(휴식 제외) / Play duration excluding breaks:
완료 지점 / Furthest point reached:

시각·사건·교대 / Time, case, shift:
플레이어 행동·원문 발언 / Action and verbatim player comment:
예상한 결과 / Expected result:
실제 결과 / Actual result:
진행자 도움과 제공 시점 / Help supplied and when:
자연 플레이/의도적 검증 / Natural play or intentional check:
재현 여부·반복 횟수 / Reproduction and attempt count:
게임 화면 증거 위치(선택) / Game-only evidence location (optional):

질문 1~10 응답 / Answers 1–10:
도움 없이 완료/도움 후 완료/미완료/미관찰 / Unassisted, assisted, incomplete, or unobserved:
관찰자 해석(사실과 분리) / Observer interpretation, separate from facts:
후속 확인할 항목 / Follow-up to check:
```

사람 모집, 메시지 발송, 응답 수집은 이 준비 작업에서 수행하지 않았습니다. 실제 사람 테스트 이후에만 이해도·재미와 다음 개선 우선순위를 판단합니다.

No recruitment, messages, or response collection were performed during preparation. Assess comprehension, enjoyment, and improvement priorities only after actual human sessions.
