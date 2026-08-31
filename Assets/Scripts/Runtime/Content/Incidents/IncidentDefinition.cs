using System;
using System.Collections.Generic;
using CurioClerk.Core.Artifacts;
using CurioClerk.Core.Incidents;
using CurioClerk.Core.Rules;
using CurioClerk.Core.Shifts;

namespace CurioClerk.Content.Incidents
{
    public enum SeniorClerkMood
    {
        Neutral = 0,
        Concerned = 1,
        Alert = 2,
        Relieved = 3
    }

    public enum IncidentVisualCue
    {
        None = 0,
        Frost = 1,
        InkSeal = 2,
        AmberWarmth = 3,
        Rain = 4
    }

    public sealed class LocalizedCopy
    {
        public LocalizedCopy(string english, string korean)
        {
            English = english;
            Korean = korean;
        }

        public string English { get; }

        public string Korean { get; }

        public string ForLocale(string locale) => locale == "ko" ? Korean : English;
    }

    public sealed class NarrativeBeat
    {
        public NarrativeBeat(LocalizedCopy copy, SeniorClerkMood mood, IncidentVisualCue visualCue)
        {
            Copy = copy;
            Mood = mood;
            VisualCue = visualCue;
        }

        public LocalizedCopy Copy { get; }

        public SeniorClerkMood Mood { get; }

        public IncidentVisualCue VisualCue { get; }
    }

    public sealed class ArtifactReaction
    {
        public ArtifactReaction(LocalizedCopy stable, LocalizedCopy precise, LocalizedCopy resonant)
        {
            Stable = stable;
            Precise = precise;
            Resonant = resonant;
        }

        public LocalizedCopy Stable { get; }

        public LocalizedCopy Precise { get; }

        public LocalizedCopy Resonant { get; }

        public LocalizedCopy ForQuality(IncidentQuality quality)
        {
            switch (quality)
            {
                case IncidentQuality.Stable:
                    return Stable;
                case IncidentQuality.Precise:
                    return Precise;
                case IncidentQuality.Resonant:
                    return Resonant;
                default:
                    throw new ArgumentOutOfRangeException(nameof(quality));
            }
        }
    }

    public sealed class IncidentArtifactEntry
    {
        public IncidentArtifactEntry(string artifactId, ArtifactTraits addedTraits)
        {
            if (string.IsNullOrWhiteSpace(artifactId))
            {
                throw new ArgumentException("Incident artifact IDs cannot be blank.", nameof(artifactId));
            }

            ArtifactId = artifactId;
            AddedTraits = addedTraits;
        }

        public string ArtifactId { get; }

        public ArtifactTraits AddedTraits { get; }
    }

    public sealed class IncidentStageDefinition
    {
        public IncidentStageDefinition(
            string id,
            IReadOnlyList<NarrativeBeat> introBeats,
            IReadOnlyList<NarrativeBeat> outroBeats,
            ArtifactReaction reactions,
            string leadArtifactId,
            string resonanceHoldArtifactId,
            IReadOnlyList<IncidentArtifactEntry> queue,
            IReadOnlyList<SortingRule> rules,
            int minimumRequiredHolds)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Incident stage IDs cannot be blank.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(leadArtifactId))
            {
                throw new ArgumentException("A lead artifact ID is required.", nameof(leadArtifactId));
            }

            if (minimumRequiredHolds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumRequiredHolds));
            }

            Id = id;
            IntroBeats = Copy(introBeats, nameof(introBeats));
            OutroBeats = Copy(outroBeats, nameof(outroBeats));
            Reactions = reactions;
            LeadArtifactId = leadArtifactId;
            ResonanceHoldArtifactId = string.IsNullOrWhiteSpace(resonanceHoldArtifactId)
                ? null
                : resonanceHoldArtifactId;
            Queue = Copy(queue, nameof(queue));
            Rules = Copy(rules, nameof(rules));
            MinimumRequiredHolds = minimumRequiredHolds;
        }

        public string Id { get; }

        public IReadOnlyList<NarrativeBeat> IntroBeats { get; }

        public IReadOnlyList<NarrativeBeat> OutroBeats { get; }

        public ArtifactReaction Reactions { get; }

        public string LeadArtifactId { get; }

        public string ResonanceHoldArtifactId { get; }

        public IReadOnlyList<IncidentArtifactEntry> Queue { get; }

        public IReadOnlyList<SortingRule> Rules { get; }

        public int MinimumRequiredHolds { get; }

        public ShiftPlan CreateShiftPlan(IReadOnlyDictionary<string, ArtifactContent> artifacts)
        {
            if (artifacts == null)
            {
                throw new ArgumentNullException(nameof(artifacts));
            }

            if (Queue.Count != 12)
            {
                throw new InvalidOperationException("Incident shift queues must contain exactly twelve artifacts.");
            }

            if (Rules.Count == 0)
            {
                throw new InvalidOperationException("Incident shifts require at least one sorting rule.");
            }

            var seenIds = new HashSet<string>(StringComparer.Ordinal);
            var authoredQueue = new Artifact[Queue.Count];
            for (var index = 0; index < Queue.Count; index++)
            {
                var entry = Queue[index];
                if (!seenIds.Add(entry.ArtifactId))
                {
                    throw new InvalidOperationException("Incident shift queues must contain unique artifact IDs.");
                }

                if (!artifacts.TryGetValue(entry.ArtifactId, out var content) || content == null)
                {
                    throw new KeyNotFoundException($"Incident artifact '{entry.ArtifactId}' is missing from the catalog.");
                }

                authoredQueue[index] = new Artifact(content.Id, content.Traits | entry.AddedTraits);
            }

            return new ShiftPlan(Id, Id, authoredQueue, Rules);
        }

        private static IReadOnlyList<T> Copy<T>(IReadOnlyList<T> source, string parameterName)
            where T : class
        {
            if (source == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            var copy = new T[source.Count];
            for (var index = 0; index < source.Count; index++)
            {
                copy[index] = source[index] ??
                    throw new ArgumentException("Incident content collections cannot contain null entries.", parameterName);
            }

            return Array.AsReadOnly(copy);
        }
    }

    public sealed class IncidentDefinition
    {
        public IncidentDefinition(string id, LocalizedCopy title, IReadOnlyList<IncidentStageDefinition> stages)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Incident IDs cannot be blank.", nameof(id));
            }

            if (title == null)
            {
                throw new ArgumentNullException(nameof(title));
            }

            if (stages == null || stages.Count == 0)
            {
                throw new ArgumentException("An incident requires at least one stage.", nameof(stages));
            }

            var copiedStages = new IncidentStageDefinition[stages.Count];
            var ids = new HashSet<string>(StringComparer.Ordinal);
            for (var index = 0; index < stages.Count; index++)
            {
                var stage = stages[index] ??
                    throw new ArgumentException("Incident stage collections cannot contain null entries.", nameof(stages));
                if (!ids.Add(stage.Id))
                {
                    throw new ArgumentException("Incident stage IDs must be unique.", nameof(stages));
                }

                copiedStages[index] = stage;
            }

            Id = id;
            Title = title;
            Stages = Array.AsReadOnly(copiedStages);
        }

        public string Id { get; }

        public LocalizedCopy Title { get; }

        public IReadOnlyList<IncidentStageDefinition> Stages { get; }
    }
}
