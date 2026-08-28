using System;
using System.Collections.Generic;

namespace CurioClerk.Core.Incidents
{
    public sealed class IncidentRunner
    {
        private readonly IReadOnlyList<string> _stageIds;

        public IncidentRunner(string incidentId, IReadOnlyList<string> stageIds, int startingStageIndex)
        {
            if (string.IsNullOrWhiteSpace(incidentId))
            {
                throw new ArgumentException("Incident IDs cannot be blank.", nameof(incidentId));
            }

            if (stageIds == null || stageIds.Count == 0)
            {
                throw new ArgumentException("An incident requires at least one stage.", nameof(stageIds));
            }

            if (startingStageIndex < 0 || startingStageIndex > stageIds.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(startingStageIndex));
            }

            var copiedStageIds = new List<string>(stageIds.Count);
            var seen = new HashSet<string>();
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

            IncidentId = incidentId;
            _stageIds = copiedStageIds.AsReadOnly();
            CurrentStageIndex = startingStageIndex;
        }

        public string IncidentId { get; }

        public int CurrentStageIndex { get; private set; }

        public string CurrentStageId => IsComplete ? null : _stageIds[CurrentStageIndex];

        public bool IsComplete => CurrentStageIndex >= _stageIds.Count;

        public IncidentStageCompletion CompleteCurrentStage(IncidentQuality quality)
        {
            if (IsComplete)
            {
                throw new InvalidOperationException("The incident is already complete.");
            }

            if (!Enum.IsDefined(typeof(IncidentQuality), quality))
            {
                throw new ArgumentOutOfRangeException(nameof(quality));
            }

            var completedIndex = CurrentStageIndex;
            var completion = new IncidentStageCompletion(
                IncidentId,
                _stageIds[completedIndex],
                completedIndex,
                quality,
                completedIndex + 1,
                completedIndex + 1 == _stageIds.Count);
            CurrentStageIndex++;
            return completion;
        }
    }

    public sealed class IncidentStageCompletion
    {
        internal IncidentStageCompletion(
            string incidentId,
            string stageId,
            int completedStageIndex,
            IncidentQuality quality,
            int nextStageIndex,
            bool incidentCompleted)
        {
            IncidentId = incidentId;
            StageId = stageId;
            CompletedStageIndex = completedStageIndex;
            Quality = quality;
            NextStageIndex = nextStageIndex;
            IncidentCompleted = incidentCompleted;
        }

        public string IncidentId { get; }
        public string StageId { get; }
        public int CompletedStageIndex { get; }
        public IncidentQuality Quality { get; }
        public int NextStageIndex { get; }
        public bool IncidentCompleted { get; }
    }
}
