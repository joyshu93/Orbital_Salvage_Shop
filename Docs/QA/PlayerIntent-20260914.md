# Purpose and investigation QA — September 14–16, 2026

Status: **development candidate validated for internal playtesting**. This cycle responds to returning-player feedback on September14; final Editor/build/native work occurred September16. It continues [PR #6](https://github.com/joyshu93/Orbital_Salvage_Shop/pull/6), without merge or release. Automated completion and source-aware replay do not establish human understanding or enjoyment.

## Reported problem and implemented behavior

The player could see improved presentation but still could not explain why these objects needed investigation, identify small `+` targets or the selected tool, or follow a compelling story. The earlier technical passes do not contradict that report.

| Problem | Change and intended effect |
|---|---|
| Puzzles arrive without a purpose | A persistent case request and current question accompany every workbench. Introductions connect the prior discovery to the next object. The first scene establishes the night clerk's job and endangered stored belongings; sealing a leak does not falsely imply the cold source is solved. The watch, umbrella and reply box have explicit reasons to investigate. |
| Tiny targets and ambiguous action | Large named target buttons accompany numbered object markers. Tapping a target always inspects/focuses it, even with a selected tool. Tool choice preserves the observation; **Use here** applies the intended combination. Drag remains an alternate deliberate action. |
| Selection is hard to see | Gold selection, a SELECTED badge and a separate tool-function description persist. Neutral Selectable tint prevents actual pointer focus from darkening the selected color. A drag updates the target used by the next explicit action. |
| Story/evidence is lost between actions | A notebook contains the request, earlier discovery, current question and only inspected evidence. Re-inspection and notebook copy reflect same-target and related-target state changes. The modal blocks underlying inputs and restores their original enabled states. |
| Repetition and arbitrary work | Ice04 is compressed from heating, levering and tweezing to warming a spring lid and retrieving its key. Ice01 no longer requires reading an unrelated leaf before sealing a dry crack. Specific wrong experiments explain physical obstructions. These improve the information and pacing available to the player; enjoyment remains a human evaluation question. |

The two cases retain their ten stage IDs, discoveries, offline path and version-4 save schema. They now have **27 actions**, down from28. No new case, monetization, dependency, bitmap or audio asset was added. Bilingual copy provenance was recorded before authoring in [AIAssetProvenance](../AIAssetProvenance.md) and [ThirdPartyNotices](../ThirdPartyNotices.md). Design and open issues are in [the plan](../superpowers/plans/2026-09-14-purpose-and-investigation.md) and [QualityReview](../QualityReview.md).

## Tests and review

Raw evidence remains ignored under `Logs/PlayerIntent-20260914/` and `Logs/ImmersiveCases-20260911/FunPolish/` in the original checkout.

| Check | Observed result |
|---|---|
| Initial UI RED | **0/3**: absent purpose/notebook and selection overwriting evidence. `red-ui/`. |
| Catalog RED | **0/1**: Ice04 still required its old three-action sequence. `red-content/`. |
| Review-driven RED | **0/3** for drag→next-action target mismatch, stale crown observation after freeing the leaf, and actual pointer tint. **0/2** for notebook input leaking to restart and unrelated leaf-reading gate. `red-drag-state/`, `red-modal-gate/`. |
| Layout RED | Two hint overflows in both portrait ratios and overlapping first-scene markers. `red-markers/`. The separate target-size test was corrected to set the supported portrait canvas explicitly, rather than relaxing its size requirement for an Editor landscape default. |
| Focused final PlayMode | **48/48**, failed0, exit0. `green-ui-03/`. Covers workbench/atmosphere behavior, both languages and portrait ratios, explicit use, drag, hints, notes, restart, progression and rendering. |
| First full Editor run | **231/231 EditMode + 141/141 PlayMode**, failed/skipped0. `purpose-20260916-01/final-*-results.xml`. APK build then failed before BuildAll at the release privacy gate. |
| Final candidate Editor run | **231/231 EditMode** (01:21:08–01:21:16 UTC) and **141/141 PlayMode** (01:22:04–01:24:08 UTC), failed/skipped0. Repository `scripts/test-unity.ps1` within the development-build route. `purpose-20260916-02/final-*-results.xml`. |

The first build's privacy gate was reproduced under its actual Windows PowerShell host: a UTF-8 file without an encoding marker was decoded with the legacy default, so an English dialogue word (`socket`) escaped string stripping and was mistaken for networking code. The same gate passed in the newer shell. Adding UTF-8 BOMs to the two new bilingual C# files made the actual build-host gate pass; neither the gate nor its forbidden-code rules was weakened. Candidate01 produced no APK, and all canonical outputs were restored. The retry retains the repository's full test/build route.

Independent read-only review identified the drag/state/tint/modal failures before their RED tests. Final source re-review found all four addressed and no additional build-blocking issue; it did not execute tests, operate the APK or evaluate human enjoyment. QA conditional-compile guards were not changed. The prior five-condition/15-compilation matrix remains historical, not rerun evidence for this candidate.

## Final artifact and native validation

Candidate **`purpose-20260916-02`** built successfully from **01:20:16–01:26:52 UTC**, September16, through the repository development route, including BuildAll. Build start was `5619fced876045df71db8381a55927b1de30628d` plus frozen dirty inputs. Product source is committed as **[`e9eb4b5b39edf76745b746bfadd5e375ba037acd`](https://github.com/joyshu93/Orbital_Salvage_Shop/commit/e9eb4b5b39edf76745b746bfadd5e375ba037acd)**. The final committed working tree matches all **126 C# paths/hashes and 71 authored inputs** recorded for the successful build. This is input identity, not a bitwise rebuild claim.

APK, relative to the worktree: **`Builds/Android/FunPolish-20260911/purpose-20260916-02/player/CurioClerk-qa.apk`**, **105,257,071 bytes**, SHA-256 **`C70EA549E4184BE2AEDA70D419753C301F6DE62283B225B2B5CEFB0EF2A557E7`**. Package inspection verifies Unity6000.3.21f1, `com.joyshu93.curioclerknightshift`, version1.0.0/10000, portrait, API29/36, arm64-v8a only, IL2CPP, Development/debuggable, Android Debug signature and official Google sample app/rewarded IDs. The existing SDK's zero-valued placeholder is also present. No real publisher ID, AAB or release signing was introduced. Successful logs retain TLS/SDK/plugin warnings; this is not a warning-free build claim.

Installation at **01:28:09 UTC** on dedicated `CurioClerk_WorkbenchQA_20260911`, Android user0, ADB5062/5597, confirmed the installed APK hash. Before/after installation saves are identical. All native play used the existing completed-save **replay** and source-aware coordinates on1080×2400; it is not a first-time unlock or novice test.

| Native route | Evidence and observed result |
|---|---|
| Korean, Ice01–05 and Rain01–05 | All27 actions complete with real inputs, preserving scene transitions and both endings. `ko-ice*`, `ko-rain*`. The two-action Ice04 works; Ice01 completes without the unrelated leaf-reading prerequisite after restart. |
| Controls and recovery | First-scene named buttons, selected badge/function, observation retained across selection/other target, explicit wrong experiment, Hint/More help, notebook open/close and Start over verified. Ice01 drying used drag and sealing used the explicit button. `ko-ice1-selected/other-observation/wrong-experiment/hint-more/restarted/dry/complete`. |
| Current evidence | After freeing the leaf, the crown describes free movement, and the notebook reflects the repaired state. `ko-ice3-crown-updated`, `ko-ice3-notes`. |
| English | Ice01 and all Rain01–05 completed, with readable named controls, selected-tool description and notebook. Revised Rain03→04 purpose and Rain05 ending inspected. English Ice02–05 were not natively replayed in this candidate; all10 scenes retain both-language Editor layout coverage. |
| Offline and replay preservation | Start/final Wi-Fi and mobile data settings0; Wi-Fi status disabled. No account, consent or ad required. Returned through settings to the Korean office. Installation/start/final save SHA remains **`01D1D32008A8F763BF54AD1D37A11BC55B1C46AF2B55C26695FEF782CF895579`**, **1,687 bytes**, sampled finally at **01:44:13 UTC**. |

Final logcat contains **324 lines** for appPID3840 with zero matches for the specified fatal/ANR, Unity/AndroidRuntime error and major managed-exception patterns. Emulator graphics/offline-service warnings in the complete log are separate. This pass did not test process-death checkpoint resume on a new unfinished save; that remains Editor/prior-candidate evidence. No QA inputs were sent to the personal player environment.

Independent static inspection of18 Korean screenshots found readable purpose/selection/notebook, without detected clipping or marker overlap. It reconfirmed **Q04b**: Ice03 describes a rear crescent seal but displays the front, Ice04 reuses a closed/mossy front, and the key's crescent is hard to identify. Later jar/water-state artwork gaps also remain open. These are not counted as fixed by clearer target labels and prose.

Cleanup archived27 raw guard/generated files, removed the two temporary Editor guard files from Assets, and restored only20 artifact metas containing trailing spaces plus the empty-keystore serialization change. All153 generated paths are represented in the final working tree; only the explicitly reviewed ProjectSettings restoration differs from the build manifest. Original checkout22 dirty-file hashes, status, index and stash passed comparison against the September14 baseline. Older APKs and canonical companion outputs remain intact. Raw screenshots, saves, APKs and logs are ignored, not committed.

Build/install/package/cleanup/source evidence is under `FunPolish/purpose-20260916-02/`: `build-summary.json`, `qa-install-1yXqrS/`, `apk-inspection-20260916T014312217Z/`, `cleanup-20260916T013751812Z/`, and `final-source-proof-e9eb4b5b39edf76745b746bfadd5e375ba037acd.json`. Native captures, input commands and save/log samples are in `PlayerIntent-20260914/native-purpose-20260916/`.

## Remaining limits

After the dedicated QA finished, the existing personal emulator was reopened for handoff. The exact new APK was installed at01:48:33 UTC with the game process absent, preserving the1,351-byte save before/after update and launch. The Android System UI displayed an unresponsive dialog during boot; selecting **Wait** once recovered to the Korean office at01:50:16 UTC. The screen shows **Remembering Rain, Continue work3/5**, with the first case available for replay. No gameplay or locale inputs were sent to this profile. `FunPolish/personal-update-OHVxvW/handoff-after-system-ui.json` and its screenshot record this separate handoff; the earlier `screen-ready.png` contains the system dialog and must not be cited as a playable menu. Windows topmost placement was not independently inspected.

This is an internal development build and a bounded improvement of the existing two cases. Real-device performance, heat, audible sound, haptics and native ad/UMP edge cases are not newly verified. Existing visual gaps in later object states remain tracked separately. The prior complaint about enjoyment remains open until the revised experience is actually evaluated by a person; source-aware agent replay is not a novice playtest. No participants were recruited and no messages were sent to testers.
