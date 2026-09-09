# Curio Clerk Android 에뮬레이터 QA — 2026-09-08

상태: 수행 가능한 에뮬레이터 QA 완료. 발견한 **Critical 0건, Important 5건은 모두 수정·재검증**했다. 기존 미관 항목 1건은 Minor로 기록했다. **UMP 폼과 네이티브 광고 시청/보상은 미검증**이며 아래 환경·UI 제한을 따른다. 릴리스 승인 보고서는 아니다.

## 범위와 보존

- 프로젝트: `C:\Users\D-\Documents\Codex_Project\Orbital_Salvage_Shop` (요청에 포함된 이스케이프 표기 대신 실제 Unity 프로젝트 경로를 확인했다).
- 시작 커밋: `d15e39a7cb94330dddbda9623071d6e5a1eed8fc`. 로컬 main, origin/main 및 원격 `git ls-remote` 결과 일치. PR #3 병합 커밋 확인.
- 작업 브랜치: `codex/emulator-qa-20260908`.
- 기존 미커밋 22개 파일은 복사 및 SHA-256 목록으로 보존했고 최종 파일과 22/22 일치를 확인했다. `codex/local-main-backup-20260908`, `stash@{0}`와 `.worktrees/three-seal-dockets`를 보존했다.
- 기존 앱을 force-stop 후 내부 데이터 전체 tar, 외부 files 디렉터리, 설치되어 있던 APK를 백업하고 `adb install -r`로 업데이트했다. uninstall, pm clear, 저장 파일 편집, 에뮬레이터 초기화는 하지 않았다.
- 입력 APK: `Builds/Android/CurioClerk-qa.apk`, 100,731,162 bytes, SHA-256 `16778BF075739FEAF67F2BE477EDFC83C453A6FF0925BFA3B1F01E2D53C04F5A`.
- 업데이트 전 APK SHA-256: `1F7536376A206C6DAC4E74E0F7C6D62C85922AF5588A8AFACAA830F682FDCA70`.
- 입력 APK, 기존 앱 APK와 앱 데이터 백업: 로컬 ignored `Logs/EmulatorQa-20260908/baseline/`. 공개 저장소에 포함하지 않는다.
- 에뮬레이터: `127.0.0.1:5589`, Android 16/API 36, `sdk_gphone64_x86_64`, `x86_64,arm64-v8a`, 1080×2400 세로. ARM64 IL2CPP 앱은 Berberis 변환으로 실행된다. 성능은 실 ARM64 기기와 동등하다고 판단하지 않는다.
- 앱: `com.joyshu93.curioclerknightshift`, 실제 런처 `com.unity3d.player.UnityPlayerGameActivity`. 최초 일반 UnityPlayerActivity 지정은 존재하지 않는 Activity 오류였으며, manifest의 런처를 확인한 뒤 정상 실행했다.
- 실 광고 ID, 릴리스 서명, 릴리스 AAB 및 배포는 범위 밖이다.

## 직접 실행 결과

증거의 공통 기준 경로는 로컬 `Logs/EmulatorQa-20260908/`이다. 각 `capture`는 PNG, 해당 앱 PID의 logcat, 저장 JSON을 같은 이름으로 기록한다. 초기 01/02 logcat에는 기기 전체 로그가 포함되어 로컬에만 보관한다. `events.jsonl`에는 UTC 시각, 정확한 ADB 명령과 종료 코드가 있다. 파일명만으로 결과를 추정하지 말고 아래 판정을 따른다.

| 항목 | 결과 | 관찰과 증거 |
| --- | --- | --- |
| 데이터 유지 업데이트 | 통과 | `install -r` 성공. 기존 version 4/한국어/첫 사건 5단계 완료 기록 유지. `baseline/external-files/curio-clerk-save.json`, `02-offline-menu-ko-save.json`. |
| 오프라인 콜드 시작 | 통과 | 비행 모드 ON, Wi-Fi/data OFF. `02-offline-connectivity.txt`의 `Active default network: none`. 한국어 메뉴와 ‘기억하는 비’ 시작 버튼 확인: `02-offline-menu-ko.png`. |
| UMP 네트워크 실패 시 메뉴 접근 | 통과 | 앱 PID 로그에서 UMP `Error making request.`가 발생했지만 메뉴와 사건 대화 진입 가능. `02-offline-menu-ko-logcat.txt`, `03-rain-opening-ko.png`. |
| 한국어 대화/규칙/오답 | 통과(확인한 화면) | 대화 3개, 규칙·물건 설명·목적지, 오답 뒤 물건 유지와 정답 피드백 확인. `03`, `04`, `05`, `06`, `07` PNG. 전체 번역 품질 및 신규 사용자 이해도 판정은 아니다. |
| 하트 소진과 재시도 | 통과 | 우산을 보관실로 오분류하여 하트 3→0. `08-rain1-failed.png`. 게임 전경 복구 후 재시도해 하트 3과 첫 물건으로 복구: `12-retry-ko.png`~`15-retry-desk.png`. 광고·타이머 없이 이후 공명 완료. |
| 오프라인 다섯 교대 | 통과 | 입력 APK에서 1~3교대 공명 완료(`24`, `38`, `45`). QA-01~03 수정 APK에서 4·5교대 공명 완료(`66-rain4-completed-4-R.png`, `78-rain5-completed-4-R.png`). 완료 저장은 Rain 5개 공명 기록 및 사건 완료 ID를 포함한다. QA-04~05가 발견된 엔딩·보드는 별도 수정 후 재검증했다. |
| 저장·프로세스 종료·재실행 | 통과 | `am force-stop` 후 콜드 시작. `27-offline-relaunch-menu.png`에서 2/5, `48-online-menu.png`에서 4/5 이어하기, `84-completion-relaunch-ko.png`에서 사건 완료 유지. 기존 Ice 5개 기록을 보존했다. 진행 중인 교대의 프로세스 강제 종료 복구는 판정 범위에 포함하지 않는다. |
| 앱 전환 후 복귀 | 통과 | 2교대에서 jar를 Hold한 뒤 Home→Android Settings→게임 복귀. `33-rain2-hold-3-H.png`, `34a-background-settings.png`, `34b-resumed-hold.png`. 하트 3/코인 10/현재 moth/Hold jar 동일, 이어서 교대 완료. |
| 한국어/영어 설정 및 진행 화면 | 통과(확인한 범위) | 한국어 1·4교대, 영어 2·3·5교대 대화/규칙/결과와 4·5교대 양 언어 규칙을 직접 확인. `28-settings-ko.png`, `29-settings-en.png`, `30-menu-en.png`, `56`, `62`, `68`, `74`. 영어 설정은 콜드 시작 후 유지됐으며 종료 전 최초 언어인 한국어로 복구했다. 모든 번역 문장의 완전 검수나 신규 사용자 이해도 판정은 아니다. |
| 온라인 UMP 요청 실패 시 정상 이용 | 통과 | Wi-Fi ON으로 default network 100, IPv4 ping 8.8.8.8 성공. UMP는 fundingchoicesmessages.google.com:443 연결 실패. 그래도 메뉴·설정·저장 정상. `47-online-connectivity.txt`, `49-online-ipv4.txt`, `48-online-menu-logcat.txt`. |
| UMP 동의/거절/철회 폼 | 미검증 — 네트워크 제한 | 온라인 콜드 시작을 두 번 시도했지만 UMP 서버 IPv6 주소의 443 연결 실패로 폼 미표시: `48-online-menu-logcat.txt`, `50-online-retry-logcat.txt`. 동의 정보 삭제·지역 강제·동의 우회는 하지 않았다. 서버 도달 가능한 에뮬레이터 네트워크와 테스트용 동의 메시지 환경에서 후속 검증 필요. |
| 샘플 보상 광고 표시/취소/실패 | 미검증 — 현재 UI 제한 | `ShowResults`와 사건 결과에 광고 요청 버튼이 없다. 기존 PlayMode 테스트도 핵심 루프에서 `RewardedAdButton` 부재를 의도적으로 요구한다. `RequestReward`는 private이며 실제 UI에서 호출되지 않는다. 광고 버튼을 제품에 임의로 추가하지 않았다. |
| 보상 금액/중복 방지 | 자동 검증 통과, 네이티브 미검증 | 최종 EditMode 164/164, PlayMode 123/123. 완료 보너스는 기본 교대 코인만큼 1회 추가, 부활 1회, 중복 Earned 무시, Dismissed/Failed/Unavailable 무보상, 동의 철회 및 지연 콜백 검증 포함. 실제 광고 시청 성공이나 Android 콜백 타이밍 통과로 해석하지 않는다. |
| 최종 APK 5교대 다시보기 | 통과 | `89`, `91`, `93`, `95`, `97`의 각 마지막 PNG에서 공명 완료. QA-01~05가 모두 포함된 APK에서 한국어 전 구간을 재완주했다. |
| 최종 엔딩 및 완료 보드 | 통과(직접 확인 범위) | 한국어 엔딩의 실제 사건명·종이 물고기·보드 복귀 확인: `97-final-replay-rain5-18-R.png`, `98-final-replay-outro-3-C.png`. 양 언어 보드에서 두 완료 카드와 유틸리티 버튼 비겹침: `99-final-board-ko.png`, `102-final-board-en.png`. Rain 다시보기 버튼의 실제 터치로 `87` 시작 확인. 영어 엔딩은 PlayMode 검증이다. |
| 최종 APK 저장·재실행·중복 기록 방지 | 통과 | 다시보기 이전 `84`와 마지막 콜드 시작 이후 `107-final-relaunch-menu-ko-save.json`의 전체 JSON이 동일하다. 기존 Ice 5개, Rain 공명 5개, 두 사건 ID가 유지되며 다시보기로 코인·기록이 추가되지 않았다. `final-verification.json`. |
| 실행 안정성 | 관측 범위 통과 | 게임 PID logcat 277개에서 Fatal/ANR/대표 미처리 Unity 예외 패턴 0건(`runtime-log-scan-final.json`). 최종 종료 이력은 앱 강제 종료·패키지 업데이트 및 WebView 정리이며 crash/ANR 항목 없음(`final-process-exits.txt`). 장시간 안정성·성능 보증은 아니다. |

## 조사 및 수정

### QA-01 — Important: 이전 광고 요청 콜백이 다음 요청을 완료함

- 재현: 요청 A 완료 또는 동의 철회 → 요청 B 시작 → A의 지연 Earned/Failed 콜백 호출. 기존 공통 콜백이 B의 active request를 가져가 완료했다.
- 실패 증거: `EditMode-red.xml`, 전체 164개 중 신규 3개 실패(161 통과).
- 최소 수정: `RewardedAdService`가 각 Show 요청의 version을 캡처하고 현재 version과 다른 콜백을 무시한다.
- 재검증: `EditMode-green.xml` 164/164, `PlayMode-green.xml` 116/116 및 최종 전체 164/123 통과. 현재 요청의 Earned 두 번에 완료 1회, 이전 요청 결과 무시. 독립 리뷰 actionable finding 없음.
- 범위: replaceable service 계약 결함이다. Google Android adapter 자체에도 ad instance 필터가 있으므로 실제 에뮬레이터에서 이 결함으로 중복 보상을 관측했다고 주장하지 않는다.

### QA-02 — Important: 5·6줄 규칙이 패널 밖으로 넘침

- 재현: 영어 Remembering Rain 2교대 진입. 5번째 기본 규칙이 RulesPanel 바닥을 넘어 다음 물건 영역에 닿음. `32-rain2-intro-2-C.png`, `33-rain2-hold-3-H.png`. 4교대도 `52-rain4-before-fix-2-C.png`에서 동일하게 확인.
- 실패 증거: `PlayMode-layout-red.xml`, 영어·한국어 2개 실패. 1080×1920 기준 필요한 높이 173.77, 할당 137.28.
- 최소 수정: 추가 규칙당 세로 공간을 확보하고 다음 물건과 현재 물건 카드 상단을 함께 이동. 24pt 규칙 글자 크기 유지.
- 재검증: `PlayMode-layout-green.xml` 2/2 및 최종 전체 PlayMode 통과. 양 언어의 5·6줄을 1080×1920 및 1080×2400 논리 영역에서 검사하며 영역 넘침, preview 겹침, 카드 크기 역전도 확인. QA-01~03 APK의 4교대 영어 `56-rain4-fixed-en-2-C.png`, 한국어 `62-rain4-fixed-ko-2-C.png`, 5교대 한국어 `68-rain5-fixed-ko-3-C.png`와 영어 `74-rain5-fixed-en-2-C.png`에서 실제 패널 안에 모든 규칙이 들어갔다. 최종 APK의 4·5교대 한국어도 `94-final-replay-rain4-intro-5-C.png`, `96-final-replay-rain5-intro-5-C.png`에서 재확인했다. 카드·버튼이 읽히며 서로 겹치지 않는다.

### QA-03 — Important: 기본 규칙이 항상 보관실로 표시됨

- 재현: Remembering Rain 3교대 첫 rain jar. 화면은 `Otherwise → Storage`지만 실제 정답은 Vault이며 V 입력으로 하트 감소 없이 다음 물건 진행. `41-rain3-intro-2-C.png`, `42-rain3-docket1-1-V.png`. 4교대 기본 정답 Repair에도 같은 고정 문구가 사용됐다.
- 실패 증거: `PlayMode-fallback-red.xml`, 영어·한국어 2/2 실패. 기대 표기는 VAULT/봉인고이며 실제 문구는 Storage/보관실이었다. 최초 테스트 초안의 명칭을 프로젝트 실제 용어로 정정한 뒤 구현을 바꾸기 전에 다시 실패를 확인했다.
- 최소 수정: 양 언어의 기존 fallback 문구를 `Otherwise → {0}` / `그 외 → {0}`으로 바꾸고 실제 rule.Destination의 번역을 전달한다. 규칙 판정과 사건 데이터는 변경하지 않는다.
- 재검증: 최종 전체 PlayMode 123/123에 양 언어의 3·4교대 fallback 테스트 포함, 모두 통과. QA-01~03 APK에서 `Otherwise → REPAIR` / `그 외 → 수리실`을 직접 확인했다(`56`, `62`). 4교대 기본 규칙 물건인 backward candle과 rusty comet를 R로 처리하고 하트 3 유지, 교대 공명 완료(`64-rain4-docket2-2-R.png`, `65-rain4-docket3-2-R.png`, `66-rain4-completed-4-R.png`). 최종 APK의 3교대 다시보기에서도 `그 외 → 봉인고`로 바로잡힌 안내를 직접 확인했다(`92-final-replay-rain3-intro-5-C.png`). 독립 코드 리뷰 actionable finding 없음.

### QA-04 — Important: Remembering Rain 결말에 첫 사건 내용이 표시됨

- 재현: 5교대를 끝내면 저장에는 remembering-rain 완료가 기록되지만 화면에 First Incident Resolved, 얼음/우산, 이전 사건 예고가 표시됨. `78-rain5-completed-4-R.png`, `79-rain-final-outro-3-C.png`. 완료 저장 자체는 정상.
- 실패 증거: `PlayMode-ending-red.xml` 영어·한국어 2/2 실패. 기대 CASE RESOLVED/사건 해결 대신 First Incident Resolved/첫 사건 해결.
- 수정: 첫 사건의 전용 연출은 해당 사건에서만 사용하고, 다른 사건은 기존 사건 해결 문구·실제 사건명·최종 교대의 대표 물건을 표시한다. 새 그림이나 서사를 생성하지 않는다.
- 재검증: 최종 PlayMode 123/123에서 양 언어의 실제 Rain 마지막 교대 완료, 사건명·종이 물고기·보드 복귀 및 Ice 전용 그림 부재 확인. 기존 Ice 결말 테스트도 통과. 최종 APK의 한국어 전 구간 다시보기 후 `97-final-replay-rain5-18-R.png`, `98-final-replay-outro-3-C.png`에서 사건 해결/기억하는 비/종이 물고기/올바른 후일담과 보드 복귀를 직접 확인했다.

### QA-05 — Important: 두 사건 완료 후 다시보기 카드가 메뉴 버튼에 가려짐

- 재현: Remembering Rain 완료 후 사건 보드로 복귀. 두 번째 완료 카드와 Replay Case 버튼이 Casebook/Free Shift 뒤에 가려짐. `80-rain-completed-board-en.png`.
- 원인: 완료 카드가 아래로 추가되지만 아래 유틸리티 버튼과 현재 사건 카드가 고정 배치돼 두 완료 사건 상태를 수용하지 못함.
- 실패 증거: `PlayMode-board-red.xml` 1/1 실패. 두 번째 완료 카드의 하단 0.305가 메뉴 버튼 상단 0.35보다 낮아 겹친다.
- 최소 수정: 완료 카드 수에 필요한 높이만큼 현재 사건 카드의 하단과 완료 카드 영역을 위로 배치한다. 두 번째 완료 카드, 다시보기 버튼, 수집 도감/자유 교대/설정이 겹치지 않는 양 언어 검사를 추가했다. 현재 공개 콘텐츠의 완료 가능 사건은 두 개다.
- 재검증: 최종 PlayMode 123/123에서 두 언어의 완료 카드/유틸리티 버튼 비겹침 검사가 통과했다. `86-final-board-ko.png`에서 Rain 다시보기 버튼을 터치해 `87-final-replay-rain-intro.png`로 진입했고 다섯 교대를 마쳤다. 완료 후 `99-final-board-ko.png`, `102-final-board-en.png`에서 두 언어의 완료 카드와 다시보기 버튼이 메뉴 버튼에 가려지지 않음을 직접 확인했다.

### QA-06 — Minor: 영어 예고 카드 제목과 그림 사이 여백이 좁음

- 두 사건 완료 후 영어 보드의 One Minute Ahead 제목이 촛불 그림에 가깝게 배치된다. 수정 전 `80-rain-completed-board-en.png`와 최종 `102-final-board-en.png`에서 같은 현상이 있다.
- 제목은 읽을 수 있으며 비활성 예고 카드의 미관 항목이다. 완료 카드·버튼 접근·Rain 진행에는 영향이 없다. 이번 Critical/Important 최소 수정 범위에서 남겼으며 다음 사건 카드의 시각 다듬기 때 제목과 그림 영역의 간격을 조정할 수 있다.

## 자동 검증 및 APK 이력

최종 QA-01~05 코드에서 **EditMode 164/164, PlayMode 123/123 — 총 287개** 통과. 실패·건너뜀 0. PlayMode 실행 시간 360.655초. `scripts/test-unity.ps1`의 전체 결과를 `EditMode-final.xml/.log`, `PlayMode-final.xml/.log`로 보존했다. 독립 에이전트 코드 리뷰에서 최종 다섯 수정에 대한 actionable finding은 없었다.

최종 APK 빌드도 Unity/스크립트 종료 0으로 성공했다(`AndroidBuild-final.log`). 2026-09-08 18:09:50 KST, **100,731,700 bytes**, SHA-256 **`794DC04820CC5C16E10DEB709C324AB083719B1C1749C6A40E0FC6793D5EC46F`**. 경로 `Builds/Android/CurioClerk-qa.apk`. `apk-inspection.json`, `apk-badging.txt`, `apk-manifest.txt`, `apk-signature.txt`는 이 최종 파일의 API 29/36, ARM64 IL2CPP, portrait, 샘플 ID, Android Debug 서명 확인 결과다. `85-final-install.txt`의 `install -r` 성공 후 실제 설치된 base.apk도 같은 SHA-256임을 확인했다(`final-installed-apk-sha256.txt`). 이 APK에서 다섯 교대 다시보기와 완료 저장 재실행까지 검증했다.

`scripts/build-android-dev.ps1`가 먼저 저장소의 `scripts/test-unity.ps1`를 실행한다. QA-01~03 코드에서 EditMode **164/164**, PlayMode **120/120** 통과, 실패·건너뜀 0. PlayMode 실행 시간 337.01초. 해당 중간 증거는 `PlayMode-fixes-1-3.xml/.log`로 분리했다. 신규 테스트 7개(광고 요청 3개, 양 언어 layout 2개, 양 언어 fallback 2개)의 구현 이전 실패를 보존했다. 이후 QA-04 결말 2개와 QA-05 보드 1개까지 총 신규 10개의 실패 증거를 보존했다. 테스트가 수행하지 않은 네이티브 광고 결과를 추정하지 않는다.

QA-01~03 수정 APK의 `scripts/build-android-dev.ps1`와 Unity 프로세스 종료 코드 0. 내부 `ProjectBuilder.BuildAll`과 콘텐츠 검증 후 ARM64 IL2CPP APK 생성 성공. `AndroidBuild-fixes-1-3.log` 보존. `inspect-qa.ps1`가 manifest/API/portrait/debuggable/ARM64 IL2CPP/샘플 ID/Android Debug 서명을 다시 확인했다.

- QA-01~03 APK: **100,735,158 bytes**, 2026-09-08 17:29:12 KST. 로컬 `CurioClerk-fixes-1-3.apk`로 보존. QA-04~05 확인으로 최종 빌드 판정을 보류했다.
- SHA-256: `7D7276140CDE490077EB26DC3A9B6BB3AC221503936848F3165AF027A0F5A03F`.
- 패키지/버전: `com.joyshu93.curioclerknightshift`, `1.0.0 (10000)`, API 29/36, ARM64, IL2CPP, portrait.
- 앱 ID: Google 샘플 `ca-app-pub-3940256099942544~3347511713`. rewarded ID: `ca-app-pub-3940256099942544/5224354917`. 검사한 player metadata/native/assets에서 비샘플 ID 없음.
- `53-update-install.txt`: 동일 에뮬레이터에 다시 `install -r` 성공. 최종 입력 APK와 설치 APK를 혼동하지 않도록 두 해시를 별도로 기록했다.

## 환경 기록

- 첫 Unity 테스트 실행은 샌드박스의 라이선스 IPC 거부 및 캐시 접근 오류로 테스트 이전에 멈췄다. 해당 작업에서 시작한 Unity PID만 종료하고 `EditMode-sandbox-license.log`를 보존한 뒤 승인된 스크립트를 권한 있는 환경에서 재실행했다. 다른 프로젝트의 기존 Unity Editor는 종료하지 않았다.
- QA 중 에뮬레이터가 다른 앱으로 전환되어 입력을 중단했다. 사용자가 다른 작업에 다른 에뮬레이터 사용을 요청했다고 확인한 뒤 5589를 재사용했다. `09-rain1-retry.png`는 게임 증거가 아니며 다른 앱의 화면은 보고서에 삽입하지 않는다. 모든 후속 입력 직전에 Curio Clerk 전경을 검사했다.
- 기본 ADB 서버 연결이 여러 번 소실돼 별도 서버 포트 5059에서 동일한 5589 에뮬레이터에 연결했다. 에뮬레이터 또는 다른 작업의 서버를 종료·초기화하지 않았다.
- 26/47/106 등 일부 콜드 시작 직후 캡처는 검은 로딩 화면이다. 이후 실제 메뉴 캡처를 판정 근거로 사용한다. 최종 재실행 근거는 `107-final-relaunch-menu-ko.png`다.

## 재현용 실제 입력 순서

R=수리실, S=보관실, V=봉인고, H=Hold, C=대화 계속. 각 입력 사이 화면 전환을 기다렸고 PNG·PID logcat·save JSON을 기록했다. 규칙 우선순위와 소스를 참고했으므로 신규 사용자의 이해도나 재미 검증은 아니다.

| 교대 | 성공 경로(첫 대화 종료 이후) | 주요 증거 prefix |
| --- | --- | --- |
| 1 | V H R S / C R S V / C R S H V / C S V R | 16~24 |
| 2 | R V H S / C R S V / C R S H V / C S V R | 33~38 |
| 3 | V S H R / C V R S / C R V H S / C V S R | 42~45 |
| 4 | R S H V / C R S V / C R V H S / C S V R | 63~66 |
| 5 | R H S V / C S H R V / C R H S V / C S V R | 75~78 |

최종 APK 재현: 한국어 사건 보드의 두 번째 완료 카드에서 ‘사건 다시보기’를 누른다(`87`). 첫 교대의 시작 대화 3개를 넘긴 뒤 위 경로를 적용한다. 1~4교대 결과 대화 2개 → 다음 교대 → 시작 대화 2개를 넘기며 이어 간다. 마지막 결과의 대화 3개 이후 ‘사건 보드로 돌아가기’를 누른다. 최종 완료 증거는 순서대로 `89-final-replay-rain1-17-R.png`, `91-final-replay-rain2-17-R.png`, `93-final-replay-rain3-17-R.png`, `95-final-replay-rain4-17-R.png`, `97-final-replay-rain5-18-R.png`다. 각 순간의 저장·로그도 동일 prefix로 보존했다.

## 최종 보존 확인

`final-verification.json`의 14개 검사가 모두 참이다. 기존 22개 파일의 SHA-256, main/origin/main, 백업 브랜치와 stash, story worktree HEAD, 기존 사건 기록과 완료 ID, Rain 다섯 공명 기록, 한국어 복구, 다시보기 전후 전체 저장 동일, 네트워크 설정 복구 및 기존 릴리스 AAB 해시를 확인했다.

- 기존 사건 기록을 보존하면서 자연스러운 첫 완료로 Rain 기록 5개와 완료 ID가 추가되고 활성 사건·단계가 Rain 5로 진행됐다. 최종 다시보기는 이 완료 저장을 더 변경하지 않았다. `84-completion-relaunch-ko-save.json` = `107-final-relaunch-menu-ko-save.json`(JSON 전체 비교).
- 시작 및 종료 네트워크 설정: airplane=0, wifi=0, mobile-data=0. `baseline/*.txt`와 `final-airplane.txt`, `final-wifi.txt`, `final-mobile-data.txt` 일치. 최종에도 default network는 없다.
- 기존 `Builds/Android/CurioClerk.aab` SHA-256 `F29973BED5E02FB521C613979B574BA9E89180D8F959B5FBD70AE4D9E6CDFEA4` 유지. 이번에는 QA APK만 새로 빌드했다.
- `evidence-index.json`은 화면·로그·명령 시각·테스트 XML·영상 등 로컬 증거의 파일 크기와 SHA-256 목록이다. 기존 데이터 백업은 별도 `baseline/`에 보존했다. 원시 앱 데이터·로그·APK는 Git에 넣지 않는다.

## 광고 검증의 경계

샘플 ID는 Google의 [Unity 테스트 광고 문서](https://developers.google.com/admob/unity/test-ads)에 지정된 rewarded 단위다. 실제 동의 폼은 [UMP 가이드](https://developers.google.com/admob/unity/privacy)의 앱 메시지/지역 조건과 SDK 요청 성공이 필요하다. 이번 실행에서 폼 승인·거절·재표시는 관측하지 못했다. 현재 core-fun 화면이 광고 offer를 숨기는 것은 기존 PlayMode 계약이다. 제품에 광고 버튼을 임의로 추가하지 않았다.

후속 검증은 동일 에뮬레이터에서도 가능하다. UMP 서버에 도달하는 네트워크, 테스트 메시지 설정, 제품 동작과 분리된 승인된 QA 광고 진입 경로가 마련되면 샘플 광고 완주·중도 닫기·로드 실패·앱 복귀·정확한 보상 1회·동의 철회 후 금지를 실제 SDK로 확인해야 한다. 실 광고 ID나 릴리스 빌드는 필요 조건이 아니다.

## 화면 증거 바로보기

아래 링크는 이 작업 PC에 보존한 원본 캡처다. 로컬 ignored 증거는 Git 커밋에 포함하지 않는다.

| 화면 | 원본 PNG |
| --- | --- |
| 수정 전: 4교대 5번째 줄 넘침과 Storage 오표시 | [수정 전 영어](C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/Logs/EmulatorQa-20260908/52-rain4-before-fix-2-C.png) |
| 수정 후: 4교대 5줄과 REPAIR 표시 | [영어](C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/Logs/EmulatorQa-20260908/56-rain4-fixed-en-2-C.png) · [한국어](C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/Logs/EmulatorQa-20260908/62-rain4-fixed-ko-2-C.png) |
| 수정 후: 5교대 6줄 | [영어](C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/Logs/EmulatorQa-20260908/74-rain5-fixed-en-2-C.png) · [한국어](C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/Logs/EmulatorQa-20260908/68-rain5-fixed-ko-3-C.png) |
| 4교대 한국어 공명 결과 | [결과 화면](C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/Logs/EmulatorQa-20260908/66-rain4-completed-4-R.png) |
| 최종 APK: 3교대 기본 목적지 봉인고 | [정정된 규칙](C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/Logs/EmulatorQa-20260908/92-final-replay-rain3-intro-5-C.png) |
| 최종 APK: Rain 사건 해결과 실제 사건 그림 | [엔딩](C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/Logs/EmulatorQa-20260908/97-final-replay-rain5-18-R.png) · [보드 복귀 전](C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/Logs/EmulatorQa-20260908/98-final-replay-outro-3-C.png) |
| 최종 APK: 가려지지 않는 완료 카드·다시보기 | [한국어](C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/Logs/EmulatorQa-20260908/99-final-board-ko.png) · [영어](C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/Logs/EmulatorQa-20260908/102-final-board-en.png) |
| 최종 APK: 콜드 시작 후 완료 기록 유지 | [재실행](C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/Logs/EmulatorQa-20260908/107-final-relaunch-menu-ko.png) |

보조 영상은 `rain2-before-layout-fix.mp4`, `rain4-after-fix.mp4`, `rain5-after-fix.mp4`에 보존했다. 각 녹화는 ADB의 180초 구간 캡처이며 전체 5교대 연속 녹화가 아니다. 실제 단계별 판정은 PNG·save JSON·명령 시각을 함께 사용한다. `rain5-after-fix.mp4`는 QA-01~03 수정 빌드이며 QA-04 결말 결함이 남아 있는 시점이다.

최종 QA-01~05 빌드의 마지막 교대 녹화는 [rain5-final-all-fixes.mp4](C:/Users/D-/Documents/Codex_Project/Orbital_Salvage_Shop/Logs/EmulatorQa-20260908/rain5-final-all-fixes.mp4)다. 이 영상도 180초 제한의 보조 증거이며 최종 판정은 위 원본 PNG·저장·로그와 함께 읽는다.

## 남은 검증

- UMP 폼: 서버 연결이 성공하는 에뮬레이터 네트워크와 테스트 메시지 환경에서 표시·동의·거절·재표시·철회를 실제 확인해야 한다. 이번 두 온라인 시도는 연결 실패였으므로 폼 동작은 통과 처리하지 않았다.
- 네이티브 샘플 광고: 현재 제품 UI에 광고 요청 진입점이 없으므로, 승인된 QA 진입 경로에서 완주·중도 닫기·로드/표시 실패·앱 전환·보상 1회·철회 후 요청 금지를 실제 SDK로 확인해야 한다. 서비스 및 게임 보상 자동 테스트 통과와 구별한다.
- 사람 신규 사용자의 규칙 이해·서사 몰입, 전체 번역 감수, 실 ARM64 하드웨어 성능·발열은 이번 기능 QA의 판정 범위 밖이다. 실기기 연결은 이번 에뮬레이터 QA의 선행 조건으로 요구하지 않았다.
