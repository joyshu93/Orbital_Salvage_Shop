# Fun and polish QA — 2026-09-11

Status: **bounded improvement cycle validated; development APK ready for internal playtesting**. Tests, build and native play were performed on September 11. The task paused before cleanup; preservation, committed-source verification and documentation were completed on September 14 without rerunning unchanged gameplay. This report separates technical checks, agent review and untested human responses. No merge or release was performed. The existing [PR #6](https://github.com/joyshu93/Orbital_Salvage_Shop/pull/6) remains the integration target.

## Selected improvements and comparison

The returning developer reported that puzzle-solving had become more enjoyable, then identified a missing hint. That positive response is preserved as prior feedback, not attributed to this candidate. This cycle uses [QualityReview](../QualityReview.md) and its [bounded implementation plan](../superpowers/plans/2026-09-11-fun-polish-cycle.md).

| Area | Before | Implemented change and intended effect |
|---|---|---|
| Watch discovery (Q02) | Ice02's completion described a leaf in the watch but displayed the leafless collection sprite. | A visible wine-red leaf is trapped under a hand; the same sprite carries into Ice03. The discovery supplies a visible problem to investigate. |
| Watch repair (part of Q04) | `clear-hinge` said the glass opened, but the picture changed only after `free-leaf`. | Separate closed/leaf, open/leaf and open/empty states change on their corresponding action. Restart reconstructs the initial state. |
| Korean reading (Q03) | Completion dialogue split Hangul words across lines, including the reported Ice02 screen. | GameApp initializes TMP's modern Korean spacing rules. Rendering checks cover both portrait layout shapes; global menu/settings implications are checked by full regression and native sampling. |
| Rain payoff (F02) | The letter directed the reply to a box, while the player later put it in the umbrella. The reason to lend the umbrella onward was weakly connected to its discovery. | Four EN/KO prose fields connect Soyeon's letter, the senior's change of mind, the repaired pocket and helping the next visitor. This is an editorial improvement hypothesis, not evidence of increased human enjoyment. |

The 10 scene IDs, 28 actions, observations, tool prerequisites, optional hints, version-4 saves and offline path are unchanged. No new case, reward economy, forced wait, ad requirement, audio or dependency was added. F01 (repetitive structure) was reviewed as an improvement candidate but not selected without a more specific failing scene; no repetition or broader fun problem is marked resolved.

## Technical evidence

Raw evidence is kept only under the original project's ignored `Logs/ImmersiveCases-20260911/`.

| Check | Result |
|---|---|
| Focused PlayMode RED | **0/2**: actual UI used the leafless collection sprite; rendered Korean split a word between 라/진. `PlayMode-fun-polish-red.xml`. |
| Import RED | **1/2**: existing atlas passed; the new watch atlas was reduced from 2172 to 2048 pixels. `EditMode-fun-polish-import-red.xml`. |
| Focused PlayMode green | **3/3**, no failed/skipped cases. Watch transition/restart, Korean word boundaries, and extended all-scene EN/KO completion readability at equivalent 1080×1920 and 1080×2400 layouts. `PlayMode-fun-polish-green.xml`. |
| Final Editor regression | **230/230 EditMode** (08:56:14–08:56:36 UTC), **132/132 PlayMode** (08:58:25–09:00:44 UTC), failed/skipped 0. Repository `scripts/test-unity.ps1`, called by the debug-build script. |
| Compiler condition matrix | **15/15 compiler exits 0**: Runtime, TestRunner and PlayModeTests across five Android/Windows define conditions. Each condition contains the same 128 ordinary test methods and five Workbench runtime types. QA/callback inclusion matches its intended conditions. Phase `funpolish-candidate-01`; this is C# compilation/inventory, separate from native execution. |

The new atlas was produced using built-in ImageGen. A checkerboard/solid-lid version was rejected. The selected unchanged PNG is **2172×724 RGB24**, SHA-256 **`E7F3F37359FEBED5AD0BA16FC196D24AD1431A2A0034554CF988325A9B3B0A8A`**, with an opaque dark-plum background and slight color variation. It is sliced in thirds, not treated as transparent. Importer and BuildAll retain a 4096 ceiling for this asset. Prompts, inputs and limitations are in [provenance](../AIAssetProvenance.md); existing notices apply.

## Build and source identity

Candidate: `FunPolish/candidate-01`. Build ran **2026-09-11 08:54:23–09:13:23 UTC**, wrapper and Unity exit **0**, from `b5444412a8ddf00aa59d0f27a19a42d17bcfdf55` plus recorded dirty implementation inputs. The product source is now committed as **[`e0e92781e1a214d2bd0ff53b098976a290dfc7a5`](https://github.com/joyshu93/Orbital_Salvage_Shop/commit/e0e92781e1a214d2bd0ff53b098976a290dfc7a5)**. All **124 C# files** and **71 authored inputs** match the exact recorded build paths and hashes. This is source/input identity, not a claim of a bitwise rebuild.

Output, relative to the worktree: **`Builds/Android/FunPolish-20260911/candidate-01/player/CurioClerk-qa.apk`**, **105,252,549 bytes**, SHA-256 **`B7CA9FB7D0136CB5247926FD44C83C344F31FBF36D0C3205AC31CB8470C2CC79`**. Actual package inspection verified `com.joyshu93.curioclerknightshift`, version **1.0.0 / 10000**, Unity **6000.3.21f1**, portrait, API **29/36**, **arm64-v8a only**, **IL2CPP**, Development/debuggable and Android Debug signature. Both official Google sample app/rewarded IDs were found; the existing SDK zero-valued placeholder occurs only in `classes5.dex`. No real publisher ID, release signing or AAB was introduced.

The wrapper's four success flags are true: build succeeded, C# hashes stable, authored-input hashes stable and original published outputs restored. The original four output entries comprise **720 files**, independently compared without a mismatch. Build logs contain licensing/TLS, SDK XML and unsupported iOS plugin warnings despite successful completion; this is not a warning-free build claim.

## Native comparison and preservation

Baseline was re-exercised on the existing Hint revision in the dedicated QA AVD, not the human's ongoing game. `funpolish-before-ice2-complete.png` and `funpolish-before-ice3-open.png` reconfirm the missing leaf, stale closed-cover picture and Korean wrapping. The route used existing completed-save replay and source-aware tool selection; it is not a novice playtest.

The candidate was installed at **09:14:43 UTC** on September 11 using ADB server5062, QA `127.0.0.1:5597`, AVD `CurioClerk_WorkbenchQA_20260911`, Android user0. The installed APK hash matches the local artifact. Only this dedicated device received input or installation. The personal AVD at5599 was not installed, restarted or controlled; its previously handed-off Candidate2 remains separate.

| Native route / comparison | Actual evidence and result |
|---|---|
| Korean Ice01–03 | Replayed eight scene actions through Ice03 completion. The Ice02 leaf remains visible in the same watch at Ice03 entry; opening, removing the leaf and restarting produce their respective states. Tap and drag both succeeded. `funpolish-after-ice2-complete.png`, `funpolish-after-ice3-start/open/free/restart/complete.png`. |
| Replay/menu behavior | Leaving a completed-case replay and choosing Replay Case starts at Ice01; `funpolish-after-ice3-resume.png` records that fresh replay introduction despite its filename. This was **not** an unfinished-progress resume test. The route was then replayed to complete the watch checks. |
| Korean and English Rain01–05 | All fourteen actions per language completed via actual input. The four revised prose fields are visible in Rain03 completion, Rain04 introduction and Rain05 completion. Screens `funpolish-ko-rain*` and `funpolish-en-rain*` retain intermediate states and both endings. Independent review found coherent motives/destination and no clipping, overlap or broken Korean words in those six changed-copy screens. |
| Settings and final state | Switched KO→EN→KO and returned to the Korean office, `funpolish-final-office-ko.png`. Start/final Wi-Fi and mobile data settings were 0; Wi-Fi status reported disabled. No ad or consent was required. |
| Save preservation | Installation before/after and native start/final saves are byte-identical: **1,687 bytes**, SHA **`01D1D32008A8F763BF54AD1D37A11BC55B1C46AF2B55C26695FEF782CF895579`**. They retain version4, localeKO, ten records, two completed cases, seven discoveries, zero coins and both data-collection consents false. Final sample: **09:53:24 UTC** September11. |

This native pass covered Korean Ice01–03 and both-language Rain01–05 on a **1080×2400 emulator**, using an already completed save and source-aware helper coordinates. It did not freshly unlock cases, replay Ice04–05, repeat the earlier hint-recovery experiment, or verify process-termination resume. Full Editor coverage and prior candidate/hint evidence remain distinct. No phone performance, audible sound or physical haptics were assessed.

For recorded app PID **11637**, the final log contains **325 lines**, with zero fatal exception/signal, actual ANR, Unity/AndroidRuntime error or specified major managed-exception patterns. A broad exception search finds an offline adservices `ServiceUnavailableException`; emulator EGL errors also remain in the full log. This is not a claim that every log line is error-free.

On September14, `cleanup-20260914T003322294Z` archived **27** raw generated/guard files, then restored only **20** older artifact metas containing trailing-space noise and the empty development keystore setting. The two temporary Editor guard files were archived, not committed. Original checkout **22 dirty-file hashes**, its status/index, **three older APK hashes** and protected worktree inputs passed preservation checks. The new watch PNG/meta were retained exactly. The final-source proof covers **153 generated files**, with one explicitly reviewed generated difference (the restored ProjectSettings file). Other branches, stash and other worktrees were not changed by this cycle.

Independent source/asset and before/after screen reviews found no confirmed material regression in the intended patch. The leaf, opening, removal, restart and bilingual story comparisons support the selected design changes; actual emotional response and broader enjoyment remain unverified. Current native screenshots reconfirm Rain01's missing captured jar and Rain02's unchanged water/name-tag depiction. Ice03's rear frost/crescent is not visible from the front; the later Ice04 watch/key limitations retain historical evidence. These remain open in [QualityReview](../QualityReview.md), rather than being counted as solved by this bounded watch and story pass. Existing unverified native-ad/UMP edge cases remain outside this change.

Evidence paths beneath the ignored `FunPolish/candidate-01/` directory include `build-summary.json`, `final-*-results.xml`, `apk-inspection-20260911T091341266Z/`, `qa-install-Oh4RNj/`, `native-start/`, `native-final/`, `cleanup-20260914T003322294Z/` and `final-source-proof-e0e92781e1a214d2bd0ff53b098976a290dfc7a5.json`. The conditional compiler evidence remains under `Candidate2/Compiler/funpolish-candidate-01/`. Raw saves, logs, screenshots and APKs are not committed.
