using CurioClerk.Infrastructure.Ads;
using CurioClerk.Infrastructure.Analytics;
using CurioClerk.Infrastructure.Diagnostics;
using CurioClerk.Infrastructure.Privacy;

namespace CurioClerk.Infrastructure
{
    public static class ServiceFactory
    {
#if UNITY_ANDROID && !UNITY_EDITOR && DEVELOPMENT_BUILD
        private const string AndroidRewardedTestUnitId = "ca-app-pub-3940256099942544/5224354917";
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
#else
            return new DefaultAdService();
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
    }
}
