# QA PlayMode 조건부 컴파일 검증 — 2026-09-09

대상: [PR #4](https://github.com/joyshu93/Orbital_Salvage_Shop/pull/4), 시작 HEAD `83c5f0a6a7e7cb7f389d1fc3d47297ff7e1d3e5c`, 브랜치 `codex/emulator-qa-20260908`. 선행 커밋 `86116ff`가 HEAD와 PR에 포함됨을 확인했다. PR은 열린 상태였으며 원격 main은 `d15e39a`였다.

## 원인과 최소 수정

PlayMode 어셈블리는 플레이어에도 포함될 수 있지만 QA 화면 테스트 3개가 일반 플레이어에서 제외되는 `NativeAdsQaSession`과 `GameApp.ShowNativeAdsQa`를 무조건 참조했다. SDK 콜백 테스트도 Windows 및 오프라인 플레이어에서 제외되는 `GoogleAdsCallbackDispatcher`를 리플렉션으로 요구했다.

- `GameAppPlayModeTests.cs`: 첫 QA 테스트 3개에 런타임과 같은 `UNITY_EDITOR || (UNITY_ANDROID && DEVELOPMENT_BUILD && CURIO_NATIVE_ADS_QA && !CURIO_OFFLINE_QA)` 가드를 적용했다. 공통 setup/teardown, 도우미와 일반 테스트는 그대로다.
- `GoogleAdsCallbackDispatchPlayModeTests.cs`: 파일 전체에 해당 런타임과 같은 `UNITY_EDITOR || (UNITY_ANDROID && !CURIO_OFFLINE_QA)` 가드를 적용했다. 일반 Android의 SDK 콜백 검증도 유지한다.
- 코드 차이는 전처리기 네 줄 추가뿐이다. 런타임, asmdef, 패키지, 콘텐츠 및 플레이어 설정의 커밋 변경은 없다. 읽기 전용 독립 리뷰에서 actionable finding은 없었다.

## 수정 전후 컴파일

고정 Unity `6000.3.21f1`의 `netcorerun`과 `DotNetSdkRoslyn/csc.dll`을 사용했다. 기존 성공 Android 플레이어 빌드의 Unity 생성 response file에서 플레이어 엔진·패키지·Core 참조와 기본 심볼을 가져왔다. 각 조건에서 실제 Runtime 소스, Unity Test Framework의 `UnityEngine.TestRunner` 소스 및 전체 PlayMode 소스를 새 DLL로 컴파일했다. 테스트용 `UNITY_INCLUDE_TESTS`를 포함하고 Editor 심볼은 제외했다. Windows 조건은 설치된 Windows 플레이어 엔진 참조와 플랫폼 심볼을 사용했다. 대역 타입이나 수정된 패키지 소스는 사용하지 않았다.

수정 전 일반 Android 조건은 Runtime과 TestRunner가 각각 exit 0이었지만 PlayMode 컴파일은 exit 1이었다. `GameAppPlayModeTests.cs` 원본 47/95행에서 `CS0234` (`CurioClerk.Qa` 부재), 75/88행에서 `CS1061` (`ShowNativeAdsQa` 부재)의 **정확히 네 오류**를 확인했다. 같은 컴파일 절차에서 가드 적용 후 모두 통과했다.

| 조건 (`UNITY_INCLUDE_TESTS` 공통) | Runtime / TestRunner / PlayMode 컴파일 | QA 화면 테스트 | SDK 콜백 테스트 | 일반 테스트 메서드 |
|---|---|---:|---:|---:|
| Android Development, QA 심볼 없음 | 모두 exit 0 | 0 | 1 | 123 |
| Android Development + `CURIO_NATIVE_ADS_QA` | 모두 exit 0 | 3 | 1 | 123 |
| 위 조건 + `CURIO_OFFLINE_QA` | 모두 exit 0 | 0 | 0 | 123 |
| Android + QA 심볼, `DEVELOPMENT_BUILD` 없음 | 모두 exit 0 | 0 | 1 | 123 |
| Windows Development, QA 심볼 없음 | 모두 exit 0 | 0 | 0 | 123 |

Unity에 포함된 Cecil로 생성 DLL을 읽어 테스트 메서드 목록과 런타임 타입·진입점의 포함 여부를 대조했다. 다섯 조건 모두 일반 테스트 123개의 식별자가 Editor 기준과 동일하고, QA 테스트는 대상 런타임이 존재하는 조건에만 남았다. 이는 **대상 조건의 C# 컴파일 및 DLL 검사**이며 전체 테스트 플레이어 패키징, IL2CPP 네이티브 링크 또는 기기 실행 검증은 아니다.

로컬 증거: `Logs/QaTestGuards-20260909/compile-matrix.ps1`, `Compiler/red/AndroidOrdinary/`, `Compiler/green/`, `compiler-results-green.json`, `assembly-inventory-green.json`. `.rsp`와 원시 출력도 해당 로컬 폴더에만 보존했다.

초기 검증 도구의 실패도 별도 보존했다. 첫 worktree 캐시 복사 누락, 도구의 Unity 내부 API 접근 오류, 공개 컴파일 API의 테스트 어셈블리 제외를 최종 RED/GREEN으로 계산하지 않았다. 내부 컴파일 옵션을 사용한 시도에서는 위 QA 오류와 함께 도구의 엔진 참조 구성으로 인한 Collections 오류가 발생했다. 공개 API 시도 뒤 Unity 종료 오류도 기록했다. 최종 판정은 이들과 분리된 위 컴파일 결과 및 아래 저장소 테스트 실행을 기준으로 한다.

## 기존 Editor 테스트

`scripts/test-unity.ps1`를 검증용 detached worktree `.worktrees/qa-test-guards-20260909`에서 실행했다. 이 사본의 임시 product name으로 테스트 저장 위치를 분리했고, 시작 시 `Application.persistentDataPath`가 별도 디렉터리임을 검사했다. 원본 PlayerSettings나 기존 앱 저장은 이 격리에 사용하지 않았다. 임시 설정과 검증 도구는 커밋 대상이 아니다.

- EditMode: **171/171**, 실패·건너뜀 0, 2026-09-09 03:57:10–03:57:13 UTC.
- PlayMode: **127/127**, 실패·건너뜀 0, 2026-09-09 03:58:13–04:04:11 UTC. QA 화면 테스트 3개와 실제 SDK 콜백 큐 테스트 1개가 모두 실행·통과했다.

두 Unity 프로세스와 저장소 스크립트 모두 exit 0. 결과와 로그는 `Logs/QaTestGuards-20260909/EditMode-final.xml/.log`, `PlayMode-final.xml/.log`에 보존했다. Editor 로그의 기존 라이선스 클라이언트 연결 재시도 및 Unity 서비스 CDN 통신 경고는 게임 코드 컴파일·테스트 실패가 아니며, 테스트가 통과한 범위를 네이티브 광고 검증으로 확장하지 않는다.

## 보존과 검증 경계

커밋 전 `preservation-precommit.json`에서 원본 dirty 파일 **22/22**, story 작업 파일 **689/689**, 기존 APK/AAB **2/2**, 기존 Editor 저장 파일 **7/7**의 SHA-256이 같음을 확인했다. 작업 브랜치 외 5개 브랜치 참조, stash, story worktree 등록·HEAD·상태도 동일하다. 기존 22개 파일은 수정하거나 커밋하지 않는다. 새 검증 worktree, 분리된 테스트 저장 및 로컬 증거는 별도로 남긴다.

[기존 네이티브 QA 보고서](AndroidNativeAdsUmpQa-20260909.md)의 미검증 판정은 변경하지 않는다. 광고 자체 X/Back의 보상 전 취소, no-fill, 네이티브 fullscreen 표시 실패, 최초 설치 미결정 상태의 거절 및 실기기 성능은 추가 통과로 처리하지 않는다. 앱·에뮬레이터 조작, APK 설치, 릴리스 서명, AAB 생성, 배포, 데이터 삭제 및 PR 병합은 수행하지 않는다. 원시 로그, 빌드 파일, 내부 앱 데이터, 동의/TCF 문자열이나 서비스 설정은 커밋하지 않는다.
