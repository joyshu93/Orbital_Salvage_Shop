using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using CurioClerk.Content.Workbench;
using CurioClerk.Core.Incidents;
using CurioClerk.Core.Progression;
using CurioClerk.Infrastructure;
using CurioClerk.Infrastructure.Ads;
using CurioClerk.Infrastructure.Privacy;
using CurioClerk.Infrastructure.Save;
using CurioClerk.Localization;
using CurioClerk.Presentation;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace CurioClerk.Tests.PlayMode
{
    public sealed class WorkbenchPlayModeTests
    {
        private GameApp _app;
        private MemorySaveStore _store;
        private static readonly string[] IceStages =
            { "ice-01-crack", "ice-02-spread", "ice-03-tomorrow", "ice-04-frozen-seal", "ice-05-thaw" };
        private static readonly string[] RainStages =
            { "rain-01-voices", "rain-02-names-under-water", "rain-03-unsent-letter", "rain-04-dry-order", "rain-05-testimony" };

        [TearDown]
        public void TearDown()
        {
            if (_app != null) UnityEngine.Object.DestroyImmediate(_app.gameObject);
            ServiceFactory.ResetTestServices();
        }

        [UnityTest]
        public IEnumerator FirstIncident_BeginsAnInteractiveWorkbenchWithAnImmediateObjective()
        {
            _app = CreateApp(new PlayerSaveData { locale = "en" });
            _app.StartIncident();
            _app.BeginIncidentStage();
            yield return null;

            Assert.That(GameObject.Find("WorkbenchScreen"), Is.Not.Null,
                "Starting a story stage must let the player inspect and change its object.");
            Assert.That(GameObject.Find("WorkbenchObjective"), Is.Not.Null,
                "The immediate physical problem must stay visible while the player works.");
            Assert.That(GameObject.Find("WorkbenchArtifact"), Is.Not.Null);
            Assert.That(GameObject.Find("RepairButton"), Is.Null,
                "The story route must not return to the destination-stamp sorting activity.");
        }

        [UnityTest]
        public IEnumerator KoreanOpening_ExplainsThePhysicalProblemThenEntersTheSameObject()
        {
            _app = CreateApp(new PlayerSaveData { locale = "ko" });
            Click("IncidentButton");
            yield return null;
            Assert.That(_app.ActiveScreen, Is.EqualTo(AppScreen.Narrative));
            Assert.That(Text("NarrativeBody"), Is.EqualTo(WorkbenchCatalog.Find("ice-01-crack").Introduction.Korean));
            Click("NarrativeContinueButton");
            yield return null;
            Assert.That(_app.IsWorkbenchActive, Is.True, "One request must lead directly to the object.");
            Assert.That(Text("WorkbenchObjective"), Does.Contain("냉기"));
            Assert.That(_app.ActiveWorkbench.Puzzle.Id, Is.EqualTo("ice-01-crack"));
        }

        [UnityTest]
        public IEnumerator InspectTarget_RecordsTheClueAndKeepsItVisibleWithoutCompletingAnAction()
        {
            yield return BeginFirst();
            Inspect("crack");
            yield return null;
            Assert.That(_app.ActiveWorkbench.HasObserved("crack"), Is.True);
            Assert.That(Text("WorkbenchObservation"), Does.Contain(Scene.Targets.Single(t => t.Id == "crack").Observation.English));
            Assert.That(_app.ActiveWorkbench.CompletedSteps, Is.Empty);
            var observation = Text("WorkbenchObservation");
            Inspect("crack");
            Assert.That(_app.ActiveWorkbench.ObservedTargets.Count, Is.EqualTo(1));
            Assert.That(Text("WorkbenchObservation"), Is.EqualTo(observation));
            Assert.That(_app.SaveData.incidentStageRecords, Is.Empty);
        }

        [UnityTest]
        public IEnumerator ToolThenTargetTap_AppliesTheRepairAndVisiblyChangesTheObject()
        {
            yield return BeginFirst();
            Inspect("crack");
            var observed = Text("WorkbenchObservation");
            var frost = GameObject.Find("WorkbenchFrost").GetComponent<Image>();
            var beforeFrost = frost.color.a;
            Click("WorkbenchTool_dry-cloth");
            Click("WorkbenchTarget_crack");
            yield return null;
            Assert.That(_app.ActiveWorkbench.HasCompleted("dry-crack"), Is.True);
            Assert.That(_app.ActiveWorkbench.IsComplete, Is.False);
            Assert.That(Text("WorkbenchObservation"), Is.Not.EqualTo(observed));
            Assert.That(Text("WorkbenchObservation"), Does.Contain(Scene.Actions[0].Result.English));
            Assert.That(frost.color.a, Is.LessThan(beforeFrost), "The leaking cold must visibly recede after a repair.");
            Assert.That(GameObject.Find("WorkbenchEffect_dry-crack").GetComponent<Image>().color.a, Is.GreaterThan(0f));
            Assert.That(_app.SaveData.incidentStageRecords, Is.Empty);
        }

        [UnityTest]
        public IEnumerator MissingObservationAndEarlierRepair_ExplainTheBlockAndRemainRecoverable()
        {
            yield return BeginFirst();
            _app.UseWorkbenchTool("dry-cloth", "crack");
            Assert.That(_app.ActiveWorkbench.CompletedSteps, Is.Empty);
            Assert.That(Text("WorkbenchObservation"), Is.Not.Empty);
            Inspect("crack");
            Inspect("leaf");
            _app.UseWorkbenchTool("insulating-putty", "crack");
            Assert.That(_app.ActiveWorkbench.CompletedSteps, Is.Empty, "The wet crack must be dried before sealing.");
            Assert.That(Text("WorkbenchObservation"), Is.Not.Empty);
            _app.UseWorkbenchTool("dry-cloth", "crack");
            _app.UseWorkbenchTool("insulating-putty", "crack");
            yield return null;
            Assert.That(_app.ActiveWorkbench.IsComplete, Is.True);
            Assert.That(_app.SaveData.incidentStageRecords.Select(r => r.stageId), Is.EqualTo(new[] { "ice-01-crack" }));
        }

        [UnityTest]
        public IEnumerator WrongExperiments_NeverChargeProgressOrRequireAnAdAndCanBeCorrected()
        {
            yield return BeginFirst("ko");
            var before = JsonUtility.ToJson(_app.SaveData);
            for (var attempt = 0; attempt < 6; attempt++)
            {
                Click("WorkbenchTool_dry-cloth");
                Click("WorkbenchTarget_base");
            }
            yield return null;
            Assert.That(_app.ActiveWorkbench.CompletedSteps, Is.Empty);
            Assert.That(JsonUtility.ToJson(_app.SaveData), Is.EqualTo(before));
            Assert.That(Text("WorkbenchObservation"), Is.Not.Empty);
            Assert.That(GameObject.Find("RewardedAdButton"), Is.Null);
            Assert.That(GameObject.Find("RetryStageButton"), Is.Null);
            yield return CompleteThroughButtons();
        }

        [UnityTest]
        public IEnumerator ToolDragOntoObservedTarget_UsesTheRealHandlerAndAppliesOnlyOnce()
        {
            yield return BeginFirst();
            Inspect("crack");
            var tool = GameObject.Find("WorkbenchTool_dry-cloth");
            var target = GameObject.Find("WorkbenchTarget_crack").GetComponent<RectTransform>();
            Canvas.ForceUpdateCanvases();
            var pointer = DragPointer(tool, RectTransformUtility.WorldToScreenPoint(null, target.TransformPoint(target.rect.center)));
            Assert.That(ExecuteEvents.Execute(tool, pointer, ExecuteEvents.beginDragHandler), Is.True);
            ExecuteEvents.Execute(tool, pointer, ExecuteEvents.dragHandler);
            Assert.That(ExecuteEvents.Execute(tool, pointer, ExecuteEvents.endDragHandler), Is.True);
            yield return null;
            Assert.That(_app.ActiveWorkbench.HasCompleted("dry-crack"), Is.True);
            ExecuteEvents.Execute(tool, pointer, ExecuteEvents.endDragHandler);
            Assert.That(_app.ActiveWorkbench.CompletedSteps.Count, Is.EqualTo(1));
            Assert.That(_app.SaveData.incidentStageRecords, Is.Empty);
        }

        [UnityTest]
        public IEnumerator ToolDragOutsideTargets_DoesNotApplyAndLeavesTapControlsUsable()
        {
            yield return BeginFirst();
            Inspect("crack");
            var tool = GameObject.Find("WorkbenchTool_dry-cloth");
            var pointer = DragPointer(tool, new Vector2(-100f, -100f));
            ExecuteEvents.Execute(tool, pointer, ExecuteEvents.beginDragHandler);
            ExecuteEvents.Execute(tool, pointer, ExecuteEvents.dragHandler);
            ExecuteEvents.Execute(tool, pointer, ExecuteEvents.endDragHandler);
            yield return null;
            Assert.That(_app.ActiveWorkbench.CompletedSteps, Is.Empty);
            Click("WorkbenchTool_dry-cloth");
            Click("WorkbenchTarget_crack");
            Assert.That(_app.ActiveWorkbench.HasCompleted("dry-crack"), Is.True);
        }

        [UnityTest]
        public IEnumerator ContinueBeforeCompletion_CannotSkipTheProblem()
        {
            yield return BeginFirst();
            var button = GameObject.Find("WorkbenchContinueButton");
            Assert.That(button == null || !button.GetComponent<Button>().IsInteractable(), Is.True);
            _app.ContinueWorkbench();
            yield return null;
            Assert.That(_app.IsWorkbenchActive, Is.True);
            Assert.That(_app.ActiveWorkbench.Puzzle.Id, Is.EqualTo("ice-01-crack"));
            Assert.That(_app.SaveData.activeIncidentStage, Is.Zero);
            Assert.That(_app.SaveData.incidentStageRecords, Is.Empty);
        }

        [UnityTest]
        public IEnumerator Completion_PersistsExactlyOnceBeforeContinuingToTheNextScene()
        {
            yield return BeginFirst();
            var savesBefore = _store.SaveCalls;
            yield return CompleteThroughButtons();
            Assert.That(_app.ActiveScreen, Is.EqualTo(AppScreen.Workbench));
            Assert.That(_store.SaveCalls - savesBefore, Is.EqualTo(1));
            Assert.That(_store.Snapshot.activeIncidentStage, Is.EqualTo(1));
            Assert.That(_store.Snapshot.incidentStageRecords, Has.Count.EqualTo(1));
            Assert.That(VisibleWorkbenchText(), Does.Contain(Scene.Ending.English));
            Assert.That(VisibleWorkbenchText(), Does.Contain(Scene.Discovery.English));
            var saved = JsonUtility.ToJson(_app.SaveData);
            _app.UseWorkbenchTool("insulating-putty", "crack");
            _app.InspectWorkbenchTarget("leaf");
            Assert.That(JsonUtility.ToJson(_app.SaveData), Is.EqualTo(saved));
            Assert.That(_store.SaveCalls - savesBefore, Is.EqualTo(1));
            Click("WorkbenchContinueButton");
            _app.ContinueWorkbench();
            yield return null;
            Assert.That(_app.ActiveScreen, Is.EqualTo(AppScreen.Narrative));
            Assert.That(Text("NarrativeBody"), Is.EqualTo(WorkbenchCatalog.Find("ice-02-spread").Introduction.English));
            yield return AdvanceIntro();
            Assert.That(_app.ActiveWorkbench.Puzzle.Id, Is.EqualTo("ice-02-spread"));
        }

        [UnityTest]
        public IEnumerator RestartIncompleteScene_ClearsExperimentsWithoutChangingTheCheckpoint()
        {
            yield return BeginFirst();
            Inspect("crack");
            Click("WorkbenchTool_dry-cloth");
            Click("WorkbenchTarget_crack");
            var previous = _app.ActiveWorkbench;
            var saved = JsonUtility.ToJson(_app.SaveData);
            Click("WorkbenchRestartButton");
            yield return null;
            Assert.That(_app.ActiveWorkbench, Is.Not.SameAs(previous));
            Assert.That(_app.ActiveWorkbench.Puzzle.Id, Is.EqualTo("ice-01-crack"));
            Assert.That(_app.ActiveWorkbench.ObservedTargets, Is.Empty);
            Assert.That(_app.ActiveWorkbench.CompletedSteps, Is.Empty);
            Assert.That(JsonUtility.ToJson(_app.SaveData), Is.EqualTo(saved));
            yield return CompleteThroughButtons();
            Assert.That(_app.SaveData.incidentStageRecords, Has.Count.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator RestartCompletedScene_CannotEraseTheResolutionOrRepeatItsReward()
        {
            yield return BeginFirst();
            yield return CompleteThroughButtons();
            var saved = JsonUtility.ToJson(_app.SaveData);
            var savesBefore = _store.SaveCalls;
            _app.RestartWorkbench();
            _app.UseWorkbenchTool("insulating-putty", "crack");
            yield return null;
            Assert.That(_app.ActiveWorkbench.IsComplete, Is.True);
            Assert.That(JsonUtility.ToJson(_app.SaveData), Is.EqualTo(saved));
            Assert.That(_store.SaveCalls, Is.EqualTo(savesBefore));
        }

        [UnityTest]
        public IEnumerator MenuDetour_PreservesCompletedStagesWithoutSkippingAnUnfinishedObject()
        {
            yield return BeginFirst();
            yield return CompleteThroughButtons();
            Click("WorkbenchContinueButton");
            yield return AdvanceIntro();
            var secondStage = _app.ActiveWorkbench.Puzzle.Id;
            Inspect(Scene.Targets.First(t => t.RevealAfterStep == null).Id);
            Click("WorkbenchMenuButton");
            yield return null;
            Assert.That(_app.ActiveScreen, Is.EqualTo(AppScreen.Menu));
            Assert.That(_app.SaveData.activeIncidentStage, Is.EqualTo(1));
            Assert.That(_app.SaveData.incidentStageRecords, Has.Count.EqualTo(1));
            Click("IncidentButton");
            yield return AdvanceIntro();
            Assert.That(_app.ActiveWorkbench.Puzzle.Id, Is.EqualTo(secondStage));
            Assert.That(_app.ActiveWorkbench.IsComplete, Is.False);
        }

        [UnityTest]
        public IEnumerator PauseAndProcessRestart_RestoreTheCheckpointWithoutRepeatingItsAward()
        {
            yield return BeginFirst();
            yield return CompleteThroughButtons();
            Click("WorkbenchContinueButton");
            yield return AdvanceIntro();
            Inspect(Scene.Targets.First(t => t.RevealAfterStep == null).Id);
            _app.SendMessage("OnApplicationPause", true);
            var persisted = _store.Snapshot;
            var coins = persisted.coins;
            UnityEngine.Object.DestroyImmediate(_app.gameObject);
            _app = CreateApp(persisted);
            Click("IncidentButton");
            yield return AdvanceIntro();
            Assert.That(_app.ActiveWorkbench.Puzzle.Id, Is.EqualTo("ice-02-spread"));
            Assert.That(_app.ActiveWorkbench.CompletedSteps, Is.Empty);
            Assert.That(_app.ActiveWorkbench.ObservedTargets, Is.Empty);
            Assert.That(_app.SaveData.incidentStageRecords, Has.Count.EqualTo(1));
            Assert.That(_app.SaveData.coins, Is.EqualTo(coins));
        }

        [UnityTest]
        public IEnumerator DisableEnable_KeepsTheRepairWithoutRepeatingItsCompletion()
        {
            yield return BeginFirst();
            Inspect("crack");
            Click("WorkbenchTool_dry-cloth");
            Click("WorkbenchTarget_crack");
            var session = _app.ActiveWorkbench;
            _app.gameObject.SetActive(false);
            yield return null;
            _app.gameObject.SetActive(true);
            yield return null;
            Assert.That(_app.ActiveWorkbench, Is.SameAs(session));
            Assert.That(session.CompletedSteps.Count, Is.EqualTo(1));
            Inspect("leaf");
            Click("WorkbenchTool_insulating-putty");
            Click("WorkbenchTarget_crack");
            Assert.That(session.IsComplete, Is.True);
            var before = JsonUtility.ToJson(_app.SaveData);
            _app.gameObject.SetActive(false);
            _app.gameObject.SetActive(true);
            yield return null;
            Assert.That(JsonUtility.ToJson(_app.SaveData), Is.EqualTo(before));
            Assert.That(_app.SaveData.incidentStageRecords, Has.Count.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator LegacyMidCaseSave_ResumesItsStableStageAndLanguageChangePreservesProgress()
        {
            var save = SaveAt("remembering-rain", 3, "ko");
            save.coins = 137;
            _app = CreateApp(save);
            Click("IncidentButton");
            yield return AdvanceIntro();
            Assert.That(_app.ActiveWorkbench.Puzzle.Id, Is.EqualTo("rain-04-dry-order"));
            Assert.That(Text("WorkbenchObjective"), Is.EqualTo(Scene.Objective.Korean));
            Click("WorkbenchMenuButton");
            Click("SettingsButton");
            Click("EnglishButton");
            Assert.That(_app.SaveData.locale, Is.EqualTo("en"));
            _app.ShowMenu();
            Click("IncidentButton");
            yield return AdvanceIntro();
            Assert.That(_app.ActiveWorkbench.Puzzle.Id, Is.EqualTo("rain-04-dry-order"));
            Assert.That(Text("WorkbenchObjective"), Is.EqualTo(Scene.Objective.English));
            Assert.That(_app.SaveData.activeIncidentStage, Is.EqualTo(3));
            Assert.That(_app.SaveData.incidentStageRecords, Has.Count.EqualTo(3));
            Assert.That(_app.SaveData.coins, Is.EqualTo(137));
        }

        [UnityTest]
        public IEnumerator ResolvedIceReplay_DoesNotMoveOrRewardTheRememberingRainCheckpoint()
        {
            _app = CreateApp(SaveAt("remembering-rain", 1, "ko"));
            var before = JsonUtility.ToJson(_app.SaveData);
            Click("ReplayIncident_unmelting-ice");
            yield return AdvanceIntro();
            Assert.That(_app.ActiveWorkbench.Puzzle.Id, Is.EqualTo("ice-01-crack"));
            yield return CompleteThroughButtons();
            Assert.That(JsonUtility.ToJson(_app.SaveData), Is.EqualTo(before));
            Assert.That(_store.SaveCalls, Is.Zero);
            Click("WorkbenchMenuButton");
            Click("IncidentButton");
            yield return AdvanceIntro();
            Assert.That(_app.ActiveWorkbench.Puzzle.Id, Is.EqualTo("rain-02-names-under-water"));
        }

        [UnityTest]
        public IEnumerator ResolvedRainReplay_DoesNotDuplicateCompletionsRecordsOrCoins()
        {
            var save = SaveAt("remembering-rain", 5, "en");
            save.completedIncidentIds.Add("remembering-rain");
            save.coins = 217;
            _app = CreateApp(save);
            var before = JsonUtility.ToJson(_app.SaveData);
            Click("ReplayIncident_remembering-rain");
            yield return AdvanceIntro();
            Assert.That(_app.ActiveWorkbench.Puzzle.Id, Is.EqualTo("rain-01-voices"));
            yield return CompleteThroughButtons();
            Assert.That(JsonUtility.ToJson(_app.SaveData), Is.EqualTo(before));
            Assert.That(_store.SaveCalls, Is.Zero);
        }

        [UnityTest]
        public IEnumerator BothCases_AllTenScenesCompleteOfflineThroughButtonsAndExposeTheirNextClue()
        {
            _app = CreateApp(new PlayerSaveData { locale = "ko" });
            foreach (var stageIds in new[] { IceStages, RainStages })
            {
                Click("IncidentButton");
                for (var index = 0; index < stageIds.Length; index++)
                {
                    yield return AdvanceIntro();
                    Assert.That(_app.ActiveWorkbench.Puzzle.Id, Is.EqualTo(stageIds[index]));
                    var scene = Scene;
                    yield return CompleteThroughButtons();
                    Assert.That(_app.SaveData.incidentStageRecords.Count(r => r.stageId == stageIds[index]), Is.EqualTo(1));
                    Assert.That(VisibleWorkbenchText(), Does.Contain(scene.Ending.Korean));
                    Assert.That(VisibleWorkbenchText(), Does.Contain(scene.Discovery.Korean));
                    Assert.That(GameObject.Find("RewardedAdButton"), Is.Null);
                    Click("WorkbenchContinueButton");
                    yield return null;
                }
                Assert.That(_app.ActiveScreen, Is.EqualTo(AppScreen.Menu));
            }
            Assert.That(_app.SaveData.completedIncidentIds, Is.EquivalentTo(new[] { "unmelting-ice", "remembering-rain" }));
            Assert.That(_app.SaveData.incidentStageRecords, Has.Count.EqualTo(10));
            Assert.That(GameObject.Find("IncidentButton"), Is.Null);
            Assert.That(GameObject.Find("ReplayIncident_unmelting-ice"), Is.Not.Null);
            Assert.That(GameObject.Find("ReplayIncident_remembering-rain"), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator CaseEndings_InBothLanguagesResolveTheCaseAndRevealTheNextBoardState()
        {
            foreach (var locale in new[] { "en", "ko" })
            foreach (var incidentId in new[] { "unmelting-ice", "remembering-rain" })
            {
                if (_app != null) UnityEngine.Object.DestroyImmediate(_app.gameObject);
                _app = CreateApp(SaveAt(incidentId, 4, locale));
                Click("IncidentButton");
                yield return AdvanceIntro();
                var scene = Scene;
                yield return CompleteThroughButtons();
                Assert.That(VisibleWorkbenchText(), Does.Contain(scene.Ending.ForLocale(locale)));
                Assert.That(VisibleWorkbenchText(), Does.Contain(scene.Discovery.ForLocale(locale)));
                Assert.That(_app.SaveData.completedIncidentIds.Count(id => id == incidentId), Is.EqualTo(1));
                Click("WorkbenchContinueButton");
                yield return null;
                Assert.That(_app.ActiveScreen, Is.EqualTo(AppScreen.Menu));
                Assert.That(GameObject.Find("ReplayIncident_" + incidentId), Is.Not.Null);
                Assert.That(GameObject.Find("IncidentButton") != null, Is.EqualTo(incidentId == "unmelting-ice"));
            }
        }

        [UnityTest]
        public IEnumerator CompletedCaseBoard_DisableEnableKeepsCurrentAndResolvedCardsReadable()
        {
            _app = CreateApp(SaveAt("unmelting-ice", 4, "en"));
            Click("IncidentButton");
            yield return AdvanceIntro();
            yield return CompleteThroughButtons();
            Click("WorkbenchContinueButton");
            yield return null;
            _app.gameObject.SetActive(false);
            yield return null;
            _app.gameObject.SetActive(true);
            yield return null;
            foreach (var name in new[] { "CurrentIncidentCard", "ResolvedIncidentCard_unmelting-ice" })
            {
                var card = GameObject.Find(name);
                Assert.That(card, Is.Not.Null);
                Assert.That(card.GetComponent<CanvasGroup>().alpha, Is.EqualTo(1f).Within(.001f));
                Assert.That(card.transform.localScale, Is.EqualTo(Vector3.one));
            }
        }

        [UnityTest]
        public IEnumerator EveryScene_BilingualObjectivesObservationsAndControlsFitBothPortraitShapes()
        {
            foreach (var locale in new[] { "en", "ko" })
            {
                if (_app != null) UnityEngine.Object.DestroyImmediate(_app.gameObject);
                _app = CreateApp(new PlayerSaveData { locale = locale });
                foreach (var scene in WorkbenchCatalog.All)
                {
                    var ice = scene.StageId.StartsWith("ice-", StringComparison.Ordinal);
                    SetField(_app, "_save", SaveAt(ice ? "unmelting-ice" : "remembering-rain",
                        Array.IndexOf(ice ? IceStages : RainStages, scene.StageId), locale));
                    _app.ShowMenu();
                    Click("IncidentButton");
                    yield return AdvanceIntro();
                    var root = GameObject.Find("ScreenRoot").GetComponent<RectTransform>();
                    root.anchorMin = root.anchorMax = new Vector2(.5f, .5f);
                    foreach (var height in new[] { 1920f, 2400f })
                    {
                        var scale = Mathf.Sqrt(height / 1920f);
                        root.sizeDelta = new Vector2(1080f / scale, height / scale);
                        Canvas.ForceUpdateCanvases();
                        AssertReadable(FindText("WorkbenchObjective"), locale + "/" + scene.StageId);
                        foreach (var tool in scene.Tools)
                            AssertReadable(FindText("WorkbenchTool_" + tool.Id), locale + "/" + scene.StageId);
                        foreach (var target in scene.Targets.Where(t => t.RevealAfterStep == null))
                        {
                            Inspect(target.Id);
                            AssertReadable(FindText("WorkbenchObservation"), locale + "/" + scene.StageId + "/" + target.Id);
                        }
                    }
                }
            }
        }

        [UnityTest]
        public IEnumerator SortingCallbacks_AfterEnteringWorkbenchCannotMutateThePreviousShift()
        {
            _app = CreateApp(new PlayerSaveData());
            _app.StartNewShift(4242);
            var previous = (CurioClerk.Core.Shifts.ShiftSession)GetField(_app, "_session");
            var previousArtifact = previous.CurrentArtifact.Id;
            var expectedDestination = previous.CurrentResolution.Destination;
            _app.ShowMenu();
            Click("IncidentButton");
            yield return AdvanceIntro();
            var workbench = _app.ActiveWorkbench;
            var saved = JsonUtility.ToJson(_app.SaveData);

            Assert.DoesNotThrow(_app.HoldCurrent);
            Assert.DoesNotThrow(() => _app.ChooseDestination(expectedDestination));
            yield return null;

            Assert.That(_app.ActiveScreen, Is.EqualTo(AppScreen.Workbench));
            Assert.That(_app.ActiveWorkbench, Is.SameAs(workbench));
            Assert.That(previous.CurrentArtifact.Id, Is.EqualTo(previousArtifact));
            Assert.That(previous.HeldArtifact, Is.Null);
            Assert.That(previous.CorrectSorts, Is.Zero);
            Assert.That(JsonUtility.ToJson(_app.SaveData), Is.EqualTo(saved));
        }

        [UnityTest]
        public IEnumerator PendingSortingReward_AfterEnteringWorkbenchCannotReviveOrReplaceItsScreen()
        {
            var ad = new DeferredWorkbenchAdService();
            _app = CreateApp(new PlayerSaveData(), ad, new AllowedPrivacyService());
            _app.StartNewShift(4242);
            var previous = (CurioClerk.Core.Shifts.ShiftSession)GetField(_app, "_session");
            var correct = previous.CurrentResolution.Destination;
            var wrong = correct == CurioClerk.Core.Rules.Destination.Repair
                ? CurioClerk.Core.Rules.Destination.Storage : CurioClerk.Core.Rules.Destination.Repair;
            _app.ChooseDestination(wrong);
            _app.ChooseDestination(wrong);
            _app.ChooseDestination(wrong);
            var deadline = Time.realtimeSinceStartup + 6f;
            while (_app.ActiveScreen != AppScreen.Results && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(_app.ActiveScreen, Is.EqualTo(AppScreen.Results));
            // Exercise the existing ad boundary, also used by the sorting reward tests.
            typeof(GameApp).GetMethod("RequestReward", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(_app, new object[] { false });
            Assert.That(ad.HasPendingRequest, Is.True);
            Click("ResultsContinueButton");
            Click("IncidentButton");
            yield return AdvanceIntro();
            var workbench = _app.ActiveWorkbench;
            var saved = JsonUtility.ToJson(_app.SaveData);

            Assert.DoesNotThrow(() => ad.Complete(RewardedAdResult.Earned));
            yield return null;

            Assert.That(_app.ActiveScreen, Is.EqualTo(AppScreen.Workbench));
            Assert.That(_app.ActiveWorkbench, Is.SameAs(workbench));
            Assert.That(previous.State, Is.EqualTo(CurioClerk.Core.Shifts.ShiftState.Failed));
            Assert.That(previous.Hearts, Is.Zero);
            Assert.That(JsonUtility.ToJson(_app.SaveData), Is.EqualTo(saved));
        }

        [UnityTest]
        public IEnumerator ObservedClue_AfterSettingsLanguageChangeResumesInTheSelectedLanguage()
        {
            yield return BeginFirst("ko");
            Inspect("crack");
            var clue = Scene.Targets.Single(t => t.Id == "crack");
            Assert.That(Text("WorkbenchObservation"), Does.Contain(clue.Observation.Korean));
            Click("WorkbenchMenuButton");
            Click("SettingsButton");
            Click("EnglishButton");
            Click("SettingsBackButton");
            Click("IncidentButton");
            yield return AdvanceIntro();

            Assert.That(_app.ActiveWorkbench.HasObserved("crack"), Is.True);
            Assert.That(Text("WorkbenchObservation"), Does.Contain(clue.Observation.English));
            Assert.That(Text("WorkbenchObservation"), Does.Not.Contain(clue.Observation.Korean));
        }


        private IEnumerator BeginFirst(string locale = "en")
        {
            _app = CreateApp(new PlayerSaveData { locale = locale });
            Click("IncidentButton");
            yield return AdvanceIntro();
        }

        [UnityTest]
        public IEnumerator ProgressLabel_DistinguishesCaseNumberFromStageAndTotalInBothLanguages()
        {
            _app = CreateApp(new PlayerSaveData { locale = "en" });
            Click("IncidentButton");
            yield return AdvanceIntro();
            Assert.That(Text("WorkbenchChapter"), Is.EqualTo("CASE 1 · 1/5"));
            UnityEngine.Object.DestroyImmediate(_app.gameObject);
            _app = CreateApp(SaveAt("remembering-rain", 3, "ko"));
            Click("IncidentButton");
            yield return AdvanceIntro();
            Assert.That(Text("WorkbenchChapter"), Is.EqualTo("사건 2 · 4/5"));
        }

        [UnityTest]
        public IEnumerator FirstTools_HaveDistinctRenderedSilhouettesWithoutInterceptingInput()
        {
            yield return BeginFirst();
            Canvas.ForceUpdateCanvases();
            var cloth = GameObject.Find("WorkbenchTool_dry-cloth").GetComponentInChildren<WorkbenchToolIcon>();
            var putty = GameObject.Find("WorkbenchTool_insulating-putty").GetComponentInChildren<WorkbenchToolIcon>();
            Assert.That(cloth.raycastTarget, Is.False);
            Assert.That(putty.raycastTarget, Is.False);
            var clothGeometry = ReadIconGeometry(cloth);
            var puttyGeometry = ReadIconGeometry(putty);
            Assert.That(clothGeometry.Length, Is.GreaterThan(0));
            Assert.That(puttyGeometry.Length, Is.GreaterThan(0));
            Assert.That(clothGeometry.SequenceEqual(puttyGeometry), Is.False,
                "The first two physical tools must not render as the same generic paper outline.");
            putty.Configure("blotting-paper");
            var paperGeometry = ReadIconGeometry(putty);
            foreach (var id in new[] { "dry-cloth", "insulating-putty", "warm-pad", "rubber-grip",
                "wooden-wedge", "thin-lever", "paper-folder", "brass-lid", "glass-dropper", "soft-ribbon", "sealed-reply" })
            {
                cloth.Configure(id);
                Assert.That(ReadIconGeometry(cloth).SequenceEqual(paperGeometry), Is.False,
                    id + " must show its physical tool family rather than a generic paper sheet.");
            }
        }

        [UnityTest]
        public IEnumerator CreamPanelCopy_MatchesObjectiveInkAndStrokeWeightInBothLanguages()
        {
            foreach (var locale in new[] { "en", "ko" })
            {
                if (_app != null) UnityEngine.Object.DestroyImmediate(_app.gameObject);
                _app = CreateApp(new PlayerSaveData { locale = locale });
                Click("IncidentButton");
                yield return null;
                AssertReadableCreamText("NarrativeBody");
                yield return AdvanceIntro();
                AssertReadableCreamText("WorkbenchObjective");
                AssertReadableCreamText("WorkbenchObservation");
                Inspect("crack");
                AssertReadableCreamText("WorkbenchObservation");
                yield return CompleteThroughButtons();
                AssertReadableCreamText("WorkbenchActionResult");
                AssertReadableCreamText("WorkbenchEndingText");
                AssertReadableCreamText("WorkbenchDiscovery");
                Assert.That(GameObject.Find("WorkbenchTitle").GetComponent<TMP_Text>().color.grayscale,
                    Is.GreaterThan(.7f), "The plum-background title must keep its light color.");
            }
        }

        private static Vector3[] ReadIconGeometry(WorkbenchToolIcon icon)
        {
            using (var helper = new VertexHelper())
            {
                typeof(WorkbenchToolIcon).GetMethod("OnPopulateMesh",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly,
                    null, new[] { typeof(VertexHelper) }, null)
                    .Invoke(icon, new object[] { helper });
                var stream = new System.Collections.Generic.List<UIVertex>();
                helper.GetUIVertexStream(stream);
                return stream.Select(vertex => vertex.position).ToArray();
            }
        }

        [UnityTest]
        public IEnumerator ResolvedObjects_ClearTemporaryMarksForBothRevealedAndOriginalArtwork()
        {
            foreach (var start in new[] { (incident: "unmelting-ice", stage: 3), (incident: "remembering-rain", stage: 3) })
            {
                if (_app != null) UnityEngine.Object.DestroyImmediate(_app.gameObject);
                _app = CreateApp(SaveAt(start.incident, start.stage, "en"));
                Click("IncidentButton");
                yield return AdvanceIntro();
                var originalSprite = GameObject.Find("WorkbenchArtifact").GetComponent<Image>().sprite;
                yield return CompleteThroughButtons();
                if (start.incident == "unmelting-ice")
                    Assert.That(GameObject.Find("WorkbenchArtifact").GetComponent<Image>().sprite, Is.Not.SameAs(originalSprite));
                var marks = GameObject.Find("WorkbenchImageFrame").GetComponentsInChildren<Image>(true)
                    .Where(image => image.name.StartsWith("WorkbenchEffect_", StringComparison.Ordinal)).ToArray();
                Assert.That(marks.Length, Is.GreaterThan(0));
                foreach (var mark in marks)
                    Assert.That(mark.isActiveAndEnabled && mark.color.a > .001f, Is.False,
                        "A resolved " + start.incident + " object must not retain floating intervention rectangles.");
            }
        }

        [UnityTest]
        public IEnumerator ReplyCompartment_ShowsPaperAndEnvelopeThenWrittenAndPackedStates()
        {
            _app = CreateApp(SaveAt("remembering-rain", 3, "ko"));
            Click("IncidentButton");
            yield return AdvanceIntro();
            Assert.That(GameObject.Find("WorkbenchReplyCard"), Is.Null);
            Inspect("letter-lock");
            Click("WorkbenchTool_crescent-key");
            Click("WorkbenchTarget_letter-lock");
            yield return null;
            var card = GameObject.Find("WorkbenchReplyCard");
            var envelope = GameObject.Find("WorkbenchReplyEnvelope");
            Assert.That(card, Is.Not.Null, "Opening the compartment must reveal a card the pencil can visibly write on.");
            Assert.That(envelope, Is.Not.Null, "The paper must have a visible envelope to go into.");
            Assert.That(Vector3.Distance(card.transform.position, GameObject.Find("WorkbenchTarget_reply-sheet").transform.position), Is.LessThan(1));
            Assert.That(Vector3.Distance(envelope.transform.position, GameObject.Find("WorkbenchTarget_envelope").transform.position), Is.LessThan(1));
            foreach (var image in card.GetComponentsInChildren<Image>(true).Concat(envelope.GetComponentsInChildren<Image>(true)))
                Assert.That(image.raycastTarget, Is.False, "Object drawing must leave its real hotspot usable.");
            Assert.That(GameObject.Find("WorkbenchReplyWriting"), Is.Null);
            Inspect("reply-sheet");
            Click("WorkbenchTool_soft-pencil");
            Click("WorkbenchTarget_reply-sheet");
            yield return null;
            Assert.That(GameObject.Find("WorkbenchReplyWriting"), Is.Not.Null, "Writing must visibly mark the card.");
            Inspect("envelope");
            Click("WorkbenchTool_reply-card");
            Click("WorkbenchTarget_envelope");
            yield return null;
            Assert.That(GameObject.Find("WorkbenchReplyCard"), Is.Null, "The written card belongs inside the packed envelope.");
            Assert.That(GameObject.Find("WorkbenchReplyEnvelope"), Is.Not.Null);
            Assert.That(GameObject.Find("WorkbenchEnvelopeSealed"), Is.Not.Null);
            Assert.That(_app.ActiveWorkbench.IsComplete, Is.True);
        }

        private static void AssertReadableCreamText(string name)
        {
            var text = GameObject.Find(name).GetComponent<TMP_Text>();
            Assert.That(text.color.a, Is.EqualTo(1).Within(.001f), name + " must use opaque ink.");
            Assert.That(text.color.grayscale, Is.LessThan(.25f), name + " must use dark ink on cream.");
            var effectiveWeight = (text.fontStyle & FontStyles.Bold) != 0 ? FontWeight.Bold : text.fontWeight;
            Assert.That((int)effectiveWeight, Is.GreaterThanOrEqualTo((int)FontWeight.Bold),
                name + " must not use the thin regular strokes that look pale beside the objective on native Android.");
        }

        private IEnumerator AdvanceIntro()
        {
            for (var safety = 0; safety < 4 && _app.ActiveScreen == AppScreen.Narrative; safety++)
            {
                Click("NarrativeContinueButton");
                yield return null;
            }
            Assert.That(_app.IsWorkbenchActive, Is.True);
        }

        private IEnumerator CompleteThroughButtons()
        {
            var scene = Scene;
            foreach (var action in scene.Actions)
            {
                foreach (var targetId in action.Step.RequiredObservations) Inspect(targetId);
                Click("WorkbenchTool_" + action.Step.ToolId);
                Click("WorkbenchTarget_" + action.Step.TargetId);
                yield return null;
                Assert.That(_app.ActiveWorkbench.HasCompleted(action.Step.Id), Is.True,
                    scene.StageId + "/" + action.Step.Id + " must complete through actual buttons.");
            }
            Assert.That(_app.ActiveWorkbench.IsComplete, Is.True, scene.StageId);
        }

        private WorkbenchSceneDefinition Scene => WorkbenchCatalog.Find(_app.ActiveWorkbench.Puzzle.Id);

        private static PlayerSaveData SaveAt(string incidentId, int completedStages, string locale)
        {
            var save = new PlayerSaveData { locale = locale, activeIncidentId = incidentId, activeIncidentStage = completedStages };
            if (incidentId == "remembering-rain") save.completedIncidentIds.Add("unmelting-ice");
            foreach (var id in (incidentId == "unmelting-ice" ? IceStages : RainStages).Take(completedStages))
                save.incidentStageRecords.Add(new IncidentStageRecord { stageId = id, bestQuality = (int)IncidentQuality.Precise });
            return save;
        }

        private static void Inspect(string id)
        {
            Click("WorkbenchInspectButton");
            Click("WorkbenchTarget_" + id);
        }

        private static PointerEventData DragPointer(GameObject tool, Vector2 destination)
        {
            return new PointerEventData(EventSystem.current)
            {
                position = destination,
                pressPosition = RectTransformUtility.WorldToScreenPoint(null, tool.transform.position),
                pointerDrag = tool,
                dragging = true
            };
        }

        private static void Click(string objectName)
        {
            var obj = GameObject.Find(objectName);
            Assert.That(obj, Is.Not.Null, objectName + " must be visible.");
            var button = obj.GetComponent<Button>();
            Assert.That(button, Is.Not.Null, objectName + " must be a player control.");
            Assert.That(button.IsInteractable(), Is.True, objectName + " must accept input.");
            button.onClick.Invoke();
        }

        private static TMP_Text FindText(string objectName)
        {
            var obj = GameObject.Find(objectName);
            Assert.That(obj, Is.Not.Null, objectName);
            var text = obj.GetComponent<TMP_Text>() ?? obj.GetComponentInChildren<TMP_Text>();
            Assert.That(text, Is.Not.Null, objectName + " needs text.");
            return text;
        }

        private static string Text(string objectName) => FindText(objectName).text;
        private static string VisibleWorkbenchText() => string.Join("\n",
            GameObject.Find("WorkbenchScreen").GetComponentsInChildren<TMP_Text>()
                .Where(t => t.isActiveAndEnabled).Select(t => t.text));

        private static void AssertReadable(TMP_Text text, string context)
        {
            Canvas.ForceUpdateCanvases();
            text.ForceMeshUpdate();
            Assert.That(text.fontSize, Is.GreaterThanOrEqualTo(22f), context + "/" + text.name);
            Assert.That(text.isTextOverflowing, Is.False, context + "/" + text.name);
            Assert.That(text.preferredHeight, Is.LessThanOrEqualTo(text.rectTransform.rect.height + 1f), context + "/" + text.name);
        }

        private GameApp CreateApp(PlayerSaveData save, IAdService adService = null, IPrivacyService privacyService = null)
        {
            ServiceFactory.SetTestServices(adService ?? new DefaultAdService(), privacyService ?? new OfflinePrivacyService());
            var app = new GameObject("WorkbenchTestHost").AddComponent<GameApp>();
            // Awake only reads the disk store. Replace both boundaries synchronously, before
            // any interaction, frame, pause, or destruction can persist this test's progress.
            _store = new MemorySaveStore(save);
            SetField(app, "_saveStore", _store);
            SetField(app, "_save", save);
            SetField(app, "_localizer", new Localizer(save.locale));
            app.ShowMenu();
            return app;
        }

        private static void SetField(GameApp app, string name, object value)
        {
            typeof(GameApp).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(app, value);
        }

        private static object GetField(GameApp app, string name)
            => typeof(GameApp).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(app);

        private sealed class MemorySaveStore : ISaveStore
        {
            private string _json;
            public int SaveCalls { get; private set; }
            public PlayerSaveData Snapshot => JsonUtility.FromJson<PlayerSaveData>(_json);
            public MemorySaveStore(PlayerSaveData data) { _json = JsonUtility.ToJson(data); }
            public PlayerSaveData LoadOrDefault() => Snapshot;
            public void Save(PlayerSaveData data) { SaveCalls++; _json = JsonUtility.ToJson(data); }
        }

        private sealed class DeferredWorkbenchAdService : IAdService
        {
            private Action<RewardedAdResult> _completion;
            public bool IsRewardedReady => true;
            public bool HasPendingRequest => _completion != null;
            public void SetRequestPermission(bool allowed) { }
            public void ShowRewarded(string placement, Action<RewardedAdResult> completed) { _completion = completed; }
            public void Complete(RewardedAdResult result) { _completion(result); }
        }

        private sealed class AllowedPrivacyService : IPrivacyService
        {
            public bool CanRequestAds => true;
            public bool PrivacyOptionsRequired => false;
            public void RequestConsent(Action<bool> completed) => completed?.Invoke(true);
            public void ShowPrivacyOptions(Action<bool> completed) => completed?.Invoke(true);
        }


        private sealed class OfflinePrivacyService : IPrivacyService
        {
            public bool CanRequestAds => false;
            public bool PrivacyOptionsRequired => false;
            public void RequestConsent(Action<bool> completed) => completed?.Invoke(false);
            public void ShowPrivacyOptions(Action<bool> completed) => completed?.Invoke(false);
        }
    }
}
