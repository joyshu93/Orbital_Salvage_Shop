using System;
using System.Threading;
using System.Threading.Tasks;
using CurioClerk.Infrastructure.Firebase;
using Firebase.Crashlytics;

namespace CurioClerk.Infrastructure.Diagnostics
{
    public interface IFirebaseCrashlyticsClient
    {
        void SetCollectionEnabled(bool enabled);

        void SetReportUncaughtExceptionsAsFatal(bool enabled);

        void Log(string message);

        void Record(Exception exception);
    }

    public sealed class FirebaseCrashReporter : ICrashReporter
    {
        private readonly Task<bool> _dependencyTask;
        private readonly IFirebaseCrashlyticsClient _client;
        private bool _dependencyAvailable;
        private bool _dependencyResolved;
        private bool _requestedConsent;
        private bool _sdkCollectionEnabled;

        public FirebaseCrashReporter()
            : this(FirebaseRuntime.DependencyTask, new FirebaseCrashlyticsClient())
        {
        }

        public FirebaseCrashReporter(Task<bool> dependencyTask, IFirebaseCrashlyticsClient client)
        {
            _dependencyTask = dependencyTask ?? throw new ArgumentNullException(nameof(dependencyTask));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            ObserveDependencyResult();
        }

        public bool IsEnabled => _sdkCollectionEnabled;

        public void SetConsent(bool enabled)
        {
            _requestedConsent = enabled;
            if (!enabled)
            {
                _sdkCollectionEnabled = false;
            }

            if (_dependencyResolved && _dependencyAvailable)
            {
                ApplyRequestedConsent();
            }
        }

        public void Log(string message)
        {
            if (!_sdkCollectionEnabled || string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            try
            {
                _client.Log(message);
            }
            catch (Exception)
            {
                // Crash reporting must never interrupt offline gameplay.
            }
        }

        public void Record(Exception exception)
        {
            if (!_sdkCollectionEnabled || exception == null)
            {
                return;
            }

            try
            {
                _client.Record(exception);
            }
            catch (Exception)
            {
                // Crash reporting must never interrupt offline gameplay.
            }
        }

        private void ObserveDependencyResult()
        {
            _dependencyTask.ContinueWith(
                CompleteDependencyCheck,
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }

        private void CompleteDependencyCheck(Task<bool> task)
        {
            _dependencyAvailable =
                !task.IsCanceled &&
                !task.IsFaulted &&
                task.Result;
            _dependencyResolved = true;
            if (_dependencyAvailable)
            {
                ApplyRequestedConsent();
            }
        }

        private void ApplyRequestedConsent()
        {
            var enabled = _requestedConsent;
            if (!enabled)
            {
                _sdkCollectionEnabled = false;
            }

            try
            {
                if (enabled)
                {
                    _client.SetReportUncaughtExceptionsAsFatal(true);
                    _client.SetCollectionEnabled(true);
                }
                else
                {
                    _client.SetCollectionEnabled(false);
                    _client.SetReportUncaughtExceptionsAsFatal(true);
                }

                _sdkCollectionEnabled = enabled;
            }
            catch (Exception)
            {
                _sdkCollectionEnabled = false;
            }
        }

        private sealed class FirebaseCrashlyticsClient : IFirebaseCrashlyticsClient
        {
            public void SetCollectionEnabled(bool enabled)
            {
                Crashlytics.IsCrashlyticsCollectionEnabled = enabled;
            }

            public void SetReportUncaughtExceptionsAsFatal(bool enabled)
            {
                Crashlytics.ReportUncaughtExceptionsAsFatal = enabled;
            }

            public void Log(string message)
            {
                Crashlytics.Log(message);
            }

            public void Record(Exception exception)
            {
                Crashlytics.LogException(exception);
            }
        }
    }
}
