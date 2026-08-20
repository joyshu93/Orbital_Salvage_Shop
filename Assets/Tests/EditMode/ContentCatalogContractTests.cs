using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        public void RuleAndDifficultyCatalog_HasTenTemplatesAndFiveValidBands()
        {
            var catalog = RequireCatalog();
            var templates = Items(catalog.GetMethod("CreateRuleTemplates").Invoke(null, null));
            Assert.That(templates, Has.Count.EqualTo(10));

            for (var band = 1; band <= 5; band++)
            {
                var rules = Items(catalog.GetMethod("CreateRulesForBand").Invoke(null, new object[] { band, 1234 }));
                Assert.That(rules.Last().GetType().GetProperty("IsFallback").GetValue(rules.Last()), Is.True);
                Assert.That(rules.Count, Is.EqualTo(band <= 2 ? 3 : band <= 4 ? 4 : 5));
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
    }
}

