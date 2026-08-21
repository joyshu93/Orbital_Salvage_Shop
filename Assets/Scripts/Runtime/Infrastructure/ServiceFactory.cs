using CurioClerk.Infrastructure.Ads;
using CurioClerk.Infrastructure.Analytics;
using CurioClerk.Infrastructure.Diagnostics;
using CurioClerk.Infrastructure.Privacy;

namespace CurioClerk.Infrastructure
{
    public static class ServiceFactory
    {
        private const string AndroidRewardedTestUnitId = "ca-app-pub-3940256099942544/5224354917";

        public static IAdService CreateAdService()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return new GoogleRewardedAdService(AndroidRewardedTestUnitId);
#else
            return new DefaultAdService();
#endif
        }

        public static IAnalyticsService CreateAnalyticsService() => new ConsentAwareAnalyticsService();

        public static IPrivacyService CreatePrivacyService()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return new GoogleUmpPrivacyService();
#else
            return new DefaultPrivacyService();
#endif
        }

        public static ICrashReporter CreateCrashReporter() => new ConsentAwareCrashReporter();
    }
}
