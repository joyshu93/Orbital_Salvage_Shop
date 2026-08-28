using System;
using CurioClerk.Core.Shifts;

namespace CurioClerk.Core.Incidents
{
    public sealed class IncidentStageRun
    {
        private readonly string _resonanceHoldArtifactId;

        public IncidentStageRun(string stageId, string resonanceHoldArtifactId)
        {
            if (string.IsNullOrWhiteSpace(stageId))
            {
                throw new ArgumentException("Stage IDs cannot be blank.", nameof(stageId));
            }

            StageId = stageId;
            _resonanceHoldArtifactId = resonanceHoldArtifactId;
        }

        public string StageId { get; }

        public bool ResonanceConditionMet { get; private set; }

        public void RecordHold(string artifactId)
        {
            if (string.IsNullOrWhiteSpace(artifactId))
            {
                throw new ArgumentException("Artifact IDs cannot be blank.", nameof(artifactId));
            }

            if (!string.IsNullOrWhiteSpace(_resonanceHoldArtifactId) && artifactId == _resonanceHoldArtifactId)
            {
                ResonanceConditionMet = true;
            }
        }

        public IncidentQuality Evaluate(ShiftResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (result.State != ShiftState.Completed)
            {
                throw new InvalidOperationException("Only completed shifts can produce an incident quality.");
            }

            if (result.Mistakes > 0)
            {
                return IncidentQuality.Stable;
            }

            return ResonanceConditionMet ? IncidentQuality.Resonant : IncidentQuality.Precise;
        }
    }
}
