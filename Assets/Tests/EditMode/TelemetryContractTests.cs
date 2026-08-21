using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CurioClerk.Infrastructure.Analytics;
using CurioClerk.Infrastructure.Diagnostics;
using CurioClerk.Infrastructure.Time;
using NUnit.Framework;

namespace CurioClerk.Tests.EditMode
{
    public sealed class TelemetryContractTests
    {
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
        public void Track_WhileDisabled_DoesNotCrossSdkBoundary()
        {
            var client = new RecordingAnalyticsClient();
            var service = ReadyService(client);
            client.Clear();

            service.Track("tutorial_started");

            Assert.That(client.Events, Is.Empty);
        }

        [Test]
        public void ConsentWithdrawal_ImmediatelyDisablesSdkCollection()
        {
            var client = new RecordingAnalyticsClient();
            var service = ReadyService(client);
            client.Clear();

            service.SetConsent(true);
            service.SetConsent(false);

            Assert.That(client.CollectionStates, Is.EqualTo(new[] { true, false }));
            Assert.That(service.IsEnabled, Is.False);
        }

        [Test]
        public void PendingInitialization_AppliesOnlyLatestWithdrawnConsent()
        {
            var availability = new TaskCompletionSource<bool>();
            var client = new RecordingAnalyticsClient();
            var service = new FirebaseAnalyticsService(availability.Task, client);

            service.SetConsent(true);
            service.SetConsent(false);
            availability.SetResult(true);

            Assert.That(client.CollectionStates, Is.EqualTo(new[] { false }));
            Assert.That(service.IsEnabled, Is.False);
        }

        [Test]
        public void Track_RejectsUnknownNamesFreeTextAndIdentifierShapedValues()
        {
            var client = new RecordingAnalyticsClient();
            var service = ReadyService(client);
            service.SetConsent(true);
            client.Clear();

            service.Track("artifact_examined");
            service.Track("shift_started", Parameters("description", "It remembers every lamp it loved."));
            service.Track("shift_started", Parameters("band", "2026-08-21"));
            service.Track("cosmetic_unlocked", Parameters("cosmetic_id", "player@example.com"));
            service.Track("cosmetic_unlocked", Parameters("cosmetic_id", "38400000-8cf0-11bd-b23e-10b96e40000d"));

            Assert.That(client.Events, Is.Empty);
        }

        [Test]
        public void Track_AllowsOnlyTheDocumentedParametersForEachEvent()
        {
            var client = new RecordingAnalyticsClient();
            var service = ReadyService(client);
            service.SetConsent(true);
            client.Clear();

            service.Track("shift_failed", new Dictionary<string, string>
            {
                { "band", "3" },
                { "sorted_bucket", "4_7" }
            });

            Assert.That(client.Events, Has.Count.EqualTo(1));
            Assert.That(client.Events[0].Name, Is.EqualTo("shift_failed"));
            Assert.That(client.Events[0].Parameters, Is.EqualTo(new Dictionary<string, string>
            {
                { "band", "3" },
                { "sorted_bucket", "4_7" }
            }));
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

        [Test]
        public void GameTelemetry_EmitsOnlyCoarseValuesAndNeverSendsLocalTime()
        {
            var client = new RecordingAnalyticsClient();
            var service = ReadyService(client);
            service.SetConsent(true);
            client.Clear();
            var clock = new FakeClock(new DateTime(2026, 8, 21, 13, 45, 12, DateTimeKind.Local));
            var telemetry = new GameTelemetry(service, clock);

            telemetry.TutorialStarted();
            telemetry.TutorialCompleted();
            telemetry.ShiftStarted(3);
            telemetry.ShiftFailed(3, 6);
            telemetry.ShiftStarted(4);
            clock.LocalNow = clock.LocalNow.AddSeconds(75.25d);
            telemetry.ShiftCompleted(4);
            telemetry.RewardOfferShown("shift_complete_double");
            telemetry.RewardResult("shift_complete_double", "earned");
            telemetry.CosmeticUnlocked("brass-lamp");

            Assert.That(client.Events, Has.Count.EqualTo(9));
            Assert.That(client.Find("shift_failed").Parameters, Is.EqualTo(new Dictionary<string, string>
            {
                { "band", "3" },
                { "sorted_bucket", "4_7" }
            }));
            Assert.That(client.Find("shift_completed").Parameters, Is.EqualTo(new Dictionary<string, string>
            {
                { "band", "4" },
                { "duration_bucket", "60_119" }
            }));
            Assert.That(client.Find("reward_result").Parameters, Is.EqualTo(new Dictionary<string, string>
            {
                { "placement", "shift_complete_double" },
                { "result", "earned" }
            }));
            Assert.That(client.Find("cosmetic_unlocked").Parameters, Is.EqualTo(new Dictionary<string, string>
            {
                { "cosmetic_id", "brass-lamp" }
            }));
        }

        [Test]
        public void CrashReporter_LogsOnlyWhileIndependentlyEnabled()
        {
            var client = new RecordingCrashlyticsClient();
            var reporter = new FirebaseCrashReporter(Task.FromResult(true), client);
            client.Clear();

            reporter.Log("disabled");
            reporter.Record(new InvalidOperationException("disabled"));
            reporter.SetConsent(true);
            reporter.Log("enabled");
            reporter.Record(new InvalidOperationException("enabled"));
            reporter.SetConsent(false);
            reporter.Log("withdrawn");

            Assert.That(client.CollectionStates, Is.EqualTo(new[] { true, false }));
            Assert.That(client.Logs, Is.EqualTo(new[] { "enabled" }));
            Assert.That(client.Exceptions, Has.Count.EqualTo(1));
            Assert.That(reporter.IsEnabled, Is.False);
        }

        [Test]
        public void CrashReporter_ConfiguresFatalPolicyBeforeEnablingCollection()
        {
            var client = new RecordingCrashlyticsClient();
            var reporter = new FirebaseCrashReporter(Task.FromResult(true), client);
            client.Clear();

            reporter.SetConsent(true);

            Assert.That(client.Operations, Is.EqualTo(new[] { "fatal:true", "collection:true" }));
        }

        private static FirebaseAnalyticsService ReadyService(RecordingAnalyticsClient client)
        {
            return new FirebaseAnalyticsService(Task.FromResult(true), client);
        }

        private static IReadOnlyDictionary<string, string> Parameters(string name, string value)
        {
            return new Dictionary<string, string> { { name, value } };
        }

        private sealed class RecordingAnalyticsClient : IFirebaseAnalyticsClient
        {
            public List<bool> CollectionStates { get; } = new List<bool>();

            public List<RecordedEvent> Events { get; } = new List<RecordedEvent>();

            public void SetCollectionEnabled(bool enabled)
            {
                CollectionStates.Add(enabled);
            }

            public void LogEvent(string eventName, IReadOnlyDictionary<string, string> parameters)
            {
                Events.Add(new RecordedEvent(eventName, parameters));
            }

            public void Clear()
            {
                CollectionStates.Clear();
                Events.Clear();
            }

            public RecordedEvent Find(string eventName)
            {
                return Events.Find(item => item.Name == eventName);
            }
        }

        private sealed class RecordedEvent
        {
            public RecordedEvent(string name, IReadOnlyDictionary<string, string> parameters)
            {
                Name = name;
                Parameters = parameters;
            }

            public string Name { get; }

            public IReadOnlyDictionary<string, string> Parameters { get; }
        }

        private sealed class RecordingCrashlyticsClient : IFirebaseCrashlyticsClient
        {
            public List<bool> CollectionStates { get; } = new List<bool>();

            public List<string> Logs { get; } = new List<string>();

            public List<Exception> Exceptions { get; } = new List<Exception>();

            public List<string> Operations { get; } = new List<string>();

            public void SetCollectionEnabled(bool enabled)
            {
                CollectionStates.Add(enabled);
                Operations.Add("collection:" + enabled.ToString().ToLowerInvariant());
            }

            public void SetReportUncaughtExceptionsAsFatal(bool enabled)
            {
                Operations.Add("fatal:" + enabled.ToString().ToLowerInvariant());
            }

            public void Log(string message)
            {
                Logs.Add(message);
            }

            public void Record(Exception exception)
            {
                Exceptions.Add(exception);
            }

            public void Clear()
            {
                CollectionStates.Clear();
                Logs.Clear();
                Exceptions.Clear();
                Operations.Clear();
            }
        }

        private sealed class FakeClock : IClock
        {
            public FakeClock(DateTime localNow)
            {
                LocalNow = localNow;
            }

            public DateTime LocalNow { get; set; }
        }
    }
}
