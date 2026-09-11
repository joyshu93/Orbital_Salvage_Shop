using System;
using System.Collections.Generic;

namespace CurioClerk.Core.Workbench
{
    public enum WorkbenchOutcomeKind
    {
        NoMatch,
        NeedsObservation,
        NeedsPreviousStep,
        Applied,
        AlreadyApplied,
        Completed
    }

    public sealed class WorkbenchOutcome
    {
        internal WorkbenchOutcome(WorkbenchOutcomeKind kind, string stepId)
        {
            Kind = kind;
            StepId = stepId;
        }

        public WorkbenchOutcomeKind Kind { get; }
        public string StepId { get; }
    }

    public sealed class WorkbenchStep
    {
        public WorkbenchStep(string id, string toolId, string targetId,
            string[] requiredObservations = null, string[] requiredSteps = null)
        {
            WorkbenchDefinitions.RequireId(id, nameof(id));
            WorkbenchDefinitions.RequireId(toolId, nameof(toolId));
            WorkbenchDefinitions.RequireId(targetId, nameof(targetId));
            Id = id;
            ToolId = toolId;
            TargetId = targetId;
            RequiredObservations = WorkbenchDefinitions.CopyIds(
                requiredObservations ?? Array.Empty<string>(), nameof(requiredObservations));
            RequiredSteps = WorkbenchDefinitions.CopyIds(
                requiredSteps ?? Array.Empty<string>(), nameof(requiredSteps));
        }

        public string Id { get; }
        public string ToolId { get; }
        public string TargetId { get; }
        public IReadOnlyList<string> RequiredObservations { get; }
        public IReadOnlyList<string> RequiredSteps { get; }
    }

    public sealed class WorkbenchPuzzle
    {
        public WorkbenchPuzzle(string id, string[] targetIds, WorkbenchStep[] steps)
        {
            WorkbenchDefinitions.RequireId(id, nameof(id));
            if (targetIds == null || targetIds.Length == 0)
            {
                throw new ArgumentException("A workbench requires at least one target.", nameof(targetIds));
            }

            if (steps == null || steps.Length == 0)
            {
                throw new ArgumentException("A workbench requires at least one step.", nameof(steps));
            }

            Id = id;
            TargetIds = WorkbenchDefinitions.CopyIds(targetIds, nameof(targetIds));
            var copiedSteps = (WorkbenchStep[])steps.Clone();
            var knownTargets = new HashSet<string>(TargetIds, StringComparer.Ordinal);
            var knownSteps = new HashSet<string>(StringComparer.Ordinal);
            var combinations = new HashSet<(string ToolId, string TargetId)>();

            foreach (var step in copiedSteps)
            {
                if (step == null)
                {
                    throw new ArgumentException("Workbench steps cannot be null.", nameof(steps));
                }

                if (!knownSteps.Add(step.Id))
                {
                    throw new ArgumentException($"Duplicate workbench step '{step.Id}'.", nameof(steps));
                }

                if (!combinations.Add((step.ToolId, step.TargetId)))
                {
                    throw new ArgumentException(
                        $"Tool '{step.ToolId}' on target '{step.TargetId}' matches more than one step.", nameof(steps));
                }

                if (!knownTargets.Contains(step.TargetId))
                {
                    throw new ArgumentException($"Step '{step.Id}' uses unknown target '{step.TargetId}'.", nameof(steps));
                }

                foreach (var target in step.RequiredObservations)
                {
                    if (!knownTargets.Contains(target))
                    {
                        throw new ArgumentException($"Step '{step.Id}' requires unknown observation '{target}'.", nameof(steps));
                    }
                }
            }

            foreach (var step in copiedSteps)
            {
                foreach (var previous in step.RequiredSteps)
                {
                    if (!knownSteps.Contains(previous))
                    {
                        throw new ArgumentException($"Step '{step.Id}' requires unknown step '{previous}'.", nameof(steps));
                    }
                }
            }

            RequireSolvableGraph(copiedSteps);
            Steps = Array.AsReadOnly(copiedSteps);
        }

        public string Id { get; }
        public IReadOnlyList<string> TargetIds { get; }
        public IReadOnlyList<WorkbenchStep> Steps { get; }

        private static void RequireSolvableGraph(WorkbenchStep[] steps)
        {
            var reachable = new HashSet<string>(StringComparer.Ordinal);
            while (reachable.Count < steps.Length)
            {
                var before = reachable.Count;
                foreach (var step in steps)
                {
                    if (!reachable.Contains(step.Id) && AllPreviousStepsReached(step, reachable))
                    {
                        reachable.Add(step.Id);
                    }
                }

                if (reachable.Count == before)
                {
                    throw new ArgumentException("Workbench step dependencies contain a cycle.", nameof(steps));
                }
            }
        }

        private static bool AllPreviousStepsReached(WorkbenchStep step, HashSet<string> reachable)
        {
            foreach (var previous in step.RequiredSteps)
            {
                if (!reachable.Contains(previous))
                {
                    return false;
                }
            }

            return true;
        }
    }

    public sealed class WorkbenchSession
    {
        private readonly HashSet<string> _knownTargets;
        private readonly List<string> _observedTargets = new List<string>();
        private readonly List<string> _completedSteps = new List<string>();

        public WorkbenchSession(WorkbenchPuzzle puzzle)
        {
            Puzzle = puzzle ?? throw new ArgumentNullException(nameof(puzzle));
            _knownTargets = new HashSet<string>(puzzle.TargetIds, StringComparer.Ordinal);
        }

        public WorkbenchPuzzle Puzzle { get; }
        public bool IsComplete => _completedSteps.Count == Puzzle.Steps.Count;
        public IReadOnlyCollection<string> ObservedTargets => Array.AsReadOnly(_observedTargets.ToArray());
        public IReadOnlyCollection<string> CompletedSteps => Array.AsReadOnly(_completedSteps.ToArray());

        public bool Observe(string targetId)
        {
            if (!_knownTargets.Contains(targetId) || HasObserved(targetId))
            {
                return false;
            }

            _observedTargets.Add(targetId);
            return true;
        }

        public bool HasObserved(string targetId) => _observedTargets.Contains(targetId);
        public bool HasCompleted(string stepId) => _completedSteps.Contains(stepId);

        public WorkbenchOutcome Apply(string toolId, string targetId)
        {
            foreach (var step in Puzzle.Steps)
            {
                if (!string.Equals(step.ToolId, toolId, StringComparison.Ordinal)
                    || !string.Equals(step.TargetId, targetId, StringComparison.Ordinal))
                {
                    continue;
                }

                if (HasCompleted(step.Id))
                {
                    return new WorkbenchOutcome(WorkbenchOutcomeKind.AlreadyApplied, step.Id);
                }

                foreach (var observation in step.RequiredObservations)
                {
                    if (!HasObserved(observation))
                    {
                        return new WorkbenchOutcome(WorkbenchOutcomeKind.NeedsObservation, step.Id);
                    }
                }

                foreach (var previous in step.RequiredSteps)
                {
                    if (!HasCompleted(previous))
                    {
                        return new WorkbenchOutcome(WorkbenchOutcomeKind.NeedsPreviousStep, step.Id);
                    }
                }

                _completedSteps.Add(step.Id);
                return new WorkbenchOutcome(
                    IsComplete ? WorkbenchOutcomeKind.Completed : WorkbenchOutcomeKind.Applied, step.Id);
            }

            return new WorkbenchOutcome(WorkbenchOutcomeKind.NoMatch, null);
        }
    }

    internal static class WorkbenchDefinitions
    {
        internal static void RequireId(string id, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Workbench IDs cannot be blank.", parameterName);
            }
        }

        internal static IReadOnlyList<string> CopyIds(string[] ids, string parameterName)
        {
            var copied = (string[])ids.Clone();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var id in copied)
            {
                RequireId(id, parameterName);
                if (!seen.Add(id))
                {
                    throw new ArgumentException($"Duplicate workbench ID '{id}'.", parameterName);
                }
            }

            return Array.AsReadOnly(copied);
        }
    }
}
