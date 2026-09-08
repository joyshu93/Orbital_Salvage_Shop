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
                completesWhenAllStagesCompleted: true,
                awaitingContentClue: null,
                stages: new[] { FirstShift(), SecondShift(), ThirdShift(), FourthShift(), FifthShift() });
        }

        private static IncidentStageDefinition FirstShift()
        {
            return new IncidentStageDefinition(
                "rain-01-voices",
                new[]
                {
                    Beat("Since midnight, it has been raining inside the sealed umbrella. Do not open it.", "자정부터 봉인된 우산 안에서 비가 내리고 있어요. 절대 열지 마세요.", SeniorClerkMood.Concerned, IncidentVisualCue.Rain),
                    Beat("Every drop repeats someone's memory. One of them is saying my name.", "빗방울마다 누군가의 기억을 되풀이합니다. 그중 하나가 제 이름을 부르고 있어요.", SeniorClerkMood.Alert, IncidentVisualCue.Rain),
                    Beat("Seal what the rain has touched. Mend the fragile. Let the living rest. Read the ledger from the top.", "비에 젖은 것은 봉인하고, 깨지기 쉬운 것은 수리하세요. 살아 있는 것은 쉬게 하세요. 규칙은 위에서부터 적용합니다.", SeniorClerkMood.Neutral, IncidentVisualCue.InkSeal)
                },
                new[]
                {
                    Beat("The rain falls silent. One drop remains on the inside of the seal.", "비가 멎습니다. 봉인 안쪽에 빗방울 하나만 남았습니다.", SeniorClerkMood.Concerned, IncidentVisualCue.Rain),
                    Beat("It whispers, “You promised to come back.” The senior clerk does not answer.", "빗방울이 속삭입니다. “돌아오겠다고 약속했잖아.” 선임 관리인은 대답하지 않습니다.", SeniorClerkMood.Alert, IncidentVisualCue.Rain)
                },
                Reactions(
                    "The last drop shivers, but does not fall.", "마지막 빗방울이 떨리지만 떨어지지 않는다.",
                    "The rain gathers into one clear memory.", "비가 하나의 선명한 기억으로 모인다.",
                    "The umbrella closes by itself, as if it recognizes your hands.", "우산이 스스로 접힌다. 당신의 손길을 알아본 듯하다."),
                "moon-umbrella", "paper-fish",
                Queue("moon-umbrella", "paper-fish", "sleeping-teacup", "clockwork-moth", "backward-candle", "patient-compass", "rain-jar", "porcelain-tooth", "humming-scarf", "yesterday-ticket", "murmur-box", "ink-snowglobe"),
                Rules(
                    Rule("rain-01-wet-vault", ArtifactTraits.Wet, Destination.Vault),
                    Rule("rain-01-fragile-repair", ArtifactTraits.Fragile, Destination.Repair),
                    Rule("rain-01-alive-storage", ArtifactTraits.Alive, Destination.Storage),
                    Fallback("rain-01-fallback-storage", Destination.Storage)),
                2,
                new[]
                {
                    Docket(1, RainVoice("Not that name. The one before it.", "그 이름 말고. 그 전의 이름.")),
                    Docket(2, Senior("The rain is reciting intake labels we erased years ago.", "비가 오래전에 지운 접수표의 이름들을 읊고 있어요.", SeniorClerkMood.Concerned, IncidentVisualCue.Rain)),
                    Docket(3, RainVoice("You kept the key. Did you keep the promise?", "열쇠는 간직했구나. 약속도 간직했니?"))
                });
        }

        private static IncidentStageDefinition SecondShift()
        {
            return new IncidentStageDefinition(
                "rain-02-names-under-water",
                new[]
                {
                    Senior("The rain washed every current name from the ledger. Older names are surfacing underneath.", "비가 장부의 지금 이름을 모두 씻어 냈어요. 그 아래에서 오래된 이름들이 떠오릅니다.", SeniorClerkMood.Concerned, IncidentVisualCue.Rain),
                    Senior("Memories that point to another time go to the Vault first. Mend what is wet, let the living rest, then repair what is merely fragile.", "다른 시간을 가리키는 기억은 먼저 봉인고로 보내세요. 젖은 것은 수리하고, 살아 있는 것은 쉬게 한 뒤, 단지 깨지기 쉬운 것을 수리합니다.", SeniorClerkMood.Neutral, IncidentVisualCue.InkSeal)
                },
                new[]
                {
                    Senior("The date inside the jar matches my oldest surviving duty sheet.", "병 속 날짜가 남아 있는 제 가장 오래된 근무표와 일치해요.", SeniorClerkMood.Concerned, IncidentVisualCue.Rain),
                    RainVoice("I remember the shift you chose not to.", "네가 기억하지 않기로 한 근무를 나는 기억해.")
                },
                Reactions(
                    "The old names remain blurred, but they no longer wash away.", "오래된 이름은 흐릿하지만 더는 씻겨 나가지 않는다.",
                    "Tuesday settles into one legible line of the ledger.", "화요일이 장부의 읽을 수 있는 한 줄로 가라앉는다.",
                    "The held jar preserves a name the senior clerk cannot bring themself to read aloud.", "보호한 빗물병이 선임 관리인이 차마 소리 내어 읽지 못하는 이름 하나를 지킨다."),
                "rain-jar", "rain-jar",
                Queue("moon-umbrella", "mossy-watch", "rain-jar", "clockwork-moth", "paper-fish", "humming-scarf", "yesterday-ticket", "porcelain-tooth", "whispering-key", "ink-snowglobe", "patient-compass", "unmelting-ice"),
                Rules(
                    Rule("rain-02-temporal-vault", ArtifactTraits.Temporal, Destination.Vault),
                    Rule("rain-02-wet-repair", ArtifactTraits.Wet, Destination.Repair),
                    Rule("rain-02-alive-storage", ArtifactTraits.Alive, Destination.Storage),
                    Rule("rain-02-fragile-repair", ArtifactTraits.Fragile, Destination.Repair),
                    Fallback("rain-02-fallback-storage", Destination.Storage)),
                2,
                new[]
                {
                    Docket(1, RainVoice("Tuesday. You always took the Tuesday watch.", "화요일. 넌 늘 화요일 당번이었지.")),
                    Docket(2, Senior("That was before this coat, before anyone called me senior.", "이 외투를 입기 전, 아무도 저를 선임이라 부르기 전의 일이에요.", SeniorClerkMood.Concerned, IncidentVisualCue.Rain)),
                    Docket(3, RainVoice("Do not let the jar forget which Tuesday.", "그 병이 어느 화요일인지 잊게 두지 마."))
                });
        }

        private static IncidentStageDefinition ThirdShift()
        {
            return new IncidentStageDefinition(
                "rain-03-unsent-letter",
                new[]
                {
                    Senior("The paper fish unfolded during the day. It was folded from a letter that was never sent.", "낮 동안 종이 물고기가 펼쳐졌어요. 보내지 못한 편지로 접혀 있었습니다.", SeniorClerkMood.Concerned, IncidentVisualCue.Rain),
                    Senior("Let living words rest before you mend their paper. Vault anything cursed. We need the sentence intact.", "살아 있는 문장은 종이를 고치기 전에 쉬게 하세요. 저주받은 것은 봉인하고요. 문장을 온전히 남겨야 합니다.", SeniorClerkMood.Neutral, IncidentVisualCue.InkSeal)
                },
                new[]
                {
                    Senior("The letter was addressed to the repository itself, before I became senior.", "이 편지는 제가 선임이 되기 전부터 보관소 자체에 보내진 것이었어요.", SeniorClerkMood.Alert, IncidentVisualCue.Rain),
                    Senior("It was not unsent. The recipient never knew how to answer.", "보내지 못한 게 아니었어요. 받는 곳이 답하는 법을 몰랐던 겁니다.", SeniorClerkMood.Concerned, IncidentVisualCue.Rain)
                },
                Reactions(
                    "The letter closes without losing another word.", "편지가 단어를 더 잃지 않고 접힌다.",
                    "Every crease returns to its original sentence.", "모든 접힌 자국이 원래 문장으로 돌아간다.",
                    "The protected paper fish swims once around the words ‘Night Repository.’", "보호한 종이 물고기가 ‘야간 보관소’라는 글자 둘레를 한 바퀴 헤엄친다."),
                "paper-fish", "paper-fish",
                Queue("rain-jar", "humming-scarf", "paper-fish", "moon-umbrella", "whispering-key", "porcelain-tooth", "clockwork-moth", "mirror-seed", "borrowed-shadow", "yesterday-ticket", "silent-bell", "patient-compass"),
                Rules(
                    Rule("rain-03-alive-storage", ArtifactTraits.Alive, Destination.Storage),
                    Rule("rain-03-fragile-repair", ArtifactTraits.Fragile, Destination.Repair),
                    Rule("rain-03-cursed-vault", ArtifactTraits.Cursed, Destination.Vault),
                    Fallback("rain-03-fallback-vault", Destination.Vault)),
                2,
                new[]
                {
                    Docket(1, RainVoice("To the place that keeps what has nowhere else to go...", "달리 갈 곳 없는 것을 지키는 곳에게...")),
                    Docket(2, Senior("There is no person’s name after ‘To.’ Keep the folds in order.", "‘받는 이’ 뒤에 사람 이름이 없어요. 접힌 순서를 지켜 주세요.", SeniorClerkMood.Concerned, IncidentVisualCue.Rain)),
                    Docket(3, RainVoice("Night Repository. If this reaches you, you have begun to forget.", "야간 보관소. 이 편지가 닿았다면, 너는 잊기 시작한 거야."))
                });
        }

        private static IncidentStageDefinition FourthShift()
        {
            return new IncidentStageDefinition(
                "rain-04-dry-order",
                new[]
                {
                    Senior("A perfectly dry order fell from the wet umbrella. It demands a retrieval review of this repository.", "젖은 우산에서 완전히 마른 명령서가 떨어졌어요. 이 보관소의 회수 심사를 요구하고 있습니다.", SeniorClerkMood.Alert, IncidentVisualCue.Rain),
                    Senior("Contain cursed evidence first. Store what is rain-touched, mend what is fragile, and let the living rest. Do not spend the umbrella’s last voice too early.", "저주받은 증거를 먼저 봉인하세요. 비에 닿은 것은 보관하고, 깨지기 쉬운 것은 수리하고, 살아 있는 것은 쉬게 하세요. 우산의 마지막 목소리를 너무 일찍 쓰면 안 됩니다.", SeniorClerkMood.Neutral, IncidentVisualCue.InkSeal)
                },
                new[]
                {
                    Senior("This order was waiting here. No courier, no outside office, no sender.", "이 명령서는 여기서 기다리고 있었어요. 배달원도, 외부 기관도, 발신자도 없이.", SeniorClerkMood.Alert, IncidentVisualCue.Rain),
                    RainVoice("Then ask who taught the house to sign.", "그렇다면 누가 이 집에 서명하는 법을 가르쳤는지 물어.")
                },
                Reactions(
                    "The dry order remains sealed and readable.", "마른 명령서가 봉인된 채 읽을 수 있게 남는다.",
                    "The blank sender line shines more clearly than the printed order.", "빈 발신란이 인쇄된 명령보다 더 선명하게 빛난다.",
                    "The protected umbrella places one warm drop beside the empty signature.", "보호한 우산이 빈 서명 옆에 따뜻한 빗방울 하나를 놓는다."),
                "moon-umbrella", "moon-umbrella",
                Queue("sleeping-teacup", "rain-jar", "moon-umbrella", "whispering-key", "backward-candle", "clockwork-moth", "silent-bell", "rusty-comet", "ink-snowglobe", "lantern-snail", "tide-locket", "murmur-box"),
                Rules(
                    Rule("rain-04-cursed-vault", ArtifactTraits.Cursed, Destination.Vault),
                    Rule("rain-04-wet-storage", ArtifactTraits.Wet, Destination.Storage),
                    Rule("rain-04-fragile-repair", ArtifactTraits.Fragile, Destination.Repair),
                    Rule("rain-04-alive-storage", ArtifactTraits.Alive, Destination.Storage),
                    Fallback("rain-04-fallback-repair", Destination.Repair)),
                2,
                new[]
                {
                    Docket(1, Senior("The form is official. The sender line is empty.", "양식은 공식 문서가 맞아요. 발신란만 비어 있습니다.", SeniorClerkMood.Alert, IncidentVisualCue.Rain)),
                    Docket(2, RainVoice("It was not delivered from outside.", "그건 밖에서 배달된 게 아니야.")),
                    Docket(3, Senior("The seal is dry because it was stamped inside the umbrella.", "인장이 마른 건 우산 안쪽에서 찍혔기 때문이에요.", SeniorClerkMood.Concerned, IncidentVisualCue.Rain))
                });
        }

        private static IncidentStageDefinition FifthShift()
        {
            return new IncidentStageDefinition(
                "rain-05-testimony",
                new[]
                {
                    Senior("The letter fragments and the rain are replaying one final night. This time, we listen in order.", "편지 조각과 비가 마지막 밤 하나를 되풀이하고 있어요. 이번에는 순서대로 듣겠습니다.", SeniorClerkMood.Alert, IncidentVisualCue.Rain),
                    Senior("Time first, then water, then the living voice. Protect the paper fish until its sentence has somewhere safe to land.", "시간을 먼저, 그다음 물을, 그다음 살아 있는 목소리를 다루세요. 종이 물고기의 문장이 안전하게 닿을 곳이 생길 때까지 보호합니다.", SeniorClerkMood.Neutral, IncidentVisualCue.InkSeal)
                },
                new[]
                {
                    RainVoice("I was the clerk before your senior. I left the truth for whoever came next.", "나는 네 선임보다 먼저 일한 관리인이야. 다음에 올 사람을 위해 진실을 남겼어."),
                    Senior("Then the order was meant to be found here—not obeyed in silence.", "그렇다면 이 명령서는 여기서 발견되기 위한 것이었어요. 아무 말 없이 따르기 위한 게 아니라.", SeniorClerkMood.Alert, IncidentVisualCue.Rain),
                    Senior("Tomorrow, we trace the time printed beneath the missing signature.", "내일은 사라진 서명 아래에 찍힌 시간을 추적하겠습니다.", SeniorClerkMood.Relieved, IncidentVisualCue.AmberWarmth)
                },
                Reactions(
                    "The testimony survives, incomplete but no longer scattered.", "증언이 불완전하지만 더는 흩어지지 않은 채 남는다.",
                    "The final shift plays in a clear, unbroken order.", "마지막 근무가 끊김 없이 선명한 순서로 재생된다.",
                    "The paper fish rests on the desk coordinates as though the letter has finally arrived.", "종이 물고기가 작업대 좌표 위에 내려앉는다. 편지가 마침내 도착한 듯하다."),
                "paper-fish", "paper-fish",
                Queue("moon-umbrella", "paper-fish", "clockwork-moth", "rain-jar", "humming-scarf", "patient-compass", "whispering-key", "thimble-storm", "ink-snowglobe", "borrowed-shadow", "murmur-box", "unmelting-ice"),
                Rules(
                    Rule("rain-05-temporal-vault", ArtifactTraits.Temporal, Destination.Vault),
                    Rule("rain-05-wet-repair", ArtifactTraits.Wet, Destination.Repair),
                    Rule("rain-05-alive-storage", ArtifactTraits.Alive, Destination.Storage),
                    Rule("rain-05-cursed-vault", ArtifactTraits.Cursed, Destination.Vault),
                    Rule("rain-05-fragile-repair", ArtifactTraits.Fragile, Destination.Repair),
                    Fallback("rain-05-fallback-storage", Destination.Storage)),
                3,
                new[]
                {
                    Docket(1, RainVoice("If the rooms forget their names, the ledger will mistake the whole house for lost property.", "방들이 자기 이름을 잊으면, 장부는 이 집 전체를 분실물로 오인할 거야.")),
                    Docket(2, RainVoice("Leave the order where the next careful hand can find it.", "다음의 조심스러운 손이 찾을 수 있는 곳에 명령서를 남겨.")),
                    Docket(3, Senior("Those coordinates are not a street. They are this desk.", "이 좌표는 거리가 아니에요. 바로 이 작업대입니다.", SeniorClerkMood.Alert, IncidentVisualCue.Rain))
                });
        }

        private static NarrativeBeat Beat(string english, string korean, SeniorClerkMood mood, IncidentVisualCue cue)
            => new NarrativeBeat(Copy(english, korean), mood, cue);

        private static NarrativeBeat Senior(string english, string korean, SeniorClerkMood mood, IncidentVisualCue cue)
            => new NarrativeBeat(Copy("Senior Clerk", "선임 관리인"), Copy(english, korean), mood, cue);

        private static NarrativeBeat RainVoice(string english, string korean, SeniorClerkMood mood = SeniorClerkMood.Alert)
            => new NarrativeBeat(Copy("Voice in the Rain", "빗속의 목소리"), Copy(english, korean), mood, IncidentVisualCue.Rain);

        private static IncidentDocketBeat Docket(int number, NarrativeBeat beat)
            => new IncidentDocketBeat(number, beat);

        private static ArtifactReaction Reactions(string stableEnglish, string stableKorean, string preciseEnglish, string preciseKorean, string resonantEnglish, string resonantKorean)
            => new ArtifactReaction(Copy(stableEnglish, stableKorean), Copy(preciseEnglish, preciseKorean), Copy(resonantEnglish, resonantKorean));

        private static IncidentArtifactEntry[] Queue(params string[] artifactIds)
        {
            var queue = new IncidentArtifactEntry[artifactIds.Length];
            for (var index = 0; index < artifactIds.Length; index++)
            {
                queue[index] = new IncidentArtifactEntry(artifactIds[index], ArtifactTraits.None);
            }

            return queue;
        }

        private static SortingRule[] Rules(params SortingRule[] rules) => rules;

        private static SortingRule Rule(string id, ArtifactTraits trait, Destination destination)
            => new SortingRule(id, trait, ArtifactTraits.None, destination, false);

        private static SortingRule Fallback(string id, Destination destination)
            => new SortingRule(id, ArtifactTraits.None, ArtifactTraits.None, destination, true);

        private static LocalizedCopy Copy(string english, string korean) => new LocalizedCopy(english, korean);
    }
}
