using System.Collections.Generic;

namespace CurioClerk.Infrastructure.Analytics
{
    public interface IAnalyticsService
    {
        bool IsEnabled { get; }

        void SetConsent(bool enabled);

        void Track(string eventName, IReadOnlyDictionary<string, string> parameters = null);
    }
}

