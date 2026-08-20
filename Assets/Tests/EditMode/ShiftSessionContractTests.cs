using System;
using System.Reflection;
using NUnit.Framework;

namespace CurioClerk.Tests.EditMode
{
    public sealed class ShiftSessionContractTests
    {
        [Test]
        public void CorrectSorts_BuildComboScoreAndCoinsThenCompleteTheShift()
        {
            var api = SessionApi.Create(2);

            api.Sort("Vault");
            api.Sort("Vault");

            Assert.That(api.Int("Combo"), Is.EqualTo(2));
            Assert.That(api.Int("Score"), Is.EqualTo(220));
            Assert.That(api.Int("Coins"), Is.EqualTo(11));
            Assert.That(api.Value("State").ToString(), Is.EqualTo("Completed"));
        }

        [Test]
        public void ThreeMistakes_FailAndOnlyOneReviveCanBeClaimed()
        {
            var api = SessionApi.Create(4);

            api.Sort("Repair");
            api.Sort("Repair");
            api.Sort("Repair");

            Assert.That(api.Int("Hearts"), Is.Zero);
            Assert.That(api.Value("State").ToString(), Is.EqualTo("Failed"));
            Assert.That(api.CallBool("TryRevive"), Is.True);
            Assert.That(api.Int("Hearts"), Is.EqualTo(1));
            Assert.That(api.Value("State").ToString(), Is.EqualTo("Active"));
            Assert.That(api.CallBool("TryRevive"), Is.False);
            Assert.That(api.Bool("RewardClaimed"), Is.True);
        }

        [Test]
        public void CompletedShift_CanDoubleCoinsOnlyOnce()
        {
            var api = SessionApi.Create(1);
            api.Sort("Vault");

            Assert.That(api.CallBool("TryDoubleCoins"), Is.True);
            Assert.That(api.Int("Coins"), Is.EqualTo(10));
            Assert.That(api.CallBool("TryDoubleCoins"), Is.False);
            Assert.That(api.Bool("RewardClaimed"), Is.True);
        }

        [Test]
        public void Hold_CannotRepeatUntilTheCurrentArtifactIsSorted()
        {
            var api = SessionApi.Create(3);

            Assert.That(api.StringFromObjectProperty("CurrentArtifact", "Id"), Is.EqualTo("artifact-0"));
            Assert.That(api.CallBool("Hold"), Is.True);
            Assert.That(api.StringFromObjectProperty("CurrentArtifact", "Id"), Is.EqualTo("artifact-1"));
            Assert.That(api.StringFromObjectProperty("HeldArtifact", "Id"), Is.EqualTo("artifact-0"));
            Assert.That(api.CallBool("Hold"), Is.False);

            api.Sort("Vault");
            Assert.That(api.StringFromObjectProperty("CurrentArtifact", "Id"), Is.EqualTo("artifact-2"));
            Assert.That(api.CallBool("Hold"), Is.True);
            Assert.That(api.StringFromObjectProperty("CurrentArtifact", "Id"), Is.EqualTo("artifact-0"));
            Assert.That(api.StringFromObjectProperty("HeldArtifact", "Id"), Is.EqualTo("artifact-2"));
        }

        private sealed class SessionApi
        {
            private readonly object _session;
            private readonly Type _sessionType;
            private readonly Type _destinationType;

            private SessionApi(object session, Type sessionType, Type destinationType)
            {
                _session = session;
                _sessionType = sessionType;
                _destinationType = destinationType;
            }

            public static SessionApi Create(int artifactCount)
            {
                var artifactType = Require("CurioClerk.Core.Artifacts.Artifact");
                var traitsType = Require("CurioClerk.Core.Artifacts.ArtifactTraits");
                var ruleType = Require("CurioClerk.Core.Rules.SortingRule");
                var destinationType = Require("CurioClerk.Core.Rules.Destination");
                var sessionType = Require("CurioClerk.Core.Shifts.ShiftSession");
                var cursed = Enum.Parse(traitsType, "Cursed");
                var none = Enum.Parse(traitsType, "None");
                var vault = Enum.Parse(destinationType, "Vault");
                var storage = Enum.Parse(destinationType, "Storage");

                var artifacts = Array.CreateInstance(artifactType, artifactCount);
                for (var index = 0; index < artifactCount; index++)
                {
                    artifacts.SetValue(Activator.CreateInstance(artifactType, $"artifact-{index}", cursed), index);
                }

                var rules = Array.CreateInstance(ruleType, 2);
                rules.SetValue(Activator.CreateInstance(ruleType, "cursed", cursed, none, vault, false), 0);
                rules.SetValue(Activator.CreateInstance(ruleType, "fallback", none, none, storage, true), 1);
                var session = Activator.CreateInstance(sessionType, artifacts, rules);
                return new SessionApi(session, sessionType, destinationType);
            }

            public void Sort(string destination)
            {
                var method = _sessionType.GetMethod("Sort", BindingFlags.Public | BindingFlags.Instance);
                Assert.That(method, Is.Not.Null);
                method.Invoke(_session, new[] { Enum.Parse(_destinationType, destination) });
            }

            public bool CallBool(string methodName)
            {
                var method = _sessionType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
                Assert.That(method, Is.Not.Null);
                return (bool)method.Invoke(_session, null);
            }

            public object Value(string propertyName)
            {
                var property = _sessionType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                Assert.That(property, Is.Not.Null);
                return property.GetValue(_session);
            }

            public int Int(string propertyName) => (int)Value(propertyName);

            public bool Bool(string propertyName) => (bool)Value(propertyName);

            public string StringFromObjectProperty(string propertyName, string childPropertyName)
            {
                var value = Value(propertyName);
                return (string)value.GetType().GetProperty(childPropertyName).GetValue(value);
            }

            private static Type Require(string fullName)
            {
                var type = Type.GetType($"{fullName}, CurioClerk.Core");
                Assert.That(type, Is.Not.Null, $"Missing production type: {fullName}");
                return type;
            }
        }
    }
}
