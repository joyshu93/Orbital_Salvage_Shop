# 작업대 선택형 힌트 QA — 2026-09-11

상태: **전체 Editor 검사 359/359·조건별 컴파일 15/15 통과, 힌트 APK 빌드·한영 힌트 및 재시작의 Android QA 완료**. 최종 소스 `6437d57f642b77ca05a2904600439199a88782a3`의 C# 124개가 기록한 빌드 입력과 일치한다. 사용자용 업데이트는 진행 중인 사람 플레이를 보존하려고 실행하지 않았으며 기존 Candidate 2를 유지한다. 제품 기준은 `codex/immersive-cases-20260911`의 `.worktrees/immersive-cases-20260911`이다. 이전 후보 전체 검증은 [기존 QA](HandsOnCases-20260911.md)에 유지한다.

## 실제 피드백과 원인

이전 버전과 개선판을 플레이한 개발자는 퍼즐을 푸는 재미가 일부 생겼지만, 얼음 두 번째 장면에서 막혔고 힌트를 찾을 수 없었다고 했다. 이는 재방문 개발자 한 명의 피드백이며 처음 보는 사람들의 이해도나 재미를 검증한 결과가 아니다.

코드에서 `ice-02-spread`의 `lift-base`는 `warm-rim` 완료와 `base-lid` 관찰을 모두 요구한다. 기존 힌트는 테두리 얼음을 녹이고 덮개 홈에 쐐기를 넣으라고만 안내해, 별도 관찰 입력이 필요함을 밝히지 않는다. 기존 도움은 도구·대상 조합이 맞지만 관찰이나 선행 행동이 부족한 시도에서 나타나므로, 막힌 플레이어가 직접 찾아 요청할 수 없었다.

## 선택한 해결 방식

- 작업대에 자발적으로 누르는 **힌트 / Hint**, 이어서 **더 자세히 / More help** 버튼을 제공한다. 첫 요청은 현재 다음 행동의 기존 한영 힌트를 보여 준다.
- 다음 행동은 아직 완료하지 않았고 선행 행동이 충족된 행동이다. 미관찰 상태라는 이유로 후보에서 제외하지 않는다.
- 더 자세한 도움은 먼저 필요한 관찰 중 아직 하지 않은 첫 대상을 지목하고 강조한다. 남아 있는 도구 선택을 해제해 해당 대상의 탭이 관찰이 되게 한다.
- 관찰을 마쳤다면 사용할 도구와 대상을 명시하고 둘을 강조한다. 다시 도움을 요청하면 현재 관찰·진행 상태로 안내를 계산한다.
- 행동 완료나 현재 장면 처음부터 재시작 시 도움 단계를 초기화한다. 힌트는 재화·광고·대기를 요구하지 않으며 저장 진행을 변경하지 않는다.
- 안내는 현재 행동에 한정한다. 다음 장면의 사연이나 결말은 추가로 공개하지 않는다. 출처는 `TEXT-WORKBENCH-HINT-20260911`로 사전 기록했다.

## 재현과 확인 경로

아래 순서는 정답을 아는 기술 QA용이다. 사람의 자연 플레이 전에 읽어 줄 안내가 아니다.

| 장면 / 상황 | 입력과 확인할 결과 |
|---|---|
| Ice02 처음 진입 | 힌트 버튼을 직접 찾을 수 있다. 힌트→더 자세히 요청 시 `얼어붙은 테두리 / Frozen rim` 관찰을 안내한다. |
| Ice02 테두리 | `frozen-rim` 관찰 → `warm-pad`를 `frozen-rim`에 사용. 다음 행동의 힌트가 선택된다. |
| Ice02 실제 막힌 지점 | `base-lid` 관찰 → `wooden-wedge`를 `base-lid`에 사용. 관찰 전 도구가 선택되어 있어도 자세한 도움 뒤 덮개 탭은 관찰로 처리되어야 한다. |
| Ice02 열린 받침 | 덮개 개방 뒤 나타나는 `watch-chain` 관찰 → `fine-tweezers`를 `watch-chain`에 사용. 시계를 꺼내 장면을 완료한다. |
| Ice01 다른 부분의 관찰 조건 | `seal-crack`은 `crack`과 `leaf` 관찰을 모두 요구한다. 틈을 닦았지만 낙엽을 보지 않았다면, 자세한 도움은 현재 도구의 대상인 틈 대신 미관찰 `leaf`를 지목해야 한다. |
| 관찰 완료 후 도움 | 아직 실행하지 않은 현재 행동의 정확한 도구·대상이 표시되고 강조된다. 같은 관찰 안내에 고정되지 않는다. |
| 재시작·저장 | 행동 완료와 처음부터에서 도움 단계가 초기화된다. 힌트 사용 전후 사건 기록·재화가 동일하며 저장을 새로 만들거나 덮어쓰지 않는다. |
| 한영·레이아웃 | 한국어·영어의 버튼과 자세한 안내가 읽히고 다른 조작을 가리지 않는다. 도구명에 붙는 한국어 조사가 어색해지지 않는다. |

## 검증 기록

| 항목 | 현재 확인 범위 |
|---|---|
| Core RED | 새 검사 3개가 다음 행동 선택 결과 `null`로 실패한 것을 확인했다. 구현 전 기대 동작의 실패 기록이며 통과 결과가 아니다. |
| UI RED | `PlayMode-hints-red.xml`: **0/3 통과, 3개 실패**, 건너뜀 0. **06:29:41–06:29:43 UTC**. 세 검사 모두 `WorkbenchHintButton must be visible` assertion으로 실패해 버튼 부재를 확인했다. |
| UI GREEN | `PlayMode-hints-green.xml`: **3/3 통과**, 실패·건너뜀 0. **06:33:26–06:33:28 UTC**, Unity exit **0**. 현재 행동 안내→미관찰 안내, 받침 수리 순서에 따른 도움 갱신, 다른 필수 관찰·재시작 초기화를 확인했다. |
| 조건별 컴파일·목록 | **15/15** exit 0: Android 일반·native QA·native offline QA·native 비개발·Windows 일반의 다섯 조건 × Runtime/TestRunner/PlayMode 어셈블리. 일반 테스트 메서드 **126개**가 다섯 조건 모두 현재 Editor와 일치하고 런타임 포함 조건도 일치했다. 기록 소스 **98개**(Runtime **66** + PlayMode **6** + Core **26**), 확인 시각 **06:39:28 UTC**. 실제 네이티브 플레이어에서 테스트를 실행하거나 다섯 APK를 패키징한 결과는 아니다. |
| 구현 후 Core / 전체 회귀 | 전체 **EditMode 229/229**, **06:42:26–06:42:38 UTC**; 전체 **PlayMode 130/130**, **06:44:22–06:46:33 UTC**. 합계 **359/359**, 실패·건너뜀 0. 확인한 worktree 원본은 빌드 실행 폴더의 `final-EditMode-results.xml`, `final-PlayMode-results.xml`로 보관했다. |
| 힌트 APK 빌드 | `Hints/run-20260911T064019580Z/build-summary.json`: **06:40:36–06:53:39 UTC**, wrapper exit **0**, `buildSucceeded`, `sourceHashesStable`, `publishedOutputsRestored` 모두 **true**. `final-AndroidDevelopmentBuild.log`의 `Build Finished, Result: Success`와 정상 종료 return code **0**을 확인했다. 기존 Candidate 2 출력은 원래 해시로 복원했다. |
| 힌트 APK 파일 | `C:\Users\D-\Documents\Codex_Project\Orbital_Salvage_Shop\.worktrees\immersive-cases-20260911\Builds\Android\WorkbenchHints-20260911\CurioClerk-qa.apk` · **105,252,170 bytes** · SHA-256 **`948F28C6D8E37600F0A1D66AED81259EBB17AC8F15C2F275DD5FFFDB43B3E79C`**. 실제 파일 크기·해시를 확인했다. |
| APK 메타데이터·QA 설치 | **06:54:40 UTC**, `CurioClerk_WorkbenchQA_20260911`, ADB `127.0.0.1:5597` / 서버 `5062`, Android user **0**에 설치. 설치된 APK의 해시가 위 파일과 일치했다. Package `com.joyshu93.curioclerknightshift`, version **1.0.0 / 10000**, API **29/36**, portrait, **Android Debug / debuggable**, native ABI는 **arm64-v8a만** 포함. Manifest의 GMA App ID는 공식 샘플 `ca-app-pub-3940256099942544~3347511713`. 근거: `Hints/qa-install-D7TCyv/qa-install-verification.json`, `apk-badging.txt`, `apk-manifest.txt`. |
| 한국어 실제 입력 | 기존 완료 저장의 다시보기로 Ice01 두 행동 후 Ice02의 세 행동을 실행했다. 각 행동에서 힌트→더 자세히→대상 관찰→도구 안내→도구·대상 탭으로 진행했다. 플레이 전 Wi-Fi **0**, mobile data **0** 확인. |
| Ice02 막힘 복구·화면 | 덮개 관찰 전 쐐기 시도가 실패한 뒤 힌트로 받침 덮개를 강조하고, 관찰 후 쐐기를 사용해 진행했다. 관찰 안내 1·2, 도구 안내 1·3, 완료 화면의 검토에서 잘림을 발견하지 못했고, 완료 화면에는 힌트 버튼이 숨겨졌다. 근거: `hints-ice2-needs-observation.png`, `hints-ice2-observe-*.png`, `hints-ice2-tool-*.png`, `hints-ice2-complete-ko.png`. |
| 영어 관찰·재시작 | Ice01 물기를 닦은 뒤 `Trapped leaf` 미관찰 안내와 해당 대상 강조를 확인했다(`hints-ice1-observe-en.png`). 처음부터 이후 첫 균열 힌트와 모든 관찰 표시의 `+` 초기화를 확인했다(`hints-restart-gentle-en.png`). 두 검토 화면에서 안내 잘림을 발견하지 못했다. |
| QA 최종 저장 | 한국어 보관소로 돌아왔다(`hints-final-office-ko.png`). 설치 전·후 저장, 한국어 Ice02 완료 후 `hints-after-ice2-ko.json`, 최종 `hints-final-save.json`의 SHA-256이 모두 **`01D1D32008A8F763BF54AD1D37A11BC55B1C46AF2B55C26695FEF782CF895579`**로 일치했다. 기존 기록 **10**, 완료 사건 **2**, 발견 **7**, coins **0**, locale **ko**, 두 동의 **false**를 유지했다. |
| 네이티브 로그 | `Hints/native-final-logcat.txt`, 앱 PID **9947**: FATAL, NullReference, MissingReference, IndexOutOfRange 패턴을 발견하지 못했다. 언어 저장 시 Android SELinux의 파일 `{ link }` 거부 경고 **2회**는 남아 있으며 최종 저장 해시는 유지됐다. |
| 최종 소스 | **`6437d57f642b77ca05a2904600439199a88782a3`**. `Hints/final-source-proof.json`에서 추적된 C# **124개**의 목록·해시가 빌드 입력과 일치하고 소스·테스트 미커밋 변경이 없음을 확인했다. 빌드는 `4b724797fd16fa1e110fb9b01f9f751f89b74415`에서 시작했으며 당시 미커밋 힌트 변경이 최종 소스 커밋에 포함됐다. |
| 검증 파일 정리·보존 | `Hints/cleanup-20260911T073220057Z/summary.json`: 임시 저장 경로 guard **2개**를 보관하고 이 작업에서 생성된 메타데이터 **21개**만 복원했다. 원본 작업공간의 기존 dirty **22개**와 힌트 소스·문서 바이트 보존을 전후 확인했다. 이전 Candidate 2 APK와 companion 출력 4개도 원래 바이트로 복원했다. |
| 사용자용 설치 | **미실행 / NOT RUN** — 현재 진행 중인 사람 플레이를 보존하기 위해 사용자용 AVD의 기존 Candidate 2를 유지했다. 힌트 수정판 설치·입력 검증은 별도 QA AVD에서만 수행했다. |

새 APK의 네이티브 확인 범위는 한국어 Ice01 경유·Ice02 세 행동 전체, 영어 Ice01의 다른 필수 관찰과 재시작이다. 새 APK로 두 사건 전체를 다시 완주한 결과는 아니다. 영어 안내 확인도 첫 장면 전체 완료로 세지 않는다.

오프라인 네이티브 로그에는 Android adservices의 measurement 서비스 `ServiceUnavailableException`도 남았다. 진행 중 게임 크래시나 Unity 관리 예외는 관찰하지 못했고 최종 저장 해시는 유지됐다. 로그 전체가 오류·경고 없이 깨끗하다는 판정은 아니다. 실기기 성능과 기존 광고 응답의 미검증 항목은 이전 QA의 한계를 그대로 유지한다.

위 XML과 같은 이름의 로그는 원래 프로젝트의 `Logs/ImmersiveCases-20260911/`에 보관한다. 컴파일·목록 증거는 그 아래 `Candidate2/compiler-results-hints.json`, `assembly-inventory-hints.json`, `editor-inventory-hints.json`, `compiler-sources-hints.json`이다. 원시 XML·로그·스크린샷·저장은 로컬 증거로만 보관하고 커밋하지 않는다. 실행되지 않은 검사나 컴파일 오류는 유효 RED로 세지 않는다. 에이전트가 풀이를 따라 완료하는 기술 QA와, 사용자가 스스로 힌트를 발견하고 막힘을 해결하는지는 별개의 결과다.
