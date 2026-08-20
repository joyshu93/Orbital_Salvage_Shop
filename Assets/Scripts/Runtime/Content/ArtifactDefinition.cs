using CurioClerk.Core.Artifacts;
using UnityEngine;

namespace CurioClerk.Content
{
    [CreateAssetMenu(menuName = "Curio Clerk/Artifact", fileName = "Artifact")]
    public sealed class ArtifactDefinition : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string symbol;
        [SerializeField] private ArtifactTraits traits;
        [SerializeField] private string nameEnglish;
        [SerializeField] private string nameKorean;
        [TextArea, SerializeField] private string descriptionEnglish;
        [TextArea, SerializeField] private string descriptionKorean;

        public string Id => id;
        public string Symbol => symbol;
        public ArtifactTraits Traits => traits;
        public string NameEnglish => nameEnglish;
        public string NameKorean => nameKorean;
        public string DescriptionEnglish => descriptionEnglish;
        public string DescriptionKorean => descriptionKorean;

        public void Configure(ArtifactContent content)
        {
            id = content.Id;
            symbol = content.Symbol;
            traits = content.Traits;
            nameEnglish = content.NameEnglish;
            nameKorean = content.NameKorean;
            descriptionEnglish = content.DescriptionEnglish;
            descriptionKorean = content.DescriptionKorean;
        }

        public Artifact ToArtifact() => new Artifact(id, traits);
    }
}

