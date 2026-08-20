using System;
using CurioClerk.Core.Artifacts;

namespace CurioClerk.Core.Rules
{
    public sealed class SortingRule
    {
        public SortingRule(
            string id,
            ArtifactTraits requiredAll,
            ArtifactTraits requiredAny,
            Destination destination,
            bool isFallback)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Rule id is required.", nameof(id));
            }

            if (isFallback && (requiredAll != ArtifactTraits.None || requiredAny != ArtifactTraits.None))
            {
                throw new ArgumentException("A fallback rule cannot require traits.", nameof(isFallback));
            }

            if (!isFallback && requiredAll == ArtifactTraits.None && requiredAny == ArtifactTraits.None)
            {
                throw new ArgumentException("A non-fallback rule must require at least one trait.", nameof(requiredAll));
            }

            Id = id;
            RequiredAll = requiredAll;
            RequiredAny = requiredAny;
            Destination = destination;
            IsFallback = isFallback;
        }

        public string Id { get; }

        public ArtifactTraits RequiredAll { get; }

        public ArtifactTraits RequiredAny { get; }

        public Destination Destination { get; }

        public bool IsFallback { get; }

        public bool Matches(Artifact artifact)
        {
            if (artifact == null)
            {
                throw new ArgumentNullException(nameof(artifact));
            }

            if (IsFallback)
            {
                return true;
            }

            var hasAll = (artifact.Traits & RequiredAll) == RequiredAll;
            var hasAny = RequiredAny == ArtifactTraits.None || (artifact.Traits & RequiredAny) != ArtifactTraits.None;
            return hasAll && hasAny;
        }
    }
}

