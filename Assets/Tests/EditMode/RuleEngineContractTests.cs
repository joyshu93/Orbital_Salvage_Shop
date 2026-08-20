using System;
using System.Reflection;
using NUnit.Framework;

namespace CurioClerk.Tests.EditMode
{
    public sealed class RuleEngineContractTests
    {
        private const string AssemblyName = "CurioClerk.Core";

        [Test]
        public void Resolve_UsesTheFirstMatchingRule()
        {
            var api = RuleApi.Load();
            var artifact = api.Artifact("two-traits", "Cursed, Fragile");
            var rules = api.Rules(
                api.Rule("cursed", "Cursed", "None", "Vault", false),
                api.Rule("fragile", "Fragile", "None", "Repair", false),
                api.Rule("fallback", "None", "None", "Storage", true));

            var destination = api.Resolve(artifact, rules);

            Assert.That(destination.ToString(), Is.EqualTo("Vault"));
        }

        [Test]
        public void Resolve_UsesTheFinalFallbackWhenNothingMatches()
        {
            var api = RuleApi.Load();
            var artifact = api.Artifact("plain", "Metallic");
            var rules = api.Rules(
                api.Rule("cursed", "Cursed", "None", "Vault", false),
                api.Rule("fallback", "None", "None", "Storage", true));

            var destination = api.Resolve(artifact, rules);

            Assert.That(destination.ToString(), Is.EqualTo("Storage"));
        }

        [Test]
        public void Resolve_RejectsRuleListsWithoutAFinalFallback()
        {
            var api = RuleApi.Load();
            var artifact = api.Artifact("plain", "Metallic");
            var rules = api.Rules(api.Rule("metal", "Metallic", "None", "Repair", false));

            var error = Assert.Throws<TargetInvocationException>(() => api.Resolve(artifact, rules));

            Assert.That(error.InnerException, Is.TypeOf<InvalidOperationException>());
        }

        private sealed class RuleApi
        {
            private readonly Type _traitsType;
            private readonly Type _destinationType;
            private readonly Type _artifactType;
            private readonly Type _ruleType;
            private readonly object _engine;
            private readonly MethodInfo _resolve;

            private RuleApi(Type traitsType, Type destinationType, Type artifactType, Type ruleType, Type engineType)
            {
                _traitsType = traitsType;
                _destinationType = destinationType;
                _artifactType = artifactType;
                _ruleType = ruleType;
                _engine = Activator.CreateInstance(engineType);
                _resolve = engineType.GetMethod("Resolve", BindingFlags.Public | BindingFlags.Instance);
                Assert.That(_resolve, Is.Not.Null, "RuleEngine.Resolve must be public.");
            }

            public static RuleApi Load()
            {
                return new RuleApi(
                    Require("CurioClerk.Core.Artifacts.ArtifactTraits"),
                    Require("CurioClerk.Core.Rules.Destination"),
                    Require("CurioClerk.Core.Artifacts.Artifact"),
                    Require("CurioClerk.Core.Rules.SortingRule"),
                    Require("CurioClerk.Core.Rules.RuleEngine"));
            }

            public object Artifact(string id, string traits)
            {
                return Activator.CreateInstance(_artifactType, id, Enum.Parse(_traitsType, traits));
            }

            public object Rule(string id, string requiredAll, string requiredAny, string destination, bool fallback)
            {
                return Activator.CreateInstance(
                    _ruleType,
                    id,
                    Enum.Parse(_traitsType, requiredAll),
                    Enum.Parse(_traitsType, requiredAny),
                    Enum.Parse(_destinationType, destination),
                    fallback);
            }

            public Array Rules(params object[] rules)
            {
                var array = Array.CreateInstance(_ruleType, rules.Length);
                for (var index = 0; index < rules.Length; index++)
                {
                    array.SetValue(rules[index], index);
                }

                return array;
            }

            public object Resolve(object artifact, Array rules)
            {
                return _resolve.Invoke(_engine, new object[] { artifact, rules });
            }

            private static Type Require(string fullName)
            {
                var type = Type.GetType($"{fullName}, {AssemblyName}");
                Assert.That(type, Is.Not.Null, $"Missing production type: {fullName}");
                return type;
            }
        }
    }
}
