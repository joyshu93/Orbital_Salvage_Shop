using UnityEngine;

namespace CurioClerk.Content
{
    [CreateAssetMenu(menuName = "Curio Clerk/Cosmetic", fileName = "Cosmetic")]
    public sealed class CosmeticDefinition : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private int cost;
        [SerializeField] private string nameEnglish;
        [SerializeField] private string nameKorean;
        [SerializeField] private Color accentColor = Color.white;

        public string Id => id;
        public int Cost => cost;
        public string NameEnglish => nameEnglish;
        public string NameKorean => nameKorean;
        public Color AccentColor => accentColor;

        public void Configure(CosmeticContent content)
        {
            id = content.Id;
            cost = content.Cost;
            nameEnglish = content.NameEnglish;
            nameKorean = content.NameKorean;
            ColorUtility.TryParseHtmlString("#" + content.AccentHex, out accentColor);
        }
    }
}
