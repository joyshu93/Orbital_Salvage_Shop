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
                    "whispering-key", "silent-bell", "sleeping-teacup", "patient-compass",
                    "backward-candle", "moon-umbrella", "humming-scarf", "clockwork-moth",
                    "unmelting-ice", "lantern-snail", "murmur-box", "yesterday-ticket"
                },
                "VVRSRSVSSRVR",
                2,
                new[] { "patient-compass", "moon-umbrella", "clockwork-moth", "unmelting-ice" },
                "The frost has marked four curios. File every white-rimmed one to Storage before the cold returns to the ice.",
                "서리가 네 물건을 골랐어요. 흰 테가 생긴 것은 모두 보관실로 보내, 추위가 얼음으로 돌아가지 못하게 하세요.",
                "All four white rims have faded. The leaf inside the ice is gone, yet not a drop escaped.",
                "네 개의 흰 테가 모두 사라졌어요. 얼음 속 낙엽도 사라졌지만, 물은 한 방울도 새지 않았습니다."),
            new StageExpectation(
                "ice-03-tomorrow",
                new[]
                {
                    "moon-umbrella", "sleeping-teacup", "clockwork-moth", "mossy-watch",
                    "patient-compass", "thimble-storm", "unmelting-ice", "porcelain-tooth",
                    "lantern-snail", "rain-jar", "tide-locket", "rusty-comet"
                },
                "RRSVSSVRRVSV",
                3,
                new[] { "clockwork-moth", "mossy-watch", "unmelting-ice", "rain-jar", "rusty-comet" },
                "The missing leaf is inside this watch, dated tomorrow. It is both frosted and temporal—time outranks frost.",
                "사라진 낙엽이 이 시계 안에 있어요. 날짜는 내일입니다. 서리와 시간성이 겹치면 시간 규칙이 먼저예요.",
                "The watch points back at this desk. Tomorrow is not waiting for us anymore.",
                "시계가 다시 이 책상을 가리킵니다. 이제 내일은 우리를 기다려 주지 않아요."),
            new StageExpectation(
                "ice-04-frozen-seal",
                new[]
                {
                    "unmelting-ice", "mossy-watch", "moon-umbrella", "clockwork-moth",
                    "sleeping-teacup", "porcelain-tooth", "patient-compass", "lantern-snail",
                    "thimble-storm", "rain-jar", "tide-locket", "rusty-comet"
                },
                "VVRSRRSRSVSV",
                2,
                new[] { "unmelting-ice", "mossy-watch", "clockwork-moth", "patient-compass", "thimble-storm", "tide-locket" },
                "The Vault seal is frozen. The watch belongs in Vault, but that seal was just used. Protect it in Hold and open Repair first.",
                "봉인고 인장이 얼었습니다. 시계는 봉인고가 맞지만 방금 그 인장을 썼어요. 보류에서 지키고 수리실을 먼저 여세요.",
                "The watch is safe. Everything you protected is trembling toward the moon-mended umbrella.",
                "시계는 무사합니다. 당신이 보호한 물건들이 모두 달빛으로 기운 우산을 향해 떨고 있어요."),
            new StageExpectation(
                "ice-05-thaw",
                new[]
                {
                    "paper-fish", "moon-umbrella", "clockwork-moth", "unmelting-ice",
                    "patient-compass", "mossy-watch", "thimble-storm", "mirror-seed",
                    "ink-snowglobe", "rain-jar", "tide-locket", "rusty-comet"
                },
                "RRSVSVSRRVSV",
                3,
                new[] { "clockwork-moth", "unmelting-ice", "rain-jar", "tide-locket" },
                "One last shift. No new rule: protect what cannot be filed, trust time over frost, and listen for rain.",
                "마지막 교대입니다. 새 규칙은 없어요. 지금 처리할 수 없는 것은 보호하고, 서리보다 시간을 믿고, 빗소리를 따라가세요.",
                "The ice collapses into warm light—without water. Rain answers from inside the sealed umbrella.",
                "얼음이 물 한 방울 없이 따뜻한 빛으로 무너집니다. 봉인된 우산 안에서 비가 대답합니다.")
        };

        [Test]
        public void FirstIncident_HasTheExactAuthoredQueuePatternAndHoldMatrix()
        {
            var incident = ContentCatalog.CreateIncidents().Single(incident => incident.Id == "unmelting-ice");
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
        public void SecondIncident_HasExactFiveStageGameplayAndNarrativeMatrix()
        {
            var incident = ContentCatalog.CreateIncidents().Single(value => value.Id == "remembering-rain");
            var artifacts = ContentCatalog.CreateArtifacts().ToDictionary(item => item.Id, StringComparer.Ordinal);
            var ruleEngine = new RuleEngine();
            var analyzer = new DocketSequenceAnalyzer();
            var expected = new[]
            {
                new RainStageExpectation("rain-01-voices", "moon-umbrella", "paper-fish", 2,
                    new[] { ArtifactTraits.Wet, ArtifactTraits.Fragile, ArtifactTraits.Alive, ArtifactTraits.None },
                    new[] { Destination.Vault, Destination.Repair, Destination.Storage, Destination.Storage },
                    new[] { "moon-umbrella", "paper-fish", "sleeping-teacup", "clockwork-moth", "backward-candle", "patient-compass", "rain-jar", "porcelain-tooth", "humming-scarf", "yesterday-ticket", "murmur-box", "ink-snowglobe" },
                    "VVRSRSVRSRSV", "promise", "약속"),
                new RainStageExpectation("rain-02-names-under-water", "rain-jar", "rain-jar", 2,
                    new[] { ArtifactTraits.Temporal, ArtifactTraits.Wet, ArtifactTraits.Alive, ArtifactTraits.Fragile, ArtifactTraits.None },
                    new[] { Destination.Vault, Destination.Repair, Destination.Storage, Destination.Repair, Destination.Storage },
                    new[] { "moon-umbrella", "mossy-watch", "rain-jar", "clockwork-moth", "paper-fish", "humming-scarf", "yesterday-ticket", "porcelain-tooth", "whispering-key", "ink-snowglobe", "patient-compass", "unmelting-ice" },
                    "RVVSRSVRSRSV", "Tuesday", "화요일"),
                new RainStageExpectation("rain-03-unsent-letter", "paper-fish", "paper-fish", 2,
                    new[] { ArtifactTraits.Alive, ArtifactTraits.Fragile, ArtifactTraits.Cursed, ArtifactTraits.None },
                    new[] { Destination.Storage, Destination.Repair, Destination.Vault, Destination.Vault },
                    new[] { "rain-jar", "humming-scarf", "paper-fish", "moon-umbrella", "whispering-key", "porcelain-tooth", "clockwork-moth", "mirror-seed", "borrowed-shadow", "yesterday-ticket", "silent-bell", "patient-compass" },
                    "VSSRVRSRVRVS", "Night Repository", "야간 보관소"),
                new RainStageExpectation("rain-04-dry-order", "moon-umbrella", "moon-umbrella", 2,
                    new[] { ArtifactTraits.Cursed, ArtifactTraits.Wet, ArtifactTraits.Fragile, ArtifactTraits.Alive, ArtifactTraits.None },
                    new[] { Destination.Vault, Destination.Storage, Destination.Repair, Destination.Storage, Destination.Repair },
                    new[] { "sleeping-teacup", "rain-jar", "moon-umbrella", "whispering-key", "backward-candle", "clockwork-moth", "silent-bell", "rusty-comet", "ink-snowglobe", "lantern-snail", "tide-locket", "murmur-box" },
                    "RSSVRSVRVRSV", "sender", "발신"),
                new RainStageExpectation("rain-05-testimony", "paper-fish", "paper-fish", 3,
                    new[] { ArtifactTraits.Temporal, ArtifactTraits.Wet, ArtifactTraits.Alive, ArtifactTraits.Cursed, ArtifactTraits.Fragile, ArtifactTraits.None },
                    new[] { Destination.Vault, Destination.Repair, Destination.Storage, Destination.Vault, Destination.Repair, Destination.Storage },
                    new[] { "moon-umbrella", "paper-fish", "clockwork-moth", "rain-jar", "humming-scarf", "patient-compass", "whispering-key", "thimble-storm", "ink-snowglobe", "borrowed-shadow", "murmur-box", "unmelting-ice" },
                    "RRSVSSVRRVSV", "clerk before your senior", "선임보다 먼저 일한 관리인")
            };

            Assert.That(incident.Title.English, Is.EqualTo("The Remembering Rain"));
            Assert.That(incident.Title.Korean, Is.EqualTo("기억하는 비"));
            Assert.That(incident.LeadArtifactId, Is.EqualTo("moon-umbrella"));
            Assert.That(incident.BoardVisualCue, Is.EqualTo(IncidentVisualCue.Rain));
            Assert.That(incident.CompletesWhenAllStagesCompleted, Is.True);
            Assert.That(incident.AwaitingContentClue, Is.Null);
            Assert.That(incident.Stages.Select(stage => stage.Id), Is.EqualTo(expected.Select(stage => stage.Id)));

            for (var index = 0; index < expected.Length; index++)
            {
                var stage = incident.Stages[index];
                var contract = expected[index];
                var plan = stage.CreateShiftPlan(artifacts);
                var destinations = plan.Queue.Select(item => ruleEngine.Resolve(item, plan.Rules)).ToArray();
                var allEnglish = NarrativeText(stage, "en");
                var allKorean = NarrativeText(stage, "ko");

                Assert.That(stage.LeadArtifactId, Is.EqualTo(contract.LeadArtifactId), stage.Id);
                Assert.That(stage.ResonanceHoldArtifactId, Is.EqualTo(contract.ProtectedArtifactId), stage.Id);
                Assert.That(stage.MinimumRequiredHolds, Is.EqualTo(contract.MinimumHolds), stage.Id);
                Assert.That(stage.Queue, Has.Count.EqualTo(12), stage.Id);
                Assert.That(stage.Queue.Select(item => item.ArtifactId), Is.EqualTo(contract.QueueIds), stage.Id);
                Assert.That(stage.Queue.Select(item => item.ArtifactId).Distinct().Count(), Is.EqualTo(12), stage.Id);
                Assert.That(stage.Rules.Select(rule => rule.RequiredAll), Is.EqualTo(contract.RequiredTraits), stage.Id);
                Assert.That(stage.Rules.Select(rule => rule.Destination), Is.EqualTo(contract.Destinations), stage.Id);
                Assert.That(stage.Rules.Take(stage.Rules.Count - 1).All(rule => !rule.IsFallback), Is.True, stage.Id);
                Assert.That(stage.Rules.Last().IsFallback, Is.True, stage.Id);
                Assert.That(Pattern(destinations), Is.EqualTo(contract.Pattern), stage.Id);
                Assert.That(destinations.Count(value => value == Destination.Repair), Is.EqualTo(4), stage.Id);
                Assert.That(destinations.Count(value => value == Destination.Storage), Is.EqualTo(4), stage.Id);
                Assert.That(destinations.Count(value => value == Destination.Vault), Is.EqualTo(4), stage.Id);
                Assert.That(analyzer.MinimumHolds(destinations), Is.EqualTo(contract.MinimumHolds), stage.Id);
                Assert.That(stage.DocketBeats.Select(beat => beat.CompletedDocketNumber), Is.EqualTo(new[] { 1, 2, 3 }), stage.Id);
                Assert.That(stage.DocketBeats.All(beat => beat.Narrative.Speaker != null), Is.True, stage.Id);
                foreach (var docketBeat in stage.DocketBeats)
                {
                    AssertBilingual(docketBeat.Narrative.Speaker, stage.Id + " docket speaker");
                    AssertBilingual(docketBeat.Narrative.Copy, stage.Id + " docket body");
                }

                StringAssert.Contains(contract.EnglishAnchor, allEnglish, stage.Id);
                StringAssert.Contains(contract.KoreanAnchor, allKorean, stage.Id);
                Assert.Throws<NotSupportedException>(() =>
                    ((IList<IncidentArtifactEntry>)stage.Queue)[0] = stage.Queue[0], stage.Id);
                Assert.Throws<NotSupportedException>(() =>
                    ((IList<SortingRule>)stage.Rules)[0] = stage.Rules[0], stage.Id);
                Assert.Throws<NotSupportedException>(() =>
                    ((IList<IncidentDocketBeat>)stage.DocketBeats)[0] = stage.DocketBeats[0], stage.Id);
            }

            Assert.Throws<NotSupportedException>(() =>
                ((IList<IncidentStageDefinition>)incident.Stages)[0] = incident.Stages[0]);
        }

        [Test]
        public void ThirdIncident_IsFixedReadOnlyUpcomingPreview()
        {
            var incident = ContentCatalog.CreateIncidents().Single(value => value.Id == "one-minute-ahead");

            Assert.That(incident.Title.English, Is.EqualTo("One Minute Ahead"));
            Assert.That(incident.Title.Korean, Is.EqualTo("1분 앞선 저녁"));
            Assert.That(incident.LeadArtifactId, Is.EqualTo("backward-candle"));
            Assert.That(incident.BoardVisualCue, Is.EqualTo(IncidentVisualCue.AmberWarmth));
            Assert.That(incident.Stages, Is.Empty);
            Assert.That(incident.CompletesWhenAllStagesCompleted, Is.False);
            Assert.That(incident.AwaitingContentClue.English, Is.EqualTo("The moss grows toward 2:17."));
            Assert.That(incident.AwaitingContentClue.Korean, Is.EqualTo("이끼가 2시 17분을 향해 자라고 있다."));
        }

        [Test]
        public void FirstIncident_HasExactBilingualOpeningsClosingsAndBoundedQualityReactions()
        {
            var incident = ContentCatalog.CreateIncidents().Single(incident => incident.Id == "unmelting-ice");
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
        public void StageThree_RepeatsFourFrostedTemporalPriorityJudgmentsAcrossTheShift()
        {
            var stage = ContentCatalog.CreateIncidents().Single(incident => incident.Id == "unmelting-ice").Stages[2];
            var artifacts = ContentCatalog.CreateArtifacts().ToDictionary(item => item.Id, StringComparer.Ordinal);
            var plan = stage.CreateShiftPlan(artifacts);
            var engine = new RuleEngine();
            var conflicts = plan.Queue.Where(item =>
                (item.Traits & ArtifactTraits.Frosted) != 0 &&
                (item.Traits & ArtifactTraits.Temporal) != 0).ToArray();

            Assert.That(conflicts.Select(item => item.Id), Is.EqualTo(new[]
            {
                "mossy-watch", "unmelting-ice", "rain-jar", "rusty-comet"
            }));
            Assert.That(conflicts.Select(item => engine.ResolveDetailed(item, plan.Rules).RuleId),
                Is.All.EqualTo("incident-temporal-vault"));
            Assert.That(conflicts.Select(item => engine.Resolve(item, plan.Rules)),
                Is.All.EqualTo(Destination.Vault));
        }

        [Test]
        public void FirstIncident_FirstShiftExplainsTheFantasyTheThreatAndTheClerksJobBeforeLeavingAStoryHook()
        {
            var stage = ContentCatalog.CreateIncidents().Single(incident => incident.Id == "unmelting-ice").Stages[0];

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

            foreach (var stage in ContentCatalog.CreateIncidents().Single(incident => incident.Id == "unmelting-ice").Stages)
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

            Assert.That(ContentCatalog.CreateIncidents().Single(incident => incident.Id == "unmelting-ice").Stages.Select(stage => stage.ResonanceHoldArtifactId),
                Is.EqualTo(new[] { null, null, null, "mossy-watch", "moon-umbrella" }));
        }

        [Test]
        public void CreateShiftPlan_AddsStageTraitsWithoutMutatingBaseArtifacts()
        {
            var artifacts = ContentCatalog.CreateArtifacts().ToDictionary(item => item.Id, StringComparer.Ordinal);
            var stage = ContentCatalog.CreateIncidents().Single(incident => incident.Id == "unmelting-ice").Stages[1];

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

        [TestCase("", "단서")]
        [TestCase("Clue", "")]
        public void OpenIncident_RequiresBilingualAwaitingContentClue(string english, string korean)
        {
            var stage = MinimalStage(QueueIds().Select(id =>
                new IncidentArtifactEntry(id, ArtifactTraits.None)));

            Assert.Throws<ArgumentException>(() => new IncidentDefinition(
                "open-incident",
                new LocalizedCopy("Title", "제목"),
                "unmelting-ice",
                IncidentVisualCue.Rain,
                completesWhenAllStagesCompleted: false,
                awaitingContentClue: new LocalizedCopy(english, korean),
                stages: new[] { stage }));
        }

        [Test]
        public void IncidentDefinition_AllowsBilingualZeroStagePreviewButRejectsConclusiveZeroStageIncident()
        {
            var title = new LocalizedCopy("Preview", "예고");
            var clue = new LocalizedCopy("Awaiting clue.", "단서를 기다린다.");

            var preview = new IncidentDefinition(
                "preview",
                title,
                "backward-candle",
                IncidentVisualCue.AmberWarmth,
                completesWhenAllStagesCompleted: false,
                awaitingContentClue: clue,
                stages: Array.Empty<IncidentStageDefinition>());

            Assert.That(preview.Stages, Is.Empty);
            Assert.That(preview.AwaitingContentClue, Is.SameAs(clue));
            Assert.Throws<ArgumentException>(() => new IncidentDefinition(
                "invalid",
                title,
                "backward-candle",
                IncidentVisualCue.AmberWarmth,
                completesWhenAllStagesCompleted: true,
                awaitingContentClue: null,
                stages: Array.Empty<IncidentStageDefinition>()));
        }

        [Test]
        public void NarrativeBeat_PreservesOptionalBilingualSpeakerAndLegacyConstructorLeavesItNull()
        {
            var speaker = new LocalizedCopy("Voice in the Rain", "빗속의 목소리");
            var named = new NarrativeBeat(
                speaker,
                new LocalizedCopy("Listen.", "들어."),
                SeniorClerkMood.Alert,
                IncidentVisualCue.Rain);
            var legacy = Beat("Legacy", "기존");

            Assert.That(named.Speaker, Is.SameAs(speaker));
            Assert.That(named.Speaker.ForLocale("ko"), Is.EqualTo("빗속의 목소리"));
            Assert.That(legacy.Speaker, Is.Null);
        }

        [TestCase(0)]
        [TestCase(4)]
        public void IncidentDocketBeat_RejectsDocketNumbersOutsideAuthoredInterludes(int docketNumber)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                new IncidentDocketBeat(docketNumber, Beat("Beat", "막간")));
        }

        [Test]
        public void IncidentDocketBeat_RejectsNullNarrative()
        {
            Assert.Throws<ArgumentNullException>(() => new IncidentDocketBeat(1, null));
        }

        [Test]
        public void IncidentStageDefinition_CopiesAndFindsAuthoredDocketBeats()
        {
            var first = new IncidentDocketBeat(1, Beat("First", "첫째"));
            var third = new IncidentDocketBeat(3, Beat("Third", "셋째"));
            var authored = new[] { first, third };
            var stage = new IncidentStageDefinition(
                "docket-beat-stage",
                new[] { Beat("Intro", "도입") },
                new[] { Beat("Outro", "마무리") },
                Reactions(),
                "unmelting-ice",
                null,
                QueueIds().Select(id => new IncidentArtifactEntry(id, ArtifactTraits.None)).ToArray(),
                DefaultRules(),
                1,
                authored);

            authored[0] = new IncidentDocketBeat(2, Beat("Changed", "변경"));

            Assert.That(stage.DocketBeats, Is.EqualTo(new[] { first, third }));
            Assert.That(stage.FindDocketBeat(1), Is.SameAs(first));
            Assert.That(stage.FindDocketBeat(2), Is.Null);
            Assert.That(stage.FindDocketBeat(3), Is.SameAs(third));
            Assert.That(stage.FindDocketBeat(0), Is.Null);
            Assert.That(stage.FindDocketBeat(4), Is.Null);
        }

        [Test]
        public void IncidentStageDefinition_RejectsDuplicateDocketBeatNumbers()
        {
            var duplicate = new[]
            {
                new IncidentDocketBeat(2, Beat("First", "첫째")),
                new IncidentDocketBeat(2, Beat("Second", "둘째"))
            };

            Assert.Throws<ArgumentException>(() => new IncidentStageDefinition(
                "duplicate-docket-beat-stage",
                new[] { Beat("Intro", "도입") },
                new[] { Beat("Outro", "마무리") },
                Reactions(),
                "unmelting-ice",
                null,
                QueueIds().Select(id => new IncidentArtifactEntry(id, ArtifactTraits.None)).ToArray(),
                DefaultRules(),
                1,
                duplicate));
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
            var incident = new IncidentDefinition(
                "copy-incident",
                new LocalizedCopy("Title", "제목"),
                "unmelting-ice",
                IncidentVisualCue.Frost,
                completesWhenAllStagesCompleted: true,
                awaitingContentClue: null,
                stages: stages);

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
            Assert.That(incident.LeadArtifactId, Is.EqualTo("unmelting-ice"));
            Assert.That(incident.BoardVisualCue, Is.EqualTo(IncidentVisualCue.Frost));
            Assert.That(incident.CompletesWhenAllStagesCompleted, Is.True);
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

        private static string NarrativeText(IncidentStageDefinition stage, string locale)
        {
            var beats = stage.IntroBeats
                .Concat(stage.DocketBeats.Select(docket => docket.Narrative))
                .Concat(stage.OutroBeats);
            return string.Join(" ", beats.Select(beat => beat.Copy.ForLocale(locale)));
        }

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

        private sealed class RainStageExpectation
        {
            public RainStageExpectation(
                string id,
                string leadArtifactId,
                string protectedArtifactId,
                int minimumHolds,
                ArtifactTraits[] requiredTraits,
                Destination[] destinations,
                string[] queueIds,
                string pattern,
                string englishAnchor,
                string koreanAnchor)
            {
                Id = id;
                LeadArtifactId = leadArtifactId;
                ProtectedArtifactId = protectedArtifactId;
                MinimumHolds = minimumHolds;
                RequiredTraits = requiredTraits;
                Destinations = destinations;
                QueueIds = queueIds;
                Pattern = pattern;
                EnglishAnchor = englishAnchor;
                KoreanAnchor = koreanAnchor;
            }

            public string Id { get; }
            public string LeadArtifactId { get; }
            public string ProtectedArtifactId { get; }
            public int MinimumHolds { get; }
            public ArtifactTraits[] RequiredTraits { get; }
            public Destination[] Destinations { get; }
            public string[] QueueIds { get; }
            public string Pattern { get; }
            public string EnglishAnchor { get; }
            public string KoreanAnchor { get; }
        }
    }
}
