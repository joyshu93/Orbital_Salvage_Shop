# Curio Clerk 직접 조사 플레이테스트 / Hands-on case playtest

**준비 상태: Candidate 2 기술 QA와 사용자용 설치·한국어 시작 화면 확인 완료.** EditMode 226/226, PlayMode 127/127, 조건별 컴파일 15/15를 통과했습니다. 최종 APK에서 영어 열 장면·28개 행동과 한국어 첫 장면, 저장·다시보기를 확인했습니다. 별도 사용자 환경에 최종 APK를 설치하고 한국어 **첫 출근** 화면에서 게임 입력을 멈췄습니다. 이는 에이전트 검증이며 사람의 재미 평가는 아직 수집하지 않았습니다.

**Preparation status: Candidate 2 technical QA, personal installation and Korean starting-screen checks are complete.** EditMode passed 226/226, PlayMode 127/127 and the compile matrix 15/15. The final APK was exercised through all ten English scenes and 28 actions, the first Korean scene, saves and replay. It was installed in a separate player environment and left at **첫 출근 / Start your first night** with no case started. These are agent checks; no human enjoyment assessment has been collected.

| 인계 항목 / Handoff item | 상태 / Value |
| --- | --- |
| 검증한 APK / Verified APK | `C:\Users\D-\Documents\Codex_Project\Orbital_Salvage_Shop\.worktrees\immersive-cases-20260911\Builds\Android\CurioClerk-qa.apk` · 105,247,879 bytes |
| 소스 커밋 / Source commit | `5f3857be83b38f683ebd8e01de387b3834505fc2` — 기록한 빌드 입력과 동일성 확인 / Matched to the recorded build inputs |
| APK SHA-256 | `5D8DF9FA112C539ED66685A6ADA395CBC23F18C641AD5530421B6783A0039831` |
| QA 기록 / QA record | [후보별 검증 현황 / Candidate validation record](../QA/HandsOnCases-20260911.md) · [개선 PR #6 / Improvement PR #6](https://github.com/joyshu93/Orbital_Salvage_Shop/pull/6) |
| QA 설치 환경 / QA installation | `CurioClerk_WorkbenchQA_20260911`, Android user 0 — 업데이트 후 기존 저장 유지 확인 / Existing save preserved after update |
| 사용자 플레이 환경 / Player environment | `CurioClerk_HandsOnPlay_20260911:5598` — 최종 APK 해시 일치, 한국어 첫 출근, 진행 기록 0개 / Matching final APK hash, Korean starting screen, no progress records |
| 사람 평가 / Human evaluation | 이 새 버전의 사람 평가 아직 수집하지 않음 / No human evaluation of this new version collected |

인계 확인은 2026-09-11 05:59:52 UTC 기준입니다. 새 에뮬레이터 창에서 **첫 출근**으로 시작합니다. 마지막 Windows 창 선택 중 Esc로 화면 제어가 중단되어 추가 조작을 멈췄습니다. Android 게임 화면은 확인했으나 Windows 최상단 표시 확인은 마치지 않았습니다. 이후 사용자가 진행한 상태는 이 기록에 포함하지 않습니다.

The handoff check was recorded at 2026-09-11 05:59:52 UTC. Choose **첫 출근 / Start your first night** in the new emulator window. Esc stopped Computer Use during final Windows window selection, and no further control was performed. The Android game screen was verified; Windows foreground placement was not. Later player progress is outside this record.

## 플레이어에게 먼저 보여줄 안내 / Read this first

보관소에 들어온 물건을 살펴보는 게임입니다. 메뉴의 **첫 출근 / Start your first night**을 눌러 시작해 주세요. 화면 안내를 보고 편하게 진행하면 됩니다. 잘해야 하는 시험이 아니며, 원하는 때 쉬거나 그만두어도 됩니다. 막히거나 무엇을 해야 할지 모르겠다면 그렇게 말해 주세요.

This game takes place in a lost-property office. Choose **Start your first night** and follow the on-screen guidance. This is a test of the game, not your ability. You may pause or stop whenever you wish. Say when you are unsure what to do or why.

물건별 정답, 도구를 쓰는 순서, 뒤에 나올 사연은 미리 설명하지 않습니다. 아래 조작 도움은 플레이어가 요청하거나 화면 안내만으로 조작을 시작하지 못했을 때 제공합니다.

Do not explain item solutions, tool order or later story events in advance. Offer the controls below when the player requests help or cannot begin using the on-screen instructions.

## 준비와 설치 / Preparation and installation

1. 담당자가 이번 작업의 최종 APK와 해시를 확인합니다. 이 파일은 개발용 APK이며 스토어 출시본이 아닙니다. 최종 검증이 끝나기 전에는 “설치 완료”, “문제없이 플레이 가능”으로 기록하지 않습니다.
2. Android 10/API 29 이상에서 ARM64 앱을 실행할 수 있는 세로 화면 환경을 사용합니다. PC에서는 이 작업을 위해 준비한 전용 Android 에뮬레이터를 사용할 수 있습니다. 에뮬레이터 실행만으로 실제 휴대전화 성능·발열·음향·진동을 검증했다고 보지 않습니다.
3. 기존 게임 저장이 없는 전용 기기나 별도 Android 사용자 프로필을 준비합니다. 기존 진행이 있다면 그대로 보존하고 “재방문 플레이”로 기록합니다. 첫 실행을 만들려고 앱을 제거하거나 데이터를 지우지 않습니다.
4. 담당자는 정확한 설치 대상과 현재 Android 사용자 프로필을 확인한 뒤 저장을 유지하는 업데이트를 설치합니다. USB 설치를 사용한다면 대상 기기를 명시한 `adb -s <테스트_기기> install -r "<검증한_APK_경로>"`를 사용합니다. 별도 ADB 서버·사용자 프로필을 쓴다면 그 환경을 명시합니다. 서명 충돌은 기록하고 기존 설치를 삭제해 해결하지 않습니다.
5. **Curio Clerk: Night Shift**를 실행하고 메뉴의 **설정 / Settings**에서 **한국어 / English**를 선택합니다. 계정 로그인과 광고 시청은 필요 없습니다. 오프라인으로 진행할 수 있습니다.

1. Verify this change's final APK and hash. This is a development build, not a store release; do not mark installation or playability as verified before the checks finish.
2. Use portrait Android 10/API 29 or later with ARM64 app support, or a dedicated prepared PC emulator. Emulator use does not validate phone performance, heat, audio or haptics.
3. Prefer a dedicated device or separate Android profile. Preserve existing progress and record it as a returning session. Do not uninstall the game or clear its data to manufacture a first run.
4. Confirm the exact device and Android profile, then install a save-preserving update. If using ADB, name the device explicitly. Record signing conflicts instead of deleting the existing installation.
5. Launch **Curio Clerk: Night Shift** and choose English or 한국어 in Settings. No account or ad viewing is required; offline play is supported.

**PC 개인 플레이 인계:** 최종 APK 설치와 실행을 담당자가 확인한 경우에만 전용 에뮬레이터 창을 사용자에게 보여줍니다. 새로 시작할 수 있는 프로필인지, 기존 진행을 이어가는 프로필인지 알려주고 현재 화면에서 멈춥니다. 사용자가 플레이하는 동안 자동 클릭이나 정답 입력을 하지 않습니다. 다른 프로젝트의 에뮬레이터나 기존 저장이 있는 환경은 그대로 둡니다.

**Personal PC handoff:** show the dedicated emulator only after the facilitator verifies the final APK is installed and running. State whether its profile is fresh or continuing existing progress, then leave control to the player. Do not automate inputs or supply solutions during their session. Preserve other emulator environments and saved games.

## 실제 진행과 조작 도움 / Route and controls

새 게임은 **첫 출근 → 선임의 짧은 부탁 → 물건 살펴보기 → 작업대** 순서로 시작합니다. 작업대에서는 현재 물건의 표시된 부분을 살펴보고, 도구를 사용했을 때 무엇이 달라지는지 확인합니다. 도움을 제공하더라도 특정 물건의 풀이를 대신하지 않습니다.

A new game follows **Start your first night → a short request from the senior → Take a look → the workbench**. Examine marked parts of the object and observe what changes when tools are used. Control help must not reveal a specific solution.

- **살펴보기 / Examine:** 물건에 표시된 부분을 누릅니다. 도구를 선택한 상태에서 다시 관찰하려면 **살펴보기**를 누른 뒤 해당 부분을 누릅니다. / Tap a marked part. If a tool is selected, choose **Examine** before inspecting a part again.
- **도구 드래그:** 도구를 물건의 사용할 부분까지 끌어 놓습니다. 에뮬레이터에서는 마우스 왼쪽 버튼을 누른 채 끕니다. / Drag a tool onto the part where you want to use it; in the emulator, hold the left mouse button while dragging.
- **탭으로 사용:** 도구를 한 번 누르고 사용할 부분을 누릅니다. / Tap a tool, then the part where you want to use it.
- **다른 시도:** 맞지 않는 도구나 아직 준비되지 않은 동작을 시도해도 목숨·재화를 잃지 않습니다. 반응을 읽고 다른 시도를 할 수 있습니다. 광고나 대기가 필요하지 않습니다. / An unsuitable or premature action does not cost lives or currency. Read the response and try something else; no ad or wait is required.
- **처음부터 / Start over:** 아직 완료하지 않은 현재 작업대의 시도를 다시 시작합니다. 이미 완료한 장면과 사건 기록을 지우는 버튼이 아닙니다. / Restart the current unfinished workbench; completed scenes and case records remain.
- **보관소 / Office:** 메뉴로 돌아갑니다. 완료한 장면은 저장됩니다. / Return to the menu; completed scenes remain saved.

첫 사건 **녹지 않는 얼음 / The Unmelting Ice**은 다섯 장면입니다. 사건이 끝나면 보관소에서 **기억하는 비 / The Remembering Rain**를 시작할 수 있습니다. 처음 열리는 두 번째 사건의 버튼은 **우산의 말 들어보기 / Listen to the umbrella**입니다. 두 사건은 모두 열 장면입니다. 가능한 만큼 진행하되, 두 결말을 보는 것을 참가 조건으로 삼지 않습니다. “이제 그만하고 싶다”는 시점도 중요한 관찰입니다.

**The Unmelting Ice** has five scenes. After its ending, the office offers **The Remembering Rain**, initially through **Listen to the umbrella**. There are ten scenes across both cases. Play as far as you wish; seeing both endings is not a condition of participation. Wanting to stop is useful feedback.

**분류 연습 / Sorting practice**는 별도 모드입니다. 이번 첫 자연 플레이는 사건 버튼으로 진행합니다. 광고·동의 QA 메뉴가 보이더라도 이번 사람 플레이 경로에 포함하지 않습니다.

**Sorting practice** is a separate mode. Use the case entry for this first natural session. Any Ads/Consent QA menu is outside this human gameplay session.

## 저장·재개·다시보기 / Saving, resuming and replay

- 저장 기준은 **완료한 장면**입니다. 장면을 끝내면 다음 장면으로 넘어가기 전에 완료 기록이 저장됩니다. / A completed scene is the save checkpoint; its completion is saved before continuing.
- 앱이 계속 실행 중인 상태에서 메뉴로 잠깐 나갔다 같은 사건으로 돌아오면 진행 중이던 작업대를 이어갈 수 있습니다. 다만 운영체제가 앱 프로세스를 종료한 경우에는 마지막 완료 지점 다음 장면의 처음부터 시작합니다. / Returning from the menu within the same running app can retain the unfinished workbench. If the OS ends the process, resume from the beginning of the scene after the last completed checkpoint.
- 홈 화면으로 이동했다 돌아오는 것과 앱 프로세스를 완전히 종료했다 실행하는 것은 다르게 기록합니다. / Record a brief app switch separately from a full process restart.
- 완료한 사건은 메뉴의 **사건 다시보기 / Replay Case**로 다시 볼 수 있습니다. 다시보기가 다른 사건의 진행, 완료 기록이나 재화를 바꾸지 않는지 확인합니다. / Use **Replay Case** for a completed case. Check that replay preserves the other case's progress, completion records and currency.
- 아직 완료하지 않은 작업대에서 **처음부터**를 누르는 것과 완료한 사건 전체를 다시 보는 것은 구분합니다. / Distinguish restarting an unfinished workbench from replaying a completed case.

저장·재시작·언어 전환을 일부러 확인하는 단계는 첫 자연 플레이를 마친 뒤에 진행합니다. 화면을 보고 스스로 선택한 행동과 진행자가 요청한 확인 작업을 같은 결과로 합치지 않습니다.

Perform deliberate restart, save and language checks after the natural session. Keep voluntary player actions separate from facilitator-requested checks.

## 진행자용 검증 가설 / Facilitator hypotheses

이전 버전에 대한 실제 개발자 플레이 피드백은 “재미없다”, “왜 해야 하는지와 어떤 행위인지 모르겠다”, “물건을 읽고 분류하는 노가다 같다”, “NPC·메뉴 문구가 어색하다”였습니다. 새 버전이 이 문제를 해결했다고 가정하지 않습니다. 이 피드백은 플레이어에게 먼저 읽어주거나 동의를 구할 문구가 아니라, 관찰자가 확인할 가설의 출발점입니다.

The developer's actual feedback on the previous version was that it was uninteresting, its purpose and actions were unclear, classification felt repetitive, and NPC/menu text sounded awkward. Do not assume this revision solves those problems. These observations motivate the hypotheses; do not prime a player with them or ask for agreement.

| 확인할 가설 / Hypothesis | 기록할 관찰 / Evidence to record |
| --- | --- |
| 목적이 이해되는가 / Purpose is understood | 플레이어가 자신의 역할과 현재 하고 싶은 일을 어떻게 설명하는지, 별도 설명이 필요했는지 / How they describe their role and immediate intention, and whether an explanation was needed |
| 행동과 결과가 연결되는가 / Actions have understandable effects | 예상한 변화와 실제로 알아차린 변화, 관찰·도구 사용을 선택한 이유 / Expected and noticed changes, and reasons for inspecting or using a tool |
| 장면을 이어갈 이유가 있는가 / There is a reason to continue | 자발적 다음 장면 선택, 궁금해한 내용, 멈추고 싶어진 지점 / Voluntary continuation, expressed curiosity and points where they wanted to stop |
| 반복이 어떻게 느껴지는가 / How repetition feels | 새롭게 느낀 행동과 되풀이로 느낀 행동, 설명 없이 남긴 발언 / Actions perceived as new or repetitive, in the player's own words |
| 문구가 자연스럽고 읽히는가 / Copy reads naturally | 다시 읽은 문장, 이해한 뜻, 어색하다고 지목한 정확한 문구와 위치 / Text reread, its understood meaning, and the exact wording/location identified as awkward |
| 조작이 의도대로 되는가 / Controls behave as intended | 드래그·탭 시도와 결과, 빗나간 입력, 같은 동작을 반복한 이유 / Drag/tap attempts and results, missed inputs and repeated actions |

첫 관찰까지와 첫 물건 변화까지의 시간을 별도로 적을 수 있지만 속도를 요구하지 않습니다. 진행자가 조작을 설명하거나 풀이를 알려준 시점과 내용을 기록합니다. 풀이를 알려준 뒤 완료한 장면을 “도움 없이 완료”로 세지 않습니다. 화면 기록은 선택 사항이며 동의한 경우에만 게임 화면을 기록합니다.

You may record time to first inspection and first object change separately, without pressuring the player to be fast. Record when and how help was given. A scene solved after receiving its solution is assisted. Screen recording is optional and should capture only the game with the player's agreement.

## 중립적인 질문 / Neutral questions

자연스러운 중단점이나 플레이 종료 후에 묻습니다. 한꺼번에 전부 답하도록 요구하지 않으며 질문에 원하는 답을 덧붙이지 않습니다.

Ask at a natural pause or after the session. Do not require every answer at once or suggest preferred answers.

1. 지금 이곳에서 무슨 일을 하고 있다고 생각했나요? / What did you think you were doing here?
2. 다음에 무엇을 하려고 했나요? 그렇게 생각한 이유는 무엇인가요? / What were you trying to do next, and why?
3. 그 행동을 하기 전에는 무엇이 일어날 거라고 예상했나요? 실제로는 무엇을 보았나요? / What did you expect that action to do, and what did you notice afterward?
4. 더 알아보고 싶은 것이 있었나요? 있었다면 무엇이었나요? / Was there anything you wanted to find out more about? What was it?
5. 계속하고 싶거나 멈추고 싶었던 순간은 언제였나요? / When did you want to continue or stop?
6. 되풀이처럼 느껴진 부분이 있었나요? 어떤 부분이었나요? / Did anything feel repetitive? Which part?
7. 읽고도 뜻을 알기 어려웠거나 말투가 어색했던 문장이 있었나요? 화면의 어느 부분이었나요? / Was any wording difficult to understand or awkward? Where was it?
8. 조작하려던 것과 다르게 된 순간이 있었나요? 그 뒤에는 무엇을 했나요? / Did a control behave differently from your intention? What did you do afterward?
9. 가장 기억에 남는 물건이나 순간은 무엇인가요? / Which object or moment do you remember most?
10. 여기까지 해본 뒤 더 해보고 싶나요? 그 이유는 무엇인가요? / After this session, would you want to play more? Why?

추가 확인을 했다면 따로 묻습니다. “돌아왔을 때 어디서 이어질 거라고 생각했나요? 실제로는 어디였나요?” / For an optional resume check: “Where did you expect to resume, and where did you actually resume?”

## 익명 관찰 기록 / Anonymous observation record

P01 같은 임의 기록 ID를 사용합니다. 이름, 연락처, 계정, 기기 일련번호·광고 ID를 수집하지 않으며 신원 대조표를 만들지 않습니다. “미관찰”은 통과도 실패도 아닙니다. 이전 버전을 해본 개발자의 재평가와 처음 보는 사람의 평가는 구분합니다.

Use an arbitrary record ID such as P01. Do not collect names, contact details, accounts, device serials or advertising IDs, or create an identity lookup list. Unobserved means neither pass nor fail. Separate a returning developer's assessment from that of a first-time player.

```text
기록 ID / Record ID:
빌드 소스 커밋·APK SHA-256 / Source commit and APK SHA-256:
검증한 설치 환경 / Verified installation environment:
언어 / Language:
기기 종류·Android 버전·화면 비율 / Device class, Android version, aspect ratio:
새 사용자/이전 버전 플레이/소스 내용을 아는 개발자 / New player, previous-version player, or source-aware developer:
기존 저장 진행 여부 / Existing saved progress:
오프라인/온라인 / Offline or online:
플레이 시간·휴식 / Play duration and breaks:
첫 관찰 시점·첫 물건 변화 시점(선택) / Time to first inspection and object change, optional:
진행한 사건·장면 / Cases and scenes reached:
자발적 종료 지점과 이유 / Voluntary stopping point and reason:

관찰 시각·사건·장면 / Time, case and scene:
자연 플레이/진행자 요청 확인 / Natural play or facilitator-requested check:
플레이어 행동·그대로의 발언 / Player action and verbatim comment:
예상한 결과·알아차린 결과 / Expected and noticed result:
조작 도움/풀이 도움·제공 시점 / Control help or solution help, and when:
도움 없이 완료/도움 후 완료/미완료/미관찰 / Unassisted, assisted, incomplete or unobserved:
정확한 문구·화면 위치 / Exact wording and screen location:
재현 순서(정답 여부와 분리) / Reproduction steps, separate from solution correctness:
게임 화면 증거 위치(선택) / Game-only evidence location, optional:

질문 응답 / Question responses:
관찰자 해석(행동·발언과 분리) / Observer interpretation, separate from actions and comments:
아직 모르는 점 / Remaining unknowns:
다음 수정·확인 후보 / Candidate changes or follow-up checks:
```

## 결과를 해석할 때 / Interpreting results

자동 테스트와 소스를 아는 에이전트의 완주 기록은 조작·진행·저장 경로의 기술적 근거입니다. 처음 접한 사람이 목적을 이해했거나 재미와 몰입을 느꼈다는 근거로 대체하지 않습니다. 완료율만으로 재미를 판정하지 않고, 스스로 한 선택·멈춘 지점·발언을 함께 봅니다. 개선 여부는 실제 새 버전 플레이 이후에 판단합니다.

Automated tests and source-aware agent playthroughs provide technical evidence about controls, progression and saves. They do not establish first-time comprehension, enjoyment or immersion. Consider voluntary choices, stopping points and comments alongside completion. Assess improvement only after people play this revision.

이 문서는 준비 자료입니다. 작성 과정에서 사람을 모집하거나 메시지를 보내거나 응답을 수집하지 않았습니다.

This is preparation material. No participants were recruited, messages sent or responses collected while writing it.
