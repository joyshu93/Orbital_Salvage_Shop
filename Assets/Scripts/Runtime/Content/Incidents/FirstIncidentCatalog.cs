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
                new[]
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
                "The frost has chosen company. Treat every white-rimmed curio as one condition.",
                "서리가 동료를 골랐군요. 흰 테가 생긴 물건은 모두 같은 상태로 보세요.",
                SeniorClerkMood.Concerned,
                IncidentVisualCue.Frost,
                "The leaf is gone. No water escaped.",
                "낙엽이 사라졌어요. 물은 한 방울도 새지 않았는데요.",
                SeniorClerkMood.Alert,
                IncidentVisualCue.Frost,
                "unmelting-ice",
                null,
                2,
                Reactions(
                    "After the correction, the wandering frost draws back from the other curios.",
                    "바로잡자 떠돌던 서리가 다른 물건들에서 물러납니다.",
                    "Your calm care keeps every white rim thin and still.",
                    "침착하게 돌보자 모든 흰 테가 얇고 고요하게 머뭅니다.",
                    "Frost traces a quiet circle around your ledger, then bows away.",
                    "서리가 장부 둘레에 고요한 원을 그리고 물러서며 답합니다."),
                Rules(
                    R(ArtifactTraits.Frosted, Destination.Storage),
                    R(ArtifactTraits.Cursed, Destination.Vault),
                    R(ArtifactTraits.Fragile, Destination.Repair)),
                Entries(
                    E("whispering-key"), E("silent-bell"), E("sleeping-teacup"), E("unmelting-ice", true),
                    E("backward-candle"), E("moon-umbrella", true), E("humming-scarf"), E("clockwork-moth", true),
                    E("patient-compass", true), E("lantern-snail"), E("murmur-box"), E("yesterday-ticket")));
        }

        private static IncidentStageDefinition StageThree()
        {
            return Stage(
                "ice-03-tomorrow",
                "This watch carries the same leaf—and tomorrow’s date. Time takes priority over frost.",
                "이 시계에도 같은 낙엽이 있어요. 날짜는 내일이고요. 시간 이상이 서리보다 우선입니다.",
                SeniorClerkMood.Alert,
                IncidentVisualCue.InkSeal,
                "Tomorrow is pointing back at this desk.",
                "내일이 이 책상을 가리키고 있습니다.",
                SeniorClerkMood.Concerned,
                IncidentVisualCue.InkSeal,
                "mossy-watch",
                null,
                3,
                Reactions(
                    "The corrected order draws the loose minute back into the watch.",
                    "바로잡은 순서가 풀려난 1분을 시계 안으로 되돌립니다.",
                    "Your calm care lets the watch keep one honest present.",
                    "침착한 손길 덕분에 시계가 정직한 현재를 지킵니다.",
                    "The watch answers with tomorrow’s rhythm once, then matches your pulse.",
                    "시계가 내일의 박자를 한 번 울리고 당신의 맥박에 맞춰 답합니다."),
                Rules(
                    R(ArtifactTraits.Temporal, Destination.Vault),
                    R(ArtifactTraits.Frosted, Destination.Storage),
                    R(ArtifactTraits.Fragile, Destination.Repair)),
                Entries(
                    E("moon-umbrella"), E("sleeping-teacup"), E("clockwork-moth", true), E("unmelting-ice", true),
                    E("patient-compass", true), E("thimble-storm", true), E("mossy-watch"), E("porcelain-tooth"),
                    E("lantern-snail"), E("rain-jar"), E("tide-locket", true), E("rusty-comet")));
        }

        private static IncidentStageDefinition StageFour()
        {
            return Stage(
                "ice-04-frozen-seal",
                "The Vault seal is frozen. Protect the next Vault curio in Hold and open Repair first.",
                "봉인고 인장이 얼었습니다. 다음 봉인 물건은 보류에서 보호하고 수리실 순서를 먼저 여세요.",
                SeniorClerkMood.Alert,
                IncidentVisualCue.InkSeal,
                "The held curios are trembling in the same rhythm.",
                "보류했던 물건들이 같은 박자로 떨고 있어요.",
                SeniorClerkMood.Concerned,
                IncidentVisualCue.Frost,
                "mossy-watch",
                "mossy-watch",
                2,
                Reactions(
                    "The corrected order releases the frozen seal without harming the held curios.",
                    "바로잡은 순서가 보류 물건을 다치게 하지 않고 얼어붙은 인장을 풉니다.",
                    "Your calm care opens Repair before the cold can tighten.",
                    "침착한 손길이 추위가 조이기 전에 수리실 순서를 엽니다.",
                    "From Hold, the watch answers; every sealed curio trembles in time.",
                    "보류된 시계가 답하자 봉인된 물건들이 같은 박자로 떨립니다."),
                Rules(
                    R(ArtifactTraits.Temporal, Destination.Vault),
                    R(ArtifactTraits.Frosted, Destination.Storage),
                    R(ArtifactTraits.Fragile, Destination.Repair)),
                Entries(
                    E("unmelting-ice", true), E("mossy-watch"), E("moon-umbrella"), E("clockwork-moth", true),
                    E("sleeping-teacup"), E("patient-compass", true), E("rain-jar"), E("thimble-storm", true),
                    E("tide-locket", true), E("porcelain-tooth"), E("rusty-comet"), E("lantern-snail")));
        }

        private static IncidentStageDefinition StageFive()
        {
            return Stage(
                "ice-05-thaw",
                "No new rule tonight. Read the priority, protect the order, and let the ice answer.",
                "오늘 새 규칙은 없습니다. 우선순위를 읽고, 순서를 보호하고, 얼음이 답하게 하세요.",
                SeniorClerkMood.Neutral,
                IncidentVisualCue.Frost,
                "The ice melts without water. Rain begins inside the sealed umbrella parcel.",
                "얼음은 물 없이 녹았습니다. 봉인된 우산 소포 안에서 빗소리가 납니다.",
                SeniorClerkMood.Relieved,
                IncidentVisualCue.Rain,
                "moon-umbrella",
                "moon-umbrella",
                3,
                Reactions(
                    "The corrected order lets the last frost recede. The umbrella stays safely sealed.",
                    "바로잡은 순서에 마지막 서리가 물러납니다. 우산은 무사히 봉인됩니다.",
                    "Your calm care leaves no water on the desk and no strain in the seals.",
                    "침착한 손길 뒤 책상에는 물도, 인장의 긴장도 남지 않습니다.",
                    "Rain answers from inside the sealed umbrella while the office turns warm.",
                    "봉인된 우산 안에서 비가 답하고 보관소가 따뜻해집니다."),
                Rules(
                    R(ArtifactTraits.Temporal, Destination.Vault),
                    R(ArtifactTraits.Frosted, Destination.Storage),
                    R(ArtifactTraits.Fragile, Destination.Repair)),
                Entries(
                    E("paper-fish"), E("moon-umbrella"), E("clockwork-moth", true), E("unmelting-ice", true),
                    E("patient-compass", true), E("thimble-storm", true), E("mossy-watch"), E("mirror-seed"),
                    E("ink-snowglobe"), E("rain-jar"), E("tide-locket", true), E("rusty-comet")));
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
