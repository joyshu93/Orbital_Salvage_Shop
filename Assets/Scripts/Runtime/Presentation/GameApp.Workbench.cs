using System;
using System.Collections.Generic;
using System.Linq;
using CurioClerk.Content.Incidents;
using CurioClerk.Content.Workbench;
using CurioClerk.Core.Incidents;
using CurioClerk.Core.Workbench;
using CurioClerk.Infrastructure.Feedback;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CurioClerk.Presentation
{
    public sealed partial class GameApp
    {
        private WorkbenchSession _workbench;
        private WorkbenchSceneDefinition _workbenchScene;
        private bool _workbenchReplay;
        private bool _workbenchCompletionApplied;
        private string _selectedWorkbenchTool;
        private LocalizedCopy _workbenchMessageCopy;
        private LocalizedCopy _workbenchMessageLabel;
        private TMP_Text _workbenchObservation;
        private TMP_Text _workbenchInstruction;
        private Image _workbenchArtifact;
        private Image _workbenchFrost;
        private Image _workbenchWarmth;
        private RectTransform _workbenchImageFrame;
        private RectTransform _workbenchToolTray;
        private RectTransform _workbenchEnding;
        private Button _workbenchContinue;
        private Button _workbenchRestart;
        private Button _workbenchInspect;
        private TMP_Text _workbenchLastResult;
        private WorkbenchAtmosphere _workbenchAtmosphere;
        private readonly Dictionary<string, Button> _workbenchTargets = new Dictionary<string, Button>();
        private readonly Dictionary<string, Button> _workbenchTools = new Dictionary<string, Button>();
        private readonly Dictionary<string, Image> _workbenchEffects = new Dictionary<string, Image>();

        public WorkbenchSession ActiveWorkbench => _workbench;
        public bool IsWorkbenchActive => ActiveScreen == AppScreen.Workbench && _workbench != null;

        private string WorkbenchCopy(string english, string korean) => _localizer.Locale == "ko" ? korean : english;
        private string WorkbenchCopy(LocalizedCopy copy) => copy.ForLocale(_localizer.Locale);
        private string WorkbenchMessage => (_workbenchMessageLabel == null ? string.Empty : WorkbenchCopy(_workbenchMessageLabel) + "\n") +
            (_workbenchMessageCopy == null ? string.Empty : WorkbenchCopy(_workbenchMessageCopy));

        private void SetWorkbenchMessage(LocalizedCopy copy, LocalizedCopy label = null)
        {
            _workbenchMessageCopy = copy;
            _workbenchMessageLabel = label;
        }

        private void DecorateWorkbenchMenu(IncidentCardView card, IncidentCardState state)
        {
            if (state == null || _activeIncident == null || _incidentRunner == null || _incidentRunner.IsContentExhausted) return;
            var scene = WorkbenchCatalog.Find(_activeIncident.Stages[_incidentRunner.CurrentStageIndex].Id);
            if (scene == null) return;
            var clue = card.transform.Find("IncidentClue").GetComponent<TMP_Text>();
            clue.text = WorkbenchCopy(scene.Objective);
            FitWorkbenchText(clue, 20, 24);
            card.transform.Find("IncidentArtwork").GetComponent<Image>().sprite = VisualAssetLibrary.Artifact(scene.ArtifactId);
            if (_activeIncident.Id != "unmelting-ice" && _incidentRunner.CurrentStageIndex == 0)
            {
                card.transform.Find("IncidentState").GetComponent<TMP_Text>().text = WorkbenchCopy("A voice in the rain", "빗속에서 들리는 목소리");
                card.transform.Find("IncidentButton").GetComponentInChildren<TMP_Text>().text = WorkbenchCopy("Listen to the umbrella", "우산의 말 들어보기");
            }
        }

        private void ShowWorkbenchIntroduction()
        {
            var scene = WorkbenchCatalog.Find(_incidentStage.Id);
            if (scene == null) throw new InvalidOperationException("Missing workbench scene: " + _incidentStage.Id);
            ActiveScreen = AppScreen.Narrative;
            var page = CreatePage("NarrativeScreen");
            CreateText(page, "NarrativeTitle", WorkbenchCopy(scene.Title), 48, Paper,
                TextAlignmentOptions.Center, new Vector2(.08f, .87f), new Vector2(.92f, .96f), true, TextRole.Display);
            var portrait = CreateArtworkImage(page, "SeniorClerkPortrait", new Vector2(.18f, .43f), new Vector2(.82f, .86f));
            portrait.sprite = VisualAssetLibrary.SeniorClerk(_incidentRunner.CurrentStageIndex == 0 ? SeniorClerkMood.Concerned : SeniorClerkMood.Neutral);
            var panel = CreatePanel(page, "NarrativeDialoguePanel", Paper, new Vector2(.06f, .18f), new Vector2(.94f, .47f));
            AddSurfaceChrome(panel, Amber, 3, .25f);
            CreateText(panel, "NarrativeSpeaker", WorkbenchCopy("Senior", "선임"), 30, Wine,
                TextAlignmentOptions.Left, new Vector2(.06f, .77f), new Vector2(.94f, .94f), true);
            FitWorkbenchText(CreateText(panel, "NarrativeBody", WorkbenchCopy(scene.Introduction), 38, Ink,
                TextAlignmentOptions.TopLeft, new Vector2(.06f, .08f), new Vector2(.94f, .75f)), 30, 38);
            CreateButton(page, "NarrativeContinueButton", WorkbenchCopy("Take a look", "물건 살펴보기"),
                new Vector2(.06f, .045f), new Vector2(.94f, .135f), Amber, Ink, BeginIncidentStage, 34);
            CreateButton(page, "NarrativeMenuButton", WorkbenchCopy("Back to the office", "보관소로 돌아가기"),
                new Vector2(.20f, .005f), new Vector2(.80f, .037f), Plum, Paper, ShowMenu, 25);
        }

        private void BeginWorkbench()
        {
            var scene = WorkbenchCatalog.Find(_incidentStage.Id);
            if (scene == null) throw new InvalidOperationException("Missing workbench scene: " + _incidentStage.Id);
            if (_workbench == null || _workbenchScene.StageId != scene.StageId || _workbench.IsComplete || _workbenchReplay != _isIncidentReplay)
            {
                _workbenchScene = scene;
                _workbench = new WorkbenchSession(scene.Puzzle);
                _workbenchReplay = _isIncidentReplay;
                _workbenchCompletionApplied = false;
                SetWorkbenchMessage(new LocalizedCopy("Touch a marked part of the object to examine it.", "물건에 표시된 부분을 눌러 살펴보세요."));
            }
            _selectedWorkbenchTool = null;
            _tutorialStage = TutorialStage.None;
            _isIncidentShift = false;
            _isDailyShift = false;
            _session = null;
            _activeRules = null;
            _plannedQueue = null;
            _activePlan = null;
            _inputLocked = false;
            BuildWorkbenchScreen();
        }

        private void BuildWorkbenchScreen()
        {
            ActiveScreen = AppScreen.Workbench;
            var page = CreatePage("WorkbenchScreen");
            _workbenchTargets.Clear();
            _workbenchTools.Clear();
            _workbenchEffects.Clear();
            CreateButton(page, "WorkbenchMenuButton", WorkbenchCopy("Office", "보관소"), new Vector2(.05f, .95f), new Vector2(.27f, .995f), Wine, Paper, ShowMenu, 27);
            _workbenchRestart = CreateButton(page, "WorkbenchRestartButton", WorkbenchCopy("Start over", "처음부터"), new Vector2(.73f, .95f), new Vector2(.95f, .995f), Wine, Paper, RestartWorkbench, 26);
            CreateText(page, "WorkbenchChapter", WorkbenchCopy("CASE ", "사건 ") + (_activeIncident.Id == "unmelting-ice" ? "1" : "2") + " / " + (_incidentRunner.CurrentStageIndex + 1),
                24, Amber, TextAlignmentOptions.Center, new Vector2(.28f, .953f), new Vector2(.72f, .992f), true);
            FitWorkbenchText(CreateText(page, "WorkbenchTitle", WorkbenchCopy(_workbenchScene.Title), 42, Paper,
                TextAlignmentOptions.Center, new Vector2(.05f, .89f), new Vector2(.95f, .945f), true, TextRole.Display), 32, 42);
            var objective = CreatePanel(page, "WorkbenchObjectivePanel", Paper, new Vector2(.05f, .793f), new Vector2(.95f, .883f));
            FitWorkbenchText(CreateText(objective, "WorkbenchObjective", WorkbenchCopy(_workbenchScene.Objective), 34, Ink,
                TextAlignmentOptions.MidlineLeft, new Vector2(.045f, .10f), new Vector2(.955f, .9f), true), 28, 34);

            var area = CreatePanel(page, "WorkbenchObjectArea", new Color(33f/255, 20f/255, 30f/255, 1), new Vector2(.05f, .387f), new Vector2(.95f, .782f));
            area.GetComponent<Image>().raycastTarget = false;
            _workbenchImageFrame = new GameObject("WorkbenchImageFrame", typeof(RectTransform), typeof(AspectRatioFitter)).GetComponent<RectTransform>();
            _workbenchImageFrame.SetParent(area, false);
            var fitter = _workbenchImageFrame.GetComponent<AspectRatioFitter>();
            fitter.aspectRatio = 1;
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            _workbenchArtifact = CreateArtworkImage(_workbenchImageFrame, "WorkbenchArtifact", Vector2.zero, Vector2.one);
            _workbenchArtifact.sprite = VisualAssetLibrary.Artifact(_workbenchScene.ArtifactId);
            _workbenchWarmth = CreateArtworkImage(_workbenchImageFrame, "WorkbenchWarmth", Vector2.zero, Vector2.one);
            _workbenchWarmth.color = new Color(1, .64f, .19f, 0);
            _workbenchFrost = CreateArtworkImage(_workbenchImageFrame, "WorkbenchFrost", Vector2.zero, Vector2.one);
            _workbenchFrost.sprite = VisualAssetLibrary.FrostOverlay;
            var icy = _workbenchScene.ArtifactId == "unmelting-ice" || _workbenchScene.Actions.Any(value => value.Effect == "frost");
            _workbenchFrost.color = new Color(.7f, .87f, 1, icy ? .45f : 0);
            _workbenchAtmosphere = page.gameObject.AddComponent<WorkbenchAtmosphere>();
            _workbenchAtmosphere.Configure(_workbenchArtifact.rectTransform, _workbenchFrost, _workbenchWarmth, _activeIncident.Id == "remembering-rain");
            foreach (var action in _workbenchScene.Actions)
            {
                var target = _workbenchScene.Targets.First(value => value.Id == action.Step.TargetId);
                var mark = CreateArtworkImage(_workbenchImageFrame, "WorkbenchEffect_" + action.Step.Id,
                    new Vector2(target.X - .07f, target.Y - .025f), new Vector2(target.X + .07f, target.Y + .025f));
                mark.color = Color.clear;
                mark.rectTransform.localRotation = Quaternion.Euler(0, 0, -18);
                _workbenchEffects.Add(action.Step.Id, mark);
            }
            foreach (var target in _workbenchScene.Targets)
            {
                var id = target.Id;
                var button = CreateButton(_workbenchImageFrame, "WorkbenchTarget_" + id, "+",
                    new Vector2(target.X, target.Y), new Vector2(target.X, target.Y), new Color(.21f, .11f, .17f, .22f), Paper,
                    () => TapWorkbenchTarget(id), 40);
                var rect = (RectTransform)button.transform;
                rect.sizeDelta = new Vector2(96, 96);
                var outline = button.gameObject.AddComponent<Outline>();
                outline.effectColor = new Color(1, .8f, .42f, .8f);
                outline.effectDistance = new Vector2(2, -2);
                _workbenchTargets.Add(id, button);
            }
            var note = CreatePanel(page, "WorkbenchObservationPanel", Paper, new Vector2(.05f, .238f), new Vector2(.95f, .373f));
            FitWorkbenchText(_workbenchObservation = CreateText(note, "WorkbenchObservation", WorkbenchMessage, 32, Ink,
                TextAlignmentOptions.MidlineLeft, new Vector2(.045f, .08f), new Vector2(.955f, .92f)), 27, 32);
            _workbenchInspect = CreateButton(page, "WorkbenchInspectButton", WorkbenchCopy("Examine", "살펴보기"), new Vector2(.05f, .191f), new Vector2(.28f, .229f), Sage, Paper,
                () => { _selectedWorkbenchTool = null; RefreshWorkbenchView(); }, 25);
            _workbenchInstruction = CreateText(page, "WorkbenchInstruction", string.Empty, 24, Paper,
                TextAlignmentOptions.MidlineRight, new Vector2(.30f, .189f), new Vector2(.95f, .233f));
            FitWorkbenchText(_workbenchInstruction, 21, 24);
            _workbenchToolTray = CreatePanel(page, "WorkbenchTools", Color.clear, new Vector2(.05f, .069f), new Vector2(.95f, .178f));
            _workbenchToolTray.GetComponent<Image>().raycastTarget = false;
            var width = 1f / _workbenchScene.Tools.Count;
            for (var index = 0; index < _workbenchScene.Tools.Count; index++)
            {
                var tool = _workbenchScene.Tools[index];
                var id = tool.Id;
                var button = CreateButton(_workbenchToolTray, "WorkbenchTool_" + id, WorkbenchCopy(tool.Label),
                    new Vector2(index * width + .009f, 0), new Vector2((index + 1) * width - .009f, 1), Wine, Paper,
                    () => SelectWorkbenchTool(id), 29);
                var label = button.GetComponentInChildren<TMP_Text>();
                SetAnchors(label.rectTransform, new Vector2(.05f, .04f), new Vector2(.95f, .55f));
                FitWorkbenchText(label, 23, 29);
                var toolMark = new GameObject("ToolIllustration", typeof(RectTransform), typeof(WorkbenchToolIcon)).GetComponent<WorkbenchToolIcon>();
                toolMark.rectTransform.SetParent(button.transform, false);
                SetAnchors(toolMark.rectTransform, new Vector2(.15f, .57f), new Vector2(.85f, .98f));
                toolMark.color = Amber;
                toolMark.Configure(id);
                button.gameObject.AddComponent<WorkbenchToolDrag>().Configure(id,
                    _workbenchTargets.Values.Select(value => (RectTransform)value.transform).ToArray(),
                    _workbenchTargets.Keys.ToArray(), UseWorkbenchTool, SelectWorkbenchTool);
                _workbenchTools.Add(id, button);
            }
            CreateText(page, "WorkbenchControls", WorkbenchCopy("Drag a tool onto the object, or tap a tool, then a marked part.", "도구를 끌어 쓰거나, 도구를 누른 뒤 사용할 부분을 누르세요."),
                22, Paper, TextAlignmentOptions.Center, new Vector2(.05f, .014f), new Vector2(.95f, .06f));
            _workbenchEnding = CreatePanel(page, "WorkbenchEnding", Paper, new Vector2(.05f, .12f), new Vector2(.95f, .373f));
            FitWorkbenchText(_workbenchLastResult = CreateText(_workbenchEnding, "WorkbenchActionResult", string.Empty, 29, Sage,
                TextAlignmentOptions.TopLeft, new Vector2(.045f, .72f), new Vector2(.955f, .96f), true), 25, 29);
            FitWorkbenchText(CreateText(_workbenchEnding, "WorkbenchEndingText", WorkbenchCopy(_workbenchScene.Ending), 32, Ink,
                TextAlignmentOptions.TopLeft, new Vector2(.045f, .34f), new Vector2(.955f, .70f)), 26, 32);
            FitWorkbenchText(CreateText(_workbenchEnding, "WorkbenchDiscovery", WorkbenchCopy(_workbenchScene.Discovery), 30, Wine,
                TextAlignmentOptions.MidlineLeft, new Vector2(.045f, .04f), new Vector2(.955f, .32f), true), 25, 30);
            _workbenchContinue = CreateButton(page, "WorkbenchContinueButton", WorkbenchCopy("Follow the clue", "다음 단서 따라가기"),
                new Vector2(.05f, .03f), new Vector2(.95f, .105f), Amber, Ink, ContinueWorkbench, 32);
            RefreshWorkbenchView();
        }

        public void InspectWorkbenchTarget(string id)
        {
            if (!IsWorkbenchActive || _workbench.IsComplete || !IsWorkbenchTargetVisible(id)) return;
            var target = _workbenchScene.Targets.First(value => value.Id == id);
            _workbench.Observe(id);
            SetWorkbenchMessage(target.Observation, target.Label);
            _feedbackService?.Play(PlayerFeedbackCue.Hold);
            RefreshWorkbenchView();
        }

        private bool IsWorkbenchTargetVisible(string id)
        {
            var target = _workbenchScene.Targets.FirstOrDefault(value => value.Id == id);
            return target != null && (string.IsNullOrEmpty(target.RevealAfterStep) || _workbench.HasCompleted(target.RevealAfterStep));
        }

        private void TapWorkbenchTarget(string id)
        {
            if (_selectedWorkbenchTool == null) InspectWorkbenchTarget(id);
            else UseWorkbenchTool(_selectedWorkbenchTool, id);
        }

        private void SelectWorkbenchTool(string id)
        {
            if (!IsWorkbenchActive || _workbench.IsComplete) return;
            var tool = _workbenchScene.Tools.FirstOrDefault(value => value.Id == id);
            if (tool == null) return;
            _selectedWorkbenchTool = id;
            SetWorkbenchMessage(tool.Description, tool.Label);
            RefreshWorkbenchView();
        }

        public void UseWorkbenchTool(string toolId, string targetId)
        {
            if (!IsWorkbenchActive || _workbench.IsComplete || !IsWorkbenchTargetVisible(targetId)) return;
            if (!_workbenchScene.Tools.Any(value => value.Id == toolId)) return;
            var outcome = _workbench.Apply(toolId, targetId);
            var action = _workbenchScene.Actions.FirstOrDefault(value => value.Step.Id == outcome.StepId);
            if (outcome.Kind == WorkbenchOutcomeKind.Applied || outcome.Kind == WorkbenchOutcomeKind.Completed)
            {
                SetWorkbenchMessage(action.Result);
                _selectedWorkbenchTool = null;
                _feedbackService?.Play(PlayerFeedbackCue.KeyReaction);
                if (_workbench.IsComplete) ApplyWorkbenchCompletion();
            }
            else if (outcome.Kind == WorkbenchOutcomeKind.AlreadyApplied)
                SetWorkbenchMessage(new LocalizedCopy("That part is already taken care of. What else has changed?", "이 부분은 이미 손봤다. 다른 곳은 어떻게 달라졌을까?"));
            else if (action != null)
            {
                SetWorkbenchMessage(action.Hint);
                if (outcome.Kind == WorkbenchOutcomeKind.NeedsObservation) _selectedWorkbenchTool = null;
            }
            else
                SetWorkbenchMessage(new LocalizedCopy("This tool doesn't help here. Examine the object for a better place to use it.", "여기에는 이 도구가 맞지 않는다. 물건을 살펴보고 쓸 곳을 찾아보자."));
            RefreshWorkbenchView();
            if (outcome.Kind == WorkbenchOutcomeKind.Applied || outcome.Kind == WorkbenchOutcomeKind.Completed)
                _workbenchAtmosphere.Apply(action.Effect, (float)_workbench.CompletedSteps.Count / _workbenchScene.Actions.Count, _workbench.IsComplete);
        }

        private void ApplyWorkbenchCompletion()
        {
            if (_workbenchCompletionApplied) return;
            _workbenchCompletionApplied = true;
            var completion = _incidentRunner.CompleteCurrentStage(IncidentQuality.Stable);
            _incidentCompletionWasFinal = completion.IncidentCompleted;
            if (!_workbenchReplay)
            {
                _progression.ApplyIncidentStage(_save, completion);
                foreach (var id in new[] { _workbenchScene.ArtifactId }.Concat(_workbenchScene.Actions.Select(value => value.RevealedArtifactId)))
                    if (!string.IsNullOrEmpty(id) && !_save.discoveredArtifactIds.Contains(id)) _save.discoveredArtifactIds.Add(id);
                if (completion.IncidentCompleted)
                    _pendingIncidentBoardReveal = IncidentBoardPresenter.ShouldRevealSuccessor(ResolveIncidentProgress(), completion.IncidentId);
                Save();
            }
            _feedbackService?.Play(completion.IncidentCompleted ? PlayerFeedbackCue.IncidentComplete : PlayerFeedbackCue.ShiftComplete);
        }

        public void ContinueWorkbench()
        {
            if (!IsWorkbenchActive || !_workbench.IsComplete || !_workbenchCompletionApplied) return;
            if (_incidentRunner.IsContentExhausted) ShowMenu();
            else ShowIncidentIntro();
        }

        public void RestartWorkbench()
        {
            if (!IsWorkbenchActive || _workbench.IsComplete) return;
            _workbench = null;
            BeginWorkbench();
        }

        private void RefreshWorkbenchView()
        {
            if (!IsWorkbenchActive || _workbenchObservation == null) return;
            var complete = _workbench.IsComplete;
            _workbenchObservation.text = WorkbenchMessage;
            _workbenchLastResult.text = WorkbenchMessage;
            _workbenchToolTray.gameObject.SetActive(!complete);
            _workbenchInspect.gameObject.SetActive(!complete);
            _workbenchInstruction.gameObject.SetActive(!complete);
            _workbenchObservation.transform.parent.gameObject.SetActive(!complete);
            _workbenchEnding.gameObject.SetActive(complete);
            _workbenchContinue.gameObject.SetActive(complete);
            _workbenchRestart.interactable = !complete;
            _workbenchInstruction.text = _selectedWorkbenchTool == null
                ? WorkbenchCopy("Look closely. Then choose a tool.", "살펴본 뒤, 필요한 도구를 골라보세요.")
                : WorkbenchCopy("Choose where to use it.", "도구를 사용할 부분을 누르세요.");
            foreach (var pair in _workbenchTools)
                pair.Value.GetComponent<Image>().color = pair.Key == _selectedWorkbenchTool ? Sage : Wine;
            foreach (var pair in _workbenchTargets)
            {
                pair.Value.gameObject.SetActive(!complete && IsWorkbenchTargetVisible(pair.Key));
                pair.Value.GetComponentInChildren<TMP_Text>().text = _workbench.HasObserved(pair.Key) ? "·" : "+";
            }
            var progress = (float)_workbench.CompletedSteps.Count / _workbenchScene.Actions.Count;
            _workbenchAtmosphere.Apply(null, progress, complete);
            foreach (var action in _workbenchScene.Actions)
            {
                if (!_workbench.HasCompleted(action.Step.Id)) continue;
                _workbenchEffects[action.Step.Id].color = action.Effect == "repair" ? new Color(.77f, .68f, .47f, .9f) : new Color(.9f, .73f, .34f, .35f);
                if (!string.IsNullOrEmpty(action.RevealedArtifactId)) _workbenchArtifact.sprite = VisualAssetLibrary.Artifact(action.RevealedArtifactId);
            }
            var changedArtwork = WorkbenchArtwork.ForSession(_workbench);
            if (changedArtwork != null)
            {
                _workbenchArtifact.sprite = changedArtwork;
                foreach (var effect in _workbenchEffects.Values) effect.color = Color.clear;
            }
            if (complete && _incidentCompletionWasFinal)
                _workbenchContinue.GetComponentInChildren<TMP_Text>().text = WorkbenchCopy("Back to the office", "보관소로 돌아가기");
        }

        private static void FitWorkbenchText(TMP_Text text, float min, float max)
        {
            text.enableAutoSizing = true;
            text.fontSizeMin = min;
            text.fontSizeMax = max;
        }
    }
}
