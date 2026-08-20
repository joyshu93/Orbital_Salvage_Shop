using System;
using UnityEngine;

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
            if (!IsEnabled || string.IsNullOrWhiteSpace(message))
            {
                return;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[Crash diagnostics consented] {message}");
#endif
        }

        public void Record(Exception exception)
        {
            if (!IsEnabled || exception == null)
            {
                return;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogException(exception);
#endif
        }
    }
}
