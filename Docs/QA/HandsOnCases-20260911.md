# 손으로 다루는 사건 전환 QA — 2026-09-11

상태: **Candidate 2 기술 QA와 사용자용 설치·한국어 시작 화면 확인 완료**. EditMode **226/226**, PlayMode **127/127**, 조건별 컴파일 **15/15**를 통과했고, 최종 APK에서 영어 열 장면·28개 행동과 한국어 첫 장면을 직접 확인했다. 저장·기존 작업 보존도 확인했다. 소스 커밋은 `5f3857be83b38f683ebd8e01de387b3834505fc2`다. 아래의 아트 표현·실기기·광고 미검증 범위와 사람의 재미 평가는 별도로 남는다.

## Candidate 2 현재 검증 결과

| 확인 항목 | 결과 / 근거 |
|---|---|
| 전체 EditMode | **226/226**, 실패·건너뜀 0. 2026-09-11 **03:51:37–03:51:46 UTC**. |
| 전체 PlayMode | **127/127**, 실패·건너뜀 0. **03:52:28–03:54:31 UTC**. |
| 플레이어 조건별 컴파일 | 다섯 조건 × 세 어셈블리 **15/15**, 일반 테스트 메서드 **123개**가 모든 조건에서 현재 Editor와 동일. |
| atlas 재임포트 회귀 | `VisualAssetImporter.OnPreprocessTexture`가 모든 텍스처를 512로 다시 제한하던 원인을 수정했다. `/Workbench/` 경로에 2048 제한을 적용한 뒤 위 전체 검사에서 원본 폭 1536 유지 검사가 통과했다. |
| 소스·APK·QA 설치 | 빌드 exit **0**, 소스·생성물 대조와 APK 검사 통과. 전용 QA AVD 업데이트 후 기존 저장 해시 유지. 아래 산출물 표 참조. |
| Candidate 2 직접 실행 | 영어 두 사건 **10장면·28행동** 완료, 한국어 첫 장면 완료. 재시작·다시보기 저장 유지, 앱 예외 패턴 0개. |
| 기존 작업·개인 저장 보존 | 원래 파일 **2,283개 변경 0**, 보존 상태 플래그 모두 true. 기존 개인 AVD 저장 재확인 해시 동일. |
| 사용자 인계 | 별도 사용자 AVD에 최종 APK 설치·해시 확인. 한국어 **첫 출근** 화면과 진행 기록 0개를 확인하고 게임 입력을 멈췄다. |

최종 XML과 정상 종료 로그는 원래 프로젝트의 `Logs/ImmersiveCases-20260911/Candidate2/final-*.xml` 및 `final-AndroidDevelopmentBuild.log`로 보관했다. 산출물·설치 근거는 같은 폴더의 `apk-verification.json`, `source-to-apk.json`, `qa-install-verification.json`, `build-summary.json`이다.

## 작업 기준과 실제 변경

작업 브랜치는 `codex/immersive-cases-20260911`, worktree는 원래 프로젝트 아래 `.worktrees/immersive-cases-20260911`이다. 시작 커밋은 `7287dc70b562533612767a83008465ce2ff18f2d`이며, 작업 기준 확인 당시 [PR #4](https://github.com/joyshu93/Orbital_Salvage_Shop/pull/4)와 [PR #5](https://github.com/joyshu93/Orbital_Salvage_Shop/pull/5)는 모두 열려 있고 미병합이었다. PR #5의 HEAD가 이 시작 커밋과 일치한다. 새 제품 커밋·APK 빌드 입력은 아래 후보별 블록에 별도로 기록한다.

[설계](../superpowers/specs/2026-09-11-hands-on-investigations-design.md)와 [구현 계획](../superpowers/plans/2026-09-11-hands-on-investigations.md)은 개발자가 직접 플레이한 뒤 전달한 네 가지 문제를 바탕으로 한다.

| 실제 플레이 피드백 | 이번 제품 변경 | 검증과 해석의 경계 |
|---|---|---|
| 왜 플레이해야 하는지 모르겠다 | 밤의 분실물 보관소에서 물건을 돌보는 일이라는 역할과, 책상을 얼리는 냉기를 막는 즉시 목표를 첫 도입에 제시한다. 발견물이 다음 물건으로 이어지고, 마지막에는 다음 방문자에게 빌려줄 우산을 남긴다. | 첫 진입·목표 표시·두 사건 진행의 PlayMode 검사 통과. |
| 지금 하는 행위가 무엇인지 모르겠다 | 큰 물건의 특정 부분을 살펴보고, 도구를 끌어 쓰거나 도구·목표를 차례로 누른다. 균열 봉합, 덮개 개방, 낙엽 추출, 종이 펼침처럼 물리적인 행동과 결과를 연결한다. | Editor 입력·관찰·오답 복구·상태 전환 검사 통과. APK 직접 실행은 아래 별도 기록. |
| 읽고 분류하는 반복 노동 같다 | 두 사건의 주 진행을 12물건 분류·세 목적지 도장 반복에서 10개 작업대 장면으로 교체한다. 장면 전체에 도구 항목 28개, 실행 단계 28개, 관찰 목표 30개를 배치한다. 일반 분류는 별도 자유 교대로 남긴다. | 28개는 장면별 도구 항목의 합이며 서로 다른 도구 종류 28종이라는 뜻이 아니다. 새 방식의 반복 피로·재미는 사람 재플레이로 확인해야 한다. |
| NPC·메뉴 문구가 어색하다 | 한영 목표·관찰·도구·피드백·NPC 대사를 구체적인 부탁과 결과 중심으로 작성했다. 첫 장면 외 관찰문 26개는 정답 도구를 직접 지목하는 대신 상태와 위험을 설명하고, 구체적인 순서는 실패 힌트로 남겼다. | 한영 문자열·두 세로 화면 비율·언어 변경 후 관찰문 검사 통과. |

기존 두 사건과 열 단계의 안정된 ID, 사건 완료 기록과 세이브 경계를 유지한다. `Core/Workbench`는 UnityEngine에 의존하지 않는 상태 모델이며, `Content/Workbench`가 한영 퍼즐·대사를 제공하고 `GameApp.Workbench`가 기존 진행·저장 경계에 연결한다. 관찰과 선행 수리가 충족돼야 행동이 진행된다. 틀린 실험은 생명이나 진행을 소비하지 않으며, 광고·강제 대기 없이 다시 시도하도록 한다. 완료는 한 번만 반영한다.

## 열 장면의 목표

| 기존 단계 ID | 플레이어가 하는 일 | 실행 단계 수 |
|---|---|---:|
| `ice-01-crack` | 젖은 틈을 닦고 메워 책상으로 새는 냉기를 막는다. | 2 |
| `ice-02-spread` | 받침의 얼음을 녹이고 덮개를 열어, 사라진 낙엽이 들어간 시계를 찾는다. | 3 |
| `ice-03-tomorrow` | 이끼를 치우고 바늘에 걸린 낙엽을 빼낸 뒤 태엽을 감는다. | 3 |
| `ice-04-frozen-seal` | 시계 뒷면의 얼어붙은 봉인을 풀고 초승달 열쇠를 꺼낸다. | 3 |
| `ice-05-thaw` | 우산의 잠금을 풀고 휘어진 살을 펴서 냉기가 새는 덧댐을 꿰맨다. | 3 |
| `rain-01-voices` | 말을 하는 빗방울을 따로 담고 뚜껑을 닫아 목소리를 보존한다. | 2 |
| `rain-02-names-under-water` | 병을 열고 물을 덜어 내어 옛 직원 소연의 이름표를 살린다. | 3 |
| `rain-03-unsent-letter` | 굳은 접힌 선을 풀어 종이 물고기를 편지로 펼치고 글씨를 보존한다. | 3 |
| `rain-04-dry-order` | 편지 칸을 열고 선임의 답장을 써서 봉투에 넣는다. | 3 |
| `rain-05-testimony` | 우산의 주머니를 수선하고 답장을 넣어, 다음 방문자를 위한 우산을 걸어 둔다. | 3 |

냉기 문제 해결 뒤 우산 안의 목소리가 다음 사건의 단서가 된다. Rain은 소연이 선임에게 빌려준 우산, 미뤄 온 답장, 밤에 찾아오는 사람에게 자리를 남기자는 약속으로 연결된다.

## 이전 실행과 실패 증거

Unity 버전은 `6000.3.21f1`이다. 아래 XML은 첫 후보와 후속 수정의 실제 실행 결과이며 실패 실행도 보존한다. 관련 테스트 통과를 이후 수정된 후보나 다른 실행 경로로 확대하지 않는다. 원시 증거는 **원래 프로젝트의 로컬 `Logs/ImmersiveCases-20260911/`**에 있으며 커밋 대상이 아니다.

| 실행 / 로컬 증거 | 결과 | 의미와 후속 상태 |
|---|---|---|
| 초기 `EditMode-red2.xml` / `PlayMode-red2.xml` | Edit 4/51, Play 0/1 | 비어 있는 카탈로그 21개 실패, Core 계약과 첫 작업대 진입의 유효 RED. |
| `EditMode-green2.xml`, `PlayMode-green2.xml`, `PlayMode-regression-red.xml`, `PlayMode-green3.xml` | 순서대로 51/51, 22/26, 26/29, 17/29 | Atmosphere 종료, 오래된 분류·보상 콜백, 언어 복귀, 작은 텍스처에 고정 픽셀 Rect를 적용하는 문제를 구분해 수정했다. |
| Candidate 1 `integration-attempt-1/`, `integration-attempt-2/`, 루트 `final-*.xml` | Edit 224/225 → Edit 225/225·Play 119/122 → **225/225·122/122** | 기존 카드·NPC 문구 기대를 새 의도에 맞춰 갱신했다. 최종 실패·건너뜀 0. |
| `BuildAll-final.log` / `final-AndroidDevelopmentBuild.log` | 생성 뒤 종료 충돌 `-1073741819` → 재실행 exit **0** | 첫 실행을 정상 성공으로 세지 않는다. 두 번째 로그가 Candidate 1 APK 생성·정상 종료 증거다. |
| `PlayMode-native-polish-red.xml` → `PlayMode-native-polish-red3.xml` | 0/3 → 2/5 | 첫 실행의 진행 표시·글자 굵기는 실제 RED, 아이콘의 `AmbiguousMatchException`은 테스트 자체 오류. 다음 실행에서는 아이콘·완료 자국·없는 카드가 실제 assertion으로 실패했다. |
| `EditMode-native-art-red2.xml` | 0/1 | 원본 폭 1536 기대에 실제 512를 반환하는 atlas 문제의 유효 RED. |
| `Candidate2/integration-attempt-1/` | Edit **225/226**, APK 미생성 | 전처리기가 512 제한을 다시 쓰고 있어 atlas 검사 1개가 남았다. 전처리기 수정 후 위 Candidate 2 전체 재검사 통과. |

이 표에서 `a/b`는 통과/전체다. Candidate 1 증거와 Candidate 2 첫 실패 실행은 보존했으며, 새 결과로 덮어쓰지 않는다.

컴파일 실패는 유효 RED와 구분한다. 초기 및 native art RED 첫 시도, native polish RED 두 번째 시도에서 지원하지 않는 NUnit API/attribute 때문에 실행되지 못한 검사는 동작 실패 증거가 아니다. 위 XML의 실제 assertion을 기준으로 수정하며 실행 횟수를 누적 합산하지 않는다.

## 플레이어 조건별 컴파일 검사

Candidate 2의 소스에서 Runtime, Unity Test Framework TestRunner, PlayMode 테스트 어셈블리를 다섯 조건으로 각각 컴파일했다. Roslyn 실행 **15개 모두 exit 0**이며, 모든 조건의 일반 테스트 메서드 **123개**가 현재 Editor 목록과 정확히 일치한다. 컴파일 중 소스와 Core DLL 해시 안정성 검사도 통과했다. 메서드 수와 매개변수화된 실행 사례 수는 서로 다른 값이다.

| 조건 (`UNITY_INCLUDE_TESTS` 공통) | 어셈블리 3개 컴파일 | QA 화면 테스트 | SDK 콜백 테스트 | 일반 테스트 메서드 |
|---|---|---:|---:|---:|
| Android Development | 통과 | 0 | 1 | 123 |
| Android Development + native QA | 통과 | 3 | 1 | 123 |
| Android Development + native QA + offline QA | 통과 | 0 | 0 | 123 |
| Android native QA, 비개발 | 통과 | 0 | 1 | 123 |
| Windows Development | 통과 | 0 | 0 | 123 |

QA 타입·진입점의 조건부 포함과 작업대 런타임 목록도 각 조건의 기대와 일치했다. 증거는 로컬 **`Candidate2/`** 아래 `compiler-results-final.json`, `assembly-inventory-final.json`, `editor-inventory-final.json`, `compiler-sources-final.json` 및 `Compiler/final/`이다. 첫 후보의 15/15·일반 메서드 118개 결과는 상위 증거 폴더에 별도로 보존했다. 이 검사는 다섯 종류의 실제 패키징이나 네이티브 광고 실행 결과가 아니다.

## 시각 자산과 읽기 검증

기존 물건·초상·폰트·음향을 재사용하며, 실제 상태 변화를 보여주는 여섯 상태의 atlas를 추가했다. 생성 전 [AI 출처 기록](../AIAssetProvenance.md)의 `ART-WORKBENCH-20260911`과 [고지](../ThirdPartyNotices.md)를 갱신했다. 외부 이미지·패키지·성우·음향은 추가하지 않았다.

선택한 `Assets/Resources/Art/Workbench/workbench-states.png`는 1536×1024 RGB24이며 SHA-256은 `8F9C39B6DDF45D55BBA74CF7B809B5327916EF098CA914208BF167A45D5F918F`이다. 첫 생성의 체커보드 배경을 거절하고, 두 번째의 불투명한 짙은 자두색 배경을 채택했다. 생성 파일은 다른 이미지 도구로 편집하지 않고 그대로 복사했다. 균일 격자 지시와 달리 상단 가운데 장식이 행 경계를 넘으므로, 해당 얼음과 아래 편지의 source Rect 예외를 provenance에 기록했다. 런타임은 임포트된 실제 크기에 맞춰 이 좌표를 사용해야 한다.

Candidate 1 실제 화면 일곱 장을 검토해 텍스트 잘림·버튼 겹침은 발견하지 못했지만 아래 여섯 UI 문제를 확인했다. 그림 상태 전환이 존재하는 것과 해당 행동이 시각적으로 명확한지는 구분한다.

| Candidate 1에서 확인한 문제 | Candidate 2 대응 / 확인 상태 |
|---|---|
| `사건 1 / 1`이 사건 수와 장면 위치를 혼동시킴 | 사건 번호와 `현재 장면/총 장면` 표시. Editor 회귀 통과. |
| 천·메움제 등이 같은 일반 종이 아이콘으로 보임 | 도구의 용도를 나타내는 별도 형태. Editor 회귀 통과. |
| 크림색 패널의 관찰·대사가 가늘고 옅음 | 목표와 같은 짙은 잉크·글자 굵기. Editor 회귀 통과. |
| 1536폭 atlas가 512폭으로 임포트되어 흐림 | 빌더와 임포트 전처리기의 `/Workbench/` 해상도 제한 수정. 원본 폭 유지 검사 통과. |
| 완료 후 누런 사각형이 남고 새 물건에도 이전 자국이 붙음 | 완료·물건 교체 시 효과 정리. Editor 회귀 통과. |
| Rain04에서 카드·봉투를 다루지만 화면에는 상자만 보임 | 열린 편지 칸에 카드·봉투와 작성·포장 상태 표시. Editor 회귀 통과. |

별도로 한영 문구 세 문제를 수정했다. 물건을 보호하는 시계 목표, 편지를 찢지 않는 종이 물고기 목표를 자연스럽게 바꾸고, 실제 그림에 없는 상자 그림 참조를 편지의 답장 지시 문장으로 교체했다. Candidate 2의 영어 전체 경로·한국어 첫 장면과 스크린샷 43장을 확인했다. 이는 한국어 전체 재실행이나 모든 행동의 그림 일관성 통과를 뜻하지 않으며, 남은 아트 한계는 아래에 기록했다.

## Candidate 2 APK와 소스·QA 설치

| 항목 | 최종 증거 |
|---|---|
| 최종 제품 소스 커밋 | `5f3857be83b38f683ebd8e01de387b3834505fc2` |
| 빌드 입력과 최종 커밋 연결 | 빌드는 `53cb0e19f5398ea040482fc96230bbcfe1b828ed`에서 미커밋 수정을 포함해 시작했다. 기록한 C# **123개**의 전체 경로·해시와 생성물 **119개**의 경로·해시가 위 최종 커밋의 worktree와 모두 일치했다. 시작 커밋만을 APK 전체 소스로 표시하지 않는다. |
| APK 절대 경로 / 파일 크기 / SHA-256 | `C:\Users\D-\Documents\Codex_Project\Orbital_Salvage_Shop\.worktrees\immersive-cases-20260911\Builds\Android\CurioClerk-qa.apk` · **105,247,879 bytes** · `5D8DF9FA112C539ED66685A6ADA395CBC23F18C641AD5530421B6783A0039831` |
| 실제 package / version / Android 설정 | `com.joyshu93.curioclerknightshift`, `1.0.0` / `10000`, portrait, min API **29**, target API **36**, `arm64-v8a` 전용, **IL2CPP**. Unity `6000.3.21f1` 개발 빌드, `CURIO_NATIVE_ADS_QA` 포함. |
| 개발 서명 / 샘플 광고 ID | `Android Debug`. App ID `ca-app-pub-3940256099942544~3347511713`, 보상형 ID `ca-app-pub-3940256099942544/5224354917`는 Google 공식 샘플이다. SDK dex의 all-zero 예시 문자열은 별도로 허용·기록했다. |
| 빌드 정상 종료·입력 안정성 | `build-summary.json`: **exit 0**, 빌드 중 소스 해시 동일. **03:50:55–04:02:15 UTC**. APK 확인 **05:20:16 UTC**, 최종 소스 연결 확인 **05:30:14 UTC**. |
| QA 설치·기존 저장 보존 | **05:20:42 UTC**, `CurioClerk_WorkbenchQA_20260911`, `127.0.0.1:5597`, Android user **0**에 위 해시의 APK 업데이트. `savePreserved=true`, 저장 SHA-256 `3C0F6E95B9C7E7ED9215B0EA5C0C1AD95A25A780815036D346C62B265CB020C9` 유지. |
| Candidate 2 전체 실제 실행 / 사용자 플레이 환경 | 영어 열 장면 기술 QA 완료. 별도 사용자 AVD 설치·한국어 첫 출근 화면 확인 완료. |

소스 대조는 기록된 C# 및 `Assets/Localization`, `Assets/Resources/Content`, `Assets/Scenes` 범위의 동일성 검증이다. atlas PNG·임포터 설정은 별도로 확인했으며, 임시 Editor 저장 보호 스크립트와 검증 중 생긴 폰트 캐시·기존 아트 임포터·일부 설정의 보관·복원은 `validation-cleanup.json`에 기록했다. 비트 단위 재빌드 검증은 수행하지 않았다.

Candidate 1은 `Builds/Android/Candidate1-20260911/CurioClerk-qa.apk`에 보관했다(**79,960,946 bytes**, SHA-256 `C9F075AFE5F6E18B44EAE9D264BEDC06E48C5D0A970F6BB000395D68554B7A51`). 소스 기준은 `53cb0e19f5398ea040482fc96230bbcfe1b828ed`이며, 당시 C# 122개 대조·빌드 exit 0을 확인했다. APK·심볼 ZIP·Burst 디버그·백업 폴더의 4개 출력은 `Candidate2/candidate1-archive-verified.json`에서 **721개 파일 해시 동일**로 확인했다.

## 전용 Android 직접 실행 — Candidate 2 결과

이번 검증용으로 별도 AVD `CurioClerk_WorkbenchQA_20260911`를 만들었다. 콘솔 포트는 `5596`, ADB serial은 `127.0.0.1:5597`, ADB 서버 포트는 `5062`다. 사용자가 플레이하던 개인 AVD의 `5594` 인스턴스와 분리해 설치·입력한다.

| 확인 경로 | 최종 APK에서 확인한 결과 |
|---|---|
| 영어 두 사건 전체 | 기존 완료 저장의 다시보기 경로로 **10장면·28개 행동**을 모두 실행하고 두 결말에 도달했다. |
| 실제 도구 입력 | Ice01 닦기와 Rain01 빗방울 담기를 실제 드래그로 수행했다. 나머지 행동은 도구→목표 탭으로 진행했다. |
| Rain04 처음부터·카드/봉투 상태 | 편지 칸 개방·답장 작성 뒤 처음부터를 눌러 화면 소품이 초기화됨을 확인하고, 다시 개방·작성·봉투 넣기를 완료했다. |
| 다시보기 중복 진행 방지 | 영어 전체 완료 시 `q2-complete-save.json`이 설치 전·기존 다시보기 저장과 바이트 단위로 동일했다. SHA-256 `3C0F6E95B9C7E7ED9215B0EA5C0C1AD95A25A780815036D346C62B265CB020C9`. |
| 한국어 확인 | 언어를 한국어로 전환하고 첫 장면 도입→작업대→닦기→완료를 확인했다(`q2-ice01-*-ko.png`). |
| 화면 검토 | 실제 스크린샷 **43장**을 검토해 잘림·완료 후 남은 작업 사각형을 발견하지 못했다. 아래 개별 물건의 상태 그림 한계는 남는다. |
| 최종 저장·환경·로그 | `candidate2-final-summary.json`, **05:39:35 UTC**: 기록 **10**, 완료 사건 **2**, 발견 **7**, coins **0**, locale `ko`. Wi-Fi **0**, mobile data **0**, analytics/crash consent **false**, 앱 예외 패턴 **0개**. |

이 결과는 정답을 아는 에이전트의 실제 입력 검증이다. 영어 전체 다시보기 뒤 한국어로 바꾸었으므로, 위 바이트 동일성은 한국어 전환 전 영어 완료 저장에 대한 비교다. 별도 분류 연습 왕복과 모든 선행 조건의 오답 조합은 이번 네이티브 확인 범위에 포함하지 않았다.

## Candidate 1 직접 실행 배경

| 확인 경로 | Candidate 1 실제 결과 / 로컬 증거 |
|---|---|
| 시작·첫 도입·즉시 목표 | 실제 실행 후 첫 선임 대사·작업대 진입 확인. 첫 시작에 검은 로딩 화면과 Android 전체 화면 안내를 거쳐 메뉴가 표시됐다. 시작 시간·성능을 측정한 결과는 아니다. |
| 한국어 두 사건·열 장면·28개 행동 | ADB 입력으로 Ice 5장면과 Rain 5장면 완료. `qa-both-completed-save.json`: 단계 기록 **10**, 완료 사건 **2**, 발견 물건 **7**, coins **0**. |
| 드래그·탭·오답 복구·장면 재시도 | Ice01과 Rain01에서 실제 도구 드래그, 나머지 도구→목표 탭 입력 확인. 첫 장면의 관찰 전 잘못된 도구 사용 후 힌트를 보고 복구했다. 처음부터 재시작 후 다시 해결했다. |
| 같은 프로세스에서 메뉴 복귀 | Ice01의 닦기 완료 뒤 메뉴→도입→작업대로 돌아와 닦은 상태가 유지됐고, 다시 닦지 않고 봉합할 수 있었다. |
| 프로세스 재실행·영어 다시보기 | force-stop 후 실행해 완료 기록을 복구하고 실제 영어 UI에서 첫 사건의 첫 장면을 다시 완료했다(`qa-replay-ice-en.png`, `qa-replay-ice-complete-en.png`). 다시보기 전후 저장은 아래 해시가 동일하다. |
| 한영 가독성·물건 상태 | 한국어 전체 경로와 영어 첫 장면에서 확인. 위 여섯 UI 문제는 미해결 후보의 발견 사항이며, 모든 장면의 영어 가독성 통과로 확대하지 않는다. |
| 네트워크·동의·앱 로그 | `Candidate1-final-summary.json`: Wi-Fi **0**, mobile data **0**, analytics/crash consent **false**. 계정·광고 시청 없이 진행했고 검사한 앱 로그에서 예외 패턴 **0개**. |

`qa-relaunch-save.json`과 `qa-replay-save.json`의 SHA-256은 모두 `3C0F6E95B9C7E7ED9215B0EA5C0C1AD95A25A780815036D346C62B265CB020C9`다. 두 저장 모두 단계 기록 10·완료 사건 2·발견 7·coins 0으로, 이 영어 다시보기 경로에서 진행·보상이 중복되지 않았다. 화면·명령·저장·앱 로그는 무시된 로컬 증거로만 남긴다.

이는 소스와 해결 경로를 아는 에이전트의 기술 QA다. 이전 분류 방식 APK의 완료 기록이나 사람의 자연 플레이 평가를 대신하지 않는다.

## 사용자 전달과 변경 검토

| 항목 | 최종 증거 |
|---|---|
| 사용자 재플레이용 별도 AVD / 실행 화면 | `CurioClerk_HandsOnPlay_20260911`, 콘솔 `5598` / ADB `127.0.0.1:5599`, 서버 `5062`, Android user **0**. **05:55:30 UTC** 최종 APK 신규 설치·해시 일치 확인. **05:59:52 UTC** 한국어 첫 출근 화면, 진행 단계 **0**, 기록·완료 사건·발견·coins **0**, 동의 두 항목 **false**, Wi-Fi·mobile data **0** 확인. |
| 기존 개인 AVD 진행 유지 | 확인 완료 — `127.0.0.1:5595` 저장 SHA-256 `F8447C4B7E3B9E19EA8DDB100844FF97AB06DD1E3D55CB9D27DD728E0A3D2B5A` 재확인, 이전 값과 동일. |
| 제품·QA 커밋 / 변경 PR | [제품 소스 `5f3857b`](https://github.com/joyshu93/Orbital_Salvage_Shop/commit/5f3857be83b38f683ebd8e01de387b3834505fc2), [통합 QA·안내서 `68ec4e7`](https://github.com/joyshu93/Orbital_Salvage_Shop/commit/68ec4e7cc896a7011130c5ad5e9e32458296db42), [개선 PR #6](https://github.com/joyshu93/Orbital_Salvage_Shop/pull/6). PR #5 브랜치 위에 쌓은 개선이며 생성 확인 시 열림·미병합, 본문·base/head 일치를 확인했다. 이번 인계 기록 보완은 문서만 변경하며 APK 제품 소스는 동일하다. |

로컬 증거는 `Candidate2/personal-install-verification.json`, `Candidate2/personal-handoff.json`, `personal-ready-ko.png`다. 새 저장 SHA-256은 `9BF349B1B4B1AA44ED64B621CF5DF49BC8CFB4C28EC75D5653E75B8DBB84BEDE`다. 이 값은 인계 당시의 기록이며 이후 사용자 플레이로 달라질 수 있다. 새 AVD는 화면이 있는 창으로 실행했고, APK 설치·언어 설정 뒤 사건은 시작하지 않았다. 마지막 Windows 창 선택 중 사용자가 Esc로 Computer Use를 중단해 추가 화면 제어를 멈췄다. Android 시작 화면은 확인했지만 Windows 최상단 창 확인이나 사람의 실제 플레이 결과로 확대하지 않는다.

첫 부팅 지연과 System UI ANR은 게임 설치 전에 발생한 에뮬레이터 시스템 문제다. 부팅 완료 후 설치했고 Android 전체 화면 안내를 닫은 뒤 게임 메뉴 진입을 확인했다. 이 과정을 실제 휴대전화 시작 성능 통과로 기록하지 않는다.

## 보존과 미검증 경계

시작 보존 기록 `preservation-baseline.json`은 01:17:17 UTC에 작성됐다. 최종 `preserve.ps1 -Verify`와 `preservation-latest.json`에서 기록 파일 **2,283개 중 변경 0개**, 원래 dirty **22개**의 status 동일, index 비어 있음, 기존 브랜치·stash·worktree 보존, story·이전 QA worktree status 동일을 모두 확인했다. 새 소스·생성물은 전용 worktree에서 작업했다. APK와 원시 로그·스크린샷·저장은 로컬 증거로 남긴다.

개인 AVD의 유효한 `personal-external-save-before-update.json`은 `activeIncidentId=unmelting-ice`, `activeIncidentStage=2`, 완료 단계 기록 2개와 사건 완료 0개를 담고 있다. 기존 개인 AVD의 게임 진행에는 입력하지 않았으며 새 플레이 AVD로 전달했다. `personal-internal-path-probe.txt`는 내부 파일 경로를 찾지 못한 명령 오류이므로 저장 JSON 증거로 사용하지 않는다.

사용자가 이전 버전을 실제로 플레이하며 전달한 피드백은 이번 방향 변경의 입력이다. 새 버전에서 역할·행동을 이해하는지, 다음 물건이 궁금한지, 반복이 지루한지, 소연과 선임의 사연이 전달되는지는 **사람 재플레이 전까지 미검증**이다. 에이전트의 소스 기반 QA는 이 평가를 대신하지 않는다. 사람 모집·외부 메시지 발송·응답 수집은 하지 않았다.

그림 상태에는 후속 보완 항목이 남는다. Ice03의 `clear-hinge` 결과는 이끼를 털고 유리가 열렸다고 설명하지만 열린 시계 그림은 다음 `free-leaf` 뒤에 나타나며, Ice02 완료·Ice03 시작 그림의 낙엽도 뚜렷하지 않다. `q2-ice04-lift.png`의 시계 속 열쇠는 금색 초승달 머리인데 `q2-ice04-complete.png`에서 꺼낸 열쇠는 기존 분홍색 꽃·하트 모양 머리로 바뀐다. Rain01의 빗방울을 담는 행동 직후에는 병 그림이 없고, Rain02에서 물을 덜어도 수위·이름표 변화가 그림에 드러나지 않는다. 이 구간에서 진행·입력 실패나 화면 잘림은 발견하지 못했으나, **28개 행동마다 고유한 상태 그림이 일치하는 것은 아니다**.

[기존 네이티브 광고 QA](AndroidNativeAdsUmpQa-20260909.md)의 다음 미검증 경계는 그대로 남는다.

- 광고 자체의 X/Back으로 보상 전에 닫기. 기존의 Home→런처 복귀에서 실제 Dismissed를 확인한 경로와 구분한다.
- 실제 no-fill(Code 3)과 네이티브 fullscreen 표시 실패. 기존 오프라인 DNS/네트워크 LoadAdError 통과와 다르다.
- 최초 설치의 미결정 동의 상태에서 거절. 기존에는 첫 동의 후 개인정보 폼을 재표시해 거절·철회했다.
- 네이티브 SDK 자체의 중복 Earned 발생 재현. 재클릭·앱 복귀 방어와 자동 중복 콜백 테스트를 이 결과로 바꾸지 않는다.
- 실제 ARM64 기기의 성능·발열, 소리·진동의 체감 품질, 모든 지역·언어의 서버 제공 동의 폼.

실제 광고 ID, 릴리스 서명, AAB, 병합, 배포·스토어 제출은 수행하지 않는다.
