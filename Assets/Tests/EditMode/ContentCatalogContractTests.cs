using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CurioClerk.Content;
using CurioClerk.Core.Rules;
using CurioClerk.Core.Shifts;
using NUnit.Framework;

namespace CurioClerk.Tests.EditMode
{
    public sealed class ContentCatalogContractTests
    {
        [Test]
        public void ArtifactCatalog_ContainsTwentyFourUniqueBilingualArtifactsWithOneToThreeTraits()
        {
            var catalog = RequireCatalog();
            var artifacts = Items(catalog.GetMethod("CreateArtifacts").Invoke(null, null));

            Assert.That(artifacts, Has.Count.EqualTo(24));
            Assert.That(artifacts.Select(item => String(item, "Id")).Distinct().Count(), Is.EqualTo(24));
            foreach (var artifact in artifacts)
            {
                Assert.That(String(artifact, "NameEnglish"), Is.Not.Empty);
                Assert.That(String(artifact, "NameKorean"), Is.Not.Empty);
                Assert.That(String(artifact, "DescriptionEnglish"), Is.Not.Empty);
                Assert.That(String(artifact, "DescriptionKorean"), Is.Not.Empty);
                var bits = Convert.ToInt32(Property(artifact, "Traits"));
                var traitCount = 0;
                while (bits != 0)
                {
                    traitCount += bits & 1;
                    bits >>= 1;
                }

                Assert.That(traitCount, Is.InRange(1, 3), String(artifact, "Id"));
            }
        }

        [Test]
        public void RuleAndDocketCatalog_HasAuthoredBalancedGameplayStructures()
        {
            Assert.That(ContentCatalog.ContentVersion, Is.EqualTo(2));

            var ruleTemplates = ContentCatalog.CreateRuleTemplates();
            Assert.That(ruleTemplates.Count, Is.EqualTo(10));
            Assert.That(ruleTemplates.Select(rule => rule.Id).Distinct().Count(), Is.EqualTo(10));

            var artifacts = ContentCatalog.CreateArtifacts()
                .Select(content => content.ToArtifact())
                .ToArray();
            var packs = ContentCatalog.CreateRulePacks();
            Assert.That(packs.Select(pack => pack.Id),
                Is.EqualTo(new[] { "pack-cursed-fragile", "pack-temporal-wet" }));
            foreach (var pack in packs)
            {
                Assert.That(pack.Rules, Has.Count.EqualTo(3), pack.Id);
                Assert.That(pack.Rules.Take(2).All(rule => !rule.IsFallback), Is.True, pack.Id);
                Assert.That(pack.Rules[2].IsFallback, Is.True, pack.Id);
                Assert.That(pack.Rules[2].Destination, Is.EqualTo(Destination.Storage), pack.Id);

                var engine = new RuleEngine();
                var destinations = artifacts
                    .Select(artifact => engine.Resolve(artifact, pack.Rules))
                    .ToArray();
                Assert.That(destinations.Count(value => value == Destination.Repair),
                    Is.GreaterThanOrEqualTo(4), pack.Id);
                Assert.That(destinations.Count(value => value == Destination.Storage),
                    Is.GreaterThanOrEqualTo(4), pack.Id);
                Assert.That(destinations.Count(value => value == Destination.Vault),
                    Is.GreaterThanOrEqualTo(4), pack.Id);
            }

            var templates = ContentCatalog.CreateShiftTemplates();
            Assert.That(templates.Select(template => template.Id),
                Is.EqualTo(new[] { "docket-band-1", "docket-band-2", "docket-band-3" }));
            Assert.That(templates.Select(template => Pattern(template.Destinations)),
                Is.EqualTo(new[]
                {
                    "RRSVRSVRSVSV",
                    "VVRSRSVSSRVR",
                    "RRSVSSVRRVSV"
                }));
            Assert.That(templates.Select(template => template.MinimumRequiredHolds),
                Is.EqualTo(new[] { 1, 2, 3 }));
            var analyzer = new DocketSequenceAnalyzer();
            foreach (var template in templates)
            {
                Assert.That(analyzer.MinimumHolds(template.Destinations),
                    Is.EqualTo(template.MinimumRequiredHolds), template.Id);
            }
        }

        [Test]
        public void CosmeticCatalog_HasSixProgressivelyPricedUnlocks()
        {
            var catalog = RequireCatalog();
            var cosmetics = Items(catalog.GetMethod("CreateCosmetics").Invoke(null, null));
            var costs = cosmetics.Select(item => Convert.ToInt32(Property(item, "Cost"))).ToArray();

            Assert.That(cosmetics, Has.Count.EqualTo(6));
            Assert.That(costs, Is.Ordered.Ascending);
            Assert.That(costs[0], Is.GreaterThan(0));
        }

        private static Type RequireCatalog()
        {
            var type = Type.GetType("CurioClerk.Content.ContentCatalog, CurioClerk.Runtime");
            Assert.That(type, Is.Not.Null, "Missing production type: CurioClerk.Content.ContentCatalog");
            return type;
        }

        private static List<object> Items(object enumerable)
        {
            var result = new List<object>();
            foreach (var item in (IEnumerable)enumerable)
            {
                result.Add(item);
            }

            return result;
        }

        private static object Property(object item, string name) => item.GetType().GetProperty(name).GetValue(item);

        private static string String(object item, string name) => (string)Property(item, name);

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
    }
}
