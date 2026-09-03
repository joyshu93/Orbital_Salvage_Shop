using CurioClerk.Core.Artifacts;
using CurioClerk.Core.Rules;

namespace CurioClerk.Content.Incidents
{
    public static class SecondIncidentCatalog
    {
        public static IncidentDefinition Create()
        {
            return new IncidentDefinition(
                "remembering-rain",
                Copy("The Remembering Rain", "기억하는 비"),
                leadArtifactId: "moon-umbrella",
                boardVisualCue: IncidentVisualCue.Rain,
                completesWhenAllStagesCompleted: false,
                awaitingContentClue: Copy(
                    "The voice inside the rain knows the senior clerk.",
                    "빗속의 목소리는 선임 관리인을 알고 있다."),
                stages: new[] { FirstShift() });
        }

        private static IncidentStageDefinition FirstShift()
        {
            return new IncidentStageDefinition(
                "rain-01-voices",
                new[]
                {
                    Beat(
                        "Since midnight, it has been raining inside the sealed umbrella. Do not open it.",
                        "자정부터 봉인된 우산 안에서 비가 내리고 있어요. 절대 열지 마세요.",
                        SeniorClerkMood.Concerned,
                        IncidentVisualCue.Rain),
                    Beat(
                        "Every drop repeats someone's memory. One of them is saying my name.",
                        "빗방울마다 누군가의 기억을 되풀이합니다. 그중 하나가 제 이름을 부르고 있어요.",
                        SeniorClerkMood.Alert,
                        IncidentVisualCue.Rain),
                    Beat(
                        "Seal what the rain has touched. Mend the fragile. Let the living rest. Read the ledger from the top.",
                        "비에 젖은 것은 봉인하고, 깨지기 쉬운 것은 수리하세요. 살아 있는 것은 쉬게 하세요. 규칙은 위에서부터 적용합니다.",
                        SeniorClerkMood.Neutral,
                        IncidentVisualCue.InkSeal)
                },
                new[]
                {
                    Beat(
                        "The rain falls silent. One drop remains on the inside of the seal.",
                        "비가 멎습니다. 봉인 안쪽에 빗방울 하나만 남았습니다.",
                        SeniorClerkMood.Concerned,
                        IncidentVisualCue.Rain),
                    Beat(
                        "It whispers, “You promised to come back.” The senior clerk does not answer.",
                        "빗방울이 속삭입니다. “돌아오겠다고 약속했잖아.” 선임 관리인은 대답하지 않습니다.",
                        SeniorClerkMood.Alert,
                        IncidentVisualCue.Rain)
                },
                new ArtifactReaction(
                    Copy(
                        "The last drop shivers, but does not fall.",
                        "마지막 빗방울이 떨리지만 떨어지지 않는다."),
                    Copy(
                        "The rain gathers into one clear memory.",
                        "비가 하나의 선명한 기억으로 모인다."),
                    Copy(
                        "The umbrella closes by itself, as if it recognizes your hands.",
                        "우산이 스스로 접힌다. 당신의 손길을 알아본 듯하다.")),
                "moon-umbrella",
                "paper-fish",
                new[]
                {
                    new IncidentArtifactEntry("moon-umbrella", ArtifactTraits.None),
                    new IncidentArtifactEntry("paper-fish", ArtifactTraits.None),
                    new IncidentArtifactEntry("sleeping-teacup", ArtifactTraits.None),
                    new IncidentArtifactEntry("clockwork-moth", ArtifactTraits.None),
                    new IncidentArtifactEntry("backward-candle", ArtifactTraits.None),
                    new IncidentArtifactEntry("patient-compass", ArtifactTraits.None),
                    new IncidentArtifactEntry("rain-jar", ArtifactTraits.None),
                    new IncidentArtifactEntry("porcelain-tooth", ArtifactTraits.None),
                    new IncidentArtifactEntry("humming-scarf", ArtifactTraits.None),
                    new IncidentArtifactEntry("yesterday-ticket", ArtifactTraits.None),
                    new IncidentArtifactEntry("murmur-box", ArtifactTraits.None),
                    new IncidentArtifactEntry("ink-snowglobe", ArtifactTraits.None)
                },
                new[]
                {
                    new SortingRule("incident-wet-vault", ArtifactTraits.Wet, ArtifactTraits.None, Destination.Vault, false),
                    new SortingRule("incident-fragile-repair", ArtifactTraits.Fragile, ArtifactTraits.None, Destination.Repair, false),
                    new SortingRule("incident-alive-storage", ArtifactTraits.Alive, ArtifactTraits.None, Destination.Storage, false),
                    new SortingRule("incident-fallback-storage", ArtifactTraits.None, ArtifactTraits.None, Destination.Storage, true)
                },
                minimumRequiredHolds: 2);
        }

        private static NarrativeBeat Beat(
            string english,
            string korean,
            SeniorClerkMood mood,
            IncidentVisualCue visualCue)
            => new NarrativeBeat(Copy(english, korean), mood, visualCue);

        private static LocalizedCopy Copy(string english, string korean) => new LocalizedCopy(english, korean);
    }
}
