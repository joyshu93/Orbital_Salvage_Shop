using System;

namespace CurioClerk.Core.Artifacts
{
    public sealed class Artifact
    {
        public Artifact(string id, ArtifactTraits traits)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Artifact id is required.", nameof(id));
            }

            Id = id;
            Traits = traits;
        }

        public string Id { get; }

        public ArtifactTraits Traits { get; }
    }
}

