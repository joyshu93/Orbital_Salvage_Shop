using System;
using System.Collections.Generic;
using System.Linq;
using CurioClerk.Content;
using CurioClerk.Content.Incidents;
using CurioClerk.Core.Artifacts;
using CurioClerk.Core.Incidents;
using CurioClerk.Core.Rules;
using CurioClerk.Core.Shifts;
using NUnit.Framework;

namespace CurioClerk.Tests.EditMode
{
    public sealed class IncidentContentContractTests
    {
        private static readonly StageExpectation[] ExpectedStages =
        {
            new StageExpectation(
                "ice-01-crack",
                new[]
                {
                    "unmelting-ice", "moon-umbrella", "clockwork-moth", "mossy-watch",
                    "sleeping-teacup", "patient-compass", "rain-jar", "porcelain-tooth",
                    "thimble-storm", "rusty-comet", "tide-locket", "borrowed-shadow"
                },
                "RRSVRSVRSVSV",
                1,
                Array.Empty<string>(),
                "First night? Remember this: nothing left behind here is truly silent.",
                "첫날이죠? 이것만 기억하세요. 이곳에 남겨진 물건은 결코 침묵하지 않습니다.",
                "The crack is sealed. The leaf inside moved anyway.",
                "금은 봉합됐어요. 그런데 안쪽의 낙엽은 움직였습니다."),
            new StageExpectation(
                "ice-02-spread",
                new[]
                {
                    "whispering-key", "silent-bell", "sleeping-teacup", "unmelting-ice",
                    "backward-candle", "moon-umbrella", "humming-scarf", "clockwork-moth",
                    "patient-compass", "lantern-snail", "murmur-box", "yesterday-ticket"
                },
                "VVRSRSVSSRVR",
                2,
                new[] { "unmelting-ice", "moon-umbrella", "clockwork-moth", "patient-compass" },
                "The frost has chosen company. Treat every white-rimmed curio as one condition.",
                "서리가 동료를 골랐군요. 흰 테가 생긴 물건은 모두 같은 상태로 보세요.",
                "The leaf is gone. No water escaped.",
                "낙엽이 사라졌어요. 물은 한 방울도 새지 않았는데요."),
            new StageExpectation(
                "ice-03-tomorrow",
                new[]
                {
                    "moon-umbrella", "sleeping-teacup", "clockwork-moth", "unmelting-ice",
                    "patient-compass", "thimble-storm", "mossy-watch", "porcelain-tooth",
                    "lantern-snail", "rain-jar", "tide-locket", "rusty-comet"
                },
                "RRSVSSVRRVSV",
                3,
                new[] { "clockwork-moth", "unmelting-ice", "patient-compass", "thimble-storm", "tide-locket" },
                "This watch carries the same leaf—and tomorrow’s date. Time takes priority over frost.",
                "이 시계에도 같은 낙엽이 있어요. 날짜는 내일이고요. 시간 이상이 서리보다 우선입니다.",
                "Tomorrow is pointing back at this desk.",
                "내일이 이 책상을 가리키고 있습니다."),
            new StageExpectation(
                "ice-04-frozen-seal",
                new[]
                {
                    "unmelting-ice", "mossy-watch", "moon-umbrella", "clockwork-moth",
                    "sleeping-teacup", "patient-compass", "rain-jar", "thimble-storm",
                    "tide-locket", "porcelain-tooth", "rusty-comet", "lantern-snail"
                },
                "VVRSRSVSSRVR",
                2,
                new[] { "unmelting-ice", "clockwork-moth", "patient-compass", "thimble-storm", "tide-locket" },
                "The Vault seal is frozen. Protect the next Vault curio in Hold and open Repair first.",
                "봉인고 인장이 얼었습니다. 다음 봉인 물건은 보류에서 보호하고 수리실 순서를 먼저 여세요.",
                "The held curios are trembling in the same rhythm.",
                "보류했던 물건들이 같은 박자로 떨고 있어요."),
            new StageExpectation(
                "ice-05-thaw",
                new[]
                {
                    "paper-fish", "moon-umbrella", "clockwork-moth", "unmelting-ice",
                    "patient-compass", "thimble-storm", "mossy-watch", "mirror-seed",
                    "ink-snowglobe", "rain-jar", "tide-locket", "rusty-comet"
                },
                "RRSVSSVRRVSV",
                3,
                new[] { "clockwork-moth", "unmelting-ice", "patient-compass", "thimble-storm", "tide-locket" },
                "No new rule tonight. Read the priority, protect the order, and let the ice answer.",
                "오늘 새 규칙은 없습니다. 우선순위를 읽고, 순서를 보호하고, 얼음이 답하게 하세요.",
                "The ice melts without water. Rain begins inside the sealed umbrella parcel.",
                "얼음은 물 없이 녹았습니다. 봉인된 우산 소포 안에서 빗소리가 납니다.")
        };

        [Test]
        public void FirstIncident_HasTheExactAuthoredQueuePatternAndHoldMatrix()
        {
            var incident = ContentCatalog.CreateIncidents().Single();
            var artifacts = ContentCatalog.CreateArtifacts().ToDictionary(item => item.Id, StringComparer.Ordinal);
            var ruleEngine = new RuleEngine();
            var analyzer = new DocketSequenceAnalyzer();

            Assert.That(incident.Id, Is.EqualTo("unmelting-ice"));
            Assert.That(incident.Stages.Select(stage => stage.Id),
                Is.EqualTo(ExpectedStages.Select(stage => stage.Id)));

            for (var index = 0; index < ExpectedStages.Length; index++)
            {
                var expected = ExpectedStages[index];
                var stage = incident.Stages[index];
                Assert.That(stage.Queue, Has.Count.EqualTo(12), stage.Id);
                Assert.That(stage.Queue.Select(entry => entry.ArtifactId), Is.EqualTo(expected.QueueIds), stage.Id);
                Assert.That(stage.Queue.Select(entry => entry.ArtifactId).Distinct(StringComparer.Ordinal).Count(),
                    Is.EqualTo(12), stage.Id);
                Assert.That(stage.Queue.Where(entry => entry.AddedTraits == ArtifactTraits.Frosted)
                    .Select(entry => entry.ArtifactId), Is.EqualTo(expected.FrostedIds), stage.Id);
                Assert.That(stage.Queue.All(entry =>
                    entry.AddedTraits == ArtifactTraits.None || entry.AddedTraits == ArtifactTraits.Frosted),
                    Is.True, stage.Id);

                AssertRuleOrder(stage);
                var plan = stage.CreateShiftPlan(artifacts);
                var destinations = plan.Queue.Select(item => ruleEngine.Resolve(item, plan.Rules)).ToArray();
                Assert.That(Pattern(destinations), Is.EqualTo(expected.Pattern), stage.Id);
                Assert.That(destinations.Count(value => value == Destination.Repair), Is.EqualTo(4), stage.Id);
                Assert.That(destinations.Count(value => value == Destination.Storage), Is.EqualTo(4), stage.Id);
                Assert.That(destinations.Count(value => value == Destination.Vault), Is.EqualTo(4), stage.Id);
                Assert.That(stage.MinimumRequiredHolds, Is.EqualTo(expected.MinimumHolds), stage.Id);
                Assert.That(analyzer.MinimumHolds(destinations), Is.EqualTo(expected.MinimumHolds), stage.Id);
            }
        }

        [Test]
        public void FirstIncident_HasExactBilingualOpeningsClosingsAndBoundedQualityReactions()
        {
            var incident = ContentCatalog.CreateIncidents().Single();
            AssertBilingual(incident.Title, "incident title");

            for (var index = 1; index < ExpectedStages.Length; index++)
            {
                var expected = ExpectedStages[index];
                var stage = incident.Stages[index];
                Assert.That(stage.IntroBeats, Has.Count.EqualTo(1), stage.Id);
                Assert.That(stage.OutroBeats, Has.Count.EqualTo(1), stage.Id);
                Assert.That(stage.IntroBeats[0].Copy.English, Is.EqualTo(expected.IntroEnglish), stage.Id);
                Assert.That(stage.IntroBeats[0].Copy.Korean, Is.EqualTo(expected.IntroKorean), stage.Id);
                Assert.That(stage.OutroBeats[0].Copy.English, Is.EqualTo(expected.OutroEnglish), stage.Id);
                Assert.That(stage.OutroBeats[0].Copy.Korean, Is.EqualTo(expected.OutroKorean), stage.Id);

                AssertReaction(stage.Reactions.Stable, stage.Id + " Stable");
                AssertReaction(stage.Reactions.Precise, stage.Id + " Precise");
                AssertReaction(stage.Reactions.Resonant, stage.Id + " Resonant");
                Assert.That(stage.Reactions.Stable.English, Does.Contain("correct").IgnoreCase, stage.Id);
                Assert.That(stage.Reactions.Stable.Korean, Does.Contain("바로잡"), stage.Id);
                Assert.That(stage.Reactions.Precise.English, Does.Contain("calm").IgnoreCase, stage.Id);
                Assert.That(stage.Reactions.Precise.Korean, Does.Contain("침착"), stage.Id);
                Assert.That(stage.Reactions.Resonant.English,
                    Is.Not.EqualTo(stage.Reactions.Stable.English).And.Not.EqualTo(stage.Reactions.Precise.English),
                    stage.Id);
                Assert.That(stage.Reactions.Resonant.Korean,
                    Is.Not.EqualTo(stage.Reactions.Stable.Korean).And.Not.EqualTo(stage.Reactions.Precise.Korean),
                    stage.Id);
                Assert.That(stage.Reactions.ForQuality(IncidentQuality.Stable), Is.SameAs(stage.Reactions.Stable));
                Assert.That(stage.Reactions.ForQuality(IncidentQuality.Precise), Is.SameAs(stage.Reactions.Precise));
                Assert.That(stage.Reactions.ForQuality(IncidentQuality.Resonant), Is.SameAs(stage.Reactions.Resonant));
            }
        }

        [Test]
        public void FirstIncident_FirstShiftExplainsTheFantasyTheThreatAndTheClerksJobBeforeLeavingAStoryHook()
        {
            var stage = ContentCatalog.CreateIncidents().Single().Stages[0];

            Assert.That(stage.IntroBeats, Has.Count.EqualTo(3));
            Assert.That(stage.IntroBeats.Select(beat => beat.Copy.English), Is.EqualTo(new[]
            {
                "First night? Remember this: nothing left behind here is truly silent.",
                "This ice refuses to melt. Sort tonight's curios before the frost reaches the shelves.",
                "Each docket needs one seal from each desk. If that desk is already sealed, protect the curio in Hold."
            }));
            Assert.That(stage.IntroBeats.Select(beat => beat.Copy.Korean), Is.EqualTo(new[]
            {
                "첫날이죠? 이것만 기억하세요. 이곳에 남겨진 물건은 결코 침묵하지 않습니다.",
                "이 얼음은 녹기를 거부합니다. 서리가 선반에 닿기 전에 오늘 밤 물건들을 분류하세요.",
                "장부마다 세 책상의 인장을 하나씩 채웁니다. 이미 찍힌 곳의 물건은 보류에서 지키세요."
            }));
            Assert.That(stage.IntroBeats.Select(beat => beat.Mood), Is.EqualTo(new[]
            {
                SeniorClerkMood.Neutral,
                SeniorClerkMood.Concerned,
                SeniorClerkMood.Alert
            }));
            Assert.That(stage.IntroBeats.Select(beat => beat.VisualCue), Is.EqualTo(new[]
            {
                IncidentVisualCue.AmberWarmth,
                IncidentVisualCue.Frost,
                IncidentVisualCue.InkSeal
            }));

            Assert.That(stage.OutroBeats, Has.Count.EqualTo(2));
            Assert.That(stage.OutroBeats.Select(beat => beat.Copy.English), Is.EqualTo(new[]
            {
                "The crack is sealed. The leaf inside moved anyway.",
                "You did more than sort it. The ice answered you. Tomorrow night, follow what the frost chooses."
            }));
            Assert.That(stage.OutroBeats.Select(beat => beat.Copy.Korean), Is.EqualTo(new[]
            {
                "금은 봉합됐어요. 그런데 안쪽의 낙엽은 움직였습니다.",
                "분류만 한 게 아니에요. 얼음이 당신에게 답했습니다. 다음 밤엔 서리가 고른 것을 따라가세요."
            }));
            Assert.That(stage.OutroBeats.Select(beat => beat.Mood), Is.EqualTo(new[]
            {
                SeniorClerkMood.Concerned,
                SeniorClerkMood.Alert
            }));
            Assert.That(stage.OutroBeats.Select(beat => beat.VisualCue), Is.EqualTo(new[]
            {
                IncidentVisualCue.Frost,
                IncidentVisualCue.InkSeal
            }));

            AssertReaction(stage.Reactions.Stable, stage.Id + " Stable");
            AssertReaction(stage.Reactions.Precise, stage.Id + " Precise");
            AssertReaction(stage.Reactions.Resonant, stage.Id + " Resonant");
            Assert.That(stage.Reactions.Stable.English, Does.Contain("correct").IgnoreCase);
            Assert.That(stage.Reactions.Stable.Korean, Does.Contain("바로잡"));
            Assert.That(stage.Reactions.Precise.English, Does.Contain("calm").IgnoreCase);
            Assert.That(stage.Reactions.Precise.Korean, Does.Contain("침착"));
        }

        [Test]
        public void FirstIncident_UsesValidLeadAndResonanceArtifactIds()
        {
            var artifactIds = ContentCatalog.CreateArtifacts()
                .Select(item => item.Id)
                .ToHashSet(StringComparer.Ordinal);

            foreach (var stage in ContentCatalog.CreateIncidents().Single().Stages)
            {
                var queueIds = stage.Queue.Select(entry => entry.ArtifactId).ToHashSet(StringComparer.Ordinal);
                Assert.That(artifactIds.Contains(stage.LeadArtifactId), Is.True, stage.Id);
                Assert.That(queueIds.Contains(stage.LeadArtifactId), Is.True, stage.Id);
                if (!string.IsNullOrEmpty(stage.ResonanceHoldArtifactId))
                {
                    Assert.That(artifactIds.Contains(stage.ResonanceHoldArtifactId), Is.True, stage.Id);
                    Assert.That(queueIds.Contains(stage.ResonanceHoldArtifactId), Is.True, stage.Id);
                }
            }

            Assert.That(ContentCatalog.CreateIncidents().Single().Stages.Select(stage => stage.ResonanceHoldArtifactId),
                Is.EqualTo(new[] { null, null, null, "mossy-watch", "moon-umbrella" }));
        }

        [Test]
        public void CreateShiftPlan_AddsStageTraitsWithoutMutatingBaseArtifacts()
        {
            var artifacts = ContentCatalog.CreateArtifacts().ToDictionary(item => item.Id, StringComparer.Ordinal);
            var stage = ContentCatalog.CreateIncidents().Single().Stages[1];

            var plan = stage.CreateShiftPlan(artifacts);

            Assert.That(artifacts["unmelting-ice"].Traits & ArtifactTraits.Frosted, Is.EqualTo(ArtifactTraits.None));
            Assert.That(plan.Queue.Single(item => item.Id == "unmelting-ice").Traits & ArtifactTraits.Frosted,
                Is.EqualTo(ArtifactTraits.Frosted));
        }

        [Test]
        public void CreateShiftPlan_RejectsMissingArtifactIds()
        {
            var stage = MinimalStage(QueueIds().Select(id =>
                new IncidentArtifactEntry(id == "unmelting-ice" ? "missing-artifact" : id, ArtifactTraits.None)));

            Assert.Throws<KeyNotFoundException>(() => stage.CreateShiftPlan(ArtifactDictionary()));
        }

        [Test]
        public void CreateShiftPlan_RejectsDuplicateQueueIds()
        {
            var entries = QueueIds().Select(id => new IncidentArtifactEntry(id, ArtifactTraits.None)).ToArray();
            entries[11] = new IncidentArtifactEntry(entries[0].ArtifactId, ArtifactTraits.None);
            var stage = MinimalStage(entries);

            Assert.Throws<InvalidOperationException>(() => stage.CreateShiftPlan(ArtifactDictionary()));
        }

        [Test]
        public void CreateShiftPlan_RejectsEmptyRules()
        {
            var stage = MinimalStage(
                QueueIds().Select(id => new IncidentArtifactEntry(id, ArtifactTraits.None)),
                Array.Empty<SortingRule>());

            Assert.Throws<InvalidOperationException>(() => stage.CreateShiftPlan(ArtifactDictionary()));
        }

        [Test]
        public void CreateShiftPlan_RejectsQueueCountsOtherThanTwelve()
        {
            var stage = MinimalStage(QueueIds().Take(11)
                .Select(id => new IncidentArtifactEntry(id, ArtifactTraits.None)));

            Assert.Throws<InvalidOperationException>(() => stage.CreateShiftPlan(ArtifactDictionary()));
        }

        [Test]
        public void IncidentContent_CopiesAuthoredCollectionsAtConstructionBoundaries()
        {
            var queue = QueueIds().Select(id => new IncidentArtifactEntry(id, ArtifactTraits.None)).ToArray();
            var rules = DefaultRules();
            var intros = new[] { Beat("intro", "도입") };
            var outros = new[] { Beat("outro", "마무리") };
            var stage = new IncidentStageDefinition(
                "copy-stage", intros, outros, Reactions(), "unmelting-ice", null,
                queue, rules, 1);
            var stages = new[] { stage };
            var incident = new IncidentDefinition("copy-incident", new LocalizedCopy("Title", "제목"), stages);

            queue[0] = new IncidentArtifactEntry("changed", ArtifactTraits.Frosted);
            rules[0] = rules[1];
            intros[0] = Beat("changed", "변경");
            outros[0] = Beat("changed", "변경");
            stages[0] = null;

            Assert.That(stage.Queue[0].ArtifactId, Is.EqualTo("unmelting-ice"));
            Assert.That(stage.Rules[0].RequiredAll, Is.EqualTo(ArtifactTraits.Fragile));
            Assert.That(stage.IntroBeats[0].Copy.English, Is.EqualTo("intro"));
            Assert.That(stage.OutroBeats[0].Copy.English, Is.EqualTo("outro"));
            Assert.That(incident.Stages[0], Is.SameAs(stage));
        }

        private static void AssertRuleOrder(IncidentStageDefinition stage)
        {
            var expectedTraits = stage.Id == "ice-01-crack"
                ? new[] { ArtifactTraits.Fragile, ArtifactTraits.Temporal, ArtifactTraits.None }
                : stage.Id == "ice-02-spread"
                    ? new[] { ArtifactTraits.Frosted, ArtifactTraits.Cursed, ArtifactTraits.Fragile, ArtifactTraits.None }
                    : new[] { ArtifactTraits.Temporal, ArtifactTraits.Frosted, ArtifactTraits.Fragile, ArtifactTraits.None };
            var expectedDestinations = stage.Id == "ice-01-crack"
                ? new[] { Destination.Repair, Destination.Vault, Destination.Storage }
                : stage.Id == "ice-02-spread"
                    ? new[] { Destination.Storage, Destination.Vault, Destination.Repair, Destination.Storage }
                    : new[] { Destination.Vault, Destination.Storage, Destination.Repair, Destination.Storage };

            Assert.That(stage.Rules.Select(rule => rule.RequiredAll), Is.EqualTo(expectedTraits), stage.Id);
            Assert.That(stage.Rules.Select(rule => rule.RequiredAny),
                Is.EqualTo(Enumerable.Repeat(ArtifactTraits.None, expectedTraits.Length)), stage.Id);
            Assert.That(stage.Rules.Select(rule => rule.Destination), Is.EqualTo(expectedDestinations), stage.Id);
            Assert.That(stage.Rules.Take(stage.Rules.Count - 1).All(rule => !rule.IsFallback), Is.True, stage.Id);
            Assert.That(stage.Rules[stage.Rules.Count - 1].IsFallback, Is.True, stage.Id);
        }

        private static void AssertReaction(LocalizedCopy copy, string label)
        {
            AssertBilingual(copy, label);
            Assert.That(copy.English.Length, Is.LessThanOrEqualTo(150), label);
            Assert.That(copy.Korean.Length, Is.LessThanOrEqualTo(90), label);
            Assert.That(SentenceCount(copy.English), Is.LessThanOrEqualTo(2), label);
            Assert.That(SentenceCount(copy.Korean), Is.LessThanOrEqualTo(2), label);
        }

        private static void AssertBilingual(LocalizedCopy copy, string label)
        {
            Assert.That(copy, Is.Not.Null, label);
            Assert.That(copy.English, Is.Not.Empty, label);
            Assert.That(copy.Korean, Is.Not.Empty, label);
            Assert.That(copy.ForLocale("en"), Is.EqualTo(copy.English), label);
            Assert.That(copy.ForLocale("ko"), Is.EqualTo(copy.Korean), label);
        }

        private static int SentenceCount(string value)
            => value.Count(character => character == '.' || character == '?' || character == '!');

        private static IncidentStageDefinition MinimalStage(
            IEnumerable<IncidentArtifactEntry> queue,
            IReadOnlyList<SortingRule> rules = null)
        {
            return new IncidentStageDefinition(
                "invalid-stage",
                new[] { Beat("Intro", "도입") },
                new[] { Beat("Outro", "마무리") },
                Reactions(),
                "unmelting-ice",
                null,
                queue.ToArray(),
                rules ?? DefaultRules(),
                1);
        }

        private static NarrativeBeat Beat(string english, string korean)
            => new NarrativeBeat(new LocalizedCopy(english, korean), SeniorClerkMood.Neutral, IncidentVisualCue.None);

        private static ArtifactReaction Reactions()
            => new ArtifactReaction(
                new LocalizedCopy("Recovered.", "회복했습니다."),
                new LocalizedCopy("Handled calmly.", "침착하게 돌봤습니다."),
                new LocalizedCopy("The office answers.", "보관소가 답합니다."));

        private static SortingRule[] DefaultRules()
        {
            return new[]
            {
                new SortingRule("fragile-repair", ArtifactTraits.Fragile, ArtifactTraits.None,
                    Destination.Repair, false),
                new SortingRule("temporal-vault", ArtifactTraits.Temporal, ArtifactTraits.None,
                    Destination.Vault, false),
                new SortingRule("fallback-storage", ArtifactTraits.None, ArtifactTraits.None,
                    Destination.Storage, true)
            };
        }

        private static IReadOnlyDictionary<string, ArtifactContent> ArtifactDictionary()
            => ContentCatalog.CreateArtifacts().ToDictionary(item => item.Id, StringComparer.Ordinal);

        private static IEnumerable<string> QueueIds() => ExpectedStages[0].QueueIds;

        private static string Pattern(IEnumerable<Destination> destinations)
        {
            return string.Concat(destinations.Select(destination =>
            {
                switch (destination)
                {
                    case Destination.Repair: return "R";
                    case Destination.Storage: return "S";
                    case Destination.Vault: return "V";
                    default: throw new ArgumentOutOfRangeException(nameof(destination));
                }
            }));
        }

        private sealed class StageExpectation
        {
            public StageExpectation(
                string id,
                string[] queueIds,
                string pattern,
                int minimumHolds,
                string[] frostedIds,
                string introEnglish,
                string introKorean,
                string outroEnglish,
                string outroKorean)
            {
                Id = id;
                QueueIds = queueIds;
                Pattern = pattern;
                MinimumHolds = minimumHolds;
                FrostedIds = frostedIds;
                IntroEnglish = introEnglish;
                IntroKorean = introKorean;
                OutroEnglish = outroEnglish;
                OutroKorean = outroKorean;
            }

            public string Id { get; }
            public string[] QueueIds { get; }
            public string Pattern { get; }
            public int MinimumHolds { get; }
            public string[] FrostedIds { get; }
            public string IntroEnglish { get; }
            public string IntroKorean { get; }
            public string OutroEnglish { get; }
            public string OutroKorean { get; }
        }
    }
}
