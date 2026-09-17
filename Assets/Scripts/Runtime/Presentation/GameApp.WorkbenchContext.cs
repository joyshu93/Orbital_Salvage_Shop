using System.Collections.Generic;
using System.Linq;
using CurioClerk.Content.Incidents;
using CurioClerk.Content.Workbench;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CurioClerk.Presentation
{
    public sealed partial class GameApp
    {
        private string _selectedWorkbenchTarget;
        private RectTransform _workbenchTargetTray;
        private RectTransform _workbenchNotes;
        private Button _workbenchUse;
        private readonly Dictionary<string, Button> _workbenchMarkers = new Dictionary<string, Button>();
        private readonly Dictionary<string, TMP_Text> _workbenchSelectedLabels = new Dictionary<string, TMP_Text>();

        private LocalizedCopy CurrentTargetObservation(string id)
        {
            var changed = _workbenchScene.Actions.LastOrDefault(action =>
                action.Step.TargetId == id && _workbench.HasCompleted(action.Step.Id));
            return changed?.Result ?? WorkbenchCaseContext.AfterRelatedAction(_workbench, id) ??
                _workbenchScene.Targets.First(target => target.Id == id).Observation;
        }

        private void BuildWorkbenchTargets(RectTransform page)
        {
            _workbenchMarkers.Clear();
            _workbenchTargetTray = CreatePanel(page, "WorkbenchNamedTargets", Color.clear,
                new Vector2(.05f, .377f), new Vector2(.95f, .438f));
            var width = 1f / _workbenchScene.Targets.Count;
            for (var index = 0; index < _workbenchScene.Targets.Count; index++)
            {
                var target = _workbenchScene.Targets[index];
                var id = target.Id;
                var label = (index + 1) + "  " + WorkbenchCopy(target.Label);
                var button = CreateButton(_workbenchTargetTray, "WorkbenchTarget_" + id, label,
                    new Vector2(index * width + .006f, .02f), new Vector2((index + 1) * width - .006f, .98f),
                    Wine, Paper, () => TapWorkbenchTarget(id), 27);
                FitWorkbenchText(button.GetComponentInChildren<TMP_Text>(), 23, 27);
                var border = button.gameObject.AddComponent<Outline>();
                border.effectDistance = new Vector2(2, -2);
                border.effectColor = Amber;
                UseNeutralWorkbenchTint(button);
                _workbenchTargets.Add(id, button);
                var point = new Vector2(target.X, target.Y);
                var marker = CreateButton(_workbenchImageFrame, "WorkbenchMarker_" + id, (index + 1).ToString(),
                    point, point, Wine, Paper, () => TapWorkbenchTarget(id), 32);
                // Large named buttons carry the touch affordance; compact opaque pins keep close clues separate.
                ((RectTransform)marker.transform).sizeDelta = new Vector2(60, 60);
                var outline = marker.gameObject.AddComponent<Outline>();
                outline.effectDistance = new Vector2(3, -3);
                outline.effectColor = Amber;
                UseNeutralWorkbenchTint(marker);
                _workbenchMarkers.Add(id, marker);
            }
        }

        private void UseSelectedWorkbenchTool()
        {
            if (_workbenchNotes != null || _selectedWorkbenchTool == null || _selectedWorkbenchTarget == null) return;
            UseWorkbenchTool(_selectedWorkbenchTool, _selectedWorkbenchTarget);
        }

        private static void UseNeutralWorkbenchTint(Button button)
        {
            // Image.color owns semantic selection; Selectable must not multiply it by the original wine color.
            var colors = button.colors;
            colors.normalColor = colors.highlightedColor = colors.selectedColor = Color.white;
            colors.pressedColor = new Color(.85f, .85f, .85f, 1);
            colors.disabledColor = new Color(.65f, .65f, .65f, .7f);
            colors.colorMultiplier = 1;
            button.colors = colors;
        }

        private void RefreshWorkbenchSelection(bool complete)
        {
            _workbenchTargetTray.gameObject.SetActive(!complete);
            _workbenchUse.gameObject.SetActive(!complete);
            _workbenchUse.interactable = !complete && _selectedWorkbenchTool != null &&
                _selectedWorkbenchTarget != null && IsWorkbenchTargetVisible(_selectedWorkbenchTarget);
            var tool = _workbenchScene.Tools.FirstOrDefault(value => value.Id == _selectedWorkbenchTool);
            _workbenchInstruction.text = tool == null
                ? WorkbenchCopy("Tap a named part to examine it. Then choose a tool.", "이름이 적힌 부분을 살펴보고, 필요한 도구를 고르세요.")
                : WorkbenchCopy("Selected: ", "선택한 도구: ") + WorkbenchCopy(tool.Label) + "\n" + WorkbenchCopy(tool.Description);
            foreach (var pair in _workbenchTools)
            {
                var selected = pair.Key == _selectedWorkbenchTool;
                var hinted = pair.Key == _workbenchHintTool;
                pair.Value.GetComponent<Image>().color = selected ? Amber : hinted ? Sage : Wine;
                pair.Value.GetComponentInChildren<TMP_Text>().color = selected ? Ink : Paper;
                pair.Value.GetComponentInChildren<WorkbenchToolIcon>().color = selected ? Ink : Amber;
                _workbenchSelectedLabels[pair.Key].gameObject.SetActive(selected && !complete);
                var outline = pair.Value.GetComponent<Outline>();
                outline.effectColor = selected ? Paper : Color.clear;
            }
            foreach (var pair in _workbenchTargets)
            {
                var visible = !complete && IsWorkbenchTargetVisible(pair.Key);
                var selected = pair.Key == _selectedWorkbenchTarget;
                var hinted = pair.Key == _workbenchHintTarget;
                pair.Value.gameObject.SetActive(visible);
                _workbenchMarkers[pair.Key].gameObject.SetActive(visible);
                foreach (var button in new[] { pair.Value, _workbenchMarkers[pair.Key] })
                {
                    button.GetComponent<Image>().color = selected ? Amber : hinted ? Sage : Wine;
                    button.GetComponentInChildren<TMP_Text>().color = selected ? Ink : Paper;
                    button.GetComponent<Outline>().effectColor = selected || hinted ? Paper : Amber;
                }
            }
        }

        private void ShowWorkbenchCaseNotes()
        {
            if (!IsWorkbenchActive || _workbenchNotes != null) return;
            var context = WorkbenchCaseContext.Find(_workbenchScene.StageId);
            var page = _workbenchImageFrame.GetComponentInParent<WorkbenchAtmosphere>().transform;
            var controls = page.GetComponentsInChildren<Button>(true).ToDictionary(button => button, button => button.interactable);
            var previousFocus = EventSystem.current == null ? null : EventSystem.current.currentSelectedGameObject;
            foreach (var control in controls.Keys) control.interactable = false;
            _workbenchNotes = CreatePanel(page,
                "WorkbenchCaseNotes", new Color(Plum.r, Plum.g, Plum.b, .99f), Vector2.zero, Vector2.one);
            CreateText(_workbenchNotes, "WorkbenchCaseNotesTitle", WorkbenchCopy("Case notebook", "사건 수첩"),
                44, Paper, TextAlignmentOptions.Center, new Vector2(.06f, .90f), new Vector2(.94f, .98f), true, TextRole.Display);
            var entries = new List<string>
            {
                WorkbenchCopy("OUR REQUEST", "우리가 맡은 일") + "\n" + WorkbenchCopy(context.Purpose),
                WorkbenchCopy("THE STORY SO FAR", "지금까지의 이야기") + "\n" + WorkbenchCopy(context.Recap),
                WorkbenchCopy("THIS INVESTIGATION", "이번에 알아낼 것") + "\n" + WorkbenchCopy(_workbenchScene.Objective)
            };
            foreach (var target in _workbenchScene.Targets.Where(target => _workbench.HasObserved(target.Id)))
                entries.Add(WorkbenchCopy(target.Label) + "\n" + WorkbenchCopy(CurrentTargetObservation(target.Id)));
            if (_workbench.IsComplete) entries.Add(WorkbenchCopy("NEW DISCOVERY", "새로 알아낸 사실") + "\n" + WorkbenchCopy(_workbenchScene.Discovery));
            var content = CreateScrollContent(_workbenchNotes, "WorkbenchNotesScroll", new Vector2(.06f, .15f), new Vector2(.94f, .88f));
            var text = CreateLayoutText(content, string.Join("\n\n", entries), 32, Paper, 0);
            text.name = "WorkbenchCaseNotesBody";
            text.fontStyle = FontStyles.Bold;
            Canvas.ForceUpdateCanvases();
            text.GetComponent<LayoutElement>().preferredHeight = text.GetPreferredValues(text.text, content.rect.width - 48, Mathf.Infinity).y + 36;
            var close = CreateButton(_workbenchNotes, "WorkbenchCaseNotesClose", WorkbenchCopy("Back to the object", "물건으로 돌아가기"),
                new Vector2(.06f, .04f), new Vector2(.94f, .12f), Amber, Ink, () =>
                {
                    _workbenchNotes.gameObject.SetActive(false);
                    Destroy(_workbenchNotes.gameObject);
                    _workbenchNotes = null;
                    foreach (var control in controls) if (control.Key != null) control.Key.interactable = control.Value;
                    if (EventSystem.current != null)
                        EventSystem.current.SetSelectedGameObject(previousFocus != null && previousFocus.activeInHierarchy
                            ? previousFocus : controls.Keys.First(button => button.name == "WorkbenchCaseNotesButton").gameObject);
                }, 32);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(close.gameObject);
        }

        private LocalizedCopy WorkbenchExperimentFeedback(string toolId, string targetId)
        {
            if (_workbenchScene.StageId == "ice-01-crack")
            {
                if (targetId == "leaf") return new LocalizedCopy(
                    "The leaf is behind solid ice. The tool cannot reach it. Its movement is a reason to leave the ice intact.",
                    "낙엽은 단단한 얼음 안에 있어 도구가 닿지 않습니다. 아직 움직이는 걸 보니 얼음은 그대로 두는 편이 좋겠습니다.");
                if (targetId == "base") return new LocalizedCopy(
                    "The cold returns to the brass as soon as you lift the tool. It is flowing down from the wet crack above.",
                    "도구를 떼자 황동에 다시 서리가 맺힙니다. 위쪽 젖은 틈에서 냉기가 계속 내려옵니다.");
            }
            if (_workbenchScene.StageId == "ice-02-spread" && targetId == "frozen-rim" && toolId != "warm-pad")
                return new LocalizedCopy("The metal flexes, but the ice ring holds. Forcing it would bend the base before breaking the ice.",
                    "금속만 휘고 테두리 얼음은 버팁니다. 억지로 힘을 주면 얼음보다 받침이 먼저 찌그러지겠습니다.");
            if (_workbenchScene.StageId == "rain-03-unsent-letter" && targetId == "letter-lines" && toolId == "water-brush")
                return new LocalizedCopy("The brush is already damp; more water would spread the ink. The opened page needs to lose moisture now.",
                    "젖은 붓을 대면 잉크가 더 번지겠습니다. 펼친 편지는 이제 물기를 빼야 합니다.");
            var target = _workbenchScene.Targets.First(value => value.Id == targetId);
            return new LocalizedCopy("Nothing changes here. " + CurrentTargetObservation(target.Id).English,
                "이 부분은 달라지지 않습니다. " + CurrentTargetObservation(target.Id).Korean);
        }
    }
}
