#if UNITY_EDITOR || (UNITY_ANDROID && !CURIO_OFFLINE_QA)
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;

namespace CurioClerk.Infrastructure.Ads
{
    internal static class GoogleAdsCallbackDispatcher
    {
        public static void Initialize()
        {
            // UMP queues callbacks before MobileAds.Initialize. Create only the Unity
            // executor here; consent still gates all native ad initialization/requests.
            MobileAdsEventExecutor.Initialize();
#pragma warning disable 0618
            MobileAds.RaiseAdEventsOnUnityMainThread = true;
#pragma warning restore 0618
        }
    }
}
#endif
