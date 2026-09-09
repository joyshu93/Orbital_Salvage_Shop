# Curio Clerk 실제 GMA·UMP SDK QA — 2026-09-09

상태: 요청한 에뮬레이터 검증과 보고서 작성 완료. **실제 Google SDK에서 UMP 폼 표시·동의·거절·재표시·철회, 샘플 광고 완주·중도 종료·로드 실패·앱 복귀, 코인 2배/부활 보상 1회 및 재클릭 중복 방지를 확인했다.** 중도 종료는 영상 재생 중 Home→런처 복귀로 검증했다. 광고 자체 X 버튼을 통한 보상 전 취소, no-fill 및 네이티브 fullscreen 표시 실패는 아래 한계에 명시한다. 자동 테스트 성공을 네이티브 통과로 대체하지 않았다. 릴리스 승인·실기기 성능 검증이 아니다.

## 기준과 보존

- 실제 경로: `C:\Users\D-\Documents\Codex_Project\Orbital_Salvage_Shop`.
- 시작 브랜치/커밋: `codex/emulator-qa-20260908`, `86116ff`. 원격 `main`은 조회 시 `d15e39a`; 기준 수정은 아직 원격 main에 병합되지 않았지만 이 작업의 HEAD에는 포함돼 있다.
- 기존 미커밋 22개 파일을 복사하고 SHA-256 목록을 기록했다. 백업 브랜치·stash·story worktree의 시작 상태도 기록했다. 기존 QA APK를 별도 복사한 뒤 작업했다. 원시 로그·APK·앱 저장은 Git에 포함하지 않는다.
- 로컬 증거 루트: `Logs/NativeAdsQa-20260909/`. `baseline/dirty-sha256.json`, `branches.txt`, `stash.txt`, `worktrees.txt`, `story-status.txt`, `release-aab.json`을 보존 기준으로 사용한다.
- 실제 광고 ID, 릴리스 서명, AAB 생성, 배포, uninstall, `pm clear`, UMP `Reset()` 및 앱 저장 주입은 사용하지 않는다.

## 5589 식별과 이전 실행의 차이

시작 시 ADB 기본 포트 5037과 이전 QA 전용 포트 5059 모두 기기가 없었고, 5589 연결은 거부됐다. 다른 Android 작업은 별도 포트 5600 및 전용 ADB를 사용한다. 해당 작업의 프로세스·기기·데이터에는 입력하지 않았다.

처음에는 이름이 일치하는 `CurioClerk_API36` AVD를 기동했으나, AVD 속성·설치 APK·저장을 대조해 이전 QA 기기가 아님을 확인했다. 이 시도의 화면과 백업은 `baseline/`, `target-baseline/`, `01`~`02` 및 `alternate-avd-final-save.json`에 별도로 남겼으며, 요청 대상 기기의 통과 증거로 사용하지 않는다. 이 AVD의 게임 저장은 기동 전후 JSON 전체가 동일했다. 이 작업에서 기동한 프로세스만 종료했다.

이전 실행 기록에서 실제 명령을 찾았다 (`prior-emulator-launch-evidence.json`, 2026-09-08 07:01:26 UTC):

```text
-avd appointment_cost_pixel_6 -port 5588 -read-only
-no-window -no-audio -no-boot-anim -no-snapshot
-gpu swiftshader_indirect -http-proxy http://127.0.0.1:9 -no-metrics
```

실제 대상은 `appointment_cost_pixel_6`의 5588 콘솔/5589 ADB 인스턴스였다. 이전 실행은 읽기 전용이므로 종료된 인스턴스의 QA 진행 상태가 AVD 원본에 영속 저장됐다고 가정할 수 없다. 현재 원본 설치 APK SHA-256은 `1F7536376A206C6DAC4E74E0F7C6D62C85922AF5588A8AFACAA830F682FDCA70`으로 이전 보고서의 업데이트 전 APK와 정확히 일치한다. 현재 원본 저장에는 Ice 완료 기록이 있고, 9/8 종료 시 Rain 진행 기록은 이전 로컬 QA 증거에 남아 있다. 이를 현재 저장에 주입하지 않았다.

새 검증도 같은 AVD·포트의 읽기 전용 인스턴스로 수행한다. 대상 확인 후 `verified-baseline/`에 설치 APK, 내부 데이터 tar, 외부 files, 네트워크 설정을 새로 백업했다. 모든 설치는 `install -r`이다. `03` 이후만 해당 AVD의 증거다.

## UMP 네트워크 조사

9/8 실패 로그의 `fundingchoicesmessages.google.com/[IPv6]:443` 연결 오류는 마지막 접속 주소를 보여준다. IPv4 ICMP ping 성공만으로 HTTPS 도달성을 입증할 수 없다. 이전 명령에는 호스트의 `127.0.0.1:9`를 쓰는 차단 프록시가 있었다. Android의 [에뮬레이터 프록시 문서](https://developer.android.com/studio/run/emulator-networking-proxy)에 따르면 이 프록시는 TCP 계층에서 적용되고 HTTPS도 터널링한다. 따라서 Android Wi-Fi ON, default network 존재, ping 성공, Android `http_proxy=null`과 TCP 차단은 동시에 가능하다.

이번에는 해당 차단 프록시 없이 기동했다. DNS 변경, IPv6 비활성화, 인증서 검증 해제 및 동의 우회는 하지 않았다. 9/8 최종 기준 APK(`794DC04820CC5C16E10DEB709C324AB083719B1C1749C6A40E0FC6793D5EC46F`)를 동일 AVD에 `install -r`한 뒤 UMP가 서버 응답을 받아 `IABTCF_gdprApplies=0`, `UMP_consentModeValues=4444`를 저장했다 (`05-install-baseline-sdk-apk.txt`, `06-baseline-sdk-no-proxy-logcat.txt`, 2026-09-09 01:16:35 UTC). 기기 기본 네트워크는 Wi-Fi 100이다. 기존 게임 저장 JSON 전체도 설치 전후 동일하다. 이전 실패의 원인은 SDK 코드나 단순 IPv6 존재가 아니라, 해당 실행에 상속된 외부 TCP 차단 프록시로 판단한다. 새 QA APK의 폼·광고 결과는 별도로 기록한다.

## 실제 SDK 콜백 정지의 추가 원인

초기 QA APK `A49975F8351220381D1B55064B0B8B0144D195043EACE12BBCBD9F2998EF6849`에서는 네이티브 UMP가 01:23:24 UTC에 서버 응답을 저장했으나, 2분 이상 기다려도 C# 완료 콜백이 없었다. `10-nativeqa-status.png`에는 네이티브 상태 `NotRequired / canRequest=True`와 아직 허용되지 않은 앱 시험 세션이 함께 보인다. 이 빌드는 실제 광고 통과로 처리하지 않는다.

고정된 GMA Unity 11.3.0의 DLL과 공식 소스를 대조했다. [UMP Update](https://github.com/googleads/googleads-mobile-unity/blob/v11.3.0/source/plugin/Assets/GoogleMobileAds/Ump/Api/ConsentInformation.cs#L72-L94)는 `MobileAds.RaiseAction`으로 콜백을 전달한다. `RaiseAdEventsOnUnityMainThread=true`는 플래그만 설정하며, [RaiseAction](https://github.com/googleads/googleads-mobile-unity/blob/v11.3.0/source/plugin/Assets/GoogleMobileAds/Api/MobileAds.cs#L304-L319)이 사용하는 큐는 실행기를 자동 생성하지 않는다. 기존 앱은 UMP 완료 후에 광고를 초기화하므로 실행기가 만들어지지 않는 순환 대기가 있었다.

수정은 UMP 요청 전에 [MobileAdsEventExecutor.Initialize](https://github.com/googleads/googleads-mobile-unity/blob/v11.3.0/source/plugin/Assets/GoogleMobileAds/Common/MobileAdsEventExecutor.cs#L33-L43)로 Unity 콜백 실행기만 생성하는 것이다. 이 메서드는 숨겨진 GameObject를 만들며 네이티브 광고 초기화나 요청을 수행하지 않는다. 실제 SDK의 worker→Unity 큐를 사용하는 PlayMode 회귀 테스트로 콜백의 정확한 1회 전달·메인 스레드·광고 미초기화를 확인한다. 별도로 QA 화면을 시작 동의 완료 전에 열었을 때 요청 중첩과 오래된 권한 상태가 생기는 경합도 실패 테스트 후 보완한다.

## QA 전용 진입점

- `scripts/build-android-dev.ps1` → `ProjectBuilder.BuildAndroidDevelopment`에서만 `CURIO_NATIVE_ADS_QA`를 플레이어 빌드에 전달한다. Android + Development + 해당 심볼 + 비오프라인 조건에서 메뉴에 QA 버튼이 생긴다.
- 릴리스 및 오프라인 QA 플레이어에는 QA 화면/시험 세션이 포함되지 않는다. Editor에는 자동 테스트를 위해 직접 호출 가능한 화면이 있지만 일반 제품 메뉴의 QA 버튼은 없다.
- 완료/실패 시험 교대는 실제 `ShiftSession`을 메모리에 생성한다. 완료 교대는 정확한 분류 3회로 40코인이 되고, Earned 처리 시 80코인으로 한 번만 증가한다. 실패 교대는 오분류 3회로 하트 0이며 Earned에서 하트 1로 한 번 부활한다. 저장/진행 서비스에는 연결하지 않는다.
- 광고는 제품과 같은 `GoogleRewardedAdService` → `RewardedAdService` → 실제 Android GMA 클라이언트를 사용한다. QA에서도 UMP `CanRequestAds()`가 요청 권한을 결정한다. 광고 준비/로드 오류/네이티브 Earned/닫힘/실패와 애플리케이션 보상 횟수는 `[CurioNativeQa]` 로그로 구분한다.
- UMP 기본 갱신, 테스트 EEA 지역의 갱신·필수 폼 표시, 개인정보 옵션 재표시가 있다. `Reset()`은 제공하지 않는다. EEA는 시험 장치에서만 적용되며, 이 작업의 에뮬레이터는 [UMP 2.2.0 이후 자동 시험 장치](https://developers.google.com/admob/android/privacy/release-notes)다. 실기기용 해시 ID 등록은 이 범위에 없다.
- QA 요청 차단 버튼은 서비스 권한 차단 시험이다. UMP 폼에서의 동의 철회와 같은 것으로 판정하지 않는다. 거절 후에도 SDK가 `CanRequestAds=true`를 반환할 수 있으므로 이를 개인화 동의로 해석하지 않는다.

## 검증 결과

| 항목 | 판정 | 증거/한계 |
|---|---|---|
| 신규 시험 세션 RED | 확인 | `EditMode-red.xml`: 기존 164 통과, 신규 7 실패. 구현 부재를 확인 후 구현. |
| QA 화면 RED | 확인 | `PlayMode-red.xml`: 신규 1 실패, QA 진입점 부재. 샌드박스 라이선스 IPC 실패와 재시도 로그는 별도 보존. |
| 시작 동의 경합 RED | 확인 | `PlayMode-startup-red.xml`: 3개 중 2개 실패. QA가 두 번째 UMP 요청을 만들고, 시작 완료 후 광고 placement가 null인 회귀를 확인. |
| 실제 SDK 콜백 큐 RED | 확인 | `PlayMode-dispatch-queue-red.xml`: 1개 실패, 전달 기대 1회 / 실제 0회. 광고 초기화 없이 pinned SDK `RaiseAction`을 worker에서 호출. |
| EditMode | 통과 | `EditMode-final.xml`: 171/171, 01:38:03–01:38:07 UTC. 실제 광고 검증 결과가 아니다. |
| PlayMode | 통과 | `PlayMode-final.xml`: 127/127, 01:39:03–01:45:04 UTC. 실제 SDK 큐·시작 동의 경합·완료/부활 보상 후 전체 게임 저장 불변 회귀 포함. |
| 원격 텔레메트리 검사 | 통과 | 최종 첫 빌드는 기존 GMA `Common.dll`이 명시 허용 목록에 없어 중단됐다. fixture에 해당 참조를 먼저 추가해 `TelemetryGate-common-red.txt`의 실패를 확인한 뒤 기존 패키지의 정확한 DLL 이름만 허용했다. `TelemetryGate-common-green.txt` 제어 변이 검사와 `TelemetryGate-release-green.txt` Release 모드 검사가 모두 exit 0. 이후 테스트가 아닌 APK 빌드 단계만 재실행. |
| QA APK | 통과 | `AndroidBuild-final.log`: BuildAll 및 Android Development 빌드 exit 0. `apk-inspection.json`: ARM64 IL2CPP, portrait, API 29/36, debuggable, Android Debug, 샘플 ID만 포함. 설치 APK 해시도 동일. |

### 실제 Android SDK 결과

아래 시각은 기기 로그의 UTC다. 공통 로그는 [native-sdk-timeline.txt](../../Logs/NativeAdsQa-20260909/native-sdk-timeline.txt), 각 화면의 같은 접두사 `-logcat.txt`에는 전체 SDK 오류와 Android 로그가 있다. 정지된 Unity 플레이어의 메인 스레드 콜백은 광고에서 복귀할 때 연속 처리되므로 `fullscreen opened` 로그 시각을 영상 시작 시각으로 해석하지 않는다. 시작은 `GMA show`, 실제 화면과 ADB 명령 기록으로 대조했다.

| 항목 | 판정 | 실제 관측과 증거 |
|---|---|---|
| 시작 UMP→GMA 초기화·로드 | 통과 | 01:53:10 UMP 업데이트/폼 완료 콜백, CanRequest=True → 01:53:12 GMA initialized → 01:53:19 load success. `13-final-menu-logcat.txt`, [준비 화면](../../Logs/NativeAdsQa-20260909/15-final-qa-ready.png). 이전 콜백 순환 대기가 해소됐다. |
| 샘플 광고 완주 | 통과 | 01:53:56 show, [Test Ad / Reward granted 화면](../../Logs/NativeAdsQa-20260909/17-rewarded-test-ad.png), X로 닫은 뒤 01:54:23 실제 earned callback(amount=10, type=coins), terminal=Earned. 광고의 Install 링크는 누르지 않았다. |
| 코인 보상 정확히 1회 | 통과 | request=2, coins 40→80, awards=1, completions=1. [19-earned-once.png](../../Logs/NativeAdsQa-20260909/19-earned-once.png). SDK의 샘플 reward amount 10을 게임 코인에 그대로 더하지 않고 실제 도메인의 2배 규칙을 적용했다. |
| 재클릭·완료 후 복귀 중복 방지 | 통과 | 01:55:01/02 두 번의 재클릭은 claimed=True로 거절; 추가 native show/보상 없음. Home→런처 복귀 후에도 80/1/1 유지. `20`~`22` 화면·로그. 이미 소비한 교대는 새 시험 교대 준비 전 재보상되지 않는다. |
| 부활 정확히 1회 | 통과 | 01:59:47 실패 교대 coins=0/hearts=0 → request=10 Earned → 02:00:35 hearts=1, awards=1, completions=1. [35-revive-earned-once.png](../../Logs/NativeAdsQa-20260909/35-revive-earned-once.png). 02:01:20 재클릭도 claimed=True로 거절. |
| 영상 도중 앱 전환·중도 종료 | 통과(명시한 경로) | 02:05:26 request=18 show. AdActivity 전경 확인 직후 Home → 런처 Activity로 복귀. 사전 screencap에는 아직 QA 이전 프레임이 남았고, Home 전환 중 캡처에는 광고 콘텐츠가 보인다. 02:05:29 실제 terminal=Dismissed, Earned 콜백 없음, coins=40, awards=0, completions=1. [표시 요청 직후 이전 프레임](../../Logs/NativeAdsQa-20260909/49-video-before-home.png), [광고에서 Home으로 전환](../../Logs/NativeAdsQa-20260909/49-home-during-video.png), [결과](../../Logs/NativeAdsQa-20260909/51-video-return-result.png), `49-video-return-launch.txt`. 프로세스 종료·강제 콜백·저장 변경 없이 재현했다. |
| 네이티브 로드 실패·무보상 | 통과 | Wi-Fi OFF/default network none. 02:01:22 GMA load callback success=False, domain=com.google.android.gms.ads, Code=0, googleads.g.doubleclick.net 이름 해석 실패. 샘플 표시/결과 확인 → request=13 Failed, coins=40, awards=0, completions=1. [실제 오류 화면](../../Logs/NativeAdsQa-20260909/37-load-offline-result.png), `37-offline-connectivity.txt`, `39-failed-no-reward-settled-logcat.txt`. Unavailable만 확인한 결과가 아니다. |
| 네트워크 복구 | 통과 | Wi-Fi ON 후 광고 해제/다시 로드 → 02:02:41 generation=19 success=True. [40-network-recovered.png](../../Logs/NativeAdsQa-20260909/40-network-recovered.png). 실패 결과 소비 때의 서비스 자동 재시도 1회도 로그에 구분돼 있다. |
| UMP EEA 폼 표시 | 통과 | 02:02:54 eea=True → 02:02:56 실제 업데이트 성공, CanRequest=False. [42-ump-eea-result.png](../../Logs/NativeAdsQa-20260909/42-ump-eea-result.png)의 “Publisher Test Ads” 폼 표시. 폼 처리 중 광고 요청 권한을 중단했다. Google 샘플 app ID에 시험 메시지가 제공돼 외부 계정 설정을 추가할 필요가 없었다. |
| UMP 동의 | 통과 | Consent 선택 → 02:03:27 폼 완료 error 없음, status=Obtained/options=Required/CanRequest=True. [44-ump-consent-granted.png](../../Logs/NativeAdsQa-20260909/44-ump-consent-granted.png), `44-consent-prefs.xml`. 이후 샘플 로드 성공. |
| UMP 재표시·거절·철회 | 통과 | 개인정보 폼 재표시 → [46-ump-privacy-form.png](../../Logs/NativeAdsQa-20260909/46-ump-privacy-form.png)에서 Do not consent 선택. 02:04:20 실제 privacy options 완료 콜백 error 없음. 기존 동의의 목적 비트 11→0, 공급업체 동의 비트 127→0. `48-withdraw-prefs.xml`, [ump-consent-comparison.json](../../Logs/NativeAdsQa-20260909/ump-consent-comparison.json). 최초 선택은 동의였으며 거절은 재표시한 폼에서 검증했다. |
| 철회 상태 재확인 | 통과 | 폼을 다시 열고 Manage options 진입: [55-withdraw-options-visible.png](../../Logs/NativeAdsQa-20260909/55-withdraw-options-visible.png)에서 동의 스위치 OFF 확인. 선택을 바꾸지 않고 Confirm choices → 02:06:37 완료. |
| QA 요청 권한 차단 | 통과(UMP 철회와 별개) | 02:07:05 permission=False → 02:07:06 show rejected allowed=False, native show 없음. `57-qa-permission-blocked-logcat.txt`. 기본 UMP 갱신으로 정상 경로 복구 후 02:07:11 재로드 성공. |

**거절/철회의 의미:** 실제 SDK는 거절 후에도 status=Obtained, CanRequest=True를 반환했다. Obtained는 선택을 수집했다는 상태이며 개인화 동의 승인으로 표시하지 않는다. 목적/공급업체 동의 비트는 0이지만 정당한 이익 비트는 각각 6/114로 유지됐고, 폼 화면에서도 별도 스위치임을 확인했다. 따라서 “모든 처리 또는 모든 광고 요청이 차단됨”을 통과로 주장하지 않는다. 이 구현은 UMP의 `CanRequestAds()`를 광고 요청 기준으로 사용한다. 별도의 QA 차단 버튼은 이 권한이 false일 때의 요청 금지만 검증한다.

### 남은 검증 한계

- **광고 자체 X/Back을 통한 보상 전 취소:** 이번 Google 샘플 크리에이티브는 재생 초반과 약 5초 시점에 X가 보이지 않았고 Back이 재생을 중단하지 않았다 (`29-early-ad-before-back.png`, `29-early-ad-after-back.png`, `33-timed-ad-5s.png`, `33-timed-ad-after-back.png`). 나중에 닫은 시도는 모두 Earned였으므로 Dismissed로 계산하지 않았다. 재생 중 Home→런처 복귀의 실제 Dismissed는 위와 같이 별도로 통과했다. X 경로를 추가 통과시키려면 **보상 전에 닫기 버튼을 제공하는 Google 시험 크리에이티브**가 필요하다. 같은 Back 실패를 더 반복하지 않았다.
- **no-fill / fullscreen 표시 실패:** 이번 실패 관측은 네트워크로 인한 LoadAdError이며 no-fill(Code=3) 또는 `OnAdFullScreenContentFailed`가 아니다. 이 두 이벤트를 실제 SDK가 반환하는 추가 재현 조건이 필요하다. 샘플 정상 로드, 미준비 상태, 콜백 모의 주입을 표시 실패 통과로 대체하지 않는다.
- **SDK 중복 이벤트 자체의 강제 재현:** 실제 시청마다 Earned/terminal/보상을 각각 1회 관측했고 재클릭과 복귀도 검증했다. 네이티브 SDK가 중복 Earned를 내보내도록 조작하지 않았다. 중복·지연 콜백과 권한 취소의 방어 로직은 별도의 자동 테스트 증거다.
- **QA 진단 화면:** 긴 SDK 오류 JSON은 상태 텍스트 영역을 넘을 수 있다 (`37` 화면). 버튼 조작과 로그 수집은 가능했고 결과 판정에는 전체 `-logcat.txt`를 사용했다. 제품 UI/릴리스에는 이 화면이 포함되지 않는다.
- 실기기, 릴리스 광고 ID, AAB/릴리스 서명/배포, 최초 설치의 미결정 상태에서 거절, 모든 지역/언어의 서버 제공 폼은 이번 통과 범위에 포함하지 않는다.

## 최종 APK와 보존 결과

- APK: [Builds/Android/CurioClerk-qa.apk](../../Builds/Android/CurioClerk-qa.apk), 100,738,890 bytes, `1.0.0 (10000)`, 빌드 파일 시각 2026-09-09 01:51:29 UTC.
- SHA-256: `E242463BA8BBC6EEF6F55635DE28A316F52E3EABB28E95BC3C9A53040CF10DBB`. 설치 후 다시 가져온 `installed-final.apk`와 동일하다.
- Google 샘플 app `ca-app-pub-3940256099942544~3347511713`, rewarded `ca-app-pub-3940256099942544/5224354917`. 샘플 외 ID 없음. 패키지·SDK·종속성 버전은 기존 pin을 유지했다.
- [preservation-final.json](../../Logs/NativeAdsQa-20260909/preservation-final.json): 원래 미커밋 **22/22 파일 SHA-256 동일**, 모든 로컬 브랜치·stash·worktree 목록 및 story worktree 상태 동일. 기준 HEAD `86116ff`를 유지했다. 새 커밋·병합·push는 하지 않았다.
- 기존 게임 저장의 **JSON 전체가 전후 동일**하다. 시험 보상은 저장에 적용되지 않았다. AVD 원본은 read-only 인스턴스로 보호했다. UMP 선택 저장은 위에서 명시한 실제 폼 조작에 의한 변경이며 초기화/삭제/백업 주입은 하지 않았다.
- 기존 릴리스 AAB SHA-256 `F29973BED5E02FB521C613979B574BA9E89180D8F959B5FBD70AE4D9E6CDFEA4` 그대로다.
- 종료 시 앱은 메뉴이며 Wi-Fi=0, mobile_data=0, airplane_mode_on=0으로 시작 설정과 같다. 따라서 다음 온라인 SDK 검증 시 **Wi-Fi를 켜야 한다**. 전용 ADB 5059 / 기기 127.0.0.1:5589는 연결 상태로 남겼다. 다른 작업의 5600 기기는 조작하지 않았다.
- 변경 범위의 whitespace 검사는 통과했다. 전체 `git diff --check`에 남는 기존 22개 serialized 파일의 공백은 원본 보존을 위해 고치지 않았다.
- 원시 증거 색인: [evidence-sha256.json](../../Logs/NativeAdsQa-20260909/evidence-sha256.json), 명령/시각/종료 코드: [events.jsonl](../../Logs/NativeAdsQa-20260909/events.jsonl). 로그·APK·내부 데이터·TCF 문자열은 로컬 `Logs/`에만 보관하고 공개 브랜치에 추가하지 않는다.

## 재현 절차

1. 기존 에뮬레이터/ADB 포트의 소유·충돌을 확인한다. 위 AVD를 5588 콘솔/5589 ADB, 읽기 전용·headless로 실행한다. UMP·샘플 광고 검증에는 차단 프록시를 지정하지 않는다.
2. `scripts/build-android-dev.ps1`로 테스트 및 QA APK를 생성한다. API 29/36, ARM64 IL2CPP, portrait, debuggable, Android Debug 서명, Google 샘플 app/rewarded ID를 검사한다.
3. 앱과 데이터 백업 후 전용 ADB 서버 5059에서 `adb -P 5059 -s 127.0.0.1:5589 install -r <QA APK>`를 실행한다. 설치된 base.apk 해시를 빌드 산출물과 비교한다.
4. 게임 메뉴의 **QA · 광고 / 동의**로 진입한다. 시작 UMP 요청이 끝날 때까지 기다린다. 기본 갱신과 EEA 폼, 개인정보 옵션 재표시를 각각 관측하고 실제 선택 및 반환 상태를 기록한다.
5. 완료 시험 교대를 준비하고 준비=True에서 샘플 표시를 누른다. 완주와 중도 닫기는 별도 교대로 검증한다. SDK Earned와 fullscreen terminal, 세션 코인/보상/완료 횟수를 화면·로그로 대조한다.
6. 중도 종료는 광고 표시 직후 AdActivity 전경을 확인하고, 영상이 끝나기 전에 Home→런처 복귀를 사용한다. `node Logs/NativeAdsQa-20260909/device.cjs resume-during-video`가 이번 재현 명령이다. 실제 terminal=Dismissed/Earned 부재/무보상을 대조한다. 다른 앱이 전경이면 입력을 중단한다. 샘플 광고의 Install 링크는 누르지 않는다.
7. 로드 실패는 네트워크 OFF 후 광고 해제/재로드, SDK 실패 콜백 관측, 결과 확인 순서로 한다. `Unavailable`만으로 네이티브 로드 실패를 통과 처리하지 않는다. 네트워크 복구 후 수동 재로드한다.
8. UMP 시험 EEA 폼의 Consent → 개인정보 폼 재표시 → Do not consent → 재표시/Manage options 순서로 검증한다. 기본 갱신은 EEA 강제를 해제한다. SDK Reset이나 앱 데이터 삭제로 첫 실행 상태를 만들지 않는다. QA 차단은 별도로 검사한다. 종료 전 원래 네트워크 설정·보존 파일·참조·AAB 해시를 확인하고 증거 목록을 작성한다.
