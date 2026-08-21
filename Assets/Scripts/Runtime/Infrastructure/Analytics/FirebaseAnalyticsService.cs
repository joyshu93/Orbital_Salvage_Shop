using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CurioClerk.Infrastructure.Firebase;
using Firebase.Analytics;

namespace CurioClerk.Infrastructure.Analytics
{
    public interface IFirebaseAnalyticsClient
    {
        void SetCollectionEnabled(bool enabled);

        void LogEvent(string eventName, IReadOnlyDictionary<string, string> parameters);
    }

    public sealed class FirebaseAnalyticsService : IAnalyticsService
    {
        private readonly Task<bool> _dependencyTask;
        private readonly IFirebaseAnalyticsClient _client;
        private bool _dependencyAvailable;
        private bool _dependencyResolved;
        private bool _requestedConsent;
        private bool _sdkCollectionEnabled;

        public FirebaseAnalyticsService()
            : this(FirebaseRuntime.DependencyTask, new FirebaseAnalyticsClient())
        {
        }

        public FirebaseAnalyticsService(Task<bool> dependencyTask, IFirebaseAnalyticsClient client)
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

        public void Track(string eventName, IReadOnlyDictionary<string, string> parameters = null)
        {
            if (!_sdkCollectionEnabled || !AnalyticsEvents.IsValid(eventName, parameters))
            {
                return;
            }

            try
            {
                _client.LogEvent(eventName, parameters);
            }
            catch (Exception)
            {
                // Analytics must never interrupt offline gameplay.
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
            try
            {
                _client.SetCollectionEnabled(enabled);
                _sdkCollectionEnabled = enabled;
            }
            catch (Exception)
            {
                _sdkCollectionEnabled = false;
            }
        }

        private sealed class FirebaseAnalyticsClient : IFirebaseAnalyticsClient
        {
            public void SetCollectionEnabled(bool enabled)
            {
                FirebaseAnalytics.SetAnalyticsCollectionEnabled(enabled);
            }

            public void LogEvent(string eventName, IReadOnlyDictionary<string, string> parameters)
            {
                if (parameters == null || parameters.Count == 0)
                {
                    FirebaseAnalytics.LogEvent(eventName);
                    return;
                }

                var sdkParameters = new Parameter[parameters.Count];
                var index = 0;
                foreach (var parameter in parameters)
                {
                    sdkParameters[index++] = new Parameter(parameter.Key, parameter.Value);
                }

                FirebaseAnalytics.LogEvent(eventName, sdkParameters);
            }
        }
    }
}
