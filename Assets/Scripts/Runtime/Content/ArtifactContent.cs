using CurioClerk.Core.Artifacts;

namespace CurioClerk.Content
{
    public sealed class ArtifactContent
    {
        public ArtifactContent(
            string id,
            string symbol,
            ArtifactTraits traits,
            string nameEnglish,
            string nameKorean,
            string descriptionEnglish,
            string descriptionKorean,
            string resolutionEnglish,
            string resolutionKorean)
        {
            Id = id;
            Symbol = symbol;
            Traits = traits;
            NameEnglish = nameEnglish;
            NameKorean = nameKorean;
            DescriptionEnglish = descriptionEnglish;
            DescriptionKorean = descriptionKorean;
            ResolutionEnglish = resolutionEnglish;
            ResolutionKorean = resolutionKorean;
        }

        public string Id { get; }
        public string Symbol { get; }
        public ArtifactTraits Traits { get; }
        public string NameEnglish { get; }
        public string NameKorean { get; }
        public string DescriptionEnglish { get; }
        public string DescriptionKorean { get; }
        public string ResolutionEnglish { get; }
        public string ResolutionKorean { get; }

        public Artifact ToArtifact() => new Artifact(Id, Traits);
    }
}
