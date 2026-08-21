using System;
using System.Text.RegularExpressions;
using CurioClerk.Infrastructure.Ads;
using CurioClerk.Infrastructure.Analytics;
using CurioClerk.Infrastructure.Diagnostics;
using CurioClerk.Infrastructure.Privacy;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine;
#endif

namespace CurioClerk.Infrastructure
{
    public static class ServiceFactory
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        private const string AndroidRewardedTestUnitId = "ca-app-pub-3940256099942544/5224354917";
        private static readonly Regex AndroidRewardedUnitIdPattern =
            new Regex(@"\Aca-app-pub-[0-9]+/[0-9]+\z", RegexOptions.CultureInvariant);
#endif
#if UNITY_INCLUDE_TESTS
        private static IAdService s_TestAdService;
        private static IPrivacyService s_TestPrivacyService;
#endif

        public static IAdService CreateAdService()
        {
#if UNITY_INCLUDE_TESTS
            if (s_TestAdService != null)
            {
                return s_TestAdService;
            }
#endif
#if UNITY_ANDROID && !UNITY_EDITOR && DEVELOPMENT_BUILD
            return new GoogleRewardedAdService(AndroidRewardedTestUnitId);
#elif UNITY_ANDROID && !UNITY_EDITOR
            var configuration = Resources.Load<ServiceConfiguration>("ServiceConfiguration");
            var rewardedId = configuration == null ? null : configuration.AndroidRewardedAdUnitId;
            if (string.IsNullOrEmpty(rewardedId) ||
                !AndroidRewardedUnitIdPattern.IsMatch(rewardedId) ||
                string.Equals(rewardedId, AndroidRewardedTestUnitId, StringComparison.Ordinal))
            {
                return new UnavailableAdService();
            }

            return new GoogleRewardedAdService(rewardedId);
#else
            return new UnavailableAdService();
#endif
        }

        public static IAnalyticsService CreateAnalyticsService() => new ConsentAwareAnalyticsService();

        public static IPrivacyService CreatePrivacyService()
        {
#if UNITY_INCLUDE_TESTS
            if (s_TestPrivacyService != null)
            {
                return s_TestPrivacyService;
            }
#endif
#if UNITY_ANDROID && !UNITY_EDITOR
            return new GoogleUmpPrivacyService();
#else
            return new DefaultPrivacyService();
#endif
        }

        public static ICrashReporter CreateCrashReporter() => new ConsentAwareCrashReporter();

#if UNITY_INCLUDE_TESTS
        internal static void SetTestServices(IAdService adService, IPrivacyService privacyService)
        {
            s_TestAdService = adService;
            s_TestPrivacyService = privacyService;
        }

        internal static void ResetTestServices()
        {
            s_TestAdService = null;
            s_TestPrivacyService = null;
        }
#endif

        private sealed class UnavailableAdService : IAdService
        {
            public bool IsRewardedReady => false;

            public void SetRequestPermission(bool allowed)
            {
            }

            public void ShowRewarded(string placement, Action<RewardedAdResult> completed)
            {
                completed?.Invoke(RewardedAdResult.Unavailable);
            }
        }
    }
}
