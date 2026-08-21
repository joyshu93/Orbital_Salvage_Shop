using System;
using System.Collections.Generic;
using System.Linq;
using CurioClerk.Infrastructure;
using CurioClerk.Infrastructure.Analytics;
using CurioClerk.Infrastructure.Diagnostics;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace CurioClerk.Tests.EditMode
{
    public sealed class TelemetryContractTests
    {
        [Test]
        public void RuntimeAssembly_DoesNotReferenceRemoteTelemetrySdk()
        {
            var referencedAssemblies = typeof(ServiceFactory).Assembly
                .GetReferencedAssemblies()
                .Select(assembly => assembly.Name)
                .ToArray();

            Assert.That(
                referencedAssemblies.Any(name => name.StartsWith("Firebase.", StringComparison.Ordinal)),
                Is.False);
        }

        [Test]
        public void ServiceFactory_CreatesOnlyLocalNonTransportTelemetryServices()
        {
            Assert.That(ServiceFactory.CreateAnalyticsService(), Is.TypeOf<ConsentAwareAnalyticsService>());
            Assert.That(ServiceFactory.CreateCrashReporter(), Is.TypeOf<ConsentAwareCrashReporter>());
        }

        [Test]
        public void LocalAnalyticsService_DoesNotLogOrRetainTrackedPayloads()
        {
            var service = new ConsentAwareAnalyticsService();
            var payload = new Dictionary<string, string>
            {
                { "band", "3" },
                { "description", "must not leave the process" }
            };

            service.SetConsent(true);
            service.Track("shift_started", payload);
            payload["description"] = "mutated after tracking";
            service.SetConsent(false);

            Assert.That(service.IsEnabled, Is.False);
            Assert.That(
                typeof(ConsentAwareAnalyticsService)
                    .GetFields(
                        System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.Static |
                        System.Reflection.BindingFlags.NonPublic)
                    .Select(field => field.FieldType),
                Is.All.EqualTo(typeof(bool)));
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void LocalCrashReporter_DoesNotLogOrRetainDiagnostics()
        {
            var reporter = new ConsentAwareCrashReporter();

            reporter.SetConsent(true);
            reporter.Log("must not leave the process");
            reporter.Record(new InvalidOperationException("must not be cached"));
            reporter.SetConsent(false);

            Assert.That(reporter.IsEnabled, Is.False);
            Assert.That(
                typeof(ConsentAwareCrashReporter)
                    .GetFields(
                        System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.Static |
                        System.Reflection.BindingFlags.NonPublic)
                    .Select(field => field.FieldType),
                Is.All.EqualTo(typeof(bool)));
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void AnalyticsSchema_AllowsOnlyDocumentedEventsAndCoarseParameters()
        {
            Assert.That(AnalyticsEvents.All, Is.EquivalentTo(new[]
            {
                "tutorial_started", "tutorial_completed", "shift_started", "shift_failed",
                "shift_completed", "reward_offer_shown", "reward_result", "cosmetic_unlocked"
            }));
            Assert.That(AnalyticsEvents.AllowedParameterNames, Is.EquivalentTo(new[]
            {
                "band", "sorted_bucket", "duration_bucket", "placement", "result", "cosmetic_id"
            }));
        }

        [Test]
        public void AnalyticsSchema_RejectsUnknownNamesFreeTextAndIdentifierShapedValues()
        {
            Assert.That(AnalyticsEvents.IsValid("artifact_examined"), Is.False);
            Assert.That(AnalyticsEvents.IsValid(
                "shift_started",
                Parameters("description", "It remembers every lamp it loved.")), Is.False);
            Assert.That(AnalyticsEvents.IsValid("shift_started", Parameters("band", "2026-08-21")), Is.False);
            Assert.That(AnalyticsEvents.IsValid(
                "cosmetic_unlocked",
                Parameters("cosmetic_id", "player@example.com")), Is.False);
            Assert.That(AnalyticsEvents.IsValid(
                "cosmetic_unlocked",
                Parameters("cosmetic_id", "38400000-8cf0-11bd-b23e-10b96e40000d")), Is.False);
        }

        [Test]
        public void AnalyticsSchema_AcceptsOnlyTheDocumentedPayloadForAnEvent()
        {
            Assert.That(AnalyticsEvents.IsValid("shift_failed", new Dictionary<string, string>
            {
                { "band", "3" },
                { "sorted_bucket", "4_7" }
            }), Is.True);
            Assert.That(AnalyticsEvents.IsValid("shift_failed", new Dictionary<string, string>
            {
                { "band", "3" },
                { "sorted_bucket", "4_7" },
                { "result", "earned" }
            }), Is.False);
        }

        [TestCase(-1, "0_3")]
        [TestCase(3, "0_3")]
        [TestCase(4, "4_7")]
        [TestCase(7, "4_7")]
        [TestCase(8, "8_12")]
        [TestCase(99, "8_12")]
        public void SortedCountBucket_UsesOnlyCoarseDocumentedRanges(int count, string expected)
        {
            Assert.That(GameTelemetry.SortedCountBucket(count), Is.EqualTo(expected));
        }

        [TestCase(-1d, "under_60")]
        [TestCase(59.999d, "under_60")]
        [TestCase(60d, "60_119")]
        [TestCase(119.999d, "60_119")]
        [TestCase(120d, "120_plus")]
        public void DurationBucket_UsesOnlyCoarseDocumentedRanges(double seconds, string expected)
        {
            Assert.That(GameTelemetry.DurationBucket(seconds), Is.EqualTo(expected));
        }

        private static IReadOnlyDictionary<string, string> Parameters(string name, string value)
        {
            return new Dictionary<string, string> { { name, value } };
        }
    }
}
