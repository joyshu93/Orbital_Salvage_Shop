using System.Linq;
using CurioClerk.Content.Incidents;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CurioClerk.Presentation
{
    public sealed partial class GameApp
    {
        private Button _workbenchHintButton;
        private string _workbenchHintStepId;
        private string _workbenchHintTarget;
        private string _workbenchHintTool;

        private void ShowWorkbenchHint()
        {
            if (!IsWorkbenchActive || _workbench.IsComplete) return;
            var step = _workbench.GetNextStep();
            if (step == null) return;
            ClearWorkbenchHintFocus();
            _selectedWorkbenchTool = null;
            var label = new LocalizedCopy("A hint from the senior", "선임의 힌트");
            if (_workbenchHintStepId != step.Id)
            {
                _workbenchHintStepId = step.Id;
                SetWorkbenchMessage(_workbenchScene.Actions.First(action => action.Step.Id == step.Id).Hint, label);
            }
            else
            {
                var missingObservation = step.RequiredObservations.FirstOrDefault(id => !_workbench.HasObserved(id));
                if (missingObservation != null)
                {
                    var target = _workbenchScene.Targets.First(value => value.Id == missingObservation);
                    _workbenchHintTarget = target.Id;
                    SetWorkbenchMessage(new LocalizedCopy(
                        "First, tap '" + target.Label.English + "' to examine it.",
                        "먼저 ‘" + target.Label.Korean + "’ 부분을 눌러 살펴보세요."), label);
                }
                else
                {
                    var target = _workbenchScene.Targets.First(value => value.Id == step.TargetId);
                    var tool = _workbenchScene.Tools.First(value => value.Id == step.ToolId);
                    _workbenchHintTarget = target.Id;
                    _workbenchHintTool = tool.Id;
                    SetWorkbenchMessage(new LocalizedCopy(
                        "Tool: " + tool.Label.English + "\nUse it on: " + target.Label.English,
                        "사용할 도구: " + tool.Label.Korean + "\n사용할 곳: " + target.Label.Korean), label);
                }
            }
            RefreshWorkbenchView();
        }

        private void ResetWorkbenchHint()
        {
            _workbenchHintStepId = null;
            ClearWorkbenchHintFocus();
        }

        private void ClearWorkbenchHintFocus()
        {
            _workbenchHintTarget = null;
            _workbenchHintTool = null;
        }

        private void RefreshWorkbenchHintView(bool complete)
        {
            _workbenchHintButton.gameObject.SetActive(!complete);
            _workbenchHintButton.GetComponentInChildren<TMP_Text>().text = _workbenchHintStepId == null
                ? WorkbenchCopy("Hint", "힌트") : WorkbenchCopy("More help", "더 자세히");
            foreach (var pair in _workbenchTargets)
            {
                var highlighted = !complete && pair.Key == _workbenchHintTarget;
                pair.Value.GetComponent<Image>().color = highlighted
                    ? new Color(Amber.r, Amber.g, Amber.b, .75f) : new Color(.21f, .11f, .17f, .22f);
                var outline = pair.Value.GetComponent<Outline>();
                outline.effectColor = highlighted ? Paper : new Color(1, .8f, .42f, .8f);
                outline.effectDistance = highlighted ? new Vector2(4, -4) : new Vector2(2, -2);
            }
            foreach (var pair in _workbenchTools)
            {
                var highlighted = !complete && pair.Key == _workbenchHintTool;
                if (highlighted) pair.Value.GetComponent<Image>().color = Amber;
                pair.Value.GetComponentInChildren<TMP_Text>().color = highlighted ? Ink : Paper;
                pair.Value.GetComponentInChildren<WorkbenchToolIcon>().color = highlighted ? Ink : Amber;
            }
        }
    }
}
