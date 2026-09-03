using System;
using System.Collections.Generic;
using CurioClerk.Core.Progression;

namespace CurioClerk.Core.Incidents
{
    public sealed class IncidentProgressResolver
    {
        public IncidentProgressSnapshot Resolve(
            PlayerSaveData save,
            IReadOnlyList<IncidentProgressDefinition> definitions)
        {
            if (save == null)
            {
                throw new ArgumentNullException(nameof(save));
            }

            if (definitions == null)
            {
                throw new ArgumentNullException(nameof(definitions));
            }

            var recordedStageIds = RecordedStageIds(save.incidentStageRecords);
            var completedIncidentIds = CompletedIncidentIds(save.completedIncidentIds);
            var entries = new List<IncidentProgressEntry>(definitions.Count);
            var knownIncidentIds = new HashSet<string>(StringComparer.Ordinal);
            var previousResolved = false;

            for (var index = 0; index < definitions.Count; index++)
            {
                var definition = definitions[index];
                if (definition == null)
                {
                    throw new ArgumentException("Incident definitions cannot contain null entries.", nameof(definitions));
                }

                if (!knownIncidentIds.Add(definition.Id))
                {
                    throw new ArgumentException("Incident definition IDs must be unique.", nameof(definitions));
                }

                var recordedPrefix = CountContiguousRecordedStages(definition.StageIds, recordedStageIds);
                var legacyIndex = string.Equals(save.activeIncidentId, definition.Id, StringComparison.Ordinal)
                    ? Math.Min(save.activeIncidentStage, definition.StageIds.Count)
                    : 0;
                var nextStageIndex = Math.Max(recordedPrefix, legacyIndex);
                var explicitlyResolved = completedIncidentIds.Contains(definition.Id);
                var resolved = explicitlyResolved ||
                               (definition.CompletesWhenAllStagesCompleted &&
                                nextStageIndex == definition.StageIds.Count);
                var unlocked = index == 0 || previousResolved;
                var lifecycle = resolved
                    ? IncidentLifecycle.Resolved
                    : !unlocked
                        ? IncidentLifecycle.Locked
                        : nextStageIndex < definition.StageIds.Count
                            ? IncidentLifecycle.Available
                            : IncidentLifecycle.AwaitingContent;

                entries.Add(new IncidentProgressEntry(definition, lifecycle, nextStageIndex));
                previousResolved = resolved;
            }

            return new IncidentProgressSnapshot(entries);
        }

        private static HashSet<string> RecordedStageIds(IEnumerable<IncidentStageRecord> records)
        {
            var stageIds = new HashSet<string>(StringComparer.Ordinal);
            if (records == null)
            {
                return stageIds;
            }

            foreach (var record in records)
            {
                if (record != null && !string.IsNullOrWhiteSpace(record.stageId))
                {
                    stageIds.Add(record.stageId);
                }
            }

            return stageIds;
        }

        private static HashSet<string> CompletedIncidentIds(IEnumerable<string> incidentIds)
        {
            var completedIds = new HashSet<string>(StringComparer.Ordinal);
            if (incidentIds == null)
            {
                return completedIds;
            }

            foreach (var incidentId in incidentIds)
            {
                if (!string.IsNullOrWhiteSpace(incidentId))
                {
                    completedIds.Add(incidentId);
                }
            }

            return completedIds;
        }

        private static int CountContiguousRecordedStages(
            IReadOnlyList<string> stageIds,
            ISet<string> recordedStageIds)
        {
            var count = 0;
            while (count < stageIds.Count && recordedStageIds.Contains(stageIds[count]))
            {
                count++;
            }

            return count;
        }
    }
}
