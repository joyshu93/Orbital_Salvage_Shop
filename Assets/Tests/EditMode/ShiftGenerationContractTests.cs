using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace CurioClerk.Tests.EditMode
{
    public sealed class ShiftGenerationContractTests
    {
        [Test]
        public void GenerateArtifactQueue_IsDeterministicAndContainsNoDuplicates()
        {
            var artifactType = Require("CurioClerk.Core.Artifacts.Artifact");
            var traitsType = Require("CurioClerk.Core.Artifacts.ArtifactTraits");
            var generatorType = Require("CurioClerk.Core.Shifts.ShiftGenerator");
            var source = Array.CreateInstance(artifactType, 24);
            var metallic = Enum.Parse(traitsType, "Metallic");
            for (var index = 0; index < source.Length; index++)
            {
                source.SetValue(Activator.CreateInstance(artifactType, $"artifact-{index:00}", metallic), index);
            }

            var generator = Activator.CreateInstance(generatorType);
            var method = generatorType.GetMethod("GenerateArtifactQueue");
            Assert.That(method, Is.Not.Null);

            var first = Ids((IEnumerable)method.Invoke(generator, new object[] { 7727, source, 12 }));
            var second = Ids((IEnumerable)method.Invoke(generator, new object[] { 7727, source, 12 }));

            Assert.That(first, Is.EqualTo(second));
            Assert.That(first, Has.Count.EqualTo(12));
            Assert.That(first.Distinct().Count(), Is.EqualTo(12));
        }

        [Test]
        public void DailySeed_UsesTheLocalCalendarDateAndContentVersion()
        {
            var providerType = Require("CurioClerk.Core.Shifts.DailySeedProvider");
            var method = providerType.GetMethod("ForDate");
            Assert.That(method, Is.Not.Null);

            var morning = (int)method.Invoke(null, new object[] { new DateTime(2026, 8, 20, 8, 0, 0), 1 });
            var evening = (int)method.Invoke(null, new object[] { new DateTime(2026, 8, 20, 22, 0, 0), 1 });
            var nextDay = (int)method.Invoke(null, new object[] { new DateTime(2026, 8, 21, 8, 0, 0), 1 });
            var nextContent = (int)method.Invoke(null, new object[] { new DateTime(2026, 8, 20, 8, 0, 0), 2 });

            Assert.That(morning, Is.EqualTo(evening));
            Assert.That(nextDay, Is.Not.EqualTo(morning));
            Assert.That(nextContent, Is.Not.EqualTo(morning));
        }

        private static List<string> Ids(IEnumerable artifacts)
        {
            var ids = new List<string>();
            foreach (var artifact in artifacts)
            {
                ids.Add((string)artifact.GetType().GetProperty("Id").GetValue(artifact));
            }

            return ids;
        }

        private static Type Require(string fullName)
        {
            var type = Type.GetType($"{fullName}, CurioClerk.Core");
            Assert.That(type, Is.Not.Null, $"Missing production type: {fullName}");
            return type;
        }
    }
}

