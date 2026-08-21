using System.Collections.Generic;

namespace CurioClerk.Infrastructure.Analytics
{
    public sealed class ConsentAwareAnalyticsService : IAnalyticsService
    {
        public bool IsEnabled { get; private set; }

        public void SetConsent(bool enabled)
        {
            IsEnabled = enabled;
        }

        public void Track(string eventName, IReadOnlyDictionary<string, string> parameters = null)
        {
        }
    }
}
