namespace CurioClerk.Content
{
    public sealed class CosmeticContent
    {
        public CosmeticContent(string id, int cost, string nameEnglish, string nameKorean, string accentHex)
        {
            Id = id;
            Cost = cost;
            NameEnglish = nameEnglish;
            NameKorean = nameKorean;
            AccentHex = accentHex;
        }

        public string Id { get; }
        public int Cost { get; }
        public string NameEnglish { get; }
        public string NameKorean { get; }
        public string AccentHex { get; }
    }
}

