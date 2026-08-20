using System;

namespace CurioClerk.Infrastructure.Diagnostics
{
    public interface ICrashReporter
    {
        bool IsEnabled { get; }

        void SetConsent(bool enabled);

        void Log(string message);

        void Record(Exception exception);
    }
}
