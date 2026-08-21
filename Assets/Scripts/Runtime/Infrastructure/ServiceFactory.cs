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

        public static IAdService CreateAdService()
        {
#if UNITY_ANDROID && !UNITY_EDITOR && DEVELOPMENT_BUILD
            return new GoogleRewardedAdService(AndroidRewardedTestUnitId);
#else
            return new DefaultAdService();
#endif
        }

        public static IAnalyticsService CreateAnalyticsService()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return new FirebaseAnalyticsService();
#else
            return new ConsentAwareAnalyticsService();
#endif
        }

        public static IPrivacyService CreatePrivacyService()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return new GoogleUmpPrivacyService();
#else
            return new DefaultPrivacyService();
#endif
        }

        public static ICrashReporter CreateCrashReporter()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return new FirebaseCrashReporter();
#else
            return new ConsentAwareCrashReporter();
#endif
        }
    }
}
