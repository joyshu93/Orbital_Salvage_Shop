using System;
using System.Collections.Generic;

namespace CurioClerk.Core.Incidents
{
    public sealed class IncidentRunner
    {
        private readonly IReadOnlyList<string> _stageIds;
        private readonly bool _completesWhenAllStagesCompleted;

        public IncidentRunner(string incidentId, IReadOnlyList<string> stageIds, int startingStageIndex)
            : this(incidentId, stageIds, startingStageIndex, completesWhenAllStagesCompleted: true)
        {
        }

        public IncidentRunner(
            string incidentId,
            IReadOnlyList<string> stageIds,
            int startingStageIndex,
            bool completesWhenAllStagesCompleted)
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
            _completesWhenAllStagesCompleted = completesWhenAllStagesCompleted;
            CurrentStageIndex = startingStageIndex;
        }

        public string IncidentId { get; }

        public int CurrentStageIndex { get; private set; }

        public string CurrentStageId => IsContentExhausted ? null : _stageIds[CurrentStageIndex];

        public bool IsContentExhausted => CurrentStageIndex >= _stageIds.Count;

        public bool IsComplete => IsContentExhausted;

        public IncidentStageCompletion CompleteCurrentStage(IncidentQuality quality)
        {
            if (IsContentExhausted)
            {
                throw new InvalidOperationException("The incident is already complete.");
            }

            if (!Enum.IsDefined(typeof(IncidentQuality), quality))
            {
                throw new ArgumentOutOfRangeException(nameof(quality));
            }

            var completedIndex = CurrentStageIndex;
            var exhausted = completedIndex + 1 == _stageIds.Count;
            var completion = new IncidentStageCompletion(
                IncidentId,
                _stageIds[completedIndex],
                completedIndex,
                quality,
                completedIndex + 1,
                exhausted && _completesWhenAllStagesCompleted);
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
