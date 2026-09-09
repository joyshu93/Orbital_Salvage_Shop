using System.Diagnostics;

namespace CurioClerk.Infrastructure.Ads
{
    internal static class NativeAdsQaTrace
    {
        public static string LastEvent { get; private set; } = "none";

        [Conditional("CURIO_NATIVE_ADS_QA")]
        public static void Record(string message)
        {
#if DEVELOPMENT_BUILD && CURIO_NATIVE_ADS_QA
            LastEvent = message;
            UnityEngine.Debug.Log("[CurioNativeQa] " + message);
#endif
        }
    }
}
