using CurioClerk.Infrastructure.Ads;
using CurioClerk.Infrastructure.Analytics;
using CurioClerk.Infrastructure.Diagnostics;
using CurioClerk.Infrastructure.Privacy;

namespace CurioClerk.Infrastructure
{
    public static class ServiceFactory
    {
        public static IAdService CreateAdService() => new DefaultAdService();

        public static IAnalyticsService CreateAnalyticsService() => new ConsentAwareAnalyticsService();

        public static IPrivacyService CreatePrivacyService() => new DefaultPrivacyService();

        public static ICrashReporter CreateCrashReporter() => new ConsentAwareCrashReporter();
    }
}
