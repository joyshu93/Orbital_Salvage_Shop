# 내부 플레이테스트 통합 QA — 2026-09-09

상태: 내부 사람 플레이테스트를 시작할 개발 APK와 안내 자료 준비 완료. 최종 Editor 회귀와 전용 에뮬레이터의 두 사건 연속 실행을 확인했다. 사람의 첫 플레이 이해도·재미는 아직 미검증이다. [한영 플레이테스트 안내와 익명 양식](../Playtests/InternalPlaytest-20260909.md)을 함께 사용한다. 이 문서는 내부 개발 빌드의 범위를 기록하며 출시 승인이 아니다.

## 기준과 변경

작업 시작 시 [PR #4](https://github.com/joyshu93/Orbital_Salvage_Shop/pull/4)는 열려 있었고 HEAD는 `49c4a01783750afd6f0917d84e286e675e0f4275`, main은 `d15e39a`였다. 선행 `86116ff` 포함도 확인했다. 새 브랜치 `codex/internal-playtest-20260909`와 전용 worktree `.worktrees/internal-playtest-20260909`를 이 HEAD에서 만들었다. 개선 PR은 미병합 #4의 브랜치를 기준으로 하여 이번 diff만 표시한다.

이전 보고서의 Hold·가독성·반복성 문제를 그대로 현재 결함으로 취급하지 않았다. 보존된 기존 개발 APK에서 새 Android 사용자로 첫 실행을 확인하고 다음 두 문제를 재현했다.

| 확인된 문제 | 재현과 영향 | 최소 수정 |
|---|---|---|
| 정상적인 Hold 선택으로 진행 불가 | Ice 1에서 Repair → Hold → Storage → Hold. Repair/Storage가 찍힌 장부에 Repair 물건이 나오고 Vault 물건은 보류 칸에 남는다. Hold는 쿨다운이고 올바른 Repair는 잠겨 있어 성공 입력이 없다. 잘못된 Vault를 선택하면 하트만 잃으며 실패로 빠져나온다. | Hold가 가져올 물건의 목적지가 이미 찍혔다면 그 Hold만 거절한다. 현재 물건·보류 물건·하트는 유지하며 장부 완료 뒤 다시 허용한다. 이유를 한영 버튼·피드백으로 표시한다. |
| 첫 사건 카드의 제목·그림 겹침 | 새 저장의 1080×2400 첫 메뉴에서 큰 제목과 사건 그림이 같은 영역을 사용했다. | 큰 현재 사건 카드에서 그림과 제목·단서를 좌우로 나누고 제목 자동 크기 조절을 적용한다. 작은 완료 카드의 동작은 유지한다. |

새 사건, 이야기·분류 규칙·보상·세이브 형식·광고 흐름은 변경하지 않았다. 입력 잠금, 보호 Hold 기록, 1회 Hold 뒤 성공 분류 조건은 유지한다. 기능 안내 문구 두 쌍의 출처는 `TEXT-PLAYTEST-20260909`로 [AI 출처 기록](../AIAssetProvenance.md)과 [고지](../ThirdPartyNotices.md)에 기록했다. 아트·오디오·외부 자산은 추가하지 않았다.

## 테스트 증거와 실패 기록

모든 Unity 작업은 `6000.3.21f1`과 새 worktree에서 수행했다. PlayMode 테스트가 실제 파일 저장소를 여는 구조이므로, 커밋하지 않는 Editor 전용 시작 검사로 product name 및 `Application.persistentDataPath`를 새 `CurioClerk-InternalPlaytest-20260909` 디렉터리로 격리했다. 빌드의 `BuildAll`은 정상 제품 설정을 적용한다. 원래 프로젝트와 이전 QA worktree의 임시 설정은 재사용하지 않았다.

| 실행 | 결과 | 해석 |
|---|---|---|
| 수정 전 저장소 전체 EditMode | 171/171 통과 | 현재 기준 재확인 |
| 수정 전 전체 PlayMode | 126/127 통과, 1 실패 | 변경하지 않은 `IncidentReaction_FrostStateAndMistakeLineRemainAtmosphericAndReadable`의 회전 각도 샘플이 0이었다. 같은 소스의 단독 재실행은 1/1 통과했다. 초기 실패를 삭제하거나 제품 수정으로 해결했다고 기록하지 않는다. |
| 새 Hold EditMode RED | 0/3, 3 실패 | 첫 Hold, 교환 Hold, 실제 세션으로 열 단계의 모든 합법적 분류/Hold 경로를 탐색하는 회귀가 수정 전에 실패 |
| 새 UI PlayMode RED | 0/2, 2 실패 | 한영 Hold 버튼/안내 및 카드 텍스트·그림 분리 검사 실패 |
| 관련 EditMode GREEN | 32/32 통과 | 실제 ShiftSession 기반 열 단계 탐색 포함 |
| 관련 PlayMode 첫 GREEN | 4/5 통과 | 신규 두 테스트는 통과. 기존 고정 seed 테스트는 저장된 완료 교대 수로 난이도가 달라져 Hold 검사 전에 시작 목적지 예상이 실패했다. 해당 테스트에 `completedShifts = 0`을 명시했다. |
| 관련 PlayMode 최종 GREEN | 5/5 통과 | 카드 검사는 실제 CanvasScaler 계산으로 1080×1920 및 1080×2400의 논리 UI 크기를 적용하고, EN/KO·사건 보드 세 상태를 검사 |
| 최종 전체 Editor 회귀 | EditMode 174/174, PlayMode 129/129 | 실패·건너뜀 0. EditMode 06:43:17–06:43:21 UTC, PlayMode 06:44:43–06:50:55 UTC. 두 Unity 프로세스 및 저장소 테스트 스크립트 exit 0. 초기 애니메이션 실패는 이 전체 실행에서 재발하지 않았다. |
| 개발 빌드 스크립트 | exit 0, APK 생성 | `scripts/build-android-dev.ps1`가 위 전체 테스트에 이어 BuildAll, 콘텐츠 검증, ARM64 IL2CPP 빌드를 수행했다. 제품/테스트 C# 해시는 빌드 전후 동일했다. |

초기 RED 도구 작성 중 존재하지 않는 테스트 API를 호출한 컴파일 오류도 별도로 보존했다. 이를 제품 결함이나 유효한 RED 실행으로 세지 않았다. 원시 증거는 원래 프로젝트의 로컬 `Logs/InternalPlaytest-20260909/`에만 보관한다. `EditMode-hold-red-valid.xml`, `PlayMode-presentation-red.xml`, `EditMode-hold-green.xml`, `PlayMode-presentation-green-final.xml` 및 각 로그가 판정 근거다.

별도 첫 `BuildAll` 실행은 자산 생성과 콘텐츠 검증 성공 로그 뒤 Unity 종료 중 `-1073741819`(접근 위반)로 끝났다. 이를 프로세스 성공으로 세지 않았다. `BuildAll-final.log`를 보존했으며, 이후 개발 빌드에 포함된 BuildAll과 Unity 종료는 모두 정상 완료했다. 최종 증거는 `final-EditMode-results.xml`, `final-PlayMode-results.xml`, `final-AndroidDevelopmentBuild.log` 및 `build-source-hashes.json`이다. Unity 라이선스 연결/CDN 인증 경고와 Burst 진단 출력은 테스트 판정 및 실제 빌드 종료 코드와 구분한다.

읽기 전용 독립 코드 리뷰는 제품·테스트 여섯 파일을 검토했다. 남은 Critical/Important/Minor 지적은 없다. 최초 지적된 화면 테스트의 CanvasScaler 차이는 보완하고 재검사했다.

## QA 컴파일 가드

QA 조건부 가드는 변경하지 않았다. 같은 파일에 일반 UI 테스트를 추가했으므로 다섯 플레이어 조건에서 Runtime, Unity Test Framework TestRunner, 전체 PlayMode 소스를 각각 새 DLL로 컴파일하고 Cecil로 포함 목록을 대조했다. Unity가 생성한 성공 Android response file의 엔진·패키지 참조를 사용하되 변경된 Core는 새 worktree의 현재 어셈블리를 참조했다. Windows는 설치된 Windows 엔진 참조와 심볼을 사용했다. 대역 타입을 만들지 않았다.

| 조건 (`UNITY_INCLUDE_TESTS` 공통) | 3개 어셈블리 컴파일 | QA 화면 테스트 | SDK 콜백 테스트 | 일반 테스트 메서드 |
|---|---|---:|---:|---:|
| Android Development | 모두 exit 0 | 0 | 1 | 125 |
| Android Development + native QA | 모두 exit 0 | 3 | 1 | 125 |
| 위 조건 + offline QA | 모두 exit 0 | 0 | 0 | 125 |
| Android native QA, 비개발 | 모두 exit 0 | 0 | 1 | 125 |
| Windows Development | 모두 exit 0 | 0 | 0 | 125 |

각 일반 테스트 목록은 Editor와 동일했고 QA 런타임 타입·진입점도 일치했다. `compiler-results-final.json`, `assembly-inventory-final.json`, `Compiler/final/`에 보존했다. 이 표는 C# 컴파일과 DLL 검사이며 다섯 종류의 테스트 플레이어 패키징·네이티브 실행을 의미하지 않는다.

## 최종 개발 APK와 실행

제품 소스 커밋: [`9a8edbeec722a72a99ca4f08d93cff0f6fdd5d8e`](https://github.com/joyshu93/Orbital_Salvage_Shop/commit/9a8edbeec722a72a99ca4f08d93cff0f6fdd5d8e). 이후 QA/안내 문서 커밋은 제품 입력을 바꾸지 않는다.

- 원래 프로젝트 기준 로컬 경로: `.worktrees/internal-playtest-20260909/Builds/Android/CurioClerk-qa.apk`.
- SHA-256: `D3E4B9343E2A379139873F14D1E38B5AD24A0A55D842BA69F152C86810C920A1`.
- 파일 크기: 79,552,972 bytes. Unity `6000.3.21f1`, version `1.0.0`/code `10000`, package `com.joyshu93.curioclerknightshift`.
- 실제 manifest와 ZIP 확인: min API 29, target/compile API 36, portrait, debuggable, 네이티브 라이브러리는 `arm64-v8a`만 존재하며 `libil2cpp.so` 포함.
- `apksigner verify --verbose --print-certs`: APK v2 서명 검증 통과, `CN=Android Debug`. 개발 빌드 추가 심볼 `CURIO_NATIVE_ADS_QA`; offline QA 대역 빌드가 아니다.
- 앱 manifest와 Unity 서비스 자산에 Google 공식 샘플 app/rewarded ID가 포함됨을 검사했다. APK 전체 문자열 검사에서 추가로 발견한 all-zero ID는 UMP SDK의 오류 메시지 속 manifest 예시이며 실행 설정이 아니다. 검사 도구의 최초 과도한 판정을 이 위치 확인 후 바로잡았다. 실제 서비스 ID는 사용하지 않았다.
- 기존 산출물을 덮어쓰지 않는 새 worktree 경로에 생성했다. APK·원시 로그·앱 저장은 Git에 넣지 않는다. 검사 증거는 로컬 `apk-verification.json`, `apk-badging.txt`, `apk-manifest.txt`, `apk-signature.txt`에 있다.

### 최종 APK의 직접 실행

전용 read-only AVD 인스턴스의 새 Android 사용자에서 시작했다. Android 16/API 36, 1080×2400 세로, x86_64 에뮬레이터의 Berberis ARM64 변환으로 위 ARM64 IL2CPP APK를 실행했다. 실제 ARM64 기기 실행이나 성능 측정으로 해석하지 않는다. Wi-Fi와 모바일 데이터를 끄고 계정·광고·동의 없이 진행했다. 입력은 소스에서 확인한 물건 순서와 규칙을 아는 에이전트가 ADB 탭으로 수행했으며 세이브 주입은 없었다.

| 범위 | 실제 실행 결과 | 로컬 화면·저장 증거 |
|---|---|---|
| 첫 실행과 큰 사건 카드 | EN 첫 메뉴의 제목·그림 겹침 해소. KO 전환 및 첫 사건 완료 뒤 Rain 카드도 분리 표시 | `12-*`, `21-*`, `44-*`, `47-*` |
| The Unmelting Ice | 5개 교대, 20개 장부, 결말과 Rain 진입 완료. 교대 1 EN, 2–5 KO. 결과 Precise 3회, Resonant 2회 | `16-*`–`44-*`, `final-ice-complete.json` |
| The Remembering Rain | 5개 교대, 20개 장부 및 최종 결말 완료. 교대 1 EN, 2–5 KO. 보호 물건을 보류한 5회 모두 Resonant. 장부 사이 대사 15개와 후속 진행 확인 | `48b-*`–`102-*`, `final-both-cases-complete.json` |
| 새 Hold 제한 EN/KO | Ice 1의 Repair → Hold → Storage → Hold 재현 경로에서 마지막 Hold가 차단됨. 현재 시계·보류 우산·하트 3 유지, 이유 표시. 올바른 Vault로 장부 완료 및 이후 진행 가능 | `15-*`, `106-*`, `110-*` |
| 앱 전환 | Rain 1의 보류 물고기·현재 찻잔·찍힌 Vault·하트 3·코인 5 상태에서 Home 후 복귀. 같은 프로세스와 같은 세션 상태 유지 | `50-*`–`53-*` |
| 교대 완료 저장 | Ice 1, Rain 1 완료 뒤 강제 종료/재실행 시 각각 2/5에서 계속 가능. 두 사건 완료 뒤 재실행도 완료 카드 2개와 한국어 유지 | `18-*`, `61c-*`, `103b-*` |
| 교대 도중 프로세스 종료 | Rain 2에서 한 물건을 처리한 뒤 종료. 완료 기록 6개와 Rain 2/5는 유지되며, 해당 교대는 첫 우산·빈 장부·코인 0·하트 3으로 다시 시작 | `66-*`–`69-*` |
| 의도적 오답·재시도 | 완료 사건 다시보기에서 오답 3회로 하트 3→2→1→0과 실패 화면 확인. ‘같은 교대 다시 하기’로 광고·대기 없이 첫 물건·빈 장부·하트 3으로 시작하고 첫 장부 재완료 | `104-*`–`110-*`, `final-after-retry.json` |
| 완료 보드 EN/KO | 다음 사건 예고와 완료 카드 2개, 다시보기·설정 버튼 확인. 언어 전환 뒤에도 완료 기록 10개 유지 | `102-*`, `103b-*`, `114-*`, `final-user11-save.json` |
| 튜토리얼 경로 점검 | 첫 사건의 도입·규칙·Hold 안내를 실제 실행. 자유 교대는 일반 교대로 바로 시작함. 별도 6물건 튜토리얼의 메뉴 진입점은 없음 | `13-*`–`27-*`, `111-*`, `112-*`; 아래 구분 참조 |
| 이전 APK 저장 호환성 | 기준 APK로 만든 별도 테스트 사용자 저장(version 4, EN, 첫 사건 교대 0)을 보존한 채 최종 APK에서 메뉴 진입. 실행 후 저장 SHA-256 동일. 오래 진행한 기존 사용자 저장의 네이티브 업데이트를 대표하지는 않음 | `118-*`, `baseline-profile10-save-root.json`, `final-baseline-user10-after-launch.json` |
| 오프라인·최종 재실행 | Wi-Fi와 모바일 데이터 모두 0 재확인. 최종 사용자로 돌아온 뒤 EN 메뉴에 완료 카드 2개 유지 | `final-wifi-enabled.txt`, `final-mobile-data-enabled.txt`, `120-*` |

완료 저장은 version 4이며 `completedIncidentIds` 두 개와 `incidentStageRecords` 열 개가 화면 결과와 일치했다. 원시 화면·로그·세이브는 로컬에만 보관한다. 두 언어로 모든 문장을 전수 재생한 검증은 아니며, 위 언어 분담과 대표 메뉴·규칙·피드백의 가독성 검사다. 일부 KO 문장/작은 미리보기 이름이 단어 중간에서 줄바꿈되는 현상은 남아 있다. 본 실행에서 잘림이나 진행 차단은 관찰하지 않았으며, 읽기 부담은 사람 테스트 관찰 항목으로 남긴다.

별도 튜토리얼 미노출은 새 회귀로 단정하지 않았다. [첫 사건 구현 계획](../superpowers/plans/2026-08-28-first-incident-vertical-slice.md)은 기존 코드를 남기면서 첫 두 사건 교대에서 역할·규칙·Hold를 가르치도록 경로 변경을 명시한다. 현재 `ShowTutorial()`의 런타임 호출자는 없고 자유 교대는 `OnStartPressed()`에서 일반 교대를 시작한다. 최종 전체 Editor 실행의 네 `Tutorial_*` 테스트는 통과했지만, 별도 튜토리얼의 APK 직접 실행은 **미실행**이다. 이 버전의 사람 안내서는 실제 첫 사건 시작 경로를 사용한다.

새 사용자 프로필의 첫 실행 때 Android의 ‘System UI isn’t responding’ 창이 한 번 나타났다. Unity 빌드와 에뮬레이터가 동시에 실행 중인 환경에서 시스템 UI 창을 닫고 앱 메뉴 진입을 다시 확인했으며, 이 최초 시도를 정상 첫 실행으로 세지 않았다. ADB 시작 명령에서 존재하지 않는 예전 Activity 이름을 지정한 1회 오류는 manifest의 `UnityPlayerGameActivity`로 바로잡았다. 초기 `run-as` 저장 읽기 실패, 한 이미지 뷰어 열기 실패도 도구 기록으로 보존했고, 실제 저장 JSON과 새 화면 캡처로 다시 확인했다.

이전 저장 호환성 검사 때 Android 사용자 전환 후 ‘Process system isn’t responding’ 창 1회와 ADB 명령 시간 초과 2회가 추가로 발생했다. 두 테스트 프로필의 앱 중 하나만 실행하도록 종료하고 시스템 창의 Wait 후 Home/복귀로 메뉴를 확인했다. Android 시스템 문제의 원인이 제품 코드라고 확인된 것은 아니지만 이 환경 이상을 숨기거나 최초 시도 통과로 바꾸지 않는다. 최종 사용자로 돌아와 재실행한 메뉴도 확인했다.

수집한 앱 PID별 로그에서는 `E Unity`, `FATAL EXCEPTION`, `Fatal signal`, `AndroidRuntime` 및 대표 관리 예외 패턴이 검출되지 않았다. 이는 모든 시스템 로그가 무오류라는 뜻은 아니다. SwiftShader가 지원하지 않는 보조 shader, EGL/Swappy 탐색, 오프라인 SDK 연결, Android 공유 저장소의 hard-link/SELinux 경고가 남아 있다. 실제 화면 렌더링, 저장 JSON 갱신과 재실행 유지로 각 기능을 판정했다. 소리·진동은 에뮬레이터의 `-no-audio` 및 하드웨어 한계 때문에 체감 검증하지 않았다.

## 보존과 미검증 경계

시작 기록은 원래 dirty 22개(아트 PNG 메타 20개, NotoSansKR-Dynamic.asset, ProjectSettings.asset), 빈 index, 브랜치·stash·worktree, story/이전 QA 작업, 기존 APK/AAB, 저장과 QA 로그를 포함한 1,560개 SHA-256을 담았다. 최종 비교에서 변경 파일 0, 원래 status 동일, index 비어 있음, 기존 브랜치·stash·worktree와 story/이전 QA status 모두 동일했다. 기존 에뮬레이터 앱 저장의 별도 SHA-256도 시작값과 같았다. 증거는 `preservation-baseline.json`, `preservation-latest.json`, `existing-device-save-sha-final.txt`에 있다.

새 worktree의 import 부산물은 diff를 로컬 보관하고 해당 경로만 복구했다. Editor 저장 격리 스크립트 두 파일은 로컬 `ArchivedEditorIsolation/`로 옮겼다. 제품 소스·테스트·생성 콘텐츠는 APK 소스 커밋과 동일하다. 원래 worktree의 미커밋 파일에는 이 정리를 적용하지 않았다.

전용 에뮬레이터는 별도 포트의 read-only AVD 인스턴스와 새 Android 사용자로 격리했다. 기존 에뮬레이터, 기존 사용자 저장, 원래 APK/AAB를 제거하거나 초기화하지 않았다. read-only 다중 인스턴스 및 사용자별 테스트 방식은 [Android 에뮬레이터 문서](https://developer.android.com/studio/run/emulator-commandline), [Android 다중 사용자 테스트 문서](https://source.android.com/docs/devices/admin/multi-user-testing)를 참고했다.

실제 사람이 처음 접했을 때의 Hold 이해도, 규칙 학습, 반복 구간의 재미·피로, 대사와 결말의 정서적 전달은 **미검증**이다. 에이전트의 소스 기반 경로 실행을 신규 사용자 성공률로 사용하지 않는다. 사람 모집·메시지·응답 수집은 하지 않았다.

기존 네이티브 미검증 항목인 보상 전 광고 X/Back 닫기, 실제 no-fill 및 fullscreen 표시 실패, 최초 미결정 동의 거절, SDK 중복 이벤트 실제 재현, 실기기 성능·발열은 그대로 미검증이다. 이번 Editor 대역·콜백 테스트를 그 항목의 네이티브 통과로 바꾸지 않는다. [기존 네이티브 QA](AndroidNativeAdsUmpQa-20260909.md), [기존 에뮬레이터 QA](AndroidEmulatorQa-20260908.md), [출시 체크리스트](../ReleaseChecklist.md)의 별도 조건도 남아 있다. 릴리스 서명, 실제 광고 ID, AAB 생성, 배포·스토어 제출·병합은 수행하지 않는다.
