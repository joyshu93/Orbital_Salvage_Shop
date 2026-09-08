using System;
using System.Collections.Generic;
using System.Linq;
using CurioClerk.Core.Artifacts;
using CurioClerk.Core.Rules;
using CurioClerk.Core.Shifts;
using NUnit.Framework;

namespace CurioClerk.Tests.EditMode
{
    public sealed class ShiftGenerationContractTests
    {
        [TestCase("RRSVRSVRSVSV", 1)]
        [TestCase("VVRSRSVSSRVR", 2)]
        [TestCase("RRSVSSVRRVSV", 3)]
        public void MinimumHolds_ReturnsTheValidatedTemplateCost(string pattern, int expected)
        {
            var destinations = pattern.Select(ParseDestination).ToArray();

            Assert.That(new DocketSequenceAnalyzer().MinimumHolds(destinations), Is.EqualTo(expected));
        }

        [Test]
        public void MinimumHolds_ReturnsMinusOneForAOneSlotDeadlock()
        {
            var destinations = "RRRRSSSSVVVV".Select(ParseDestination).ToArray();

            Assert.That(new DocketSequenceAnalyzer().MinimumHolds(destinations), Is.EqualTo(-1));
        }

        [Test]
        public void MinimumHolds_RejectsSequencesThatCannotFormWholeDockets()
        {
            Assert.Throws<ArgumentException>(() =>
                new DocketSequenceAnalyzer().MinimumHolds(
                    new[]
                    {
                        Destination.Repair,
                        Destination.Storage,
                        Destination.Vault,
                        Destination.Repair
                    }));
        }

        [Test]
        public void Generate_IsDeterministicBalancedAndUsesDistinctArtifacts()
        {
            var artifacts = CreateBalancedArtifacts();
            var packs = new[]
            {
                CreatePack("pack-a", "a"),
                CreatePack("pack-b", "b")
            };
            var templates = CreateTemplates();
            var generator = new ShiftPlanGenerator();

            var first = generator.Generate(7727, 2, artifacts, packs, templates);
            var second = generator.Generate(7727, 2, artifacts, packs, templates);

            Assert.That(first.RulePackId, Is.EqualTo(second.RulePackId));
            Assert.That(first.SequenceTemplateId, Is.EqualTo(second.SequenceTemplateId));
            Assert.That(first.Queue.Select(artifact => artifact.Id),
                Is.EqualTo(second.Queue.Select(artifact => artifact.Id)));
            Assert.That(first.Queue, Has.Count.EqualTo(12));
            Assert.That(first.Queue.Select(artifact => artifact.Id).Distinct().Count(), Is.EqualTo(12));

            var ruleEngine = new RuleEngine();
            var resolved = first.Queue
                .Select(artifact => ruleEngine.Resolve(artifact, first.Rules))
                .ToArray();
            Assert.That(resolved.Count(destination => destination == Destination.Repair), Is.EqualTo(4));
            Assert.That(resolved.Count(destination => destination == Destination.Storage), Is.EqualTo(4));
            Assert.That(resolved.Count(destination => destination == Destination.Vault), Is.EqualTo(4));

            var selectedTemplate = templates.Single(template => template.Id == first.SequenceTemplateId);
            var minimumHolds = new DocketSequenceAnalyzer().MinimumHolds(resolved);
            Assert.That(minimumHolds, Is.GreaterThanOrEqualTo(selectedTemplate.MinimumRequiredHolds));
        }

        [Test]
        public void Generate_RejectsRulePackWithFewerThanFourStorageCandidates()
        {
            var artifacts = CreateArtifacts(5, 4, 3);
            var packs = new[] { CreatePack("undersupplied", "u") };
            var templates = new[]
            {
                new ShiftSequenceTemplate("balanced", 1, 3, 1,
                    ParsePattern("RRSVRSVRSVSV"))
            };

            Assert.Throws<InvalidOperationException>(() =>
                new ShiftPlanGenerator().Generate(9, 1, artifacts, packs, templates));
        }

        [Test]
        public void Generate_RejectsTemplateWithoutFourOfEveryDestination()
        {
            var templates = new[]
            {
                new ShiftSequenceTemplate("unbalanced", 1, 3, 0,
                    ParsePattern("RRRRRRRRRRRR"))
            };

            Assert.Throws<InvalidOperationException>(() =>
                new ShiftPlanGenerator().Generate(
                    9,
                    1,
                    CreateBalancedArtifacts(),
                    new[] { CreatePack("pack", "p") },
                    templates));
        }

        [Test]
        public void Generate_RejectsTemplateWhoseDeclaredMinimumExceedsActualCost()
        {
            var templates = new[]
            {
                new ShiftSequenceTemplate("overstated", 1, 3, 2,
                    ParsePattern("RRSVRSVRSVSV"))
            };

            Assert.Throws<InvalidOperationException>(() =>
                new ShiftPlanGenerator().Generate(
                    9,
                    1,
                    CreateBalancedArtifacts(),
                    new[] { CreatePack("pack", "p") },
                    templates));
        }

        [Test]
        public void Generate_RejectsDuplicateArtifactIdsBeforeSeededSelection()
        {
            var artifacts = CreateBalancedArtifacts();
            artifacts[artifacts.Length - 1] =
                new Artifact(artifacts[0].Id, ArtifactTraits.Metallic);

            Assert.Throws<InvalidOperationException>(() =>
                new ShiftPlanGenerator().Generate(
                    7727,
                    1,
                    artifacts,
                    new[] { CreatePack("pack", "p") },
                    new[]
                    {
                        new ShiftSequenceTemplate("balanced", 1, 3, 1,
                            ParsePattern("RRSVRSVRSVSV"))
                    }));
        }

        [Test]
        public void PlanInputs_AreCopiedAtConstructionBoundaries()
        {
            var rules = CreatePack("source", "s").Rules.ToArray();
            var pack = new ShiftRulePack("copied-pack", 1, 3, rules);
            var destinations = ParsePattern("RRSVRSVRSVSV");
            var template = new ShiftSequenceTemplate("copied-template", 1, 3, 1, destinations);
            var queue = CreateBalancedArtifacts().Take(12).ToArray();
            var plan = new ShiftPlan(pack.Id, template.Id, queue, rules);

            var firstRuleId = pack.Rules[0].Id;
            var firstDestination = template.Destinations[0];
            var firstArtifactId = plan.Queue[0].Id;
            rules[0] = rules[1];
            destinations[0] = Destination.Storage;
            queue[0] = queue[1];

            Assert.That(pack.Rules[0].Id, Is.EqualTo(firstRuleId));
            Assert.That(template.Destinations[0], Is.EqualTo(firstDestination));
            Assert.That(plan.Queue[0].Id, Is.EqualTo(firstArtifactId));
        }

        [Test]
        public void SequenceTemplate_RejectsLengthsOtherThanTwelve()
        {
            Assert.Throws<ArgumentException>(() =>
                new ShiftSequenceTemplate(
                    "short",
                    1,
                    3,
                    0,
                    new[] { Destination.Repair, Destination.Storage, Destination.Vault }));
        }

        [Test]
        public void DailySeed_UsesTheLocalCalendarDateAndContentVersion()
        {
            var morning = DailySeedProvider.ForDate(new DateTime(2026, 8, 20, 8, 0, 0), 1);
            var evening = DailySeedProvider.ForDate(new DateTime(2026, 8, 20, 22, 0, 0), 1);
            var nextDay = DailySeedProvider.ForDate(new DateTime(2026, 8, 21, 8, 0, 0), 1);
            var nextContent = DailySeedProvider.ForDate(new DateTime(2026, 8, 20, 8, 0, 0), 2);

            Assert.That(morning, Is.EqualTo(evening));
            Assert.That(nextDay, Is.Not.EqualTo(morning));
            Assert.That(nextContent, Is.Not.EqualTo(morning));
        }

        private static Artifact[] CreateBalancedArtifacts()
            => CreateArtifacts(8, 8, 8);

        private static Artifact[] CreateArtifacts(int vaultCount, int repairCount, int storageCount)
        {
            var artifacts = new List<Artifact>();
            AddArtifacts(artifacts, "vault", vaultCount, ArtifactTraits.Cursed);
            AddArtifacts(artifacts, "repair", repairCount, ArtifactTraits.Fragile);
            AddArtifacts(artifacts, "storage", storageCount, ArtifactTraits.Metallic);
            return artifacts.ToArray();
        }

        private static void AddArtifacts(
            ICollection<Artifact> artifacts,
            string prefix,
            int count,
            ArtifactTraits traits)
        {
            for (var index = 0; index < count; index++)
            {
                artifacts.Add(new Artifact($"{prefix}-{index:00}", traits));
            }
        }

        private static ShiftRulePack CreatePack(string id, string rulePrefix)
        {
            return new ShiftRulePack(id, 1, 3, new[]
            {
                new SortingRule($"{rulePrefix}-cursed-vault", ArtifactTraits.Cursed,
                    ArtifactTraits.None, Destination.Vault, false),
                new SortingRule($"{rulePrefix}-fragile-repair", ArtifactTraits.Fragile,
                    ArtifactTraits.None, Destination.Repair, false),
                new SortingRule($"{rulePrefix}-fallback-storage", ArtifactTraits.None,
                    ArtifactTraits.None, Destination.Storage, true)
            });
        }

        private static ShiftSequenceTemplate[] CreateTemplates()
        {
            return new[]
            {
                new ShiftSequenceTemplate("template-one", 1, 3, 1,
                    ParsePattern("RRSVRSVRSVSV")),
                new ShiftSequenceTemplate("template-two", 1, 3, 2,
                    ParsePattern("VVRSRSVSSRVR")),
                new ShiftSequenceTemplate("template-three", 1, 3, 3,
                    ParsePattern("RRSVSSVRRVSV"))
            };
        }

        private static Destination[] ParsePattern(string pattern)
            => pattern.Select(ParseDestination).ToArray();

        private static Destination ParseDestination(char value)
        {
            switch (value)
            {
                case 'R': return Destination.Repair;
                case 'S': return Destination.Storage;
                case 'V': return Destination.Vault;
                default: throw new ArgumentOutOfRangeException(nameof(value));
            }
        }
    }
}
