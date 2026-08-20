using UnityEngine;

namespace CurioClerk.Content
{
    [CreateAssetMenu(menuName = "Curio Clerk/Difficulty Profile", fileName = "Difficulty")]
    public sealed class DifficultyProfile : ScriptableObject
    {
        [Range(1, 5), SerializeField] private int band = 1;
        [SerializeField] private int artifactCount = 12;
        [SerializeField] private int activeRuleCount = 2;

        public int Band => band;
        public int ArtifactCount => artifactCount;
        public int ActiveRuleCount => activeRuleCount;

        public void Configure(int newBand)
        {
            band = Mathf.Clamp(newBand, 1, 5);
            artifactCount = 12;
            activeRuleCount = band <= 2 ? 2 : band <= 4 ? 3 : 4;
        }
    }
}

