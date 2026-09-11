# Workbench test migration — 2026-09-11

The developer rejected repetitive classification as the story's main activity and delegated the redesign. The [approved design](../superpowers/specs/2026-09-11-hands-on-investigations-design.md) replaces both cases with direct object inspection and tools. This migration tests that new player contract rather than retaining an inaccessible sorting route for tests.

## Counts and scope

- Before: 94 GameApp PlayMode tests.
- Retained: 53 tests for menu, free sorting, tutorial components, art, collection, cosmetics, feedback, daily mode, saves and ads. Two menu tests update their exact first-night labels to the new authored copy.
- Retargeted: 5 tests described below; GameApp now contains 58 tests.
- Replaced: 36 obsolete story-route tests, mapped individually below.
- Added: 25 Workbench PlayMode tests: 22 replacement/interaction tests plus three new regressions found during review. This is consolidation of overlapping old story assertions, not a claim that a larger test count implies coverage.
- All 12 IncidentPresentationViewPlayModeTests and existing Core sorting, Hold, IncidentStageRun, IncidentRunner and progress-resolver tests remain. No production flag or hidden alternative story route was added for testing.

Both GameApp fixture helpers immediately replace the disk save store and loaded save with fresh in-memory instances before a frame, interaction or destruction can write. The workbench memory store serializes snapshots so assertions check persisted checkpoints independently of the live mutable save object. Awake's initial disk operation is read-only.

## Five sorting tests retained through the sorting-practice presentation

| Existing method | New route or name |
| --- | --- |
| `UnsafeHold_ShowsWhyItWaitsAndPreservesThePlayableCurioInBothLanguages` | Public `StartNewShift(4242)`, with fresh progression. Checks the actual current artifact survives a rejected Hold; no story queue assumption. |
| `EnglishFallbackRule_ShowsAuthoredDestination` | Regular sorting screen with explicit rule fixtures; verifies non-Storage fallback destinations are rendered correctly. |
| `KoreanFallbackRule_ShowsAuthoredDestination` | Same fallback contract in Korean. |
| `EnglishIncidentRules_FitPortraitPanels` | Renamed `EnglishSortingRules_FitPortraitPanels`; both actual sorting rule packs at 16:9 and tall CanvasScaler dimensions. |
| `KoreanIncidentRules_FitPortraitPanels` | Renamed `KoreanSortingRules_FitPortraitPanels`; equivalent Korean layout coverage. |

The explicit rule fixture uses the existing test-only queue/session injection pattern to exercise the regular sorting presenter. It does not mark the session as an incident or introduce a production entry point.

## Replacement test index

All W identifiers refer to methods in [WorkbenchPlayModeTests.cs](../../Assets/Tests/PlayMode/WorkbenchPlayModeTests.cs).

| ID | Method |
| --- | --- |
| W01 | `FirstIncident_BeginsAnInteractiveWorkbenchWithAnImmediateObjective` |
| W02 | `KoreanOpening_ExplainsThePhysicalProblemThenEntersTheSameObject` |
| W03 | `InspectTarget_RecordsTheClueAndKeepsItVisibleWithoutCompletingAnAction` |
| W04 | `ToolThenTargetTap_AppliesTheRepairAndVisiblyChangesTheObject` |
| W05 | `MissingObservationAndEarlierRepair_ExplainTheBlockAndRemainRecoverable` |
| W06 | `WrongExperiments_NeverChargeProgressOrRequireAnAdAndCanBeCorrected` |
| W07 | `ToolDragOntoObservedTarget_UsesTheRealHandlerAndAppliesOnlyOnce` |
| W08 | `ToolDragOutsideTargets_DoesNotApplyAndLeavesTapControlsUsable` |
| W09 | `ContinueBeforeCompletion_CannotSkipTheProblem` |
| W10 | `Completion_PersistsExactlyOnceBeforeContinuingToTheNextScene` |
| W11 | `RestartIncompleteScene_ClearsExperimentsWithoutChangingTheCheckpoint` |
| W12 | `RestartCompletedScene_CannotEraseTheResolutionOrRepeatItsReward` |
| W13 | `MenuDetour_PreservesCompletedStagesWithoutSkippingAnUnfinishedObject` |
| W14 | `PauseAndProcessRestart_RestoreTheCheckpointWithoutRepeatingItsAward` |
| W15 | `DisableEnable_KeepsTheRepairWithoutRepeatingItsCompletion` |
| W16 | `LegacyMidCaseSave_ResumesItsStableStageAndLanguageChangePreservesProgress` |
| W17 | `ResolvedIceReplay_DoesNotMoveOrRewardTheRememberingRainCheckpoint` |
| W18 | `ResolvedRainReplay_DoesNotDuplicateCompletionsRecordsOrCoins` |
| W19 | `BothCases_AllTenScenesCompleteOfflineThroughButtonsAndExposeTheirNextClue` |
| W20 | `CaseEndings_InBothLanguagesResolveTheCaseAndRevealTheNextBoardState` |
| W21 | `CompletedCaseBoard_DisableEnableKeepsCurrentAndResolvedCardsReadable` |
| W22 | `EveryScene_BilingualObjectivesObservationsAndControlsFitBothPortraitShapes` |
| W23 | `SortingCallbacks_AfterEnteringWorkbenchCannotMutateThePreviousShift` |
| W24 | `PendingSortingReward_AfterEnteringWorkbenchCannotReviveOrReplaceItsScreen` |
| W25 | `ObservedClue_AfterSettingsLanguageChangeResumesInTheSelectedLanguage` |

## Individual old-to-new mapping

| Removed GameApp method | Replacement | Preserved behavior or intentional change |
| --- | --- | --- |
| `CompletedIncident_ReplayStartsAtFirstStageWithoutChangingSavedProgress` | W17 | Replay starts with the first physical scene and leaves the current case save unchanged. |
| `RememberingRain_FirstShiftUsesApprovedKoreanOpeningAndQueue` | W19 | Rain's authored introduction leads to its workbench object; fixed sorting queue is intentionally retired. |
| `RememberingRain_FirstShiftCompletionPersistsAndStartsSecondShift` | W10, W19 | Once-only stage persistence and the next scene are tested through real tool/target buttons. |
| `RememberingRain_FourthShiftResultContinuesAtFifthShift` | W19 | All five Rain scenes run in order, including the fourth-to-fifth transition. |
| `RememberingRain_FinalShiftResolvesOnceAndRevealsReadOnlySuccessor` | W19, W20 | Final completion is recorded once and returns to the next read-only case preview. |
| `RememberingRain_EnglishEndingShowsTheCompletedCase` | W20 | English ending and next-board state render the resolved Rain case. |
| `RememberingRain_KoreanEndingShowsTheCompletedCase` | W20 | Korean ending and next-board state render the resolved Rain case. |
| `RememberingRain_KoreanAcceptanceRouteConnectsAllFiveShifts` | W19 | Full Korean button-driven route covers all ten scenes, findings and both case resolutions. |
| `RememberingRain_ReplayDoesNotDuplicateCompletionRewardsOrRecords` | W18 | Rain replay leaves completion IDs, stage records and coins unchanged. |
| `RememberingRain_OldAndMidCaseSavesResumeAndLanguageSwitchPreservesState` | W16 | Legacy stage ID checkpoint and real Settings language control preserve progress. |
| `ResolvedIceReplay_DoesNotMoveRememberingRainProgress` | W17 | Ice replay cannot move the in-progress Rain checkpoint. |
| `IncidentBoardTransition_DisableAppliesStaticFinalState` | W21 | Current/resolved board cards survive disable-enable after real workbench completion. |
| `IncidentOpening_IsLargeReadableKoreanNarrativeThenStartsAuthoredShift` | W01, W02 | One concrete bilingual request leads to an interactive object rather than a sorting session. |
| `IncidentOpening_ContinuesFromRestoredStageInsteadOfRestartingTheCase` | W16, W22 | Stable stage IDs restore the requested scene; every authored stage enters through a prepared saved checkpoint. |
| `IncidentShift_UsesAuthoredJudgmentLayoutAndLocalizedFrost` | W01, W03, W04, W22 | Objective, inspectable object, visible physical change and readable controls replace the judgment/stamp layout. |
| `FrozenSeal_RequiresHoldingTheWatchAfterTheIceUsesVault` | W05 | Physically grounded observation/action prerequisites replace the frozen-desk Hold gate. |
| `FrozenSeal_ReturningHeldWatchKeepsMovingAndRingsOnlySealedDesks` | W04, W15 | Physical effects and lifecycle persistence replace the held-watch stamp pulse; reaction-view unit tests remain. |
| `FirstIncidentHoldPrompt_TeachesProtectionAndAnOpenDeskInBothLanguages` | W02, W03, W05 | In-context object inspection and prerequisite feedback replace the protective-Hold instruction. |
| `FailedSecondHold_DoesNotRecordTheNewCurrentArtifact` | W05, W06 | Rejected actions do not record completed interventions. Original Hold/resonance core tests remain. |
| `IncidentShift_ShowsCalmFeedbackAfterThreeConsecutiveCorrectSorts` | W04 | Successful intervention produces authored feedback and visible object change; sorting calm streak is obsolete. |
| `IncidentWrongSort_ResetsThePresentationOnlyCalmCounter` | W06 | Incorrect experiments leave stage state intact and remain recoverable; sorting calm streak is obsolete. |
| `IncidentWrongSort_LeadsWithTheRuleAndNextCorrectClosesTheDocketCrack` | W05, W06, W04 | Feedback explains the unmet condition; a correct action repairs the object without charging a heart. |
| `IncidentOrdinaryCorrect_UsesTheProceduralCueWithoutAKeyReaction` | W04 | Concrete physical intervention supplies feedback without a separate narrative continue tap. |
| `IncidentLeadIce_OwnsTheCardWithAuthoredReactionThenFilesWithoutAnotherTap` | W04 | Ice repair visibly changes the object and shows its authored consequence without a follow-up tap. |
| `IncidentLeadUmbrella_UsesItsAuthoredStageReaction` | W19, W20 | The umbrella's workbench actions, findings and ending are covered through actual buttons. |
| `IncidentLeadReaction_DisableThenEnableContinuesFilingExactlyOnce` | W15 | Disable-enable retains applied intervention and cannot duplicate completion. |
| `IncidentLeadReaction_RebuildingTheScreenFlushesTheFilingContinuationExactlyOnce` | W13, W15 | Menu reconstruction/lifecycle transitions preserve completed progress and current-stage validity. |
| `IncidentPendingTransition_PauseAndDestroyEachFlushExactlyOnce` | W14 | Pause, destruction and a new app instance restore the last completed-stage checkpoint without extra reward. |
| `IncidentDocketComplete_RevealsConnectedSigilAndWarmsDeskBeforeAdvancing` | W04, W10 | Physical change plus exactly-once stage resolution replaces three-stamp docket feedback. |
| `RememberingRain_DocketInterludesBlockInputAdvanceOnceAndSkipFinalDocket` | W09, W10, W19 | Incomplete scenes cannot continue; one completed scene advances once. Old docket interlude gates are obsolete. |
| `IncidentDocketInterlude_DisabledViewFlushesOwnedContinuationExactlyOnce` | W15 | Disable-enable preserves the current intervention and does not repeat completion. |
| `IncidentSuccess_PersistsQualityOncePlaysOutroAndStartsTheNextAuthoredStage` | W10 | Completion crosses the memory persistence boundary once before the next scene is shown. |
| `IncidentResults_AllQualitiesShowBilingualBodiesAndAllowTheNextShift` | W05, W06, W20 | Exploration no longer assigns a sorting quality. Harmless mistakes remain solvable and both endings localize. |
| `IncidentFailure_DoesNotAdvanceOrOfferAnAdAndRetriesTheSameStageImmediately` | W06, W11, W12 | Mistakes are harmless, retry clears only an incomplete scene, completed scenes cannot repeat rewards, no ad gate. |
| `FinalIncidentStage_RecedesFrostWarmsTheOfficeAndLeavesTheUmbrellaHook` | W04, W19, W20 | Ice's interventions change its visual state and resolution reveals the next case in both languages. |
| `KoreanStoryAndArtifactCopy_UseReadableBodyTypography` | W02, W22 | Bilingual introductions, objectives, observations and tool controls stay legible on both portrait shapes. |

## Evidence and limits

The initial compiling regression failed at `WorkbenchScreen` expected non-null in the original workspace's `Logs/ImmersiveCases-20260911/PlayMode-red2.xml`. The earlier RED attempt had test compile errors and is not a valid behavior failure. The initial 22 WorkbenchPlayModeTests passed in `PlayMode-green2.xml`; that run also contains four intentionally failing atmosphere tests owned by a separate implementation task. W23–W25 subsequently capture stale sorting input, delayed rewarded-ad callbacks and an observed clue that retained its previous language after changing Settings. Parent orchestration owns Unity execution and the final combined result logs.

Workbench route tests exercise the actual buttons. Drag tests dispatch the real EventSystem begin-drag/drag/end-drag handlers, including cancellation outside targets and duplicate release. Core prerequisite and malformed-input coverage stays in WorkbenchSessionTests; catalog validation independently checks all references, bilingual copy and solvability.

Automated route completion does not establish novice comprehension, immersion or fun. Native drag/tap, visual review, sound and physical-device behavior require their separately recorded validation.
