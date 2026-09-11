using System;
using System.Collections.Generic;
using System.Linq;
using CurioClerk.Content.Incidents;
using CurioClerk.Core.Workbench;

namespace CurioClerk.Content.Workbench
{
    public sealed class WorkbenchTarget
    {
        public WorkbenchTarget(string id, LocalizedCopy label, LocalizedCopy observation,
            float x, float y, string revealAfterStep = null)
        {
            Id = id;
            Label = label;
            Observation = observation;
            X = x;
            Y = y;
            RevealAfterStep = revealAfterStep;
        }

        public string Id { get; }
        public LocalizedCopy Label { get; }
        public LocalizedCopy Observation { get; }
        public float X { get; }
        public float Y { get; }
        public string RevealAfterStep { get; }
    }

    public sealed class WorkbenchTool
    {
        public WorkbenchTool(string id, LocalizedCopy label, LocalizedCopy description)
        {
            Id = id;
            Label = label;
            Description = description;
        }

        public string Id { get; }
        public LocalizedCopy Label { get; }
        public LocalizedCopy Description { get; }
    }

    public sealed class WorkbenchAction
    {
        public WorkbenchAction(WorkbenchStep step, LocalizedCopy result, LocalizedCopy hint,
            string effect, string revealedArtifactId = null)
        {
            Step = step ?? throw new ArgumentNullException(nameof(step));
            Result = result;
            Hint = hint;
            Effect = effect;
            RevealedArtifactId = revealedArtifactId;
        }

        public WorkbenchStep Step { get; }
        public LocalizedCopy Result { get; }
        public LocalizedCopy Hint { get; }
        public string Effect { get; }
        public string RevealedArtifactId { get; }
    }

    public sealed class WorkbenchSceneDefinition
    {
        public WorkbenchSceneDefinition(string stageId, string artifactId,
            LocalizedCopy title, LocalizedCopy objective, LocalizedCopy introduction,
            LocalizedCopy ending, LocalizedCopy discovery, WorkbenchTarget[] targets,
            WorkbenchTool[] tools, WorkbenchAction[] actions)
        {
            StageId = stageId;
            ArtifactId = artifactId;
            Title = title;
            Objective = objective;
            Introduction = introduction;
            Ending = ending;
            Discovery = discovery;
            Targets = Array.AsReadOnly((WorkbenchTarget[])targets.Clone());
            Tools = Array.AsReadOnly((WorkbenchTool[])tools.Clone());
            Actions = Array.AsReadOnly((WorkbenchAction[])actions.Clone());
            Puzzle = new WorkbenchPuzzle(stageId, targets.Select(target => target.Id).ToArray(),
                actions.Select(action => action.Step).ToArray());
        }

        public string StageId { get; }
        public string ArtifactId { get; }
        public LocalizedCopy Title { get; }
        public LocalizedCopy Objective { get; }
        public LocalizedCopy Introduction { get; }
        public LocalizedCopy Ending { get; }
        public LocalizedCopy Discovery { get; }
        public IReadOnlyList<WorkbenchTarget> Targets { get; }
        public IReadOnlyList<WorkbenchTool> Tools { get; }
        public IReadOnlyList<WorkbenchAction> Actions { get; }
        public WorkbenchPuzzle Puzzle { get; }
    }
}
