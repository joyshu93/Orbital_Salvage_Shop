using CurioClerk.Core.Artifacts;
using CurioClerk.Core.Rules;

namespace CurioClerk.Content.Incidents
{
    public static class FirstIncidentCatalog
    {
        public static IncidentDefinition Create()
        {
            return new IncidentDefinition(
                "unmelting-ice",
                Copy("The Unmelting Ice", "녹지 않는 얼음"),
                leadArtifactId: "unmelting-ice",
                boardVisualCue: IncidentVisualCue.Frost,
                completesWhenAllStagesCompleted: true,
                awaitingContentClue: null,
                stages: new[]
                {
                    StageOne(),
                    StageTwo(),
                    StageThree(),
                    StageFour(),
                    StageFive()
                });
        }

        private static IncidentStageDefinition StageOne()
        {
            return Stage(
                "ice-01-crack",
                new[]
                {
                    Beat(
                        "First night? Remember this: nothing left behind here is truly silent.",
                        "첫날이죠? 이것만 기억하세요. 이곳에 남겨진 물건은 결코 침묵하지 않습니다.",
                        SeniorClerkMood.Neutral,
                        IncidentVisualCue.AmberWarmth),
                    Beat(
                        "This ice refuses to melt. Sort tonight's curios before the frost reaches the shelves.",
                        "이 얼음은 녹기를 거부합니다. 서리가 선반에 닿기 전에 오늘 밤 물건들을 분류하세요.",
                        SeniorClerkMood.Concerned,
                        IncidentVisualCue.Frost),
                    Beat(
                        "Each docket needs one seal from each desk. If that desk is already sealed, protect the curio in Hold.",
                        "장부마다 세 책상의 인장을 하나씩 채웁니다. 이미 찍힌 곳의 물건은 보류에서 지키세요.",
                        SeniorClerkMood.Alert,
                        IncidentVisualCue.InkSeal)
                },
                new[]
                {
                    Beat(
                        "The crack is sealed. The leaf inside moved anyway.",
                        "금은 봉합됐어요. 그런데 안쪽의 낙엽은 움직였습니다.",
                        SeniorClerkMood.Concerned,
                        IncidentVisualCue.Frost),
                    Beat(
                        "You did more than sort it. The ice answered you. Tomorrow night, follow what the frost chooses.",
                        "분류만 한 게 아니에요. 얼음이 당신에게 답했습니다. 다음 밤엔 서리가 고른 것을 따라가세요.",
                        SeniorClerkMood.Alert,
                        IncidentVisualCue.InkSeal)
                },
                "unmelting-ice",
                null,
                1,
                Reactions(
                    "The corrected route stops the frost at the shelves. The crack steadies, but the leaf keeps turning.",
                    "바로잡은 경로가 선반 앞에서 서리를 막습니다. 금은 잦아들지만 낙엽은 계속 돕니다.",
                    "Every seal lands cleanly under your calm hands. The leaf presses against the ice as if it knows your name.",
                    "침착한 손길에 모든 인장이 정확히 찍힙니다. 낙엽이 당신의 이름을 아는 듯 얼음 벽에 닿습니다.",
                    "The final seal rings. The leaf opens like an eye, and the whole office exhales warm air.",
                    "마지막 인장이 울립니다. 낙엽이 눈처럼 펼쳐지고, 보관소 전체가 따뜻한 숨을 내쉽니다."),
                Rules(
                    R(ArtifactTraits.Fragile, Destination.Repair),
                    R(ArtifactTraits.Temporal, Destination.Vault)),
                Entries(
                    E("unmelting-ice"), E("moon-umbrella"), E("clockwork-moth"), E("mossy-watch"),
                    E("sleeping-teacup"), E("patient-compass"), E("rain-jar"), E("porcelain-tooth"),
                    E("thimble-storm"), E("rusty-comet"), E("tide-locket"), E("borrowed-shadow")));
        }

        private static IncidentStageDefinition StageTwo()
        {
            return Stage(
                "ice-02-spread",
                "The frost has marked four curios. File every white-rimmed one to Storage before the cold returns to the ice.",
                "서리가 네 물건을 골랐어요. 흰 테가 생긴 것은 모두 보관실로 보내, 추위가 얼음으로 돌아가지 못하게 하세요.",
                SeniorClerkMood.Concerned,
                IncidentVisualCue.Frost,
                "All four white rims have faded. The leaf inside the ice is gone, yet not a drop escaped.",
                "네 개의 흰 테가 모두 사라졌어요. 얼음 속 낙엽도 사라졌지만, 물은 한 방울도 새지 않았습니다.",
                SeniorClerkMood.Alert,
                IncidentVisualCue.Frost,
                "unmelting-ice",
                null,
                2,
                Reactions(
                    "The corrected routes pull all four white rims away from the shelves. The ice keeps the last trace.",
                    "바로잡은 경로를 따라 네 개의 흰 테가 선반에서 걷힙니다. 마지막 서리만 얼음에 남습니다.",
                    "Under your calm care, three rims fade. The fourth races back into the ice.",
                    "침착한 손길에 세 개의 흰 테가 사라집니다. 네 번째 서리는 얼음 속으로 달아납니다.",
                    "The fourth rim snaps shut around the ice. Inside it, the leaf vanishes without a drop.",
                    "네 번째 흰 테가 얼음 둘레로 닫힙니다. 그 안에서 낙엽이 물 한 방울 없이 사라집니다."),
                Rules(
                    R(ArtifactTraits.Frosted, Destination.Storage),
                    R(ArtifactTraits.Cursed, Destination.Vault),
                    R(ArtifactTraits.Fragile, Destination.Repair)),
                Entries(
                    E("whispering-key"), E("silent-bell"), E("sleeping-teacup"), E("patient-compass", true),
                    E("backward-candle"), E("moon-umbrella", true), E("humming-scarf"), E("clockwork-moth", true),
                    E("unmelting-ice", true), E("lantern-snail"), E("murmur-box"), E("yesterday-ticket")));
        }

        private static IncidentStageDefinition StageThree()
        {
            return Stage(
                "ice-03-tomorrow",
                "The missing leaf is inside this watch, dated tomorrow. It is both frosted and temporal—time outranks frost.",
                "사라진 낙엽이 이 시계 안에 있어요. 날짜는 내일입니다. 서리와 시간성이 겹치면 시간 규칙이 먼저예요.",
                SeniorClerkMood.Alert,
                IncidentVisualCue.InkSeal,
                "The watch points back at this desk. Tomorrow is not waiting for us anymore.",
                "시계가 다시 이 책상을 가리킵니다. 이제 내일은 우리를 기다려 주지 않아요.",
                SeniorClerkMood.Concerned,
                IncidentVisualCue.InkSeal,
                "mossy-watch",
                null,
                3,
                Reactions(
                    "The corrected priority drags tomorrow's loose minute back into the watch.",
                    "바로잡은 우선순위가 내일에서 풀려난 1분을 시계 안으로 끌어옵니다.",
                    "Your calm judgment sends frost aside and gives the watch one honest present.",
                    "침착한 판단이 서리를 밀어내고 시계에 정직한 현재를 돌려줍니다.",
                    "Four frosted clocks strike tomorrow together. The watch answers by matching your pulse.",
                    "서리 낀 네 개의 시간이 함께 내일을 울립니다. 시계가 당신의 맥박에 맞춰 답합니다."),
                Rules(
                    R(ArtifactTraits.Temporal, Destination.Vault),
                    R(ArtifactTraits.Frosted, Destination.Storage),
                    R(ArtifactTraits.Fragile, Destination.Repair)),
                Entries(
                    E("moon-umbrella"), E("sleeping-teacup"), E("clockwork-moth", true), E("mossy-watch", true),
                    E("patient-compass"), E("thimble-storm"), E("unmelting-ice", true), E("porcelain-tooth"),
                    E("lantern-snail"), E("rain-jar", true), E("tide-locket"), E("rusty-comet", true)));
        }

        private static IncidentStageDefinition StageFour()
        {
            return Stage(
                "ice-04-frozen-seal",
                "The Vault seal is frozen. The watch belongs in Vault, but that seal was just used. Protect it in Hold and open Repair first.",
                "봉인고 인장이 얼었습니다. 시계는 봉인고가 맞지만 방금 그 인장을 썼어요. 보류에서 지키고 수리실을 먼저 여세요.",
                SeniorClerkMood.Alert,
                IncidentVisualCue.InkSeal,
                "The watch is safe. Everything you protected is trembling toward the moon-mended umbrella.",
                "시계는 무사합니다. 당신이 보호한 물건들이 모두 달빛으로 기운 우산을 향해 떨고 있어요.",
                SeniorClerkMood.Concerned,
                IncidentVisualCue.Frost,
                "mossy-watch",
                "mossy-watch",
                2,
                Reactions(
                    "The corrected order frees the frozen seal. The watch leaves Hold without losing a minute.",
                    "바로잡은 순서가 얼어붙은 인장을 풉니다. 시계는 1분도 잃지 않고 보류에서 나옵니다.",
                    "Your calm care opens Repair, then returns the watch to a living Vault seal.",
                    "침착한 손길이 수리실을 열고, 살아난 봉인고 인장에 시계를 돌려보냅니다.",
                    "The watch bursts out of Hold with one clear chime. Every protected curio turns toward the umbrella.",
                    "시계가 보류에서 맑은 종소리와 함께 깨어납니다. 보호한 물건들이 모두 우산을 향합니다."),
                Rules(
                    R(ArtifactTraits.Temporal, Destination.Vault),
                    R(ArtifactTraits.Frosted, Destination.Storage),
                    R(ArtifactTraits.Fragile, Destination.Repair)),
                Entries(
                    E("unmelting-ice", true), E("mossy-watch", true), E("moon-umbrella"), E("clockwork-moth", true),
                    E("sleeping-teacup"), E("porcelain-tooth"), E("patient-compass", true), E("lantern-snail"),
                    E("thimble-storm", true), E("rain-jar"), E("tide-locket", true), E("rusty-comet")));
        }

        private static IncidentStageDefinition StageFive()
        {
            return Stage(
                "ice-05-thaw",
                "One last shift. No new rule: protect what cannot be filed, trust time over frost, and listen for rain.",
                "마지막 교대입니다. 새 규칙은 없어요. 지금 처리할 수 없는 것은 보호하고, 서리보다 시간을 믿고, 빗소리를 따라가세요.",
                SeniorClerkMood.Neutral,
                IncidentVisualCue.Frost,
                "The ice collapses into warm light—without water. Rain answers from inside the sealed umbrella.",
                "얼음이 물 한 방울 없이 따뜻한 빛으로 무너집니다. 봉인된 우산 안에서 비가 대답합니다.",
                SeniorClerkMood.Relieved,
                IncidentVisualCue.Rain,
                "moon-umbrella",
                "moon-umbrella",
                3,
                Reactions(
                    "The corrected order draws the last frost out of the room. The umbrella stays sealed, but something rains inside.",
                    "바로잡은 순서가 방 안의 마지막 서리를 걷어냅니다. 우산은 봉인됐지만, 그 안에서 무언가 비를 내립니다.",
                    "Your calm care leaves the desk dry. A single raindrop rings from inside the sealed umbrella.",
                    "침착한 손길 뒤 책상은 마른 채로 남습니다. 봉인된 우산 안에서 빗방울 하나가 울립니다.",
                    "The ice collapses into warm light. Rain drums inside the umbrella, and the whole office answers.",
                    "얼음이 따뜻한 빛으로 무너집니다. 우산 안에서 비가 북을 울리고 보관소 전체가 답합니다."),
                Rules(
                    R(ArtifactTraits.Temporal, Destination.Vault),
                    R(ArtifactTraits.Frosted, Destination.Storage),
                    R(ArtifactTraits.Fragile, Destination.Repair)),
                Entries(
                    E("paper-fish"), E("moon-umbrella"), E("clockwork-moth", true), E("unmelting-ice", true),
                    E("patient-compass"), E("mossy-watch"), E("thimble-storm"), E("mirror-seed"),
                    E("ink-snowglobe"), E("rain-jar", true), E("tide-locket", true), E("rusty-comet")));
        }

        private static IncidentStageDefinition Stage(
            string id,
            NarrativeBeat[] introBeats,
            NarrativeBeat[] outroBeats,
            string leadArtifactId,
            string resonanceHoldArtifactId,
            int minimumRequiredHolds,
            ArtifactReaction reactions,
            SortingRule[] rules,
            IncidentArtifactEntry[] queue)
        {
            return new IncidentStageDefinition(
                id,
                introBeats,
                outroBeats,
                reactions,
                leadArtifactId,
                resonanceHoldArtifactId,
                queue,
                rules,
                minimumRequiredHolds);
        }

        private static IncidentStageDefinition Stage(
            string id,
            string introEnglish,
            string introKorean,
            SeniorClerkMood introMood,
            IncidentVisualCue introCue,
            string outroEnglish,
            string outroKorean,
            SeniorClerkMood outroMood,
            IncidentVisualCue outroCue,
            string leadArtifactId,
            string resonanceHoldArtifactId,
            int minimumRequiredHolds,
            ArtifactReaction reactions,
            SortingRule[] rules,
            IncidentArtifactEntry[] queue)
        {
            return Stage(
                id,
                new[] { new NarrativeBeat(Copy(introEnglish, introKorean), introMood, introCue) },
                new[] { new NarrativeBeat(Copy(outroEnglish, outroKorean), outroMood, outroCue) },
                leadArtifactId,
                resonanceHoldArtifactId,
                minimumRequiredHolds,
                reactions,
                rules,
                queue);
        }

        private static NarrativeBeat Beat(
            string english,
            string korean,
            SeniorClerkMood mood,
            IncidentVisualCue visualCue)
            => new NarrativeBeat(Copy(english, korean), mood, visualCue);

        private static ArtifactReaction Reactions(
            string stableEnglish,
            string stableKorean,
            string preciseEnglish,
            string preciseKorean,
            string resonantEnglish,
            string resonantKorean)
        {
            return new ArtifactReaction(
                Copy(stableEnglish, stableKorean),
                Copy(preciseEnglish, preciseKorean),
                Copy(resonantEnglish, resonantKorean));
        }

        private static SortingRule[] Rules(params RuleSpec[] orderedRules)
        {
            var rules = new SortingRule[orderedRules.Length + 1];
            for (var index = 0; index < orderedRules.Length; index++)
            {
                var rule = orderedRules[index];
                rules[index] = new SortingRule(
                    $"incident-{rule.Traits.ToString().ToLowerInvariant()}-{rule.Destination.ToString().ToLowerInvariant()}",
                    rule.Traits,
                    ArtifactTraits.None,
                    rule.Destination,
                    false);
            }

            rules[rules.Length - 1] = new SortingRule(
                "incident-fallback-storage",
                ArtifactTraits.None,
                ArtifactTraits.None,
                Destination.Storage,
                true);
            return rules;
        }

        private static RuleSpec R(ArtifactTraits traits, Destination destination)
            => new RuleSpec(traits, destination);

        private static IncidentArtifactEntry[] Entries(params IncidentArtifactEntry[] entries) => entries;

        private static IncidentArtifactEntry E(string id, bool frosted = false)
            => new IncidentArtifactEntry(id, frosted ? ArtifactTraits.Frosted : ArtifactTraits.None);

        private static LocalizedCopy Copy(string english, string korean) => new LocalizedCopy(english, korean);

        private readonly struct RuleSpec
        {
            public RuleSpec(ArtifactTraits traits, Destination destination)
            {
                Traits = traits;
                Destination = destination;
            }

            public ArtifactTraits Traits { get; }

            public Destination Destination { get; }
        }
    }
}
