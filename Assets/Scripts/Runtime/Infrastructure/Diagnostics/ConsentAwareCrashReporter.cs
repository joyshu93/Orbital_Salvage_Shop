using System;

namespace CurioClerk.Infrastructure.Diagnostics
{
    public sealed class ConsentAwareCrashReporter : ICrashReporter
    {
        public bool IsEnabled { get; private set; }

        public void SetConsent(bool enabled)
        {
            IsEnabled = enabled;
        }

        public void Log(string message)
        {
        }

        public void Record(Exception exception)
        {
        }
    }
}
