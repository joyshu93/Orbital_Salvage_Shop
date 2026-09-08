using System;
using System.Collections.Generic;
using System.Linq;
using CurioClerk.Content.Incidents;
using CurioClerk.Core.Incidents;
using CurioClerk.Localization;

namespace CurioClerk.Presentation
{
    public enum IncidentCardAction
    {
        None = 0,
        Start = 1,
        Replay = 2
    }

    public sealed class IncidentCardState
    {
        public IncidentCardState(
            string incidentId,
            string title,
            string leadArtifactId,
            string status,
            string clue,
            string actionLabel,
            IncidentCardAction action,
            IncidentLifecycle lifecycle)
        {
            IncidentId = incidentId;
            Title = title;
            LeadArtifactId = leadArtifactId;
            Status = status;
            Clue = clue;
            ActionLabel = actionLabel;
            Action = action;
            Lifecycle = lifecycle;
        }

        public string IncidentId { get; }
        public string Title { get; }
        public string LeadArtifactId { get; }
        public string Status { get; }
        public string Clue { get; }
        public string ActionLabel { get; }
        public IncidentCardAction Action { get; }
        public IncidentLifecycle Lifecycle { get; }
    }

    public sealed class IncidentBoardState
    {
        public IncidentBoardState(IncidentCardState current, IReadOnlyList<IncidentCardState> resolved)
        {
            Current = current;
            Resolved = Array.AsReadOnly((resolved ?? Array.Empty<IncidentCardState>()).ToArray());
        }

        public IncidentCardState Current { get; }
        public IReadOnlyList<IncidentCardState> Resolved { get; }
    }

    public sealed class IncidentBoardPresenter
    {
        public static bool ShouldRevealSuccessor(
            IncidentProgressSnapshot progress,
            string completedIncidentId)
        {
            if (progress == null) throw new ArgumentNullException(nameof(progress));
            if (string.IsNullOrWhiteSpace(completedIncidentId)) return false;
            return progress.Current != null &&
                   !string.Equals(progress.Current.Definition.Id, completedIncidentId, StringComparison.Ordinal);
        }

        public IncidentBoardState Build(
            IReadOnlyList<IncidentDefinition> incidents,
            IncidentProgressSnapshot progress,
            Localizer localizer)
        {
            if (incidents == null) throw new ArgumentNullException(nameof(incidents));
            if (progress == null) throw new ArgumentNullException(nameof(progress));
            if (localizer == null) throw new ArgumentNullException(nameof(localizer));

            var incidentById = incidents.ToDictionary(incident => incident.Id, StringComparer.Ordinal);
            var resolved = new List<IncidentCardState>();
            IncidentCardState current = null;
            foreach (var entry in progress.Incidents)
            {
                if (!incidentById.TryGetValue(entry.Definition.Id, out var incident)) continue;
                if (entry.Lifecycle == IncidentLifecycle.Resolved)
                {
                    resolved.Add(Card(incident, entry, localizer));
                }
                else if (current == null &&
                         (entry.Lifecycle == IncidentLifecycle.Available || entry.Lifecycle == IncidentLifecycle.AwaitingContent))
                {
                    current = Card(incident, entry, localizer);
                }
            }

            return new IncidentBoardState(current, resolved);
        }

        private static IncidentCardState Card(IncidentDefinition incident, IncidentProgressEntry entry, Localizer localizer)
        {
            var title = incident.Title.ForLocale(localizer.Locale);
            var stageNumber = entry.NextStageIndex + 1;
            var stageCount = incident.Stages.Count;
            switch (entry.Lifecycle)
            {
                case IncidentLifecycle.Available:
                    var isFirstInvestigation = entry.NextStageIndex == 0;
                    return new IncidentCardState(
                        incident.Id, title, incident.LeadArtifactId,
                        isFirstInvestigation
                            ? localizer.Get("incident_first_investigation")
                            : localizer.Get("incident_in_progress"),
                        string.Empty,
                        isFirstInvestigation
                            ? localizer.Get("incident_first_investigation")
                            : localizer.Get("incident_continue", stageNumber, stageCount),
                        IncidentCardAction.Start, entry.Lifecycle);
                case IncidentLifecycle.AwaitingContent:
                    return new IncidentCardState(
                        incident.Id, title, incident.LeadArtifactId,
                        localizer.Get("incident_waiting"),
                        incident.AwaitingContentClue?.ForLocale(localizer.Locale) ?? string.Empty,
                        string.Empty, IncidentCardAction.None, entry.Lifecycle);
                case IncidentLifecycle.Resolved:
                    return new IncidentCardState(
                        incident.Id, title, incident.LeadArtifactId,
                        localizer.Get("incident_resolved"), string.Empty,
                        localizer.Get("incident_replay_case"), IncidentCardAction.Replay, entry.Lifecycle);
                default:
                    throw new InvalidOperationException("Locked incidents are not board cards.");
            }
        }
    }
}
