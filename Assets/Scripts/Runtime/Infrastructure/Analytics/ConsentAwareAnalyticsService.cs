using System;
using System.Collections.Generic;
using UnityEngine;

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
            if (!IsEnabled || string.IsNullOrWhiteSpace(eventName))
            {
                return;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"[Analytics consented] {eventName}");
#endif
        }
    }
}

