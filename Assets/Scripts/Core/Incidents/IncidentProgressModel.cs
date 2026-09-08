using System;
using System.Collections.Generic;

namespace CurioClerk.Core.Incidents
{
    public enum IncidentLifecycle
    {
        Locked = 0,
        Available = 1,
        AwaitingContent = 2,
        Resolved = 3
    }

    public sealed class IncidentProgressDefinition
    {
        public IncidentProgressDefinition(
            string id,
            IReadOnlyList<string> stageIds,
            bool completesWhenAllStagesCompleted)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Incident IDs cannot be blank.", nameof(id));
            }

            if (stageIds == null || stageIds.Count == 0)
            {
                throw new ArgumentException("An incident requires at least one stage.", nameof(stageIds));
            }

            var copiedStageIds = new List<string>(stageIds.Count);
            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (var index = 0; index < stageIds.Count; index++)
            {
                var stageId = stageIds[index];
                if (string.IsNullOrWhiteSpace(stageId))
                {
                    throw new ArgumentException("Stage IDs cannot be blank.", nameof(stageIds));
                }

                if (!seen.Add(stageId))
                {
                    throw new ArgumentException("Stage IDs must be unique.", nameof(stageIds));
                }

                copiedStageIds.Add(stageId);
            }

            Id = id;
            StageIds = copiedStageIds.AsReadOnly();
            CompletesWhenAllStagesCompleted = completesWhenAllStagesCompleted;
        }

        public string Id { get; }

        public IReadOnlyList<string> StageIds { get; }

        public bool CompletesWhenAllStagesCompleted { get; }
    }

    public sealed class IncidentProgressEntry
    {
        public IncidentProgressEntry(
            IncidentProgressDefinition definition,
            IncidentLifecycle lifecycle,
            int nextStageIndex)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (!Enum.IsDefined(typeof(IncidentLifecycle), lifecycle))
            {
                throw new ArgumentOutOfRangeException(nameof(lifecycle));
            }

            if (nextStageIndex < 0 || nextStageIndex > definition.StageIds.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(nextStageIndex));
            }

            Definition = definition;
            Lifecycle = lifecycle;
            NextStageIndex = nextStageIndex;
        }

        public IncidentProgressDefinition Definition { get; }

        public IncidentLifecycle Lifecycle { get; }

        public int NextStageIndex { get; }
    }

    public sealed class IncidentProgressSnapshot
    {
        private readonly IReadOnlyList<IncidentProgressEntry> _incidents;

        public IncidentProgressSnapshot(IReadOnlyList<IncidentProgressEntry> incidents)
        {
            if (incidents == null)
            {
                throw new ArgumentNullException(nameof(incidents));
            }

            var copiedIncidents = new List<IncidentProgressEntry>(incidents.Count);
            var seenIds = new HashSet<string>(StringComparer.Ordinal);
            for (var index = 0; index < incidents.Count; index++)
            {
                var incident = incidents[index];
                if (incident == null)
                {
                    throw new ArgumentException("Incident progress entries cannot be null.", nameof(incidents));
                }

                if (!seenIds.Add(incident.Definition.Id))
                {
                    throw new ArgumentException("Incident progress entries must have unique IDs.", nameof(incidents));
                }

                copiedIncidents.Add(incident);
            }

            _incidents = copiedIncidents.AsReadOnly();
            Current = FindFirstUnlockedNonResolved(_incidents);
        }

        public IReadOnlyList<IncidentProgressEntry> Incidents => _incidents;

        public IncidentProgressEntry Current { get; }

        public IncidentProgressEntry Find(string incidentId)
        {
            if (string.IsNullOrWhiteSpace(incidentId))
            {
                return null;
            }

            foreach (var incident in _incidents)
            {
                if (string.Equals(incident.Definition.Id, incidentId, StringComparison.Ordinal))
                {
                    return incident;
                }
            }

            return null;
        }

        private static IncidentProgressEntry FindFirstUnlockedNonResolved(
            IReadOnlyList<IncidentProgressEntry> incidents)
        {
            foreach (var incident in incidents)
            {
                if (incident.Lifecycle == IncidentLifecycle.Available ||
                    incident.Lifecycle == IncidentLifecycle.AwaitingContent)
                {
                    return incident;
                }
            }

            return null;
        }
    }
}
